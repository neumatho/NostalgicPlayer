/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.DeltaMusic10.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.DeltaMusic10
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class DeltaMusic10Worker : ModulePlayerWithSubSongDurationAgentBase
	{
		private Track[][] tracks;
		private BlockLine[][] blocks;
		private Instrument[] backupInstruments;

		private GlobalPlayingInfo playingInfo;
		private ChannelInfo[] channels;

		private const int InfoPositionLine = 3;
		private const int InfoTrackLine = 4;
		private const int InfoSpeedLine = 5;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "dm1" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 104)
				return AgentResult.Unknown;

			// Check mark
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint mark = moduleStream.Read_B_UINT32();
			if (mark != 0x414c4c20)				// ALL
				return AgentResult.Unknown;

			// Check all the lengths
			int totalLength = 104;

			for (int i = 0; i < 25; i++)
				totalLength += moduleStream.Read_B_INT32();

			if (totalLength > moduleStream.Length)
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		/********************************************************************/
		public override bool GetInformationString(int line, out string description, out string value)
		{
			// Find out which line to take
			switch (line)
			{
				// Number of positions
				case 0:
				{
					description = Resources.IDS_DM1_INFODESCLINE0;
					value = FormatPositionLengths();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_DM1_INFODESCLINE1;
					value = blocks.Length.ToString();
					break;
				}

				// Supported / used samples
				case 2:
				{
					description = Resources.IDS_DM1_INFODESCLINE2;
					value = "20";
					break;
				}

				// Playing positions
				case 3:
				{
					description = Resources.IDS_DM1_INFODESCLINE3;
					value = FormatPositions();
					break;
				}

				// Playing tracks
				case 4:
				{
					description = Resources.IDS_DM1_INFODESCLINE4;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 5:
				{
					description = Resources.IDS_DM1_INFODESCLINE5;
					value = playingInfo.PlaySpeed.ToString();
					break;
				}

				default:
				{
					description = null;
					value = null;

					return false;
				}
			}

			return true;
		}
		#endregion

		#region IModulePlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Will load the file into memory
		/// </summary>
		/********************************************************************/
		public override AgentResult Load(PlayerFileInfo fileInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			try
			{
				ModuleStream moduleStream = fileInfo.ModuleStream;

				// Read all the different lengths
				moduleStream.Seek(4, SeekOrigin.Begin);

				uint[] trackLengths = new uint[4];
				moduleStream.ReadArray_B_UINT32s(trackLengths, 0, 4);

				uint blockLength = moduleStream.Read_B_UINT32();

				uint[] instrumentLengths = new uint[20];
				moduleStream.ReadArray_B_UINT32s(instrumentLengths, 0, 20);

				// Read the tracks
				tracks = new Track[4][];

				for (int i = 0; i < 4; i++)
				{
					uint length = trackLengths[i] / 2;
					tracks[i] = new Track[length];

					for (int j = 0; j < length; j++)
					{
						tracks[i][j] = new Track
						{
							BlockNumber = moduleStream.Read_UINT8(),
							Transpose = moduleStream.Read_INT8()
						};
					}
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_DM1_ERR_LOADING_HEADER;
					return AgentResult.Error;
				}

				// Read the blocks
				uint count = blockLength / 64;
				blocks = new BlockLine[count][];

				for (int i = 0; i < count; i++)
				{
					BlockLine[] lines = new BlockLine[16];

					for (int j = 0; j < 16; j++)
					{
						lines[j] = new BlockLine
						{
							Instrument = moduleStream.Read_UINT8(),
							Note = moduleStream.Read_UINT8(),
							Effect = (Effect)moduleStream.Read_UINT8(),
							EffectArg = moduleStream.Read_UINT8()
						};
					}

					blocks[i] = lines;

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_DM1_ERR_LOADING_BLOCK;
						return AgentResult.Error;
					}
				}

				// Read the instruments
				backupInstruments = new Instrument[20];

				for (short i = 0; i < 20; i++)
				{
					uint length = instrumentLengths[i];
					if (length != 0)
					{
						Instrument inst = new Instrument
						{
							Number = i,

							AttackStep = moduleStream.Read_UINT8(),
							AttackDelay = moduleStream.Read_UINT8(),
							DecayStep = moduleStream.Read_UINT8(),
							DecayDelay = moduleStream.Read_UINT8(),
							Sustain = moduleStream.Read_B_UINT16(),
							ReleaseStep = moduleStream.Read_UINT8(),
							ReleaseDelay = moduleStream.Read_UINT8(),
							Volume = moduleStream.Read_UINT8(),
							VibratoWait = moduleStream.Read_UINT8(),
							VibratoStep = moduleStream.Read_UINT8(),
							VibratoLength = moduleStream.Read_UINT8(),
							BendRate = moduleStream.Read_INT8(),
							Portamento = moduleStream.Read_UINT8(),
							IsSample = moduleStream.Read_UINT8() != 0,
							TableDelay = moduleStream.Read_UINT8()
						};

						moduleStream.Read(inst.Arpeggio, 0, 8);

						inst.SampleLength = moduleStream.Read_B_UINT16();
						inst.RepeatStart = moduleStream.Read_B_UINT16();
						inst.RepeatLength = moduleStream.Read_B_UINT16();

						if (!inst.IsSample)
						{
							inst.Table = new byte[48];
							moduleStream.Read(inst.Table, 0, 48);
						}

						if (moduleStream.EndOfStream)
						{
							errorMessage = Resources.IDS_DM1_ERR_LOADING_BLOCK;
							return AgentResult.Error;
						}

						inst.SampleData = moduleStream.ReadSampleData(i, (int)length - (inst.IsSample ? 30 : 78), out _);

						if (moduleStream.EndOfStream)
						{
							errorMessage = Resources.IDS_DM1_ERR_LOADING_SAMPLES;
							return AgentResult.Error;
						}

						backupInstruments[i] = inst;
					}
				}
			}
			catch (Exception)
			{
				Cleanup();
				throw;
			}

			// Ok, we're done
			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public override void CleanupPlayer()
		{
			Cleanup();

			base.CleanupPlayer();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the current song
		/// </summary>
		/********************************************************************/
		public override bool InitSound(int songNumber, out string errorMessage)
		{
			if (!base.InitSound(songNumber, out errorMessage))
				return false;

			InitializeSound();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			for (int i = 0; i < 4; i++)
				CalculateFrequency(i, channels[i]);
		}



		/********************************************************************/
		/// <summary>
		/// Returns all the samples available in the module. If none, null
		/// is returned
		/// </summary>
		/********************************************************************/
		public override IEnumerable<SampleInfo> Samples
		{
			get
			{
				// Build frequency table
				uint[] frequencies = new uint[10 * 12];

				for (int j = 0; j < 6 * 12; j++)
					frequencies[1 * 12 + j] = 3546895U / Tables.Periods[j + 1];

				foreach (Instrument inst in backupInstruments)
				{
					SampleInfo sampleInfo;

					if (inst != null)
					{
						sampleInfo = new SampleInfo
						{
							Name = string.Empty,
							Flags = SampleInfo.SampleFlag.None,
							Volume = (ushort)(inst.Volume * 3),
							Panning = -1,
							NoteFrequencies = frequencies
						};

						if (inst.IsSample)
						{
							sampleInfo.Type = SampleInfo.SampleType.Sample;
							sampleInfo.Sample = inst.SampleData;
							sampleInfo.Length = inst.SampleLength;

							if (inst.RepeatLength > 1)
							{
								// Sample loops
								sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
								sampleInfo.LoopStart = inst.RepeatStart;
								sampleInfo.LoopLength = inst.RepeatLength;
							}
							else
							{
								// No loop
								sampleInfo.LoopStart = 0;
								sampleInfo.LoopLength = 0;
							}
						}
						else
						{
							sampleInfo.Type = SampleInfo.SampleType.Synthesis;
							sampleInfo.LoopStart = 0;
							sampleInfo.LoopLength = 0;
						}
					}
					else
					{
						sampleInfo = new SampleInfo
						{
							Name = string.Empty,
							Flags = SampleInfo.SampleFlag.None,
							Type = SampleInfo.SampleType.Sample,
							Volume = 256,
							Panning = -1,
							Sample = null,
							Length = 0,
							LoopStart = 0,
							LoopLength = 0,
							NoteFrequencies = null
						};
					}

					yield return sampleInfo;
				}
			}
		}
		#endregion

		#region ModulePlayerWithSubSongDurationAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected override void InitDuration(int subSong)
		{
			InitializeSound();
		}



		/********************************************************************/
		/// <summary>
		/// Create a snapshot of all the internal structures and return it
		/// </summary>
		/********************************************************************/
		protected override ISnapshot CreateSnapshot()
		{
			return new Snapshot(playingInfo, channels);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize internal structures based on the snapshot given
		/// </summary>
		/********************************************************************/
		protected override bool SetSnapshot(ISnapshot snapshot, out string errorMessage)
		{
			errorMessage = string.Empty;

			// Start to make a clone of the snapshot
			Snapshot currentSnapshot = (Snapshot)snapshot;
			Snapshot clonedSnapshot = new Snapshot(currentSnapshot.PlayingInfo, currentSnapshot.Channels);

			playingInfo = clonedSnapshot.PlayingInfo;
			channels = clonedSnapshot.Channels;

			UpdateModuleInformation();

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound()
		{
			// Initialize work variables
			playingInfo = new GlobalPlayingInfo
			{
				PlaySpeed = 6
			};

			// Rebuild instruments from backup variables
			playingInfo.Instruments = new Instrument[20];

			for (int i = 0; i < 20; i++)
			{
				if (backupInstruments[i] != null)
					playingInfo.Instruments[i] = backupInstruments[i].MakeDeepClone();
			}

			// Initialize channel structure
			channels = new ChannelInfo[4];

			for (int i = 0; i < 4; i++)
			{
				channels[i] = new ChannelInfo
				{
					SoundData = null,
					Period = 0,
					SoundTable = playingInfo.Instruments.FirstOrDefault(x => x != null)?.Table,
					SoundTableCounter = 0,
					SoundTableDelay = 0,
					Track = tracks[i],
					TrackCounter = 0,
					Block = blocks[0],
					BlockCounter = 0,
					VibratoWait = 0,
					VibratoLength = 0,
					VibratoPosition = 0,
					VibratoCompare = 0,
					VibratoFrequency = 0,
					FrequencyData = 0,
					ActualVolume = 0,
					AttackDelay = 0,
					DecayDelay = 0,
					Sustain = 0,
					ReleaseDelay = 0,
					PlaySpeed = 1,
					BendRateFrequency = 0,
					Transpose = 0,
					Status = 0,
					ArpeggioCounter = 0,
					EffectNumber = Effect.None,
					EffectData = 0,
					RetriggerSound = false
				};
			}
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player has allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			tracks = null;
			blocks = null;
			backupInstruments = null;

			playingInfo = null;
			channels = null;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate all effects and notes for a single channel
		/// </summary>
		/********************************************************************/
		private void CalculateFrequency(int channelNumber, ChannelInfo channel)
		{
			Instrument inst = channel.SoundData;

			channel.PlaySpeed--;
			if (channel.PlaySpeed == 0)
			{
				channel.PlaySpeed = playingInfo.PlaySpeed;

				if (channel.BlockCounter == 0)
				{
					Track newTrack;

					if (channel.TrackCounter == channel.Track.Length)
					{
						channel.TrackCounter = 0;
						OnEndReached(channelNumber);
					}

					for (;;)
					{
						newTrack = channel.Track[channel.TrackCounter];
						if ((newTrack.BlockNumber != 0xff) || (newTrack.Transpose != -1))
							break;

						ushort oldTrackCounter = channel.TrackCounter;

						newTrack = channel.Track[channel.TrackCounter + 1];
						channel.TrackCounter = (ushort)(((newTrack.BlockNumber << 8) | (byte)newTrack.Transpose) & 0x7ff);

						if (channel.TrackCounter < oldTrackCounter)
							OnEndReached(channelNumber);
					}

					channel.Transpose = newTrack.Transpose;
					channel.Block = blocks[newTrack.BlockNumber];
					channel.TrackCounter++;

					ShowSongPositions();
					ShowTracks();
				}

				BlockLine blockLine = channel.Block[channel.BlockCounter];

				if (blockLine.Effect != Effect.None)
				{
					channel.EffectNumber = blockLine.Effect;
					channel.EffectData = blockLine.EffectArg;
				}

				if (blockLine.Note != 0)
				{
					channel.FrequencyData = (byte)(blockLine.Note + channel.Transpose);

					channel.Status = 0;
					channel.BendRateFrequency = 0;
					channel.ArpeggioCounter = 0;

					channel.EffectNumber = blockLine.Effect;
					channel.EffectData = blockLine.EffectArg;

					inst = playingInfo.Instruments[blockLine.Instrument];
					channel.SoundData = inst;

					channel.SoundTable = inst.Table;
					channel.SoundTableCounter = 0;

					if (inst.IsSample)
					{
						VirtualChannels[channelNumber].PlaySample(inst.Number, inst.SampleData, 0, inst.SampleLength);

						if (inst.RepeatLength > 1)
							VirtualChannels[channelNumber].SetLoop(inst.RepeatStart, inst.RepeatLength);
					}
					else
						channel.RetriggerSound = true;

					channel.VibratoWait = inst.VibratoWait;
					byte vibLen = inst.VibratoLength;
					channel.VibratoLength = vibLen;
					channel.VibratoPosition = vibLen;
					channel.VibratoCompare = (byte)(vibLen * 2);

					channel.ActualVolume = 0;
					channel.SoundTableDelay = 0;
					channel.SoundTableCounter = 0;
					channel.AttackDelay = 0;
					channel.DecayDelay = 0;
					channel.Sustain = inst.Sustain;
					channel.ReleaseDelay = 0;
				}

				channel.BlockCounter++;
				if (channel.BlockCounter == 16)
					channel.BlockCounter = 0;
			}

			if (inst != null)
			{
				if (!inst.IsSample)
				{
					if (channel.SoundTableDelay == 0)
						SoundTableHandler(channelNumber, channel, inst);
					else
						channel.SoundTableDelay--;
				}

				PortamentoHandler(channel, inst);
				VibratoHandler(channel, inst);
				BendrateHandler(channel, inst);
				EffectHandler(channel, inst);
				ArpeggioHandler(channelNumber, channel, inst);
				VolumeHandler(channelNumber, channel, inst);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Find out what to play
		/// </summary>
		/********************************************************************/
		private void SoundTableHandler(int channelNumber, ChannelInfo channel, Instrument inst)
		{
			channel.SoundTableDelay = inst.TableDelay;

			for (;;)
			{
				if (channel.SoundTableCounter >= 48)
					channel.SoundTableCounter = 0;

				byte data = channel.SoundTable[channel.SoundTableCounter];
				if (data == 0xff)
					channel.SoundTableCounter = channel.SoundTable[channel.SoundTableCounter + 1];
				else if (data >= 0x80)
				{
					inst.TableDelay = (byte)(data & 0x7f);
					channel.SoundTableCounter++;
				}
				else
				{
					channel.SoundTableCounter++;

					data *= 32;

					if (channel.RetriggerSound)
					{
						VirtualChannels[channelNumber].PlaySample(inst.Number, inst.SampleData, data, inst.SampleLength);
						channel.RetriggerSound = false;
					}
					else
						VirtualChannels[channelNumber].SetSample(inst.SampleData, data, inst.SampleLength);

					VirtualChannels[channelNumber].SetLoop(inst.SampleData, data, inst.SampleLength);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Run the portamento effect
		/// </summary>
		/********************************************************************/
		private void PortamentoHandler(ChannelInfo channel, Instrument inst)
		{
			if (inst.Portamento != 0)
			{
				if (channel.Period == 0)
					channel.Period = (ushort)(Tables.Periods[channel.FrequencyData] + channel.BendRateFrequency);
				else
				{
					ushort period = channel.Period;
					ushort wantedPeriod = (ushort)(Tables.Periods[channel.FrequencyData] + channel.BendRateFrequency);

					if (period > wantedPeriod)
					{
						period -= inst.Portamento;
						if (period < wantedPeriod)
							channel.Period = wantedPeriod;
						else
							channel.Period = period;
					}
					else if (period < wantedPeriod)
					{
						period += inst.Portamento;
						if (period > wantedPeriod)
							channel.Period = wantedPeriod;
						else
							channel.Period = period;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Run the vibrato effect
		/// </summary>
		/********************************************************************/
		private void VibratoHandler(ChannelInfo channel, Instrument inst)
		{
			if (channel.VibratoWait == 0)
			{
				channel.VibratoFrequency = (ushort)(channel.VibratoPosition * inst.VibratoStep);

				if ((channel.Status & 0x01) != 0)
				{
					channel.VibratoPosition--;
					if (channel.VibratoPosition == 0)
						channel.Status ^= 0b00000001;
				}
				else
				{
					channel.VibratoPosition++;
					if (channel.VibratoPosition == channel.VibratoCompare)
						channel.Status ^= 0b00000001;
				}
			}
			else
				channel.VibratoWait--;
		}



		/********************************************************************/
		/// <summary>
		/// Run the bend effect
		/// </summary>
		/********************************************************************/
		private void BendrateHandler(ChannelInfo channel, Instrument inst)
		{
			if (inst.BendRate >= 0)
				channel.BendRateFrequency = (short)(channel.BendRateFrequency - inst.BendRate);
			else
				channel.BendRateFrequency = (short)(channel.BendRateFrequency + -inst.BendRate);
		}



		/********************************************************************/
		/// <summary>
		/// Handle the different effects
		/// </summary>
		/********************************************************************/
		private void EffectHandler(ChannelInfo channel, Instrument inst)
		{
			byte data = channel.EffectData;

			switch (channel.EffectNumber)
			{
				case Effect.SetSpeed:
				{
					if (data != 0)
						ChangeSpeed(data);

					break;
				}

				case Effect.SlideUp:
				{
					channel.BendRateFrequency -= data;
					break;
				}

				case Effect.SlideDown:
				{
					channel.BendRateFrequency += data;
					break;
				}

				case Effect.SetFilter:
				{
					AmigaFilter = data == 0;
					break;
				}

				case Effect.SetVibratoWait:
				{
					inst.VibratoWait = data;
					break;
				}

				case Effect.SetVibratoStep:
				{
					inst.VibratoStep = data;
					break;
				}

				case Effect.SetVibratoLength:
				{
					inst.VibratoLength = data;
					break;
				}

				case Effect.SetBendRate:
				{
					inst.BendRate = (sbyte)data;
					break;
				}

				case Effect.SetPortamento:
				{
					inst.Portamento = data;
					break;
				}

				case Effect.SetVolume:
				{
					if (data > 64)
						data = 64;

					inst.Volume = data;
					break;
				}

				case Effect.SetArp1:
				{
					inst.Arpeggio[0] = data;
					break;
				}

				case Effect.SetArp2:
				{
					inst.Arpeggio[1] = data;
					break;
				}

				case Effect.SetArp3:
				{
					inst.Arpeggio[2] = data;
					break;
				}

				case Effect.SetArp4:
				{
					inst.Arpeggio[3] = data;
					break;
				}

				case Effect.SetArp5:
				{
					inst.Arpeggio[4] = data;
					break;
				}

				case Effect.SetArp6:
				{
					inst.Arpeggio[5] = data;
					break;
				}

				case Effect.SetArp7:
				{
					inst.Arpeggio[6] = data;
					break;
				}

				case Effect.SetArp8:
				{
					inst.Arpeggio[7] = data;
					break;
				}

				case Effect.SetArp1_5:
				{
					inst.Arpeggio[0] = data;
					inst.Arpeggio[4] = data;
					break;
				}

				case Effect.SetArp2_6:
				{
					inst.Arpeggio[1] = data;
					inst.Arpeggio[5] = data;
					break;
				}

				case Effect.SetArp3_7:
				{
					inst.Arpeggio[2] = data;
					inst.Arpeggio[6] = data;
					break;
				}

				case Effect.SetArp4_8:
				{
					inst.Arpeggio[3] = data;
					inst.Arpeggio[7] = data;
					break;
				}

				case Effect.SetAttackStep:
				{
					if (data > 64)
						data = 64;

					inst.AttackStep = data;
					break;
				}

				case Effect.SetAttackDelay:
				{
					inst.AttackDelay = data;
					break;
				}

				case Effect.SetDecayStep:
				{
					if (data > 64)
						data = 64;

					inst.DecayStep = data;
					break;
				}

				case Effect.SetDecayDelay:
				{
					inst.DecayDelay = data;
					break;
				}

				case Effect.SetSustain1:
				{
					inst.Sustain = (ushort)((inst.Sustain & 0x00ff) | (data << 8));
					break;
				}

				case Effect.SetSustain2:
				{
					inst.Sustain = (ushort)((inst.Sustain & 0xff00) | data);
					break;
				}

				case Effect.SetReleaseStep:
				{
					if (data > 64)
						data = 64;

					inst.ReleaseStep = data;
					break;
				}

				case Effect.SetReleaseDelay:
				{
					inst.ReleaseDelay = data;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Run the arpeggio effect
		/// </summary>
		/********************************************************************/
		private void ArpeggioHandler(int channelNumber, ChannelInfo channel, Instrument inst)
		{
			byte arp = inst.Arpeggio[channel.ArpeggioCounter];

			channel.ArpeggioCounter++;
			channel.ArpeggioCounter &= 0b00000111;

			ushort newPeriod = Tables.Periods[channel.FrequencyData + arp];
			newPeriod = (ushort)((newPeriod - channel.VibratoLength * inst.VibratoStep) + channel.BendRateFrequency);

			if (inst.Portamento != 0)
				newPeriod = channel.Period;
			else
				channel.Period = 0;

			newPeriod += channel.VibratoFrequency;
			VirtualChannels[channelNumber].SetAmigaPeriod(newPeriod);
		}



		/********************************************************************/
		/// <summary>
		/// Run the volume (envelope) effect
		/// </summary>
		/********************************************************************/
		private void VolumeHandler(int channelNumber, ChannelInfo channel, Instrument inst)
		{
			int actualVolume = channel.ActualVolume;
			byte status = (byte)(channel.Status & 0b00001110);

			if (status == 0)
			{
				if (channel.AttackDelay == 0)
				{
					channel.AttackDelay = inst.AttackDelay;
					actualVolume += inst.AttackStep;

					if (actualVolume >= 64)
					{
						actualVolume = 64;
						status |= 0b00000010;
						channel.Status |= 0b00000010;
					}
				}
				else
					channel.AttackDelay--;
			}

			if (status == 0b00000010)
			{
				if (channel.DecayDelay == 0)
				{
					channel.DecayDelay = inst.DecayDelay;
					actualVolume -= inst.DecayStep;

					if (actualVolume <= inst.Volume)
					{
						actualVolume = inst.Volume;
						status |= 0b00000110;
						channel.Status |= 0b00000110;
					}
				}
				else
					channel.DecayDelay--;
			}

			if (status == 0b00000110)
			{
				if (channel.Sustain == 0)
				{
					status |= 0b00001110;
					channel.Status |= 0b00001110;
				}
				else
					channel.Sustain--;
			}

			if (status == 0b00001110)
			{
				if (channel.ReleaseDelay == 0)
				{
					channel.ReleaseDelay = inst.ReleaseDelay;
					actualVolume -= inst.ReleaseStep;

					if (actualVolume <= 0)
					{
						actualVolume = 0;
						channel.Status &= 0b00001001;
					}
				}
				else
					channel.ReleaseDelay--;
			}

			channel.ActualVolume = (byte)actualVolume;
			VirtualChannels[channelNumber].SetAmigaVolume((ushort)actualVolume);
		}



		/********************************************************************/
		/// <summary>
		/// Will change the speed on the module
		/// </summary>
		/********************************************************************/
		private void ChangeSpeed(byte newSpeed)
		{
			if (newSpeed != playingInfo.PlaySpeed)
			{
				// Remember the speed
				playingInfo.PlaySpeed = newSpeed;

				ShowSpeed();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current song positions
		/// </summary>
		/********************************************************************/
		private void ShowSongPositions()
		{
			OnModuleInfoChanged(InfoPositionLine, FormatPositions());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with track numbers
		/// </summary>
		/********************************************************************/
		private void ShowTracks()
		{
			OnModuleInfoChanged(InfoTrackLine, FormatTracks());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current speed
		/// </summary>
		/********************************************************************/
		private void ShowSpeed()
		{
			OnModuleInfoChanged(InfoSpeedLine, playingInfo.PlaySpeed.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with all dynamic values
		/// </summary>
		/********************************************************************/
		private void UpdateModuleInformation()
		{
			ShowSongPositions();
			ShowTracks();
			ShowSpeed();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the position list lengths
		/// </summary>
		/********************************************************************/
		private string FormatPositionLengths()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < 4; i++)
			{
				sb.Append(channels[i].Track.Length - 2);	// -2 because the last two block are an "end of track" + "new position" blocks
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing positions
		/// </summary>
		/********************************************************************/
		private string FormatPositions()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < 4; i++)
			{
				sb.Append(channels[i].TrackCounter - 1);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing tracks
		/// </summary>
		/********************************************************************/
		private string FormatTracks()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < 4; i++)
			{
				int index = channels[i].TrackCounter;
				if (index > 0)
					index--;

				sb.Append(channels[i].Track[index].BlockNumber);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
		#endregion
	}
}
