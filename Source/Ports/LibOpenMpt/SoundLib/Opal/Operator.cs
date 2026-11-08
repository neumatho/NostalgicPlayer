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
		/// A single FM operator
		/// </summary>
		private class Operator
		{
			private enum Env
			{
				Off = -1,
				Att,
				Dec,
				Sus,
				Rel
			}

			/// <summary>
			/// Master object
			/// </summary>
			private Opal Master;

			/// <summary>
			/// Owning channel
			/// </summary>
			private Channel Chan;

			/// <summary>
			/// The current offset in the selected waveform
			/// </summary>
			private uint32_t Phase;

			/// <summary>
			/// The waveform id this operator is using
			/// </summary>
			private uint16_t Waveform;

			/// <summary>
			/// Frequency multiplier * 2
			/// </summary>
			private uint16_t FreqMultTimes2;

			/// <summary>
			/// Which stage the envelope is at (see Env enums above)
			/// </summary>
			private Env EnvelopeStage;

			/// <summary>
			/// 0 - $1FF, 0 being the loudest
			/// </summary>
			private int16_t EnvelopeLevel;

			/// <summary>
			/// 0 - $FF
			/// </summary>
			private uint16_t OutputLevel;

			private uint16_t AttackRate;
			private uint16_t DecayRate;
			private uint16_t SustainLevel;
			private uint16_t ReleaseRate;
			private uint16_t AttackShift;
			private uint16_t AttackMask;
			private uint16_t AttackAdd;
			private uint16_t[] AttackTab;
			private uint16_t DecayShift;
			private uint16_t DecayMask;
			private uint16_t DecayAdd;
			private uint16_t[] DecayTab;
			private uint16_t ReleaseShift;
			private uint16_t ReleaseMask;
			private uint16_t ReleaseAdd;
			private uint16_t[] ReleaseTab;
			private uint16_t KeyScaleShift;
			private uint16_t KeyScaleLevel;
			private readonly int16_t[] Out = new int16_t[2];
			private bool KeyOn;

			/// <summary>
			/// Affects envelope rate scaling
			/// </summary>
			private bool KeyScaleRate;

			/// <summary>
			/// Whether to sustain during the sustain phase, or release instead
			/// </summary>
			private bool SustainMode;

			private bool TremoloEnable;
			private bool VibratoEnable;

			/// <summary>
			/// Needs to be multiplied by two (and divided by two later when we use it) because the first
			/// entry is actually .5
			/// </summary>
			private static readonly uint16_t[] mul_Times_2 =
			[
				1, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 20, 24, 24, 30, 30
			];

			private static readonly uint8_t[] kslShift =
			[
				8, 1, 2, 0
			];

			private static readonly uint8_t[] levTab =
			[
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 12, 16, 20, 24, 28, 32,
				0, 0, 0, 0, 0, 12, 20, 28, 32, 40, 44, 48, 52, 56, 60, 64,
				0, 0, 0, 20, 32, 44, 52, 60, 64, 72, 76, 80, 84, 88, 92, 96,
				0, 0, 32, 52, 64, 76, 84, 92, 96, 104, 108, 112, 116, 120, 124, 128,
				0, 32, 64,  84,  96,  108, 116, 124, 128, 136, 140, 144, 148, 152, 156, 160,
				0, 64, 96,  116, 128, 140, 148, 156, 160, 168, 172, 176, 180, 184, 188, 192,
				0, 96, 128, 148, 160, 172, 180, 188, 192, 200, 204, 208, 212, 216, 220, 224
			];

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Operator()
			{
				Master = null;
				Chan = null;
				Phase = 0;
				Waveform = 0;
				FreqMultTimes2 = 1;
				EnvelopeStage = Env.Off;
				EnvelopeLevel = 0x1ff;
				AttackRate = 0;
				DecayRate = 0;
				SustainLevel = 0;
				ReleaseRate = 0;
				KeyScaleShift = 0;
				KeyScaleLevel = 0;
				Out[0] = Out[1] = 0;
				KeyOn = false;
				KeyScaleRate = false;
				SustainMode = false;
				TremoloEnable = false;
				VibratoEnable = false;
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
			public void SetChannel(Channel chan)
			{
				Chan = chan;
			}



			/********************************************************************/
			/// <summary>
			/// Produce output from operator
			/// </summary>
			/********************************************************************/
			public int16_t Output(uint16_t _/*keyScaleNum*/, uint32_t phase_Step, int16_t vibrato, int16_t mod = 0, int16_t fbShift = 0)
			{
				// Advance wave phase
				if (VibratoEnable)
					phase_Step = (uint32_t)(phase_Step + vibrato);

				Phase += (phase_Step * FreqMultTimes2) / 2;

				uint16_t level = (uint16_t)((EnvelopeLevel + OutputLevel + KeyScaleLevel + (TremoloEnable ? Master.TremoloLevel : 0)) << 3);

				switch (EnvelopeStage)
				{
					// Attack stage
					case Env.Att:
					{
						uint16_t add = (uint16_t)(((AttackAdd >> AttackTab[(Master.Clock >> AttackShift) & 7]) * ~EnvelopeLevel) >> 3);

						if (AttackRate == 0)
							add = 0;

						if ((AttackMask != 0) && ((Master.Clock & AttackMask) != 0))
							add = 0;

						EnvelopeLevel = (int16_t)(EnvelopeLevel + add);

						if (EnvelopeLevel <= 0)
						{
							EnvelopeLevel = 0;
							EnvelopeStage = Env.Dec;
						}

						break;
					}

					// Decay stage
					case Env.Dec:
					{
						uint16_t add = (uint16_t)(DecayAdd >> DecayTab[(Master.Clock >> DecayShift) & 7]);

						if (DecayRate == 0)
							add = 0;

						if ((DecayMask != 0) && ((Master.Clock & DecayMask) != 0))
							add = 0;

						EnvelopeLevel = (int16_t)(EnvelopeLevel + add);

						if (EnvelopeLevel >= SustainLevel)
						{
							EnvelopeLevel = (int16_t)SustainLevel;
							EnvelopeStage = Env.Sus;
						}

						break;
					}

					// Sustain stage
					case Env.Sus:
					{
						if (SustainMode)
							break;

						goto case Env.Rel;
					}

					// Release stage
					case Env.Rel:
					{
						uint16_t add = (uint16_t)(ReleaseAdd >> ReleaseTab[(Master.Clock >> ReleaseShift) & 7]);

						if (ReleaseRate == 0)
							add = 0;

						if ((ReleaseMask != 0) && ((Master.Clock & ReleaseMask) != 0))
							add = 0;

						EnvelopeLevel = (int16_t)(EnvelopeLevel + add);

						if (EnvelopeLevel >= 0x1ff)
						{
							EnvelopeLevel = 0x1ff;
							EnvelopeStage = Env.Off;

							Out[0] = Out[1] = 0;

							return 0;
						}

						break;
					}

					// Envelope, and therefore the operator, is not running
					default:
					{
						Out[0] = Out[1] = 0;

						return 0;
					}
				}

				// Feedback? In that case we modulate by a blend of the last two samples
				if (fbShift != 0)
					mod = (int16_t)(mod + ((Out[0] + Out[1]) >> fbShift));

				uint16_t phase = (uint16_t)((Phase >> 10) + mod);
				uint16_t offset = (uint16_t)(phase & 0xff);
				uint16_t logSin;
				bool negate = false;

				switch (Waveform)
				{
					// Standard sine wave
					case 0:
					{
						if ((phase & 0x100) != 0)
							offset ^= 0xff;

						logSin = LogSinTable[offset];
						negate = (phase & 0x200) != 0;
						break;
					}

					// Half sine wave
					case 1:
					{
						if ((phase & 0x200) != 0)
							offset = 0;
						else if ((phase & 0x100) != 0)
							offset ^= 0xff;

						logSin = LogSinTable[offset];
						break;
					}

					// Positive sine wave
					case 2:
					{
						if ((phase & 0x100) != 0)
							offset ^= 0xff;

						logSin = LogSinTable[offset];
						break;
					}

					// Quarter positive sine wave
					case 3:
					{
						if ((phase & 0x100) != 0)
							offset = 0;

						logSin = LogSinTable[offset];
						break;
					}

					// Double-speed sine wave
					case 4:
					{
						if ((phase & 0x200) != 0)
							offset = 0;
						else
						{
							if ((phase & 0x80) != 0)
								offset ^= 0xff;

							offset = (uint16_t)((offset + offset) & 0xff);
							negate = (phase & 0x100) != 0;
						}

						logSin = LogSinTable[offset];
						break;
					}

					// Double-speed positive sine wave
					case 5:
					{
						if ((phase & 0x200) != 0)
							offset = 0;
						else
						{
							offset = (uint16_t)((offset + offset) & 0xff);

							if ((phase & 0x80) != 0)
								offset ^= 0xff;
						}

						logSin = LogSinTable[offset];
						break;
					}

					// Square wave
					case 6:
					{
						logSin = 0;
						negate = (phase & 0x200) != 0;
						break;
					}

					// Exponentiation wave
					default:
					{
						logSin = (uint16_t)(phase & 0x1ff);

						if ((phase & 0x200) != 0)
						{
							logSin ^= 0x1ff;
							negate = true;
						}

						logSin <<= 3;
						break;
					}
				}

				uint16_t mix = (uint16_t)(logSin + level);

				if (mix > 0x1fff)
					mix = 0x1fff;

				// From the OPLx decapsulated docs:
				// "When such a table is used for calculation of the exponential, the table is read at the
				// position given by the 8 LSB's of the input. The value + 1024 (the hidden bit) is then the
				// significand of the floating point output and the yet unused MSB's of the input are the
				// exponent of the floating point output."
				int16_t v = (int16_t)((ExpTable[mix & 0xff] + 1024U) >> (mix >> 8));
				v += v;

				if (negate)
					v = (int16_t)(~v);

				// Keep last two results for feedback calculation
				Out[1] = Out[0];
				Out[0] = v;

				return v;
			}



			/********************************************************************/
			/// <summary>
			/// Trigger operator
			/// </summary>
			/********************************************************************/
			public void SetKeyOn(bool on)
			{
				// Already on/off?
				if (KeyOn == on)
					return;

				KeyOn = on;

				if (on)
				{
					// The highest attack rate is instant; it bypasses the attack phase
					if (AttackRate == 15)
					{
						EnvelopeStage = Env.Dec;
						EnvelopeLevel = 0;
					}
					else
						EnvelopeStage = Env.Att;

					Phase = 0;
				}
				else
				{
					// Stopping current sound?
					if ((EnvelopeStage != Env.Off) && (EnvelopeStage != Env.Rel))
						EnvelopeStage = Env.Rel;
				}
			}



			/********************************************************************/
			/// <summary>
			/// Enable amplitude vibrato
			/// </summary>
			/********************************************************************/
			public void SetTremoloEnable(bool on)
			{
				TremoloEnable = on;
			}



			/********************************************************************/
			/// <summary>
			/// Enable frequency vibrato
			/// </summary>
			/********************************************************************/
			public void SetVibratoEnable(bool on)
			{
				VibratoEnable = on;
			}



			/********************************************************************/
			/// <summary>
			/// Sets whether we release or sustain during the sustain phase of
			/// the envelope. 'true' is to sustain, otherwise release
			/// </summary>
			/********************************************************************/
			public void SetSustainMode(bool on)
			{
				SustainMode = on;
			}



			/********************************************************************/
			/// <summary>
			/// Key scale rate. Sets how much the Key Scaling Number affects the
			/// envelope rates
			/// </summary>
			/********************************************************************/
			public void SetEnvelopeScaling(bool on)
			{
				KeyScaleRate = on;
				ComputeRates();
			}



			/********************************************************************/
			/// <summary>
			/// Multiplies the phase frequency
			/// </summary>
			/********************************************************************/
			public void SetFrequencyMultiplier(uint16_t scale)
			{
				FreqMultTimes2 = mul_Times_2[scale & 15];
			}



			/********************************************************************/
			/// <summary>
			/// Attenuates output level towards higher pitch
			/// </summary>
			/********************************************************************/
			public void SetKeyScale(uint16_t scale)
			{
				KeyScaleShift = kslShift[scale];
				ComputeKeyScaleLevel();
			}



			/********************************************************************/
			/// <summary>
			/// Sets the output level (volume) of the operator
			/// </summary>
			/********************************************************************/
			public void SetOutputLevel(uint16_t level)
			{
				OutputLevel = (uint16_t)(level * 4);
			}



			/********************************************************************/
			/// <summary>
			/// Operator attack rate
			/// </summary>
			/********************************************************************/
			public void SetAttackRate(uint16_t rate)
			{
				AttackRate = rate;

				ComputeRates();
			}



			/********************************************************************/
			/// <summary>
			/// Operator decay rate
			/// </summary>
			/********************************************************************/
			public void SetDecayRate(uint16_t rate)
			{
				DecayRate = rate;

				ComputeRates();
			}



			/********************************************************************/
			/// <summary>
			/// Operator sustain level
			/// </summary>
			/********************************************************************/
			public void SetSustainLevel(uint16_t level)
			{
				SustainLevel = (uint16_t)(level < 15 ? level : 31);
				SustainLevel *= 16;
			}



			/********************************************************************/
			/// <summary>
			/// Operator release rate
			/// </summary>
			/********************************************************************/
			public void SetReleaseRate(uint16_t rate)
			{
				ReleaseRate = rate;

				ComputeRates();
			}



			/********************************************************************/
			/// <summary>
			/// Assign the waveform this operator will use
			/// </summary>
			/********************************************************************/
			public void SetWaveform(uint16_t wave)
			{
				Waveform = (uint16_t)(wave & 7);
			}



			/********************************************************************/
			/// <summary>
			/// Compute actual rate from register rate. From the Yamaha data
			/// sheet:
			///
			/// Actual rate = Rate value * 4 + Rof, if Rate value = 0, actual
			/// rate = 0
			///
			/// Rof is set as follows depending on the KSR setting:
			///
			///  Key scale   0   1   2   3   4   5   6   7   8   9   10  11  12  13  14  15
			///  KSR = 0     0   0   0   0   1   1   1   1   2   2   2   2   3   3   3   3
			///  KSR = 1     0   1   2   3   4   5   6   7   8   9   10  11  12  13  14  15
			///
			/// Note: zero rates are infinite, and are treated separately
			/// elsewhere
			/// </summary>
			/********************************************************************/
			public void ComputeRates()
			{
				c_int combined_Rate = (AttackRate * 4) + (Chan.GetKeyScaleNumber() >> (KeyScaleRate ? 0 : 2));
				c_int rate_High = combined_Rate >> 2;
				c_int rate_Low = combined_Rate & 3;

				AttackShift = (uint16_t)(rate_High < 12 ? 12 - rate_High : 0);
				AttackMask = (uint16_t)((1 << AttackShift) - 1);
				AttackAdd = (uint16_t)((rate_High < 12) ? 1 : 1 << (rate_High - 12));
				AttackTab = RateTables[rate_Low];

				// Attack rate of 15 is always instant
				if (AttackRate == 15)
					AttackAdd = 0xfff;

				combined_Rate = (DecayRate * 4) + (Chan.GetKeyScaleNumber() >> (KeyScaleRate ? 0 : 2));
				rate_High = combined_Rate >> 2;
				rate_Low = combined_Rate & 3;

				DecayShift = (uint16_t)(rate_High < 12 ? 12 - rate_High : 0);
				DecayMask = (uint16_t)((1 << DecayShift) - 1);
				DecayAdd = (uint16_t)((rate_High < 12) ? 1 : 1 << (rate_High - 12));
				DecayTab = RateTables[rate_Low];

				combined_Rate = (ReleaseRate * 4) + (Chan.GetKeyScaleNumber() >> (KeyScaleRate ? 0 : 2));
				rate_High = combined_Rate >> 2;
				rate_Low = combined_Rate & 3;

				ReleaseShift = (uint16_t)(rate_High < 12 ? 12 - rate_High : 0);
				ReleaseMask = (uint16_t)((1 << ReleaseShift) - 1);
				ReleaseAdd = (uint16_t)((rate_High < 12) ? 1 : 1 << (rate_High - 12));
				ReleaseTab = RateTables[rate_Low];
			}



			/********************************************************************/
			/// <summary>
			/// Compute the operator's key scale level. This changes based on the
			/// channel frequency/octave and operator key scale value
			/// </summary>
			/********************************************************************/
			public void ComputeKeyScaleLevel()
			{
				// This uses a combined value of the top four bits of frequency with the octave/block
				uint16_t i = (uint16_t)((Chan.GetOctave() << 4) | (Chan.GetFreq() >> 6));
				KeyScaleLevel = (uint16_t)(levTab[i] >> KeyScaleShift);
			}
		}
	}
}
