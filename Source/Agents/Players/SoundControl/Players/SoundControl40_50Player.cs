/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Agent.Player.SoundControl.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SoundControl.Players.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundControl.Players
{
	/// <summary>
	/// Implementation of the SoundControl 4.0/5.0 player
	/// </summary>
	internal class SoundControl40_50Player : ISoundControlPlayer
	{
		/// <summary>
		/// Only 4.0 modules are stored in this dictionary
		/// </summary>
		private static readonly Dictionary<uint, ModuleInfo40_50> specificModuleInfo = new Dictionary<uint, ModuleInfo40_50>
		{
			// Hot number deluxe intro
			{ 81906, new ModuleInfo40_50
			{
				SongInfoList =
				[
					new SongInfo40_50(68, 84, 435),
					new SongInfo40_50(84, 104, 210),
					new SongInfo40_50(104, 132, 181),
					new SongInfo40_50(0, 68, 292)
				]
			}},
			// Hot number deluxe ongame 2
			{ 95960, new ModuleInfo40_50
			{
				SongInfoList =
				[
					new SongInfo40_50(24, 25, 0),
					new SongInfo40_50(0, 24, 0)
				]
			}},
			// Hot number deluxe title
			{ 54544, new ModuleInfo40_50
			{
				SongInfoList =
				[
					new SongInfo40_50(0, 22, 0)
				]
			}}
		};

		private readonly SoundControlWorker worker;
		private readonly ModuleType moduleType;

		private ushort[] periods;

		private readonly ModuleData moduleData;
		private readonly SongInfo40_50[] songInfoList;

		private SongInfo40_50 currentSongInfo;

		private GlobalPlayingInfo40_50 playingInfo;
		private VoiceInfo40_50[] voices;

		private bool endReached;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SoundControl40_50Player(SoundControlWorker worker, ModuleData moduleData, uint totalLength, ModuleType moduleType)
		{
			this.worker = worker;
			this.moduleData = moduleData;
			this.moduleType = moduleType;

			if (moduleType == ModuleType.SoundControl40)
			{
				if (specificModuleInfo.TryGetValue(totalLength, out ModuleInfo40_50 moduleInfo))
					songInfoList = moduleInfo.SongInfoList;
			}
			else
				songInfoList = [ new SongInfo40_50(0, (ushort)moduleData.PositionList.Positions[0].Length, moduleData.Speed) ];
		}



		/********************************************************************/
		/// <summary>
		/// Check if the length given match a version 4.0 module
		/// </summary>
		/********************************************************************/
		public static bool IsVersion40Module(uint totalLength)
		{
			return specificModuleInfo.ContainsKey(totalLength);
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public void InitPlayer()
		{
			CalculatePeriodTable();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the current song
		/// </summary>
		/********************************************************************/
		public void InitSound(int songNumber)
		{
			currentSongInfo = songInfoList[songNumber];

			playingInfo = new GlobalPlayingInfo40_50
			{
				SongPosition = currentSongInfo.StartPosition,
				Speed = currentSongInfo.Speed != 0 ? currentSongInfo.Speed : moduleData.Speed,
				MaxSpeed = moduleType == ModuleType.SoundControl40 ? (ushort)187 : (ushort)46,
				SpeedCounter = 0,
				ChannelCounter = 0
			};

			voices = new VoiceInfo40_50[6];

			for (int i = 0; i < 6; i++)
			{
				voices[i] = new VoiceInfo40_50
				{
					WaitCounter = 0,
					Track = null,
					TrackPosition = 0,
					Transpose = 0,
					TransposedNote = 0,
					SampleTransposedNote = 0,
					Period = 0,
					InstrumentNumber = 0,
					SampleCommandWaitCounter = 0,
					SampleCommandList = null,
					SampleCommandPosition = 0,
					PlaySampleCommand = PlaySampleCommand.Mute,
					SampleNumber = 0,
					Sample = null,
					SampleLength = 0,
					SampleData = null,
					Volume = 0,
					EnvelopeCommand = EnvelopeCommand.Done,
					EnvelopeCounter = 0,
					EnvelopeVolume = 0,
					StartEnvelopeRelease = false,
					Hardware_SampleData = null,
					Hardware_StartOffset = 0,
					Hardware_SampleLength = 0
				};
			}

			SetTracks();
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public bool Play()
		{
			endReached = false;

			ProcessCounter();
			HandleSampleCommands();
			HandleEnvelope();

			for (int i = 0; i < 4; i++)
			{
				VoiceInfo40_50 voiceInfo = voices[i];
				IChannel channel = worker.VirtualChannels[i];

				if (voiceInfo.Hardware_SampleData != null)
				{
					uint length = voiceInfo.Hardware_SampleLength;

					if ((voiceInfo.Hardware_StartOffset + length) > voiceInfo.Hardware_SampleData.Length)
						length = (uint)(voiceInfo.Hardware_SampleData.Length - voiceInfo.Hardware_StartOffset);

					if (length != 0)
					{
						channel.SetSample(voiceInfo.Hardware_SampleData, voiceInfo.Hardware_StartOffset, length);
						channel.SetLoop(voiceInfo.Hardware_StartOffset, length);
					}
				}
			}

			return endReached;
		}



		/********************************************************************/
		/// <summary>
		/// Return the calculated period table
		/// </summary>
		/********************************************************************/
		public IEnumerable<ushort> PeriodTable => periods;



		/********************************************************************/
		/// <summary>
		/// Return the number of sub songs
		/// </summary>
		/********************************************************************/
		public int NumberOfSubSongs => songInfoList.Length;



		/********************************************************************/
		/// <summary>
		/// Return the number of positions
		/// </summary>
		/********************************************************************/
		public int NumberOfPositions => currentSongInfo.EndPosition - currentSongInfo.StartPosition;



		/********************************************************************/
		/// <summary>
		/// Return the current song position
		/// </summary>
		/********************************************************************/
		public ushort SongPosition => (ushort)(playingInfo.SongPosition - currentSongInfo.StartPosition);



		/********************************************************************/
		/// <summary>
		/// Get or set a snapshot
		/// </summary>
		/********************************************************************/
		public Snapshot Snapshot
		{
			get => new Snapshot(playingInfo, voices);

			set
			{
				playingInfo = (GlobalPlayingInfo40_50)value.PlayingInfo;
				voices = value.Voices.Cast<VoiceInfo40_50>().ToArray();
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Calculate the period table
		/// </summary>
		/********************************************************************/
		private void CalculatePeriodTable()
		{
			periods = new ushort[8 * 16];

			int index = 0;

			for (int i = 2; i < 10; i++)
			{
				for (int j = 0; j < 12; j++)
					periods[index++] = (ushort)(Tables.BasePeriod[j] >> i);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Process the next tick
		/// </summary>
		/********************************************************************/
		private void ProcessCounter()
		{
			playingInfo.SpeedCounter += playingInfo.Speed;

			while (playingInfo.SpeedCounter > playingInfo.MaxSpeed)
			{
				playingInfo.SpeedCounter -= playingInfo.MaxSpeed;

				if (moduleType == ModuleType.SoundControl50)
				{
					playingInfo.ChannelCounter++;

					if ((playingInfo.ChannelCounter & 3) != 0)
						continue;
				}

				ProcessTrack();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Process the track data
		/// </summary>
		/********************************************************************/
		private void ProcessTrack()
		{
			bool redo;

			do
			{
				redo = false;

				ProcessVoice6();

				for (int i = 0; i < 4; i++)
				{
					VoiceInfo40_50 voiceInfo = voices[i];

					if (voiceInfo.WaitCounter == 0)
					{
						byte[] track = voiceInfo.Track;
						int trackPosition = voiceInfo.TrackPosition;

						voiceInfo.WaitCounter = (ushort)(track[trackPosition + 1] - 1);
						voiceInfo.TrackPosition += 4;

						if (track[trackPosition] == 0xff)
						{
							NewPosition();

							redo = true;
							break;
						}

						byte note = track[trackPosition];
						if (note == 0)
							continue;

						byte instrumentNumber = track[trackPosition + 2];
						if (instrumentNumber == 0xff)
						{
							NewPosition();

							redo = true;
							break;
						}

						byte volume = track[trackPosition + 3];
						if (volume > 64)
							volume = 64;

						SetupVoiceInfo(voiceInfo, note, instrumentNumber, volume);
					}
					else
						voiceInfo.WaitCounter--;
				}
			}
			while (redo);
		}



		/********************************************************************/
		/// <summary>
		/// Processing of voice 6
		/// </summary>
		/********************************************************************/
		private void ProcessVoice6()
		{
			VoiceInfo40_50 voiceInfo = voices[5];

			byte[] track = voiceInfo.Track;
			int trackPosition = voiceInfo.TrackPosition;

			if (voiceInfo.WaitCounter == 0)
			{
				voiceInfo.WaitCounter = (ushort)(track[trackPosition + 1] - 1);
				voiceInfo.TrackPosition += 4;

				if (track[trackPosition] != 0x00)
				{
					if (track[trackPosition] == 0xff)
					{
						voiceInfo.TrackPosition -= 4;
						return;
					}

					byte dat1 = (byte)(track[trackPosition + 2] - 1);

					ushort dat2 = (ushort)(track[trackPosition + 3] & 0x3f);
					if ((dat2 & 0x40) != 0)
						dat2 |= 0xff10;

					if (dat1 == 4)
					{
						voices[0].Transpose = (short)dat2;
						voices[1].Transpose = (short)dat2;
						voices[2].Transpose = (short)dat2;
						voices[3].Transpose = (short)dat2;
					}

					voices[dat1 & 3].Transpose = (sbyte)dat2;
				}
			}
			else
				voiceInfo.WaitCounter--;
		}



		/********************************************************************/
		/// <summary>
		/// Setup voice structure with new note
		/// </summary>
		/********************************************************************/
		private void SetupVoiceInfo(VoiceInfo40_50 voiceInfo, ushort note, ushort instrumentNumber, ushort volume)
		{
			voiceInfo.InstrumentNumber = instrumentNumber;
			voiceInfo.SampleCommandList = moduleData.Instruments[instrumentNumber].SampleCommands;

			voiceInfo.SampleCommandWaitCounter = 0;
			voiceInfo.SampleCommandPosition = 0;
			voiceInfo.PlaySampleCommand = PlaySampleCommand.Mute;

			voiceInfo.Volume = volume;

			voiceInfo.RepeatListStack.Clear();

			voiceInfo.TransposedNote = (ushort)((note & 0x0f) + (((note & 0xf0) >> 2) * 3) + voiceInfo.Transpose);

			voiceInfo.EnvelopeVolume = 0;
			voiceInfo.EnvelopeCounter = 1;
			voiceInfo.EnvelopeCommand = EnvelopeCommand.Attack;
			voiceInfo.StartEnvelopeRelease = false;
		}



		/********************************************************************/
		/// <summary>
		/// Change to next position
		/// </summary>
		/********************************************************************/
		private void NewPosition()
		{
			playingInfo.SongPosition++;

			if (playingInfo.SongPosition == currentSongInfo.EndPosition)
			{
				playingInfo.SongPosition = currentSongInfo.StartPosition;
				endReached = true;
			}

			SetTracks();

			worker.ShowSongPosition();
			worker.ShowTracks();
		}



		/********************************************************************/
		/// <summary>
		/// Find the tracks at the current song position
		/// </summary>
		/********************************************************************/
		private void SetTracks()
		{
			ushort songPosition = playingInfo.SongPosition;

			for (int i = 0; i < 6; i++)
			{
				VoiceInfo40_50 voiceInfo = voices[i];

				voiceInfo.WaitCounter = 0;

				Position positionInfo = moduleData.PositionList.Positions[i][songPosition];

				voiceInfo.Track = moduleData.Tracks[positionInfo.TrackNumber];
				voiceInfo.TrackPosition = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will handle the sample commands for all voices
		/// </summary>
		/********************************************************************/
		private void HandleSampleCommands()
		{
			for (int i = 0; i < 4; i++)
				HandleSampleCommands(voices[i], worker.VirtualChannels[i]);
		}



		/********************************************************************/
		/// <summary>
		/// Will handle the sample commands on a single voice
		/// </summary>
		/********************************************************************/
		private void HandleSampleCommands(VoiceInfo40_50 voiceInfo, IChannel channel)
		{
			if (voiceInfo.SampleCommandList == null)
				return;

			for (;;)
			{
				if (voiceInfo.SampleCommandWaitCounter == 0)
				{
					SampleCommandInfo sampleCommandInfo = voiceInfo.SampleCommandList[voiceInfo.SampleCommandPosition++];

					SampleCommand command = sampleCommandInfo.Command;
					ushort arg1 = sampleCommandInfo.Argument1;
					ushort arg2 = sampleCommandInfo.Argument2;

					if ((((ushort)command) & 0x4000) != 0)
						arg1 = voiceInfo.RepeatListValues[arg1];

					if ((((ushort)command) & 0x8000) != 0)
						arg2 = voiceInfo.RepeatListValues[arg2];

					command = (SampleCommand)(((ushort)command) & 0x1f);

					switch (command)
					{
						case SampleCommand.Stop:
						{
							voiceInfo.SampleCommandPosition--;
							voiceInfo.SampleCommandWaitCounter = 1;
							break;
						}

						case SampleCommand.SwitchSample:
						{
							voiceInfo.SampleNumber = arg1;

							Sample sample = moduleData.Samples[arg1];
							voiceInfo.Sample = sample;
							voiceInfo.SampleLength = sample.Length;

							voiceInfo.SampleTransposedNote = (ushort)(voiceInfo.TransposedNote + sample.NoteTranspose);
							voiceInfo.Period = periods[voiceInfo.SampleTransposedNote];
							voiceInfo.SampleData = sample.SampleData;
							break;
						}

						case SampleCommand.Wait:
						{
							voiceInfo.SampleCommandWaitCounter = arg1;
							break;
						}

						case SampleCommand.ChangeAddress:
						{
							voiceInfo.Hardware_SampleData = voiceInfo.Sample.SampleData;
							voiceInfo.Hardware_StartOffset = arg1;
							break;
						}

						case SampleCommand.SwitchSampleAndChangeAddress:
						{
							voiceInfo.SampleNumber = arg1;

							Sample sample = moduleData.Samples[arg1];
							voiceInfo.Sample = sample;
							voiceInfo.SampleData = sample.SampleData;

							voiceInfo.Hardware_SampleData = voiceInfo.SampleData;
							voiceInfo.Hardware_StartOffset = 0;
							break;
						}

						case SampleCommand.ChangeLength:
						{
							voiceInfo.SampleLength = arg1;
							voiceInfo.Hardware_SampleLength = arg1;
							break;
						}

						case SampleCommand.SwitchSampleAndChangeLength:
						{
							voiceInfo.SampleNumber = arg1;

							Sample sample = moduleData.Samples[arg1];
							voiceInfo.Sample = sample;
							voiceInfo.SampleLength = sample.Length;

							voiceInfo.Hardware_SampleLength = voiceInfo.SampleLength;
							break;
						}

						case SampleCommand.ChangePeriod:
						{
							voiceInfo.Period = (ushort)(voiceInfo.Period + (short)arg1);

							channel.SetAmigaPeriod(voiceInfo.Period);
							break;
						}

						case SampleCommand.Transpose:
						{
							voiceInfo.SampleTransposedNote = (ushort)(voiceInfo.SampleTransposedNote + (short)arg1);
							voiceInfo.Period = periods[voiceInfo.SampleTransposedNote];

							channel.SetAmigaPeriod(voiceInfo.Period);
							break;
						}

						case SampleCommand.ChangeVolume:
						{
							int volume = voiceInfo.Volume + (short)arg1;

							if (volume < 0)
								volume = 0;
							else if (volume > 64)
								volume = 64;

							voiceInfo.Volume = (ushort)volume;
							break;
						}

						case SampleCommand.SetListRepeat:
						{
							voiceInfo.RepeatListStack.Push(voiceInfo.SampleCommandPosition - 1);
							voiceInfo.RepeatListValues[arg1] = arg2;
							break;
						}

						case SampleCommand.DoListRepeat:
						{
							short tempArg2 = (short)arg2;

							int repeatPos = voiceInfo.RepeatListStack.Peek();
							SampleCommandInfo repeatCommand = voiceInfo.SampleCommandList[repeatPos];

							voiceInfo.RepeatListValues[repeatCommand.Argument1] = (ushort)(voiceInfo.RepeatListValues[repeatCommand.Argument1] + tempArg2);

							if (tempArg2 < 0)
							{
								if (voiceInfo.RepeatListValues[repeatCommand.Argument1] > arg1)
									voiceInfo.SampleCommandPosition = repeatPos + 1;
								else
									voiceInfo.RepeatListStack.Pop();
							}
							else
							{
								if (voiceInfo.RepeatListValues[repeatCommand.Argument1] < arg1)
									voiceInfo.SampleCommandPosition = repeatPos + 1;
								else
									voiceInfo.RepeatListStack.Pop();
							}
							break;
						}

						case SampleCommand.ChangeListRepeatValue:
						{
							voiceInfo.RepeatListValues[arg1] = (ushort)(voiceInfo.RepeatListValues[arg1] + (short)arg2);
							break;
						}

						case SampleCommand.SetListRepeatValue:
						{
							voiceInfo.RepeatListValues[arg1] = arg2;
							break;
						}

						case SampleCommand.PlaySample:
						{
							switch (voiceInfo.PlaySampleCommand)
							{
								case PlaySampleCommand.Mute:
								{
									channel.Mute();

									voiceInfo.SampleCommandPosition--;
									voiceInfo.SampleCommandWaitCounter = 1;

									voiceInfo.PlaySampleCommand = PlaySampleCommand.Play;
									break;
								}

								case PlaySampleCommand.Play:
								{
									channel.SetAmigaPeriod(voiceInfo.Period);

									channel.PlaySample((short)voiceInfo.SampleNumber, voiceInfo.SampleData, 0, voiceInfo.SampleLength);

									Sample sample = voiceInfo.Sample;

									if (sample.LoopEnd != 0)
									{
										voiceInfo.Hardware_SampleData = voiceInfo.SampleData;
										voiceInfo.Hardware_StartOffset = sample.LoopStart;
										voiceInfo.Hardware_SampleLength = (ushort)(sample.LoopEnd - sample.LoopStart);
									}
									else
										voiceInfo.Hardware_SampleLength = 0;

									voiceInfo.SampleCommandWaitCounter = 1;

									voiceInfo.PlaySampleCommand = PlaySampleCommand.Mute;
									break;
								}
							}
							break;
						}
					}
				}
				else
				{
					voiceInfo.SampleCommandWaitCounter--;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will handle the envelope for all voices
		/// </summary>
		/********************************************************************/
		private void HandleEnvelope()
		{
			for (int i = 0; i < 4; i++)
			{
				VoiceInfo40_50 voiceInfo = voices[i];
				IChannel channel = worker.VirtualChannels[i];

				if (voiceInfo.EnvelopeCounter == 0)
				{
					Envelope envelope = moduleData.Instruments[voiceInfo.InstrumentNumber].Envelope;

					switch (voiceInfo.EnvelopeCommand)
					{
						case EnvelopeCommand.Attack:
						{
							voiceInfo.EnvelopeVolume += envelope.AttackIncrement;

							if (voiceInfo.EnvelopeVolume >= 256)
							{
								voiceInfo.EnvelopeVolume = 256;
								voiceInfo.EnvelopeCommand = EnvelopeCommand.Decay;
							}

							voiceInfo.EnvelopeCounter = envelope.AttackSpeed;
							break;
						}

						case EnvelopeCommand.Decay:
						{
							voiceInfo.EnvelopeVolume -= envelope.DecayDecrement;

							if (voiceInfo.EnvelopeVolume <= envelope.DecayValue)
							{
								voiceInfo.EnvelopeVolume = (short)envelope.DecayValue;
								voiceInfo.EnvelopeCommand = EnvelopeCommand.Sustain;
							}

							voiceInfo.EnvelopeCounter = envelope.DecaySpeed;
							break;
						}

						case EnvelopeCommand.Sustain:
						{
							if (voiceInfo.StartEnvelopeRelease)
								voiceInfo.EnvelopeCommand = EnvelopeCommand.Release;

							break;
						}

						case EnvelopeCommand.Release:
						{
							voiceInfo.EnvelopeVolume -= envelope.ReleaseDecrement;

							if (voiceInfo.EnvelopeVolume <= 0)
							{
								voiceInfo.EnvelopeVolume = 0;
								voiceInfo.EnvelopeCommand = EnvelopeCommand.Done;
							}

							voiceInfo.EnvelopeCounter = envelope.ReleaseSpeed;
							break;
						}
					}
				}
				else
					voiceInfo.EnvelopeCounter--;

				int volume = (voiceInfo.Volume * voiceInfo.EnvelopeVolume) / 256;
				channel.SetAmigaVolume((ushort)volume);
			}
		}
		#endregion
	}
}
