/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.DeltaMusic20.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.DeltaMusic20
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class DeltaMusic20Worker : ModulePlayerWithSubSongDurationAgentBase
	{
		private sbyte[][] arpeggios;
		private Instrument[] instruments;

		private TrackInfo[] tracks;
		private BlockLine[][] blocks;

		private sbyte[][] waveforms;

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
		public override string[] FileExtensions => [ "dm2" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 0xfda)
				return AgentResult.Unknown;

			// Check mark
			moduleStream.Seek(0xbc6, SeekOrigin.Begin);

			uint mark = moduleStream.Read_B_UINT32();
			if (mark != 0x2e464e4c)				// .FNL
				return AgentResult.Unknown;

			// Check all the lengths
			long totalLength = 0xfda;

			moduleStream.Seek(0xfca, SeekOrigin.Begin);

			// Add track lengths
			long tempLength = 0;
			for (int i = 0; i < 4; i++)
			{
				moduleStream.Seek(2, SeekOrigin.Current);
				tempLength += moduleStream.Read_B_INT16();
			}

			totalLength += tempLength;

			// Add block length
			moduleStream.Seek(tempLength, SeekOrigin.Current);
			tempLength = moduleStream.Read_B_INT32();
			totalLength += tempLength + 4;

			// Add instruments length
			moduleStream.Seek(tempLength + 256 - 2, SeekOrigin.Current);
			tempLength = moduleStream.Read_B_INT16();
			totalLength += tempLength + 256;

			// Add waveforms length
			moduleStream.Seek(tempLength, SeekOrigin.Current);
			tempLength = moduleStream.Read_B_INT32();
			totalLength += tempLength + 4 + 64;

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
				// Number of positions:
				case 0:
				{
					description = Resources.IDS_DM2_INFODESCLINE0;
					value = FormatPositionLengths();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_DM2_INFODESCLINE1;
					value = blocks.Length.ToString();
					break;
				}

				// Used samples
				case 2:
				{
					description = Resources.IDS_DM2_INFODESCLINE2;

					int index = Array.IndexOf(instruments, null);
					if (index < 0)
						index = instruments.Length;

					value = index.ToString();
					break;
				}

				// Playing positions
				case 3:
				{
					description = Resources.IDS_DM2_INFODESCLINE3;
					value = FormatPositions();
					break;
				}

				// Playing tracks
				case 4:
				{
					description = Resources.IDS_DM2_INFODESCLINE4;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 5:
				{
					description = Resources.IDS_DM2_INFODESCLINE5;
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

				// Read arpeggios
				moduleStream.Seek(0xbca, SeekOrigin.Begin);

				arpeggios = new sbyte[64][];

				for (int i = 0; i < 64; i++)
				{
					arpeggios[i] = new sbyte[16];
					moduleStream.ReadSigned(arpeggios[i], 0, 16);
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_DM2_ERR_LOADING_ARPEGGIOS;
					return AgentResult.Error;
				}

				// Read tracks
				tracks = new TrackInfo[4];

				for (int i = 0; i < 4; i++)
				{
					tracks[i] = new TrackInfo
					{
						LoopPosition = moduleStream.Read_B_UINT16(),
						Track = new Track[moduleStream.Read_B_UINT16() / 2]
					};
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_DM2_ERR_LOADING_TRACK;
					return AgentResult.Error;
				}

				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < tracks[i].Track.Length; j++)
					{
						tracks[i].Track[j] = new Track
						{
							BlockNumber = moduleStream.Read_UINT8(),
							Transpose = moduleStream.Read_INT8()
						};
					}

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_DM2_ERR_LOADING_TRACK;
						return AgentResult.Error;
					}
				}

				// Read blocks
				blocks = new BlockLine[moduleStream.Read_B_UINT32() / 64][];

				for (int i = 0; i < blocks.Length; i++)
				{
					BlockLine[] lines = new BlockLine[16];

					for (int j = 0; j < 16; j++)
					{
						lines[j] = new BlockLine
						{
							Note = moduleStream.Read_UINT8(),
							Instrument = moduleStream.Read_UINT8(),
							Effect = (Effect)moduleStream.Read_UINT8(),
							EffectArg = moduleStream.Read_UINT8()
						};
					}

					blocks[i] = lines;

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_DM2_ERR_LOADING_BLOCK;
						return AgentResult.Error;
					}
				}

				// Read instruments
				ushort[] instrumentOffsets = new ushort[128];
				moduleStream.ReadArray_B_UINT16s(instrumentOffsets, 1, 127);

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_DM2_ERR_LOADING_INSTRUMENT;
					return AgentResult.Error;
				}

				instruments = new Instrument[128];

				ushort breakOffset = moduleStream.Read_B_UINT16();
				long startOffset = moduleStream.Position;

				for (short i = 0; i < 128; i++)
				{
					if (instrumentOffsets[i] == breakOffset)
						break;

					moduleStream.Seek(startOffset + instrumentOffsets[i], SeekOrigin.Begin);

					Instrument inst = new Instrument
					{
						Number = i,
						SampleLength = (ushort)(moduleStream.Read_B_UINT16() * 2),
						RepeatStart = moduleStream.Read_B_UINT16(),
						RepeatLength = (ushort)(moduleStream.Read_B_UINT16() * 2)
					};

					if (inst.RepeatStart + inst.RepeatLength >= inst.SampleLength)
						inst.RepeatLength = (ushort)(inst.SampleLength - inst.RepeatStart);

					for (int j = 0; j < 5; j++)
					{
						inst.VolumeTable[j] = new VolumeInfo
						{
							Speed = moduleStream.Read_UINT8(),
							Level = moduleStream.Read_UINT8(),
							Sustain = moduleStream.Read_UINT8()
						};
					}

					for (int j = 0; j < 5; j++)
					{
						inst.VibratoTable[j] = new VibratoInfo
						{
							Speed = moduleStream.Read_UINT8(),
							Delay = moduleStream.Read_UINT8(),
							Sustain = moduleStream.Read_UINT8()
						};
					}

					inst.PitchBend = moduleStream.Read_B_UINT16();
					inst.IsSample = moduleStream.Read_UINT8() == 0xff;
					inst.SampleNumber = (byte)(moduleStream.Read_UINT8() & 0x7);

					int bytesRead = moduleStream.Read(inst.Table, 0, 48);

					if (bytesRead < 48)
					{
						errorMessage = Resources.IDS_DM2_ERR_LOADING_INSTRUMENT;
						return AgentResult.Error;
					}

					instruments[i] = inst;
				}

				// Read waveforms
				waveforms = new sbyte[moduleStream.Read_B_UINT32() / 256][];

				for (int i = 0; i < waveforms.Length; i++)
				{
					waveforms[i] = new sbyte[256];
					moduleStream.ReadSigned(waveforms[i], 0, 256);

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_DM2_ERR_LOADING_WAVEFORM;
						return AgentResult.Error;
					}
				}

				// Skip unknown data
				moduleStream.Seek(64, SeekOrigin.Current);

				// Read sample data
				uint[] sampleOffsets = new uint[8];
				moduleStream.ReadArray_B_UINT32s(sampleOffsets, 0, 8);

				startOffset = moduleStream.Position;

				foreach (Instrument inst in instruments.Where(i => (i != null) && i.IsSample))
				{
					moduleStream.Seek(startOffset + sampleOffsets[inst.SampleNumber], SeekOrigin.Begin);

					inst.SampleData = moduleStream.ReadSampleData(inst.SampleNumber, inst.SampleLength, out int readBytes);

					if (readBytes < (inst.SampleLength - 256))
					{
						errorMessage = Resources.IDS_DM2_ERR_LOADING_SAMPLES;
						return AgentResult.Error;
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
			GenerateNoiseWaveform();

			playingInfo.Tick--;
			if (playingInfo.Tick < 0)
				playingInfo.Tick = playingInfo.PlaySpeed;

			for (int i = 0; i < 4; i++)
				ProcessChannel(i, channels[i]);
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

				foreach (Instrument inst in instruments)
				{
					if (inst == null)
						break;

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = string.Empty,
						Flags = SampleInfo.SampleFlag.None,
						Volume = 256,
						Panning = -1,
						NoteFrequencies = frequencies
					};

					if (inst.IsSample)
					{
						sampleInfo.Type = SampleInfo.SampleType.Sample;
						sampleInfo.Sample = inst.SampleData;
						sampleInfo.Length = inst.SampleLength;

						if (inst.RepeatLength > 2)
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
				GlobalVolume = 63,
				PlaySpeed = 6,
				Tick = 1
			};

			// Initialize channel structure
			channels = new ChannelInfo[4];

			for (int i = 0; i < 4; i++)
			{
				channels[i] = new ChannelInfo
				{
					Track = tracks[i].Track,
					TrackLoopPosition = tracks[i].LoopPosition,
					TrackLength = (ushort)tracks[i].Track.Length,
					BlockPosition = 0,
					CurrentTrackPosition = -1,
					NextTrackPosition = 0,
					Instrument = instruments[0],
					ArpeggioPosition = 0,
					Arpeggio = arpeggios[0],
					ActualVolume = 0,
					VolumePosition = 0,
					VolumeSustain = 0,
					Portamento = 0,
					PitchBend = 0,
					MaxVolume = 63,
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
			arpeggios = null;
			instruments = null;

			tracks = null;
			blocks = null;

			waveforms = null;

			playingInfo = null;
			channels = null;
		}



		/********************************************************************/
		/// <summary>
		/// Noise generator
		/// </summary>
		/********************************************************************/
		private void GenerateNoiseWaveform()
		{
			uint noiseValue = playingInfo.LastNoiseValue;
			Span<uint> waveform = MemoryMarshal.Cast<sbyte, uint>(waveforms[0]);

			for (int i = 0; i < 16; i++)
			{
				noiseValue = BitOperations.RotateLeft(noiseValue, 7);
				noiseValue += 0x6eca756d;
				noiseValue ^= 0x9e59a92b;
				waveform[i] = noiseValue;
			}

			playingInfo.LastNoiseValue = noiseValue;
		}



		/********************************************************************/
		/// <summary>
		/// Process a single channel
		/// </summary>
		/********************************************************************/
		private void ProcessChannel(int channelNumber, ChannelInfo channel)
		{
			if (channel.TrackLength == 0)
			{
				OnEndReached(channelNumber);
				return;
			}

			Instrument inst = channel.Instrument;

			if (playingInfo.Tick == 0)
			{
				if (channel.BlockPosition == 0)
				{
					Track track = channel.Track[channel.NextTrackPosition];
					channel.Transpose = track.Transpose;
					channel.Block = blocks[track.BlockNumber];

					if (channel.NextTrackPosition <= channel.CurrentTrackPosition)
						OnEndReached(channelNumber);

					channel.CurrentTrackPosition = (short)channel.NextTrackPosition;

					channel.NextTrackPosition++;
					if (channel.NextTrackPosition >= channel.TrackLength)
						channel.NextTrackPosition = channel.TrackLoopPosition >= channel.TrackLength ? (ushort)0 : channel.TrackLoopPosition;

					ShowSongPositions();
					ShowTracks();
				}

				BlockLine block = channel.Block[channel.BlockPosition];

				if (block.Note != 0)
				{
					channel.Note = block.Note;
					channel.Period = Tables.Periods[channel.Note + channel.Transpose];

					inst = channel.Instrument = instruments[block.Instrument];
					if (inst != null)
					{
						if (inst.IsSample)
						{
							VirtualChannels[channelNumber].PlaySample(inst.Number, inst.SampleData, 0, inst.SampleLength);

							if (inst.RepeatLength > 1)
								VirtualChannels[channelNumber].SetLoop(inst.RepeatStart, inst.RepeatLength);
						}
						else
							channel.RetriggerSound = true;

						channel.SoundTableDelay = 0;
						channel.SoundTablePosition = 0;
						channel.ActualVolume = 0;
						channel.VolumeSustain = 0;
						channel.VolumePosition = 0;
						channel.ArpeggioPosition = 0;
						channel.VibratoDirection = false;
						channel.VibratoPeriod = 0;
						channel.VibratoDelay = inst.VibratoTable[0].Delay;
						channel.VibratoPosition = 0;
						channel.VibratoSustain = inst.VibratoTable[0].Sustain;
					}
					else
						VirtualChannels[channelNumber].Mute();
				}

				ParseEffect(channel, block.Effect, block.EffectArg);

				channel.BlockPosition++;
				channel.BlockPosition &= 0x0f;
			}

			if (inst != null)
			{
				SoundTableHandler(channelNumber, channel, inst);
				VibratoHandler(channel, inst);
				VolumeHandler(channel, inst);
				PortamentoHandler(channel);
				ArpeggioHandler(channel);

				channel.VibratoPeriod -= (ushort)(inst.PitchBend - channel.PitchBend);

				ushort newPeriod = (ushort)(channel.FinalPeriod + channel.VibratoPeriod);
				VirtualChannels[channelNumber].SetAmigaPeriod(newPeriod);

				byte newVolume = (byte)((channel.ActualVolume >> 2) & 0x3f);

				if (newVolume > channel.MaxVolume)
					newVolume = channel.MaxVolume;

				if (newVolume > playingInfo.GlobalVolume)
					newVolume = playingInfo.GlobalVolume;

				VirtualChannels[channelNumber].SetAmigaVolume(newVolume);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse effect
		/// </summary>
		/********************************************************************/
		private void ParseEffect(ChannelInfo channel, Effect effect, byte effectArg)
		{
			switch (effect)
			{
				case Effect.SetSpeed:
				{
					ChangeSpeed((sbyte)(effectArg & 0x0f));
					break;
				}

				case Effect.SetFilter:
				{
					AmigaFilter = effectArg != 0;
					break;
				}

				case Effect.SetBendRateUp:
				{
					channel.PitchBend = (short)-(effectArg & 0x00ff);
					break;
				}

				case Effect.SetBendRateDown:
				{
					channel.PitchBend = (short)(effectArg & 0x00ff);
					break;
				}

				case Effect.SetPortamento:
				{
					channel.Portamento = effectArg;
					break;
				}

				case Effect.SetVolume:
				{
					channel.MaxVolume = (byte)(effectArg & 0x3f);
					break;
				}

				case Effect.SetGlobalVolume:
				{
					playingInfo.GlobalVolume = (byte)(effectArg & 0x3f);
					break;
				}

				case Effect.SetArp:
				{
					channel.Arpeggio = arpeggios[effectArg & 0x3f];
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Find out what to play
		/// </summary>
		/********************************************************************/
		private void SoundTableHandler(int channelNumber, ChannelInfo channel, Instrument inst)
		{
			if (!inst.IsSample)
			{
				if (channel.SoundTableDelay != 0)
					channel.SoundTableDelay--;
				else
				{
					channel.SoundTableDelay = inst.SampleNumber;

					byte data = inst.Table[channel.SoundTablePosition];
					if (data == 0xff)
					{
						channel.SoundTablePosition = inst.Table[channel.SoundTablePosition + 1];

						data = inst.Table[channel.SoundTablePosition];
						if (data == 0xff)
							return;
					}

					if (channel.RetriggerSound)
					{
						if (inst.SampleLength > 0)
						{
							VirtualChannels[channelNumber].PlaySample(inst.Number, waveforms[data], 0, inst.SampleLength);
							VirtualChannels[channelNumber].SetLoop(0, inst.SampleLength);
						}
						else
							VirtualChannels[channelNumber].Mute();

						channel.RetriggerSound = false;
					}
					else
					{
						if (inst.SampleLength > 0)
						{
							VirtualChannels[channelNumber].SetSample(waveforms[data], 0, inst.SampleLength);
							VirtualChannels[channelNumber].SetLoop(0, inst.SampleLength);
						}
					}

					channel.SoundTablePosition++;
					if (channel.SoundTablePosition >= 48)
						channel.SoundTablePosition = 0;
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
			VibratoInfo info = inst.VibratoTable[channel.VibratoPosition];

			if (channel.VibratoDirection)
				channel.VibratoPeriod -= info.Speed;
			else
				channel.VibratoPeriod += info.Speed;

			channel.VibratoDelay--;
			if (channel.VibratoDelay == 0)
			{
				channel.VibratoDelay = info.Delay;
				channel.VibratoDirection = !channel.VibratoDirection;
			}

			if (channel.VibratoSustain != 0)
				channel.VibratoSustain--;
			else
			{
				channel.VibratoPosition++;
				if (channel.VibratoPosition == 5)
					channel.VibratoPosition = 4;

				channel.VibratoSustain = inst.VibratoTable[channel.VibratoPosition].Sustain;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Run the volume (envelope) effect
		/// </summary>
		/********************************************************************/
		private void VolumeHandler(ChannelInfo channel, Instrument inst)
		{
			if (channel.VolumeSustain != 0)
				channel.VolumeSustain--;
			else
			{
				VolumeInfo info = inst.VolumeTable[channel.VolumePosition];

				if (channel.ActualVolume >= info.Level)
				{
					channel.ActualVolume -= info.Speed;

					if (channel.ActualVolume < info.Level)
					{
						channel.ActualVolume = info.Level;

						channel.VolumePosition++;
						if (channel.VolumePosition == 5)
							channel.VolumePosition = 4;

						channel.VolumeSustain = info.Sustain;
					}
				}
				else
				{
					channel.ActualVolume += info.Speed;

					if (channel.ActualVolume > info.Level)
					{
						channel.ActualVolume = info.Level;

						channel.VolumePosition++;
						if (channel.VolumePosition == 5)
							channel.VolumePosition = 4;

						channel.VolumeSustain = info.Sustain;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Run the portamento effect
		/// </summary>
		/********************************************************************/
		private void PortamentoHandler(ChannelInfo channel)
		{
			if (channel.Portamento != 0)
			{
				if (channel.FinalPeriod >= channel.Period)
				{
					channel.FinalPeriod -= channel.Portamento;

					if (channel.FinalPeriod < channel.Period)
						channel.FinalPeriod = channel.Period;
				}
				else
				{
					channel.FinalPeriod += channel.Portamento;

					if (channel.FinalPeriod > channel.Period)
						channel.FinalPeriod = channel.Period;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Run the arpeggio effect
		/// </summary>
		/********************************************************************/
		private void ArpeggioHandler(ChannelInfo channel)
		{
			sbyte arp = channel.Arpeggio[channel.ArpeggioPosition];

			if ((channel.ArpeggioPosition != 0) && (arp == -128))
			{
				channel.ArpeggioPosition = 0;

				arp = channel.Arpeggio[0];
			}

			channel.ArpeggioPosition++;
			channel.ArpeggioPosition &= 0x0f;

			if (channel.Portamento == 0)
			{
				byte index = (byte)(arp + channel.Note + channel.Transpose);
				if (index >= Tables.Periods.Length)
					index = (byte)(Tables.Periods.Length - 1);

				channel.FinalPeriod = Tables.Periods[index];
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will change the speed on the module
		/// </summary>
		/********************************************************************/
		private void ChangeSpeed(sbyte newSpeed)
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
				sb.Append(channels[i].TrackLength);
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
				sb.Append(channels[i].CurrentTrackPosition);
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
				int index = channels[i].CurrentTrackPosition;
				if (index < 0)
					index = 0;

				if (index < channels[i].Track.Length)
					sb.Append(channels[i].Track[index].BlockNumber);
				else
					sb.Append("-");

				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
		#endregion
	}
}
