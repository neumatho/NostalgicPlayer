/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Opal
{
	/// <summary>
	/// This is the Opal OPL3 emulator from Reality Adlib Tracker v2.0a (http://www.3eality.com/productions/reality-adlib-tracker).
	/// It was released by Shayde/Reality into the public domain.
	/// Minor modifications to silence some warnings and fix a bug in the envelope generator have been applied.
	/// Additional fixes by JP Cimalando.
	///
	/// The Opal OPL3 emulator.
	///
	/// Note: this is not a complete emulator, just enough for Reality Adlib Tracker tunes.
	///
	/// Missing features compared to a real OPL3:
	///
	/// - Timers/interrupts
	/// - OPL3 enable bit (it defaults to always on)
	/// - CSW mode
	/// - Test register
	/// - Percussion mode
	/// </summary>
	internal partial class Opal
	{
		private const c_int Opl3SampleRate = 49716;
		private const c_int NumChannels = 18;
		private const c_int NumOperators = 36;

		private int32_t SampleRate;
		private int32_t SampleAccum;
		private readonly int16_t[] LastOutput = new int16_t[2];
		private readonly int16_t[] CurrOutput = new int16_t[2];
		private readonly Channel[] Chan = ArrayHelper.InitializeArray<Channel>(NumChannels);
		private readonly Operator[] Op = ArrayHelper.InitializeArray<Operator>(NumOperators);
		private uint16_t Clock;
		private uint16_t TremoloClock;
		private uint16_t TremoloLevel;
		private uint16_t VibratoTick;
		private uint16_t VibratoClock;
		private bool NoteSel;
		private bool TremoloDepth;
		private bool VibratoDepth;

		private static readonly c_int[] chan_Ops =
		[
			0, 1, 2, 6, 7, 8, 12, 13, 14, 18, 19, 20, 24, 25, 26, 30, 31, 32
		];

		private static readonly int8_t[] op_Lookup =
		[
			//  00  01  02  03  04  05  06  07  08  09  0A  0B  0C  0D  0E  0F
			    0,  1,  2,  3,  4,  5,  -1, -1, 6,  7,  8,  9,  10, 11, -1, -1,
			//  10  11  12  13  14  15  16  17  18  19  1A  1B  1C  1D  1E  1F
			    12, 13, 14, 15, 16, 17, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Opal(c_int sample_Rate)
		{
			Init(sample_Rate);
		}



		/********************************************************************/
		/// <summary>
		/// Change the sample rate
		/// </summary>
		/********************************************************************/
		public void SetSampleRate(c_int sample_Rate)
		{
			// Sanity
			if (sample_Rate == 0)
				sample_Rate = Opl3SampleRate;

			SampleRate = sample_Rate;
			SampleAccum = 0;
			LastOutput[0] = LastOutput[1] = 0;
			CurrOutput[0] = CurrOutput[1] = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Write a value to an OPL3 register
		/// </summary>
		/********************************************************************/
		public void Port(uint16_t reg_Num, uint8_t val)
		{
			uint16_t type = (uint16_t)(reg_Num & 0xe0);

			// Is it BD, the one-off register stuck in the middle of the register array?
			if (reg_Num == 0xbd)
			{
				TremoloDepth = (val & 0x80) != 0;
				VibratoDepth = (val & 0x40) != 0;
				return;
			}

			// Global registers
			if (type == 0x00)
			{
				// 4-OP enables
				if (reg_Num == 0x104)
				{
					// Enable/disable channels based on which 4-op enables
					uint8_t mask = 1;

					for (c_int i = 0; i < 6; i++, mask <<= 1)
					{
						// The 4-op channels are 0, 1, 2, 9, 10, 11
						uint16_t chan = (uint16_t)(i < 3 ? i : i + 6);

						Channel primary = Chan[chan];
						Channel secondary = Chan[chan + 3];

						if ((val & mask) != 0)
						{
							// Let primary channel know it's controlling the secondary channel
							primary.SetChannelPair(secondary);

							// Turn off the second channel in the pair
							secondary.SetEnable(false);
						}
						else
						{
							// Let primary channel know it's no longer controlling the secondary channel
							primary.SetChannelPair(null);

							// Turn on the second channel in the pair
							secondary.SetEnable(true);
						}
					}
				}
				else if (reg_Num == 0x08)	// CSW / Note-sel
				{
					NoteSel = (val & 0x40) != 0;

					// Get the channels to recompute the Key Scale No. as this varies based on NoteSel
					for (c_int i = 0; i < NumChannels; i++)
						Chan[i].ComputeKeyScaleNumber();
				}
			}
			else if ((type >= 0xa0) && (type <= 0xc0))	// Channel registers
			{
				// Convert to channel number
				c_int chan_Num = reg_Num & 15;

				// Valid channel?
				if (chan_Num >= 9)
					return;

				// Is it the other bank of channels?
				if ((reg_Num & 0x100) != 0)
					chan_Num += 9;

				Channel chan = Chan[chan_Num];

				// Registers Ax and Bx affect both channels
				Channel[] chans = [ chan, chan.GetChannelPair() ];
				c_int numChans = chans[1] != null ? 2 : 1;

				// Do specific registers
				switch (reg_Num & 0xf0)
				{
					// Frequency low
					case 0xa0:
					{
						for (c_int i = 0; i < numChans; i++)
							chans[i].SetFrequencyLow(val);

						break;
					}

					// Key-on / Octave / Frequency High
					case 0xb0:
					{
						for (c_int i = 0; i < numChans; i++)
						{
							chans[i].SetKeyOn((val & 0x20) != 0);
							chans[i].SetOctave((uint16_t)((val >> 2) & 7));
							chans[i].SetFrequencyHigh((uint16_t)(val & 3));
						}

						break;
					}

					// Right Stereo Channel Enable / Left Stereo Channel Enable / Feedback Factor / Modulation Type
					case 0xc0:
					{
						chan.SetRightEnable((val & 0x20) != 0);
						chan.SetLeftEnable((val & 0x10) != 0);
						chan.SetFeedback((uint16_t)((val >> 1) & 7));
						chan.SetModulationType((uint16_t)(val & 1));
						break;
					}
				}
			}
			else if (((type >= 0x20) && (type <= 0x80)) || (type == 0xe0))	// Operator registers
			{
				// Convert to operator number
				c_int op_Num = op_Lookup[reg_Num & 0x1f];

				// Valid register?
				if (op_Num < 0)
					return;

				// Is it the other bank of operators?
				if ((reg_Num & 0x100) != 0)
					op_Num += 18;

				Operator op = Op[op_Num];

				// Do specific registers
				switch (type)
				{
					// Tremolo Enable / Vibrato Enable / Sustain Mode / Envelope Scaling / Frequency Multiplier
					case 0x20:
					{
						op.SetTremoloEnable((val & 0x80) != 0);
						op.SetVibratoEnable((val & 0x40) != 0);
						op.SetSustainMode((val & 0x20) != 0);
						op.SetEnvelopeScaling((val & 0x10) != 0);
						op.SetFrequencyMultiplier((uint16_t)(val & 15));
						break;
					}

					// Key Scale / Output Level
					case 0x40:
					{
						op.SetKeyScale((uint16_t)(val >> 6));
						op.SetOutputLevel((uint16_t)(val & 0x3f));
						break;
					}

					// Attack Rate / Decay Rate
					case 0x60:
					{
						op.SetAttackRate((uint16_t)(val >> 4));
						op.SetDecayRate((uint16_t)(val & 15));
						break;
					}

					// Sustain Level / Release Rate
					case 0x80:
					{
						op.SetSustainLevel((uint16_t)(val >> 4));
						op.SetReleaseRate((uint16_t)(val & 15));
						break;
					}

					// Waveform
					case 0xe0:
					{
						op.SetWaveform((uint16_t)(val & 7));
						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Generate sample. Every time you call this you will get two signed
		/// 16-bit samples (one for each stereo channel) which will sound
		/// correct when played back at the sample rate given when the class
		/// was constructed
		/// </summary>
		/********************************************************************/
		public void Sample(out int16_t left, out int16_t right)
		{
			// If the destination sample rate is higher than the OPL3 sample rate, we need to skip ahead
			while (SampleAccum >= SampleRate)
			{
				LastOutput[0] = CurrOutput[0];
				LastOutput[1] = CurrOutput[1];

				Output(out CurrOutput[0], out CurrOutput[1]);

				SampleAccum -= SampleRate;
			}

			// Mix with the partial accumulation
			int32_t fract = Util.MulDivR(SampleAccum, 65536, SampleRate);
			left = (int16_t)(LastOutput[0] + ((fract * (CurrOutput[0] - LastOutput[0])) / 65536));
			right = (int16_t)(LastOutput[1] + ((fract * (CurrOutput[1] - LastOutput[1])) / 65536));

			SampleAccum += Opl3SampleRate;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initialise the emulation
		/// </summary>
		/********************************************************************/
		private void Init(c_int sample_Rate)
		{
			Clock = 0;
			TremoloClock = 0;
			TremoloLevel = 0;
			VibratoTick = 0;
			VibratoClock = 0;
			NoteSel = false;
			TremoloDepth = false;
			VibratoDepth = false;

			// Let sub-objects know where to find us
			for (c_int i = 0; i < NumOperators; i++)
				Op[i].SetMaster(this);

			for (c_int i = 0; i < NumChannels; i++)
				Chan[i].SetMaster(this);

			// Add the operators to the channels.  Note, some channels can't use all the operators
			// FIXME: put this into a separate routine
			for (c_int i = 0; i < NumChannels; i++)
			{
				Channel chan = Chan[i];
				c_int op = chan_Ops[i];

				if ((i < 3) || ((i >= 9) && (i < 12)))
					chan.SetOperators(Op[op], Op[op + 3], Op[op + 6], Op[op + 9]);
				else
					chan.SetOperators(Op[op], Op[op + 3], null, null);
			}

			// Initialise the operator rate data.  We can't do this in the Operator constructor as it
			// relies on referencing the master and channel objects
			for (c_int i = 0; i < NumOperators; i++)
				Op[i].ComputeRates();

			SetSampleRate(sample_Rate);
		}



		/********************************************************************/
		/// <summary>
		/// Produce final output from the chip. This is at the OPL3
		/// sample-rate
		/// </summary>
		/********************************************************************/
		private void Output(out int16_t left, out int16_t right)
		{
			int32_t leftMix = 0, rightMix = 0;

			// Sum the output of each channel
			for (c_int i = 0; i < NumChannels; i++)
			{
				Chan[i].Output(out int16_t chanLeft, out int16_t chanRight);

				leftMix += chanLeft;
				rightMix += chanRight;
			}

			// Clamp
			if (leftMix < -0x8000)
				left = -0x8000;
			else if (leftMix > 0x7fff)
				left = 0x7fff;
			else
				left = (int16_t)leftMix;

			if (rightMix < -0x8000)
				right = -0x8000;
			else if (rightMix > 0x7fff)
				right = 0x7fff;
			else
				right = (int16_t)rightMix;

			Clock++;

			// Tremolo. According to this post, the OPL3 tremolo is a 13,440 sample length triangle wave
			// with a peak at 26 and a trough at 0 and is simply added to the logarithmic level accumulator
			//      http://forums.submarine.org.uk/phpBB/viewtopic.php?f=9&t=1171
			TremoloClock = (uint16_t)((TremoloClock + 1) % 13440);
			TremoloLevel = (uint16_t)(((TremoloClock < 13440 / 2) ? TremoloClock : 13440 - TremoloClock) / 256);

			if (!TremoloDepth)
				TremoloLevel >>= 2;

			// Vibrato. This appears to be an 8 sample long triangle wave with a magnitude of the three
			// high bits of the channel frequency, positive and negative, divided by two if the vibrato
			// depth is zero. It is only cycled every 1,024 samples
			VibratoTick++;

			if (VibratoTick >= 1024)
			{
				VibratoTick = 0;
				VibratoClock = (uint16_t)((VibratoClock + 1) & 7);
			}
		}
		#endregion
	}
}
