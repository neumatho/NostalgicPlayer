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
	/// The player itself
	/// </summary>
	internal partial class MusiclineEditorWorker
	{
		private bool playingEnabled;

		/********************************************************************/
		/// <summary>
		/// Stop playing
		/// </summary>
		/********************************************************************/
		private void StopPlay()
		{
			foreach (VoiceInfo voiceInfo in voices)
				voiceInfo.WaveSampleNumberOld = 0;

			playingEnabled = false;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		private void PlayModule()
		{
			if (playingEnabled)
			{
				PlayTune();
				PlayEffects();
				PeriodCalculation();
				PeriodVolumePlay();
				DmaPlay();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Tell NostalgicPlayer what to be played
		/// </summary>
		/********************************************************************/
		private void DmaPlay()
		{
			for (int i = 0; i < numberOfChannels; i++)
			{
				VoiceInfo voiceInfo = voices[i];
				IChannel channel = VirtualChannels[i];

				if (voiceInfo.Play.HasFlag(PlayFlag.Retrigger))
				{
					bool retrig = false;

					byte sampleNumber = voiceInfo.WaveSampleNumber;
					byte oldSampleNumber = voiceInfo.WaveSampleNumberOld;
					voiceInfo.WaveSampleNumberOld = sampleNumber;

					if ((voiceInfo.WaveOrSample == SampleType.Sample) || (sampleNumber != oldSampleNumber))
						retrig = true;

					// Dma1
					voiceInfo.Play &= ~PlayFlag.Retrigger;

					channel.SetAmigaVolume((ushort)((((voiceInfo.Volume3 * voiceInfo.ChannelVolume) / 1024) * playingInfo.MasterVolume) / 16384));
					channel.SetAmigaPeriod(voiceInfo.Period2);

					if (retrig)
					{
						if (voiceInfo.WaveSamplePointer.HasValue)
						{
							uint startOffset = voiceInfo.WaveSamplePointer.Value.StartOffset;
							uint length = voiceInfo.WaveSampleLength * 2U;

							if ((startOffset + length) >= voiceInfo.WaveSamplePointer.Value.SampleData.Length)
								length = (uint)(voiceInfo.WaveSamplePointer!.Value.SampleData.Length - startOffset);

							channel.PlaySample(sampleNumber, voiceInfo.WaveSamplePointer.Value.SampleData, startOffset, length);
						}
					}
					else
					{
						// This is not needed for the music to play correctly, but only
						// to tell the visualizers that a new note has started
						channel.SetSampleNumber(sampleNumber);
					}

					// Dma2
					if (voiceInfo.WaveSampleRepeatLength != 0)
					{
						uint startOffset = voiceInfo.WaveSampleRepeatPointer!.Value.StartOffset;
						uint length = voiceInfo.WaveSampleRepeatLength * 2U;

						if ((startOffset + length) >= voiceInfo.WaveSampleRepeatPointer!.Value.SampleData.Length)
							length = (uint)(voiceInfo.WaveSampleRepeatPointer!.Value.SampleData.Length - startOffset);

						channel.SetLoop(voiceInfo.WaveSampleRepeatPointer!.Value.SampleData, startOffset, length);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse the next track data
		/// </summary>
		/********************************************************************/
		private void PlayTune()
		{
			for (int i = 0; i < numberOfChannels; i++)
				PlayVoice(voices[i], currentSong.Sequences[i]);
		}



		/********************************************************************/
		/// <summary>
		/// Parse the next track data for a single voice
		/// </summary>
		/********************************************************************/
		private void PlayVoice(VoiceInfo voiceInfo, ushort[] sequence)
		{
			if (voiceInfo.VoiceOff)
				return;

			voiceInfo.SpeedCounter--;
			if (voiceInfo.SpeedCounter > 0)
				return;

			voiceInfo.PartPositionWork = voiceInfo.PartPosition;

			byte spd = voiceInfo.Speed;

			voiceInfo.PartGroove = !voiceInfo.PartGroove;
			if (voiceInfo.PartGroove && (voiceInfo.Groove != 0))
				spd = voiceInfo.Groove;

			voiceInfo.SpeedCounter = spd;
			voiceInfo.SpeedPart = 0;
			voiceInfo.GroovePart = 0;

			if (playingInfo.CurrentSpeed != spd)
			{
				playingInfo.CurrentSpeed = spd;
				ShowSpeed();
			}

			int loopError = 256;

			while (--loopError > 0)
			{
				byte tunePosition = voiceInfo.TunePosition;
				ushort sequenceData = sequence[tunePosition];

				if ((sequenceData & 0x20) != 0)
				{
					if (ParseControlCommand(voiceInfo, sequenceData))
						break;
				}
				else
				{
					if (ParsePart(voiceInfo, sequenceData))
						break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse sequence control command
		/// </summary>
		/********************************************************************/
		private bool ParseControlCommand(VoiceInfo voiceInfo, ushort sequenceData)
		{
			byte sequenceArgument = (byte)(sequenceData & 0x1f);
			SequenceCommand command = (SequenceCommand)((sequenceData & 0xc0) >> 6);

			switch (command)
			{
				case SequenceCommand.End:
				{
					voiceInfo.VoiceOff = true;

					OnEndReached(voiceInfo.ChannelNumber);

					return true;
				}

				case SequenceCommand.Jump:
				{
					byte oldPosition = voiceInfo.TunePosition;

					try
					{
						byte target = (byte)((sequenceData & 0xff00) >> 8);

						if (voiceInfo.TuneJumpCounter != 0)
						{
							// TNE: TuneJumpPosition has been added to prevent Sonde Indure infinity looping
							if ((voiceInfo.TuneJumpPosition != -1) && (voiceInfo.TunePosition != voiceInfo.TuneJumpPosition))
							{
								voiceInfo.TunePosition++;
								return false;
							}

							voiceInfo.TuneJumpCounter--;

							if (voiceInfo.TuneJumpCounter != 0)
								voiceInfo.TunePosition = target;
							else
								voiceInfo.TunePosition++;

							return false;
						}

						if (voiceInfo.TunePosition > target)
						{
							voiceInfo.TuneJumpPosition = voiceInfo.TunePosition;
							voiceInfo.TuneJumpCounter = sequenceArgument;
							voiceInfo.TunePosition = target;

							if (sequenceArgument == 0)
							{
								// Capture this channel's loop period the first time it wraps back.
								// Each channel can loop independently with its own period; the
								// overall song restart time is derived from the longest of these
								if (channelLoopLengths[voiceInfo.ChannelNumber] == TimeSpan.Zero)
								{
									TimeSpan? currentTime = GetCurrentTime();
									if (currentTime.HasValue && positionTimes[voiceInfo.ChannelNumber].TryGetValue(target, out TimeSpan firstVisit))
										channelLoopLengths[voiceInfo.ChannelNumber] = currentTime.Value - firstVisit;
								}

								OnEndReached(voiceInfo.ChannelNumber);
							}
						}
						else
						{
							voiceInfo.TunePosition++;
							voiceInfo.TuneJumpPosition = -1;
						}

						return false;
					}
					finally
					{
						if (voiceInfo.TunePosition != oldPosition)
						{
							ShowChannelPositions();
							ShowTracks();
							SetPositionTime(voiceInfo);
						}
					}
				}

				case SequenceCommand.Wait:
				{
					if (voiceInfo.TuneWait != 0)
					{
						voiceInfo.TuneWait--;

						if (voiceInfo.TuneWait != 0)
							return true;

						voiceInfo.TunePosition++;
						ShowChannelPositions();
						ShowTracks();
						SetPositionTime(voiceInfo);

						return false;
					}

					byte waitCount = (byte)((sequenceData & 0xff00) >> 8);
					voiceInfo.TuneWait = waitCount;

					if (waitCount == 0)
					{
						voiceInfo.TunePosition++;
						ShowChannelPositions();
						ShowTracks();
						SetPositionTime(voiceInfo);

						return false;
					}

					voiceInfo.PtPitchSlide = 0;
					voiceInfo.PitchSlide = 0;
					voiceInfo.VolumeSlide = PartEffect.None;
					voiceInfo.PartNote = 0;
					voiceInfo.PartInstrument = 0;

					if (sequenceArgument != 0)
					{
						voiceInfo.Speed = sequenceArgument;
						voiceInfo.SpeedCounter = sequenceArgument;
					}

					return true;
				}

				default:
					return ParsePart(voiceInfo, sequenceData);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse part
		/// </summary>
		/********************************************************************/
		private bool ParsePart(VoiceInfo voiceInfo, ushort sequenceData)
		{
			voiceInfo.PartNumber = (ushort)(((sequenceData << 2) & 0x300) | ((sequenceData & 0xff00) >> 8));
			voiceInfo.TransposeNumber = (byte)((sequenceData & 0x1f) - 16);

			Part part = parts[voiceInfo.PartNumber];

			for (;;)
			{
				byte partPosition = voiceInfo.PartPosition;

				voiceInfo.PartPosition++;
				voiceInfo.PartPosition &= 0x7f;

				if (voiceInfo.PartPosition == 0)
				{
					voiceInfo.TunePosition++;
					ShowChannelPositions();
					ShowTracks();
					SetPositionTime(voiceInfo);

					voiceInfo.Speed = playingInfo.TuneSpeed;
					voiceInfo.Groove = playingInfo.TuneGroove;
				}

				PartLine line = part.PartData[partPosition];
				voiceInfo.PartNote = line.Note;
				voiceInfo.PartInstrument = line.Instrument;

				for (int i = 0; i < 5; i++)
					voiceInfo.PartEffects[i] = line.Effects[i];

				if (voiceInfo.PartNote == 61)
				{
					if (partPosition == 0)
					{
						StopPlay();
						OnEndReached(voiceInfo.ChannelNumber);

						return true;
					}

					voiceInfo.PartPosition = 0;
					voiceInfo.PartPositionWork = 0;
					voiceInfo.Speed = playingInfo.TuneSpeed;
					voiceInfo.Groove = playingInfo.TuneGroove;
					voiceInfo.SpeedCounter = voiceInfo.Speed;

					voiceInfo.TunePosition++;
					ShowChannelPositions();
					ShowTracks();
					SetPositionTime(voiceInfo);

					return false;
				}

				byte jumpPosition = (byte)(voiceInfo.PartNote & 0x7f);

				if ((voiceInfo.PartNote & 0x80) != 0)
				{
					if (voiceInfo.PartJumpCounter != 0)
					{
						voiceInfo.PartJumpCounter--;

						if (voiceInfo.PartJumpCounter != 0)
						{
							voiceInfo.PartPosition = jumpPosition;
							voiceInfo.PartPositionWork = jumpPosition;
						}
					}
					else
					{
						if (partPosition > jumpPosition)
						{
							voiceInfo.PartJumpCounter = voiceInfo.PartInstrument;
							voiceInfo.PartPosition = jumpPosition;
							voiceInfo.PartPositionWork = jumpPosition;

							if (voiceInfo.PartJumpCounter == 0)
								OnEndReached(voiceInfo.ChannelNumber);
						}
					}
				}
				else
				{
					if (jumpPosition != 0)
						voiceInfo.Transpose = (short)((sbyte)voiceInfo.TransposeNumber << 5);

					CheckInstrument(voiceInfo);
					return true;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CheckInstrument(VoiceInfo voiceInfo)
		{
			if (voiceInfo.PartInstrument != 0)
			{
				Instrument inst = instruments[voiceInfo.PartInstrument];
				if (inst != null)
				{
					voiceInfo.Instrument = inst;

					if (voiceInfo.PartInstrument != voiceInfo.OldInstrument)
					{
						voiceInfo.Arpeggio = ArpeggioFlag.None;
						voiceInfo.InstrumentPitchSlide = PartEffect.None;
						voiceInfo.OldInstrument = voiceInfo.PartInstrument;
					}
				}
			}

			PlayPartFx(voiceInfo);
			PlayArpeggio(voiceInfo);
			PlayInstrument(voiceInfo);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PlayArpeggio(VoiceInfo voiceInfo)
		{
			if ((voiceInfo.PartNote == 0) || (voiceInfo.Instrument == null))
				return;

			Instrument inst = voiceInfo.Instrument;

			voiceInfo.ArpeggioWait = false;

			if (!voiceInfo.Arpeggio.HasFlag(ArpeggioFlag.UseTable) && !voiceInfo.Arpeggio.HasFlag(ArpeggioFlag.Enabled))
			{
				if (!inst.Effect1.HasFlag(InstrumentEffect1.Arpeggio))
					return;

				voiceInfo.Arpeggio |= ArpeggioFlag.Enabled;
			}

			if ((voiceInfo.Restart == RestartFlag.None) && (voiceInfo.PartInstrument == 0))
				return;

			voiceInfo.Arpeggio |= ArpeggioFlag.WillSetNote;

			ushort arpeggioNumber = inst.ArpeggioTable;
			if (voiceInfo.Arpeggio.HasFlag(ArpeggioFlag.UseTable))
				arpeggioNumber = voiceInfo.ArpeggioTable;

			Arpeggio arpeggio = arpeggios[arpeggioNumber];
			if (arpeggio == null)
				return;

			voiceInfo.ArpeggioPosition = 0;
			voiceInfo.ArpeggioWait = false;
			voiceInfo.ArpeggioVolumeSlide = ArpeggioEffect.None;
			voiceInfo.ArpeggioPitchSlide = ArpeggioEffect.None;
			voiceInfo.ArpeggioPitchSlideNote = 0;
			voiceInfo.ArpeggioSpeedCounter = inst.ArpeggioSpeed;

			for (;;)
			{
				byte arpeggioPosition = voiceInfo.ArpeggioPosition++;
				voiceInfo.ArpeggioPosition &= 0x7f;

				ArpeggioLine arpeggioLine = arpeggio.ArpeggioData[arpeggioPosition];

				voiceInfo.ArpeggioNote = voiceInfo.PartNote;

				sbyte transpose = arpeggioLine.NoteTranspose;

				if (transpose == 0)
				{
					voiceInfo.ArpeggioWait = true;
					return;
				}

				if (transpose == 61)
				{
					voiceInfo.Effects1.Effect &= ~InstrumentEffect1.Arpeggio;
					return;
				}

				if (transpose == 62)
					continue;

				byte sampleNumber = arpeggioLine.SampleNumber;
				if (sampleNumber == 0)
					sampleNumber = inst.SampleNumber;

				voiceInfo.WaveSampleNumber = sampleNumber;

				for (int i = 0; i < 2; i++)
					DoArpeggioEffect(voiceInfo, arpeggioLine.Effects[i]);

				if (transpose < 0)
					transpose += (sbyte)(61 + voiceInfo.ArpeggioNote);
				else
					voiceInfo.Arpeggio |= ArpeggioFlag.NoteIsAbsolute;

				voiceInfo.ArpeggioCalculatedNote = voiceInfo.Note = (ushort)(transpose << 5);

				ArpeggioWaitStart(voiceInfo, arpeggioLine.SampleNumber);
				return;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ArpeggioWaitStart(VoiceInfo voiceInfo, byte sampleNumber)
		{
			if (sampleNumber == 0)
				return;

			Sample sample = samples[sampleNumber];
			if (sample == null)
				return;

			voiceInfo.WaveSample = sample;
			voiceInfo.Arpeggio |= ArpeggioFlag.UseWaveSample;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PlayInstrument(VoiceInfo voiceInfo)
		{
			Instrument inst = voiceInfo.Instrument;

			if (voiceInfo.ArpeggioWait || (inst == null))
				return;

			InstrumentSampleBase instSamp = inst;
			SampleType sampleType = inst.SampleType;

			if ((voiceInfo.Restart & (RestartFlag.RestartFromPartFx | RestartFlag.RestartFromArp)) != 0)
				voiceInfo.WaveSampleNumber = inst.SampleNumber;
			else
			{
				if (voiceInfo.PartInstrument == 0)
					goto PlayNote;

				if ((voiceInfo.PitchSlide == PartEffect.Portamento) || (voiceInfo.PartNote == 0))
				{
					voiceInfo.Volume1 = (ushort)(inst.Volume * 16);
					goto PlayNote;
				}

				if (voiceInfo.Arpeggio.HasFlag(ArpeggioFlag.UseWaveSample))
				{
					instSamp = voiceInfo.WaveSample;
					if (instSamp == null)
						return;

					if (instSamp.SampleType == SampleType.Sample)
						sampleType = instSamp.SampleType;
				}
				else
					voiceInfo.WaveSampleNumber = inst.SampleNumber;
			}

			if (!instSamp.SamplePointer.HasValue)
				return;

			SamplePointer samplePointer = instSamp.SamplePointer.Value;

			if (!inst.Transposable)
				voiceInfo.Transpose = 0;

			InstrumentFlag flag = inst.EnvTraPhaFilBits;

			if (voiceInfo.Effects1.Effect.HasFlag(InstrumentEffect1.HoldSustain))
			{
				flag &= ~InstrumentFlag.EnvelopeHoldSustain;
				voiceInfo.Effects1.Argument &= InstrumentFlag.EnvelopeHoldSustain;
				flag |= voiceInfo.Effects1.Argument;
			}

			voiceInfo.Effects1.Argument = flag;
			voiceInfo.Effects2.Argument = inst.MixResLooBits;
			voiceInfo.Effects1.Effect = inst.Effect1;
			voiceInfo.Effects2.Effect = inst.Effect2;

			voiceInfo.WaveOrSample = sampleType;
			if (sampleType != SampleType.Sample)
			{
				// Wave
				FixWaveLength(voiceInfo, inst, sampleType, samplePointer);
			}
			else
			{
				// Normal sample
				ushort sampleLength = instSamp.SampleLength;

				if (voiceInfo.SampleOffsetActive)
				{
					ushort offset = (ushort)(voiceInfo.SampleOffset << 7);

					if (offset < sampleLength)
					{
						sampleLength -= offset;
						samplePointer.StartOffset += offset * 2U;
					}
					else
						sampleLength = 1;
				}

				voiceInfo.WaveSamplePointer = samplePointer;
				voiceInfo.WaveSampleLength = sampleLength;

				SamplePointer? repeatPointer = instSamp.SampleRepeatPointer;
				ushort repeatLength = instSamp.SampleRepeatLength;

				if (!voiceInfo.Arpeggio.HasFlag(ArpeggioFlag.UseWaveSample) && !inst.Effect1.HasFlag(InstrumentEffect1.WaveSampleLoop))
				{
					// No loop
					repeatPointer = null;
					repeatLength = 0;
				}

				voiceInfo.WaveSampleRepeatPointer = repeatPointer;
				voiceInfo.WaveSampleRepeatPointerOriginal = repeatPointer;
				voiceInfo.WaveSampleRepeatLength = repeatLength;
			}

			voiceInfo.Restart &= RestartFlag.RestartFromPartFx | RestartFlag.RestartFromArp | RestartFlag.ArpHasSetVolume;
			if (voiceInfo.Restart == RestartFlag.None)
				voiceInfo.Volume1 = (ushort)(inst.Volume * 16);

			PlayNote:
			short volume = (short)(voiceInfo.Volume != PartEffect.None ? voiceInfo.VolumeSet : voiceInfo.Volume1);

			if (voiceInfo.VolumeAdd != PartEffect.None)
			{
				volume += voiceInfo.VolumeAddNumber;

				if (volume < 0)
					volume = 0;
				else if (volume > 64 * 16)
					volume = 64 * 16;
			}

			voiceInfo.Volume1 = voiceInfo.Volume2 = voiceInfo.Volume3 = (ushort)volume;

			if (voiceInfo.PitchSlide == PartEffect.Portamento)
				return;

			if ((inst.SlideSpeed != 0) && (voiceInfo.PartNote != 0))
			{
				voiceInfo.PitchSlideSpeed = inst.SlideSpeed;

				ushort newNote = (ushort)(voiceInfo.Note + voiceInfo.PitchSlideNote);
				voiceInfo.PitchSlideToNote = (short)(voiceInfo.PartNote << 5);
				voiceInfo.PitchSlideType = voiceInfo.PitchSlideToNote < newNote ? Direction.Downward : Direction.Upward;

				if (voiceInfo.InstrumentPitchSlide != PartEffect.None)
				{
					InstPlay(voiceInfo, inst);
					return;
				}

				voiceInfo.InstrumentPitchSlide = PartEffect.Portamento;
			}

			if (!voiceInfo.TestAndClearArpFlag(ArpeggioFlag.WillSetNote))
			{
				if (voiceInfo.PartNote == 0)
					return;

				voiceInfo.Note = (ushort)(voiceInfo.PartNote << 5);
				voiceInfo.ArpeggioVolumeSlide = ArpeggioEffect.None;
				voiceInfo.ArpeggioPitchSlide = ArpeggioEffect.None;
				voiceInfo.ArpeggioPitchSlideNote = 0;
			}

			voiceInfo.SemiTone = (short)(instSamp.SemiTone << 5);

			if (!voiceInfo.Play.HasFlag(PlayFlag.KeepEffectFineTune))
				voiceInfo.FineTune = instSamp.FineTune;

			voiceInfo.PtPitchSlideNote = 0;
			voiceInfo.PitchSlideNote = 0;
			voiceInfo.PtVibratoNote = 0;
			voiceInfo.PtTremoloPosition = 0;
			voiceInfo.PtVibratoPosition = 0;
			voiceInfo.VibratoNote = 0;
			voiceInfo.PtPitchAdd = 0;
			voiceInfo.PitchAdd = 0;

			if ((voiceInfo.PartInstrument != 0) || ((voiceInfo.Restart & (RestartFlag.RestartFromPartFx | RestartFlag.RestartFromArp)) != 0))
				InstPlay(voiceInfo, inst);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FixWaveLength(VoiceInfo voiceInfo, Instrument inst, SampleType sampleType, SamplePointer samplePointer)
		{
			ushort sampleLength = inst.SampleLength;

			switch (sampleType)
			{
				case SampleType.Wave16:
				{
					samplePointer.StartOffset += 256 + 128 + 64 + 32;
					sampleLength >>= 4;
					break;
				}

				case SampleType.Wave32:
				{
					samplePointer.StartOffset += 256 + 128 + 64;
					sampleLength >>= 3;
					break;
				}

				case SampleType.Wave64:
				{
					samplePointer.StartOffset += 256 + 128;
					sampleLength >>= 2;
					break;
				}

				case SampleType.Wave128:
				{
					samplePointer.StartOffset += 256;
					sampleLength >>= 1;
					break;
				}
			}

			voiceInfo.WaveSamplePointer = samplePointer;
			voiceInfo.WaveSampleRepeatPointer = samplePointer;
			voiceInfo.WaveSampleRepeatPointerOriginal = samplePointer;
			voiceInfo.WaveSampleLength = sampleLength;
			voiceInfo.WaveSampleRepeatLength = sampleLength;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void InstPlay(VoiceInfo voiceInfo, Instrument inst)
		{
			if ((voiceInfo.PitchSlide != PartEffect.Portamento) && !voiceInfo.ArpeggioWait)
			{
				voiceInfo.Play |= PlayFlag.Retrigger;

				if ((voiceInfo.Vibrato == PartEffect.None) && voiceInfo.Effects1.Effect.HasFlag(InstrumentEffect1.Vibrato))
				{
					voiceInfo.VibratoCounter = 0;
					voiceInfo.VibratoCommandDepth = 0;

					voiceInfo.VibratoCommandSpeed = inst.VibratoSpeed;
					voiceInfo.VibratoCommandDelay = inst.VibratoDelay;
					voiceInfo.VibratoAttackSpeed = inst.VibratoAttackSpeed;
					voiceInfo.VibratoAttackLength = inst.VibratoAttack;
					voiceInfo.VibratoDepth = inst.VibratoDepth;
					voiceInfo.VibratoWaveType = inst.VibratoWaveType;
					voiceInfo.VibratoDirection = inst.VibratoDirection;
				}
				else if (!voiceInfo.Effects1.Effect.HasFlag(InstrumentEffect1.Vibrato))
				{
					// TNE: This fix has been added, so Fletch Dance won't make some funny
					// sounds. The original player also make the sounds, so this is a
					// real bug fix
					voiceInfo.VibratoCommandSpeed = 0;
				}

				if ((voiceInfo.Tremolo == PartEffect.None) && voiceInfo.Effects1.Effect.HasFlag(InstrumentEffect1.Tremolo))
				{
					voiceInfo.TremoloCounter = 0;
					voiceInfo.TremoloCommandDepth = 0;

					voiceInfo.TremoloCommandSpeed = inst.TremoloSpeed;
					voiceInfo.TremoloCommandDelay = inst.TremoloDelay;
					voiceInfo.TremoloAttackSpeed = inst.TremoloAttackSpeed;
					voiceInfo.TremoloAttackLength = inst.TremoloAttack;
					voiceInfo.TremoloDepth = inst.TremoloDepth;
					voiceInfo.TremoloWaveType = inst.TremoloWaveType;
					voiceInfo.TremoloDirection = inst.TremoloDirection;
				}

				if (voiceInfo.Effects1.Effect.HasFlag(InstrumentEffect1.Envelope))
				{
					voiceInfo.EnvelopeVolume = 0;

					voiceInfo.EnvelopeData[0] = inst.EnvelopeAttackLength;
					voiceInfo.EnvelopeData[1] = inst.EnvelopeDecayLength;
					voiceInfo.EnvelopeData[2] = inst.EnvelopeSustainLength;
					voiceInfo.EnvelopeData[3] = inst.EnvelopeReleaseLength;
					voiceInfo.EnvelopeData[4] = inst.EnvelopeAttackSpeed;
					voiceInfo.EnvelopeData[5] = inst.EnvelopeDecaySpeed;
					voiceInfo.EnvelopeData[6] = inst.EnvelopeSustainSpeed;
					voiceInfo.EnvelopeData[7] = inst.EnvelopeReleaseSpeed;
					voiceInfo.EnvelopeData[8] = inst.EnvelopeAttackVolume;
					voiceInfo.EnvelopeData[9] = inst.EnvelopeDecayVolume;
					voiceInfo.EnvelopeData[10] = inst.EnvelopeSustainVolume;
					voiceInfo.EnvelopeData[11] = inst.EnvelopeReleaseVolume;
				}

				if (voiceInfo.Effects2.Effect.HasFlag(InstrumentEffect2.Phase))
				{
					voiceInfo.PhaseType = inst.PhaseType;

					CounterInfo counter = voiceInfo.PhaseCounter;

					bool init = false;
					bool check = false;

					counter.Step = voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.PhaseStep);
					if (counter.Step)
					{
						voiceInfo.PhaseSpeed = inst.PhaseTurns;
						counter.Turns = 0;

						check = true;
					}
					else
					{
						counter.Turns = (short)inst.PhaseTurns;

						if (!voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.PhaseInit))
						{
							voiceInfo.PhaseInit = 0;
							init = true;
						}
						else
							check = true;
					}

					if (check)
					{
						byte oldInit = voiceInfo.PhaseInit;
						voiceInfo.PhaseInit = voiceInfo.PartInstrument;

						if (oldInit == voiceInfo.PartInstrument)
							counter.Delay = inst.PhaseDelay;
						else
							init = true;
					}

					if (init)
					{
						counter.Counter = inst.PhaseStart;
						counter.Speed = (short)inst.PhaseSpeed;
						counter.Repeat = inst.PhaseRepeat;
						counter.RepeatEnd = inst.PhaseRepeatEnd;

						if (inst.PhaseStart > inst.PhaseRepeat)
							counter.Speed = (short)-counter.Speed;

						counter.Delay = inst.PhaseDelay;
					}
				}

				voiceInfo.MixResFilBoost = inst.MixResFilBoost;

				if (voiceInfo.Effects2.Effect.HasFlag(InstrumentEffect2.Resonance))
				{
					voiceInfo.ResonanceAmp = inst.ResponanceAmp;

					CounterInfo counter = voiceInfo.ResonanceCounter;

					bool init = false;
					bool check = false;

					counter.Step = voiceInfo.Effects2.Argument.HasFlag(MixFlag.ResonanceStep);
					if (counter.Step)
					{
						voiceInfo.ResonanceSpeed = inst.ResonanceTurns;
						counter.Turns = 0;

						check = true;
					}
					else
					{
						counter.Turns = (short)inst.ResonanceTurns;

						if (!voiceInfo.Effects2.Argument.HasFlag(MixFlag.ResonanceInit))
						{
							voiceInfo.ResonanceInit = 0;
							init = true;
						}
						else
							check = true;
					}

					if (check)
					{
						byte oldInit = voiceInfo.ResonanceInit;
						voiceInfo.ResonanceInit = voiceInfo.PartInstrument;

						if (oldInit == voiceInfo.PartInstrument)
							counter.Delay = inst.ResonanceDelay;
						else
							init = true;
					}

					if (init)
					{
						voiceInfo.ResonanceLastInit = true;

						counter.Counter = inst.ResonanceStart;
						counter.Speed = (short)inst.ResonanceSpeed;
						counter.Repeat = inst.ResonanceRepeat;
						counter.RepeatEnd = inst.ResonanceRepeatEnd;

						if (inst.ResonanceStart > inst.ResonanceRepeat)
							counter.Speed = (short)-counter.Speed;

						counter.Delay = inst.ResonanceDelay;
					}
				}

				if (voiceInfo.Effects2.Effect.HasFlag(InstrumentEffect2.Filter))
				{
					voiceInfo.FilterType = inst.FilterType;

					CounterInfo counter = voiceInfo.FilterCounter;

					bool init = false;
					bool check = false;

					counter.Step = voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.FilterStep);
					if (counter.Step)
					{
						voiceInfo.FilterSpeed = inst.FilterTurns;
						counter.Turns = 0;

						check = true;
					}
					else
					{
						counter.Turns = (short)inst.FilterTurns;

						if (!voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.FilterInit))
						{
							voiceInfo.FilterInit = 0;
							init = true;
						}
						else
							check = true;
					}

					if (check)
					{
						byte oldInit = voiceInfo.FilterInit;
						voiceInfo.FilterInit = voiceInfo.PartInstrument;

						if (oldInit == voiceInfo.PartInstrument)
							counter.Delay = inst.FilterDelay;
						else
							init = true;
					}

					if (init)
					{
						voiceInfo.FilterLastInit = true;

						counter.Counter = inst.FilterStart;
						counter.Speed = (short)inst.FilterSpeed;
						counter.Repeat = inst.FilterRepeat;
						counter.RepeatEnd = inst.FilterRepeatEnd;

						if (inst.FilterStart > inst.FilterRepeat)
							counter.Speed = (short)-counter.Speed;

						counter.Delay = inst.FilterDelay;
					}
				}

				if (voiceInfo.Effects2.Effect.HasFlag(InstrumentEffect2.Mix))
				{
					voiceInfo.MixWaveNumber = inst.MixWaveNumber;

					CounterInfo counter = voiceInfo.MixCounter;

					bool init = false;
					bool check = false;

					counter.Step = voiceInfo.Effects2.Argument.HasFlag(MixFlag.MixStep);
					if (counter.Step)
					{
						voiceInfo.MixSpeed = inst.MixTurns;
						counter.Turns = 0;

						check = true;
					}
					else
					{
						counter.Turns = (short)inst.MixTurns;

						if (!voiceInfo.Effects2.Argument.HasFlag(MixFlag.MixInit))
						{
							voiceInfo.MixInit = 0;
							init = true;
						}
						else
							check = true;
					}

					if (check)
					{
						byte oldInit = voiceInfo.MixInit;
						voiceInfo.MixInit = voiceInfo.PartInstrument;

						if (oldInit == voiceInfo.PartInstrument)
							counter.Delay = inst.MixDelay;
						else
							init = true;
					}

					if (init)
					{
						counter.Counter = inst.MixStart;
						counter.Speed = (short)inst.MixSpeed;
						counter.Repeat = inst.MixRepeat;
						counter.RepeatEnd = inst.MixRepeatEnd;

						if (inst.MixStart > inst.MixRepeat)
							counter.Speed = (short)-counter.Speed;

						counter.Delay = inst.MixDelay;
					}
				}

				if (voiceInfo.Effects2.Effect.HasFlag(InstrumentEffect2.Transform))
				{
					voiceInfo.TransformWaveSampleNumbers[0] = inst.SampleNumber;
					voiceInfo.TransformWaveSampleNumbers[1] = inst.TransformWaveNumbers[0];
					voiceInfo.TransformWaveSampleNumbers[2] = inst.TransformWaveNumbers[1];
					voiceInfo.TransformWaveSampleNumbers[3] = inst.TransformWaveNumbers[2];
					voiceInfo.TransformWaveSampleNumbers[4] = inst.TransformWaveNumbers[3];
					voiceInfo.TransformWaveSampleNumbers[5] = inst.TransformWaveNumbers[4];

					CounterInfo counter = voiceInfo.TransformCounter;

					bool init = false;
					bool check = false;

					counter.Step = voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.TransformStep);
					if (counter.Step)
					{
						voiceInfo.TransformSpeed = inst.TransformTurns;
						counter.Turns = 0;

						check = true;
					}
					else
					{
						counter.Turns = (short)inst.TransformTurns;

						if (!voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.TransformInit))
						{
							voiceInfo.TransformInit = 0;
							init = true;
						}
						else
							check = true;
					}

					if (check)
					{
						byte oldInit = voiceInfo.TransformInit;
						voiceInfo.TransformInit = voiceInfo.PartInstrument;

						if (oldInit == voiceInfo.PartInstrument)
							counter.Delay = inst.TransformDelay;
						else
							init = true;
					}

					if (init)
					{
						counter.Counter = inst.TransformStart;
						counter.Speed = (short)inst.TransformSpeed;
						counter.Repeat = inst.TransformRepeat;
						counter.RepeatEnd = inst.TransformRepeatEnd;

						if (inst.TransformStart > inst.TransformRepeat)
							counter.Speed = (short)-counter.Speed;

						counter.Delay = inst.TransformDelay;
					}
				}

				if (voiceInfo.Effects1.Effect.HasFlag(InstrumentEffect1.Loop))
				{
					if ((inst.SampleType != SampleType.Sample) || (inst.LoopLength == 0))
						voiceInfo.Effects1.Effect &= ~InstrumentEffect1.Loop;
					else
					{
						bool init = false;
						bool partInit = false;
						bool check = false;

						if (voiceInfo.Effects2.Argument.HasFlag(MixFlag.LoopStep))
						{
							voiceInfo.LoopSpeed = (short)inst.LoopTurns;
							voiceInfo.LoopTurns = 0;

							check = true;
						}
						else
						{
							voiceInfo.LoopTurns = (short)inst.LoopTurns;

							if (!voiceInfo.Effects2.Argument.HasFlag(MixFlag.LoopInit))
							{
								voiceInfo.LoopInit = 0;
								init = true;
							}
							else
								check = true;
						}

						if (check)
						{
							byte oldInit = voiceInfo.LoopInit;
							voiceInfo.LoopInit = voiceInfo.PartInstrument;

							if (oldInit == voiceInfo.PartInstrument)
							{
								voiceInfo.Play &= ~PlayFlag.Retrigger;
								partInit = true;
							}
							else
								init = true;
						}

						if (init)
						{
							Sample sample = samples[inst.SampleNumber];

							voiceInfo.LoopWaveSample = sample.SamplePointer;
							voiceInfo.LoopCounter = inst.LoopStart;
							voiceInfo.LoopCounterSave = inst.LoopStart;
							voiceInfo.LoopWaveSampleCounterMax = (ushort)(sample.SampleLength - inst.LoopLength);
							voiceInfo.LoopLength = inst.LoopLength;
							voiceInfo.LoopRepeatEnd = inst.LoopRepeatEnd;
							voiceInfo.LoopWait = inst.LoopWait;
							voiceInfo.LoopStep = inst.LoopLoopStep;
							voiceInfo.LoopRepeat = inst.LoopRepeat;

							if (inst.LoopStart > inst.LoopRepeat)
								voiceInfo.LoopStep = -voiceInfo.LoopStep;
						}

						if (init || partInit)
						{
							voiceInfo.LoopDelay = (short)inst.LoopDelay;
							voiceInfo.LoopWaitCounter = 0;
							voiceInfo.WaveSamplePointer = new SamplePointer(voiceInfo.LoopWaveSample.Value.SampleData, voiceInfo.LoopWaveSample.Value.StartOffset + (voiceInfo.LoopCounterSave * 2U));
							voiceInfo.WaveSampleRepeatPointer = voiceInfo.WaveSamplePointer;
							voiceInfo.WaveSampleRepeatPointerOriginal = voiceInfo.WaveSamplePointer;
							voiceInfo.WaveSampleLength = voiceInfo.LoopLength;
							voiceInfo.WaveSampleRepeatLength = voiceInfo.LoopLength;
						}
					}
				}
			}

			ushort repeatLength = voiceInfo.WaveSampleRepeatLength;

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
								voiceInfo.Effects2.Effect = InstrumentEffect2.None;
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
		private void PeriodCalculation()
		{
			for (int i = 0; i < numberOfChannels; i++)
				Period(voices[i]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Period(VoiceInfo voiceInfo)
		{
			short note = (short)(voiceInfo.Note + voiceInfo.VibratoNote + voiceInfo.PitchSlideNote + voiceInfo.ArpeggioPitchSlideNote + voiceInfo.SemiTone + voiceInfo.FineTune + voiceInfo.PitchAdd);

			if (!voiceInfo.Arpeggio.HasFlag(ArpeggioFlag.NoteIsAbsolute) && (voiceInfo.Transpose != 0))
				note += voiceInfo.Transpose;

			if (note < -32)
				note = -32;
			else if (note > 5 * 12 * 32)
				note = 5 * 12 * 32;

			ushort period = Tables.PalPitchTable[note + 32];
			period += (ushort)(voiceInfo.PtPitchSlideNote + voiceInfo.PtVibratoNote + voiceInfo.PtPitchAdd);

			if (period < 106)
				period = 106;
			else if (period > 3591)
				period = 3591;

			voiceInfo.Period1 = period;
			voiceInfo.Period2 = period;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PeriodVolumePlay()
		{
			for (int i = 0; i < numberOfChannels; i++)
			{
				VoiceInfo voiceInfo = voices[i];
				IChannel channel = VirtualChannels[i];

				if (!voiceInfo.Play.HasFlag(PlayFlag.Retrigger))
				{
					channel.SetAmigaPeriod(voiceInfo.Period2);
					channel.SetAmigaVolume((ushort)((((voiceInfo.Volume3 * voiceInfo.ChannelVolume) / 1024) * playingInfo.MasterVolume) / 16384));
				}
			}
		}
	}
}
