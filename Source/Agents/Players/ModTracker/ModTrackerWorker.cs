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
using Polycode.NostalgicPlayer.Agent.Player.ModTracker.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Extensions;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class ModTrackerWorker : ModulePlayerWithPositionDurationAgentBase
	{
		private const int NumberOfNotes = 3 * 12;

		private const int MinPeriod = 113;
		private const int MaxPeriod = 856;

		private readonly ModuleType currentModuleType;
		private bool packed;
		private bool showTracks;

		private string songName;
		private ushort maxPattern;
		private ushort channelNum;
		private ushort sampleNum;
		private ushort songLength;
		private ushort trackNum;
		private ushort patternLength;
		private ushort restartPos;
		private byte initTempo;
		private byte globalVolume;

		private byte[] positions;

		private Sample[] samples;
		private TrackLine[][] tracks;
		private ushort[,] sequences;

		private AmSample[] amData;
		private HmnSynthData[] hmnSynthData;

		private GlobalPlayingInfo playingInfo;
		private ModChannel[] channels;

		private bool endReached;
		private bool restartSong;

		private const int InfoPositionLine = 3;
		private const int InfoPatternLine = 4;
		private const int InfoSpeedLine = 5;
		private const int InfoTempoLine = 6;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModTrackerWorker(ModuleType moduleType = ModuleType.Unknown)
		{
			currentModuleType = moduleType;
		}

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => ModTrackerIdentifier.FileExtensions;



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Return some extra information about the format. If it returns
		/// null or an empty string, nothing extra is shown
		/// </summary>
		/********************************************************************/
		public override string ExtraFormatInfo
		{
			get
			{
				if (!packed)
					return null;

				return Resources.IDS_MOD_PACKED;
			}
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
			// Load the module
			packed = false;
			showTracks = false;

			if ((currentModuleType == ModuleType.SoundTracker26) || (currentModuleType == ModuleType.IceTracker))
				return LoadSoundTracker26(fileInfo, out errorMessage);

			if (currentModuleType == ModuleType.ProTrackerIff)
				return LoadProTrackerIff(fileInfo, out errorMessage);

			return LoadTracker(fileInfo, out errorMessage);
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

			// Initialize structures
			channels = new ModChannel[channelNum];

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

			InitializeSound(0);

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
			byte currentSpeed = (playingInfo.PatternPos & 1) == 0 ? playingInfo.SpeedEven : playingInfo.SpeedOdd;
			ChangeSpeed(currentSpeed);

			if (currentSpeed != 0)				// Only play if speed <> 0
			{
				playingInfo.Counter++;				// Count speed counter
				if (playingInfo.Counter >= currentSpeed)   // Do we have to change pattern line?
				{
					playingInfo.Counter = 0;

					if (playingInfo.PattDelayTime2 != 0)	// Pattern delay active
						NoNewAllChannels();
					else
						GetNewNote();

					// Get next pattern line
					playingInfo.PatternPos++;

					if (playingInfo.PattDelayTime != 0)   // New pattern delay time
					{
						// Activate the pattern delay
						playingInfo.PattDelayTime2 = playingInfo.PattDelayTime;
						playingInfo.PattDelayTime = 0;
					}

					// Pattern delay routine, jump one line back again
					if (playingInfo.PattDelayTime2 != 0)
					{
						if (--playingInfo.PattDelayTime2 != 0)
							playingInfo.PatternPos--;
					}

					// Pattern loop?
					if (playingInfo.BreakFlag)
					{
						playingInfo.BreakFlag = false;
						playingInfo.PatternPos = playingInfo.BreakPos;
						playingInfo.BreakPos = 0;
					}

					// Have we played the whole pattern?
					if (playingInfo.PatternPos >= patternLength)
						NextPosition();
				}
				else
					NoNewAllChannels();

				if (playingInfo.PosJumpFlag)
					NextPosition();
			}
			else
			{
				NoNewAllChannels();

				if (playingInfo.PosJumpFlag)
					NextPosition();
			}

			if (IsStarTrekker())
				StarAmHandler();

			// Set volume on all channels
			for (int i = 0; i < channelNum; i++)
			{
				IChannel chan = VirtualChannels[i];
				ModChannel modChan = channels[i];

				chan.SetVolume((ushort)(modChan.Volume * modChan.StarVolume * modChan.HmnVolume * globalVolume / 262144));
			}

			// Have we reached the end of the module
			if (endReached)
			{
				OnEndReached(playingInfo.SongPos);
				endReached = false;

				if (restartSong)
				{
					RestartSong();
					restartSong = false;
				}

				MarkPositionAsVisited(playingInfo.SongPos);
			}
		}
		#endregion

		#region Information
		/********************************************************************/
		/// <summary>
		/// Return the title
		/// </summary>
		/********************************************************************/
		public override string Title => songName;



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public override int ModuleChannelCount => channelNum;



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
				for (int i = 0, cnt = samples.Length; i < cnt; i++)
				{
					Sample sample = samples[i];

					// Build frequency table
					uint[] frequencies = new uint[10 * 12];

					if (currentModuleType == ModuleType.HisMastersNoise)
					{
						for (int j = 0; j < 3 * 12; j++)
							frequencies[4 * 12 + j] = (uint)(7093789.2 / (Tables.Periods[0, j] + (Tables.Periods[0, j] * sample.FineTuneHmn) / 256));
					}
					else
					{
						for (int j = 0; j < 3 * 12; j++)
							frequencies[4 * 12 + j] = PeriodToFrequency(Tables.Periods[sample.FineTune, j]);
					}

					yield return new SampleInfo
					{
						Name = sample.SampleName,
						Flags = sample.LoopLength <= 1 ? SampleInfo.SampleFlag.None : SampleInfo.SampleFlag.Loop,
						Type = ((amData?[i] != null) && (amData[i].Mark == 0x414d)) || ((hmnSynthData != null) && (hmnSynthData[i] != null)) ? SampleInfo.SampleType.Synthesis : SampleInfo.SampleType.Sample,
						Volume = (ushort)(sample.Volume * 4),
						Panning = -1,
						Sample = sample.Data,
						Length = (uint)sample.Length * 2,
						LoopStart = (uint)sample.LoopStart * 2,
						LoopLength = (uint)sample.LoopLength * 2,
						NoteFrequencies = frequencies
					};
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
					description = Resources.IDS_MOD_INFODESCLINE0;
					value = songLength.ToString();
					break;
				}

				// Used patterns / Used tracks
				case 1:
				{
					if (showTracks)
					{
						description = Resources.IDS_MOD_INFODESCLINE1b;
						value = trackNum.ToString();
					}
					else
					{
						description = Resources.IDS_MOD_INFODESCLINE1a;
						value = maxPattern.ToString();
					}
					break;
				}

				// Supported / used samples
				case 2:
				{
					description = Resources.IDS_MOD_INFODESCLINE2;
					value = sampleNum.ToString();
					break;
				}

				// Playing position
				case 3:
				{
					description = Resources.IDS_MOD_INFODESCLINE3;
					value = playingInfo.SongPos.ToString();
					break;
				}

				// Playing pattern / Playing tracks
				case 4:
				{
					if (showTracks)
					{
						description = Resources.IDS_MOD_INFODESCLINE4b;
						value = FormatTracks();
					}
					else
					{
						description = Resources.IDS_MOD_INFODESCLINE4a;
						value = positions[playingInfo.SongPos].ToString();
					}
					break;
				}

				// Current speed
				case 5:
				{
					description = Resources.IDS_MOD_INFODESCLINE5;
					value = FormatSpeed();
					break;
				}

				// Current tempo (BPM)
				case 6:
				{
					description = Resources.IDS_MOD_INFODESCLINE6;
					value = playingInfo.Tempo.ToString();
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
		protected override int InitDuration(int songNumber, int startPosition)
		{
			InitializeSound(startPosition);
			MarkPositionAsVisited(startPosition);

			return startPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Return the total number of positions
		/// </summary>
		/********************************************************************/
		protected override int GetTotalNumberOfPositions()
		{
			return songLength;
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
		/// Tests the current module to see if it's in sound tracker format
		/// </summary>
		/********************************************************************/
		private bool IsSoundTracker()
		{
			return currentModuleType <= ModuleType.IceTracker;
		}



		/********************************************************************/
		/// <summary>
		/// Tests the current module to see if it's in NoiseTracker or
		/// similar format
		/// </summary>
		/********************************************************************/
		private bool IsNoiseTracker()
		{
			return (currentModuleType >= ModuleType.NoiseTracker) && (currentModuleType <= ModuleType.AudioSculpture);
		}



		/********************************************************************/
		/// <summary>
		/// Tests the current module to see if it's in StarTrekker or
		/// similar format
		/// </summary>
		/********************************************************************/
		private bool IsStarTrekker()
		{
			return (currentModuleType >= ModuleType.StarTrekker) && (currentModuleType <= ModuleType.AudioSculpture);
		}



		/********************************************************************/
		/// <summary>
		/// Tests the current module to see if it's in ProTracker or
		/// similar format
		/// </summary>
		/********************************************************************/
		private bool IsProTracker()
		{
			return currentModuleType >= ModuleType.ProTracker;
		}

		#region Loaders

		#region Tracker loader
		/********************************************************************/
		/// <summary>
		/// Will load a tracker module into memory
		/// </summary>
		/********************************************************************/
		private AgentResult LoadTracker(PlayerFileInfo fileInfo, out string errorMessage)
		{
			try
			{
				byte[] buf = new byte[23];

				ModuleStream moduleStream = fileInfo.ModuleStream;
				Encoding encoder = EncoderCollection.Amiga;

				// This is only used for His Master's Noise and contains patterns
				// that holds synth wave forms instead of real pattern data.
				// Therefore this list holds the patterns that should not be loaded
				// normally
				HashSet<int> skipPatterns = new HashSet<int>();

				// Find out the number of samples
				sampleNum = (ushort)((currentModuleType <= ModuleType.SoundTracker2x) ? 15 : 31);

				// Read the song name
				buf[20] = 0x00;
				moduleStream.ReadInto(buf, 0, 20);

				songName = encoder.GetString(buf);

				// Allocate space to the samples
				samples = new Sample[sampleNum];

				// Read the samples
				for (int i = 0; i < sampleNum; i++)
				{
					Sample sample = samples[i] = new Sample();

					// Read the sample info
					buf[22] = 0x00;
					moduleStream.ReadInto(buf, 0, 22);				// Name of the sample

					ushort length = moduleStream.Read_B_UINT16();			// Length in words
					byte fineTune = moduleStream.Read_UINT8();				// Only the low nibble is used (mask it out and extend the sign)
					byte volume = moduleStream.Read_UINT8();				// The volume
					ushort repeatStart = moduleStream.Read_B_UINT16();		// Number of words from the beginning where the loop starts
					ushort repeatLength = moduleStream.Read_B_UINT16();		// The loop length in words

					if (sampleNum == 15)
					{
						// For all 15 samples modules, the repeat start is in bytes, so convert it to words
						repeatStart /= 2;
					}

					// If repeat length is 1, it is the same as no loop, so reset it
					if (repeatLength == 1)
						repeatLength = 0;

					// Correct "funny" modules
					if (repeatStart > length)
					{
						repeatStart = 0;
						repeatLength = 0;
					}

					if ((repeatStart + repeatLength) > length)
						repeatLength = (ushort)(length - repeatStart);

					// Check for synth sounds
					if (currentModuleType == ModuleType.HisMastersNoise)
					{
						// If sample name starts with 'Mupp', it is a synth sample
						// and the name contains extra information
						if ((buf[0] == 0x4d) && (buf[1] == 0x75) && (buf[2] == 0x70) && (buf[3] == 0x70))
						{
							// Make sure we have a place to store the data
							if (hmnSynthData == null)
								hmnSynthData = new HmnSynthData[31];

							HmnSynthData synthData = hmnSynthData[i] = new HmnSynthData();

							synthData.PatternNumber = buf[4];		// Wave form data are stored in a pattern
							synthData.DataLoopStart = buf[5];
							synthData.DataLoopEnd = buf[6];

							// Clear the rest of the sample name
							buf[4] = 0x00;

							// Make the pattern to be skipped
							skipPatterns.Add(synthData.PatternNumber);
						}
					}
					else
					{
						// Do the recognized format support fine tune?
						if (IsSoundTracker() || IsNoiseTracker())
							fineTune = 0;
					}

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MOD_ERR_LOADING_SAMPLEINFO;
						Cleanup();

						return AgentResult.Error;
					}

					// Put the information into the sample structure
					sample.SampleName = encoder.GetString(buf).RemoveInvalidChars();
					sample.Data = null;
					sample.Length = length;
					sample.LoopStart = repeatStart;
					sample.LoopLength = repeatLength;
					sample.Volume = volume;

					if (currentModuleType == ModuleType.HisMastersNoise)
					{
						sample.FineTune = 0;
						sample.FineTuneHmn = (sbyte)fineTune;
					}
					else
					{
						sample.FineTune = (byte)(fineTune & 0x0f);
						sample.FineTuneHmn = 0;
					}
				}

				// Read more header information
				songLength = moduleStream.Read_UINT8();

				// Make beatwave.mod to work
				if (songLength > 128)
					songLength = 128;

				if (IsNoiseTracker())
				{
					initTempo = 125;
					restartPos = (ushort)(moduleStream.Read_UINT8() & 0x7f);

					if (restartPos >= songLength)
						restartPos = 0;
				}
				else
				{
					if (sampleNum == 15)
					{
						restartPos = 0;
						byte temp = moduleStream.Read_UINT8();

						if (temp < 100)			// 100 is just a guess. Starwars.Remix is playing too slow if used the tempo in the module
							initTempo = 125;
						else
						{
							switch (currentModuleType)
							{
								case ModuleType.UltimateSoundTracker10:
								case ModuleType.SoundTrackerII:
								case ModuleType.SoundTrackerVI:
								case ModuleType.MasterSoundTracker10:
								case ModuleType.SoundTracker2x:
								{
									initTempo = 125;
									break;
								}

								case ModuleType.UltimateSoundTracker18:
								case ModuleType.SoundTrackerIX:
								{
									if (temp != 0x78)
										initTempo = (byte)ConvertSoundTrackerBpm(temp);
									else
										initTempo = (byte)(temp * 25 / 24);			// Some modules won't play correctly if the tempo isn't converted

									break;
								}
							}
						}
					}
					else
					{
						moduleStream.Read_UINT8();
						initTempo = 125;
						restartPos = 0;
					}
				}

				positions = new byte[128];
				int bytesRead = moduleStream.Read(positions, 0, 128);

				if (bytesRead < 128)
				{
					errorMessage = Resources.IDS_MOD_ERR_LOADING_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				// All 31 samples modules have a mark
				if (sampleNum == 31)
					moduleStream.Seek(4, SeekOrigin.Current);

				// Find the missing information
				patternLength = 64;
				globalVolume = 64;

				// Find the number of channels used
				channelNum = (ushort)(currentModuleType == ModuleType.StarTrekker8 ? 8 : 4);

				// If we load a StarTrekker 8 voices module, divide all the
				// pattern numbers by 2
				if (currentModuleType == ModuleType.StarTrekker8)
				{
					for (int i = 0; i < 128; i++)
						positions[i] /= 2;
				}

				// Find the highest pattern number
				maxPattern = 0;

				for (int i = 0; i < 128; i++)
				{
					if (positions[i] > maxPattern)
						maxPattern = positions[i];
				}

				maxPattern++;
				trackNum = (ushort)(maxPattern * channelNum);

				// Allocate space for the patterns
				tracks = new TrackLine[trackNum][];

				// Read the tracks
				TrackLine[][] line = new TrackLine[channelNum][];

				long startOfPatternData = moduleStream.Position;

				for (int i = 0; i < trackNum / channelNum; i++)
				{
					if (!skipPatterns.Contains(i))
					{
						// Allocate memory to hold the tracks
						for (int j = 0; j < channelNum; j++)
							line[j] = tracks[i * channelNum + j] = new TrackLine[patternLength];

						if (currentModuleType == ModuleType.StarTrekker8)
						{
							LoadModTracks(moduleStream, line, 0, 4);
							LoadModTracks(moduleStream, line, 4, 4);
						}
						else
							LoadModTracks(moduleStream, line, 0, channelNum);

						if (moduleStream.EndOfStream)
						{
							errorMessage = Resources.IDS_MOD_ERR_LOADING_PATTERNS;
							Cleanup();

							return AgentResult.Error;
						}
					}
					else
					{
						// Just skip it
						moduleStream.Seek(4 * patternLength * channelNum, SeekOrigin.Current);
					}
				}

				// Allocate memory to hold the sequences
				sequences = new ushort[channelNum, maxPattern];

				// Calculate the sequence numbers
				for (int i = 0; i < maxPattern; i++)
				{
					for (int j = 0; j < channelNum; j++)
						sequences[j, i] = (ushort)(i * channelNum + j);
				}

				byte[] decodeTable = null;

				// Read the samples
				for (int i = 0; i < sampleNum; i++)
				{
					// Allocate space to hold the sample
					int length = samples[i].Length * 2;
					if (length != 0)
					{
						sbyte[] sampleBuffer = new sbyte[length];
						samples[i].Data = sampleBuffer;

						using (ModuleStream sampleDataStream = moduleStream.GetSampleDataStream(i, length))
						{
							// Check for Mod Plugin packed samples
							sampleDataStream.ReadInto(buf, 0, 5);

							if ((buf[0] == 0x41) && (buf[1] == 0x44) && (buf[2] == 0x50) && (buf[3] == 0x43) && (buf[4] == 0x4d))	// ADPCM
							{
								// It is, so read and depack it
								packed = true;

								// Read a 16 byte buffer with delta values
								sbyte[] compressionTable = new sbyte[16];
								sampleDataStream.ReadSigned(compressionTable, 0, 16);

								if (decodeTable == null)
									decodeTable = new byte[8192];

								sbyte adpcmDelta = 0;
								int offset = 0;
								length /= 2;

								while (length > 0)
								{
									int todo = Math.Min(decodeTable.Length, length);
									int read = sampleDataStream.Read(decodeTable, 0, todo);
									if (read != todo)
									{
										errorMessage = Resources.IDS_MOD_ERR_LOADING_SAMPLES;
										Cleanup();

										return AgentResult.Error;
									}

									for (int j = 0; j < todo; j++)
									{
										byte b = decodeTable[j];

										adpcmDelta += compressionTable[b & 0x0f];
										sampleBuffer[offset++] = adpcmDelta;
										adpcmDelta += compressionTable[(b >> 4) & 0x0f];
										sampleBuffer[offset++] = adpcmDelta;
									}

									length -= todo;
								}

								// Continue with next sample
								continue;
							}

							// It is not, so seek back and read the sample
							sampleDataStream.Seek(-5, SeekOrigin.Current);

							// Check to see if we miss too much from the last sample
							if (sampleDataStream.Length - sampleDataStream.Position < (length - 512))
							{
								errorMessage = Resources.IDS_MOD_ERR_LOADING_SAMPLES;
								Cleanup();

								return AgentResult.Error;
							}

							// Read the sample
							sampleDataStream.ReadSigned(sampleBuffer, 0, length);
						}
					}
				}

				// Read His Master's Noise synth wave forms
				if (currentModuleType == ModuleType.HisMastersNoise)
				{
					if (hmnSynthData != null)
					{
						foreach (HmnSynthData synthData in hmnSynthData)
						{
							if (synthData != null)
							{
								// Seek to the "pattern"
								moduleStream.Seek(startOfPatternData + synthData.PatternNumber * 4 * patternLength * channelNum, SeekOrigin.Begin);

								synthData.WaveData = new sbyte[0x380];
								synthData.Data = new byte[0x40];
								synthData.VolumeData = new byte[0x40];

								moduleStream.ReadSigned(synthData.WaveData, 0, synthData.WaveData.Length);
								moduleStream.ReadInto(synthData.Data, 0, synthData.Data.Length);
								moduleStream.ReadInto(synthData.VolumeData, 0, synthData.VolumeData.Length);
							}
						}
					}
				}

				// Ok, we're done, load any extra files if needed
				if (IsStarTrekker())
					LoadSynthSamples(fileInfo);

				errorMessage = string.Empty;
				return AgentResult.Ok;
			}
			catch (Exception)
			{
				Cleanup();
				throw;
			}
		}
		#endregion

		#region SoundTracker 2.6 loader
		/********************************************************************/
		/// <summary>
		/// Will load a SoundTracker 2.6 module into memory
		/// </summary>
		/********************************************************************/
		private AgentResult LoadSoundTracker26(PlayerFileInfo fileInfo, out string errorMessage)
		{
			try
			{
				byte[] buf = new byte[23];

				ModuleStream moduleStream = fileInfo.ModuleStream;
				Encoding encoder = EncoderCollection.Amiga;

				sampleNum = 31;
				globalVolume = 64;

				// Read the song name
				buf[20] = 0x00;
				moduleStream.ReadInto(buf, 0, 20);

				songName = encoder.GetString(buf);

				// Allocate space to the samples
				samples = new Sample[sampleNum];

				// Read the samples
				for (int i = 0; i < sampleNum; i++)
				{
					Sample sample = samples[i] = new Sample();

					// Read the sample info
					buf[22] = 0x00;
					moduleStream.ReadInto(buf, 0, 22);				// Name of the sample

					ushort length = moduleStream.Read_B_UINT16();			// Length in words
					ushort volume = moduleStream.Read_B_UINT16();			// The volume
					ushort repeatStart = moduleStream.Read_B_UINT16();		// Number of words from the beginning where the loop starts
					ushort repeatLength = moduleStream.Read_B_UINT16();		// The loop length in words

					// If repeat length is 1, it is the same as no loop, so reset it
					if (repeatLength == 1)
						repeatLength = 0;

					// Correct "funny" modules
					if (repeatStart > length)
					{
						repeatStart = 0;
						repeatLength = 0;
					}

					if ((repeatStart + repeatLength) > length)
						repeatLength = (ushort)(length - repeatStart);

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MOD_ERR_LOADING_SAMPLEINFO;
						Cleanup();

						return AgentResult.Error;
					}

					// Put the information into the sample structure
					sample.SampleName = encoder.GetString(buf).RemoveInvalidChars();
					sample.Data = null;
					sample.Length = length;
					sample.LoopStart = repeatStart;
					sample.LoopLength = repeatLength;
					sample.Volume = (byte)volume;
					sample.FineTune = 0;
					sample.FineTuneHmn = 0;
				}

				// Read more header information
				songLength = moduleStream.Read_UINT8();
				trackNum = moduleStream.Read_UINT8();

				initTempo = 125;
				restartPos = 0;
				showTracks = true;

				positions = new byte[128 * 4];
				int bytesRead = moduleStream.Read(positions, 0, positions.Length);

				if (bytesRead < positions.Length)
				{
					errorMessage = Resources.IDS_MOD_ERR_LOADING_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				// Skip mark
				moduleStream.Seek(4, SeekOrigin.Current);

				// Set the missing information
				patternLength = 64;
				channelNum = 4;

				// Find the highest track number
				ushort maxTrack = 0;

				for (int i = 0; i < 128 * 4; i++)
				{
					if (positions[i] > maxTrack)
						maxTrack = positions[i];
				}

				maxTrack++;
				if (maxTrack != trackNum)
				{
					errorMessage = Resources.IDS_MOD_ERR_LOADING_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				// Allocate space for the tracks
				tracks = new TrackLine[trackNum][];

				// Read the tracks
				for (int i = 0; i < trackNum; i++)
				{
					// Allocate memory to hold the track
					tracks[i] = new TrackLine[patternLength];

					LoadModTracks(moduleStream, tracks, i, 1);

					if (moduleStream.EndOfStream)
					{
						errorMessage = Resources.IDS_MOD_ERR_LOADING_TRACKS;
						Cleanup();

						return AgentResult.Error;
					}
				}

				// Allocate memory to hold the sequences
				sequences = new ushort[channelNum, songLength];

				// Calculate the sequence numbers
				for (int i = 0; i < songLength; i++)
				{
					for (int j = 0; j < channelNum; j++)
						sequences[j, i] = positions[i * channelNum + j];

					positions[i] = (byte)i;
				}

				// Read the samples
				for (int i = 0; i < sampleNum; i++)
				{
					// Allocate space to hold the sample
					int length = samples[i].Length * 2;
					if (length != 0)
					{
						sbyte[] sampleBuffer = new sbyte[length];
						samples[i].Data = sampleBuffer;

						using (ModuleStream sampleDataStream = moduleStream.GetSampleDataStream(i, length))
						{
							// Check to see if we miss too much from the last sample
							if (sampleDataStream.Length - sampleDataStream.Position < (length - 512))
							{
								errorMessage = Resources.IDS_MOD_ERR_LOADING_SAMPLES;
								Cleanup();

								return AgentResult.Error;
							}

							// Read the sample
							sampleDataStream.ReadSigned(sampleBuffer, 0, length);
						}
					}
				}

				errorMessage = string.Empty;
				return AgentResult.Ok;
			}
			catch (Exception)
			{
				Cleanup();
				throw;
			}
		}
		#endregion

		#region ProTracker IFF loader
		/********************************************************************/
		/// <summary>
		/// Will load a ProTracker IFF module into memory
		/// </summary>
		/********************************************************************/
		private AgentResult LoadProTrackerIff(PlayerFileInfo fileInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			try
			{
				ModuleStream moduleStream = fileInfo.ModuleStream;

				moduleStream.Seek(12, SeekOrigin.Begin);

				InfoChunk infoChunk = null;
				bool hasPtdt = false;

				for (;;)
				{
					// Read the chunk name and length
					string chunkName = moduleStream.ReadMark();
					uint chunkSize = moduleStream.Read_B_UINT32() - 8;

					// Do we have any chunks left?
					if (moduleStream.EndOfStream)
						break;			// No, stop the loading

					// Find out what the chunk is and begin to parse it
					switch (chunkName)
					{
						// Version
						case "VERS":
						{
							// Ignore the VERS chunk, but it has an invalid size, so skip an absolute number
							moduleStream.Seek(10, SeekOrigin.Current);
							break;
						}

						// Module information
						case "INFO":
						{
							infoChunk = ParseInfo(moduleStream, chunkSize, out errorMessage);
							break;
						}

						// Ordinary module
						case "PTDT":
						{
							ParsePtdt(fileInfo, out errorMessage);
							hasPtdt = true;
							break;
						}

						// Unknown chunks
						default:
						{
							// Ignore unknown chunks
							moduleStream.Seek(chunkSize, SeekOrigin.Current);
							break;
						}
					}

					if (!string.IsNullOrEmpty(errorMessage))
					{
						Cleanup();
						return AgentResult.Error;
					}
				}

				if ((infoChunk == null) || !hasPtdt)
				{
					Cleanup();

					errorMessage = Resources.IDS_MOD_ERR_MISSING_CHUNK;
					return AgentResult.Error;
				}

				initTempo = (byte)infoChunk.DefaultBpm;
				globalVolume = (byte)infoChunk.GlobalVolume;

				return AgentResult.Ok;
			}
			catch (Exception)
			{
				Cleanup();
				throw;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse the INFO chunk
		/// </summary>
		/********************************************************************/
		private InfoChunk ParseInfo(ModuleStream moduleStream, uint chunkSize, out string errorMessage)
		{
			if (chunkSize < 64)
			{
				errorMessage = Resources.IDS_MOD_ERR_LOADING_HEADER;
				return null;
			}

			InfoChunk infoChunk = new InfoChunk();

			// Skip no needed information
			moduleStream.Seek(38, SeekOrigin.Current);

			// Read needed information
			infoChunk.GlobalVolume = moduleStream.Read_B_UINT16();
			infoChunk.DefaultBpm = moduleStream.Read_B_UINT16();

			if (moduleStream.EndOfStream)
			{
				errorMessage = Resources.IDS_MOD_ERR_LOADING_HEADER;
				return null;
			}

			// Skip rest of chunk
			moduleStream.Seek(chunkSize - 64 + 22, SeekOrigin.Current);

			errorMessage = string.Empty;

			return infoChunk;
		}



		/********************************************************************/
		/// <summary>
		/// Parse the PTDT chunk
		/// </summary>
		/********************************************************************/
		private void ParsePtdt(PlayerFileInfo fileInfo, out string errorMessage)
		{
			LoadTracker(fileInfo, out errorMessage);
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Will load x number of tracks in MOD format
		/// </summary>
		/********************************************************************/
		private void LoadModTracks(ModuleStream moduleStream, TrackLine[][] tracks, int offset, int channels)
		{
			for (int i = 0; i < patternLength; i++)
			{
				for (int j = 0; j < channels; j++)
				{
					TrackLine workLine = tracks[offset + j][i] = new TrackLine();

					byte a = moduleStream.Read_UINT8();
					byte b = moduleStream.Read_UINT8();
					byte c = moduleStream.Read_UINT8();
					byte d = moduleStream.Read_UINT8();

					ushort note = (ushort)(((a & 0x0f) << 8) | b);

					// Is there any note?
					if (note != 0)
					{
						int n;
						for (n = 0; n < NumberOfNotes; n++)
						{
							if (note >= Tables.Periods[0, n])
								break;		// Found the note number
						}

						workLine.Note = (byte)(n == NumberOfNotes ? n : n + 1);
					}
					else
						workLine.Note = 0;

					workLine.Sample = (byte)((a & 0xf0) | ((c & 0xf0) >> 4));
					workLine.Effect = (Effect)(c & 0x0f);
					workLine.EffectArg = d;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will load the extra synth file used by StarTrekker
		/// </summary>
		/********************************************************************/
		private void LoadSynthSamples(PlayerFileInfo fileInfo)
		{
			using (ModuleStream moduleStream = ModTrackerIdentifier.OpenSynthFile(fileInfo, true))
			{
				// Did we get any file at all
				if (moduleStream != null)
				{
					byte[] id = new byte[16];
					if ((moduleStream.Read(id, 0, 16) == 16) && (id.SequenceEqual(ModTrackerIdentifier.StSynthId1) || id.SequenceEqual(ModTrackerIdentifier.StSynthId2) || id.SequenceEqual(ModTrackerIdentifier.AsSynthId1)))
					{
						amData = new AmSample[31];

						// Skip header
						moduleStream.Seek(144, SeekOrigin.Begin);

						// Load the AM data
						for (int i = 0; i < 31; i++)
						{
							AmSample amSample = new AmSample();

							amSample.Mark = moduleStream.Read_B_UINT16();
							moduleStream.Seek(4, SeekOrigin.Current);
							amSample.StartAmp = moduleStream.Read_B_UINT16();
							amSample.Attack1Level = moduleStream.Read_B_UINT16();
							amSample.Attack1Speed = moduleStream.Read_B_UINT16();
							amSample.Attack2Level = moduleStream.Read_B_UINT16();
							amSample.Attack2Speed = moduleStream.Read_B_UINT16();
							amSample.SustainLevel = moduleStream.Read_B_UINT16();
							amSample.DecaySpeed = moduleStream.Read_B_UINT16();
							amSample.SustainTime = moduleStream.Read_B_UINT16();
							moduleStream.Seek(2, SeekOrigin.Current);
							amSample.ReleaseSpeed = moduleStream.Read_B_UINT16();
							amSample.Waveform = moduleStream.Read_B_UINT16();
							amSample.PitchFall = moduleStream.Read_B_UINT16();
							amSample.VibAmp = (short)moduleStream.Read_B_UINT16();
							amSample.VibSpeed = moduleStream.Read_B_UINT16();
							amSample.BaseFreq = moduleStream.Read_B_UINT16();

							moduleStream.Seek(84, SeekOrigin.Current);

							amData[i] = amSample;

							if (moduleStream.EndOfStream)
								break;
						}
					}
				}
			}
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int startPosition)
		{
			// Initialize all the variables
			playingInfo = new GlobalPlayingInfo
			{
				SpeedEven = 6,
				SpeedOdd = 6,
				LastShownSpeed = 6,
				Tempo = initTempo,
				PatternPos = 0,
				Counter = 0,
				SongPos = (ushort)startPosition,
				BreakPos = 0,
				PosJumpFlag = false,
				BreakFlag = false,
				GotBreak = false,
				LowMask = 0xff,
				PattDelayTime = 0,
				PattDelayTime2 = 0,
				LastUsedPositionJumpArgument = -1,
				LastUsedBreakPositionArgument = -1
			};

			endReached = false;
			restartSong = false;

			for (int i = 0; i < channelNum; i++)
			{
				ModChannel modChan = channels[i] = new ModChannel();

				modChan.TrackLine.Note = 0;
				modChan.TrackLine.Sample = 0;
				modChan.TrackLine.Effect = 0;
				modChan.TrackLine.EffectArg = 0;

				modChan.SampleNumber = 0;
				modChan.SampleData = null;
				modChan.Offset = 0;
				modChan.Length = 0;
				modChan.LoopStart = 0;
				modChan.LoopLength = 0;
				modChan.StartOffset = 0;
				modChan.Period = 0;
				modChan.FineTune = 0;
				modChan.Volume = 0;
				modChan.TonePortDirec = 0;
				modChan.TonePortSpeed = 0;
				modChan.WantedPeriod = 0;
				modChan.VibratoCmd = 0;
				modChan.VibratoPos = 0;
				modChan.TremoloCmd = 0;
				modChan.TremoloPos = 0;
				modChan.WaveControl = 0;
				modChan.GlissFunk = 0;
				modChan.SampleOffset = 0;
				modChan.PattPos = 0;
				modChan.LoopCount = 0;
				modChan.FunkOffset = 0;
				modChan.WaveStart = 0;
				modChan.AutoSlide = false;
				modChan.AutoSlideArg = 0;

				modChan.SynthSample = false;

				modChan.AmToDo = AmToDo.None;
				modChan.VibDegree = 0;
				modChan.SustainCounter = 0;
				modChan.StarVolume = 0;

				modChan.DataCounter = 0;
				modChan.HmnVolume = 0;
				modChan.SynthData = null;
			}

			SetBpmTempo(initTempo);
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			positions = null;

			samples = null;
			tracks = null;
			sequences = null;

			channels = null;
			playingInfo = null;
		}



		/********************************************************************/
		/// <summary>
		/// Convert a SoundTracker BPM to a real BPM
		/// </summary>
		/********************************************************************/
		private ushort ConvertSoundTrackerBpm(ushort bpm)
		{
			bpm = (ushort)(((709378.92 / 50) * 125) / ((240 - bpm) * 122));
			if (bpm > 255)
				bpm = 255;

			return bpm;
		}



		/********************************************************************/
		/// <summary>
		/// Jumps to the next song position
		/// </summary>
		/********************************************************************/
		private void NextPosition()
		{
			playingInfo.SongPos += 1;

			if (playingInfo.SongPos >= songLength)
			{
				playingInfo.SongPos = restartPos;
				playingInfo.LastUsedBreakPositionArgument = -1;
				playingInfo.LastUsedPositionJumpArgument = -1;

				// And the module has repeated
				endReached = true;
			}
			else
			{
				if (HasPositionBeenVisited(playingInfo.SongPos))
				{
					// No Dxx                 || Change of position
					if (!playingInfo.GotBreak || (playingInfo.SongPos != playingInfo.OldSongPos))
						endReached = true;
					else
					{
						if ((playingInfo.GotBreak && playingInfo.GotPositionJump && (playingInfo.SongPos == playingInfo.LastUsedPositionJumpArgument) && (playingInfo.BreakPos == playingInfo.LastUsedBreakPositionArgument)))
						{
							endReached = true;
							restartSong = true;
						}
					}
				}
			}

			// Initialize the position variables
			MarkPositionAsVisited(playingInfo.SongPos);

			if (playingInfo.GotBreak)
				playingInfo.LastUsedBreakPositionArgument = (short)playingInfo.BreakPos;

			if (playingInfo.GotPositionJump)
				playingInfo.LastUsedPositionJumpArgument = (short)playingInfo.SongPos;

			playingInfo.PatternPos = playingInfo.BreakPos;
			playingInfo.BreakPos = 0;
			playingInfo.PosJumpFlag = false;
			playingInfo.GotBreak = false;
			playingInfo.GotPositionJump = false;

			ShowSongPosition();
			ShowPattern();
		}



		/********************************************************************/
		/// <summary>
		/// Set the period in NostalgicPlayer
		/// </summary>
		/********************************************************************/
		private void SetPeriod(ushort period, IChannel chan, ModChannel modChan)
		{
			if (currentModuleType == ModuleType.HisMastersNoise)
			{
				// Do special fine tuning
				period = (ushort)(period + (period * modChan.FineTuneHmn) / 256);
			}

			chan.SetAmigaPeriod(period);
		}



		/********************************************************************/
		/// <summary>
		/// Checks all channels to see if any commands should run
		/// </summary>
		/********************************************************************/
		private void NoNewAllChannels()
		{
			for (int i = 0; i < channelNum; i++)
			{
				IChannel chan = VirtualChannels[i];
				ModChannel modChan = channels[i];

				CheckEffects(chan, modChan);

				if (currentModuleType == ModuleType.HisMastersNoise)
				{
					ProgHandler(modChan);

					if (modChan.SynthSample)
					{
						chan.SetSample(modChan.SynthData.WaveData, modChan.LoopStart, (uint)(modChan.LoopLength * 2));
						chan.SetLoop(modChan.LoopStart, (uint)modChan.LoopLength * 2);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parses the next pattern line
		/// </summary>
		/********************************************************************/
		private void GetNewNote()
		{
			// Get position information into temporary variables
			ushort curSongPos = playingInfo.SongPos;
			ushort curPattPos = playingInfo.PatternPos;

			for (int i = 0; i < channelNum; i++)
			{
				// Find the track to use
				ushort trkNum = sequences[i, positions[curSongPos]];
				TrackLine trackData = tracks[trkNum][curPattPos];

				IChannel chan = VirtualChannels[i];
				ModChannel modChan = channels[i];

				PlayVoice(trackData, chan, modChan);

				if (currentModuleType == ModuleType.HisMastersNoise)
					ProgHandler(modChan);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parses one pattern line for one channel
		/// </summary>
		/********************************************************************/
		private void PlayVoice(TrackLine trackData, IChannel chan, ModChannel modChan)
		{
			// Check for any note or effect running
			if ((modChan.TrackLine.Note == 0) && (modChan.TrackLine.Sample == 0) && (modChan.TrackLine.Effect == 0) && (modChan.TrackLine.EffectArg == 0))
			{
				// Nothing runs, so set the period
				SetPeriod(modChan.Period, chan, modChan);
			}

			// Copy pattern line to fields in our channel structure
			modChan.TrackLine.Note = trackData.Note;
			modChan.TrackLine.Sample = trackData.Sample;
			modChan.TrackLine.Effect = trackData.Effect;
			modChan.TrackLine.EffectArg = trackData.EffectArg;

			byte sampNum = modChan.TrackLine.Sample;
			if ((sampNum != 0) && (sampNum <= sampleNum))
			{
				// New sample
				Sample sample = samples[sampNum - 1];

				modChan.SampleNumber = (short)(sampNum - 1);
				modChan.SynthSample = false;

				modChan.SampleData = sample.Data;
				modChan.Offset = 0;
				modChan.Length = sample.Length;
				modChan.StartOffset = 0;
				modChan.FineTune = sample.FineTune;
				modChan.FineTuneHmn = sample.FineTuneHmn;
				modChan.Volume = (sbyte)sample.Volume;
				modChan.HmnVolume = 64;
				modChan.StarVolume = 256;

				if (IsStarTrekker())
				{
					AmSample amSamp = amData?[sampNum - 1];
					if ((amSamp != null) && (amSamp.Mark == 0x414d))	// AM
					{
						modChan.StarVolume = (short)amSamp.StartAmp;
						modChan.Volume = 64;
						modChan.SynthSample = true;
					}
				}

				// Check to see if we got a loop
				if ((sample.LoopStart != 0) && (sample.LoopLength > 1))
				{
					// We have, now check the player mode
					if (currentModuleType <= ModuleType.SoundTracker2x)
					{
						// Only plays the loop part. Loop start has been converted to words in loader
						modChan.Offset = sample.LoopStart * 2U;
						modChan.LoopStart = modChan.Offset;
						modChan.WaveStart = modChan.Offset;
						modChan.Length = sample.LoopLength;
						modChan.LoopLength = modChan.Length;
					}
					else
					{
						modChan.LoopStart = sample.LoopStart * 2U;
						modChan.WaveStart = modChan.LoopStart;

						modChan.Length = (ushort)(sample.LoopStart + sample.LoopLength);
						modChan.LoopLength = sample.LoopLength;
					}
				}
				else
				{
					// No loop
					modChan.LoopStart = sample.LoopStart * 2U;
					modChan.WaveStart = modChan.LoopStart;
					modChan.LoopLength = sample.LoopLength;
				}

				if (currentModuleType == ModuleType.HisMastersNoise)
				{
					if (hmnSynthData != null)
					{
						HmnSynthData synthData = hmnSynthData[sampNum - 1];
						if (synthData != null)
						{
							modChan.SynthSample = true;
							modChan.SampleData = synthData.WaveData;
							modChan.LoopStart = (ushort)(synthData.Data[0] * 32);
							modChan.LoopLength = 16;
							modChan.SynthData = synthData;

							modChan.HmnVolume = (short)(synthData.VolumeData[0] & 0x7f);
						}
					}
				}
			}

			// Check for some commands
			if (modChan.TrackLine.Note != 0)
			{
				// There is a new note to play
				Effect cmd = modChan.TrackLine.Effect;

				if (!modChan.SynthSample || !IsStarTrekker())
				{
					if ((currentModuleType == ModuleType.SoundTrackerII) || (currentModuleType == ModuleType.SoundTrackerVI))
					{
						if ((Effect2)cmd == Effect2.AutoVolumeSlide)
						{
							modChan.AutoSlide = true;
							modChan.AutoSlideArg = modChan.TrackLine.EffectArg;
						}
						else
						{
							if (modChan.TrackLine.EffectArg == 0)
							{
								modChan.AutoSlide = false;
								modChan.AutoSlideArg = 0;
							}
						}
					}
					else
					{
						if (IsNoiseTracker() || IsProTracker())
						{
							// Check for SetFineTune
							if (IsProTracker() && (cmd == Effect.ExtraEffect) && ((ExtraEffect)(modChan.TrackLine.EffectArg & 0xf0) == ExtraEffect.SetFineTune))
								SetFineTune(modChan);
							else
							{
								switch (cmd)
								{
									case Effect.TonePortamento:
									case Effect.TonePort_VolSlide:
									{
										SetTonePorta(modChan);
										CheckMoreEffects(chan, modChan);
										return;
									}

									case Effect.SampleOffset:
									{
										CheckMoreEffects(chan, modChan);
										break;
									}
								}
							}
						}
					}
				}

				// Set the period
				modChan.Period = Tables.Periods[modChan.FineTune, modChan.TrackLine.Note - 1];

				if (!IsProTracker() || (cmd != Effect.ExtraEffect) || ((ExtraEffect)(modChan.TrackLine.EffectArg & 0xf0) != ExtraEffect.NoteDelay))
				{
					if ((modChan.WaveControl & 4) == 0)
						modChan.VibratoPos = 0;

					if ((modChan.WaveControl & 64) == 0)
						modChan.TremoloPos = 0;

					if (modChan.SynthSample)
					{
						if (IsStarTrekker())
						{
							// Setup AM sample
							AmSample amSample = amData?[modChan.SampleNumber];
							if (amSample != null)
							{
								modChan.SampleData = Tables.AmWaveforms[amSample.Waveform];
								modChan.Offset = 0;
								modChan.StartOffset = 0;
								modChan.Length = 16;
								modChan.LoopStart = 0;
								modChan.LoopLength = 16;

								modChan.AmToDo = AmToDo.Attack1;
								modChan.StarVolume = (short)amSample.StartAmp;
								modChan.VibDegree = 0;
								modChan.Period = (ushort)(modChan.Period << amSample.BaseFreq);
							}
						}
						else if (currentModuleType == ModuleType.HisMastersNoise)
						{
							modChan.DataCounter = 0;

							modChan.StartOffset = modChan.LoopStart;
							modChan.Length = modChan.LoopLength;
						}
					}

					// Fill out the channel
					if ((modChan.Length > 0) && (modChan.SampleData != null))
					{
						uint offset = modChan.Offset + modChan.StartOffset;
						chan.PlaySample(modChan.SampleNumber, modChan.SampleData, offset, (uint)(modChan.Length * 2));
						SetPeriod(modChan.Period, chan, modChan);

						// Setup loop
						if (modChan.LoopLength > 0)
							chan.SetLoop(modChan.LoopStart, (uint)modChan.LoopLength * 2);
					}
					else
						chan.Mute();
				}
			}

			CheckMoreEffects(chan, modChan);
		}



		/********************************************************************/
		/// <summary>
		/// Check one channel to see if there are some commands to run
		/// </summary>
		/********************************************************************/
		private void CheckEffects(IChannel chan, ModChannel modChan)
		{
			UpdateFunk(modChan);

			Effect cmd = modChan.TrackLine.Effect;
			if ((cmd != 0) || (modChan.TrackLine.EffectArg != 0))
			{
				if ((currentModuleType == ModuleType.UltimateSoundTracker10) || (currentModuleType == ModuleType.UltimateSoundTracker18))
				{
					switch ((UltraEffect)cmd)
					{
						case UltraEffect.Arpreggio:
						{
							Arpeggio(chan, modChan);
							break;
						}

						case UltraEffect.PitchBend:
						{
							PitchBend(chan, modChan);
							break;
						}
					}
				}
				else if ((currentModuleType == ModuleType.SoundTrackerII) || (currentModuleType == ModuleType.SoundTrackerVI))
				{
					if (modChan.AutoSlide)
						VolumeSlide(modChan, modChan.AutoSlideArg);

					switch ((Effect2)cmd)
					{
						case Effect2.Arpeggio:
//						case Effect2.ModulateVolume:
//						case Effect2.ModulatePeriod:
//						case Effect2.ModulateVolumePeriod:
						{
							// Arpeggio or normal note
							Arpeggio(chan, modChan);
							break;
						}

						case Effect2.SlideUp:
//						case Effect2.ModulateVolumeSlideUp:
//						case Effect2.ModulatePeriodSlideUp:
//						case Effect2.ModulateVolumePeriodSlideUp:
						{
							PortaUp(chan, modChan);
							break;
						}

						case Effect2.SlideDown:
//						case Effect2.ModulateVolumeSlideDown:
//						case Effect2.ModulatePeriodSlideDown:
//						case Effect2.ModulateVolumePeriodSlideDown:
						{
							PortaDown(chan, modChan);
							break;
						}

						case Effect2.VolumeSlide:
						{
							VolumeSlide(modChan, modChan.TrackLine.EffectArg);
							break;
						}
					}
				}
				else if (currentModuleType == ModuleType.HisMastersNoise)
				{
					switch (cmd)
					{
						case Effect.Arpeggio:
						{
							// Arpeggio or normal note
							Arpeggio(chan, modChan);
							break;
						}

						case Effect.SlideUp:
						{
							PortaUp(chan, modChan);
							break;
						}

						case Effect.SlideDown:
						{
							PortaDown(chan, modChan);
							break;
						}

						case Effect.TonePortamento:
						{
							TonePortamento(chan, modChan);
							break;
						}

						case Effect.Vibrato:
						{
							Vibrato(chan, modChan);
							break;
						}

						case Effect.TonePort_VolSlide:
						{
							TonePlusVolSlide(chan, modChan);
							break;
						}

						case Effect.Vibrato_VolSlide:
						{
							VibratoPlusVolSlide(chan, modChan);
							break;
						}

						case Effect.MegaArp:
						{
							MegaArpeggio(chan, modChan);
							break;
						}

						default:
						{
							SetPeriod(modChan.Period, chan, modChan);

							if (cmd == Effect.VolumeSlide)
								VolumeSlide(modChan, modChan.TrackLine.EffectArg);

							break;
						}
					}
				}
				else if (currentModuleType == ModuleType.SoundTracker26)
				{
					switch (cmd)
					{
						case Effect.Arpeggio:
						{
							// Arpeggio or normal note
							Arpeggio(chan, modChan);
							break;
						}

						case Effect.SlideUp:
						{
							PortaUp(chan, modChan);
							break;
						}

						case Effect.SlideDown:
						{
							PortaDown(chan, modChan);
							break;
						}

						case Effect.TonePortamento:
						{
							TonePortamento(chan, modChan);
							break;
						}

						case Effect.Vibrato:
						{
							Vibrato(chan, modChan);
							break;
						}

						default:
						{
							SetPeriod(modChan.Period, chan, modChan);

							if (cmd == Effect.VolumeSlide)
								VolumeSlide(modChan, modChan.TrackLine.EffectArg);

							break;
						}
					}
				}
				else
				{
					switch (cmd)
					{
						case Effect.Arpeggio:
						{
							// Arpeggio or normal note
							Arpeggio(chan, modChan);
							break;
						}

						case Effect.SlideUp:
						{
							PortaUp(chan, modChan);
							break;
						}

						case Effect.SlideDown:
						{
							PortaDown(chan, modChan);
							break;
						}

						case Effect.TonePortamento:
						{
							TonePortamento(chan, modChan);
							break;
						}

						case Effect.Vibrato:
						{
							Vibrato(chan, modChan);
							break;
						}

						case Effect.TonePort_VolSlide:
						{
							TonePlusVolSlide(chan, modChan);
							break;
						}

						case Effect.Vibrato_VolSlide:
						{
							VibratoPlusVolSlide(chan, modChan);
							break;
						}

						case Effect.ExtraEffect:
						{
							ECommands(chan, modChan);
							break;
						}

						default:
						{
							SetPeriod(modChan.Period, chan, modChan);

							if (cmd == Effect.Tremolo)
								Tremolo(modChan);
							else
							{
								if (cmd == Effect.VolumeSlide)
									VolumeSlide(modChan, modChan.TrackLine.EffectArg);
							}
							break;
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Check one channel to see if there are some commands to run
		/// </summary>
		/********************************************************************/
		private void CheckMoreEffects(IChannel chan, ModChannel modChan)
		{
			switch (currentModuleType)
			{
				case ModuleType.UltimateSoundTracker10:
				case ModuleType.UltimateSoundTracker18:
				{
					SetPeriod(modChan.Period, chan, modChan);
					break;
				}

				case ModuleType.SoundTrackerII:
				case ModuleType.SoundTrackerVI:
				{
					switch ((Effect2)modChan.TrackLine.Effect)
					{
						case Effect2.SetVolume:
						{
							VolumeChange(modChan);
							break;
						}

						case Effect2.SetSpeed:
						{
							SetSpeed(modChan);
							break;
						}
					}
					break;
				}

				case ModuleType.SoundTrackerIX:
				case ModuleType.MasterSoundTracker10:
				{
					switch (modChan.TrackLine.Effect)
					{
						case Effect.SetVolume:
						{
							VolumeChange(modChan);
							break;
						}

						case Effect.ExtraEffect:
						{
							ECommands(chan, modChan);
							break;
						}

						case Effect.SetSpeed:
						{
							SetSpeed(modChan);
							break;
						}

						default:
						{
							SetPeriod(modChan.Period, chan, modChan);
							break;
						}
					}
					break;
				}

				default:
				{
					switch (modChan.TrackLine.Effect)
					{
						case Effect.SampleOffset:
						{
							if (!IsNoiseTracker() && !IsSoundTracker())
								SampleOffset(modChan);

							break;
						}

						case Effect.PosJump:
						{
							PositionJump(modChan);
							break;
						}

						case Effect.SetVolume:
						{
							VolumeChange(modChan);
							break;
						}

						case Effect.PatternBreak:
						{
							PatternBreak(modChan);
							break;
						}

						case Effect.ExtraEffect:
						{
							ECommands(chan, modChan);
							break;
						}

						case Effect.SetSpeed:
						{
							SetSpeed(modChan);
							break;
						}

						default:
						{
							SetPeriod(modChan.Period, chan, modChan);
							break;
						}
					}
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Check one channel to see if there are some of the extra commands
		/// to run
		/// </summary>
		/********************************************************************/
		private void ECommands(IChannel chan, ModChannel modChan)
		{
			if (currentModuleType >= ModuleType.SoundTrackerIX)
			{
				if ((currentModuleType == ModuleType.IceTracker) && ((ExtraEffect)(modChan.TrackLine.EffectArg & 0xf0) == ExtraEffect.InvertLoop))
					FunkIt(modChan);
				else if (IsSoundTracker() || IsNoiseTracker())
					FilterOnOff(modChan);
				else
				{
					switch ((ExtraEffect)(modChan.TrackLine.EffectArg & 0xf0))
					{
						case ExtraEffect.SetFilter:
						{
							FilterOnOff(modChan);
							break;
						}

						case ExtraEffect.FineSlideUp:
						{
							FinePortaUp(chan, modChan);
							break;
						}

						case ExtraEffect.FineSlideDown:
						{
							FinePortaDown(chan, modChan);
							break;
						}

						case ExtraEffect.GlissandoCtrl:
						{
							SetGlissControl(modChan);
							break;
						}

						case ExtraEffect.VibratoWaveform:
						{
							SetVibratoControl(modChan);
							break;
						}

						case ExtraEffect.SetFineTune:
						{
							SetFineTune(modChan);
							break;
						}

						case ExtraEffect.JumpToLoop:
						{
							JumpLoop(modChan);
							break;
						}

						case ExtraEffect.TremoloWaveform:
						{
							SetTremoloControl(modChan);
							break;
						}

						case ExtraEffect.KarplusStrong:
						{
							KarplusStrong(modChan);
							break;
						}

						case ExtraEffect.Retrig:
						{
							RetrigNote(chan, modChan);
							break;
						}

						case ExtraEffect.FineVolSlideUp:
						{
							VolumeFineUp(modChan);
							break;
						}

						case ExtraEffect.FineVolSlideDown:
						{
							VolumeFineDown(modChan);
							break;
						}

						case ExtraEffect.NoteCut:
						{
							NoteCut(modChan);
							break;
						}

						case ExtraEffect.NoteDelay:
						{
							NoteDelay(chan, modChan);
							break;
						}

						case ExtraEffect.PatternDelay:
						{
							PatternDelay(modChan);
							break;
						}

						case ExtraEffect.InvertLoop:
						{
							FunkIt(modChan);
							break;
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Sets the portamento frequency
		/// </summary>
		/********************************************************************/
		private void SetTonePorta(ModChannel modChan)
		{
			ushort period = Tables.Periods[0, modChan.TrackLine.Note - 1];

			int i;
			for (i = 0; i < NumberOfNotes; i++)
			{
				if (Tables.Periods[modChan.FineTune, i] <= period)
				{
					i++;
					break;
				}
			}

			// Decrement counter so it have the right value.
			// This is because if the loop goes all the way through
			i--;

			if ((modChan.FineTune > 7) && (i != 0))
				i--;

			period = Tables.Periods[modChan.FineTune, i];

			modChan.WantedPeriod = period;
			modChan.TonePortDirec = 0;

			if (modChan.Period == period)
				modChan.WantedPeriod = 0;
			else
			{
				if (modChan.Period > period)
					modChan.TonePortDirec = 1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Updates funk?
		/// </summary>
		/********************************************************************/
		private void UpdateFunk(ModChannel modChan)
		{
			byte glissFunk = (byte)(modChan.GlissFunk >> 4);
			if (glissFunk != 0)
			{
				modChan.FunkOffset += Tables.FunkTable[glissFunk];
				if (modChan.FunkOffset >= 128)
				{
					modChan.FunkOffset = 0;

					uint waveStart = modChan.WaveStart + 1;
					if (waveStart >= (modChan.LoopStart + modChan.LoopLength * 2))
						waveStart = modChan.LoopStart;

					modChan.WaveStart = waveStart;

					// Invert the sample data
					if (modChan.SampleData != null)
						modChan.SampleData[waveStart] = (sbyte)~modChan.SampleData[waveStart];
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Jump to the given position
		/// </summary>
		/********************************************************************/
		private void SetPosition(ushort pos)
		{
			// Set the new position
			playingInfo.OldSongPos = playingInfo.SongPos;
			playingInfo.SongPos = (ushort)(pos - 1);
			playingInfo.BreakPos = 0;
			playingInfo.PosJumpFlag = true;
			playingInfo.GotPositionJump = true;
		}



		/********************************************************************/
		/// <summary>
		/// Handles StarTrekker AM samples
		/// </summary>
		/********************************************************************/
		private void StarAmHandler()
		{
			for (int i = 0; i < channelNum; i++)
			{
				ModChannel modChan = channels[i];

				if (modChan.SynthSample)
				{
					AmSample amSamp = amData?[modChan.SampleNumber];
					if (amSamp != null)
					{
						IChannel chan = VirtualChannels[i];

						switch (modChan.AmToDo)
						{
							case AmToDo.Attack1:
							{
								if (modChan.StarVolume == amSamp.Attack1Level)
									modChan.AmToDo = AmToDo.Attack2;
								else
								{
									if (modChan.StarVolume < amSamp.Attack1Level)
									{
										modChan.StarVolume += (short)amSamp.Attack1Speed;
										if (modChan.StarVolume >= amSamp.Attack1Level)
										{
											modChan.StarVolume = (short)amSamp.Attack1Level;
											modChan.AmToDo = AmToDo.Attack2;
										}
									}
									else
									{
										modChan.StarVolume -= (short)amSamp.Attack1Speed;
										if (modChan.StarVolume <= amSamp.Attack1Level)
										{
											modChan.StarVolume = (short)amSamp.Attack1Level;
											modChan.AmToDo = AmToDo.Attack2;
										}
									}
								}
								break;
							}

							case AmToDo.Attack2:
							{
								if (modChan.StarVolume == amSamp.Attack2Level)
									modChan.AmToDo = AmToDo.Sustain;
								else
								{
									if (modChan.StarVolume < amSamp.Attack2Level)
									{
										modChan.StarVolume += (short)amSamp.Attack2Speed;
										if (modChan.StarVolume >= amSamp.Attack2Level)
										{
											modChan.StarVolume = (short)amSamp.Attack2Level;
											modChan.AmToDo = AmToDo.Sustain;
										}
									}
									else
									{
										modChan.StarVolume -= (short)amSamp.Attack2Speed;
										if (modChan.StarVolume <= amSamp.Attack2Level)
										{
											modChan.StarVolume = (short)amSamp.Attack2Level;
											modChan.AmToDo = AmToDo.Sustain;
										}
									}
								}
								break;
							}

							case AmToDo.Sustain:
							{
								if (modChan.StarVolume == amSamp.SustainLevel)
								{
									modChan.SustainCounter = (short)amSamp.SustainTime;
									modChan.AmToDo = AmToDo.SustainDecay;
								}
								else
								{
									if (modChan.StarVolume < amSamp.SustainLevel)
									{
										modChan.StarVolume += (short)amSamp.DecaySpeed;
										if (modChan.StarVolume >= amSamp.SustainLevel)
										{
											modChan.StarVolume = (short)amSamp.SustainLevel;
											modChan.SustainCounter = (short)amSamp.SustainTime;
											modChan.AmToDo = AmToDo.SustainDecay;
										}
									}
									else
									{
										modChan.StarVolume -= (short)amSamp.DecaySpeed;
										if (modChan.StarVolume <= amSamp.SustainLevel)
										{
											modChan.StarVolume = (short)amSamp.SustainLevel;
											modChan.SustainCounter = (short)amSamp.SustainTime;
											modChan.AmToDo = AmToDo.SustainDecay;
										}
									}
								}
								break;
							}

							case AmToDo.SustainDecay:
							{
								modChan.SustainCounter--;
								if (modChan.SustainCounter < 0)
									modChan.AmToDo = AmToDo.Release;

								break;
							}

							case AmToDo.Release:
							{
								modChan.StarVolume -= (short)amSamp.ReleaseSpeed;
								if (modChan.StarVolume <= 0)
								{
									modChan.AmToDo = AmToDo.None;
									modChan.StarVolume = 0;
									modChan.SynthSample = false;

									chan.Mute();
								}
								break;
							}
						}

						// Do the pitch fall
						modChan.Period += amSamp.PitchFall;

						// Do vibrato
						int vibVal = amSamp.VibAmp;
						if (vibVal != 0)
						{
							bool flag = false;
							ushort degree = modChan.VibDegree;

							if (degree >= 180)
							{
								degree -= 180;
								flag = true;
							}

							vibVal = Tables.AmSinus[degree] * amSamp.VibAmp / 128;
							if (flag)
								vibVal = -vibVal;
						}

						// Set new frequency
						SetPeriod((ushort)(modChan.Period + vibVal), chan, modChan);

						modChan.VibDegree += amSamp.VibSpeed;
						if (modChan.VibDegree >= 360)
							modChan.VibDegree -= 360;
					}
				}
			}

			// Generate noise waveform
			for (int i = 0; i < 32; i++)
				Tables.AmWaveforms[3][i] = (sbyte)RandomGenerator.GetRandomNumber(255);
		}



		/********************************************************************/
		/// <summary>
		/// Handles His Master's Noise synth samples
		/// </summary>
		/********************************************************************/
		private void ProgHandler(ModChannel modChan)
		{
			if (modChan.SynthSample)
			{
				byte dataCounter = modChan.DataCounter;

				modChan.HmnVolume = (short)(modChan.SynthData.VolumeData[dataCounter] & 0x7f);
				modChan.LoopStart = (ushort)(modChan.SynthData.Data[dataCounter] * 32);

				dataCounter++;
				if (dataCounter > modChan.SynthData.DataLoopEnd)
					dataCounter = modChan.SynthData.DataLoopStart;

				modChan.DataCounter = dataCounter;
			}
		}

		#region Functions to all the normal effects
		/********************************************************************/
		/// <summary>
		/// 0x00 - Plays arpeggio or normal note
		/// 0x01 for Ultimate SoundTracker
		/// </summary>
		/********************************************************************/
		private void Arpeggio(IChannel chan, ModChannel modChan)
		{
			ushort period;

			if (modChan.TrackLine.EffectArg != 0)
			{
				byte arp;
				byte modulus = (byte)(playingInfo.Counter % 3);

				switch (modulus)
				{
					case 1:
					{
						arp = (byte)(modChan.TrackLine.EffectArg >> 4);
						break;
					}

					case 2:
					{
						arp = (byte)(modChan.TrackLine.EffectArg & 0x0f);
						break;
					}

					default:
					{
						arp = 0;
						break;
					}
				}

				// Find the index into the period tables
				int i;
				for (i = 0; i < NumberOfNotes; i++)
				{
					if (Tables.Periods[modChan.FineTune, i] <= modChan.Period)
						break;
				}

				// Get the period
				int note = i + arp >= NumberOfNotes ? NumberOfNotes - 1 : i + arp;
				period = Tables.Periods[modChan.FineTune, note];
			}
			else
			{
				// Normal note
				period = modChan.Period;
			}

			SetPeriod(period, chan, modChan);
		}



		/********************************************************************/
		/// <summary>
		/// 0x01 - Slides the frequency up or down (Ultimate SoundTracker)
		/// </summary>
		/********************************************************************/
		private void PitchBend(IChannel chan, ModChannel modChan)
		{
			if ((modChan.TrackLine.EffectArg & 0xf0) == 0)
			{
				modChan.Period -= (ushort)(modChan.TrackLine.EffectArg & 0x0f);
				if (modChan.Period < MinPeriod)
					modChan.Period = MinPeriod;
			}
			else
			{
				modChan.Period += (ushort)((modChan.TrackLine.EffectArg & 0xf0) >> 4);
				if (modChan.Period > MaxPeriod)
					modChan.Period = MaxPeriod;
			}

			SetPeriod(modChan.Period, chan, modChan);
		}



		/********************************************************************/
		/// <summary>
		/// 0x01 - Slides the frequency up
		/// </summary>
		/********************************************************************/
		private void PortaUp(IChannel chan, ModChannel modChan)
		{
			modChan.Period -= (ushort)(modChan.TrackLine.EffectArg & playingInfo.LowMask);
			if (modChan.Period < MinPeriod)
				modChan.Period = MinPeriod;

			playingInfo.LowMask = 0xff;

			SetPeriod(modChan.Period, chan, modChan);
		}



		/********************************************************************/
		/// <summary>
		/// 0x02 - Slides the frequency down
		/// </summary>
		/********************************************************************/
		private void PortaDown(IChannel chan, ModChannel modChan)
		{
			modChan.Period += (ushort)(modChan.TrackLine.EffectArg & playingInfo.LowMask);
			if (modChan.Period > MaxPeriod)
				modChan.Period = MaxPeriod;

			playingInfo.LowMask = 0xff;

			SetPeriod(modChan.Period, chan, modChan);
		}



		/********************************************************************/
		/// <summary>
		/// 0x03 - Slides the frequency to the current note
		/// </summary>
		/********************************************************************/
		private void TonePortamento(IChannel chan, ModChannel modChan, bool skip = false)
		{
			if (!skip)
			{
				if (modChan.TrackLine.EffectArg != 0)
				{
					// Set the slide speed
					modChan.TonePortSpeed = modChan.TrackLine.EffectArg;
					modChan.TrackLine.EffectArg = 0;
				}
			}

			// Is slide mode enabled?
			if (modChan.WantedPeriod != 0)
			{
				int period;

				if (modChan.TonePortDirec != 0)
				{
					// Slide up
					period = modChan.Period - modChan.TonePortSpeed;
					if (modChan.WantedPeriod >= period)
					{
						// Set to the final period and disable slide
						period = modChan.WantedPeriod;
						modChan.WantedPeriod = 0;
					}

					modChan.Period = (ushort)period;
				}
				else
				{
					// Slide down
					period = modChan.Period + modChan.TonePortSpeed;
					if (modChan.WantedPeriod <= period)
					{
						// Set to final period and disable slide
						period = modChan.WantedPeriod;
						modChan.WantedPeriod = 0;
					}

					modChan.Period = (ushort)period;
				}

				// Is glissando enabled?
				if ((modChan.GlissFunk & 0x0f) != 0)
				{
					int i;
					for (i = 0; i < NumberOfNotes; i++)
					{
						if (Tables.Periods[modChan.FineTune, i] <= period)
						{
							i++;
							break;
						}
					}

					period = Tables.Periods[modChan.FineTune, i - 1];
				}

				SetPeriod((ushort)period, chan, modChan);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0x04 - Vibrates the frequency
		/// </summary>
		/********************************************************************/
		private void Vibrato(IChannel chan, ModChannel modChan, bool skip = false)
		{
			// Get the effect argument
			byte effArg = modChan.TrackLine.EffectArg;

			// Setup vibrato command
			if (!skip && (effArg != 0))
			{
				byte vibCmd = modChan.VibratoCmd;

				if (IsProTracker())
				{
					if ((effArg & 0x0f) != 0)
						vibCmd = (byte)((vibCmd & 0xf0) | (effArg & 0x0f));

					if ((effArg & 0xf0) != 0)
						vibCmd = (byte)((vibCmd & 0x0f) | (effArg & 0xf0));
				}

				modChan.VibratoCmd = vibCmd;
			}

			// Calculate new position
			byte vibPos = (byte)((modChan.VibratoPos / 4) & 0x1f);
			byte waveCtrl = (byte)(modChan.WaveControl & 0x03);

			byte addVal;

			if (waveCtrl != 0)
			{
				vibPos *= 8;
				if (waveCtrl != 1)
					addVal = 255;		// Square vibrato
				else
				{
					// Ramp down vibrato
					if (modChan.VibratoPos < 0)
						addVal = (byte)(255 - vibPos);
					else
						addVal = vibPos;
				}
			}
			else
			{
				// Sine vibrato
				addVal = Tables.VibratoTable[vibPos];
			}

			// Set the vibrato
			addVal = (byte)(addVal * (modChan.VibratoCmd & 0x0f) / 128);
			ushort period = modChan.Period;

			if (modChan.VibratoPos < 0)
				period -= addVal;
			else
				period += addVal;

			SetPeriod(period, chan, modChan);

			modChan.VibratoPos += (sbyte)((modChan.VibratoCmd / 4) & 0x3c);
		}



		/********************************************************************/
		/// <summary>
		/// 0x05 - Is both effect 0x03 and 0x0a
		/// </summary>
		/********************************************************************/
		private void TonePlusVolSlide(IChannel chan, ModChannel modChan)
		{
			TonePortamento(chan, modChan, true);
			VolumeSlide(modChan, modChan.TrackLine.EffectArg);
		}



		/********************************************************************/
		/// <summary>
		/// 0x06 - Is both effect 0x04 and 0x0a
		/// </summary>
		/********************************************************************/
		private void VibratoPlusVolSlide(IChannel chan, ModChannel modChan)
		{
			Vibrato(chan, modChan, true);
			VolumeSlide(modChan, modChan.TrackLine.EffectArg);
		}



		/********************************************************************/
		/// <summary>
		/// 0x07 - Plays mega arpeggio
		/// </summary>
		/********************************************************************/
		private void MegaArpeggio(IChannel chan, ModChannel modChan)
		{
			sbyte vibPos = (sbyte)(modChan.VibratoPos & 0x0f);
			modChan.VibratoPos++;

			// Get the effect argument
			byte effArg = (byte)(modChan.TrackLine.EffectArg & 0x0f);
			byte arp = Tables.MegaArps[effArg, vibPos];

			// Find the index into the period tables
			int i;
			for (i = 0; i < NumberOfNotes; i++)
			{
				if (Tables.Periods[0, i] <= modChan.Period)
					break;
			}

			// Get the period
			int note = i + arp >= NumberOfNotes ? NumberOfNotes - 1 : i + arp;
			ushort period = Tables.Periods[0, note];
			SetPeriod(period, chan, modChan);
		}



		/********************************************************************/
		/// <summary>
		/// 0x07 - Makes vibrato on the volume
		/// </summary>
		/********************************************************************/
		private void Tremolo(ModChannel modChan)
		{
			// Get the effect argument
			byte effArg = modChan.TrackLine.EffectArg;

			// Setup tremolo command
			if (effArg != 0)
			{
				byte treCmd = modChan.TremoloCmd;

				if ((effArg & 0x0f) != 0)
					treCmd = (byte)((treCmd & 0xf0) | (effArg & 0x0f));

				if ((effArg & 0xf0) != 0)
					treCmd = (byte)((treCmd & 0x0f) | (effArg & 0xf0));

				modChan.TremoloCmd = treCmd;
			}

			// Calculate new position
			byte trePos = (byte)((modChan.TremoloPos / 4) & 0x1f);
			byte waveCtrl = (byte)((modChan.WaveControl >> 4) & 0x03);

			byte addVal;

			if (waveCtrl != 0)
			{
				trePos *= 8;
				if (waveCtrl != 1)
					addVal = 255;		// Square vibrato
				else
				{
					// Ramp down tremolo
					if (modChan.TremoloPos < 0)
						addVal = (byte)(255 - trePos);
					else
						addVal = trePos;
				}
			}
			else
			{
				// Sine vibrato
				addVal = Tables.VibratoTable[trePos];
			}

			// Set the tremolo
			addVal = (byte)(addVal * (modChan.TremoloCmd & 0x0f) / 64);
			short volume = modChan.Volume;

			if (modChan.TremoloPos < 0)
			{
				volume -= addVal;
				if (volume < 0)
					volume = 0;
			}
			else
			{
				volume += addVal;
				if (volume > 64)
					volume = 64;
			}

			modChan.HmnVolume = (sbyte)volume;

			modChan.TremoloPos += (sbyte)((modChan.TremoloCmd / 4) & 0x3c);
		}



		/********************************************************************/
		/// <summary>
		/// 0x09 - Starts the sample somewhere else, but the start
		/// </summary>
		/********************************************************************/
		private void SampleOffset(ModChannel modChan)
		{
			// Check for initialize value
			if (modChan.TrackLine.EffectArg != 0)
				modChan.SampleOffset = modChan.TrackLine.EffectArg;

			// Calculate the offset
			ushort offset = (ushort)(modChan.SampleOffset * 128);
			if (offset < modChan.Length)
			{
				modChan.Length -= offset;
				modChan.StartOffset = offset * 2U;
			}
			else
				modChan.Length = 1;
		}



		/********************************************************************/
		/// <summary>
		/// 0x0A - Slides the volume
		/// </summary>
		/********************************************************************/
		private void VolumeSlide(ModChannel modChan, byte effectArg)
		{
			byte spd = (byte)(effectArg >> 4);
			if (spd != 0)
			{
				// Slide up
				modChan.Volume += (sbyte)spd;
				if (modChan.Volume > 64)
					modChan.Volume = 64;
			}
			else
			{
				// Slide down
				modChan.Volume -= (sbyte)(effectArg & 0x0f);
				if (modChan.Volume < 0)
					modChan.Volume = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0x0B - Jumps to another position
		/// </summary>
		/********************************************************************/
		private void PositionJump(ModChannel modChan)
		{
			byte pos = modChan.TrackLine.EffectArg;
			SetPosition(pos);
		}



		/********************************************************************/
		/// <summary>
		/// 0x0C - Sets the channel volume
		/// </summary>
		/********************************************************************/
		private void VolumeChange(ModChannel modChan)
		{
			byte vol = modChan.TrackLine.EffectArg;
			if (vol > 64)
				vol = 64;

			modChan.Volume = (sbyte)vol;
		}



		/********************************************************************/
		/// <summary>
		/// 0x0D - Breaks the pattern and jump to the next position
		/// </summary>
		/********************************************************************/
		private void PatternBreak(ModChannel modChan)
		{
			byte arg;

			if (IsNoiseTracker() || IsSoundTracker())
				arg = 0;
			else
				arg = modChan.TrackLine.EffectArg;

			playingInfo.BreakPos = (ushort)(((arg >> 4) & 0x0f) * 10 + (arg & 0x0f));
			if (playingInfo.BreakPos > 63)
				playingInfo.BreakPos = 0;

			playingInfo.PosJumpFlag = true;
			playingInfo.GotBreak = true;
		}



		/********************************************************************/
		/// <summary>
		/// 0x0F - Changes the speed of the module
		/// </summary>
		/********************************************************************/
		private void SetSpeed(ModChannel modChan)
		{
			// Get the new speed
			byte newSpeed = modChan.TrackLine.EffectArg;

			if (currentModuleType == ModuleType.SoundTracker26)
			{
				if (newSpeed != 0)
				{
					byte speed1 = (byte)((newSpeed & 0xf0) >> 4);
					byte speed2 = (byte)(newSpeed & 0x0f);

					if (speed1 == 0)
						speed1 = speed2;

					if (speed2 == 0)
						speed2 = speed1;

					playingInfo.SpeedEven = speed2;
					playingInfo.SpeedOdd = speed1;
					playingInfo.Counter = 0;
				}
			}
			else if (currentModuleType == ModuleType.IceTracker)
			{
				newSpeed &= 0x1f;
				if (newSpeed != 0)
				{
					// Set the new speed
					playingInfo.SpeedEven = newSpeed;
					playingInfo.SpeedOdd = newSpeed;
					playingInfo.Counter = 0;
				}
			}
			else if (IsSoundTracker())
			{
				newSpeed &= 0x0f;
				if (newSpeed != 0)
				{
					// Set the new speed
					playingInfo.SpeedEven = newSpeed;
					playingInfo.SpeedOdd = newSpeed;
				}
			}
			else if (IsNoiseTracker())
			{
				if (newSpeed != 0)
				{
					if (newSpeed > 31)
						newSpeed = 31;

					// Set the new speed
					playingInfo.SpeedEven = newSpeed;
					playingInfo.SpeedOdd = newSpeed;
				}
			}
			else
			{
				// New trackers
				if (newSpeed > 0)
				{
					if (newSpeed >= 32)
						ChangeTempo(newSpeed);
					else
					{
						// Set the new speed
						playingInfo.SpeedEven = newSpeed;
						playingInfo.SpeedOdd = newSpeed;
						playingInfo.Counter = 0;
					}
				}
				else
				{
					// If speed is 0, we assume the module has ended (this will fix Kenmare River.mod)
					endReached = true;

					// Make the song start over from the beginning
					restartSong = true;

					// Restart the module
					SetPosition(restartPos);
				}
			}
		}
		#endregion

		#region Functions to all the extended effects
		/********************************************************************/
		/// <summary>
		/// 0xE0 - Changes the filter
		/// </summary>
		/********************************************************************/
		private void FilterOnOff(ModChannel modChan)
		{
			AmigaFilter = (modChan.TrackLine.EffectArg & 0x01) == 0;
		}



		/********************************************************************/
		/// <summary>
		/// 0xE1 - Fine slide the frequency up
		/// </summary>
		/********************************************************************/
		private void FinePortaUp(IChannel chan, ModChannel modChan)
		{
			if (playingInfo.Counter == 0)
			{
				playingInfo.LowMask = 0x0f;
				PortaUp(chan, modChan);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0xE2 - Fine slide the frequency down
		/// </summary>
		/********************************************************************/
		private void FinePortaDown(IChannel chan, ModChannel modChan)
		{
			if (playingInfo.Counter == 0)
			{
				playingInfo.LowMask = 0x0f;
				PortaDown(chan, modChan);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0xE3 - Sets a new glissando control
		/// </summary>
		/********************************************************************/
		private void SetGlissControl(ModChannel modChan)
		{
			modChan.GlissFunk &= 0xf0;
			modChan.GlissFunk |= (byte)(modChan.TrackLine.EffectArg & 0x0f);
		}



		/********************************************************************/
		/// <summary>
		/// 0xE4 - Sets a new vibrato waveform
		/// </summary>
		/********************************************************************/
		private void SetVibratoControl(ModChannel modChan)
		{
			modChan.WaveControl &= 0xf0;
			modChan.WaveControl |= (byte)(modChan.TrackLine.EffectArg & 0x0f);
		}



		/********************************************************************/
		/// <summary>
		/// 0xE5 - Changes the fine tune
		/// </summary>
		/********************************************************************/
		private void SetFineTune(ModChannel modChan)
		{
			modChan.FineTune = (byte)(modChan.TrackLine.EffectArg & 0x0f);
		}



		/********************************************************************/
		/// <summary>
		/// 0xE6 - Jump to pattern loop position
		/// </summary>
		/********************************************************************/
		private void JumpLoop(ModChannel modChan)
		{
			if (playingInfo.Counter == 0)
			{
				byte arg = (byte)(modChan.TrackLine.EffectArg & 0x0f);

				if (arg != 0)
				{
					// Jump to the loop currently set
					if (modChan.LoopCount == 0)
						modChan.LoopCount = arg;
					else
						modChan.LoopCount--;

					if ((modChan.LoopCount != 0))// && (modChan.PattPos != -1))
					{
						playingInfo.BreakPos = (byte)modChan.PattPos;
						playingInfo.BreakFlag = true;
					}
				}
				else
				{
					// Set the loop start point
					modChan.PattPos = (sbyte)playingInfo.PatternPos;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0xE7 - Sets a new tremolo waveform
		/// </summary>
		/********************************************************************/
		private void SetTremoloControl(ModChannel modChan)
		{
			modChan.WaveControl &= 0x0f;
			modChan.WaveControl |= (byte)((modChan.TrackLine.EffectArg & 0x0f) << 4);
		}



		/********************************************************************/
		/// <summary>
		/// 0xE8 - Karplus strong
		/// </summary>
		/********************************************************************/
		private void KarplusStrong(ModChannel modChan)
		{
			if (modChan.SampleData != null)
			{
				uint index = modChan.LoopStart;

				for (int i = modChan.LoopLength * 2 - 2; i >= 0; i--)
				{
					modChan.SampleData[index] = (sbyte)((modChan.SampleData[index] + modChan.SampleData[index + 1]) / 2);
					index++;
				}

				modChan.SampleData[index] = (sbyte)((modChan.SampleData[index] + modChan.SampleData[modChan.LoopStart]) / 2);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0xE9 - Retrigs the current note
		/// </summary>
		/********************************************************************/
		private void RetrigNote(IChannel chan, ModChannel modChan)
		{
			byte arg = (byte)(modChan.TrackLine.EffectArg & 0x0f);

			if (arg != 0)
			{
				if ((playingInfo.Counter != 0) && (modChan.TrackLine.Note == 0))
				{
					if ((playingInfo.Counter % arg) == 0)
					{
						// Retrig the sample
						if (modChan.Length != 0)
						{
							chan.PlaySample(modChan.SampleNumber, modChan.SampleData, 0, (uint)modChan.Length * 2);

							if (modChan.LoopLength != 0)
								chan.SetLoop(modChan.LoopStart, (uint)modChan.LoopLength * 2);

							SetPeriod(modChan.Period, chan, modChan);
						}
						else
							chan.Mute();
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0xEA - Fine slide the volume up
		/// </summary>
		/********************************************************************/
		private void VolumeFineUp(ModChannel modChan)
		{
			if (playingInfo.Counter == 0)
			{
				modChan.Volume += (sbyte)(modChan.TrackLine.EffectArg & 0x0f);
				if (modChan.Volume > 64)
					modChan.Volume = 64;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0xEB - Fine slide the volume down
		/// </summary>
		/********************************************************************/
		private void VolumeFineDown(ModChannel modChan)
		{
			if (playingInfo.Counter == 0)
			{
				modChan.Volume -= (sbyte)(modChan.TrackLine.EffectArg & 0x0f);
				if (modChan.Volume < 0)
					modChan.Volume = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0xEC - Stops the current note from playing
		/// </summary>
		/********************************************************************/
		private void NoteCut(ModChannel modChan)
		{
			if ((modChan.TrackLine.EffectArg & 0x0f) == playingInfo.Counter)
				modChan.Volume = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 0xED - Waits a little while before playing
		/// </summary>
		/********************************************************************/
		private void NoteDelay(IChannel chan, ModChannel modChan)
		{
			if (((modChan.TrackLine.EffectArg & 0x0f) == playingInfo.Counter) && (modChan.TrackLine.Note != 0))
			{
				// Retrig the sample
				if (modChan.Length != 0)
				{
					chan.PlaySample(modChan.SampleNumber, modChan.SampleData, 0, (uint)modChan.Length * 2);

					if (modChan.LoopLength != 0)
						chan.SetLoop(modChan.LoopStart, (uint)modChan.LoopLength * 2);

					SetPeriod(modChan.Period, chan, modChan);
				}
				else
					chan.Mute();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 0xEE - Pauses the pattern for a little while
		/// </summary>
		/********************************************************************/
		private void PatternDelay(ModChannel modChan)
		{
			if ((playingInfo.Counter == 0) && (playingInfo.PattDelayTime2 == 0))
				playingInfo.PattDelayTime = (byte)((modChan.TrackLine.EffectArg & 0x0f) + 1);
		}



		/********************************************************************/
		/// <summary>
		/// 0xEF - Inverts the loop
		/// </summary>
		/********************************************************************/
		private void FunkIt(ModChannel modChan)
		{
			if (playingInfo.Counter == 0)
			{
				byte arg = (byte)((modChan.TrackLine.EffectArg & 0x0f) << 4);

				modChan.GlissFunk &= 0x0f;
				modChan.GlissFunk |= arg;

				if (arg != 0)
					UpdateFunk(modChan);
			}
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Will change the speed on the module
		/// </summary>
		/********************************************************************/
		private void ChangeSpeed(byte newSpeed)
		{
			if (newSpeed != playingInfo.LastShownSpeed)
			{
				// Remember the speed
				playingInfo.LastShownSpeed = newSpeed;

				// Change the module info
				ShowSpeed();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will change the tempo on the module
		/// </summary>
		/********************************************************************/
		private void ChangeTempo(byte newTempo)
		{
			if (newTempo != playingInfo.Tempo)
			{
				// BPM tempo
				SetBpmTempo(newTempo);

				// Remember the tempo
				playingInfo.Tempo = newTempo;

				// Change the module info
				ShowTempo();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current song position
		/// </summary>
		/********************************************************************/
		private void ShowSongPosition()
		{
			OnModuleInfoChanged(InfoPositionLine, playingInfo.SongPos.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with pattern number
		/// </summary>
		/********************************************************************/
		private void ShowPattern()
		{
			OnModuleInfoChanged(InfoPatternLine, showTracks ? FormatTracks() : positions[playingInfo.SongPos].ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current speed
		/// </summary>
		/********************************************************************/
		private void ShowSpeed()
		{
			OnModuleInfoChanged(InfoSpeedLine, FormatSpeed());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with tempo
		/// </summary>
		/********************************************************************/
		private void ShowTempo()
		{
			OnModuleInfoChanged(InfoTempoLine, playingInfo.Tempo.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with all dynamic values
		/// </summary>
		/********************************************************************/
		private void UpdateModuleInformation()
		{
			ShowSongPosition();
			ShowPattern();
			ShowSpeed();
			ShowTempo();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing tracks
		/// </summary>
		/********************************************************************/
		private string FormatTracks()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < channelNum; i++)
			{
				sb.Append(sequences[i, playingInfo.SongPos]);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the current speed
		/// </summary>
		/********************************************************************/
		private string FormatSpeed()
		{
			return ((playingInfo.PatternPos & 1) == 0 ? playingInfo.SpeedEven : playingInfo.SpeedOdd).ToString();
		}
		#endregion
	}
}
