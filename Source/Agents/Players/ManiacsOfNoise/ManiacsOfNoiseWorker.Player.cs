/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.ManiacsOfNoise.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.ManiacsOfNoise
{
	internal partial class ManiacsOfNoiseWorker
	{
		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		private void PlayModule()
		{
			if (!playingInfo.EnablePlaying)
				return;

			for (int i = 3; i >= 0; i--)
			{
				VoiceInfo voiceInfo = voices[i];
				IChannel channel = VirtualChannels[i];

				if (IsVoiceMuted(voiceInfo))
				{
					OnEndReached(voiceInfo.ChannelNumber);
					continue;
				}

				if (voiceInfo.FirstTickFlag)
				{
					voiceInfo.FirstTickFlag = false;

					if (ParsePositionList(voiceInfo, channel))
						return;
				}
				else
				{
					voiceInfo.NoteTickCounter--;
					if (voiceInfo.NoteTickCounter == 0)
					{
						if (ParseTrack(voiceInfo, channel))
							return;
					}
					else
						DoEffects(voiceInfo, channel);
				}
			}

			DoFade();
		}



		/********************************************************************/
		/// <summary>
		/// Tells whether the current voice is muted
		/// </summary>
		/********************************************************************/
		private bool IsVoiceMuted(VoiceInfo voiceInfo)
		{
			return voiceInfo.PositionList == null;
		}



		/********************************************************************/
		/// <summary>
		/// Return a period from the given index safely
		/// </summary>
		/********************************************************************/
		private ushort GetPeriod(byte index)
		{
			if (index >= Tables.Periods.Length)
				return 0;

			return Tables.Periods[index];
		}



		/********************************************************************/
		/// <summary>
		/// Parse the next command in position list
		/// </summary>
		/********************************************************************/
		private bool ParsePositionList(VoiceInfo voiceInfo, IChannel channel)
		{
			int currentPosition = voiceInfo.PositionListIndex;

			for (;;)
			{
				ushort cmd = voiceInfo.PositionList[currentPosition++];

				if (cmd < 0xf000)
				{
					// Set new track
					voiceInfo.Track = trackLists[cmd];
					voiceInfo.TrackNumber = trackLists.IndexOfKey(cmd);
					voiceInfo.TrackIndex = 0;

					voiceInfo.PositionListIndex = currentPosition;

					ShowPositions();
					ShowTracks();

					return ParseTrack(voiceInfo, channel);
				}

				byte data = (byte)(cmd & 0x00ff);

				switch (cmd & 0x0f00)
				{
					// Set transpose
					case 0x0000:
					{
						voiceInfo.Transpose = (sbyte)data;
						break;
					}

					// Repeat track
					case 0x0100:
					{
						voiceInfo.TrackRepeatCount = data;
						break;
					}

					// Fade in
					case 0x0200:
					{
						playingInfo.FadeIncrement = 1;
						playingInfo.FadeSpeed = data;
						playingInfo.FadeSpeedCounter = data;
						playingInfo.MasterVolume = 0;
						playingInfo.FadeDestination = playingInfo.FadeStartVolume;
						break;
					}

					// Fade out
					case 0x0300:
					{
						playingInfo.FadeIncrement = -1;
						playingInfo.FadeSpeed = data;
						playingInfo.FadeSpeedCounter = data;
						playingInfo.FadeDestination = 0;
						break;
					}

					// Set speed
					case 0x0400:
					{
						playingInfo.CurrentSpeed = data;

						ShowSpeed();
						break;
					}

					// Jump to position
					case 0x0500:
					{
						currentPosition = data;

						OnEndReached(voiceInfo.ChannelNumber);
						break;
					}

					// Stop song
					case 0x0600:
					{
						playingInfo.EnablePlaying = false;

						for (int i = 0; i < 4; i++)
							VirtualChannels[i].Mute();

						return true;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse the next command in track
		/// </summary>
		/********************************************************************/
		private bool ParseTrack(VoiceInfo voiceInfo, IChannel channel)
		{
			int currentPosition = voiceInfo.TrackIndex;

			if (playerVersion == 1)
			{
				for (;;)
				{
					byte cmd = voiceInfo.Track[currentPosition++];

					switch (cmd)
					{
						// Set instrument
						case 0xfe:
						{
							SetInstrument(voiceInfo, ref currentPosition);
							break;
						}

						// Set volume
						case 0xfd:
						{
							SetVolume(voiceInfo, ref currentPosition);
							break;
						}

						// Portamento to note
						case 0xfc:
						{
							PortamentoToNoteV1(voiceInfo, ref currentPosition);
							break;
						}

						// Portamento from note
						case 0xfb:
						{
							PortamentoFromNoteV1(voiceInfo, ref currentPosition);
							return false;
						}

						// Track end
						case 0xff:
						{
							voiceInfo.TrackRepeatCount--;
							if (voiceInfo.TrackRepeatCount == 0)
							{
								voiceInfo.TrackRepeatCount = 1;
								return ParsePositionList(voiceInfo, channel);
							}

							currentPosition = 0;
							break;
						}

						default:
						{
							PlayNote(cmd, voiceInfo, channel, ref currentPosition);

							return false;
						}
					}
				}
			}
			else
			{
				for (;;)
				{
					byte cmd = voiceInfo.Track[currentPosition++];

					if (cmd <= 0x7f)
					{
						PlayNote(cmd, voiceInfo, channel, ref currentPosition);
						return false;
					}

					switch (cmd & 0x0f)
					{
						// Track end
						case 0x00:
						{
							voiceInfo.TrackRepeatCount--;
							if (voiceInfo.TrackRepeatCount == 0)
							{
								voiceInfo.TrackRepeatCount = 1;
								return ParsePositionList(voiceInfo, channel);
							}

							currentPosition = 0;
							break;
						}

						// Set instrument
						case 0x01:
						{
							SetInstrument(voiceInfo, ref currentPosition);
							break;
						}

						// Set volume
						case 0x02:
						{
							SetVolume(voiceInfo, ref currentPosition);
							break;
						}

						// Portamento to note
						case 0x03:
						{
							PortamentoToNote(voiceInfo, ref currentPosition);
							break;
						}

						// Portamento from note
						case 0x04:
						{
							PortamentoFromNote(voiceInfo, ref currentPosition);
							return false;
						}

						// Pause
						case 0x05:
						{
							Pause(voiceInfo, channel, ref currentPosition);
							return false;
						}

						// From v3+

						// Set arpeggio
						case 0x06:
						{
							SetArpeggioTable(voiceInfo, ref currentPosition);
							break;
						}

						// Wait
						case 0x07:
						{
							Wait(voiceInfo, channel, ref currentPosition);
							return false;
						}

						// Note delay
						case 0x08:
						{
							NoteDelay(voiceInfo, ref currentPosition);
							break;
						}

						// Key off
						case 0x09:
						{
							KeyOff(voiceInfo, ref currentPosition);
							return false;
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set new instrument
		/// </summary>
		/********************************************************************/
		private void SetInstrument(VoiceInfo voiceInfo, ref int currentPosition)
		{
			voiceInfo.SampleNumber = voiceInfo.Track[currentPosition++];
			voiceInfo.CurrentSample = samples[voiceInfo.SampleNumber];
			voiceInfo.EffectFlag = true;
			voiceInfo.Volume = voiceInfo.CurrentSample.Volume;

			if (playerVersion >= 3)
				voiceInfo.ArpeggioOffset = voiceInfo.CurrentSample.ArpeggioOffset;
		}



		/********************************************************************/
		/// <summary>
		/// Set new volume
		/// </summary>
		/********************************************************************/
		private void SetVolume(VoiceInfo voiceInfo, ref int currentPosition)
		{
			voiceInfo.Volume = voiceInfo.Track[currentPosition++];
		}



		/********************************************************************/
		/// <summary>
		/// Portamento to note
		/// </summary>
		/********************************************************************/
		private void PortamentoToNoteV1(VoiceInfo voiceInfo, ref int currentPosition)
		{
			byte note = (byte)(voiceInfo.Track[currentPosition++] + voiceInfo.Transpose);
			ushort period = GetPeriod(note);
			voiceInfo.PortamentoStartPeriod = (ushort)((period * voiceInfo.CurrentSample.PeriodMultiplier) / 1024);

			byte rows = voiceInfo.Track[currentPosition++];
			voiceInfo.PortamentoTickCounter = (ushort)(rows * playingInfo.CurrentSpeed);
		}



		/********************************************************************/
		/// <summary>
		/// Portamento to note
		/// </summary>
		/********************************************************************/
		private void PortamentoToNote(VoiceInfo voiceInfo, ref int currentPosition)
		{
			ushort multiplier = voiceInfo.CurrentSample?.PeriodMultiplier ?? 0;

			byte note = (byte)(voiceInfo.Track[currentPosition++] + voiceInfo.Transpose);
			ushort period = GetPeriod(note);
			voiceInfo.PortamentoStartPeriod = (ushort)((period * multiplier) / 1024);

			byte rows = voiceInfo.Track[currentPosition++];
			voiceInfo.PortamentoDuration = (ushort)(rows * playingInfo.CurrentSpeed);
		}



		/********************************************************************/
		/// <summary>
		/// Portamento from note
		/// </summary>
		/********************************************************************/
		private void PortamentoFromNoteV1(VoiceInfo voiceInfo, ref int currentPosition)
		{
			byte note = (byte)(voiceInfo.Track[currentPosition++] + voiceInfo.Transpose);
			ushort period = GetPeriod(note);
			voiceInfo.PortamentoStartPeriod = voiceInfo.NotePeriod;
			voiceInfo.NotePeriod = (ushort)((period * voiceInfo.CurrentSample.PeriodMultiplier) / 1024);
			voiceInfo.PortamentoCurrentPeriod = voiceInfo.NotePeriod;

			byte rows = voiceInfo.Track[currentPosition++];
			voiceInfo.PortamentoTickCounter = (ushort)(rows * playingInfo.CurrentSpeed);

			rows = voiceInfo.Track[currentPosition++];
			voiceInfo.NoteTickCounter = (ushort)(rows * playingInfo.CurrentSpeed);

			voiceInfo.VoiceState = 3;

			voiceInfo.TrackIndex = currentPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Portamento from note
		/// </summary>
		/********************************************************************/
		private void PortamentoFromNote(VoiceInfo voiceInfo, ref int currentPosition)
		{
			ushort multiplier = voiceInfo.CurrentSample?.PeriodMultiplier ?? 0;

			byte note = (byte)(voiceInfo.Track[currentPosition++] + voiceInfo.Transpose);
			ushort period = GetPeriod(note);
			voiceInfo.PortamentoStartPeriod = voiceInfo.NotePeriod;
			voiceInfo.NotePeriod = (ushort)((period * multiplier) / 1024);
			voiceInfo.PortamentoCurrentPeriod = voiceInfo.NotePeriod;

			byte rows = voiceInfo.Track[currentPosition++];
			voiceInfo.PortamentoDuration = (ushort)(rows * playingInfo.CurrentSpeed);

			rows = voiceInfo.Track[currentPosition++];
			voiceInfo.NoteTickCounter = (ushort)(rows * playingInfo.CurrentSpeed);

			if (playerVersion >= 3)
			{
				voiceInfo.NoteTickCounter -= voiceInfo.NoteDelay;
				voiceInfo.NoteDelay = 0;
			}

			voiceInfo.VoiceState = 3;
			voiceInfo.Playing = true;

			voiceInfo.TrackIndex = currentPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Pause the voice
		/// </summary>
		/********************************************************************/
		private void Pause(VoiceInfo voiceInfo, IChannel channel, ref int currentPosition)
		{
			byte rows = voiceInfo.Track[currentPosition++];
			voiceInfo.NoteTickCounter = (ushort)(rows * playingInfo.CurrentSpeed);

			if (playerVersion >= 3)
			{
				voiceInfo.NoteTickCounter -= voiceInfo.NoteDelay;
				voiceInfo.NoteDelay = 0;
			}

			channel.SetAmigaVolume(0);
			voiceInfo.Playing = false;

			voiceInfo.TrackIndex = currentPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Set new arpeggio table
		/// </summary>
		/********************************************************************/
		private void SetArpeggioTable(VoiceInfo voiceInfo, ref int currentPosition)
		{
			voiceInfo.ArpeggioSpeedCounter = 1;
			voiceInfo.ArpeggioIndex = 0;
			voiceInfo.ArpeggioDifference = 0;
			voiceInfo.ArpeggioOffset = (ushort)((voiceInfo.Track[currentPosition++] << 8) | voiceInfo.Track[currentPosition++]);
		}



		/********************************************************************/
		/// <summary>
		/// Wait given number of ticks
		/// </summary>
		/********************************************************************/
		private void Wait(VoiceInfo voiceInfo, IChannel channel, ref int currentPosition)
		{
			voiceInfo.NoteTickCounter = voiceInfo.Track[currentPosition++];

			voiceInfo.Playing = false;
			channel.Mute();

			voiceInfo.TrackIndex = currentPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Note delay
		/// </summary>
		/********************************************************************/
		private void NoteDelay(VoiceInfo voiceInfo, ref int currentPosition)
		{
			voiceInfo.NoteDelay = voiceInfo.Track[currentPosition++];
		}



		/********************************************************************/
		/// <summary>
		/// Key off
		/// </summary>
		/********************************************************************/
		private void KeyOff(VoiceInfo voiceInfo, ref int currentPosition)
		{
			voiceInfo.NoteTickCounter = (ushort)((voiceInfo.Track[currentPosition++] * playingInfo.CurrentSpeed) - voiceInfo.NoteDelay);
			voiceInfo.NoteDelay = 0;
			voiceInfo.EnvelopeFlag = false;

			voiceInfo.TrackIndex = currentPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Play the given note
		/// </summary>
		/********************************************************************/
		private void PlayNote(byte note, VoiceInfo voiceInfo, IChannel channel, ref int currentPosition)
		{
			voiceInfo.CurrentNote = (byte)(note + voiceInfo.Transpose);
			ushort period = GetPeriod(voiceInfo.CurrentNote);
			voiceInfo.NotePeriod = (ushort)((period * voiceInfo.CurrentSample.PeriodMultiplier) / 1024);
			voiceInfo.PortamentoCurrentPeriod = voiceInfo.NotePeriod;

			byte rows = voiceInfo.Track[currentPosition++];
			voiceInfo.NoteTickCounter = (ushort)(rows * playingInfo.CurrentSpeed);

			if (playerVersion >= 3)
			{
				voiceInfo.NoteTickCounter -= voiceInfo.NoteDelay;
				voiceInfo.NoteDelay = 0;
			}

			voiceInfo.EnvelopeSpeedCounter = 1;
			voiceInfo.EnvelopeIndex = 0;

			if (playerVersion >= 3)
				voiceInfo.EnvelopeFlag = true;

			voiceInfo.ArpeggioSpeedCounter = 1;
			voiceInfo.ArpeggioIndex = 0;
			voiceInfo.ArpeggioDifference = 0;

			voiceInfo.VibratoDelay = voiceInfo.CurrentSample.VibratoDelay;
			voiceInfo.VibratoSpeedCounter = 1;
			voiceInfo.VibratoIndex = 0;
			voiceInfo.VibratoValue = 0;

			if (playerVersion <= 2)
				voiceInfo.WaveformSpeedCounter = 1;

			voiceInfo.VoiceState = 2;
			voiceInfo.Playing = true;

			voiceInfo.TrackIndex = currentPosition;

			channel.Mute();
		}



		/********************************************************************/
		/// <summary>
		/// Run the different effects
		/// </summary>
		/********************************************************************/
		private void DoEffects(VoiceInfo voiceInfo, IChannel channel)
		{
			if (!voiceInfo.Playing || (voiceInfo.CurrentSample == null))
				return;

			DoEnvelope(voiceInfo);

			if (playerVersion == 1)
				DoPortamentoV1(voiceInfo);
			else
				DoPortamento(voiceInfo);

			if (playerVersion != 3)
				DoArpeggio(voiceInfo);

			voiceInfo.FinalPeriod = (ushort)(voiceInfo.PortamentoCurrentPeriod + voiceInfo.ArpeggioDifference);

			DoVibrato(voiceInfo);

			voiceInfo.FinalPeriod = (ushort)(voiceInfo.FinalPeriod + voiceInfo.VibratoValue);

			SetupChannel(voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// Run the envelope
		/// </summary>
		/********************************************************************/
		private void DoEnvelope(VoiceInfo voiceInfo)
		{
			voiceInfo.EnvelopeSpeedCounter--;
			if (voiceInfo.EnvelopeSpeedCounter != 0)
				return;

			Sample sample = voiceInfo.CurrentSample;

			voiceInfo.EnvelopeSpeedCounter = sample.EnvelopeSpeed;

			byte[] envelope = envelopes[sample.EnvelopeOffset];
			bool oneMore;

			do
			{
				oneMore = false;

				// This check fixes Chinese Chess Ingame. There is an envelope which
				// jump to the end of another envelope, where volume is set to 0 for infinity
				byte vol = voiceInfo.EnvelopeIndex < envelope.Length ? envelope[voiceInfo.EnvelopeIndex] : (byte)0;

				if (playerVersion <= 2)
				{
					if (vol >= 0x80)
					{
						voiceInfo.EnvelopeIndex = (byte)(vol & 0x7f);
						oneMore = true;
					}
					else
					{
						voiceInfo.EnvelopeIndex++;

						voiceInfo.FinalVolume = (ushort)((((vol * voiceInfo.Volume) / 64) * playingInfo.MasterVolume) / 64);
					}
				}
				else
				{
					oneMore = true;

					if (vol == 0x81)
					{
						if (!voiceInfo.EnvelopeFlag)
						{
							voiceInfo.EnvelopeIndex += 2;
							continue;
						}
					}
					else if (vol != 0x80)
					{
						voiceInfo.EnvelopeIndex++;

						voiceInfo.FinalVolume = (ushort)((((vol * voiceInfo.Volume) / 64) * playingInfo.MasterVolume) / 64);
						return;
					}

					byte dat = envelope[voiceInfo.EnvelopeIndex + 1];
					voiceInfo.EnvelopeIndex -= dat;
				}
			}
			while (oneMore);
		}



		/********************************************************************/
		/// <summary>
		/// Run the portamento
		/// </summary>
		/********************************************************************/
		private void DoPortamentoV1(VoiceInfo voiceInfo)
		{
			if (voiceInfo.PortamentoTickCounter == 0)
				return;

			if (voiceInfo.VoiceState == 3)
				voiceInfo.VoiceState = 0;
			else if (voiceInfo.VoiceState != 2)
			{
				voiceInfo.PortamentoCurrentPeriod = (ushort)(voiceInfo.PortamentoCurrentPeriod - voiceInfo.PortamentoStep);

				voiceInfo.PortamentoTickCounter--;
				if (voiceInfo.PortamentoTickCounter == 0)
					voiceInfo.PortamentoCurrentPeriod = (ushort)(voiceInfo.PortamentoCurrentPeriod - voiceInfo.PortamentoStepLeftOver);

				return;
			}

			ushort period = voiceInfo.PortamentoStartPeriod;
			voiceInfo.PortamentoCurrentPeriod = period;

			int difference = period - voiceInfo.NotePeriod;
			voiceInfo.PortamentoStep = (short)(difference / voiceInfo.PortamentoTickCounter);
			voiceInfo.PortamentoStepLeftOver = (short)(difference % voiceInfo.PortamentoTickCounter);
		}



		/********************************************************************/
		/// <summary>
		/// Run the portamento
		/// </summary>
		/********************************************************************/
		private void DoPortamento(VoiceInfo voiceInfo)
		{
			if (voiceInfo.PortamentoDuration == 0)
				return;

			if (voiceInfo.VoiceState == 3)
				voiceInfo.VoiceState = 0;
			else if (voiceInfo.VoiceState != 2)
			{
				voiceInfo.PortamentoCurrentPeriod = (ushort)(voiceInfo.PortamentoStartPeriod - (voiceInfo.PortamentoTick * voiceInfo.PortamentoDifference / voiceInfo.PortamentoDuration));

				if (voiceInfo.PortamentoTick == voiceInfo.PortamentoDuration)
					voiceInfo.PortamentoDuration = 0;
				else
					voiceInfo.PortamentoTick++;

				return;
			}

			voiceInfo.PortamentoTick = 1;

			ushort period = voiceInfo.PortamentoStartPeriod;
			voiceInfo.PortamentoCurrentPeriod = period;

			voiceInfo.PortamentoDifference = (short)(period - voiceInfo.NotePeriod);
		}



		/********************************************************************/
		/// <summary>
		/// Run the arpeggio
		/// </summary>
		/********************************************************************/
		private void DoArpeggio(VoiceInfo voiceInfo)
		{
			Sample sample = voiceInfo.CurrentSample;

			ushort offset = playerVersion >= 4 ? voiceInfo.ArpeggioOffset : sample.ArpeggioOffset;

			if (offset == 0)
				return;

			voiceInfo.ArpeggioSpeedCounter--;
			if (voiceInfo.ArpeggioSpeedCounter != 0)
				return;

			voiceInfo.ArpeggioSpeedCounter = sample.ArpeggioSpeed;

			byte[] arpeggio = arpeggios[offset];
			bool oneMore;

			do
			{
				oneMore = false;

				byte arp = arpeggio[voiceInfo.ArpeggioIndex];

				if (playerVersion < 4)
				{
					if (arp >= 0x80)
					{
						voiceInfo.ArpeggioIndex = (byte)(arp & 0x7f);
						oneMore = true;
					}
					else
					{
						voiceInfo.ArpeggioIndex++;

						ushort period = GetPeriod((byte)(arp + voiceInfo.CurrentNote));
						voiceInfo.ArpeggioDifference = (short)(((period * sample.PeriodMultiplier) / 1024) - voiceInfo.NotePeriod);
					}
				}
				else
				{
					if (arp == 0x80)
					{
						voiceInfo.ArpeggioIndex = (byte)(voiceInfo.ArpeggioIndex - arpeggio[voiceInfo.ArpeggioIndex + 1]);

						if ((sbyte)voiceInfo.ArpeggioIndex < 0)
							voiceInfo.ArpeggioIndex = 0;

						oneMore = true;
					}
					else
					{
						voiceInfo.ArpeggioIndex++;

						ushort period = GetPeriod((byte)(arp + voiceInfo.CurrentNote));
						voiceInfo.ArpeggioDifference = (short)(((period * sample.PeriodMultiplier) / 1024) - voiceInfo.NotePeriod);
					}
				}
			}
			while (oneMore);
		}



		/********************************************************************/
		/// <summary>
		/// Run the vibrato
		/// </summary>
		/********************************************************************/
		private void DoVibrato(VoiceInfo voiceInfo)
		{
			Sample sample = voiceInfo.CurrentSample;

			if (sample.VibratoOffset == 0)
				return;

			if (voiceInfo.VibratoDelay != 0)
			{
				voiceInfo.VibratoDelay--;
				return;
			}

			voiceInfo.VibratoSpeedCounter--;
			if (voiceInfo.VibratoSpeedCounter != 0)
				return;

			voiceInfo.VibratoSpeedCounter = sample.VibratoSpeed;

			sbyte[] vibrato = vibratos[sample.VibratoOffset];
			bool oneMore;

			do
			{
				oneMore = false;

				sbyte vib = vibrato[voiceInfo.VibratoIndex];

				if (vib == -128)
				{
					if (playerVersion <= 2)
						voiceInfo.VibratoIndex = (byte)vibrato[voiceInfo.VibratoIndex + 1];
					else
						voiceInfo.VibratoIndex -= (byte)vibrato[voiceInfo.VibratoIndex + 1];

					oneMore = true;
				}
				else
				{
					voiceInfo.VibratoIndex++;

					voiceInfo.VibratoValue = (short)((voiceInfo.FinalPeriod / 128) * -vib);
				}
			}
			while (oneMore);
		}



		/********************************************************************/
		/// <summary>
		/// Tell NostalgicPlayer what to play
		/// </summary>
		/********************************************************************/
		private void SetupChannel(VoiceInfo voiceInfo, IChannel channel)
		{
			Sample sample = voiceInfo.CurrentSample;

			if (sample.Type == SampleType.Sample)
			{
				if (voiceInfo.VoiceState != 0)
				{
					voiceInfo.VoiceState--;

					if (voiceInfo.VoiceState != 0)
					{
						sbyte[] data = sampleData[sample.DataOffset];
						channel.PlaySample(voiceInfo.SampleNumber, data, 0, sample.Length * 2U);

						// In the original player, the loop is set when VoiceState is 1, but we do it here immediately instead
						if (sample.LoopStart != 0xffff)
						{
							// Bug in v1
							if (playerVersion == 1)
								channel.SetLoop(sample.LoopStart, (uint)(sample.Length - sample.LoopStart) * 2U);
							else
								channel.SetLoop(sample.LoopStart * 2U, (uint)(sample.Length - sample.LoopStart) * 2U);
						}
					}
				}
			}
			else
			{
				if (playerVersion <= 2)
					DoWaveform(voiceInfo, channel, sample);
				else if (playerVersion == 5)
					DoSampleSequence(voiceInfo, channel, sample);
			}

			voiceInfo.EffectFlag = false;

			channel.SetAmigaPeriod(voiceInfo.FinalPeriod);
			channel.SetAmigaVolume(voiceInfo.FinalVolume);
		}



		/********************************************************************/
		/// <summary>
		/// Handle the waveform table
		/// </summary>
		/********************************************************************/
		private void DoWaveform(VoiceInfo voiceInfo, IChannel channel, Sample sample)
		{
			if ((voiceInfo.VoiceState != 2) || (!voiceInfo.EffectFlag && (sample.Type == SampleType.WaveformContinue)))
			{
				voiceInfo.WaveformSpeedCounter--;
				if (voiceInfo.WaveformSpeedCounter != 0)
					return;

				voiceInfo.WaveformSpeedCounter = sample.WaveformSpeed;
			}
			else
			{
				voiceInfo.WaveformIndex = 0;
				voiceInfo.WaveformSpeedCounter = 0;
			}

			for (;;)
			{
				ushort[] waveform = waveforms[sample.DataOffset];
				ushort cmd = waveform[voiceInfo.WaveformIndex];

				switch (cmd)
				{
					// Set length
					case 0xfffd:
					{
						voiceInfo.WaveformLength = waveform[voiceInfo.WaveformIndex + 1];
						voiceInfo.WaveformIndex += 2;
						break;
					}

					// Set period
					case 0xfffe:
					{
						voiceInfo.FinalPeriod = waveform[voiceInfo.WaveformIndex + 1];
						voiceInfo.WaveformIndex += 2;
						break;
					}

					// Jump
					case 0xffff:
					{
						voiceInfo.WaveformIndex = (ushort)(voiceInfo.WaveformIndex - (waveform[voiceInfo.WaveformIndex + 1] / 2));
						break;
					}

					default:
					{
						sbyte[] waveformData = sampleData[cmd];
						uint waveformLength = voiceInfo.WaveformLength * 2U;

						voiceInfo.WaveformSpeedCounter = sample.WaveformSpeed;
						voiceInfo.WaveformIndex++;

						if (voiceInfo.VoiceState == 2)
						{
							voiceInfo.VoiceState = 0;

							channel.PlaySample(voiceInfo.SampleNumber, waveformData, 0, waveformLength);
							channel.SetLoop(0, waveformLength);
						}
						else
						{
							channel.SetSample(waveformData, 0, waveformLength);
							channel.SetLoop(0, waveformLength);
						}

						return;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle the sample sequence
		/// </summary>
		/********************************************************************/
		private void DoSampleSequence(VoiceInfo voiceInfo, IChannel channel, Sample sample)
		{
			if (voiceInfo.VoiceState == 2)
			{
				if (voiceInfo.EffectFlag)
				{
					voiceInfo.SampleSequenceIndex = 0;
					voiceInfo.SampleSequenceSpeedCounter = 1;
				}
				else
				{
					if ((sample.WaveformSpeed & 1) != 0)
					{
						voiceInfo.SampleSequenceIndex = 0;
						voiceInfo.SampleSequenceSpeedCounter = 1;
					}

					// The original player also test for bit 1 and 2 and
					// initialize two other pairs, but they are not used
				}
			}

			voiceInfo.SampleSequenceSpeedCounter--;
			if (voiceInfo.SampleSequenceSpeedCounter == 0)
			{
				voiceInfo.SampleSequenceSpeedCounter = voiceInfo.SampleSequenceSpeed;

				byte[] sampleSequence = sampleSequences[sample.DataOffset];
				bool oneMore;

				do
				{
					oneMore = true;

					byte seq = sampleSequence[voiceInfo.SampleSequenceIndex];

					switch (seq)
					{
						case 0xfe:
						{
							voiceInfo.SampleSequenceSpeed = sampleSequence[voiceInfo.SampleSequenceIndex + 1];
							voiceInfo.SampleSequenceSpeedCounter = voiceInfo.SampleSequenceSpeed;

							voiceInfo.SampleSequenceIndex += 2;
							break;
						}

						case 0xff:
						{
							voiceInfo.SampleSequenceIndex -= sampleSequence[voiceInfo.SampleSequenceIndex + 1];
							break;
						}

						default:
						{
							voiceInfo.SampleSequenceIndex++;

							voiceInfo.SampleSequenceLength = sampleSequenceTable[seq * 2];
							voiceInfo.SampleSequence = sampleData[sampleSequenceTable[(seq * 2) + 1]];

							oneMore = false;
							break;
						}
					}
				}
				while (oneMore);
			}

			voiceInfo.SampleSequenceToPlay = voiceInfo.SampleSequence;

			if (voiceInfo.VoiceState == 2)
			{
				voiceInfo.VoiceState = 0;

				channel.PlaySample(voiceInfo.SampleNumber, voiceInfo.SampleSequenceToPlay, 0, voiceInfo.SampleSequenceLength * 2U);
				channel.SetLoop(0, voiceInfo.SampleSequenceLength * 2U);
			}
			else
			{
				channel.SetSample(voiceInfo.SampleSequenceToPlay, 0, voiceInfo.SampleSequenceLength * 2U);
				channel.SetLoop(0, voiceInfo.SampleSequenceLength * 2U);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Run the fade
		/// </summary>
		/********************************************************************/
		private void DoFade()
		{
			if (playingInfo.FadeIncrement == 0)
				return;

			playingInfo.FadeSpeedCounter--;
			if (playingInfo.FadeSpeedCounter != 0)
				return;

			playingInfo.FadeSpeedCounter = playingInfo.FadeSpeed;
			playingInfo.MasterVolume = (ushort)(playingInfo.MasterVolume + playingInfo.FadeIncrement);

			if (playingInfo.MasterVolume == playingInfo.FadeDestination)
			{
				playingInfo.FadeIncrement = 0;

				if (playingInfo.MasterVolume == 0)
				{
					for (int i = 0; i < 4; i++)
						VirtualChannels[i].Mute();

					playingInfo.EnablePlaying = false;
				}
			}
		}
	}
}
