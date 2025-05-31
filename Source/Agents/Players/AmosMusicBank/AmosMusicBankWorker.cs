/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.AmosMusicBank.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.AmosMusicBank
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class AmosMusicBankWorker : ModulePlayerWithSubSongDurationAgentBase
	{
		private Sample[] samples;
		private ushort[][] tracks;
		private SongInfo[] songInfo;

		private ushort tempoBase;	// 100 = PAL, 120 = NTSC

		private GlobalPlayingInfo playingInfo;

		private const int InfoPositionLine = 3;
		private const int InfoTrackLine = 4;
		private const int InfoSpeedLine = 5;

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "abk" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 36)
				return AgentResult.Unknown;

			// Check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);

			if (moduleStream.Read_B_UINT32() != 0x416d426b)		// AmBk
				return AgentResult.Unknown;

			// Check the bank number. Normally, this will be 3, but
			// Millennium.abk has a value of 5. According to the AMOS
			// manual, only 1-4 are reserved for specific things. All
			// numbers from 5 and up can be used for anything
			ushort type = moduleStream.Read_B_UINT16();
			if ((type != 3) && (type < 5))
				return AgentResult.Unknown;

			// Check the identifier string
			moduleStream.Seek(12, SeekOrigin.Begin);

			if ((moduleStream.Read_B_UINT32() != 0x4d757369) || (moduleStream.Read_B_UINT32() != 0x63202020))	// "Music   "
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}
		#endregion

		#region Loading
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

				// Read the different offsets
				moduleStream.Seek(20, SeekOrigin.Begin);

				uint sampleInfoOffset = moduleStream.Read_B_UINT32() + 20;
				uint songDataOffset = moduleStream.Read_B_UINT32() + 20;
				uint trackOffset = moduleStream.Read_B_UINT32() + 20;

				if (!LoadSamples(moduleStream, sampleInfoOffset))
				{
					errorMessage = Resources.IDS_ABK_ERR_LOADING_SAMPLES;
					return AgentResult.Error;
				}

				if (!LoadTracks(moduleStream, trackOffset, out ushort[][] patternTracks))
				{
					errorMessage = Resources.IDS_ABK_ERR_LOADING_TRACKS;
					return AgentResult.Error;
				}

				if (!LoadSubSongsAndSequences(moduleStream, songDataOffset, patternTracks))
				{
					errorMessage = Resources.IDS_ABK_ERR_LOADING_SEQUENCES;
					return AgentResult.Error;
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
		#endregion

		#region Initialization and cleanup
		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer(out string errorMessage)
		{
			if (!base.InitPlayer(out errorMessage))
				return false;

			tempoBase = 100;

			return true;
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

			InitializeSound(songNumber);

			return true;
		}
		#endregion

		#region Playing
		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			// Here is a smart counter, which gives progessive results
			// from zero to 100 (PAL), 120 (NTSC)...
			playingInfo.MuCpt += playingInfo.MuTempo;

			if (playingInfo.MuCpt >= tempoBase)
			{
				playingInfo.MuCpt -= tempoBase;

				// Lets go for one step of music
				for (int i = 0; i < 4; i++)
				{
					VoiceInfo voiceInfo = playingInfo.VoiceInfo[i];

					if (voiceInfo.VoiCpt != 0)
					{
						voiceInfo.VoiCpt--;

						if (voiceInfo.VoiCpt == 0)
							MuStep(voiceInfo, VirtualChannels[i]);
					}
				}
			}
			else
				DoEffects();
		}
		#endregion

		#region Information
		/********************************************************************/
		/// <summary>
		/// Return the name of the module
		/// </summary>
		/********************************************************************/
		public override string ModuleName => songInfo[0].Name;



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs => new SubSongInfo(songInfo.Length, 0);



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

				for (int i = 0; i < 3 * 12; i++)
					frequencies[4 * 12 + i] = PeriodToFrequency(Tables.Periods[i]);

				foreach (Sample sample in samples)
				{
					SampleInfo sampleInfo = new SampleInfo
					{
						Name = sample.Name,
						Flags = SampleInfo.SampleFlag.None,
						Type = SampleInfo.SampleType.Sample,
						Volume = (ushort)(sample.Volume * 4),
						Panning = -1,
						Sample = sample.SampleData,
						Length = (uint)sample.SampleData.Length,
						NoteFrequencies = frequencies
					};

					if (sample.LoopLength == 0)
					{
						// No loop
						sampleInfo.LoopStart = 0;
						sampleInfo.LoopLength = 0;
					}
					else
					{
						// Sample loops
						sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
						sampleInfo.LoopStart = sample.LoopStart;
						sampleInfo.LoopLength = sample.LoopLength;
					}

					yield return sampleInfo;
				}
			}
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
					description = Resources.IDS_ABK_INFODESCLINE0;
					value = FormatPositionLengths();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_ABK_INFODESCLINE1;
					value = tracks.Length.ToString();
					break;
				}

				// Used samples
				case 2:
				{
					description = Resources.IDS_ABK_INFODESCLINE2;
					value = samples.Length.ToString();
					break;
				}

				// Playing positions
				case 3:
				{
					description = Resources.IDS_ABK_INFODESCLINE3;
					value = FormatPositions();
					break;
				}

				// Playing tracks
				case 4:
				{
					description = Resources.IDS_ABK_INFODESCLINE4;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 5:
				{
					description = Resources.IDS_ABK_INFODESCLINE5;
					value = playingInfo.MuTempo.ToString();
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

		#region Duration calculation
		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected override void InitDuration(int subSong)
		{
			InitializeSound(subSong);
			MarkPositionAsVisited(0);
		}



		/********************************************************************/
		/// <summary>
		/// Return the total number of positions. You only need to derive
		/// from this method, if your player have one position list for all
		/// channels and can restart on another position than 0
		/// </summary>
		/********************************************************************/
		protected override int GetTotalNumberOfPositions()
		{
			return playingInfo.VoiceInfo[0].VoiPat.TrackNumbers.Length;
		}



		/********************************************************************/
		/// <summary>
		/// Create a snapshot of all the internal structures and return it
		/// </summary>
		/********************************************************************/
		protected override ISnapshot CreateSnapshot()
		{
			return new Snapshot(playingInfo);
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
			Snapshot clonedSnapshot = new Snapshot(currentSnapshot.PlayingInfo);

			playingInfo = clonedSnapshot.PlayingInfo;

			UpdateModuleInformation();

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Load all the samples
		/// </summary>
		/********************************************************************/
		private bool LoadSamples(ModuleStream moduleStream, uint sampleInfoOffset)
		{
			Encoding encoding = EncoderCollection.Amiga;
			byte[] name = new byte[16];

			// Seek to the start of the sample info block
			moduleStream.Seek(sampleInfoOffset, SeekOrigin.Begin);

			ushort numberOfSamples = moduleStream.Read_B_UINT16();
			sampleInfoOffset += 2;

			samples = new Sample[numberOfSamples];

			for (int i = 0; i < numberOfSamples; i++)
			{
				uint startPosition = moduleStream.Read_B_UINT32();
				uint loopPosition = moduleStream.Read_B_UINT32();
				ushort nonLoopLength = (ushort)(moduleStream.Read_B_UINT16() * 2);
				ushort loopLength = (ushort)(moduleStream.Read_B_UINT16() * 2);
				ushort volume = moduleStream.Read_B_UINT16();
				ushort length = (ushort)(moduleStream.Read_B_UINT16() * 2);
				int bytesRead = moduleStream.Read(name, 0, 16);

				if (bytesRead < 16)
					return false;

				if (volume > 64)
					volume = 64;

				if (length <= 4)
					length = nonLoopLength;

				Sample sample = new Sample
				{
					Name = encoding.GetString(name),
					Length = length,
					Volume = volume
				};

				if ((loopPosition - startPosition) > length)
					loopLength = 0;

				if (loopLength > 4)
				{
					if ((loopPosition - startPosition + loopLength) > length)
						loopLength = (ushort)(length - (loopPosition - startPosition));

					// For this to make sense, it should be doubled up.
					// Then it match with the other values, but as I can see
					// in the original player, it just adds the loop position
					// without any modifications to the start of instruments
					// and write that to the hardware registers. The same with
					// "nonLoopLength". It could be a bug in the player, but I
					// do the same here so this player behave the same way
					sample.LoopStart = loopPosition - startPosition;
					sample.LoopLength = loopLength;

					// The original player starts to play the sample part before
					// the loop and then sets up the loop afterwards. We simulate
					// this by calculate the sample length to be "before loop" + "loop length"
					sample.Length = sample.LoopStart + sample.LoopLength;
				}
				else
				{
					sample.LoopStart = 0;
					sample.LoopLength = 0;
				}

				// Load sample data
				long currentPosition = moduleStream.Position;

				moduleStream.Seek(sampleInfoOffset + startPosition, SeekOrigin.Begin);
				sample.SampleData = moduleStream.ReadSampleData(i, length, out int readBytes);

				if (readBytes != length)
					return false;

				moduleStream.Position = currentPosition;

				samples[i] = sample;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the tracks
		/// </summary>
		/********************************************************************/
		private bool LoadTracks(ModuleStream moduleStream, uint trackOffset, out ushort[][] patternTracks)
		{
			// Seek to the start of the tracks block
			moduleStream.Seek(trackOffset, SeekOrigin.Begin);

			// The module format contains pattern holding 4 tracks. Each channel
			// contain a separate position list pointing to the pattern.
			// Because a pattern contains a track for each channel, pattern 0
			// is different for channel 1 than channel 2. To make it easier
			// for us, we combine all the tracks into a big list and then
			// the position lists will point on tracks instead of patterns
			ushort numberOfPatterns = moduleStream.Read_B_UINT16();

			Dictionary<ushort, ushort> takenTracks = new Dictionary<ushort, ushort>();
			patternTracks = new ushort[numberOfPatterns][];

			for (int i = 0; i < numberOfPatterns; i++)
			{
				ushort[] trackNumbers = new ushort[4];

				for (int j = 0; j < 4; j++)
				{
					ushort offset = moduleStream.Read_B_UINT16();

					if (moduleStream.EndOfStream)
						return false;

					if (!takenTracks.TryGetValue(offset, out ushort trackNumber))
					{
						trackNumber = (ushort)takenTracks.Count;
						takenTracks[offset] = trackNumber;
					}

					trackNumbers[j] = trackNumber;
				}

				patternTracks[i] = trackNumbers;
			}

			// Now load the tracks itself
			tracks = new ushort[takenTracks.Count][];

			foreach (KeyValuePair<ushort, ushort> pair in takenTracks)
			{
				moduleStream.Seek(trackOffset + pair.Key, SeekOrigin.Begin);

				List<ushort> trackData = new List<ushort>();

				for (;;)
				{
					ushort value = moduleStream.Read_B_UINT16();

					if (moduleStream.EndOfStream)
						return false;

					trackData.Add(value);

					value &= 0xff00;
					if ((value == 0x8000) || (value == 0x9100))
						break;
				}

				tracks[pair.Value] = trackData.ToArray();
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sub-songs including sequences
		/// </summary>
		/********************************************************************/
		private bool LoadSubSongsAndSequences(ModuleStream moduleStream, uint songDataOffset, ushort[][] patternTracks)
		{
			Encoding encoding = EncoderCollection.Amiga;
			byte[] name = new byte[16];

			// Seek to the start of the song data block
			moduleStream.Seek(songDataOffset, SeekOrigin.Begin);

			ushort numberOfSongs = moduleStream.Read_B_UINT16();
			songInfo = new SongInfo[numberOfSongs];

			uint[] songOffsets = new uint[numberOfSongs];
			moduleStream.ReadArray_B_UINT32s(songOffsets, 0, numberOfSongs);

			if (moduleStream.EndOfStream)
				return false;

			ushort[] positionListOffsets = new ushort[4];

			for (int i = 0; i < numberOfSongs; i++)
			{
				long songOffset = songDataOffset + songOffsets[i];
				moduleStream.Seek(songOffset, SeekOrigin.Begin);

				positionListOffsets[0] = moduleStream.Read_B_UINT16();
				positionListOffsets[1] = moduleStream.Read_B_UINT16();
				positionListOffsets[2] = moduleStream.Read_B_UINT16();
				positionListOffsets[3] = moduleStream.Read_B_UINT16();

				// Skip default tempo and padding, since it is not used by the original player
				moduleStream.Seek(4, SeekOrigin.Current);
				int bytesRead = moduleStream.Read(name, 0, 16);

				if (bytesRead < 16)
					return false;

				SongInfo song = new SongInfo
				{
					Name = encoding.GetString(name),
					PositionLists = new PositionList[4]
				};

				// Load position lists
				for (int j = 0; j < 4; j++)
				{
					List<short> list = new List<short>();

					moduleStream.Seek(songOffset + positionListOffsets[j], SeekOrigin.Begin);

					for (;;)
					{
						short value = moduleStream.Read_B_INT16();

						if (moduleStream.EndOfStream)
							return false;

						if (value >= 0)
							value = (short)(value < patternTracks.Length ? patternTracks[value][j] : -3);	// -3 -> my own special value to indicate invalid pattern number

						list.Add(value);

						if ((value == -2) || (value == -1))
							break;
					}

					song.PositionLists[j] = new PositionList
					{
						TrackNumbers = list.ToArray()
					};
				}

				songInfo[i] = song;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			playingInfo = null;

			samples = null;
			tracks = null;
			songInfo = null;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int subSong)
		{
			SongInfo song = songInfo[subSong];

			playingInfo = new GlobalPlayingInfo
			{
				MuCpt = tempoBase,
				MuTempo = 17,

				VoiceInfo = new VoiceInfo[4]
			};

			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = new VoiceInfo
				{
					ChannelNumber = i,

					VoiInst = null,
					VoiInstNumber = -1,
					VoiNote = 0,
					VoiPToTo = 0,
					VoiPTone = false,
					VoiRep = 0,
					VoiDeb = -1,
					VoiDVol = 0,
					VoiVol = 0,
					VoiValue = 0,
					VoiVib = 0,

					VoiCpt = 1,
					VoiAdr = Tables.EmptyTrack,
					VoiAdrIndex = 0,
					VoiPat = song.PositionLists[i],
					VoiPatIndex = 0,
					VoiPatDIndex = 0,
					VoiEffect = NoEffect
				};

				playingInfo.VoiceInfo[i] = voiceInfo;
			}
		}



		/********************************************************************/
		/// <summary>
		/// One step of music for one voice
		/// </summary>
		/********************************************************************/
		private void MuStep(VoiceInfo voiceInfo, IChannel channel)
		{
			int voiAdr = voiceInfo.VoiAdrIndex;

			for (;;)
			{
				ushort trackValue = voiceInfo.VoiAdr[voiAdr++];
				ushort commandArgument = (ushort)(trackValue & 0x00ff);

				if ((trackValue & 0x8000) == 0x8000)
				{
					switch (trackValue & 0x7f00)
					{
						// Fin pattern
						case 0x0000:
						{
							FinPatternCommand(voiceInfo, ref voiAdr);
							break;
						}

						// Old slide up / old slide down
						case 0x0100:
						case 0x0200:
							break;

						// Set volume
						case 0x0300:
						{
							SetVolumeCommand(voiceInfo, commandArgument);
							break;
						}

						// Stop effect
						case 0x0400:
						{
							StopEffectCommand(voiceInfo);
							break;
						}

						// Repeat
						case 0x0500:
						{
							RepeatCommand(voiceInfo, commandArgument, ref voiAdr);
							break;
						}

						// Led on
						case 0x0600:
						{
							LedOnCommand();
							break;
						}

						// Led off
						case 0x0700:
						{
							LedOffCommand();
							break;
						}

						// Set tempo
						case 0x0800:
						{
							SetTempoCommand(commandArgument);
							break;
						}

						// Set instrument
						case 0x0900:
						{
							SetInstrumentCommand(voiceInfo, commandArgument);
							break;
						}

						// Arpeggio
						case 0x0a00:
						{
							ArpeggioCommand(voiceInfo, commandArgument);
							break;
						}

						// Portamento
						case 0x0b00:
						{
							PortamentoCommand(voiceInfo, commandArgument);
							break;
						}

						// Vibrato
						case 0x0c00:
						{
							VibratoCommand(voiceInfo, commandArgument);
							break;
						}

						// Volume slide
						case 0x0d00:
						{
							VolumeSlideCommand(voiceInfo, commandArgument);
							break;
						}

						// Slide up
						case 0x0e00:
						{
							SlideUpCommand(voiceInfo, commandArgument);
							break;
						}

						// Slide down
						case 0x0f00:
						{
							SlideDownCommand(voiceInfo, commandArgument);
							break;
						}

						// Delay
						case 0x1000:
						{
							DelayCommand(voiceInfo, commandArgument, voiAdr);
							return;
						}

						// Position jump
						case 0x1100:
						{
							PositionJumpCommand(voiceInfo, commandArgument, ref voiAdr);

							voiceInfo.VoiAdrIndex = voiAdr;
							return;
						}
					}
				}
				else
				{
					// Play a note
					Sample sample = voiceInfo.VoiInst;

					if ((trackValue & 0x4000) == 0x4000)
					{
						// Play note compatible with first version
						voiceInfo.VoiCpt = (ushort)(trackValue & 0x00ff);

						trackValue = voiceInfo.VoiAdr[voiAdr++];
						if (trackValue != 0)
						{
							trackValue &= 0x0fff;

							voiceInfo.VoiNote = trackValue;
							channel.SetAmigaPeriod(trackValue);

							if (sample != null)
							{
								channel.PlaySample(voiceInfo.VoiInstNumber, sample.SampleData, 0, sample.Length);

								if (sample.LoopLength != 0)
									channel.SetLoop(sample.LoopStart, sample.LoopLength);
							}
						}

						voiceInfo.VoiAdrIndex = voiAdr;
						break;
					}

					// Play normal note
					trackValue &= 0x0fff;

					if (sample != null)
					{
						channel.PlaySample(voiceInfo.VoiInstNumber, sample.SampleData, 0, sample.Length);

						if (sample.LoopLength != 0)
							channel.SetLoop(sample.LoopStart, sample.LoopLength);
					}

					// The original player does not set the volume here, but I have added it
					// so melodian.abk sounds ok
					channel.SetAmigaVolume(voiceInfo.VoiVol);

					if (voiceInfo.VoiPTone)
					{
						// Start portamento
						voiceInfo.VoiPTone = false;
						voiceInfo.VoiPToTo = trackValue;
						voiceInfo.VoiEffect = PortamentoEffect;
					}
					else
					{
						// No portamento
						voiceInfo.VoiNote = trackValue;
						channel.SetAmigaPeriod(trackValue);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Performs all effects
		/// </summary>
		/********************************************************************/
		private void DoEffects()
		{
			for (int i = 0; i < 4; i++)
			{
				VoiceInfo voiceInfo = playingInfo.VoiceInfo[i];
				IChannel channel = VirtualChannels[i];

				voiceInfo.VoiEffect(voiceInfo, channel);
				channel.SetAmigaVolume(voiceInfo.VoiVol);
			}
		}

		#region Commands
		/********************************************************************/
		/// <summary>
		/// Command 00 - Fin pattern
		/// </summary>
		/********************************************************************/
		private void FinPatternCommand(VoiceInfo voiceInfo, ref int voiAdr)
		{
			voiceInfo.VoiCpt = 0;
			voiceInfo.VoiRep = 0;
			voiceInfo.VoiDeb = -1;
			voiceInfo.VoiEffect = NoEffect;

			int voiPat = voiceInfo.VoiPatIndex;
			bool oneMore;

			do
			{
				oneMore = false;

				short positionValue = voiceInfo.VoiPat.TrackNumbers[voiPat++];
				if (positionValue < 0)
				{
					if (positionValue == -3)
					{
						// Invalid pattern number
						voiceInfo.VoiPatIndex = voiPat;

						ShowChannelPositions();
						ShowTracks();
						return;
					}

					OnEndReached(voiceInfo.ChannelNumber);

					if (positionValue == -1)
					{
						// Do not loop position list
						return;
					}

					voiPat = voiceInfo.VoiPatDIndex;
					oneMore = true;
				}
				else
				{
					voiceInfo.VoiPatIndex = voiPat;
					voiceInfo.VoiAdr = tracks[positionValue];
					voiAdr = 0;

					if (voiceInfo.ChannelNumber == 0)
						MarkPositionAsVisited(voiPat);

					ShowChannelPositions();
					ShowTracks();
				}
			}
			while (oneMore);
		}



		/********************************************************************/
		/// <summary>
		/// Command 03 - Set volume
		/// </summary>
		/********************************************************************/
		private void SetVolumeCommand(VoiceInfo voiceInfo, ushort commandArgument)
		{
			if (commandArgument >= 64)
				commandArgument = 63;

			voiceInfo.VoiDVol = commandArgument;
			voiceInfo.VoiVol = commandArgument;
		}



		/********************************************************************/
		/// <summary>
		/// Command 04 - Stop effect
		/// </summary>
		/********************************************************************/
		private void StopEffectCommand(VoiceInfo voiceInfo)
		{
			voiceInfo.VoiEffect = NoEffect;
		}



		/********************************************************************/
		/// <summary>
		/// Command 05 - Repeat
		/// </summary>
		/********************************************************************/
		private void RepeatCommand(VoiceInfo voiceInfo, ushort commandArgument, ref int voiAdr)
		{
			if (commandArgument == 0)
				voiceInfo.VoiDeb = voiAdr;
			else
			{
				if (voiceInfo.VoiRep == 0)
					voiceInfo.VoiRep = commandArgument;
				else
				{
					voiceInfo.VoiRep--;

					if ((voiceInfo.VoiRep != 0) && (voiceInfo.VoiDeb != -1))
						voiAdr = voiceInfo.VoiDeb;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Command 06 - Led on
		/// </summary>
		/********************************************************************/
		private void LedOnCommand()
		{
			AmigaFilter = true;
		}



		/********************************************************************/
		/// <summary>
		/// Command 07 - Led off
		/// </summary>
		/********************************************************************/
		private void LedOffCommand()
		{
			AmigaFilter = false;
		}



		/********************************************************************/
		/// <summary>
		/// Command 08 - Set tempo
		/// </summary>
		/********************************************************************/
		private void SetTempoCommand(ushort commandArgument)
		{
			playingInfo.MuTempo = commandArgument;
			ShowSpeed();
		}



		/********************************************************************/
		/// <summary>
		/// Command 09 - Set instrument
		/// </summary>
		/********************************************************************/
		private void SetInstrumentCommand(VoiceInfo voiceInfo, ushort commandArgument)
		{
			if (commandArgument < samples.Length)
			{
				voiceInfo.VoiInst = samples[commandArgument];
				voiceInfo.VoiInstNumber = (short)commandArgument;

				ushort volume = voiceInfo.VoiInst.Volume;
				if (volume >= 64)
					volume = 63;

				voiceInfo.VoiDVol = volume;
				voiceInfo.VoiVol = volume;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Command 0A - Arpeggio
		/// </summary>
		/********************************************************************/
		private void ArpeggioCommand(VoiceInfo voiceInfo, ushort commandArgument)
		{
			voiceInfo.VoiValue = (short)commandArgument;
			voiceInfo.VoiEffect = ArpeggioEffect;
		}



		/********************************************************************/
		/// <summary>
		/// Command 0B - Portamento
		/// </summary>
		/********************************************************************/
		private void PortamentoCommand(VoiceInfo voiceInfo, ushort commandArgument)
		{
			voiceInfo.VoiPTone = true;
			voiceInfo.VoiValue = (short)commandArgument;
			voiceInfo.VoiEffect = PortamentoEffect;
		}



		/********************************************************************/
		/// <summary>
		/// Command 0C - Vibrato
		/// </summary>
		/********************************************************************/
		private void VibratoCommand(VoiceInfo voiceInfo, ushort commandArgument)
		{
			voiceInfo.VoiValue = (short)commandArgument;
			voiceInfo.VoiEffect = VibratoEffect;
		}



		/********************************************************************/
		/// <summary>
		/// Command 0D - Volume slide
		/// </summary>
		/********************************************************************/
		private void VolumeSlideCommand(VoiceInfo voiceInfo, ushort commandArgument)
		{
			short value = (short)(commandArgument >> 4);

			if (value == 0)
				value = (short)-(commandArgument & 0x0f);

			voiceInfo.VoiValue = value;
			voiceInfo.VoiEffect = VolumeSlideEffect;
		}



		/********************************************************************/
		/// <summary>
		/// Command 0E - Slide up
		/// </summary>
		/********************************************************************/
		private void SlideUpCommand(VoiceInfo voiceInfo, ushort commandArgument)
		{
			voiceInfo.VoiValue = (short)-commandArgument;
			voiceInfo.VoiEffect = SlideEffect;
		}



		/********************************************************************/
		/// <summary>
		/// Command 0F - Slide down
		/// </summary>
		/********************************************************************/
		private void SlideDownCommand(VoiceInfo voiceInfo, ushort commandArgument)
		{
			voiceInfo.VoiValue = (short)commandArgument;
			voiceInfo.VoiEffect = SlideEffect;
		}



		/********************************************************************/
		/// <summary>
		/// Command 10 - Delay
		/// </summary>
		/********************************************************************/
		private void DelayCommand(VoiceInfo voiceInfo, ushort commandArgument, int voiAdr)
		{
			voiceInfo.VoiCpt = commandArgument;
			voiceInfo.VoiAdrIndex = voiAdr;
		}



		/********************************************************************/
		/// <summary>
		/// Command 11 - Position jump
		/// </summary>
		/********************************************************************/
		private void PositionJumpCommand(VoiceInfo voiceInfo, ushort commandArgument, ref int voiAdr)
		{
			int newPosition = voiceInfo.VoiPatDIndex + commandArgument;
			if (newPosition < voiceInfo.VoiPatIndex)
			{
				OnEndReached(voiceInfo.ChannelNumber);
				SetPositionTime(newPosition + 1);		// Need to increment by one, because when the module starts, it plays the "empty pattern", which will store an extra position
			}

			voiceInfo.VoiPatIndex = newPosition;
			FinPatternCommand(voiceInfo, ref voiAdr);

			voiceInfo.VoiCpt = 1;
		}
		#endregion

		#region Effect handlers
		/********************************************************************/
		/// <summary>
		/// Effect handler: No effect
		/// </summary>
		/********************************************************************/
		private void NoEffect(VoiceInfo voiceInfo, IChannel channel)
		{
			channel.SetAmigaPeriod(voiceInfo.VoiNote);
		}



		/********************************************************************/
		/// <summary>
		/// Effect handler: Arpeggio
		/// </summary>
		/********************************************************************/
		private void ArpeggioEffect(VoiceInfo voiceInfo, IChannel channel)
		{
			byte val1 = (byte)(voiceInfo.VoiValue & 0x00ff);
			byte val2 = (byte)(voiceInfo.VoiValue >> 8);

			if (val2 >= 3)
				val2 = 2;

			val2--;
			voiceInfo.VoiValue = (short)((val2 << 8) | val1);

			ushort newPeriod;

			if (val2 == 0)
				newPeriod = voiceInfo.VoiNote;
			else if ((sbyte)val2 < 0)
				newPeriod = FindArpeggioPeriod(voiceInfo.VoiNote, val1 >> 4);
			else
				newPeriod = FindArpeggioPeriod(voiceInfo.VoiNote, val1 & 0x0f);

			channel.SetAmigaPeriod(newPeriod);
		}



		/********************************************************************/
		/// <summary>
		/// Find arpeggio note
		/// </summary>
		/********************************************************************/
		private ushort FindArpeggioPeriod(ushort currentNote, int arpValue)
		{
			for (int i = 0; i < Tables.Periods.Length; i++)
			{
				ushort period = Tables.Periods[i + arpValue];

				if (currentNote >= Tables.Periods[i])
					return period;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect handler: Portamento
		/// </summary>
		/********************************************************************/
		private void PortamentoEffect(VoiceInfo voiceInfo, IChannel channel)
		{
			ushort newPeriod = voiceInfo.VoiNote;

			if (newPeriod == voiceInfo.VoiPToTo)
				voiceInfo.VoiEffect = NoEffect;
			else if (newPeriod < voiceInfo.VoiPToTo)
			{
				newPeriod = (ushort)(newPeriod + voiceInfo.VoiValue);
				if (newPeriod >= voiceInfo.VoiPToTo)
				{
					newPeriod = voiceInfo.VoiPToTo;
					voiceInfo.VoiEffect = NoEffect;
				}
			}
			else
			{
				newPeriod = (ushort)(newPeriod - voiceInfo.VoiValue);
				if (newPeriod <= voiceInfo.VoiPToTo)
				{
					newPeriod = voiceInfo.VoiPToTo;
					voiceInfo.VoiEffect = NoEffect;
				}
			}

			voiceInfo.VoiNote = newPeriod;
			channel.SetAmigaPeriod(newPeriod);
		}



		/********************************************************************/
		/// <summary>
		/// Effect handler: Vibrato
		/// </summary>
		/********************************************************************/
		private void VibratoEffect(VoiceInfo voiceInfo, IChannel channel)
		{
			int vibIndex = (voiceInfo.VoiVib / 4) & 0x1f;
			byte vibValue = Tables.Sinus[vibIndex];
			ushort speed = (ushort)((voiceInfo.VoiValue & 0x0f) * vibValue / 64);

			ushort newPeriod = voiceInfo.VoiNote;

			if (voiceInfo.VoiVib < 0)
				newPeriod -= speed;
			else
				newPeriod += speed;

			channel.SetAmigaPeriod(newPeriod);

			voiceInfo.VoiVib += (sbyte)((voiceInfo.VoiValue / 4) & 0x3c);
		}



		/********************************************************************/
		/// <summary>
		/// Effect handler: Volume slide
		/// </summary>
		/********************************************************************/
		private void VolumeSlideEffect(VoiceInfo voiceInfo, IChannel channel)
		{
			short newVolume = (short)(voiceInfo.VoiDVol + voiceInfo.VoiValue);

			if (newVolume < 0)
				newVolume = 0;
			else if (newVolume >= 64)
				newVolume = 63;

			voiceInfo.VoiDVol = (ushort)newVolume;
			voiceInfo.VoiVol = (ushort)newVolume;
		}



		/********************************************************************/
		/// <summary>
		/// Effect handler: Tone slide
		/// </summary>
		/********************************************************************/
		private void SlideEffect(VoiceInfo voiceInfo, IChannel channel)
		{
			ushort newPeriod = voiceInfo.VoiNote;

			if (voiceInfo.VoiValue == 0)
				voiceInfo.VoiEffect = NoEffect;
			else
			{
				newPeriod = (ushort)(newPeriod + voiceInfo.VoiValue);

				if (newPeriod < 113)
				{
					newPeriod = 113;
					voiceInfo.VoiEffect = NoEffect;
				}
				else if (newPeriod >= 856)
				{
					newPeriod = 856;
					voiceInfo.VoiEffect = NoEffect;
				}

				voiceInfo.VoiNote = newPeriod;
				channel.SetAmigaPeriod(newPeriod);
			}
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Will update the module information with current channel positions
		/// </summary>
		/********************************************************************/
		private void ShowChannelPositions()
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
			OnModuleInfoChanged(InfoSpeedLine, playingInfo.MuTempo.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with all dynamic values
		/// </summary>
		/********************************************************************/
		private void UpdateModuleInformation()
		{
			ShowChannelPositions();
			ShowTracks();
			ShowSpeed();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the songs position lengths
		/// </summary>
		/********************************************************************/
		private string FormatPositionLengths()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < 4; i++)
			{
				sb.Append(playingInfo.VoiceInfo[i].VoiPat.TrackNumbers.Length - 1);
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
				int index = playingInfo.VoiceInfo[i].VoiPatIndex - 1;
				sb.Append(index < 0 ? 0 : index);
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
				VoiceInfo voiceInfo = playingInfo.VoiceInfo[i];

				int index = voiceInfo.VoiPatIndex - 1;
				sb.Append(voiceInfo.VoiPat.TrackNumbers[index < 0 ? 0 : index]);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
		#endregion
	}
}
