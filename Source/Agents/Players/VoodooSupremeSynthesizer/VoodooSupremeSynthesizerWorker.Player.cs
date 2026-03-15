/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Player.VoodooSupremeSynthesizer.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Sound;

namespace Polycode.NostalgicPlayer.Agent.Player.VoodooSupremeSynthesizer
{
	/// <summary>
	/// The player itself
	/// </summary>
	internal partial class VoodooSupremeSynthesizerWorker
	{
		private enum TrackCommandResult
		{
			NextCommand,
			Exit,
			SetWait
		}

		private ushort allVoicesTaken;

		/********************************************************************/
		/// <summary>
		/// Is called when the audio buffer has been played
		/// </summary>
		/********************************************************************/
		private InterruptResult AudioInterrupt(int voiceNumber)
		{
			VoiceInfo voiceInfo = voices[voiceNumber];
			PaulaChannel channel = channels[voiceNumber];

			if (voiceInfo.SynthesisMode.HasFlag(SynthesisFlag.FrequencyMapped))
			{
				uint prevSampleOffset = voiceInfo.Sample1Offset;
				voiceInfo.Sample1Offset += (uint)((voiceInfo.WaveformIncrement << 8) | voiceInfo.WaveformStartPosition);

				if (voiceInfo.SynthesisMode.HasFlag(SynthesisFlag.StopSample))
				{
					channel.SetAddress(-1, Tables.EmptySample.Data, 0, false);

					voiceInfo.Sample1 = Tables.EmptySample;
					voiceInfo.Sample1Offset = 0;
				}
				else
				{
					Sample sample = (Sample)voiceInfo.Sample2;

					if (voiceInfo.Sample1Offset >= sample.Length)
					{
						channel.SetAddress(-1, Tables.EmptySample.Data, 0, false);

						voiceInfo.Sample1 = Tables.EmptySample;
						voiceInfo.Sample1Offset = 0;
						voiceInfo.SynthesisMode |= SynthesisFlag.StopSample;
					}
					else
					{
						channel.SetAddress(voiceInfo.Sample1Number, voiceInfo.Sample1.Data, prevSampleOffset, voiceInfo.NewNote);
						voiceInfo.NewNote = false;
					}
				}
			}

			return channel.Interrupt();
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		private void PlayModule()
		{
			allVoicesTaken = 0;

			for (int i = 0; i < 4; i++)
				ProcessVoice(voices[i], channels[i]);

			if (allVoicesTaken == 15)
				endReached = true;
		}



		/********************************************************************/
		/// <summary>
		/// Parse and play a single voice
		/// </summary>
		/********************************************************************/
		private void ProcessVoice(VoiceInfo voiceInfo, PaulaChannel channel)
		{
			voiceInfo.TickCounter--;

			if (voiceInfo.TickCounter == 0)
				ParseTrackData(voiceInfo, channel);

			DoVolumeEnvelope(voiceInfo);
			DoPeriodTablePart1(voiceInfo);
			DoPortamento(voiceInfo);
			DoPeriodTablePart2(voiceInfo);
			SetHardware(voiceInfo, channel);
			WaveformGenerator(voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// Get period based on the given note
		/// </summary>
		/********************************************************************/
		private ushort GetPeriod(VoiceInfo voiceInfo, byte note)
		{
			return Tables.Periods[note + voiceInfo.Transpose];
		}



		/********************************************************************/
		/// <summary>
		/// Get structure on the given offset
		/// </summary>
		/********************************************************************/
		private IModuleData GetStructure(byte offset)
		{
			return currentSong.Data[offset];
		}



		/********************************************************************/
		/// <summary>
		/// Handle the volume envelope
		/// </summary>
		/********************************************************************/
		private void DoVolumeEnvelope(VoiceInfo voiceInfo)
		{
			voiceInfo.VolumeEnvelopeTickCounter--;

			if (voiceInfo.VolumeEnvelopeTickCounter == 0)
			{
				byte[] envelope = voiceInfo.VolumeEnvelope.Data;

				while (envelope[voiceInfo.VolumeEnvelopePosition] == 0x88)
				{
					voiceInfo.VolumeEnvelopePosition = envelope[voiceInfo.VolumeEnvelopePosition + 1];
				}

				if (envelope[voiceInfo.VolumeEnvelopePosition + 1] == 0)
				{
					voiceInfo.CurrentVolume = envelope[voiceInfo.VolumeEnvelopePosition];
					voiceInfo.VolumeEnvelopeTickCounter = 1;
				}
				else
				{
					voiceInfo.VolumeEnvelopeDelta = envelope[voiceInfo.VolumeEnvelopePosition];
					voiceInfo.VolumeEnvelopeTickCounter = envelope[voiceInfo.VolumeEnvelopePosition + 1];

					voiceInfo.CurrentVolume += voiceInfo.VolumeEnvelopeDelta;
				}

				voiceInfo.VolumeEnvelopePosition += 2;
			}
			else
				voiceInfo.CurrentVolume += voiceInfo.VolumeEnvelopeDelta;
		}



		/********************************************************************/
		/// <summary>
		/// Handle the period table - first part
		/// </summary>
		/********************************************************************/
		private void DoPeriodTablePart1(VoiceInfo voiceInfo)
		{
			voiceInfo.PeriodTableTickCounter--;

			if (voiceInfo.PeriodTableTickCounter == 0)
			{
				if (voiceInfo.PeriodTable != null)
				{
					byte[] table = voiceInfo.PeriodTable.Data;

					while (table[voiceInfo.PeriodTablePosition] == 0xff)
					{
						voiceInfo.PeriodTablePosition = table[voiceInfo.PeriodTablePosition + 1];
					}

					voiceInfo.PeriodTableCommand = table[voiceInfo.PeriodTablePosition++];
					voiceInfo.PeriodTableTickCounter = table[voiceInfo.PeriodTablePosition++];
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle the period table - second part
		/// </summary>
		/********************************************************************/
		private void DoPeriodTablePart2(VoiceInfo voiceInfo)
		{
			if (voiceInfo.PeriodTableCommand == 0xfe)
			{
				voiceInfo.NotePeriod <<= 1;
			}
			else if (voiceInfo.PeriodTableCommand == 0x7f)
			{
				voiceInfo.NotePeriod >>= 1;
			}
			else if (voiceInfo.PeriodTableCommand == 0x7e)
			{
				byte counter = voiceInfo.PeriodTableTickCounter;
				voiceInfo.PeriodTableTickCounter = 1;

				ushort numerator, denominator;
				bool round;

				if ((counter & 0x80) != 0)
				{
					counter &= 0x7f;

					if (counter >= 13)
					{
						int octave = (counter / 12) & 7;
						voiceInfo.NotePeriod <<= octave;

						counter = (byte)(counter % 12);
					}

					counter *= 2;
					numerator = Tables.FrequencyRatio[counter];
					denominator = Tables.FrequencyRatio[counter + 1];
					round = true;
				}
				else
				{
					if (counter >= 13)
					{
						int octave = (counter / 12) & 7;
						voiceInfo.NotePeriod >>= octave;

						counter = (byte)(counter % 12);
					}

					counter *= 2;
					denominator = Tables.FrequencyRatio[counter];
					numerator = Tables.FrequencyRatio[counter + 1];
					round = false;
				}

				int temp = voiceInfo.NotePeriod * numerator;
				voiceInfo.NotePeriod = (ushort)(temp / denominator);

				if (round && ((temp % denominator) != 0))
					voiceInfo.NotePeriod++;
			}
			else if (voiceInfo.PeriodTableCommand < 0x7f)
				voiceInfo.NotePeriod += voiceInfo.PeriodTableCommand;
			else
				voiceInfo.NotePeriod -= (ushort)(voiceInfo.PeriodTableCommand & 0x7f);
		}



		/********************************************************************/
		/// <summary>
		/// Handle the portamento
		/// </summary>
		/********************************************************************/
		private void DoPortamento(VoiceInfo voiceInfo)
		{
			voiceInfo.PortamentoDuration--;

			if (voiceInfo.PortamentoDuration == 0)
			{
				voiceInfo.PortamentoIncrement = 0;
				voiceInfo.PortamentoTickCounter = 0;
			}
			else
			{
				voiceInfo.PortamentoTickCounter--;

				if (voiceInfo.PortamentoTickCounter == 0)
				{
					voiceInfo.PortamentoTickCounter = voiceInfo.PortamentoDelay;

					if (voiceInfo.PortamentoDirection)
						voiceInfo.NotePeriod -= voiceInfo.PortamentoIncrement;
					else
						voiceInfo.NotePeriod += voiceInfo.PortamentoIncrement;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Setup the Paula chip
		/// </summary>
		/********************************************************************/
		private void SetHardware(VoiceInfo voiceInfo, PaulaChannel channel)
		{
			channel.SetDma(true);
			channel.Period = voiceInfo.TargetPeriod;
			channel.Volume = voiceInfo.FinalVolume;

			voiceInfo.TargetPeriod = voiceInfo.NotePeriod;

			voiceInfo.FinalVolume = (byte)((voiceInfo.CurrentVolume * voiceInfo.MasterVolume) / 64);
		}



		/********************************************************************/
		/// <summary>
		/// Generate realtime waveforms
		/// </summary>
		/********************************************************************/
		private void WaveformGenerator(VoiceInfo voiceInfo, PaulaChannel channel)
		{
			if (voiceInfo.SynthesisMode.HasFlag(SynthesisFlag.XorRingModulation))
				DoXorRingModulation(voiceInfo, channel);
			else if (voiceInfo.SynthesisMode.HasFlag(SynthesisFlag.Morphing))
				DoMorphing(voiceInfo, channel);
			else if (voiceInfo.SynthesisMode.HasFlag(SynthesisFlag.FrequencyMapped))
				DoFrequencyMapped(voiceInfo);
			else
				DoRingModulation(voiceInfo, channel);
		}



		/********************************************************************/
		/// <summary>
		/// Generate ring modulation waveforms
		/// </summary>
		/********************************************************************/
		private void DoRingModulation(VoiceInfo voiceInfo, PaulaChannel channel)
		{
			voiceInfo.WaveformTickCounter--;

			if (voiceInfo.WaveformTickCounter == 0)
			{
				if (voiceInfo.WaveformTable == null)
					return;

				byte[] table = voiceInfo.WaveformTable.Data;

				while ((table[voiceInfo.WaveformTablePosition] & 0x80) != 0)
				{
					voiceInfo.WaveformTablePosition = table[voiceInfo.WaveformTablePosition + 1];
				}

				voiceInfo.WaveformIncrement = table[voiceInfo.WaveformTablePosition++];
				voiceInfo.WaveformTickCounter = table[voiceInfo.WaveformTablePosition++];
			}

			voiceInfo.WaveformPosition = (byte)((voiceInfo.WaveformPosition + voiceInfo.WaveformIncrement) & 0x1f);

			sbyte[] playingAudioBuffer = voiceInfo.AudioBuffer[voiceInfo.UseAudioBuffer];
			channel.SetAddress(voiceInfo.Sample1Number, playingAudioBuffer, 0, voiceInfo.NewNote);

			voiceInfo.NewNote = false;
			voiceInfo.UseAudioBuffer ^= 1;

			if ((voiceInfo.Sample1 != null) && (voiceInfo.Sample2 != null))
			{
				sbyte[] fillAudioBuffer = voiceInfo.AudioBuffer[voiceInfo.UseAudioBuffer];
				uint fillOffset = 0;

				sbyte[] sample1 = voiceInfo.Sample1.Data;
				uint sample1Offset = 0;

				sbyte[] sample2 = voiceInfo.Sample2.Data;
				uint sample2Offset = voiceInfo.WaveformPosition;

				byte mask = voiceInfo.WaveformMask;

				for (int i = 0; i < 32; i++)
				{
					short sample1Data = sample1[sample1Offset++];
					short sample2Data = sample2[sample2Offset++];
					byte sample = (byte)((sample1Data + sample2Data) >> 1);

					if ((sample & 0x80) != 0)
						sample = (byte)(-((-(sbyte)sample) | mask));
					else
						sample |= mask;

					fillAudioBuffer[fillOffset++] = (sbyte)sample;

					if (sample2Offset == 32)
						sample2Offset = 0;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Generate xor ring modulation waveforms
		/// </summary>
		/********************************************************************/
		private void DoXorRingModulation(VoiceInfo voiceInfo, PaulaChannel channel)
		{
			voiceInfo.WaveformTickCounter--;

			if (voiceInfo.WaveformTickCounter == 0)
			{
				if (voiceInfo.WaveformTable == null)
					return;

				byte[] table = voiceInfo.WaveformTable.Data;

				while (table[voiceInfo.WaveformTablePosition] == 0xff)
				{
					voiceInfo.WaveformTablePosition = table[voiceInfo.WaveformTablePosition + 1];
				}

				voiceInfo.WaveformIncrement = table[voiceInfo.WaveformTablePosition++];
				voiceInfo.WaveformTickCounter = table[voiceInfo.WaveformTablePosition++];
			}

			sbyte[] playingAudioBuffer = voiceInfo.AudioBuffer[voiceInfo.UseAudioBuffer];
			channel.SetAddress(voiceInfo.Sample1Number, playingAudioBuffer, 0, voiceInfo.NewNote);

			voiceInfo.NewNote = false;
			voiceInfo.UseAudioBuffer ^= 1;

			if ((voiceInfo.Sample1 != null) && (voiceInfo.Sample2 != null))
			{
				sbyte[] fillAudioBuffer = voiceInfo.AudioBuffer[voiceInfo.UseAudioBuffer];
				uint fillOffset = 0;

				if ((voiceInfo.WaveformIncrement & 0x80) != 0)
				{
					sbyte[] sample1 = voiceInfo.Sample1.Data;

					for (int i = 0; i < 32; i++)
						fillAudioBuffer[i] = sample1[i];
				}
				else
				{
					sbyte[] sample2 = voiceInfo.Sample2.Data;

					byte position = voiceInfo.WaveformPosition;

					byte switchPosition = (byte)((voiceInfo.WaveformIncrement & 0x1f) + position);
					bool flag = false;

					if ((switchPosition & 0x20) != 0)
					{
						flag = true;
						switchPosition &= 0x1f;
					}

					uint playOffset = 0;

					for (int i = 0; i < 32; i++)
					{
						sbyte sampleData = playingAudioBuffer[playOffset++];

						if (i == position)
							flag = !flag;

						if (i == switchPosition)
							flag = !flag;

						if (flag)
							sampleData ^= (sbyte)(sample2[i] & voiceInfo.WaveformMask);

						fillAudioBuffer[fillOffset++] = sampleData;
					}

					voiceInfo.WaveformPosition = (byte)((voiceInfo.WaveformIncrement + voiceInfo.WaveformPosition) & 0x1f);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Generate morphed waveforms
		/// </summary>
		/********************************************************************/
		private void DoMorphing(VoiceInfo voiceInfo, PaulaChannel channel)
		{
			voiceInfo.WaveformTickCounter--;

			if (voiceInfo.WaveformTickCounter == 0)
			{
				if (voiceInfo.WaveformTable == null)
					return;

				byte[] table = voiceInfo.WaveformTable.Data;

				while (table[voiceInfo.WaveformTablePosition] == 0xff)
				{
					voiceInfo.WaveformTablePosition = table[voiceInfo.WaveformTablePosition + 1];
				}

				voiceInfo.WaveformIncrement = table[voiceInfo.WaveformTablePosition++];
				voiceInfo.MorphSpeed = table[voiceInfo.WaveformTablePosition++];
				voiceInfo.WaveformTickCounter = table[voiceInfo.WaveformTablePosition++];
			}

			sbyte[] playingAudioBuffer = voiceInfo.AudioBuffer[voiceInfo.UseAudioBuffer];
			channel.SetAddress(voiceInfo.Sample1Number, playingAudioBuffer, 0, voiceInfo.NewNote);

			voiceInfo.NewNote = false;
			voiceInfo.UseAudioBuffer ^= 1;

			if ((voiceInfo.Sample1 != null) && (voiceInfo.Sample2 != null))
			{
				sbyte[] fillAudioBuffer = voiceInfo.AudioBuffer[voiceInfo.UseAudioBuffer];
				uint fillOffset = 0;

				if (voiceInfo.WaveformIncrement == 0x80)
				{
					sbyte[] sample1 = voiceInfo.Sample1.Data;

					for (int i = 0; i < 32; i++)
						fillAudioBuffer[i] = sample1[i];
				}
				else
				{
					byte speed = voiceInfo.MorphSpeed;

					sbyte[] sample = (voiceInfo.WaveformIncrement & 0xc0) == 0x40 ? voiceInfo.Sample1.Data : voiceInfo.Sample2.Data;

					byte position = voiceInfo.WaveformPosition;

					byte switchPosition = (byte)((voiceInfo.WaveformIncrement & 0x1f) + position);
					bool flag = false;

					if ((switchPosition & 0x20) != 0)
					{
						flag = true;
						switchPosition &= 0x1f;
					}

					uint playOffset = 0;

					for (int i = 0; i < 32; i++)
					{
						sbyte sampleData = playingAudioBuffer[playOffset++];

						if (i == position)
							flag = !flag;

						if (i == switchPosition)
							flag = !flag;

						if (flag)
						{
							byte sampleData1 = (byte)(sampleData - 0x80);
							byte sampleData2 = (byte)(sample[i] - 0x80);
							byte diff = (byte)(sampleData2 - sampleData1);

							if (sampleData2 < sampleData1)
							{
								diff = (byte)-diff;

								if (speed >= diff)
									sampleData = sample[i];
								else
									sampleData = (sbyte)(sampleData1 + 0x80 - speed);
							}
							else
							{
								if (speed >= diff)
									sampleData = sample[i];
								else
									sampleData = (sbyte)(sampleData1 + 0x80 + speed);
							}
						}

						fillAudioBuffer[fillOffset++] = sampleData;
					}

					if ((voiceInfo.WaveformIncrement & 0xc0) != 0xc0)
						voiceInfo.WaveformPosition = (byte)((voiceInfo.WaveformIncrement + voiceInfo.WaveformPosition) & 0x1f);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Generate frequency mapped waveforms
		/// </summary>
		/********************************************************************/
		private void DoFrequencyMapped(VoiceInfo voiceInfo)
		{
			voiceInfo.WaveformTickCounter--;

			if (voiceInfo.WaveformTickCounter == 0)
			{
				byte[] table = voiceInfo.WaveformTable.Data;

				while (table[voiceInfo.WaveformTablePosition] == 0xff)
				{
					voiceInfo.WaveformTablePosition = table[voiceInfo.WaveformTablePosition + 1];
				}

				voiceInfo.Sample1 = voiceInfo.Sample2;
				voiceInfo.Sample1Offset = (uint)((table[voiceInfo.WaveformTablePosition] << 8) + table[voiceInfo.WaveformTablePosition + 1]);
				voiceInfo.Sample1Number = voiceInfo.Sample2Number;

				voiceInfo.SynthesisMode &= ~SynthesisFlag.StopSample;
				voiceInfo.WaveformTickCounter = table[voiceInfo.WaveformTablePosition + 2];
				voiceInfo.WaveformTablePosition += 3;
			}

			if (voiceInfo.SynthesisMode.HasFlag(SynthesisFlag.FrequencyBasedLength))
			{
				ushort period = Tables.Periods[voiceInfo.WaveformMask];
				int delta = voiceInfo.NotePeriod / period;
				int deltaRemainder = voiceInfo.NotePeriod % period;

				ushort result = (ushort)(((deltaRemainder * 128) / period) + (delta * 128));
				voiceInfo.WaveformIncrement = (byte)(result >> 8);
				voiceInfo.WaveformStartPosition = (byte)(result & 0xff);
			}
			else
			{
				voiceInfo.WaveformIncrement = 0;
				voiceInfo.WaveformStartPosition = 128;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse the next track data
		/// </summary>
		/********************************************************************/
		private void ParseTrackData(VoiceInfo voiceInfo, PaulaChannel channel)
		{
			for (;;)
			{
				TimeSpan? currentTime = GetCurrentTime();
				if (currentTime.HasValue && !trackTimes[voiceInfo.ChannelNumber].ContainsKey(voiceInfo.TrackPosition))
					trackTimes[voiceInfo.ChannelNumber][voiceInfo.TrackPosition] = currentTime.Value;

				byte[] track = voiceInfo.Track.Track;
				byte cmd = track[voiceInfo.TrackPosition++];

				if (cmd >= 0x80)
				{
					TrackCommandResult result = ParseTrackCommand(voiceInfo, channel, track, cmd);

					if (result == TrackCommandResult.NextCommand)
						continue;

					if (result == TrackCommandResult.Exit)
						break;
				}
				else
				{
					voiceInfo.NotePeriod = GetPeriod(voiceInfo, cmd);
					voiceInfo.NewNote = true;
				}

				voiceInfo.SynthesisMode &= ~SynthesisFlag.StopSample;
				voiceInfo.TickCounter = track[voiceInfo.TrackPosition++];

				if (!voiceInfo.ResetFlags.HasFlag(ResetFlag.WaveformTable))
				{
					voiceInfo.WaveformTablePosition = 0;
					voiceInfo.WaveformTickCounter = 1;
					voiceInfo.WaveformPosition = voiceInfo.WaveformStartPosition;
				}

				if (!voiceInfo.ResetFlags.HasFlag(ResetFlag.VolumeEnvelope))
				{
					voiceInfo.VolumeEnvelopePosition = 0;
					voiceInfo.VolumeEnvelopeTickCounter = 1;
				}

				if (!voiceInfo.ResetFlags.HasFlag(ResetFlag.PeriodTable))
				{
					voiceInfo.PeriodTablePosition = 0;
					voiceInfo.PeriodTableTickCounter = 1;
				}

				break;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse the given track command
		/// </summary>
		/********************************************************************/
		private TrackCommandResult ParseTrackCommand(VoiceInfo voiceInfo, PaulaChannel channel, byte[] track, byte cmd)
		{
			switch ((Effect)cmd)
			{
				case Effect.NoteCut:
				{
					CmdNoteCut(voiceInfo, track);

					return TrackCommandResult.Exit;
				}

				case Effect.Gosub:
				{
					CmdGosub(voiceInfo, track);
					break;
				}

				case Effect.Return:
				{
					CmdReturn(voiceInfo);
					break;
				}

				case Effect.StartLoop:
				{
					CmdStartLoop(voiceInfo, track);
					break;
				}

				case Effect.DoLoop:
				{
					CmdDoLoop(voiceInfo);
					break;
				}

				case Effect.SetSample:
				{
					CmdSetSample(voiceInfo, track);
					break;
				}

				case Effect.SetVolumeEnvelope:
				{
					CmdSetVolumeEnvelope(voiceInfo, track);
					break;
				}

				case Effect.SetPeriodTable:
				{
					CmdSetPeriodTable(voiceInfo, track);
					break;
				}

				case Effect.SetWaveformTable:
				{
					CmdSetWaveformTable(voiceInfo, channel, track);
					break;
				}

				case Effect.Portamento:
				{
					CmdPortamento(voiceInfo, track);
					return TrackCommandResult.SetWait;
				}

				case Effect.SetTranspose:
				{
					CmdSetTranspose(voiceInfo, track);
					break;
				}

				case Effect.Goto:
				{
					CmdGoto(voiceInfo, track);
					break;
				}

				case Effect.SetResetFlags:
				{
					CmdSetResetFlags(voiceInfo, track);
					break;
				}

				case Effect.SetWaveformMask:
				{
					CmdSetWaveformMask(voiceInfo, track);
					break;
				}

				default:
					return TrackCommandResult.Exit;
			}

			return TrackCommandResult.NextCommand;
		}



		/********************************************************************/
		/// <summary>
		/// Parse the NoteCut track command
		/// </summary>
		/********************************************************************/
		private void CmdNoteCut(VoiceInfo voiceInfo, byte[] track)
		{
			voiceInfo.CurrentVolume = 0;
			voiceInfo.VolumeEnvelopeTickCounter = 0;

			voiceInfo.TickCounter = track[voiceInfo.TrackPosition++];
			voiceInfo.FinalVolume = 0;
			voiceInfo.VolumeEnvelopeDelta = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Parse the Gosub track command
		/// </summary>
		/********************************************************************/
		private void CmdGosub(VoiceInfo voiceInfo, byte[] track)
		{
			byte trackNum = track[voiceInfo.TrackPosition++];

			voiceInfo.Stack.Push(voiceInfo.TrackPosition);
			voiceInfo.Stack.Push(voiceInfo.Track);

			voiceInfo.Track = (TrackData)GetStructure(trackNum);
			voiceInfo.TrackPosition = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Parse the Return track command
		/// </summary>
		/********************************************************************/
		private void CmdReturn(VoiceInfo voiceInfo)
		{
			voiceInfo.Track = (TrackData)voiceInfo.Stack.Pop();
			voiceInfo.TrackPosition = (int)voiceInfo.Stack.Pop();
		}



		/********************************************************************/
		/// <summary>
		/// Parse the StartLoop track command
		/// </summary>
		/********************************************************************/
		private void CmdStartLoop(VoiceInfo voiceInfo, byte[] track)
		{
			byte loopCount = track[voiceInfo.TrackPosition];
			voiceInfo.TrackPosition += 2;

			voiceInfo.Stack.Push(voiceInfo.TrackPosition);
			voiceInfo.Stack.Push(loopCount);
		}



		/********************************************************************/
		/// <summary>
		/// Parse the DoLoop track command
		/// </summary>
		/********************************************************************/
		private void CmdDoLoop(VoiceInfo voiceInfo)
		{
			byte loopCount = (byte)voiceInfo.Stack.Pop();
			int loopPosition = (int)voiceInfo.Stack.Pop();

			loopCount--;
			if (loopCount != 0)
			{
				voiceInfo.Stack.Push(loopPosition);
				voiceInfo.Stack.Push(loopCount);

				voiceInfo.TrackPosition = loopPosition;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse the SetSample track command
		/// </summary>
		/********************************************************************/
		private void CmdSetSample(VoiceInfo voiceInfo, byte[] track)
		{
			byte number = track[voiceInfo.TrackPosition++];
			voiceInfo.Sample1 = (Waveform)GetStructure(number);
			voiceInfo.Sample1Offset = 0;
			voiceInfo.Sample1Number = (short)Array.FindIndex(allSamples, x => x.Offset == voiceInfo.Sample1.Offset);

			number = track[voiceInfo.TrackPosition++];
			voiceInfo.Sample2 = (Waveform)GetStructure(number);
			voiceInfo.Sample2Number = (short)Array.FindIndex(allSamples, x => x.Offset == voiceInfo.Sample2.Offset);
		}



		/********************************************************************/
		/// <summary>
		/// Parse the SetVolumeEnvelope track command
		/// </summary>
		/********************************************************************/
		private void CmdSetVolumeEnvelope(VoiceInfo voiceInfo, byte[] track)
		{
			byte number = track[voiceInfo.TrackPosition++];
			voiceInfo.VolumeEnvelope = (Table)GetStructure(number);

			voiceInfo.VolumeEnvelopePosition = 0;
			voiceInfo.VolumeEnvelopeTickCounter = 1;
		}



		/********************************************************************/
		/// <summary>
		/// Parse the SetPeriodTable track command
		/// </summary>
		/********************************************************************/
		private void CmdSetPeriodTable(VoiceInfo voiceInfo, byte[] track)
		{
			byte number = track[voiceInfo.TrackPosition++];
			voiceInfo.PeriodTable = (Table)GetStructure(number);

			voiceInfo.PeriodTablePosition = 0;
			voiceInfo.PeriodTableTickCounter = 1;
		}



		/********************************************************************/
		/// <summary>
		/// Parse the SetWaveformTable track command
		/// </summary>
		/********************************************************************/
		private void CmdSetWaveformTable(VoiceInfo voiceInfo, PaulaChannel channel, byte[] track)
		{
			byte number = track[voiceInfo.TrackPosition++];
			voiceInfo.WaveformTable = (Table)GetStructure(number);

			voiceInfo.WaveformTablePosition = 0;
			voiceInfo.WaveformTickCounter = 1;

			SynthesisFlag mode = (SynthesisFlag)track[voiceInfo.TrackPosition++];
			SynthesisFlag oldMode = voiceInfo.SynthesisMode;

			if (mode.HasFlag(SynthesisFlag.FrequencyMapped))
			{
				voiceInfo.SynthesisMode = mode;
				channel.Length = 0x40;
			}
			else
			{
				voiceInfo.WaveformPosition = (byte)((byte)mode & 0x1f);
				voiceInfo.SynthesisMode = mode;
				voiceInfo.WaveformStartPosition = voiceInfo.WaveformPosition;

				if (oldMode.HasFlag(SynthesisFlag.FrequencyMapped))
				{
					channel.SetAddress(-1, voiceInfo.AudioBuffer[0], 0, voiceInfo.NewNote);
					channel.Length = 0x10;

					voiceInfo.WaveformIncrement = 0;
					voiceInfo.WaveformStartPosition = 0;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse the Portamento track command
		/// </summary>
		/********************************************************************/
		private void CmdPortamento(VoiceInfo voiceInfo, byte[] track)
		{
			byte startNote = track[voiceInfo.TrackPosition++];
			ushort startPeriod = GetPeriod(voiceInfo, startNote);
			voiceInfo.NotePeriod = startPeriod;
			voiceInfo.NewNote = true;

			byte stopNote = track[voiceInfo.TrackPosition++];
			ushort stopPeriod = GetPeriod(voiceInfo, stopNote);

			int delta = stopPeriod - startPeriod;
			if (delta < 0)
			{
				voiceInfo.PortamentoDirection = true;
				delta = -delta;
			}
			else
				voiceInfo.PortamentoDirection = false;

			byte ticks = track[voiceInfo.TrackPosition];
			int increment = delta / ticks;
			if (increment == 0)
				increment = 1;

			voiceInfo.PortamentoIncrement = (byte)increment;
			voiceInfo.PortamentoDelay = (byte)(ticks / delta);
			voiceInfo.PortamentoTickCounter = 1;

			if (voiceInfo.PortamentoDelay == 0)
				voiceInfo.PortamentoDelay = 1;

			voiceInfo.PortamentoDuration = ticks;
		}



		/********************************************************************/
		/// <summary>
		/// Parse the SetTranspose track command
		/// </summary>
		/********************************************************************/
		private void CmdSetTranspose(VoiceInfo voiceInfo, byte[] track)
		{
			voiceInfo.Transpose = (sbyte)track[voiceInfo.TrackPosition++];
		}



		/********************************************************************/
		/// <summary>
		/// Parse the Goto track command
		/// </summary>
		/********************************************************************/
		private void CmdGoto(VoiceInfo voiceInfo, byte[] track)
		{
			// The original player read an offset number to lookup
			// in the offset list.
			//
			// The loader has been changed to store a 16-bit position
			// into the current track instead
			voiceInfo.TrackPosition = (track[voiceInfo.TrackPosition] << 8) | track[voiceInfo.TrackPosition + 1];

			allVoicesTaken <<= 1;
			allVoicesTaken |= 1;

			voiceInfo.RestartTime = trackTimes[voiceInfo.ChannelNumber][voiceInfo.TrackPosition];
		}



		/********************************************************************/
		/// <summary>
		/// Parse the ResetFlags track command
		/// </summary>
		/********************************************************************/
		private void CmdSetResetFlags(VoiceInfo voiceInfo, byte[] track)
		{
			voiceInfo.ResetFlags = (ResetFlag)track[voiceInfo.TrackPosition++];
		}



		/********************************************************************/
		/// <summary>
		/// Parse the WaveformMask track command
		/// </summary>
		/********************************************************************/
		private void CmdSetWaveformMask(VoiceInfo voiceInfo, byte[] track)
		{
			voiceInfo.WaveformMask = track[voiceInfo.TrackPosition++];
		}
	}
}
