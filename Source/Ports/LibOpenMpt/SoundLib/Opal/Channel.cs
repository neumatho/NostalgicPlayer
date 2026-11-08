/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Opal
{
	internal partial class Opal
	{
		/// <summary>
		/// A single channel, which can contain two or more operators
		/// </summary>
		private class Channel
		{
			private readonly Operator[] Op = new Operator[4];

			/// <summary>
			/// Master object
			/// </summary>
			private Opal Master;

			/// <summary>
			/// Frequency; actually it's a phase stepping value
			/// </summary>
			private uint16_t Freq;

			/// <summary>
			/// Also known as "block" in Yamaha parlance
			/// </summary>
			private uint16_t Octave;

			private uint32_t PhaseStep;
			private uint16_t KeyScaleNumber;
			private uint16_t FeedbackShift;
			private uint16_t ModulationType;
			private Channel ChannelPair;
			private bool Enable;
			private bool LeftEnable;
			private bool RightEnable;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Channel()
			{
				Master = null;
				Freq = 0;
				Octave = 0;
				PhaseStep = 0;
				KeyScaleNumber = 0;
				FeedbackShift = 0;
				ModulationType = 0;
				ChannelPair = null;
				Enable = true;
				LeftEnable = true;
				RightEnable = true;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void SetMaster(Opal opal)
			{
				Master = opal;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void SetOperators(Operator a, Operator b, Operator c, Operator d)
			{
				Op[0] = a;
				Op[1] = b;
				Op[2] = c;
				Op[3] = d;

				if (a != null)
					a.SetChannel(this);

				if (b != null)
					b.SetChannel(this);

				if (c != null)
					c.SetChannel(this);

				if (d != null)
					d.SetChannel(this);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void SetEnable(bool on)
			{
				Enable = on;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void SetChannelPair(Channel pair)
			{
				ChannelPair = pair;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public uint16_t GetFreq()
			{
				return Freq;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public uint16_t GetOctave()
			{
				return Octave;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public uint16_t GetKeyScaleNumber()
			{
				return KeyScaleNumber;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public uint16_t GetModulationType()
			{
				return ModulationType;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Channel GetChannelPair()
			{
				return ChannelPair;
			}



			/********************************************************************/
			/// <summary>
			/// Produce output from channel
			/// </summary>
			/********************************************************************/
			public void Output(out int16_t left, out int16_t right)
			{
				// Has the channel been disabled? This is usually a result of the 4-op enables being used to
				// disable the secondary channel in each 4-op pair
				if (!Enable)
				{
					left = right = 0;
					return;
				}

				int16_t vibrato = (int16_t)((Freq >> 7) & 7);

				if (!Master.VibratoDepth)
					vibrato >>= 1;

				// 0  3  7  3  0  -3  -7  -3
				uint16_t clk = Master.VibratoClock;

				if ((clk & 3) == 0)
					vibrato = 0;	// Position 0 and 4 is zero
				else
				{
					if ((clk & 1) != 0)
						vibrato >>= 1;	// Odd position are half magnitude

					vibrato <<= Octave;

					if ((clk & 4) != 0)
						vibrato = (int16_t)(-vibrato);	// The second half positions are negative
				}

				// Combine individual operator outputs
				int16_t @out, acc;

				// Running in 4-op mode?
				if (ChannelPair != null)
				{
					// Get the secondary channel's modulation type.  This is the only thing from the secondary
					// channel that is used
					if (ChannelPair.GetModulationType() == 0)
					{
						if (ModulationType == 0)
						{
							// feedback -> modulator -> modulator -> modulator -> carrier
							@out = Op[0].Output(KeyScaleNumber, PhaseStep, vibrato, 0, (int16_t)FeedbackShift);
							@out = Op[1].Output(KeyScaleNumber, PhaseStep, vibrato, @out, 0);
							@out = Op[2].Output(KeyScaleNumber, PhaseStep, vibrato, @out, 0);
							@out = Op[3].Output(KeyScaleNumber, PhaseStep, vibrato, @out, 0);
						}
						else
						{
							// (feedback -> carrier) + (modulator -> modulator -> carrier)
							@out = Op[0].Output(KeyScaleNumber, PhaseStep, vibrato, 0, (int16_t)FeedbackShift);
							acc = Op[1].Output(KeyScaleNumber, PhaseStep, vibrato, 0, 0);
							acc = Op[2].Output(KeyScaleNumber, PhaseStep, vibrato, acc, 0);
							@out += Op[3].Output(KeyScaleNumber, PhaseStep, vibrato, acc, 0);
						}
					}
					else
					{
						if (ModulationType == 0)
						{
							// (feedback -> modulator -> carrier) + (modulator -> carrier)
							@out = Op[0].Output(KeyScaleNumber, PhaseStep, vibrato, 0, (int16_t)FeedbackShift);
							@out = Op[1].Output(KeyScaleNumber, PhaseStep, vibrato, @out, 0);
							acc = Op[2].Output(KeyScaleNumber, PhaseStep, vibrato, 0, 0);
							@out += Op[3].Output(KeyScaleNumber, PhaseStep, vibrato, acc, 0);
						}
						else
						{
							// (feedback -> carrier) + (modulator -> carrier) + carrier
							@out = Op[0].Output(KeyScaleNumber, PhaseStep, vibrato, 0, (int16_t)FeedbackShift);
							acc = Op[1].Output(KeyScaleNumber, PhaseStep, vibrato, 0, 0);
							@out += Op[2].Output(KeyScaleNumber, PhaseStep, vibrato, acc, 0);
							@out += Op[3].Output(KeyScaleNumber, PhaseStep, vibrato, 0, 0);
						}
					}
				}
				else
				{
					// Standard 2-op mode
					if (ModulationType == 0)
					{
						// Frequency modulation (well, phase modulation technically)
						@out = Op[0].Output(KeyScaleNumber, PhaseStep, vibrato, 0, (int16_t)FeedbackShift);
						@out = Op[1].Output(KeyScaleNumber, PhaseStep, vibrato, @out, 0);
					}
					else
					{
						// Additive
						@out = Op[0].Output(KeyScaleNumber, PhaseStep, vibrato, 0, (int16_t)FeedbackShift);
						@out += Op[1].Output(KeyScaleNumber, PhaseStep, vibrato);
					}
				}

				left = (int16_t)(LeftEnable ? @out : 0);
				right = (int16_t)(RightEnable ? @out : 0);
			}



			/********************************************************************/
			/// <summary>
			/// Set phase step for operators using this channel
			/// </summary>
			/********************************************************************/
			public void SetFrequencyLow(uint16_t freq)
			{
				Freq = (uint16_t)((Freq & 0x300) | (freq & 0xff));
				ComputePhaseStep();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void SetFrequencyHigh(uint16_t freq)
			{
				Freq = (uint16_t)((Freq & 0xff) | ((freq & 3) << 8));
				ComputePhaseStep();

				// Only the high bits of Freq affect the Key Scale No
				ComputeKeyScaleNumber();
			}



			/********************************************************************/
			/// <summary>
			/// Set the octave of the channel (0 to 7)
			/// </summary>
			/********************************************************************/
			public void SetOctave(uint16_t oct)
			{
				Octave = (uint16_t)(oct & 7);
				ComputePhaseStep();
				ComputeKeyScaleNumber();
			}



			/********************************************************************/
			/// <summary>
			/// Keys the channel on/off
			/// </summary>
			/********************************************************************/
			public void SetKeyOn(bool on)
			{
				Op[0].SetKeyOn(on);
				Op[1].SetKeyOn(on);
			}



			/********************************************************************/
			/// <summary>
			/// Enable left stereo channel
			/// </summary>
			/********************************************************************/
			public void SetLeftEnable(bool on)
			{
				LeftEnable = on;
			}



			/********************************************************************/
			/// <summary>
			/// Enable right stereo channel
			/// </summary>
			/********************************************************************/
			public void SetRightEnable(bool on)
			{
				RightEnable = on;
			}



			/********************************************************************/
			/// <summary>
			/// Set the channel feedback amount
			/// </summary>
			/********************************************************************/
			public void SetFeedback(uint16_t val)
			{
				FeedbackShift = (uint16_t)(val != 0 ? 9 - val : 0);
			}



			/********************************************************************/
			/// <summary>
			/// Set frequency modulation/additive modulation
			/// </summary>
			/********************************************************************/
			public void SetModulationType(uint16_t type)
			{
				ModulationType = type;
			}



			/********************************************************************/
			/// <summary>
			/// Compute the key scale number and key scale levels.
			///
			/// From the Yamaha data sheet this is the block/octave number as
			/// bits 3-1, with bit 0 coming from the MSB of the frequency if
			/// NoteSel is 1, and the 2nd MSB if NoteSel is 0
			/// </summary>
			/********************************************************************/
			public void ComputeKeyScaleNumber()
			{
				uint16_t lsb = (uint16_t)(Master.NoteSel ? Freq >> 9 : (Freq >> 8) & 1);
				KeyScaleNumber = (uint16_t)((Octave << 1) | lsb);

				// Get the channel operators to recompute their rates as they're dependent on this number.
				// They also need to recompute their key scale level
				for (c_int i = 0; i < 4; i++)
				{
					if (Op[i] == null)
						continue;

					Op[i].ComputeRates();
					Op[i].ComputeKeyScaleLevel();
				}
			}

			#region Private methods
			/********************************************************************/
			/// <summary>
			/// Compute the stepping factor for the operator waveform phase based
			/// on the frequency and octave values of the channel
			/// </summary>
			/********************************************************************/
			private void ComputePhaseStep()
			{
				PhaseStep = (uint32_t)Freq << Octave;
			}
			#endregion
		}
	}
}
