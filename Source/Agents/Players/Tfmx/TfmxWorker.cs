/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class TfmxWorker : ModulePlayerWithSubSongDurationAgentBase
	{
		private class SampleRange : IComparable<SampleRange>
		{
			public int Start;
			public int End;
			public int LoopStart;
			public int LoopLength;

			public int CompareTo(SampleRange other)
			{
				return Start.CompareTo(other.Start);
			}
		}

		private const int BufSize = 1024;

		private readonly ModuleType currentModuleType;
		private readonly bool isLittleEndian;

		private int currentSong;

		private string[] comment;

		private ushort[] songStart;
		private ushort[] songEnd;
		private ushort[] tempo;

		private int trackStart;
		private int patternStart;
		private int macroStart;

		private int numMacro;
		private int numPattern;
		private int numTrackSteps;

		private byte[] musicData;
		private sbyte[] sampleData;

		private int musicLen;
		private int sampleLen;

		private bool gemx;
		private bool dangerFreakHack;

		private int startPat;

		private int[][] tBuf;
		private short[][] outBuf;

		private int numBytes;
		private int bytesDone;

		private bool firstTime;

		private GlobalPlayingInfo playingInfo;

		private bool endReached;

		private SampleInfo[] sampleInfo;

		private const int InfoPositionLine = 3;
		private const int InfoTrackLine = 4;
		private const int InfoSpeedLine = 5;
		private const int InfoTempoLine = 6;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TfmxWorker(ModuleType moduleType = ModuleType.Unknown)
		{
			currentModuleType = moduleType;

			isLittleEndian = BitConverter.IsLittleEndian;
		}

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => Tfmx.fileExtensions;



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
		/// Return the comment separated in lines
		/// </summary>
		/********************************************************************/
		public override string[] Comment => comment;



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
					description = Resources.IDS_TFMX_INFODESCLINE0;
					value = FormatPositionLength();
					break;
				}

				// Used tracks
				case 1:
				{
					description = Resources.IDS_TFMX_INFODESCLINE1;
					value = numPattern.ToString();
					break;
				}

				// Used macros
				case 2:
				{
					description = Resources.IDS_TFMX_INFODESCLINE2;
					value = numMacro.ToString();
					break;
				}

				// Playing position
				case 3:
				{
					description = Resources.IDS_TFMX_INFODESCLINE3;
					value = FormatPosition();
					break;
				}

				// Playing tracks
				case 4:
				{
					description = Resources.IDS_TFMX_INFODESCLINE4;
					value = FormatTracks();
					break;
				}

				// Current speed
				case 5:
				{
					description = Resources.IDS_TFMX_INFODESCLINE5;
					value = (playingInfo.Pdb.PreScale + 1).ToString();
					break;
				}

				// Current tempo (BPM)
				case 6:
				{
					description = Resources.IDS_TFMX_INFODESCLINE6;
					value = (0x1b51f8 / playingInfo.Mdb.CiaSave).ToString();
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
		/// Return some flags telling what the player supports
		/// </summary>
		/********************************************************************/
		public override ModulePlayerSupportFlag SupportFlags => base.SupportFlags | ModulePlayerSupportFlag.BufferMode;



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
				Encoding encoder = EncoderCollection.Amiga;

				OneFile header = IsOneFile(moduleStream);
				int startOffset = header != null ? (int)header.HeaderSize : 0;

				// Skip the mark and other stuff
				moduleStream.Seek(startOffset + 16, SeekOrigin.Begin);

				// Read the comment block
				comment = moduleStream.ReadCommentBlock(6 * 40, 40, encoder);

				// Read the sub-song information
				songStart = new ushort[32];
				songEnd = new ushort[32];
				tempo = new ushort[32];

				moduleStream.ReadArray_B_UINT16s(songStart, 0, 32);
				moduleStream.ReadArray_B_UINT16s(songEnd, 0, 32);
				moduleStream.ReadArray_B_UINT16s(tempo, 0, 32);

				// Get the start offset to the module information
				moduleStream.Seek(16, SeekOrigin.Current);

				trackStart = (int)moduleStream.Read_B_UINT32();
				patternStart = (int)moduleStream.Read_B_UINT32();
				macroStart = (int)moduleStream.Read_B_UINT32();

				if (trackStart == 0)
					trackStart = 0x600;
				else
					trackStart -= 0x200;

				if (patternStart == 0)
					patternStart = 0x200;
				else
					patternStart -= 0x200;

				if (macroStart == 0)
					macroStart = 0x400;
				else
					macroStart -= 0x200;

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_TFMX_ERR_LOADING_HEADER;
					Cleanup();

					return AgentResult.Error;
				}

				// Skip the last part of the header
				moduleStream.Seek(36, SeekOrigin.Current);

				// Now read the rest of the file
				if (header != null)
					musicLen = (int)header.MdatSize;
				else
					musicLen = (int)(moduleStream.Length - moduleStream.Position);

				musicData = new byte[16384 * 4];
				moduleStream.Read(musicData, 0, musicLen);

				int offset = ((musicLen + 3) / 4) * 4;
				musicData[offset] = 0xff;
				musicData[offset + 1] = 0xff;
				musicData[offset + 2] = 0xff;
				musicData[offset + 3] = 0xff;

				// Now calculate a MD5 checksum on the rest of the file,
				// just to find out if it is a special module that needs
				// to be take special care of
				using (MD5 md5 = MD5.Create())
				{
					byte[] checksum = md5.ComputeHash(musicData, 0, musicLen);

					// Check the checksum
					dangerFreakHack = checksum.SequenceEqual(Tables.DangerFreakTitle);
					gemx = checksum.SequenceEqual(Tables.GemXTitle);
				}

				// Now that we have pointers to almost everything, this would be a good
				// time to fix everything we can... fix endianess on track steps,
				// convert pointers to array indices and endianess the patterns and
				// instrument macros. We fix the macros first, then the patterns, and
				// then the track steps (because we have to know when the patterns
				// begin to know the track steps end...)
				int x, z;
				for (x = 0, z = macroStart; x < 128; x++, z += 4)
				{
					int y = ToInt32(z) - 0x200;
					if (((y & 3) != 0) || (y > musicLen))		// Probably not strictly right
						break;

					StoreInt32(z, y);
				}

				numMacro = x;

				// Endianess the pattern pointers
				numPattern = Math.Min((macroStart - patternStart) >> 2, 128);

				for (x = 0, z = patternStart; x < numPattern; x++, z += 4)
				{
					int y = ToInt32(z) - 0x200;
					if (y > musicLen)
						break;

					StoreInt32(z, y);
				}

				// Endianess the track step data
				int lg = BitConverter.ToInt32(musicData, patternStart);
				int sh = trackStart;
				numTrackSteps = (lg - sh) >> 4;

				while (sh < lg)
				{
					short y = ToInt16(sh);
					StoreInt16(sh, y);

					sh += 2;
				}

				// Now the song is fully loaded, except for the sample data.
				// Everything is done but fixing endianess on the actual
				// pattern and macro data. The routines that use the data do
				// it for themselves
				if (header != null)
				{
					// Okay, we need to load the sample data here
					sampleLen = (int)header.SmplSize;

					// Read the sample data
					moduleStream.Seek(header.HeaderSize + header.MdatSize, SeekOrigin.Begin);
					sampleData = moduleStream.ReadSampleData(0, sampleLen, out int readBytes);
					if (readBytes != sampleLen)
					{
						errorMessage = Resources.IDS_TFMX_ERR_LOADING_SAMPLE;
						Cleanup();

						return AgentResult.Error;
					}
				}
				else
				{
					// Two file format. Find the other file and read the samples from it
					ModuleStream sampleStream = fileInfo.Loader?.OpenExtraFileByExtension("smpl");
					if (sampleStream == null)
						sampleStream = fileInfo.Loader?.OpenExtraFileByExtension("sam");

					if (sampleStream == null)
					{
						errorMessage = Resources.IDS_TFMX_ERR_LOADING_SAMPLE;
						Cleanup();

						return AgentResult.Error;
					}

					try
					{
						// Read the samples
						sampleLen = (int)sampleStream.Length;
						sampleData = sampleStream.ReadSampleData(0, sampleLen, out _);
					}
					finally
					{
						sampleStream.Dispose();
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
			if (!base.InitPlayer(out errorMessage))
				return false;

			playingInfo = new GlobalPlayingInfo
			{
				Hdb = ArrayHelper.InitializeArray<Hdb>(8),
				Mdb = new Mdb(),
				Cdb = ArrayHelper.InitializeArray<Cdb>(16),
				Pdb = new PdBlk(),
				Idb = new Idb()
			};

			// Initialize some of the member variables
			playingInfo.EClocks = 14318;

			if (currentModuleType == ModuleType.Tfmx7V)
				playingInfo.MultiMode = true;
			else
				playingInfo.MultiMode = false;

			// Allocate mixer buffers
			tBuf = new int[7][];
			outBuf = new short[7][];

			for (int i = 0; i < 4; i++)
			{
				tBuf[i] = new int[BufSize];
				outBuf[i] = new short[BufSize];
			}

			if (playingInfo.MultiMode)
			{
				for (int i = 4; i < 7; i++)
				{
					tBuf[i] = new int[BufSize];
					outBuf[i] = new short[BufSize];
				}
			}

			numBytes = 0;
			bytesDone = 0;

			ExtractSampleInfo();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public override void CleanupPlayer()
		{
			tBuf = null;
			outBuf = null;

			playingInfo = null;

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



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			bool stop = false;

			while (bytesDone < BufSize)
			{
				while (numBytes > 0)
				{
					int n = BufSize - bytesDone;

					if (n > numBytes)
						n = numBytes;

					MixIt(n, bytesDone);

					bytesDone += n;
					numBytes -= n;

					if (bytesDone == BufSize)
					{
						stop = true;
						break;
					}
				}

				if (stop)
					break;

				if (playingInfo.Mdb.PlayerEnable)
				{
					DoAllMacros();

					if (currentSong >= 0)
						DoTracks();

					numBytes = (int)(playingInfo.EClocks * (mixerFreq >> 1));
					playingInfo.ERem += (numBytes % 357955);
					numBytes /= 357955;

					if (playingInfo.ERem > 357955)
					{
						numBytes++;
						playingInfo.ERem -= 357955;
					}
				}
				else
					bytesDone = BufSize;
			}

			Conv16(tBuf[0], outBuf[0], bytesDone);
			Conv16(tBuf[1], outBuf[1], bytesDone);
			Conv16(tBuf[2], outBuf[2], bytesDone);
			Conv16(tBuf[3], outBuf[3], bytesDone);

			if (playingInfo.MultiMode)
			{
				Conv16(tBuf[4], outBuf[4], bytesDone);
				Conv16(tBuf[5], outBuf[5], bytesDone);
				Conv16(tBuf[6], outBuf[6], bytesDone);
			}

			bytesDone = 0;

			// Tell NostalgicPlayer what to play
			SetupChannel(0);
			SetupChannel(1);
			SetupChannel(2);
			SetupChannel(3);

			if (playingInfo.MultiMode)
			{
				SetupChannel(4);
				SetupChannel(5);
				SetupChannel(6);
			}

			if (endReached)
			{
				OnEndReachedOnAllChannels(playingInfo.Pdb.FirstPos);
				endReached = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public override int ModuleChannelCount
		{
			get
			{
				if (currentModuleType == ModuleType.Tfmx7V)
					return 7;

				return 4;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs
		{
			get
			{
				short songNum = -1;
				short maxNumberOfZero = 2;

				// Find the number of sub-songs
				for (int i = 0; (i < 32) && (maxNumberOfZero > 0); i++)
				{
					songNum++;
					if (songStart[i] == 0)
						maxNumberOfZero--;

					// Extra check added by Thomas Neumann
					if ((songStart[i] == 0x1ff) || (songEnd[i] == 0x1ff))
						break;

					// Check for Abandoned Places - Part1_2 and Part1_3.
					// There is only one sub-song. Added by Thomas Neumann
					if (songStart[i] >= numTrackSteps)
						break;

					if ((songStart[i] == songEnd[i]) && (songStart[i] == 0) && (songStart[i + 1] == 0))
						break;
				}

				int songCount = songNum == 0 ? 1 : songNum;

				return new SubSongInfo(songCount, 0);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Returns all the samples available in the module. If none, null
		/// is returned
		/// </summary>
		/********************************************************************/
		public override IEnumerable<SampleInfo> Samples => sampleInfo;
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
			InitializeSound(subSong);
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

		#region Identify methods
		/********************************************************************/
		/// <summary>
		/// Tests the module to see which type of module it is
		/// </summary>
		/********************************************************************/
		public static ModuleType TestModule(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 512)
				return ModuleType.Unknown;

			int startOffset = 0;

			// First check for one-file format
			OneFile header = IsOneFile(moduleStream);
			if (header != null)
			{
				// Check to see if the module is forced or not checked
				if (((header.Type & 128) != 0) || ((header.Type & 127) == 0))
				{
					// Well, we can't count on the type now, so we skip
					// the header and make our own check
					startOffset = (int)header.HeaderSize;
				}
				else
				{
					switch (header.Type & 127)
					{
						case 1:
							return ModuleType.Tfmx15;

						case 2:
							return ModuleType.TfmxPro;

						case 3:
							return ModuleType.Tfmx7V;
					}

					return ModuleType.Unknown;
				}
			}

			// Check for two-file format. Read the mark
			moduleStream.Seek(startOffset, SeekOrigin.Begin);

			uint mark1 = moduleStream.Read_B_UINT32();
			uint mark2 = moduleStream.Read_B_UINT32();
			byte mark3 = moduleStream.Read_UINT8();

			// And check it
			//
			// If the file starts with TFMX and does not have SONG, it's the old format
			if ((mark1 == 0x54464d58) && ((mark2 & 0x00ffffff) != 0x00534f4e) && (mark3 != 0x47))
				return ModuleType.Tfmx15;

			// TFMX-SONG / TFMX_SONG / tfmxsong
			if (((mark1 == 0x54464d58) && ((mark2 == 0x2d534f4e) || (mark2 == 0x5f534f4e)) && (mark3 == 0x47)) || ((mark1 == 0x74666d78) && (mark2 == 0x736f6e67)))
			{
				// Okay, it is either a professional or 7 voices, so check for the difference
				ushort[] songStarts = new ushort[31];

				// Get the start positions
				moduleStream.Seek(startOffset + 0x100, SeekOrigin.Begin);
				moduleStream.ReadArray_B_UINT16s(songStarts, 0, 31);

				// Get the track step offset
				moduleStream.Seek(startOffset + 0x1d0, SeekOrigin.Begin);
				uint offset = moduleStream.Read_B_UINT32();
				if (offset == 0)
					offset = 0x800;

				// Take all the sub-songs
				short times = 0;
				bool gotTimeShare = false;

				for (int i = 0; i < 31; i++)
				{
					bool getNext = true;

					// Get the current sub-song start position
					ushort position = songStarts[i];
					if (position == 0x1ff)
						break;

					// Read the track step information
					while (getNext)
					{
						// Find the position in the file where the current track step
						// information to read is stored
						moduleStream.Seek(startOffset + offset + position * 16, SeekOrigin.Begin);

						// If the track step information isn't a command, stop
						// the checking
						if (moduleStream.Read_B_UINT16() != 0xeffe)
							getNext = false;
						else
						{
							// Get the command
							switch (moduleStream.Read_B_UINT16())
							{
								// Loop a section
								case 1:
								{
									if (times == 0)
									{
										times = -1;
										position++;
									}
									else
									{
										if (times < 0)
										{
											position = moduleStream.Read_B_UINT16();
											times = (short)(moduleStream.Read_B_UINT16() - 1);
										}
										else
										{
											times--;
											position = moduleStream.Read_B_UINT16();
										}
									}
									break;
								}

								// Set tempo + start master volume slide
								case 2:
								case 4:
								{
									position++;
									break;
								}

								// Time share
								case 3:
								{
									gotTimeShare = true;
									position++;
									break;
								}

								// Unknown command
								default:
								{
									getNext = false;
									gotTimeShare = false;
									break;
								}
							}
						}
					}

					if (gotTimeShare)
						break;
				}

				return gotTimeShare ? ModuleType.Tfmx7V : ModuleType.TfmxPro;
			}

			return ModuleType.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Will check the current file to see if it's in one-file format.
		/// If that is true, it will load the structure
		/// </summary>
		/********************************************************************/
		private static OneFile IsOneFile(ModuleStream moduleStream)
		{
			// Seek to the start of the file
			moduleStream.Seek(0, SeekOrigin.Begin);

			// Check the mark
			OneFile oneFile = new OneFile();
			oneFile.Mark = moduleStream.Read_B_UINT32();

			if (oneFile.Mark == 0x54464844)		// TFHD
			{
				// Ok, it seems it's a one-file, so read the whole structure
				oneFile.HeaderSize = moduleStream.Read_B_UINT32();
				oneFile.Type = moduleStream.Read_UINT8();
				oneFile.Version = moduleStream.Read_UINT8();
				oneFile.MdatSize = moduleStream.Read_B_UINT32();
				oneFile.SmplSize = moduleStream.Read_B_UINT32();

				return oneFile;
			}

			return null;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Convert the int at the given position to host endian format and
		/// return it
		/// </summary>
		/********************************************************************/
		private short ToInt16(int index)
		{
			return (short)((musicData[index] << 8) | musicData[index + 1]);
		}



		/********************************************************************/
		/// <summary>
		/// Convert the int at the given position to host endian format and
		/// return it
		/// </summary>
		/********************************************************************/
		private int ToInt32(int index)
		{
			return (musicData[index] << 24) | (musicData[index + 1] << 16) | (musicData[index + 2] << 8) | musicData[index + 3];
		}



		/********************************************************************/
		/// <summary>
		/// Store the given int at the given index in host format
		/// </summary>
		/********************************************************************/
		private void StoreInt16(int index, short value)
		{
			if (isLittleEndian)
			{
				musicData[index] = (byte)(value & 0xff);
				musicData[index + 1] = (byte)((value & 0xff00) >> 8);
			}
			else
			{
				musicData[index] = (byte)((value & 0xff00) >> 8);
				musicData[index + 1] = (byte)(value & 0xff);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Store the given int at the given index in host format
		/// </summary>
		/********************************************************************/
		private void StoreInt32(int index, int value)
		{
			if (isLittleEndian)
			{
				musicData[index] = (byte)(value & 0xff);
				musicData[index + 1] = (byte)((value & 0xff00) >> 8);
				musicData[index + 2] = (byte)((value & 0xff0000) >> 16);
				musicData[index + 3] = (byte)((value & 0xff000000) >> 24);
			}
			else
			{
				musicData[index] = (byte)((value & 0xff000000) >> 24);
				musicData[index + 1] = (byte)((value & 0xff0000) >> 16);
				musicData[index + 2] = (byte)((value & 0xff00) >> 8);
				musicData[index + 3] = (byte)(value & 0xff);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int songNumber)
		{
			// Initialize member variables
			currentSong = songNumber;
			firstTime = true;

			playingInfo.Loops = 0;		// Infinity loop on the modules
			playingInfo.ERem = 0;

			playingInfo.LastTrackPlayed = new int[ModuleChannelCount];

			startPat = -1;

			endReached = false;

			// Initialize the player
			TfmxInit();
			StartSong(songNumber, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			// Delete the module
			musicData = null;

			// Other structures
			songStart = null;
			songEnd = null;
			tempo = null;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the player so it's ready to roll
		/// </summary>
		/********************************************************************/
		private void TfmxInit()
		{
			AllOff();

			for (int x = 0; x < 8; x++)
			{
				playingInfo.Hdb[x].C = playingInfo.Cdb[x];
				playingInfo.Pdb.p[x].PNum = 0xff;
				playingInfo.Pdb.p[x].PAddr = 0;

				ChannelOff(x);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the player with the sub-song given
		/// </summary>
		/********************************************************************/
		private void StartSong(int song, int mode)
		{
			playingInfo.Mdb.PlayerEnable = false;		// Sort of locking mechanism

			playingInfo.Mdb.MasterVol = 0x40;
			playingInfo.Mdb.FadeSlope = 0;
			playingInfo.Mdb.TrackLoop = -1;
			playingInfo.Mdb.PlayPattFlag = false;
			playingInfo.Mdb.CiaSave = 14318;			// Assume 125 BPM, NTSC timing

			if (mode != 2)
			{
				playingInfo.Pdb.CurrPos = playingInfo.Pdb.FirstPos = songStart[song];
				playingInfo.Pdb.LastPos = songEnd[song];

				ushort x = tempo[song];
				if (x >= 0x10)
				{
					playingInfo.Mdb.CiaSave = (ushort)(0x1b51f8 / x);
					playingInfo.Pdb.PreScale = 5;		// Fix by Thomas Neumann
				}
				else
					playingInfo.Pdb.PreScale = x;
			}

			playingInfo.EClocks = playingInfo.Mdb.CiaSave;

			for (int x = 0; x < 8; x++)
			{
				playingInfo.Pdb.p[x].PAddr = 0;
				playingInfo.Pdb.p[x].PNum = 0xff;
				playingInfo.Pdb.p[x].PxPose = 0;
				playingInfo.Pdb.p[x].PStep = 0;
			}

			if (mode != 2)
				GetTrackStep();

			if (startPat != -1)
			{
				playingInfo.Pdb.CurrPos = playingInfo.Pdb.FirstPos = (ushort)startPat;
				GetTrackStep();
				startPat = -1;
			}

			playingInfo.Mdb.SpeedCnt = 0;
			playingInfo.Mdb.EndFlag = false;

			playingInfo.Mdb.PlayerEnable = true;
		}



		/********************************************************************/
		/// <summary>
		/// Setup the note period
		/// </summary>
		/********************************************************************/
		private void NotePort(uint i)
		{
			Uni x = new Uni(i);
			Cdb c = playingInfo.Cdb[x.b2 & (playingInfo.MultiMode ? 7 : 3)];

			if (x.b0 == 0xfc)
			{
				// Lock
				c.SfxFlag = x.b1;
				c.SfxLockTime = x.b3;
				return;
			}

			if (c.SfxFlag != 0)
				return;

			if (x.b0 < 0xc0)
			{
				if (x.b1 >= numMacro)
				{
					c.MacroRun = 0;
					return;
				}

				if (!dangerFreakHack)
					c.FineTune = x.b3;
				else
					c.FineTune = 0;

				c.Velocity = (byte)((x.b2 >> 4) & 0xf);
				c.PrevNote = c.CurrNote;
				c.CurrNote = x.b0;
				c.ReallyWait = 1;
				c.NewStyleMacro = 0xff;
				c.MacroNum = x.b1;
				c.MacroPtr = (uint)BitConverter.ToInt32(musicData, macroStart + c.MacroNum * 4);
				c.MacroStep = c.MacroWait = 0;
				c.EfxRun = 0;
				c.KeyUp = true;
				c.Loop = -1;
				c.MacroRun = -1;
			}
			else
			{
				if (x.b0 < 0xf0)
				{
					c.PortaReset = x.b1;
					c.PortaTime = 1;

					if (c.PortaRate == 0)
						c.PortaPer = c.DestPeriod;

					c.PortaRate = x.b3;
					c.DestPeriod = (ushort)(Tables.NoteVals[c.CurrNote = (byte)(x.b0 & 0x3f)]);
				}
				else
				{
					switch (x.b0)
					{
						// Enve
						case 0xf7:
						{
							c.EnvRate = x.b1;
							c.EnvReset = c.EnvTime = (byte)((x.b2 >> 4) + 1);
							c.EnvEndVol = (sbyte)x.b3;
							break;
						}

						// Vibr
						case 0xf6:
						{
							c.VibTime = (byte)((c.VibReset = (byte)(x.b1 & 0xfe)) >> 1);
							c.VibWidth = (sbyte)x.b3;
							c.VibFlag = true;
							c.VibOffset = 0;
							break;
						}

						// Kup^
						case 0xf5:
						{
							c.KeyUp = false;
							break;
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Doesn't do anything
		/// </summary>
		/********************************************************************/
		private bool LoopOff(Hdb hw)
		{
			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Turn on loop
		/// </summary>
		/********************************************************************/
		private bool LoopOn(Hdb hw)
		{
			if (hw.C == null)
				return true;

			if (hw.C.WaitDmaCount-- != 0)
				return true;

			hw.Loop = LoopOff;
			hw.C.MacroRun = -1;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Runs a macro
		/// </summary>
		/********************************************************************/
		private void RunMacro(Cdb c)
		{
			c.MacroWait = 0;

			for (;;)
			{
				Uni x = new Uni((uint)ToInt32((int)c.MacroPtr + c.MacroStep * 4));
				c.MacroStep++;

				int a = x.b0;
				x.b0 = 0;

				switch (a)
				{
					// DMA off + reset
					case 0:
					{
						c.EnvReset = c.VibReset = c.AddBeginTime = 0;
						c.PortaRate = 0;

						if (gemx)
						{
							if (x.b2 != 0)
								c.CurVol = (sbyte)x.b3;
							else
								c.CurVol = (sbyte)(x.b3 + c.Velocity * 3);
						}

						goto case 0x13;
					}

					// DMA off
					case 0x13:
					{
						c.hw.Loop = LoopOff;

						if (x.b1 == 0)
						{
							c.hw.Mode = 0;

							// Added by Stefan Ohlsson
							// Removes glitch in Turrican II World 2 Song 0, among others
							if (c.NewStyleMacro != 0)
								c.hw.SLen = 0;

							break;
						}

						c.hw.Mode |= 4;
						c.NewStyleMacro = 0;
						return;
					}

					// DMA on
					case 0x1:
					{
						c.EfxRun = (sbyte)x.b1;
						c.hw.Mode = 1;

						if ((c.NewStyleMacro == 0) || dangerFreakHack)
						{
							c.hw.SampleStart = (int)c.SaveAddr;
							c.hw.SampleLength = (ushort)(c.SaveLen != 0 ? c.SaveLen << 1 : 131072);
							c.hw.SBeg = c.hw.SampleStart;
							c.hw.SLen = c.hw.SampleLength;
							c.hw.Pos = 0;
							c.hw.Mode |= 2;
						}
						break;
					}

					// SetBegin
					case 0x2:
					{
						c.AddBeginTime = 0;
						c.SaveAddr = c.CurAddr = x.l;
						break;
					}

					// AddBegin
					case 0x11:
					{
						c.AddBeginTime = c.AddBeginReset = x.b1;
						a = (int)c.CurAddr + (c.AddBegin = (short)x.w1);

						c.SaveAddr = c.CurAddr = (uint)a;
						break;
					}

					// SetLen
					case 0x3:
					{
						c.SaveLen = c.CurrLength = x.w1;
						break;
					}

					// AddLen
					case 0x12:
					{
						c.CurrLength += x.w1;
						a = c.CurrLength;

						c.SaveLen = (ushort)a;
						break;
					}

					// Wait
					case 0x4:
					{
						if ((x.b1 & 0x01) != 0)
						{
							if (c.ReallyWait++ != 0)
								return;
						}

						c.MacroWait = x.w1;

						if (c.NewStyleMacro == 0x0)
						{
							c.NewStyleMacro = 0xff;
							break;
						}
						return;
					}

					// Wait on DMA
					case 0x1a:
					{
						c.hw.Loop = LoopOn;
						c.hw.C = c;
						c.WaitDmaCount = x.w1;
						c.MacroRun = 0;

						if (c.NewStyleMacro == 0x0)
						{
							c.NewStyleMacro = 0xff;
							break;
						}
						return;
					}

					// Note split
					case 0x1c:
					{
						if (c.CurrNote > x.b1)
							c.MacroStep = x.w1;

						break;
					}

					// Vol split
					case 0x1d:
					{
						if (c.CurVol > x.b1)
							c.MacroStep = x.w1;

						break;
					}

					// Loop key up
					case 0x10:
					{
						if (!c.KeyUp)
							break;

						goto case 0x5;
					}

					// Loop
					case 0x5:
					{
						if ((c.Loop--) == 0)
							break;

						if (c.Loop < 0)
							c.Loop = (short)(x.b1 - 1);

						c.MacroStep = x.w1;
						break;
					}

					// Stop
					case 0x7:
					{
						c.MacroRun = 0;
						return;
					}

					// Add volume
					case 0xd:
					{
						if (x.b2 != 0xfe)
						{
							// --- neofix ---
							sbyte tempVol = (sbyte)((c.Velocity * 3) + x.b3);
							if (tempVol > 0x40)
								c.CurVol = 0x40;
							else
								c.CurVol = tempVol;

							break;
						}

						// NOTSUPPORTED
						break;
					}

					// Set volume
					case 0xe:
					{
						if (x.b2 != 0xfe)
						{
							c.CurVol = (sbyte)x.b3;
							break;
						}

						// NOTSUPPORTED
						break;
					}

					// Start macro
					case 0x21:
					{
						x.b0 = c.CurrNote;
						x.b2 |= (byte)(c.Velocity << 4);

						NotePort(x.l);
						break;
					}

					// Set prev note
					case 0x1f:
					{
						a = c.PrevNote;

						a = (Tables.NoteVals[a + x.b1 & 0x3f] * (0x100 + c.FineTune + (sbyte)x.b3)) >> 8;
						c.DestPeriod = (ushort)a;

						if (c.PortaRate == 0)
							c.CurPeriod = (ushort)a;


						if (c.NewStyleMacro == 0x0)
						{
							c.NewStyleMacro = 0xff;
							break;
						}
						return;
					}

					// Add note
					case 0x8:
					{
						a = c.CurrNote;

						a = (Tables.NoteVals[a + x.b1 & 0x3f] * (0x100 + c.FineTune + (sbyte)x.b3)) >> 8;
						c.DestPeriod = (ushort)a;

						if (c.PortaRate == 0)
							c.CurPeriod = (ushort)a;

						if (c.NewStyleMacro == 0x0)
						{
							c.NewStyleMacro = 0xff;
							break;
						}
						return;
					}

					// Set note
					case 0x9:
					{
						a = 0;

						a = (Tables.NoteVals[a + x.b1 & 0x3f] * (0x100 + c.FineTune + (sbyte)x.b3)) >> 8;
						c.DestPeriod = (ushort)a;

						if (c.PortaRate == 0)
							c.CurPeriod = (ushort)a;


						if (c.NewStyleMacro == 0x0)
						{
							c.NewStyleMacro = 0xff;
							break;
						}
						return;
					}

					// Set period
					case 0x17:
					{
						c.DestPeriod = x.w1;

						if (c.PortaRate == 0)
							c.CurPeriod = x.w1;

						break;
					}

					// Portamento
					case 0xb:
					{
						c.PortaReset = x.b1;
						c.PortaTime = 1;

						if (c.PortaRate == 0)
							c.PortaPer = c.DestPeriod;

						c.PortaRate = (short)x.w1;
						break;
					}

					// Vibrato
					case 0xc:
					{
						c.VibTime = (byte)((c.VibReset = x.b1) >> 1);
						c.VibWidth = (sbyte)x.b3;
						c.VibFlag = true;

						if (c.PortaRate == 0)
						{
							c.CurPeriod = c.DestPeriod;
							c.VibOffset = 0;
						}
						break;
					}

					// Envelope
					case 0xf:
					{
						c.EnvReset = c.EnvTime = x.b2;
						c.EnvEndVol = (sbyte)x.b3;
						c.EnvRate = x.b1;
						break;
					}

					// Reset
					case 0xa:
					{
						c.EnvReset = c.VibReset = c.AddBeginTime = 0;
						c.PortaRate = 0;
						break;
					}

					// Wait key up
					case 0x14:
					{
						// Fix by Thomas Neumann so the "Master Blaster - load" don't crash
						if (!c.KeyUp)
							break;

						if (c.Loop == 0)
						{
							c.Loop = -1;
							break;
						}

						if (c.Loop == -1)
							c.Loop = (short)(x.b3 - 1);
						else
							c.Loop--;

						c.MacroStep--;
						return;
					}

					// Go sub
					case 0x15:
					{
						c.ReturnPtr = (ushort)c.MacroPtr;
						c.ReturnStep = c.MacroStep;
						goto case 0x6;
					}

					// Continue
					case 0x6:
					{
						c.MacroPtr = c.MacroNum = (ushort)BitConverter.ToInt32(musicData, macroStart + x.b1 * 4);
						c.MacroStep = x.w1;
						c.Loop = -1;
						break;
					}

					// Return sub
					case 0x16:
					{
						c.MacroPtr = c.ReturnPtr;
						c.MacroStep = c.ReturnStep;
						break;
					}

					// Sample loop
					case 0x18:
					{
						c.SaveAddr += (uint)(x.w1 & 0xfffe);
						c.SaveLen -= (ushort)(x.w1 >> 1);
						c.CurrLength = c.SaveLen;
						c.CurAddr = c.SaveAddr;
						break;
					}

					// One shot
					case 0x19:
					{
						c.AddBeginTime = 0;
						c.SaveAddr = c.CurAddr = 0;
						c.SaveLen = c.CurrLength = 1;
						break;
					}

					// Cue
					case 0x20:
					{
						playingInfo.Idb.Cue[x.b1 & 0x03] = x.w1;
						break;
					}

					// Turrican 3 title - we can safely ignore
					case 0x31:
						break;

					default:
					{
						// NOTSUPPORTED
						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will run the run-always effects, like vibrato and portamento
		/// </summary>
		/********************************************************************/
		private void DoEffects(Cdb c)
		{
			int a = 0;

			if (c.EfxRun < 0)
				return;

			if (c.EfxRun == 0)
			{
				c.EfxRun = 1;
				return;
			}

			if (c.AddBeginTime != 0)
			{
				c.CurAddr += (uint)c.AddBegin;
				c.SaveAddr = c.CurAddr;
				c.AddBeginTime--;

				if (c.AddBeginTime == 0)
				{
					c.AddBegin = -c.AddBegin;
					c.AddBeginTime = c.AddBeginReset;
				}
			}

			if ((int)c.CurAddr < 0)
			{
				c.AddBegin = 0;
				c.AddBeginTime = c.AddBeginReset = 0;
				c.CurAddr = 0;
			}

			if (c.VibReset != 0)
			{
				a = (c.VibOffset += c.VibWidth);
				a = (c.DestPeriod * (0x800 + a)) >> 11;

				if (c.PortaRate == 0)
					c.CurPeriod = (ushort)a;

				if ((--c.VibTime) == 0)
				{
					c.VibTime = c.VibReset;
					c.VibWidth = (sbyte)-c.VibWidth;
				}
			}

			if ((c.PortaRate != 0) && ((--c.PortaTime) == 0))
			{
				c.PortaTime = c.PortaReset;

				if (c.PortaPer > c.DestPeriod)
				{
					a = (c.PortaPer * (256 - c.PortaRate) - 128) >> 8;

					if (a <= c.DestPeriod)
						c.PortaRate = 0;
				}
				else
				{
					if (c.PortaPer < c.DestPeriod)
					{
						a = (c.PortaPer * (256 + c.PortaRate)) >> 8;

						if (a >= c.DestPeriod)
							c.PortaRate = 0;
					}
					else
						c.PortaRate = 0;
				}

				if (c.PortaRate == 0)
					a = c.DestPeriod;

				c.PortaPer = c.CurPeriod = (ushort)a;
			}

			if ((c.EnvReset != 0) && ((c.EnvTime--) == 0))
			{
				c.EnvTime = c.EnvReset;

				if (c.CurVol > c.EnvEndVol)
				{
					if (c.CurVol < c.EnvRate)
						c.EnvReset = 0;
					else
						c.CurVol -= (sbyte)c.EnvRate;

					if (c.EnvEndVol > c.CurVol)
						c.EnvReset = 0;
				}
				else
				{
					if (c.CurVol < c.EnvEndVol)
					{
						c.CurVol += (sbyte)c.EnvRate;

						if (c.EnvEndVol < c.CurVol)
							c.EnvReset = 0;
					}
				}

				if (c.EnvReset == 0)
				{
					c.EnvReset = c.EnvTime = 0;
					c.CurVol = c.EnvEndVol;
				}
			}

			if ((playingInfo.Mdb.FadeSlope != 0) && ((--playingInfo.Mdb.FadeTime) == 0))
			{
				playingInfo.Mdb.FadeTime = playingInfo.Mdb.FadeReset;
				playingInfo.Mdb.MasterVol += playingInfo.Mdb.FadeSlope;

				if (playingInfo.Mdb.FadeDest == playingInfo.Mdb.MasterVol)
					playingInfo.Mdb.FadeSlope = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Run the macro on one channel
		/// </summary>
		/********************************************************************/
		private void DoMacro(int cc)
		{
			Cdb c = playingInfo.Cdb[cc];

			// Locking
			if (c.SfxLockTime >= 0)
				c.SfxLockTime--;
			else
				c.SfxFlag = c.SfxPriority = 0;

			int a = (int)c.SfxCode;
			if (a != 0)
			{
				c.SfxFlag = 0;
				c.SfxCode = 0;

				NotePort((uint)a);

				c.SfxCode = c.SfxPriority;
			}

			if ((c.MacroRun != 0) && ((c.MacroWait--) == 0))
				RunMacro(c);

			DoEffects(c);

			// Has to be here because of if (efxRun = 1)
			c.hw.Delta = c.CurPeriod != 0 ? (3579545 << 9) / (c.CurPeriod * mixerFreq >> 5) : 0;
			c.hw.SampleStart = (int)c.SaveAddr;
			c.hw.SampleLength = (ushort)(c.SaveLen != 0 ? c.SaveLen << 1 : 131072);

			if ((c.hw.Mode & 3) == 1)
			{
				c.hw.SBeg = c.hw.SampleStart;
				c.hw.SLen = c.hw.SampleLength;
			}

			c.hw.Vol = (byte)((c.CurVol * playingInfo.Mdb.MasterVol) >> 6);
		}



		/********************************************************************/
		/// <summary>
		/// Runs the macros for each voice
		/// </summary>
		/********************************************************************/
		private void DoAllMacros()
		{
			DoMacro(0);
			DoMacro(1);
			DoMacro(2);

			if (playingInfo.MultiMode)
			{
				DoMacro(4);
				DoMacro(5);
				DoMacro(6);
				DoMacro(7);
			}

			DoMacro(3);
		}



		/********************************************************************/
		/// <summary>
		/// Turns off a single channel
		/// </summary>
		/********************************************************************/
		private void ChannelOff(int i)
		{
			Cdb c = playingInfo.Cdb[i & 0xf];
			if (c.SfxFlag == 0)
			{
				c.hw.Mode = 0;
				c.AddBeginTime = c.AddBeginReset = 0;
				c.MacroRun = 0;
				c.NewStyleMacro = 0xff;
				c.SaveAddr = 0;
				c.CurVol = 0;
				c.hw.Vol = 0;
				c.SaveLen = c.CurrLength = 1;
				c.hw.Loop = LoopOff;
				c.hw.C = c;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Fades the volume
		/// </summary>
		/********************************************************************/
		private void DoFade(sbyte sp, sbyte dv)
		{
			playingInfo.Mdb.FadeDest = dv;

			if (((playingInfo.Mdb.FadeTime = playingInfo.Mdb.FadeReset = sp) == 0) || (playingInfo.Mdb.MasterVol == sp))
			{
				playingInfo.Mdb.MasterVol = dv;
				playingInfo.Mdb.FadeSlope = 0;
				return;
			}

			playingInfo.Mdb.FadeSlope = (sbyte)((playingInfo.Mdb.MasterVol > playingInfo.Mdb.FadeDest) ? -1 : 1);
		}



		/********************************************************************/
		/// <summary>
		/// Parses a track step
		/// </summary>
		/********************************************************************/
		private void GetTrackStep()
		{
			for (;;)
			{
				// Check for "end of module"
				if (playingInfo.Pdb.CurrPos == playingInfo.Pdb.FirstPos)
				{
					if (firstTime)
						firstTime = false;
					else
						endReached = true;
				}

				int l = trackStart + (playingInfo.Pdb.CurrPos * 16);

				if (BitConverter.ToUInt16(musicData, l) == 0xeffe)
				{
					switch (BitConverter.ToUInt16(musicData, l + 1 * 2))
					{
						// Stop
						case 0:
						{
							endReached = true;
							return;
						}

						// Loop
						case 1:
						{
							if (playingInfo.Loops != 0)
							{
								if ((--playingInfo.Loops) == 0)
								{
									endReached = true;
									return;
								}
							}

							if ((playingInfo.Mdb.TrackLoop--) == 0)
							{
								playingInfo.Mdb.TrackLoop = -1;
								playingInfo.Pdb.CurrPos++;

								ShowSongPosition();
								ShowTracks();
								break;
							}

							if (playingInfo.Mdb.TrackLoop < 0)
							{
								playingInfo.Mdb.TrackLoop = BitConverter.ToInt16(musicData, l + 3 * 2);
								if ((playingInfo.Mdb.TrackLoop == 0) || (playingInfo.Mdb.TrackLoop > 1000))		// Fix for Rames - Title song 0
									endReached = true;
							}

							// If we jump back, the module has probably ended
							ushort l2 = BitConverter.ToUInt16(musicData, l + 2 * 2);

							// Fix for PP_Hammer - Ingame song 8 by Thomas Neumann
							if (l2 < playingInfo.Pdb.FirstPos)
								l2 = playingInfo.Pdb.FirstPos;

							if ((l2 < playingInfo.Pdb.CurrPos) && (playingInfo.Mdb.TrackLoop < 0))
								endReached = true;

							playingInfo.Pdb.CurrPos = l2;

							ShowSongPosition();
							ShowTracks();
							break;
						}

						// Speed
						case 2:
						{
							playingInfo.Mdb.SpeedCnt = playingInfo.Pdb.PreScale = BitConverter.ToUInt16(musicData, l + 2 * 2);

							ShowSpeed();

							int x;
							ushort l3 = BitConverter.ToUInt16(musicData, l + 3 * 2);
							if (((l3 & 0xf200) == 0) && ((x = (l3 & 0x1ff)) > 0xf))
							{
								playingInfo.Mdb.CiaSave = (ushort)(0x1b51f8 / x);
								playingInfo.EClocks = playingInfo.Mdb.CiaSave;

								ShowTempo();
							}

							playingInfo.Pdb.CurrPos++;

							ShowSongPosition();
							ShowTracks();
							break;
						}

						// Time share
						case 3:
						{
							int x = BitConverter.ToUInt16(musicData, l + 3 * 2);
							if ((x & 0x8000) == 0)
							{
								sbyte xx = (sbyte)x;
								xx = xx < -0x20 ? (sbyte)-0x20 : xx;
								playingInfo.Mdb.CiaSave = (ushort)((14318 * (xx + 100)) / 100);
								playingInfo.EClocks = playingInfo.Mdb.CiaSave;
								playingInfo.MultiMode = true;

								ShowTempo();
							}

							playingInfo.Pdb.CurrPos++;

							ShowSongPosition();
							ShowTracks();
							break;
						}

						// Fade
						case 4:
						{
							ushort l2 = BitConverter.ToUInt16(musicData, l + 2 * 2);
							ushort l3 = BitConverter.ToUInt16(musicData, l + 3 * 2);

							DoFade((sbyte)(l2 & 0xff), (sbyte)(l3 & 0xff));
							playingInfo.Pdb.CurrPos++;

							ShowSongPosition();
							ShowTracks();
							break;
						}

						default:
						{
							playingInfo.Pdb.CurrPos++;

							ShowSongPosition();
							ShowTracks();
							break;
						}
					}
				}
				else
				{
					for (int x = 0; x < 8; x++)
					{
						ushort lv = BitConverter.ToUInt16(musicData, l + x * 2);

						playingInfo.Pdb.p[x].PxPose = (sbyte)(lv & 0xff);

						byte y = playingInfo.Pdb.p[x].PNum = (byte)(lv >> 8);
						if (y < 0x80)
						{
							playingInfo.Pdb.p[x].PStep = 0;
							playingInfo.Pdb.p[x].PWait = 0;
							playingInfo.Pdb.p[x].PLoop = 0xffff;
							playingInfo.Pdb.p[x].PAddr = BitConverter.ToUInt32(musicData, patternStart + y * 4);
							playingInfo.Pdb.p[x].Looped = false;
						}
					}
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse one single track
		/// </summary>
		/********************************************************************/
		private bool DoTrack(Pdb[] p1, Pdb p, bool simulating)
		{
			if (p.PNum == 0xfe)
			{
				p.PNum++;

				if (!simulating)
					ChannelOff(p.PxPose);
			
				return false;
			}

			if (p.PAddr == 0)
				return false;

			if (p.PNum >= 0x90)
				return false;

			if (p.PWait-- != 0)
				return false;

			for (;;)
			{
				Uni x = new Uni((uint)ToInt32((int)p.PAddr + p.PStep * 4));
				p.PStep++;
				byte t = x.b0;

				if (t < 0xf0)
				{
					if ((t & 0xc0) == 0x80)
					{
						p.PWait = x.b3;
						x.b3 = 0;
					}

					x.b0 = (byte)((t + p.PxPose) & 0x3f);

					if ((t & 0xc0) == 0xc0)
						x.b0 |= 0xc0;

					if (!simulating)
						NotePort(x.l);

					if ((t & 0xc0) == 0x80)
						return false;
				}
				else
				{
					switch (t & 0xf)
					{
						// NOP
						case 15:
							break;

						// End
						case 0:
						{
							p.PNum = 0xff;

							if (!simulating)
							{
								if (playingInfo.Pdb.CurrPos >= playingInfo.Pdb.LastPos)
								{
									playingInfo.Pdb.CurrPos = playingInfo.Pdb.FirstPos;
									endReached = true;
								}
								else
									playingInfo.Pdb.CurrPos++;

								ShowSongPosition();
								ShowTracks();

								GetTrackStep();
							}

							return true;
						}

						// Loop
						case 1:
						{
							if (p.PLoop == 0)
							{
								p.PLoop = 0xffff;
								break;
							}
							else
							{
								if (p.PLoop == 0xffff)	// FF --'ed
									p.PLoop = x.b1;
							}

							p.PLoop--;
							p.PStep = x.w1;

							// Infinity check added by Thomas Neumann
							// so Turrican II - Alien world song 8 and others can be stopped
							if (p.PLoop == 0xffff)
							{
								// Loop infinity, means the module ends, but
								// only if all the other channels are stopped
								p.Looped = true;

								ushort j = 0;
								for (ushort i = 0; i < 8; i++)
								{
									if ((p1[i].PNum >= 0x90) || p1[i].Looped)
										j++;
								}

								if (j == 8)
								{
									if (simulating)
										return true;

									endReached = true;
								}
							}
							else
								p.Looped = false;

							break;
						}

						// GsPt
						case 8:
						{
							p.PrOAddr = p.PAddr;
							p.PrOStep = p.PStep;
							goto case 2;
						}

						// Cont
						case 2:
						{
							if (x.b1 < numPattern)
							{
								p.PAddr = BitConverter.ToUInt32(musicData, patternStart + x.b1 * 4);
								p.PStep = x.w1;
							}
							else
								p.PNum = 0xff;

							break;
						}

						// Wait
						case 3:
						{
							p.PWait = x.b1;
							return false;
						}

						// StCu
						case 14:
						{
							playingInfo.Mdb.PlayPattFlag = false;
							goto case 4;
						}

						// Stop
						case 4:
						{
							p.PNum = 0xff;
							return false;
						}

						// Kup^
						// Vibr
						// Enve
						// Lock
						case 5:
						case 6:
						case 7:
						case 12:
						{
							if (!simulating)
								NotePort(x.l);

							break;
						}

						// RoPt
						case 9:
						{
							p.PAddr = p.PrOAddr;
							p.PStep = p.PrOStep;
							break;
						}

						// Fade
						case 10:
						{
							if (!simulating)
								DoFade((sbyte)x.b1, (sbyte)x.b3);

							break;
						}

						// Cue
						case 13:
						{
							if (!simulating)
								playingInfo.Idb.Cue[x.b1 & 0x3] = x.w1;

							break;
						}

						// PPat
						case 11:
						{
							t = (byte)(x.b2 & 0x07);

							if (x.b1 < numPattern)
							{
								p1[t].PNum = x.b1;
								p1[t].PAddr = BitConverter.ToUInt32(musicData, patternStart + x.b1 * 4);
								p1[t].PxPose = (sbyte)x.b3;
								p1[t].PStep = 0;
								p1[t].PWait = 0;
								p1[t].PLoop = 0xffff;
							}
							else
								p1[t].PNum = 0xff;

							break;
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse all the tracks
		/// </summary>
		/********************************************************************/
		private void DoTracks()
		{
			if (playingInfo.Mdb.SpeedCnt-- == 0)
			{
				playingInfo.Mdb.SpeedCnt = playingInfo.Pdb.PreScale;

				for (int x = 0; x < 8; x++)
				{
					if (DoTrack(playingInfo.Pdb.p, playingInfo.Pdb.p[x], false))
					{
						if (endReached)
							break;

						x = -1;
					}
				}

				// Extra check for "end of module" added by Thomas Neumann
				int y = 0;
				for (int x = 0; x < 8; x++)
				{
					if (playingInfo.Pdb.p[x].PNum >= 0x80)
						y++;
				}

				if (y == 8)
				{
					if (playingInfo.Pdb.CurrPos >= playingInfo.Pdb.LastPos)
					{
						playingInfo.Pdb.CurrPos = playingInfo.Pdb.FirstPos;
						endReached = true;
					}
					else
						playingInfo.Pdb.CurrPos++;

					ShowSongPosition();
					ShowTracks();

					GetTrackStep();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Turns all the channels off
		/// </summary>
		/********************************************************************/
		private void AllOff()
		{
			playingInfo.Mdb.PlayerEnable = false;

			for (int x = 0; x < 8; x++)
			{
				Cdb c = playingInfo.Cdb[x];
				c.hw = playingInfo.Hdb[x];
				c.hw.C = c;			// Wait on DMA

				playingInfo.Hdb[x].Mode = 0;
				c.MacroWait = 0;
				c.MacroRun = c.CurVol = 0;
				c.SfxFlag = 0;
				c.SfxCode = c.SaveAddr = 0;
				playingInfo.Hdb[x].Vol = 0;
				c.Loop = c.SfxLockTime = -1;
				c.NewStyleMacro = 0xff;

				c.hw.SBeg = c.hw.SampleStart = 0;
				c.hw.SampleLength = c.hw.SLen = c.SaveLen = 2;
				c.hw.Loop = LoopOff;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert a sample buffer to 16 bit
		/// </summary>
		/********************************************************************/
		private void Conv16(int[] b, short[] outBuf, int num)
		{
			for (int x = 0; x < num; x++)
				outBuf[x] = (short)b[x];

			// Clear source buffer
			Array.Clear(b, 0, num);
		}



		/********************************************************************/
		/// <summary>
		/// Generate the sample data for all channels
		/// </summary>
		/********************************************************************/
		private void MixIt(int n, int b)
		{
			if (playingInfo.MultiMode)
			{
				MixAdd(playingInfo.Hdb[4], n, tBuf[3], b);
				MixAdd(playingInfo.Hdb[5], n, tBuf[4], b);
				MixAdd(playingInfo.Hdb[6], n, tBuf[5], b);
				MixAdd(playingInfo.Hdb[7], n, tBuf[6], b);
			}
			else
				MixAdd(playingInfo.Hdb[3], n, tBuf[3], b);

			MixAdd(playingInfo.Hdb[0], n, tBuf[0], b);
			MixAdd(playingInfo.Hdb[1], n, tBuf[1], b);
			MixAdd(playingInfo.Hdb[2], n, tBuf[2], b);
		}



		/********************************************************************/
		/// <summary>
		/// Generate the sample data for one channel
		/// </summary>
		/********************************************************************/
		private void MixAdd(Hdb hw, int n, int[] b, int offset)
		{
			int p = hw.SBeg;
			uint ps = hw.Pos;
			int v = hw.Vol;
			uint d = hw.Delta;
			uint l = (uint)(hw.SLen << 14);

			if ((hw.SampleStart < 0) || (hw.SBeg < 0))
				return;

			if ((hw.SampleStart >= sampleLen) || (hw.SBeg >= sampleLen))
				return;

			if (v > 0x40)
				v = 0x40;

			if (((hw.Mode & 1) == 0) || (l < 0x10000))
				return;

			if ((hw.Mode & 3) == 1)
			{
				p = hw.SBeg = hw.SampleStart;
				l = (uint)((hw.SLen = hw.SampleLength) << 14);
				ps = 0;
				hw.Mode |= 2;
			}

			while (n-- != 0)
			{
				// p[] went out of sampleData, when SetBegin messed up c->saveAddr (sBeg):
				// p[] goes negative on repeated AddBegin:s (R-Type)
				// p[] went out of sampleData on Apidya Title (Fix by Thomas Neumann)
				int index = (int)((ps += d) >> 14);

				if ((p + index) >= sampleLen)
					b[offset++] = 0x00;
				else
					b[offset++] = sampleData[p + index] * v * 4;

				if (ps < l)
					continue;

				ps -= l;
				p = hw.SampleStart;

				l = (uint)((hw.SLen = hw.SampleLength) << 14);
				if ((l < 0x10000) || !hw.Loop(hw))
				{
					hw.SLen = 0;
					ps = d = 0;
					p = 0;
					break;
				}
			}

			hw.SBeg = p;
			hw.Pos = ps;
			hw.Delta = d;

			if ((hw.Mode & 4) != 0)
				hw.Mode = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Setup the NostalgicPlayer channel
		/// </summary>
		/********************************************************************/
		private void SetupChannel(int chan)
		{
			IChannel channel = VirtualChannels[chan];

			channel.PlayBuffer(outBuf[chan], 0, BufSize, 16);
			channel.SetFrequency(mixerFreq);
			channel.SetVolume(256);

			if (playingInfo.MultiMode)
				channel.SetPanning((ushort)Tables.Pan7[chan]);
			else
				channel.SetPanning((ushort)Tables.Pan4[chan]);
		}



		/********************************************************************/
		/// <summary>
		/// Extract the sample information by parsing the instrument macros
		/// </summary>
		/********************************************************************/
		private void ExtractSampleInfo()
		{
			List<SampleRange> samples = new List<SampleRange>();

			for (int i = 0; i < numMacro; i++)
			{
				// Find the start of the macro
				int macroOffset = BitConverter.ToInt32(musicData, macroStart + i * 4);
				bool endOfMacro = false;

				SampleRange range = new SampleRange();

				for (int step = 0; !endOfMacro; step += 4)
				{
					Uni x = new Uni((uint)ToInt32(macroOffset + step));
					byte b0 = x.b0;
					x.b0 = 0;

					switch (b0)
					{
						// Stop
						case 0x07:
						case 0xff:
						{
							endOfMacro = true;
							break;
						}

						// SetBegin
						case 0x02:
						{
							if (range.Start != 0)
							{
								AddSample(samples, range);
								range = new SampleRange();
							}

							range.Start = (int)x.l;
							break;
						}

						// SetLen
						case 0x03:
						{
							range.End = range.Start + x.w1 * 2;
							break;
						}

						// SampleLoop
						case 0x18:
						{
							range.LoopStart = (int)(range.Start + x.l);
							range.LoopLength = range.End - range.LoopStart;
							break;
						}
					}
				}

				AddSample(samples, range);
			}

			sampleInfo = new SampleInfo[samples.Count];

			if (samples.Count > 0)
			{
				for (int i = 0; i < samples.Count - 1; i++)
					sampleInfo[i] = GetSampleInfo(samples[i], samples[i + 1]);

				sampleInfo[samples.Count - 1] = GetSampleInfo(samples[samples.Count - 1], null);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Add current range into the right position in the list
		/// </summary>
		/********************************************************************/
		private void AddSample(List<SampleRange> list, SampleRange range)
		{
			if (range.End == 0)
				return;

			int j;

			for (j = 0; j < list.Count; j++)
			{
				SampleRange storedRange = list[j];

				if ((range.Start >= storedRange.Start) && (range.Start < storedRange.End))
				{
					if (range.End > storedRange.End)
						storedRange.End = range.End;

					j = -1;
					break;
				}

				if ((range.Start < storedRange.Start) && (range.End > storedRange.Start))
				{
					storedRange.Start = range.Start;

					if (range.End > storedRange.End)
						storedRange.End = range.End;

					j = -1;
					break;
				}

				if (range.Start < storedRange.Start)
					break;
			}

			if (j != -1)
				list.Insert(j, range);
		}



		/********************************************************************/
		/// <summary>
		/// Return an initialized sample info object
		/// </summary>
		/********************************************************************/
		private SampleInfo GetSampleInfo(SampleRange current, SampleRange next)
		{
			int sampleLength = (next == null ? current.End : next.Start) - current.Start;

			SampleInfo info = new SampleInfo
			{
				Name = string.Empty,
				Flags = SampleInfo.SampleFlag.None,
				Type = SampleInfo.SampleType.Sample,
				BitSize = SampleInfo.SampleSize._8Bit,
				Panning = -1,
				Volume = 256,
				Sample = sampleData.AsSpan(current.Start, sampleLength).ToArray(),//XX
				Length = (uint)sampleLength
			};

			if (current.LoopLength != 0)
			{
				info.Flags |= SampleInfo.SampleFlag.Loop;
				info.LoopStart = (uint)(current.LoopStart - current.Start);
				info.LoopLength = (uint)current.LoopLength;
			}

			return info;
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current song position
		/// </summary>
		/********************************************************************/
		private void ShowSongPosition()
		{
			OnModuleInfoChanged(InfoPositionLine, FormatPosition());
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
			OnModuleInfoChanged(InfoSpeedLine, (playingInfo.Pdb.PreScale + 1).ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current tempo
		/// </summary>
		/********************************************************************/
		private void ShowTempo()
		{
			OnModuleInfoChanged(InfoTempoLine, (0x1b51f8 / playingInfo.Mdb.CiaSave).ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with all dynamic values
		/// </summary>
		/********************************************************************/
		private void UpdateModuleInformation()
		{
			ShowSongPosition();
			ShowTracks();
			ShowSpeed();
			ShowTempo();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the number of positions
		/// </summary>
		/********************************************************************/
		private string FormatPositionLength()
		{
			return (playingInfo.Pdb.LastPos - playingInfo.Pdb.FirstPos + 1).ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the current position
		/// </summary>
		/********************************************************************/
		private string FormatPosition()
		{
			return (playingInfo.Pdb.CurrPos - playingInfo.Pdb.FirstPos).ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing tracks
		/// </summary>
		/********************************************************************/
		private string FormatTracks()
		{
			StringBuilder sb = new StringBuilder();

			int count = ModuleChannelCount;
			for (int i = 0; i < count; i++)
			{
				int track = BitConverter.ToUInt16(musicData, trackStart + (playingInfo.Pdb.CurrPos * 16) + i * 2) >> 8;
				if (track >= 0x90)
					sb.Append("-, ");
				else
				{
					if (track >= 0x80)
						track = playingInfo.LastTrackPlayed[i];
					else
						playingInfo.LastTrackPlayed[i] = track;

					sb.Append(track);
					sb.Append(", ");
				}
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
		#endregion
	}
}
