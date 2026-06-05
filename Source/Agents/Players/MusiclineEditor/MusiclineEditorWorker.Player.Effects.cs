/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor
{
	/// <summary>
	/// Handle all the real time effects
	/// </summary>
	internal partial class MusiclineEditorWorker
	{
		/********************************************************************/
		/// <summary>
		/// Return the right wave type table to use
		/// </summary>
		/********************************************************************/
		private sbyte[] GetWaveTypeTable(WaveType waveType)
		{
			switch (waveType)
			{
				case WaveType.Sine:
					return Tables.Sine;

				case WaveType.RampDown:
					return Tables.DownRamp;

				case WaveType.Sawtooth:
					return Tables.Sawtooth;

				case WaveType.Square:
					return Tables.Square;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PlayEffects()
		{
			for (int i = 0; i < numberOfChannels; i++)
			{
				VoiceInfo voiceInfo = voices[i];
				IChannel channel = VirtualChannels[i];

				voiceInfo.WaveSampleRepeatPointer = voiceInfo.WaveSampleRepeatPointerOriginal;

				if (!voiceInfo.Play.HasFlag(PlayFlag.Retrigger))
				{
					SlideVolume(voiceInfo);
					SlideChannelVolume(voiceInfo);
					SlideMasterVolume(voiceInfo);
					SlideArpeggioVolume(voiceInfo);
					SlideNote(voiceInfo);
					SlideArpeggioNote(voiceInfo);
					ArpeggioPlay(voiceInfo);
					VibratoPlay(voiceInfo);
					TremoloPlay(voiceInfo);
				}

				EnvelopePlay(voiceInfo);
				MoveLoop(voiceInfo, channel);
				TransformPlay(voiceInfo);
				PhasePlay(voiceInfo);
				MixPlay(voiceInfo);
				ResonancePlay(voiceInfo);
				FilterPlay(voiceInfo);

				// If WaveSampleRepeatPointer ended up pointing into one of the
				// effect output buffers, copy the data back into the channel's
				// WaveBuffer and rewire the pointers to point at it
				if (voiceInfo.WaveSampleRepeatPointer.HasValue && IsEffectBuffer(voiceInfo, voiceInfo.WaveSampleRepeatPointer.Value.SampleData))
				{
					sbyte[] sourceBuffer = voiceInfo.WaveSampleRepeatPointer.Value.SampleData;
					sbyte[] waveBuffer = voiceInfo.WaveBuffer;

					SamplePointer newPointer = new SamplePointer(waveBuffer);
					voiceInfo.WaveSampleRepeatPointer = newPointer;

					if ((voiceInfo.WaveOrSample != SampleType.Sample) || voiceInfo.Effects1.Effect.HasFlag(InstrumentEffect1.Loop))
						voiceInfo.WaveSamplePointer = newPointer;

					Array.Copy(sourceBuffer, 0, waveBuffer, 0, voiceInfo.WaveSampleRepeatLength * 2);
				}

				// If the channel is currently looping (i.e. we are NOT about to
				// retrigger) and the instrument has the Loop effect active,
				// push the updated loop pointer/length to the mixer so it picks
				// them up when the current iteration ends
				if (!voiceInfo.Play.HasFlag(PlayFlag.Retrigger) && voiceInfo.Effects1.Effect.HasFlag(InstrumentEffect1.Loop))
				{
					channel.SetSample(voiceInfo.WaveSampleRepeatPointer!.Value.SampleData, voiceInfo.WaveSampleRepeatPointer.Value.StartOffset, voiceInfo.LoopLength * 2U);
					channel.SetLoop(voiceInfo.WaveSampleRepeatPointer!.Value.SampleData, voiceInfo.WaveSampleRepeatPointer.Value.StartOffset, voiceInfo.LoopLength * 2U);

					voiceInfo.WaveSampleRepeatLength = voiceInfo.LoopLength;
				}

				if (!voiceInfo.ArpeggioWait)
					voiceInfo.PartNote = 0;

				voiceInfo.Play &= PlayFlag.Retrigger;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the given array is one of this voice's effect
		/// output buffers (between TransformWaveBuffer and FilterWaveBuffer
		/// in the original layout)
		/// </summary>
		/********************************************************************/
		private bool IsEffectBuffer(VoiceInfo voiceInfo, sbyte[] sampleData)
		{
			return (sampleData == voiceInfo.TransformWaveBuffer) || (sampleData == voiceInfo.PhaseWaveBuffer) ||
				   (sampleData == voiceInfo.MixWaveBuffer) || (sampleData == voiceInfo.ResonanceWaveBuffer) ||
				   (sampleData == voiceInfo.FilterWaveBuffer);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ushort GetPeriod(VoiceInfo voiceInfo)
		{
			return GetPeriod2(voiceInfo, voiceInfo.Note);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ushort GetPeriod2(VoiceInfo voiceInfo, ushort note)
		{
			short newNote = (short)(note + voiceInfo.VibratoNote + voiceInfo.PitchSlideNote + voiceInfo.ArpeggioPitchSlideNote + voiceInfo.SemiTone + voiceInfo.FineTune + voiceInfo.PitchAdd);

			if (voiceInfo.Transpose != 0)
				newNote += voiceInfo.Transpose;

			if (newNote < -32)
				newNote = -32;
			else if (newNote > 5 * 12 * 32)
				newNote = 5 * 12 * 32;

			return Tables.PalPitchTable[newNote + 32];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SlideNote(VoiceInfo voiceInfo)
		{
			if (voiceInfo.PtPitchSlide == PartEffect.None)
			{
				if ((voiceInfo.PitchSlide != PartEffect.None) || (voiceInfo.InstrumentPitchSlide != PartEffect.None))
				{
					if (voiceInfo.PitchSlideToNote < 0)
						return;

					if (voiceInfo.PitchSlideType == Direction.Downward)
					{
						voiceInfo.PitchSlideNote -= voiceInfo.PitchSlideSpeed;

						short note = (short)(voiceInfo.Note + voiceInfo.PitchSlideNote - voiceInfo.PitchSlideToNote);
						if (note <= 0)
						{
							voiceInfo.PitchSlideNote = (ushort)(voiceInfo.PitchSlideNote - note);
							voiceInfo.PitchSlideToNote = -1;
						}
					}
					else
					{
						voiceInfo.PitchSlideNote += voiceInfo.PitchSlideSpeed;

						short note = (short)(voiceInfo.Note + voiceInfo.PitchSlideNote - voiceInfo.PitchSlideToNote);
						if (note >= 0)
						{
							voiceInfo.PitchSlideNote = (ushort)(voiceInfo.PitchSlideNote - note);
							voiceInfo.PitchSlideToNote = -1;
						}
					}
				}
			}
			else
			{
				if (!voiceInfo.Play.HasFlag(PlayFlag.FirstRowTick) && (voiceInfo.PtPitchSlideToNote >= 0))
				{
					ushort speed = voiceInfo.PtPitchSlideSpeed;

					if (voiceInfo.PtPitchSlide == PartEffect.PtPortamento)
						speed = voiceInfo.PtPitchSlideSpeed2;

					if (voiceInfo.PtPitchSlideType == Direction.Downward)
					{
						voiceInfo.PtPitchSlideNote += speed;

						short period = (short)(GetPeriod(voiceInfo) + voiceInfo.PtPitchSlideNote - voiceInfo.PtPitchSlideToNote);
						if (period >= 0)
						{
							voiceInfo.PtPitchSlideNote = (ushort)(voiceInfo.PtPitchSlideNote - period);
							voiceInfo.PtPitchSlideToNote = -1;
						}
					}
					else
					{
						voiceInfo.PtPitchSlideNote -= speed;

						short period = (short)(GetPeriod(voiceInfo) + voiceInfo.PtPitchSlideNote - voiceInfo.PtPitchSlideToNote);
						if (period <= 0)
						{
							voiceInfo.PtPitchSlideNote = (ushort)(voiceInfo.PtPitchSlideNote - period);
							voiceInfo.PtPitchSlideToNote = -1;
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SlideArpeggioNote(VoiceInfo voiceInfo)
		{
			if ((voiceInfo.ArpeggioPitchSlide != ArpeggioEffect.None) && (voiceInfo.ArpeggioPitchSlideToNote >= 0))
			{
				if (voiceInfo.ArpeggioPitchSlideType == Direction.Downward)
				{
					voiceInfo.ArpeggioPitchSlideNote -= voiceInfo.ArpeggioPitchSlideSpeed;

					short note = (short)(voiceInfo.ArpeggioCalculatedNote + voiceInfo.ArpeggioPitchSlideNote - voiceInfo.ArpeggioPitchSlideToNote);
					if (note <= 0)
					{
						voiceInfo.ArpeggioPitchSlideNote = (ushort)(voiceInfo.ArpeggioPitchSlideNote - note);
						voiceInfo.ArpeggioPitchSlideToNote = -1;
					}
				}
				else
				{
					voiceInfo.ArpeggioPitchSlideNote += voiceInfo.ArpeggioPitchSlideSpeed;

					short note = (short)(voiceInfo.ArpeggioCalculatedNote + voiceInfo.ArpeggioPitchSlideNote - voiceInfo.ArpeggioPitchSlideToNote);
					if (note >= 0)
					{
						voiceInfo.ArpeggioPitchSlideNote = (ushort)(voiceInfo.ArpeggioPitchSlideNote - note);
						voiceInfo.ArpeggioPitchSlideToNote = -1;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SlideVolume(VoiceInfo voiceInfo)
		{
			short volume;

			if (voiceInfo.VolumeSlide == PartEffect.SlideToInstrumentVolume)
			{
				if (voiceInfo.VolumeSlideToVolumeOff)
					return;

				if (voiceInfo.VolumeSlideType == Direction.Downward)
				{
					volume = (short)(voiceInfo.Volume1 - voiceInfo.VolumeSlideSpeed);
					if (volume < voiceInfo.VolumeSlideToVolume)
					{
						volume = (short)voiceInfo.VolumeSlideToVolume;
						voiceInfo.VolumeSlideToVolumeOff = true;
					}
				}
				else
				{
					volume = (short)(voiceInfo.Volume1 + voiceInfo.VolumeSlideSpeed);
					if (volume > voiceInfo.VolumeSlideToVolume)
					{
						volume = (short)voiceInfo.VolumeSlideToVolume;
						voiceInfo.VolumeSlideToVolumeOff = true;
					}
				}

				voiceInfo.Volume1 = voiceInfo.Volume2 = voiceInfo.Volume3 = (ushort)volume;
			}
			else if ((voiceInfo.VolumeSlide == PartEffect.InstrumentVolumeSlideUp) || ((voiceInfo.VolumeSlide == PartEffect.PtVolumeSlideUp) && !voiceInfo.Play.HasFlag(PlayFlag.FirstRowTick)))
			{
				volume = (short)(voiceInfo.Volume1 + voiceInfo.VolumeSlideSpeed);
				if (volume > 64 * 16)
					volume = 64 * 16;

				voiceInfo.Volume1 = voiceInfo.Volume2 = voiceInfo.Volume3 = (ushort)volume;
			}
			else if ((voiceInfo.VolumeSlide == PartEffect.InstrumentVolumeSlideDown) || ((voiceInfo.VolumeSlide == PartEffect.PtVolumeSlideDown) && !voiceInfo.Play.HasFlag(PlayFlag.FirstRowTick)))
			{
				volume = (short)(voiceInfo.Volume1 - voiceInfo.VolumeSlideSpeed);
				if (volume < 0)
					volume = 0;

				voiceInfo.Volume1 = voiceInfo.Volume2 = voiceInfo.Volume3 = (ushort)volume;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SlideChannelVolume(VoiceInfo voiceInfo)
		{
			short volume;

			if (voiceInfo.ChannelVolumeSlide == PartEffect.SlideToChannelVolume)
			{
				if (voiceInfo.ChannelVolumeSlideToVolumeOff)
					return;

				if (voiceInfo.ChannelVolumeSlideType == Direction.Downward)
				{
					volume = (short)(voiceInfo.ChannelVolume - voiceInfo.ChannelVolumeSlideSpeed);
					if (volume < voiceInfo.ChannelVolumeSlideToVolume)
					{
						volume = (short)voiceInfo.ChannelVolumeSlideToVolume;
						voiceInfo.ChannelVolumeSlideToVolumeOff = true;
					}
				}
				else
				{
					volume = (short)(voiceInfo.ChannelVolume + voiceInfo.ChannelVolumeSlideSpeed);
					if (volume > voiceInfo.ChannelVolumeSlideToVolume)
					{
						volume = (short)voiceInfo.ChannelVolumeSlideToVolume;
						voiceInfo.ChannelVolumeSlideToVolumeOff = true;
					}
				}

				voiceInfo.ChannelVolume = (ushort)volume;
			}
			else if (voiceInfo.ChannelVolumeSlide == PartEffect.ChannelVolumeSlideUp)
			{
				volume = (short)(voiceInfo.ChannelVolume + voiceInfo.ChannelVolumeSlideSpeed);
				if (volume > 64 * 16)
					volume = 64 * 16;

				voiceInfo.ChannelVolume = (ushort)volume;
			}
			else if (voiceInfo.ChannelVolumeSlide == PartEffect.ChannelVolumeSlideDown)
			{
				volume = (short)(voiceInfo.ChannelVolume - voiceInfo.ChannelVolumeSlideSpeed);
				if (volume < 0)
					volume = 0;

				voiceInfo.ChannelVolume = (ushort)volume;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SlideMasterVolume(VoiceInfo voiceInfo)
		{
			short volume;

			if (voiceInfo.MasterVolumeSlide == PartEffect.SlideToMasterVolume)
			{
				if (voiceInfo.MasterVolumeSlideToVolumeOff)
					return;

				if (voiceInfo.MasterVolumeSlideType == Direction.Downward)
				{
					volume = (short)(playingInfo.MasterVolume - voiceInfo.MasterVolumeSlideSpeed);
					if (volume < voiceInfo.MasterVolumeSlideToVolume)
					{
						volume = (short)voiceInfo.MasterVolumeSlideToVolume;
						voiceInfo.MasterVolumeSlideToVolumeOff = true;
					}
				}
				else
				{
					volume = (short)(playingInfo.MasterVolume + voiceInfo.MasterVolumeSlideSpeed);
					if (volume > voiceInfo.MasterVolumeSlideToVolume)
					{
						volume = (short)voiceInfo.MasterVolumeSlideToVolume;
						voiceInfo.MasterVolumeSlideToVolumeOff = true;
					}
				}

				playingInfo.MasterVolume = (ushort)volume;
			}
			else if (voiceInfo.MasterVolumeSlide == PartEffect.MasterVolumeSlideUp)
			{
				volume = (short)(playingInfo.MasterVolume + voiceInfo.MasterVolumeSlideSpeed);
				if (volume > 64 * 16)
					volume = 64 * 16;

				playingInfo.MasterVolume = (ushort)volume;
			}
			else if (voiceInfo.MasterVolumeSlide == PartEffect.MasterVolumeSlideDown)
			{
				volume = (short)(playingInfo.MasterVolume - voiceInfo.MasterVolumeSlideSpeed);
				if (volume < 0)
					volume = 0;

				playingInfo.MasterVolume = (ushort)volume;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SlideArpeggioVolume(VoiceInfo voiceInfo)
		{
			short volume;

			if (voiceInfo.ArpeggioVolumeSlide == ArpeggioEffect.VolumeSlideUp)
			{
				volume = (short)(voiceInfo.Volume1 + voiceInfo.ArpeggioVolumeSlideSpeed);
				if (volume > 64 * 16)
					volume = 64 * 16;

				voiceInfo.Volume1 = voiceInfo.Volume2 = voiceInfo.Volume3 = (ushort)volume;
			}
			else if (voiceInfo.ArpeggioVolumeSlide == ArpeggioEffect.VolumeSlideDown)
			{
				volume = (short)(voiceInfo.Volume1 - voiceInfo.ArpeggioVolumeSlideSpeed);
				if (volume < 0)
					volume = 0;

				voiceInfo.Volume1 = voiceInfo.Volume2 = voiceInfo.Volume3 = (ushort)volume;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ArpeggioPlay(VoiceInfo voiceInfo)
		{
			if (voiceInfo.Arpeggio.HasFlag(ArpeggioFlag.UseTable) || voiceInfo.Arpeggio.HasFlag(ArpeggioFlag.Enabled))
			{
				voiceInfo.ArpeggioSpeedCounter--;
				if (voiceInfo.ArpeggioSpeedCounter != 0)
					return;

				Instrument inst = voiceInfo.Instrument;

				byte spd = inst.ArpeggioSpeed;

				voiceInfo.ArpeggioGroove = !voiceInfo.ArpeggioGroove;
				if (voiceInfo.ArpeggioGroove && (inst.ArpeggioGroove != 0))
					spd = inst.ArpeggioGroove;

				voiceInfo.ArpeggioSpeedCounter = spd;

				ushort arpeggioNumber = inst.ArpeggioTable;
				if (voiceInfo.Arpeggio.HasFlag(ArpeggioFlag.UseTable))
					arpeggioNumber = voiceInfo.ArpeggioTable;

				Arpeggio arpeggio = arpeggios[arpeggioNumber];
				if (arpeggio == null)
					return;

				for (;;)
				{
					byte arpeggioPosition = voiceInfo.ArpeggioPosition++;
					voiceInfo.ArpeggioPosition &= 0x7f;

					ArpeggioLine arpeggioLine = arpeggio.ArpeggioData[arpeggioPosition];

					if (voiceInfo.ArpeggioWait && (arpeggioLine.NoteTranspose == 0))
						return;

					sbyte transpose = arpeggioLine.NoteTranspose;

					if (transpose == 61)
					{
						voiceInfo.Arpeggio = ArpeggioFlag.None;
						voiceInfo.Note = voiceInfo.ArpeggioCalculatedNote;
						return;
					}

					if (transpose == 62)
					{
						if (arpeggioLine.SampleNumber != arpeggioPosition)
							voiceInfo.ArpeggioPosition = arpeggioLine.SampleNumber;

						continue;
					}

					byte sampleNumber = arpeggioLine.SampleNumber;
					if (sampleNumber == 0)
						sampleNumber = inst.SampleNumber;

					voiceInfo.WaveSampleNumber = sampleNumber;
					voiceInfo.Restart = RestartFlag.None;
					voiceInfo.ArpeggioPitchSlide = ArpeggioEffect.None;
					voiceInfo.ArpeggioVolumeSlide = ArpeggioEffect.None;

					for (int i = 0; i < 2; i++)
					{
						ArpeggioEffectEntry effect = arpeggioLine.Effects[i];

						if (effect.Effect == ArpeggioEffect.Restart)
							ArpeggioRestart(voiceInfo);
						else
							DoArpeggioEffect(voiceInfo, effect);
					}

					voiceInfo.Arpeggio &= ~ArpeggioFlag.NoteIsAbsolute;

					if (transpose == 0)
						return;

					if (transpose < 0)
						transpose += (sbyte)(61 + voiceInfo.ArpeggioNote);
					else
						voiceInfo.Arpeggio |= ArpeggioFlag.NoteIsAbsolute;

					voiceInfo.ArpeggioCalculatedNote = voiceInfo.Note = (ushort)(transpose << 5);
					voiceInfo.ArpeggioPitchSlideNote = 0;

					if (voiceInfo.TestAndClearArpWait())
					{
						ArpeggioWaitStart(voiceInfo, arpeggioLine.SampleNumber);
						PlayInstrument(voiceInfo);
						return;
					}

					Sample sample;
					ushort repeatLength;

					if (voiceInfo.Restart.HasFlag(RestartFlag.RestartFromArp))
					{
						voiceInfo.Arpeggio |= ArpeggioFlag.WillSetNote;

						if (arpeggioLine.SampleNumber != 0)
						{
							sample = samples[arpeggioLine.SampleNumber];
							if (sample == null)
								return;

							voiceInfo.WaveSample = sample;
							voiceInfo.Arpeggio |= ArpeggioFlag.UseWaveSample;
						}

						PlayInstrument(voiceInfo);
						return;
					}

					if (arpeggioLine.SampleNumber == 0)
					{
						voiceInfo.SemiTone = (short)(inst.SemiTone << 5);

						if (!voiceInfo.Play.HasFlag(PlayFlag.KeepEffectFineTune))
							voiceInfo.FineTune = inst.FineTune;

						return;
					}

					voiceInfo.Play |= PlayFlag.Retrigger;

					sample = samples[arpeggioLine.SampleNumber];
					if (sample == null)
						return;

					voiceInfo.SemiTone = (short)(sample.SemiTone << 5);

					if (!voiceInfo.Play.HasFlag(PlayFlag.KeepEffectFineTune))
						voiceInfo.FineTune = sample.FineTune;

					voiceInfo.WaveOrSample = sample.SampleType;
					if (voiceInfo.WaveOrSample != SampleType.Sample)
					{
						SampleType sampleType = inst.SampleType;
						if (sampleType == SampleType.Sample)
							sampleType = SampleType.Wave64;

						voiceInfo.WaveOrSample = sampleType;

						FixWaveLength(voiceInfo, inst, sampleType, new SamplePointer(sample.SampleData));
					}
					else
					{
						voiceInfo.WaveSamplePointer = sample.SampleData != null ? new SamplePointer(sample.SampleData) : null;
						voiceInfo.WaveSampleLength = sample.SampleLength;

						SamplePointer? repeatPointer = sample.SampleRepeatPointer;
						repeatLength = sample.SampleRepeatLength;

						if (repeatLength == 0)
						{
							repeatPointer = null;
							repeatLength = 0;
						}

						voiceInfo.WaveSampleRepeatPointer = repeatPointer;
						voiceInfo.WaveSampleRepeatPointerOriginal = repeatPointer;
						voiceInfo.WaveSampleRepeatLength = repeatLength;
					}

					repeatLength = voiceInfo.WaveSampleRepeatLength;

					repeatLength -= 8;
					if (repeatLength != 0)
					{
						repeatLength -= 8;
						if (repeatLength != 0)
						{
							repeatLength -= 16;
							if (repeatLength != 0)
							{
								repeatLength -= 32;
								if (repeatLength != 0)
								{
									repeatLength -= 64;
									if (repeatLength != 0)
									{
										voiceInfo.Effects2.Effect = InstrumentEffect2.None;
										return;
									}
								}
							}
						}
					}

					voiceInfo.Effects2.Effect = inst.Effect2;
					return;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DoArpeggioEffect(VoiceInfo voiceInfo, ArpeggioEffectEntry effect)
		{
			switch (effect.Effect)
			{
				case ArpeggioEffect.SlideUp:
				{
					ArpeggioSlideUp(voiceInfo, effect);
					break;
				}

				case ArpeggioEffect.SlideDown:
				{
					ArpeggioSlideDown(voiceInfo, effect);
					break;
				}

				case ArpeggioEffect.SetVolume:
				{
					ArpeggioSetVolume(voiceInfo, effect);
					break;
				}

				case ArpeggioEffect.VolumeSlideUp:
				case ArpeggioEffect.VolumeSlideDown:
				{
					ArpeggioSlideVolume(voiceInfo, effect);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ArpeggioSlideUp(VoiceInfo voiceInfo, ArpeggioEffectEntry effect)
		{
			voiceInfo.ArpeggioPitchSlide = effect.Effect;
			voiceInfo.ArpeggioPitchSlideType = Direction.Upward;

			if (effect.Argument != 0)
			{
				voiceInfo.ArpeggioPitchSlideSpeed = effect.Argument;
				voiceInfo.ArpeggioPitchSlideToNote = (59 * 32) + 32;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ArpeggioSlideDown(VoiceInfo voiceInfo, ArpeggioEffectEntry effect)
		{
			voiceInfo.ArpeggioPitchSlide = effect.Effect;
			voiceInfo.ArpeggioPitchSlideType = Direction.Downward;

			if (effect.Argument != 0)
			{
				voiceInfo.ArpeggioPitchSlideSpeed = effect.Argument;
				voiceInfo.ArpeggioPitchSlideToNote = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ArpeggioSetVolume(VoiceInfo voiceInfo, ArpeggioEffectEntry effect)
		{
			ushort volume = (ushort)(effect.Argument << 4);

			if (volume > 64 * 16)
				volume = 64 * 16;

			voiceInfo.Restart |= RestartFlag.ArpHasSetVolume;
			voiceInfo.Volume1 = volume;
			voiceInfo.Volume2 = volume;
			voiceInfo.Volume3 = volume;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ArpeggioSlideVolume(VoiceInfo voiceInfo, ArpeggioEffectEntry effect)
		{
			voiceInfo.ArpeggioVolumeSlide = effect.Effect;

			if (effect.Argument != 0)
				voiceInfo.ArpeggioVolumeSlideSpeed = effect.Argument;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ArpeggioRestart(VoiceInfo voiceInfo)
		{
			voiceInfo.Restart |= RestartFlag.RestartFromArp;

			if (voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.PhaseInit))
				voiceInfo.PhaseInit = 0;

			// The original code uses ResonanceInit which exists
			// in Effect2, but it is tested on Effect1. I guess
			// this is a bug, but I keep it to be compatible with
			// the original player
			if (voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.PhaseStep))
				voiceInfo.ResonanceInit = 0;

			if (voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.FilterInit))
				voiceInfo.FilterInit = 0;

			if (voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.TransformInit))
				voiceInfo.TransformInit = 0;

			// Same bug as above, but this time with MixInit
			if (voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.EnvelopeHoldSustain))
				voiceInfo.MixInit = 0;

			if (voiceInfo.Effects2.Argument.HasFlag(MixFlag.LoopInit))
				voiceInfo.LoopInit = 0;

		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void EnvelopePlay(VoiceInfo voiceInfo)
		{
			if (voiceInfo.Effects1.Effect.HasFlag(InstrumentEffect1.Envelope))
			{
				ushort[] dataArray = voiceInfo.EnvelopeData;
				int index = 0;

				ushort data = dataArray[index++];
				if (data == 0)
				{
					data = dataArray[index++];
					if (data == 0)
					{
						data = dataArray[index++];
						if (data == 0)
						{
							if (voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.EnvelopeHoldSustain))
							{
								data = dataArray[index + 7];
								voiceInfo.Volume3 = (ushort)((data * voiceInfo.Volume2) >> 6);

								if (voiceInfo.Volume3 > 1024)
									voiceInfo.Volume3 = 1024;

								return;
							}

							data = dataArray[index++];
							if (data == 0)
							{
								data = dataArray[index + 7];
								voiceInfo.Volume3 = (ushort)((data * voiceInfo.Volume2) >> 6);

								if (voiceInfo.Volume3 > 1024)
									voiceInfo.Volume3 = 1024;

								return;
							}
						}
					}
				}

				ushort volume = (ushort)(voiceInfo.EnvelopeVolume + (short)dataArray[index + 3]);
				voiceInfo.EnvelopeVolume = volume;
				volume >>= 8;

				data--;
				dataArray[--index] = data;

				if (data == 0)
					volume = dataArray[index + 8];

				voiceInfo.Volume3 = (ushort)((volume * voiceInfo.Volume2) >> 6);

				if (voiceInfo.Volume3 > 1024)
					voiceInfo.Volume3 = 1024;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TremoloPlay(VoiceInfo voiceInfo)
		{
			if (voiceInfo.Tremolo == PartEffect.PtVibrato)
			{
				if (voiceInfo.Play.HasFlag(PlayFlag.FirstRowTick))
					return;

				byte position = (byte)((voiceInfo.PtTremoloPosition >> 2) & 0x1f);
				byte waveData;

				switch (voiceInfo.PtTremoloWaveType)
				{
					case PtWaveType.Sine:
					{
						waveData = Tables.PtVibratoTable[position];
						break;
					}

					case PtWaveType.RampDown:
					{
						position <<= 3;

						if (voiceInfo.PtTremoloPosition < 0)
							waveData = (byte)(255 - position);
						else
							waveData = position;

						break;
					}

					default:
					{
						waveData = 255;
						break;
					}
				}

				int tremoloValue = ((voiceInfo.PtTremoloCommand & 15) * waveData) >> 2;

				if (voiceInfo.PtTremoloPosition < 0)
				{
					if (tremoloValue >= 0)
						tremoloValue = -tremoloValue;
				}
				else
				{
					if (tremoloValue < 0)
						tremoloValue = -tremoloValue;
				}

				int volume = voiceInfo.Volume1;
				if (volume != 0)
				{
					volume += tremoloValue;

					if (volume < 0)
						volume = 0;
					else if (volume > 64 * 16)
						volume = 64 * 16;
				}

				voiceInfo.Volume2 = (ushort)volume;
				voiceInfo.Volume3 = (ushort)volume;

				voiceInfo.PtTremoloPosition += (sbyte)((voiceInfo.PtTremoloCommand >> 2) & 0x3c);
			}
			else
			{
				if (voiceInfo.Tremolo == PartEffect.None)
				{
					if (!voiceInfo.Effects1.Effect.HasFlag(InstrumentEffect1.Tremolo))
						return;

					if (voiceInfo.TremoloCommandDelay != 0)
					{
						voiceInfo.TremoloCommandDelay--;
						return;
					}
				}

				if (voiceInfo.TremoloAttackLength == 0)
					voiceInfo.TremoloCommandDepth = voiceInfo.TremoloDepth;
				else
				{
					voiceInfo.TremoloCommandDepth += voiceInfo.TremoloAttackSpeed;

					voiceInfo.TremoloAttackLength--;
					if (voiceInfo.TremoloAttackLength == 0)
						voiceInfo.TremoloCommandDepth = voiceInfo.TremoloDepth;
				}

				ushort tremoloCounter = (ushort)(voiceInfo.TremoloCounter >> 2);
				ushort tremoloCommandSpeed = voiceInfo.TremoloCommandSpeed;
				ushort tremoloCommandDepth = (ushort)(voiceInfo.TremoloCommandDepth >> 8);
				sbyte[] waveTypeTable = GetWaveTypeTable(voiceInfo.TremoloWaveType);

				sbyte waveData = waveTypeTable[tremoloCounter];

				if (voiceInfo.TremoloDirection == Direction.Downward)
					waveData = (sbyte)-waveData;

				short tremoloValue = (short)((waveData * tremoloCommandDepth) >> 1);
				if (tremoloValue < 0)
					tremoloValue += 16;

				int volume = voiceInfo.Volume1;
				if (voiceInfo.Volume1 != 0)
				{
					volume += tremoloValue;

					if (volume < 0)
						volume = 0;
					else if (volume > 64 * 16)
						volume = 64 * 16;
				}

				voiceInfo.Volume2 = (ushort)volume;
				voiceInfo.Volume3 = (ushort)volume;

				voiceInfo.TremoloCounter = (ushort)((voiceInfo.TremoloCounter + tremoloCommandSpeed) & 0x1ff);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void VibratoPlay(VoiceInfo voiceInfo)
		{
			if (voiceInfo.Vibrato == PartEffect.PtVibrato)
			{
				if (voiceInfo.Play.HasFlag(PlayFlag.FirstRowTick))
					return;

				byte position = (byte)((voiceInfo.PtVibratoPosition >> 2) & 0x1f);
				byte waveData;

				switch (voiceInfo.PtVibratoWaveType)
				{
					case PtWaveType.Sine:
					{
						waveData = Tables.PtVibratoTable[position];
						break;
					}

					case PtWaveType.RampDown:
					{
						position <<= 3;

						if (voiceInfo.PtVibratoPosition < 0)
							waveData = (byte)(255 - position);
						else
							waveData = position;

						break;
					}

					default:
					{
						waveData = 255;
						break;
					}
				}

				int vibratoValue = ((voiceInfo.PtVibratoCommand & 15) * waveData) >> 7;

				if (voiceInfo.PtVibratoPosition < 0)
				{
					if (vibratoValue >= 0)
						vibratoValue = -vibratoValue;
				}
				else
				{
					if (vibratoValue < 0)
						vibratoValue = -vibratoValue;
				}

				voiceInfo.PtVibratoNote = (ushort)vibratoValue;
				voiceInfo.PtVibratoPosition += (sbyte)((voiceInfo.PtVibratoCommand >> 2) & 0x3c);
			}
			else
			{
				if (voiceInfo.Vibrato == PartEffect.None)
				{
					if (!voiceInfo.Effects1.Effect.HasFlag(InstrumentEffect1.Vibrato))
						return;

					if (voiceInfo.VibratoCommandDelay != 0)
					{
						voiceInfo.VibratoCommandDelay--;
						return;
					}
				}

				if (voiceInfo.VibratoAttackLength == 0)
					voiceInfo.VibratoCommandDepth = voiceInfo.VibratoDepth;
				else
				{
					voiceInfo.VibratoCommandDepth += voiceInfo.VibratoAttackSpeed;

					voiceInfo.VibratoAttackLength--;
					if (voiceInfo.VibratoAttackLength == 0)
						voiceInfo.VibratoCommandDepth = voiceInfo.VibratoDepth;
				}

				ushort vibratoCounter = (ushort)(voiceInfo.VibratoCounter >> 2);
				ushort vibratoCommandSpeed = voiceInfo.VibratoCommandSpeed;
				ushort vibratoCommandDepth = (ushort)(voiceInfo.VibratoCommandDepth >> 8);
				sbyte[] waveTypeTable = GetWaveTypeTable(voiceInfo.VibratoWaveType);

				sbyte waveData = waveTypeTable[vibratoCounter];

				if (voiceInfo.VibratoDirection == Direction.Downward)
					waveData = (sbyte)-waveData;

				short vibratoNote = (short)((waveData * vibratoCommandDepth) >> 4);
				if (vibratoNote < 0)
					vibratoNote++;

				voiceInfo.VibratoNote = (ushort)vibratoNote;
				voiceInfo.VibratoCounter = (ushort)((voiceInfo.VibratoCounter + vibratoCommandSpeed) & 0x1ff);
			}
		}
	}
}
