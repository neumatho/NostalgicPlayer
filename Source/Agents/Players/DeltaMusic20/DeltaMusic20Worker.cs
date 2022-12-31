/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Agent.Player.DeltaMusic20.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.DeltaMusic20
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class DeltaMusic20Worker : ModulePlayerAgentBase
	{
		private sbyte[][] arpeggios;
		private Instrument[] instruments;

		private TrackInfo[] tracks;
		private BlockLine[][] blocks;

		private sbyte[][] waveforms;

		private ChannelInfo[] channels;

		private int positionTrackIndex;

		private uint lastNoiseValue;
		private byte globalVolume;
		private sbyte playSpeed;
		private sbyte tick;

		private const int InfoSpeedLine = 3;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "dm2" };



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
				// Song length
				case 0:
				{
					description = Resources.IDS_DM2_INFODESCLINE0;
					value = SongLength.ToString();
					break;
				}

				// Used blocks
				case 1:
				{
					description = Resources.IDS_DM2_INFODESCLINE1;
					value = blocks.Length.ToString();
					break;
				}

				// Supported / used samples
				case 2:
				{
					description = Resources.IDS_DM2_INFODESCLINE2;
					int index = Array.IndexOf(instruments, null);
					if (index < 0)
						index = instruments.Length;

					value = index.ToString();
					break;
				}

				// Current speed
				case 3:
				{
					description = Resources.IDS_DM2_INFODESCLINE3;
					value = playSpeed.ToString();
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
		public override ModulePlayerSupportFlag SupportFlags => ModulePlayerSupportFlag.SetPosition;



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

					moduleStream.Read(inst.Table, 0, 48);

					if (moduleStream.EndOfStream)
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
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer(out string errorMessage)
		{
			int max = 0;

			for (int i = 0; i < 4; i++)
			{
				if (tracks[i].Track.Length > max)
				{
					positionTrackIndex = i;
					max = tracks[i].Track.Length;
				}
			}

			return base.InitPlayer(out errorMessage);
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
		public override bool InitSound(int songNumber, DurationInfo durationInfo, out string errorMessage)
		{
			if (!base.InitSound(songNumber, durationInfo, out errorMessage))
				return false;

			InitializeSound(durationInfo.StartPosition);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the duration for all sub-songs
		/// </summary>
		/********************************************************************/
		public override DurationInfo[] CalculateDuration()
		{
			return CalculateDurationBySongPosition();
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			GenerateNoiseWaveform();

			tick--;
			if (tick < 0)
				tick = playSpeed;

			for (int i = 0; i < 4; i++)
				ProcessChannel(i, channels[i]);
		}



		/********************************************************************/
		/// <summary>
		/// Return the length of the current song
		/// </summary>
		/********************************************************************/
		public override int SongLength => tracks[positionTrackIndex].Track.Length;



		/********************************************************************/
		/// <summary>
		/// Return the current position of the song
		/// </summary>
		/********************************************************************/
		public override int GetSongPosition()
		{
			return channels[positionTrackIndex].CurrentTrackPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Set a new position of the song
		/// </summary>
		/********************************************************************/
		public override void SetSongPosition(int position, PositionInfo positionInfo)
		{
			// Change the position
			for (int i = 0; i < 4; i++)
			{
				ChannelInfo channel = channels[i];

				channel.NextTrackPosition = ((ushort[])positionInfo.ExtraInfo)[i];
				channel.BlockPosition = 0;
				channel.PositionChangedByUser = true;
			}

			tick = 0;

			// Change the speed
			ChangeSpeed((sbyte)positionInfo.Speed);

			base.SetSongPosition(position, positionInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Returns all the samples available in the module. If none, null
		/// is returned
		/// </summary>
		/********************************************************************/
		public override SampleInfo[] Samples
		{
			get
			{
				List<SampleInfo> result = new List<SampleInfo>();

				foreach (Instrument inst in instruments)
				{
					if (inst == null)
						break;

					// Build frequency table
					uint[] frequencies = new uint[10 * 12];

					for (int j = 1; j < 7 * 12; j++)
						frequencies[j - 1] = 3546895U / Tables.Periods[j];

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = string.Empty,
						BitSize = 8,
						MiddleC = frequencies[3 * 12],
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
							sampleInfo.Flags = SampleInfo.SampleFlags.Loop;
							sampleInfo.LoopStart = inst.RepeatStart;
							sampleInfo.LoopLength = inst.RepeatLength;
						}
						else
						{
							// No loop
							sampleInfo.Flags = SampleInfo.SampleFlags.None;
							sampleInfo.LoopStart = 0;
							sampleInfo.LoopLength = 0;
						}
					}
					else
					{
						sampleInfo.Type = SampleInfo.SampleType.Synth;
						sampleInfo.Flags = SampleInfo.SampleFlags.None;
						sampleInfo.LoopStart = 0;
						sampleInfo.LoopLength = 0;
					}

					result.Add(sampleInfo);
				}

				return result.ToArray();
			}
		}
		#endregion

		#region Duration calculation methods
		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected override int InitDurationCalculationByStartPos(int startPosition)
		{
			InitializeSound(startPosition);

			return startPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current speed
		/// </summary>
		/********************************************************************/
		protected override byte GetCurrentSpeed()
		{
			return (byte)playSpeed;
		}



		/********************************************************************/
		/// <summary>
		/// Return extra information for the current position
		/// </summary>
		/********************************************************************/
		protected override object GetExtraPositionInfo()
		{
			return new[] { (ushort)(channels[0].NextTrackPosition - 1), (ushort)(channels[1].NextTrackPosition - 1), (ushort)(channels[2].NextTrackPosition - 1), (ushort)(channels[3].NextTrackPosition - 1) };
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int startPosition)		// Currently, only start positions of 0 is supported, but it shouldn't have any problems, since no modules has sub-tunes anyway
		{
			// Initialize work variables
			globalVolume = 63;
			playSpeed = 6;
			tick = 1;

			// Initialize channel structure
			channels = new ChannelInfo[4];

			for (int i = 0; i < 4; i++)
			{
				ChannelInfo channelInfo = new ChannelInfo
				{
					Hardware = VirtualChannels[i],
					Track = tracks[i].Track,
					TrackLoopPosition = tracks[i].LoopPosition,
					TrackLength = (ushort)tracks[i].Track.Length,
					BlockPosition = 0,
					CurrentTrackPosition = 0,
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

				channels[i] = channelInfo;
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
		}



		/********************************************************************/
		/// <summary>
		/// Noise generator
		/// </summary>
		/********************************************************************/
		private void GenerateNoiseWaveform()
		{
			uint noiseValue = lastNoiseValue;
			Span<uint> waveform = MemoryMarshal.Cast<sbyte, uint>(waveforms[0]);

			for (int i = 0; i < 16; i++)
			{
				noiseValue = BitOperations.RotateLeft(noiseValue, 7);
				noiseValue += 0x6eca756d;
				noiseValue ^= 0x9e59a92b;
				waveform[i] = noiseValue;
			}

			lastNoiseValue = noiseValue;
		}



		/********************************************************************/
		/// <summary>
		/// Process a single channel
		/// </summary>
		/********************************************************************/
		private void ProcessChannel(int channelNumber, ChannelInfo channel)
		{
			if (channel.TrackLength == 0)
				return;

			Instrument inst = channel.Instrument;

			if (tick == 0)
			{
				if (channel.BlockPosition == 0)
				{
					Track track = channel.Track[channel.NextTrackPosition];
					channel.Transpose = track.Transpose;
					channel.Block = blocks[track.BlockNumber];

					channel.CurrentTrackPosition = channel.NextTrackPosition;

					channel.NextTrackPosition++;
					if (channel.NextTrackPosition >= channel.TrackLength)
						channel.NextTrackPosition = channel.TrackLoopPosition >= channel.TrackLength ? (ushort)0 : channel.TrackLoopPosition;

					if (channel.PositionChangedByUser)
						channel.PositionChangedByUser = false;
					else
					{
						if (channelNumber == positionTrackIndex)
						{
							if (HasPositionBeenVisited(channel.CurrentTrackPosition))
							{
								if (currentDurationInfo == null)
									playSpeed = 6;
								else
								{
									PositionInfo positionInfo = currentDurationInfo.PositionInfo[channel.CurrentTrackPosition];

									ChangeSpeed((sbyte)positionInfo.Speed);
									ChangeSubSong(positionInfo.SubSong);
								}

								// Tell NostalgicPlayer that the module has ended
								OnEndReached();
							}

							OnPositionChanged();

							MarkPositionAsVisited(channel.CurrentTrackPosition);
						}
					}
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
							channel.Hardware.PlaySample(inst.Number, inst.SampleData, 0, inst.SampleLength);

							if (inst.RepeatLength > 1)
								channel.Hardware.SetLoop(inst.RepeatStart, inst.RepeatLength);
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
						channel.Hardware.Mute();
				}

				ParseEffect(channel, block.Effect, block.EffectArg);

				channel.BlockPosition++;
				channel.BlockPosition &= 0x0f;
			}

			if (inst != null)
			{
				SoundTableHandler(channel, inst);
				VibratoHandler(channel, inst);
				VolumeHandler(channel, inst);
				PortamentoHandler(channel);
				ArpeggioHandler(channel);

				channel.VibratoPeriod -= (ushort)(inst.PitchBend - channel.PitchBend);

				ushort newPeriod = (ushort)(channel.FinalPeriod + channel.VibratoPeriod);
				channel.Hardware.SetAmigaPeriod(newPeriod);

				byte newVolume = (byte)((channel.ActualVolume >> 2) & 0x3f);

				if (newVolume > channel.MaxVolume)
					newVolume = channel.MaxVolume;

				if (newVolume > globalVolume)
					newVolume = globalVolume;

				channel.Hardware.SetAmigaVolume(newVolume);
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
					globalVolume = (byte)(effectArg & 0x3f);
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
		private void SoundTableHandler(ChannelInfo channel, Instrument inst)
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
							channel.Hardware.PlaySample(inst.Number, waveforms[data], 0, inst.SampleLength);
						else
							channel.Hardware.Mute();

						channel.RetriggerSound = false;
					}

					if (inst.SampleLength > 0)
						channel.Hardware.SetLoop(waveforms[data], 0, inst.SampleLength);

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
			if (newSpeed != playSpeed)
			{
				// Change the module info
				OnModuleInfoChanged(InfoSpeedLine, newSpeed.ToString());

				// Remember the speed
				playSpeed = newSpeed;
			}
		}
		#endregion
	}
}
