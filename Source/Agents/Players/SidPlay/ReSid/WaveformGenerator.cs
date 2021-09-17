/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSid.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSid.Data;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSid
{
	/// <summary>
	/// A 24 bit accumulator is the basis for waveform generation. FREQ is added to
	/// the lower 16 bits of the accumulator each cycle.
	/// The accumulator is set to zero when TEST is set, and starts counting
	/// when TEST is cleared.
	/// The noise waveform is taken from intermediate bits of a 23 bit shift
	/// register. This register is clocked by bit 19 of the accumulator
	/// </summary>
	internal class WaveformGenerator
	{
		private WaveformGenerator syncSource;
		internal WaveformGenerator syncDest;

		// Tell whether the accumulator MSB was set high on the cycle
		private bool msbRising;

		internal uint accumulator;
		private uint shiftRegister;

		// Fout = (Fn*Fclk/16777216) Hz
		internal uint freq;

		// PWout = (PWn/40.95)%
		private uint pw;

		// The control register right-shifted 4 bits; used for output function
		// table loopup
		private uint waveForm;

		// The remaining control register bits
		private uint test;
		private uint ringMod;
		internal uint sync;

		// Sample data for combinations of waveforms
		private byte[] wave__ST;
		private byte[] wave_P_T;
		private byte[] wave_PS_;
		private byte[] wave_PST;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public WaveformGenerator()
		{
			syncSource = this;

			SetChipModel(ChipModel.Mos6581);

			Reset();
		}



		/********************************************************************/
		/// <summary>
		/// Set sync source
		/// </summary>
		/********************************************************************/
		public void SetSyncSource(WaveformGenerator source)
		{
			syncSource = source;
			source.syncDest = this;
		}



		/********************************************************************/
		/// <summary>
		/// Set chip model
		/// </summary>
		/********************************************************************/
		public void SetChipModel(ChipModel model)
		{
			if (model == ChipModel.Mos6581)
			{
				wave__ST = WaveData.Wave6581__ST;
				wave_P_T = WaveData.Wave6581_P_T;
				wave_PS_ = WaveData.Wave6581_PS_;
				wave_PST = WaveData.Wave6581_PST;
			}
			else
			{
				wave__ST = WaveData.Wave8580__ST;
				wave_P_T = WaveData.Wave8580_P_T;
				wave_PS_ = WaveData.Wave8580_PS_;
				wave_PST = WaveData.Wave8580_PST;
			}
		}



		/********************************************************************/
		/// <summary>
		/// SID reset
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			accumulator = 0;
			shiftRegister = 0x7ffff8;
			freq = 0;
			pw = 0;

			test = 0;
			ringMod = 0;
			sync = 0;

			msbRising = false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint ReadOsc()
		{
			return Output() >> 4;
		}



		/********************************************************************/
		/// <summary>
		/// SID clocking - 1 cycle
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clock()
		{
			// No operation if test bit is set
			if (test != 0)
				return;

			uint accumulatorPrev = accumulator;

			// Calculate new accumulator value
			accumulator += freq;
			accumulator &= 0xffffff;

			// Check whether the MSB is set high. This is used for synchronization
			msbRising = !((accumulatorPrev & 0x800000) != 0) && ((accumulator & 0x800000) != 0);

			// Shift noise register once for each time accumulator bit 19 is set high
			if (!((accumulatorPrev & 0x80000) != 0) && ((accumulator & 0x80000) != 0))
			{
				uint bit0 = ((shiftRegister >> 22) ^ (shiftRegister >> 17)) & 0x1;
				shiftRegister <<= 1;
				shiftRegister &= 0x7fffff;
				shiftRegister |= bit0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// SID clocking - delta cycles
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clock(int delta)
		{
			// No operation if test bit is set
			if (test != 0)
				return;

			uint accumulatorPrev = accumulator;

			// Calculate new accumulator value
			uint deltaAccumulator = (uint)(delta * freq);
			accumulator += deltaAccumulator;
			accumulator &= 0xffffff;

			// Check whether the MSB is set high. This is used for synchronization
			msbRising = !((accumulatorPrev & 0x800000) != 0) && ((accumulator & 0x800000) != 0);

			// Shift noise register once for each time accumulator bit 19 is set high.
			// Bit 19 is set high each time 2^20 (0x100000) is added to the accumulator
			uint shiftPeriod = 0x100000;

			while (deltaAccumulator != 0)
			{
				if (deltaAccumulator < shiftPeriod)
				{
					shiftPeriod = deltaAccumulator;

					// Determine whether bit 19 is set on the last period.
					// NB! Requires two's complement integer
					if (shiftPeriod <= 0x080000)
					{
						// Check flip from 0 to 1
						if ((((accumulator - shiftPeriod) & 0x080000) != 0) || !((accumulator & 0x080000) != 0))
							break;
					}
					else
					{
						// Check for flip from 0 (to 1 or via 1 to 0) or from 1 via 0 to 1
						if ((((accumulator - shiftPeriod) & 0x080000) != 0) && !((accumulator & 0x080000) != 0))
							break;
					}
				}

				// Shift the noise/random register
				// NB! The shift is actually delayed 2 cycles, this is not modeled
				uint bit0 = ((shiftRegister >> 22) ^ (shiftRegister >> 17)) & 0x1;
				shiftRegister <<= 1;
				shiftRegister &= 0x7fffff;
				shiftRegister |= bit0;

				deltaAccumulator -= shiftPeriod;
			}
		}



		/********************************************************************/
		/// <summary>
		/// SID clocking - delta cycles
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Synchronize()
		{
			// A special case occurs when a sync source is synched itself on the same
			// cycle as when its MSB is set high. In this case the destination will
			// not be synced. This has been verified by sampling OSC3
			if (msbRising && (syncDest.sync != 0) && !((sync != 0) && syncSource.msbRising))
				syncDest.accumulator = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Select one of 16 possible combinations of waveforms
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint Output()
		{
			// It may seem cleaner to use an array of member functions to return
			// waveform output; however a switch with inline functions is faster
			switch (waveForm)
			{
				default:
				case 0x0:
					return Output____();

				case 0x1:
					return Output___T();

				case 0x2:
					return Output__S_();

				case 0x3:
					return Output__ST();

				case 0x4:
					return Output_P__();

				case 0x5:
					return Output_P_T();

				case 0x6:
					return Output_PS_();

				case 0x7:
					return Output_PST();

				case 0x8:
					return OutputN___();

				case 0x9:
					return OutputN__T();

				case 0xa:
					return OutputN_S_();

				case 0xb:
					return OutputN_ST();

				case 0xc:
					return OutputNP__();

				case 0xd:
					return OutputNP_T();

				case 0xe:
					return OutputNPS_();

				case 0xf:
					return OutputNPST();
			}
		}

		#region Register methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void WriteFreqLo(uint freqLo)
		{
			freq = (freq & 0xff00) | (freqLo & 0x00ff);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void WriteFreqHi(uint freqHi)
		{
			freq = ((freqHi << 8) & 0xff00) | (freq & 0x00ff);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void WritePwLo(uint pwLo)
		{
			pw = (pw & 0xf00) | (pwLo & 0x0ff);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void WritePwHi(uint pwHi)
		{
			pw = ((pwHi << 8) & 0xf00) | (pw & 0x0ff);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void WriteControlReg(uint control)
		{
			waveForm = (control >> 4) & 0x0f;
			ringMod = control & 0x04;
			sync = control & 0x02;

			uint testNext = control & 0x08;

			// Test bit set.
			// The accumulator and the shift register are both cleared.
			// NB! The shift register is not really cleared immediately. It seems like
			// the individual bits in the shift register start to fade down towards
			// zero when test is set. All bits reach zero within approximately
			// $2000 - $4000 cycles.
			// This is not modeled. There should fortunately be little audible output
			// from this peculiar behavior
			if (testNext != 0)
			{
				accumulator = 0;
				shiftRegister = 0;
			}
			else
			{
				// Test bit cleared.
				// The accumulator starts counting, and the shift register is reset to
				// the value 0x7ffff8.
				// NB! The shift register will not actually be set to this exact value if the
				// shift register bits have not had time to fade to zero.
				// This is not modeled
				if (test != 0)
					shiftRegister = 0x7ffff8;
			}

			test = testNext;

			// The gate bit is handled by the EnvelopeGenerator
		}
		#endregion

		#region Output methods

		// NB! The output from SID 8580 is delayed one cycle compared to SID 6581,
		// this is not modeled

		/********************************************************************/
		/// <summary>
		/// No waveform:
		/// Zero output
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint Output____()
		{
			return 0x000;
		}



		/********************************************************************/
		/// <summary>
		/// Triangle:
		/// The upper 12 bits of the accumulator are used.
		/// The MSB is used to create the falling edge of the triangle by
		/// inverting the lower 11 bits. The MSB is thrown away and the lower
		/// 11 bits are left-shifted (half the resolution, full amplitude).
		/// Ring modulation substitutes the MSB with MSB EOR syncSource MSB
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint Output___T()
		{
			uint msb = (ringMod != 0 ? accumulator ^ syncSource.accumulator : accumulator) & 0x800000;
			return ((msb != 0 ? ~accumulator : accumulator) >> 11) & 0xfff;
		}



		/********************************************************************/
		/// <summary>
		/// Sawtooth:
		/// The output is identical to the upper 12 bits of the accumulator
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint Output__S_()
		{
			return accumulator >> 12;
		}



		/********************************************************************/
		/// <summary>
		/// Pulse:
		/// The upper 12 bits of the accumulator are used.
		/// These bits are compared to the pulse width register by a 12 bit
		/// digital comparator; output is either all one or all zero bits.
		/// 
		/// NB! The output is actually delayed one cycle after the compare.
		/// This is not modeled.
		///
		/// The test bit, when set to one, holds the pulse waveform output
		/// at 0xfff regardless of the pulse width setting
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint Output_P__()
		{
			return (test != 0) || ((accumulator >> 12) >= pw) ? (uint)0xfff : 0x000;
		}



		/********************************************************************/
		/// <summary>
		/// Noise:
		/// The noise output is taken from intermediate bits of a 23-bit
		/// shift register which is clocked by bit 19 of the accumulator.
		/// NB! The output is actually delayed 2 cycles after bit 19 is set
		/// high. This is not modeled.
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint OutputN___()
		{
			// Operation: Calculate EOR result, shift register, set bit 0 = result.
			//
			//                         ----------------------->---------------------
			//                         |                                            |
			//                    ----EOR----                                       |
			//                    |         |                                       |
			//                    2 2 2 1 1 1 1 1 1 1 1 1 1                         |
			//  Register bits:    2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 <---
			//                    |   |       |     |   |       |     |   |
			//  OSC3 bits  :      7   6       5     4   3       2     1   0
			//
			//  Since waveform output is 12 bits the output is left-shifted 4 times.
			return 
				((shiftRegister & 0x400000) >> 11) |
				((shiftRegister & 0x100000) >> 10) |
				((shiftRegister & 0x010000) >> 7) |
				((shiftRegister & 0x002000) >> 5) |
				((shiftRegister & 0x000800) >> 4) |
				((shiftRegister & 0x000080) >> 1) |
				((shiftRegister & 0x000010) << 1) |
				((shiftRegister & 0x000004) << 2);
		}

		// Combined waveforms:
		// By combining waveforms, the bits of each waveform are effectively short
		// circuited. A zero bit in one waveform will result in a zero output bit
		// (thus the infamous claim that the waveforms are AND'ed).
		// However, a zero bit in one waveform will also affect the neighboring bits
		// in the output. The reason for this has not been determined.
		//
		// Example:
		// 
		//             1 1
		// Bit #       1 0 9 8 7 6 5 4 3 2 1 0
		//             -----------------------
		// Sawtooth    0 0 0 1 1 1 1 1 1 0 0 0
		//
		// Triangle    0 0 1 1 1 1 1 1 0 0 0 0
		//
		// AND         0 0 0 1 1 1 1 1 0 0 0 0
		//
		// Output      0 0 0 0 1 1 1 0 0 0 0 0
		//
		//
		// This behavior would be quite difficult to model exactly, since the SID
		// in this case does not act as a digital state machine. Tests show that minor
		// (1 bit)  differences can actually occur in the output from otherwise
		// identical samples from OSC3 when waveforms are combined. To further
		// complicate the situation the output changes slightly with time (more
		// neighboring bits are successively set) when the 12-bit waveform
		// registers are kept unchanged.
		//
		// It is probably possible to come up with a valid model for the
		// behavior, however this would be far too slow for practical use since it
		// would have to be based on the mutual influence of individual bits.
		//
		// The output is instead approximated by using the upper bits of the
		// accumulator as an index to look up the combined output in a table
		// containing actual combined waveform samples from OSC3.
		// These samples are 8 bit, so 4 bits of waveform resolution is lost.
		// All OSC3 samples are taken with FREQ=0x1000, adding a 1 to the upper 12
		// bits of the accumulator each cycle for a sample period of 4096 cycles.

		/********************************************************************/
		/// <summary>
		/// Sawtooth+Triangle:
		/// The sawtooth output is used to lookup an OSC3 sample
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint Output__ST()
		{
			return (uint)wave__ST[Output__S_()] << 4;
		}



		/********************************************************************/
		/// <summary>
		/// Pulse+Triangle:
		/// The triangle output is right -shifted and used to look up an OSC3
		/// sample. The sample is output if the pulse output is on.
		/// The reason for using the triangle output as the index is to
		/// handle ring modulation. Only the first half of the sample is
		/// used, which should be OK since the triangle waveform has half the
		/// resolution of the accumulator
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint Output_P_T()
		{
			return (uint)(wave_P_T[Output___T() >> 1] << 4) & Output_P__();
		}



		/********************************************************************/
		/// <summary>
		/// Pulse+Sawtooth:
		/// The sawtooth output is used to lookup an OSC3 sample. The sample
		/// is output if the pulse output is on
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint Output_PS_()
		{
			return (uint)(wave_PS_[Output__S_()] << 4) & Output_P__();
		}



		/********************************************************************/
		/// <summary>
		/// Pulse+Sawtooth+Triangle:
		/// The sawtooth output is used to lookup an OSC3 sample. The sample
		/// is output if the pulse output is on
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint Output_PST()
		{
			return (uint)(wave_PST[Output__S_()] << 4) & Output_P__();
		}

		// Combined waveforms including noise:
		// All waveform combinations including noise output zero after a few cycles.
		// NB! The effects of such combinations are not fully explored. It is claimed
		// that the shift register may be filled with zeroes and locked up, which
		// seems to be true.
		// We have not attempted to model this behavior, suffice to say that
		// there is very little audible output from waveform combinations including
		// noise. We hope that nobody is actually using it

		/********************************************************************/
		/// <summary>
		/// Noise+Triangle
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint OutputN__T()
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Noise+Sawtooth
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint OutputN_S_()
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Noise+Sawtooth+Triangle
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint OutputN_ST()
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Noise+Pulse
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint OutputNP__()
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Noise+Pulse+Triangle
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint OutputNP_T()
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Noise+Pulse+Sawtooth
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint OutputNPS_()
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Noise+Pulse+Sawtooth+Triangle
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private uint OutputNPST()
		{
			return 0;
		}
		#endregion
	}
}
