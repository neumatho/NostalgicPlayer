/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.ReSidFp
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

		private const uint shift_mask =
			~(
				(1U <<  2) |	// Bit 20
				(1U <<  4) |	// Bit 18
				(1U <<  8) |	// Bit 14
				(1U << 11) |	// Bit 11
				(1U << 13) |	// Bit  9
				(1U << 17) |	// Bit  5
				(1U << 20) |	// Bit  2
				(1U << 22) 		// Bit  0
			);

		private matrix_t model_wave = null;
		private matrix_t model_pulldown = null;

		private short[] wave = null;
		private short[] pulldown = null;

		/// <summary>
		/// PWout = (PWn/40.95)%
		/// </summary>
		private uint pw = 0;

		internal uint shift_register = 0;

		/// <summary>
		/// Shift register is latched when transitioning to shift phase 1
		/// </summary>
		internal uint shift_latch = 0;

		/// <summary>
		/// Emulation of pipeline causing bit 19 to clock the shift register
		/// </summary>
		private int shift_pipeline = 0;

		private uint ring_msb_mask = 0;
		private uint no_noise = 0;
		internal uint noise_output = 0;
		private uint no_noise_or_noise_output = 0;
		private uint no_pulse = 0;
		private uint pulse_output = 0;

		/// <summary>
		/// The control register right-shifted 4 bits; used for output function table lookup
		/// </summary>
		internal uint waveform = 0;

		private uint waveform_output = 0;

		/// <summary>
		/// Current accumulator value
		/// </summary>
		private uint accumulator = 0x555555;	// Accumulator's even bits are high on powerup

		/// <summary>
		/// Fout = (Fn*Fclk/16777216)Hz
		/// </summary>
		private uint freq = 0;

		/// <summary>
		/// 8580 tri/saw pipeline
		/// </summary>
		private uint tri_saw_pipeline = 0x555;

		/// <summary>
		/// The OSC3 value
		/// </summary>
		private uint osc3 = 0;

		/// <summary>
		/// Remaining time to fully reset shift register
		/// </summary>
		private uint shift_register_reset = 0;

		/// <summary>
		/// The wave signal TTL when no waveform is selected
		/// </summary>
		private uint floating_output_ttl = 0;

		// The control register bits. Gate is handled by EnvelopeGenerator
		private bool test = false;
		private bool sync = false;

		/// <summary>
		/// Test bit is latched at phi2 for the noise XOR
		/// </summary>
		internal bool test_or_reset;

		/// <summary>
		/// Tell whether the accumulator MSB was set high on this cycle
		/// </summary>
		private bool msb_rising = false;

		private bool is6581;	// This is initialized in the SID constructor

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
		/// 
		/// </summary>
		/********************************************************************/
		public void SetPulldownModels(matrix_t models)
		{
			model_pulldown = models;
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
				// Set up waveform tables
				wave = model_wave[waveform & 0x3];

				// We assume tha combinations include noise
				// behave the same as without
				switch (waveform & 0x7)
				{
					case 3:
					{
						pulldown = model_pulldown[0];
						break;
					}

					case 4:
					{
						pulldown = (waveform & 0x8) != 0 ? model_pulldown[4] : null;
						break;
					}

					case 5:
					{
						pulldown = model_pulldown[1];
						break;
					}

					case 6:
					{
						pulldown = model_pulldown[2];
						break;
					}

					case 7:
					{
						pulldown = model_pulldown[3];
						break;
					}

					default:
					{
						pulldown = null;
						break;
					}
				}

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

					// Latch the shift register value
					shift_latch = shift_register;

					// Set reset time for shift register
					shift_register_reset = is6581 ? SHIFT_REGISTER_RESET_6581R3 : SHIFT_REGISTER_RESET_8580R5;
				}
				else
				{
					// When the test bit is falling, the second phase of the shift is
					// completed by enabling SRAM write
					Shift_Phase2(waveform_prev, waveform);
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
			pulldown = null;

			ring_msb_mask = 0;
			no_noise = 0xfff;
			no_pulse = 0xfff;
			pulse_output = 0xfff;

			shift_register_reset = 0;
			shift_register = 0x7fffff;

			// When reset is released the shift register is clocked once
			// so the lower bit is zeroed out
			// bit0 = (bit22 | test) ^ bit17 = 1 ^ 1 = 0
			test_or_reset = true;
			shift_latch = shift_register;
			Shift_Phase2(0, 0);

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
					shift_latch = shift_register;

					// New noise waveform output
					Set_Noise_Output();
				}

				// Latch the test bit value for shift phase 2
				test_or_reset = true;

				// The test bit sets pulse high
				pulse_output = 0xfff;
			}
			else
			{
				// Calculate new accumulator value
				uint accumulator_old = accumulator;
				accumulator = (accumulator + freq) & 0xffffff;

				// Check which bit have changed from low to high
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
				else if (shift_pipeline != 0)
				{
					switch (--shift_pipeline)
					{
						case 0:
						{
							Shift_Phase2(waveform, waveform);
							break;
						}

						case 1:
						{
							// Start shift phase 1
							test_or_reset = false;
							shift_latch = shift_register;
							break;
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 12-bit waveform output
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint Output(WaveformGenerator ringModulator)
		{
			// Set output value
			if (waveform != 0)
			{
				uint ix = (accumulator ^ (~ringModulator.accumulator & ring_msb_mask)) >> 12;

				// The bit masks no_pulse and no_noise are used to achieve branch-free
				// calculation of the output value
				waveform_output = (uint)(wave[ix] & (no_pulse | pulse_output) & no_noise_or_noise_output);
				if (pulldown != null)
					waveform_output = (uint)pulldown[waveform_output];

				// Triangle/Sawtooth output is delayed half cycle on 8580.
				// This will appear as a one cycle delay on OSC3 as it is latched
				// in the first phase of the clock
				if (((waveform & 3) != 0) && !is6581)
				{
					osc3 = tri_saw_pipeline & (no_pulse | pulse_output) & no_noise_or_noise_output;
					if (pulldown != null)
						osc3 = (uint)pulldown[osc3];

					tri_saw_pipeline = (uint)wave[ix];
				}
				else
					osc3 = waveform_output;

				// In the 6581 the top bit of the accumulator may be driven low by combined waveforms
				// when the sawtooth is selected
				if (is6581 && ((waveform & 2) != 0) && ((waveform_output & 0x800) == 0))
				{
					msb_rising = false;
					accumulator &= 0x7fffff;
				}

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

			return waveform_output;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// This is what happens when the lfsr is clocked:
		///
		/// cycle 0: bit 19 of the accumulator goes from low to high, the
		/// noise register acts normally, the output may pulldown a bit;
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
		/// executed at every cycle until the signal is released triggering
		/// the second phase.
		/// </summary>
		///
		//      |       |    bit n     |   bit n+1
		//      | bit19 | latch output | latch output
		// -----+-------+--------------+--------------
		// phi1 |   0   |   A <-> A    |   B <-> B
		// phi2 |   0   |   A <-> A    |   B <-> B
		// -----+-------+--------------+--------------
		// phi1 |   1   |   A <-> A    |   B <-> B      <- bit19 raises
		// phi2 |   1   |   A <-> A    |   B <-> B
		// -----+-------+--------------+--------------
		// phi1 |   1   |   X     A  --|-> A     B      <- shift phase 1
		// phi2 |   1   |   X     A  --|-> A     B
		// -----+-------+--------------+--------------
		// phi1 |   1   |   X --> X    |   A --> A      <- shift phase 2
		// phi2 |   1   |   X <-> X    |   A <-> A
		//
		//
		// Normal cycles
		// -------------
		// Normally, when noise is selected along with another waveform,
		// c1 and c2 are closed and the output bits pull down the
		// corresponding shift register bits.
		//
		//        noi_out_x             noi_out_x+1
		//          ^                     ^
		//          |                     |
		//          +-------------+       +-------------+
		//          |             |       |             |
		//          +---o<|---+   |       +---o<|---+   |
		//          |         |   |       |         |   |
		//       c2 |      c1 |   |    c2 |      c1 |   |
		//          |         |   |       |         |   |
		//  >---/---+---|>o---+   +---/---+---|>o---+   +---/--->
		//      LC                    LC                    LC
		//
		//
		// Shift phase 1
		// -------------
		// During shift phase 1 c1 and c2 are open, the SR bits are floating
		// and will be driven by the output of combined waveforms,
		// or slowly turn high.
		//
		//        noi_out_x             noi_out_x+1
		//          ^                     ^
		//          |                     |
		//          +-------------+       +-------------+
		//          |             |       |             |
		//          +---o<|---+   |       +---o<|---+   |
		//          |         |   |       |         |   |
		//       c2 /      c1 /   |    c2 /      c1 /   |
		//          |         |   |       |         |   |
		//  >-------+---|>o---+   +-------+---|>o---+   +------->
		//      LC                    LC                    LC
		//
		//
		// Shift phase 2 (phi1)
		// --------------------
		// During the first half cycle of shift phase 2 c1 is closed
		// so the value from of noi_out_x-1 enters the bit.
		//
		//        noi_out_x             noi_out_x+1
		//          ^                     ^
		//          |                     |
		//          +-------------+       +-------------+
		//          |             |       |             |
		//          +---o<|---+   |       +---o<|---+   |
		//          |         |   |       |         |   |
		//       c2 /      c1 |   |    c2 /      c1 |   |
		//          |         |   |       |         |   |
		//  >---/---+---|>o---+   +---/---+---|>o---+   +---/--->
		//      LC                    LC                    LC
		//
		//
		// Shift phase 2 (phi2)
		// --------------------
		// On the second half of shift phase 2 c2 closes and we're back to
		// normal cycles.
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool Do_Writeback(uint waveform_old, uint waveform_new, bool is6581)
		{
			// No writeback without combined waveforms
			if (waveform_old <= 8)
			{
				// Fixes SID/noisewriteback/noise_writeback_test2-{old,new}
				return false;
			}

			if (waveform_new < 8)
				return false;

			if ((waveform_new == 8)
				// Breaks noise_writeback_check_F_to_8_old
				// but fixes simple and scan
				&& (waveform_old != 0xf))
			{
				// Fixes
				// noise_writeback_check_9_to_8_old
				// noise_writeback_check_A_to_8_old
				// noise_writeback_check_B_to_8_old
				// noise_writeback_check_D_to_8_old
				// noise_writeback_check_E_to_8_old
				// noise_writeback_check_F_to_8_old
				// noise_writeback_check_9_to_8_new
				// noise_writeback_check_A_to_8_new
				// noise_writeback_check_D_to_8_new
				// noise_writeback_check_E_to_8_new
				// noise_writeback_test1-{old,new}
				return false;
			}

			// What's happening here?
			if (is6581 && ((((waveform_old & 0x3) == 0x1) && ((waveform_new & 0x3) == 0x2)) || (((waveform_old & 0x3) == 0x2) && ((waveform_new & 0x3) == 0x1))))
			{
				// Fixes
				// noise_writeback_check_9_to_A_old
				// noise_writeback_check_9_to_E_old
				// noise_writeback_check_A_to_9_old
				// noise_writeback_check_A_to_D_old
				// noise_writeback_check_D_to_A_old
				// noise_writeback_check_E_to_9_old
				return false;
			}

			if (waveform_old == 0xc)
			{
				// Fixes
				// noise_writeback_check_C_to_A_new
				return false;
			}

			if (waveform_new == 0xc)
			{
				// Fixes
				// noise_writeback_check_9_to_C_old
				// noise_writeback_check_A_to_C_old
				return false;
			}

			// Ok, do the writeback
			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint Get_Noise_Writeback(uint waveform_output)
		{
			return
				((waveform_output & (1U << 11)) >>  9) |	// Bit 11 -> bit 20
				((waveform_output & (1U << 10)) >>  6) |	// Bit 10 -> bit 18
				((waveform_output & (1U <<  9)) >>  1) |	// Bit  9 -> bit 14
				((waveform_output & (1U <<  8)) <<  3) |	// Bit  8 -> bit 11
				((waveform_output & (1U <<  7)) <<  6) |	// Bit  7 -> bit  9
				((waveform_output & (1U <<  6)) << 11) |	// Bit  6 -> bit  5
				((waveform_output & (1U <<  5)) << 15) |	// Bit  5 -> bit  2
				((waveform_output & (1U <<  4)) << 18);		// Bit  4 -> bit  0
		}



		/********************************************************************/
		/// <summary>
		/// Perform the actual shifting, moving the latched value into
		/// following bits. The XORing for bit0 is done in this cycle using
		/// the test bit latched during the previous phi2 cycle
		/// </summary>
		/********************************************************************/
		internal void Shift_Phase2(uint waveform_old, uint waveform_new)
		{
			if (Do_Writeback(waveform_old, waveform_new, is6581))
			{
				// If noise is combined with another waveform the output drives the SR bits
				shift_latch = (shift_register & shift_mask) | Get_Noise_Writeback(waveform_output);
			}

			// Bit0 = (bit22 | test | reset) ^ bit17 = 1 ^ bit17 = ~bit17
			uint bit22 = ((test_or_reset ? 1U : 0) | shift_latch) << 22;
			uint bit0 = (bit22 ^ (shift_latch << 17)) & (1 << 22);

			shift_register = (shift_latch >> 1) | bit0;

			Set_Noise_Output();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal void Write_Shift_Register()
		{
			if (waveform > 0x8)
			{
				// Write changes to the shift register output caused by combined waveforms
				// back into the shift register
				if ((shift_pipeline != 1) && !test)
				{
					// The output pulls down the SR bits
					shift_register = shift_register & (shift_mask | Get_Noise_Writeback(waveform_output));
					noise_output &= waveform_output;
				}
				else
				{
					// Shift phase 1 the output drives the SR bits
					noise_output = waveform_output;
				}

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
				((shift_register & (1U <<  2)) <<  9) |	// Bit 20 -> bit 11
				((shift_register & (1U <<  4)) <<  6) |	// Bit 18 -> bit 10
				((shift_register & (1U <<  8)) <<  1) |	// Bit 14 -> bit  9
				((shift_register & (1U << 11)) >>  3) |	// Bit 11 -> bit  8
				((shift_register & (1U << 13)) >>  6) |	// Bit  9 -> bit  7
				((shift_register & (1U << 17)) >> 11) |	// Bit  5 -> bit  6
				((shift_register & (1U << 20)) >> 15) |	// Bit  2 -> bit  5
				((shift_register & (1U << 22)) >> 18);	// Bit  0 -> bit  4

			Set_No_Noise_Or_Noise_Output();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Set_No_Noise_Or_Noise_Output()
		{
			no_noise_or_noise_output = no_noise | noise_output;
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
