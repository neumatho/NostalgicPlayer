/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp
{
	/// <summary>
	/// 
	/// </summary>
	internal class WaveformGenerator
	{
		// A 24 bit accumulator is the basis for waveform generation.
		// FREQ is added to the lower 16 bits of the accumulator each cycle.
		// The accumulator is set to zero when TEST is set, and starts counting
		// when TEST is cleared.
		//
		// Waveforms are generated as follows:
		//
		// - No waveform:
		// When no waveform is selected, the DAC input is floating.
		//
		//
		// - Triangle:
		// The upper 12 bits of the accumulator are used.
		// The MSB is used to create the falling edge of the triangle by inverting
		// the lower 11 bits. The MSB is thrown away and the lower 11 bits are
		// left-shifted (half the resolution, full amplitude).
		// Ring modulation substitutes the MSB with MSB EOR NOT sync_source MSB.
		//
		//
		// - Sawtooth:
		// The output is identical to the upper 12 bits of the accumulator.
		//
		//
		// - Pulse:
		// The upper 12 bits of the accumulator are used.
		// These bits are compared to the pulse width register by a 12 bit digital
		// comparator; output is either all one or all zero bits.
		// The pulse setting is delayed one cycle after the compare.
		// The test bit, when set to one, holds the pulse waveform output at 0xfff
		// regardless of the pulse width setting.
		//
		//
		// - Noise:
		// The noise output is taken from intermediate bits of a 23-bit shift register
		// which is clocked by bit 19 of the accumulator.
		// The shift is delayed 2 cycles after bit 19 is set high.
		//
		// Operation: Calculate EOR result, shift register, set bit 0 = result.
		//
		//                    reset  +--------------------------------------------+
		//                      |    |                                            |
		//               test--OR-->EOR<--+                                       |
		//                      |         |                                       |
		//                      2 2 2 1 1 1 1 1 1 1 1 1 1                         |
		//     Register bits:   2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 <---+
		//                          |   |       |     |   |       |     |   |
		//     Waveform bits:       1   1       9     8   7       6     5   4
		//                          1   0
		//
		// The low 4 waveform bits are zero (grounded).

		// Number of cycles after which the waveform output fades to 0 when setting
		// the waveform register to 0.
		// Values measured on warm chips (6581R3/R4 and 8580R5)
		// checking OSC3.
		// Times vary wildly with temperature and may differ
		// from chip to chip so the numbers here represent
		// only the big difference between the old and new models.
		//
		// See [VICE Bug #290](http://sourceforge.net/p/vice-emu/bugs/290/)
		// and [VICE Bug #1128](http://sourceforge.net/p/vice-emu/bugs/1128/)
		// ~95ms
		private const uint FLOATING_OUTPUT_TTL_6581R3 = 54000;
		private const uint FLOATING_OUTPUT_FADE_6581R3 = 1400;

		// ~1s
		private const uint FLOATING_OUTPUT_TTL_8580R5 = 800000;
		private const uint FLOATING_OUTPUT_FADE_8580R5 = 50000;

		// Number of cycles after which the shift register is reset
		// when the test bit is set.
		// Values measured on warm chips (6581R3/R4 and 8580R5)
		// checking OSC3.
		// Times vary wildly with temperature and may differ
		// from chip to chip so the numbers here represent
		// only the big difference between the old and new models
		// ~210ms
		private const uint SHIFT_REGISTER_RESET_6581R3 = 50000;
		private const uint SHIFT_REGISTER_FADE_6581R3 = 15000;
		// ~2.8s
		private const uint SHIFT_REGISTER_RESET_8580R5 = 986000;
		private const uint SHIFT_REGISTER_FADE_8580R5 = 314300;

		private matrix_t model_wave;

		private short[] wave;

		/// <summary>
		/// PWout = (PWn/40.95)%
		/// </summary>
		private uint pw;

		internal uint shift_register;

		/// <summary>
		/// Emulation of pipeline causing bit 19 to clock the shift register
		/// </summary>
		private int shift_pipeline;

		private uint ring_msb_mask;
		private uint no_noise;
		internal uint noise_output;
		private uint no_noise_or_noise_output;
		private uint no_pulse;
		private uint pulse_output;

		/// <summary>
		/// The control register right-shifted 4 bits; used for output function table lookup
		/// </summary>
		internal uint waveform;

		private uint waveform_output;

		/// <summary>
		/// Current accumulator value
		/// </summary>
		private uint accumulator;

		/// <summary>
		/// Fout = (Fn*Fclk/16777216)Hz
		/// </summary>
		private uint freq;

		/// <summary>
		/// 8580 tri/saw pipeline
		/// </summary>
		private uint tri_saw_pipeline;

		/// <summary>
		/// The OSC3 value
		/// </summary>
		private uint osc3;

		/// <summary>
		/// Remaining time to fully reset shift register
		/// </summary>
		private uint shift_register_reset;

		/// <summary>
		/// The wave signal TTL when no waveform is selected
		/// </summary>
		private uint floating_output_ttl;

		// The control register bits. Gate is handled by EnvelopeGenerator
		private bool test;
		private bool sync;

		/// <summary>
		/// Tell whether the accumulator MSB was set high on this cycle
		/// </summary>
		private bool msb_rising;

		private bool is6581;	// -V730_NOINIT this is initialized in the SID constructor

		/// <summary>
		/// The DAC LUT for analog input
		/// </summary>
		private float[] dac;	// -V730_NOINIT this is initialized in the SID constructor

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public WaveformGenerator()
		{
			model_wave = null;
			wave = null;
			pw = 0;
			shift_register = 0;
			shift_pipeline = 0;
			ring_msb_mask = 0;
			no_noise = 0;
			noise_output = 0;
			no_noise_or_noise_output = 0;
			no_pulse = 0;
			pulse_output = 0;
			waveform = 0;
			waveform_output = 0;
			accumulator = 0x555555;			// Accumulator's even bits are high on powerup
			freq = 0;
			tri_saw_pipeline = 0x555;
			osc3 = 0;
			shift_register_reset = 0;
			floating_output_ttl = 0;
			test = false;
			sync = false;
			msb_rising = false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetWaveformModels(matrix_t models)
		{
			model_wave = models;
		}



		/********************************************************************/
		/// <summary>
		/// Set the analog DAC emulation:
		/// 8580 is perfectly linear while 6581 is nonlinear.
		/// Must be called before any operation
		/// </summary>
		/********************************************************************/
		public void SetDac(float[] dac)
		{
			this.dac = dac;
		}



		/********************************************************************/
		/// <summary>
		/// Set the chip model.
		/// Must be called before any operation
		/// </summary>
		/********************************************************************/
		public void SetModel(bool is6581)
		{
			this.is6581 = is6581;
		}



		/********************************************************************/
		/// <summary>
		/// Synchronize oscillators.
		/// This must be done after all the oscillators have been Clock()'ed,
		/// so that they are in the same state
		/// </summary>
		/********************************************************************/
		public void Synchronize(WaveformGenerator syncDest, WaveformGenerator syncSource)
		{
			// A special case occurs when a sync source is synced itself on the same
			// cycle as when its MSB is set high. In this case the destination will
			// not be synched. This has been verified by sampling OSC3
			if (msb_rising && syncDest.sync && !(sync && syncSource.msb_rising))
				syncDest.accumulator = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Write FREQ LO register
		/// </summary>
		/********************************************************************/
		public void WriteFreq_Lo(byte freq_lo)
		{
			freq = (freq & 0xff00) | (uint)(freq_lo & 0xff);
		}



		/********************************************************************/
		/// <summary>
		/// Write FREQ HI register
		/// </summary>
		/********************************************************************/
		public void WriteFreq_Hi(byte freq_hi)
		{
			freq = ((uint)freq_hi << 8 & 0xff00) | (freq & 0xff);
		}



		/********************************************************************/
		/// <summary>
		/// Write PW LO register
		/// </summary>
		/********************************************************************/
		public void WritePw_Lo(byte pw_lo)
		{
			pw = (pw & 0xf00) | (uint)(pw_lo & 0x0ff);
		}



		/********************************************************************/
		/// <summary>
		/// Write PW HI register
		/// </summary>
		/********************************************************************/
		public void WritePw_Hi(byte pw_hi)
		{
			pw = ((uint)pw_hi << 8 & 0xf00) | (pw & 0x0ff);
		}



		/********************************************************************/
		/// <summary>
		/// Write CONTROL REGISTER register
		/// </summary>
		/********************************************************************/
		public void WriteControl_Reg(byte control)
		{
			uint waveform_prev = waveform;
			bool test_prev = test;

			waveform = (uint)(control >> 4) & 0x0f;
			test = (control & 0x08) != 0;
			sync = (control & 0x02) != 0;

			// Substitution of accumulator MSB when sawtooth = 0, ring_mod = 1
			ring_msb_mask = (uint)((~control >> 5) & (control >> 2) & 0x1) << 23;

			if (waveform != waveform_prev)
			{
				// Set up waveform table
				wave = model_wave[waveform & 0x7];

				// No_noise and no_pulse are used in Set_Waveform_Output() as bitmasks to
				// only let the noise or pulse influence the output when the noise or pulse
				// waveforms are selected
				no_noise = (waveform & 0x8) != 0 ? (uint)0x000 : 0xfff;
				Set_No_Noise_Or_Noise_Output();
				no_pulse = (waveform & 0x4) != 0 ? (uint)0x000 : 0xfff;

				if (waveform == 0)
				{
					// Change to floating DAC input.
					// Reset fading time for floating DAC input
					floating_output_ttl = is6581 ? FLOATING_OUTPUT_TTL_6581R3 : FLOATING_OUTPUT_TTL_8580R5;
				}
			}

			if (test != test_prev)
			{
				if (test)
				{
					// Reset accumulator
					accumulator = 0;

					// Flush shift pipeline
					shift_pipeline = 0;

					// Set reset time for shift register
					shift_register_reset = is6581 ? SHIFT_REGISTER_RESET_6581R3 : SHIFT_REGISTER_RESET_8580R5;
				}
				else
				{
					// When the test bit is falling, the second phase of the shift is
					// completed by enabling SRAM write.

					// During first phase of the shift the bits are interconnected
					// and the output of each bit is latched into the following.
					// The output may overwrite the latched value
					if (Do_Pre_Writeback(waveform_prev, waveform, is6581))
						shift_register &= Get_Noise_Writeback();

					// Bit0 = (bit22 | test) ^ bit17 = 1 ^ bit17 = ~bit17
					Clock_Shift_Register((~shift_register << 17) & (1 << 22));
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// SID reset
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			// Accumulator is not changed on reset
			freq = 0;
			pw = 0;

			msb_rising = false;

			waveform = 0;
			osc3 = 0;

			test = false;
			sync = false;

			wave = model_wave != null ? model_wave[0] : null;

			ring_msb_mask = 0;
			no_noise = 0xfff;
			no_pulse = 0xfff;
			pulse_output = 0xfff;

			shift_register_reset = 0;
			shift_register = 0x7fffff;

			// When reset is released the shift register is clocked once
			// so the lower bit is zeroed out
			// bit0 = (bit22 | test) ^ bit17 = 1 ^ 1 = 0
			Clock_Shift_Register(0);

			shift_pipeline = 0;

			waveform_output = 0;
			floating_output_ttl = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Read OSC3 value
		/// </summary>
		/********************************************************************/
		public byte ReadOsc()
		{
			return (byte)(osc3 >> 4);
		}



		/********************************************************************/
		/// <summary>
		/// Read accumulator value
		/// </summary>
		/********************************************************************/
		public uint ReadAccumulator()
		{
			return accumulator;
		}



		/********************************************************************/
		/// <summary>
		/// Read freq value
		/// </summary>
		/********************************************************************/
		public uint ReadFreq()
		{
			return freq;
		}



		/********************************************************************/
		/// <summary>
		/// Read test value
		/// </summary>
		/********************************************************************/
		public bool ReadTest()
		{
			return test;
		}



		/********************************************************************/
		/// <summary>
		/// Read sync value
		/// </summary>
		/********************************************************************/
		public bool ReadSync()
		{
			return sync;
		}



		/********************************************************************/
		/// <summary>
		/// SID clocking
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clock()
		{
			if (test)
			{
				if ((shift_register_reset != 0) && (--shift_register_reset == 0))
				{
					ShiftRegBitFade();

					// New noise waveform output
					Set_Noise_Output();
				}

				// The test bit sets pulse high
				pulse_output = 0xfff;
			}
			else
			{
				// Calculate new accumulator value
				uint accumulator_old = accumulator;
				accumulator = (accumulator + freq) & 0xffffff;

				// Check which bit have changed
				uint accumulator_bits_set = ~accumulator_old & accumulator;

				// Check whether the MSB is set high. This is used for synchronization
				msb_rising = (accumulator_bits_set & 0x800000) != 0;

				// Shift noise register once for each time accumulator bit 19 is set high.
				// The shift is delayed 2 cycles
				if ((accumulator_bits_set & 0x080000) != 0)
				{
					// Pipeline: Detect rising bit, shift phase 1, shift phase 2
					shift_pipeline = 2;
				}
				else if ((shift_pipeline != 0) && (--shift_pipeline == 0))
				{
					// Bit0 = (bit22 | test) ^ bit17
					Clock_Shift_Register(((shift_register << 22) ^ (shift_register << 17)) & (1 << 22));
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 12-bit waveform output as an analogue float value
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float Output(WaveformGenerator ringModulator)
		{
			// Set output value
			if (waveform != 0)
			{
				uint ix = (accumulator ^ (~ringModulator.accumulator & ring_msb_mask)) >> 12;

				// The bit masks no_pulse and no_noise are used to achieve branch-free
				// calculation of the output value
				waveform_output = (uint)(wave[ix] & (no_pulse | pulse_output) & no_noise_or_noise_output);

				// Triangle/Sawtooth output is delayed half cycle on 8580.
				// This will appear as a one cycle delay on OSC3 as it is latched first phase of the clock
				if (((waveform & 3) != 0) && !is6581)
				{
					osc3 = tri_saw_pipeline & (no_pulse | pulse_output) & no_noise_or_noise_output;
					tri_saw_pipeline = (uint)wave[ix];
				}
				else
					osc3 = waveform_output;

				// In the 6581 the top bit of the accumulator may be driven low by combined waveforms
				// when the sawtooth is selected
				// FIXME doesn't seem to always happen
				if (((waveform & 2) != 0) && ((waveform & 0xd) != 0) && is6581)
					accumulator &= (waveform_output << 12) | 0x7fffff;

				Write_Shift_Register();
			}
			else
			{
				// Age floating DAC input
				if ((floating_output_ttl != 0) && (--floating_output_ttl == 0))
					WaveBitFade();
			}

			// The pulse level is defined as (accumulator >> 12) >= pw ? 0xfff : 0x000.
			// The expression -((accumulator >> 12) >= pw) & 0xfff yields the same
			// results without any branching (and thus without any pipeline stalls).
			// NB! This expression relies on that the result of a boolean expression
			// is either 0 or 1, and furthermore requires two's complement integer.
			// A few more cycles may be saved by storing the pulse width left shifted
			// 12 bits, and dropping the and with 0xfff (this is valid since pulse is
			// used as a bit mask on 12 bit values), yielding the expression
			// -(accumulator >= pw24). However this only results in negligible savings.

			// The result of the pulse width compare is delayed one cycle.
			// Push next pulse level into pulse level pipeline
			pulse_output = ((accumulator >> 12) >= pw) ? (uint)0xfff : 0x000;

			// DAC imperfections are emulated by using waveform_output as an index
			// into a DAC lookup table. ReadOsc() uses waveform_output directly
			return dac[waveform_output];
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// This is what happens when the lfsr is clocked:
		///
		/// cycle 0: bit 19 of the accumulator goes from low to high, the
		/// noise register acts normally, the output may overwrite a bit;
		///
		/// cycle 1: first phase of the shift, the bits are interconnected
		/// and the output of each bit is latched into the following. The
		/// output may overwrite the latched value.
		///
		/// cycle 2: second phase of the shift, the latched value becomes
		/// active in the first half of the clock and from the second half
		/// the register returns to normal operation.
		///
		/// When the test or reset lines are active the first phase is
		/// executed at every cyle until the signal is released triggering
		/// the second phase
		/// </summary>
		/********************************************************************/
		internal void Clock_Shift_Register(uint bit0)
		{
			shift_register = (shift_register >> 1) | bit0;

			// New noise waveform output
			Set_Noise_Output();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint Get_Noise_Writeback()
		{
			return
				~(uint)(
					(1 <<  2) |		// Bit 20
					(1 <<  4) |		// Bit 18
					(1 <<  8) |		// Bit 14
					(1 << 11) |		// Bit 11
					(1 << 13) |		// Bit  9
					(1 << 17) |		// Bit  5
					(1 << 20) |		// Bit  2
					(1 << 22)		// Bit  0
				) |
				((waveform_output & (1 << 11)) >>  9) |		// Bit 11 -> bit 20
				((waveform_output & (1 << 10)) >>  6) |		// Bit 10 -> bit 18
				((waveform_output & (1 <<  9)) >>  1) |		// Bit  9 -> bit 14
				((waveform_output & (1 <<  8)) <<  3) |		// Bit  8 -> bit 11
				((waveform_output & (1 <<  7)) <<  6) |		// Bit  7 -> bit  9
				((waveform_output & (1 <<  6)) << 11) |		// Bit  6 -> bit  5
				((waveform_output & (1 <<  5)) << 15) |		// Bit  5 -> bit  2
				((waveform_output & (1 <<  4)) << 18);		// Bit  4 -> bit  0
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal void Write_Shift_Register()
		{
			if ((waveform > 0x8) && !test && (shift_pipeline != 1))
			{
				// Write changes to the shift register output caused by combined waveforms
				// back into the shift register. This happens only when the register is clocked
				// (see $D1+$81 wave_test [1]) or when the test bit is falling.
				// A bit once set to zero cannot be changed, hence the and'ing.
				//
				// [1] ftp://ftp.untergrund.net/users/nata/sid_test/$D1+$81_wave_test.7z
				//
				// FIXME: Write test program to check the effect of 1 bits and whether
				// neighboring bits are affected

				shift_register &= Get_Noise_Writeback();

				noise_output &= waveform_output;
				Set_No_Noise_Or_Noise_Output();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal void Set_Noise_Output()
		{
			noise_output =
				((shift_register & (1 <<  2)) <<  9) |	// Bit 20 -> bit 11
				((shift_register & (1 <<  4)) <<  6) |	// Bit 18 -> bit 10
				((shift_register & (1 <<  8)) <<  1) |	// Bit 14 -> bit  9
				((shift_register & (1 << 11)) >>  3) |	// Bit 11 -> bit  8
				((shift_register & (1 << 13)) >>  6) |	// Bit  9 -> bit  7
				((shift_register & (1 << 17)) >> 11) |	// Bit  5 -> bit  6
				((shift_register & (1 << 20)) >> 15) |	// Bit  2 -> bit  5
				((shift_register & (1 << 22)) >> 18);	// Bit  0 -> bit  4

			Set_No_Noise_Or_Noise_Output();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Do_Pre_Writeback(uint waveform_prev, uint waveform, bool is6581)
		{
			// No writeback without combined waveforms
			if (waveform_prev <= 0x8)
				return false;

			// No writeback when changing to noise
			if (waveform == 8)
				return false;

			// What's happening here?
			if (is6581 &&
					((((waveform_prev & 0x3) == 0x1) && ((waveform & 0x3) == 0x2))
					|| (((waveform_prev & 0x3) == 0x2) && ((waveform & 0x3) == 0x1))))
			{
				return false;
			}

			if (waveform_prev == 0xc)
			{
				if (is6581)
					return false;

				if ((waveform != 0x9) && (waveform != 0xe))
					return false;
			}

			// Ok do the writeback
			return true;
		}



		/********************************************************************/
		/// <summary>
		/// When noise and pulse are combined all the bits are connected and
		/// the four lower ones are grounded. This causes the adjacent bits
		/// to be pulled down, with different strength depending on model.
		///
		/// This is just a rough attempt at modelling the effect
		/// </summary>
		/********************************************************************/
		private uint Noise_Pulse6581(uint noise)
		{
			return noise < 0xf00 ? 0x000 : noise & (noise << 1) & (noise << 2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint Noise_Pulse8580(uint noise)
		{
			return noise < 0xfc0 ? noise & (noise << 1) : 0xfc0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Set_No_Noise_Or_Noise_Output()
		{
			no_noise_or_noise_output = no_noise | noise_output;

			// Pulse+noise
			if ((waveform & 0xc) == 0xc)
				no_noise_or_noise_output = is6581 ? Noise_Pulse6581(no_noise_or_noise_output) : Noise_Pulse8580(no_noise_or_noise_output);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void WaveBitFade()
		{
			waveform_output &= waveform_output >> 1;
			osc3 = waveform_output;

			if (waveform_output != 0)
				floating_output_ttl = is6581 ? FLOATING_OUTPUT_FADE_6581R3 : FLOATING_OUTPUT_FADE_8580R5;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ShiftRegBitFade()
		{
			shift_register |= shift_register >> 1;
			shift_register |= 0x400000;

			if (shift_register != 0x7fffff)
				shift_register_reset = is6581 ? SHIFT_REGISTER_FADE_6581R3 : SHIFT_REGISTER_FADE_8580R5;
		}
		#endregion
	}
}
