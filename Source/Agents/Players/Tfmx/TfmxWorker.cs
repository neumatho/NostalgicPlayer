/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
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
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class TfmxWorker : ModulePlayerAgentBase
	{
		private static readonly Dictionary<Guid, ModuleType> moduleTypeLookup = new Dictionary<Guid, ModuleType>
		{
			{ Tfmx.Agent1Id, ModuleType.Tfmx15 },
			{ Tfmx.Agent2Id, ModuleType.TfmxPro },
			{ Tfmx.Agent3Id, ModuleType.Tfmx7V }
		};

		// MD5 checksum for Danger Freak Title song
		private static readonly byte[] dangerFreakTitle =
		{
			0x0a, 0x7b, 0xc5, 0x73, 0x1c, 0x51, 0xf8, 0x1b,
			0x6c, 0x88, 0xe3, 0xd6, 0x03, 0x13, 0xca, 0xba
		};

		// MD5 checksum for Gem X Title song
		private static readonly byte[] gemXTitle =
		{
			0x92, 0x31, 0xbe, 0xb5, 0x3a, 0x18, 0xb2, 0xf4,
			0xfc, 0x0d, 0x4d, 0xce, 0x0c, 0x1c, 0xa6, 0x79
		};

		private static readonly ChannelPanning[] pan4 =
		{
			ChannelPanning.Left, ChannelPanning.Right, ChannelPanning.Right, ChannelPanning.Left
		};

		private static readonly ChannelPanning[] pan7 =
		{
			ChannelPanning.Left, ChannelPanning.Right, ChannelPanning.Right,
			ChannelPanning.Left, ChannelPanning.Right, ChannelPanning.Right, ChannelPanning.Left
		};

		private static readonly int[] noteVals =
		{
			0x6ae, 0x64e, 0x5f4, 0x59e, 0x54d, 0x501,
			0x4b9, 0x475, 0x435, 0x3f9, 0x3c0, 0x38c, 0x358, 0x32a, 0x2fc, 0x2d0, 0x2a8, 0x282,
			0x25e, 0x23b, 0x21b, 0x1fd, 0x1e0, 0x1c6, 0x1ac, 0x194, 0x17d, 0x168, 0x154, 0x140,
			0x12f, 0x11e, 0x10e, 0x0fe, 0x0f0, 0x0e3, 0x0d6, 0x0ca, 0x0bf, 0x0b4, 0x0aa, 0x0a0,
			0x097, 0x08f, 0x087, 0x07f, 0x078, 0x071, 0x0d6, 0x0ca, 0x0bf, 0x0b4, 0x0aa, 0x0a0,
			0x097, 0x08f, 0x087, 0x07f, 0x078, 0x071, 0x0d6, 0x0ca, 0x0bf, 0x0b4
		};

		private const int BufSize = 1024;

		private const int InfoSpeedLine = 3;
		private const int InfoTempoLine = 4;

		private readonly ModuleType currentModuleType;
		private readonly bool isLittleEndian;

		private int currentSong;

		private bool firstTime;

		private OneFile header;

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

		private Hdb[] hdb;
		private Mdb mdb;
		private Cdb[] cdb;
		private PdBlk pdb;
		private Idb idb;

		private uint outRate;

		private bool gemx;
		private bool dangerFreakHack;

		private int jiffies;
		private bool multiMode;

		private int loops;
		private int startPat;

		private uint eClocks;
		private int eRem;

		private int[][] tBuf;
		private short[][] outBuf;

		private int numBytes;
		private int bytesDone;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TfmxWorker(Guid typeId)
		{
			if (!moduleTypeLookup.TryGetValue(typeId, out currentModuleType))
				currentModuleType = ModuleType.Unknown;

			isLittleEndian = BitConverter.IsLittleEndian;
		}

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "tfx", "mdat", "tfm" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			// Check the module
			ModuleType checkType = TestModule(fileInfo);
			if (checkType == currentModuleType)
				return AgentResult.Ok;

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
				// Used track steps
				case 0:
				{
					description = Resources.IDS_TFMX_INFODESCLINE0;
					value = numTrackSteps.ToString();
					break;
				}

				// Used patterns
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

				// Current speed
				case 3:
				{
					description = Resources.IDS_TFMX_INFODESCLINE3;
					value = pdb.PreScale.ToString();
					break;
				}

				// BPM
				case 4:
				{
					description = Resources.IDS_TFMX_INFODESCLINE4;
					value = (0x1b51f8 / mdb.CiaSave).ToString();
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
		public override ModulePlayerSupportFlag SupportFlags => ModulePlayerSupportFlag.BufferMode | ModulePlayerSupportFlag.SetPosition;



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
				MD5 md5 = new MD5CryptoServiceProvider();
				byte[] checksum = md5.ComputeHash(musicData, 0, musicLen);

				// Check the checksum
				dangerFreakHack = checksum.SequenceEqual(dangerFreakTitle);
				gemx = checksum.SequenceEqual(gemXTitle);

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
					ModuleStream sampleStream = fileInfo.Loader?.OpenExtraFile("smpl");
					if (sampleStream == null)
						sampleStream = fileInfo.Loader?.OpenExtraFile("sam");

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
		public override bool InitPlayer()
		{
			hdb = Helpers.InitializeArray<Hdb>(8);
			mdb = new Mdb();
			cdb = Helpers.InitializeArray<Cdb>(16);
			pdb = new PdBlk();
			idb = new Idb();

			// Initialize some of the member variables
			outRate = 44100;
			eClocks = 14318;

			if (currentModuleType == ModuleType.Tfmx7V)
				multiMode = true;
			else
				multiMode = false;

			// Allocate mixer buffers
			tBuf = new int[7][];
			outBuf = new short[7][];

			for (int i = 0; i < 4; i++)
			{
				tBuf[i] = new int[BufSize];
				outBuf[i] = new short[BufSize];
			}

			if (multiMode)
			{
				for (int i = 4; i < 7; i++)
				{
					tBuf[i] = new int[BufSize];
					outBuf[i] = new short[BufSize];
				}
			}

			numBytes = 0;
			bytesDone = 0;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public override void CleanupPlayer()
		{
			idb = null;
			pdb = null;
			cdb = null;
			mdb = null;
			hdb = null;

			tBuf = null;
			outBuf = null;

			Cleanup();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the current song
		/// </summary>
		/********************************************************************/
		public override void InitSound(int songNumber, DurationInfo durationInfo)
		{
			// Initialize member variables
			currentSong = songNumber;

			firstTime = true;

			jiffies = 0;
			loops = 0;				// Infinity loop on the modules
			startPat = -1;
			eRem = 0;

			// Initialize the player
			TfmxInit();
			StartSong(songNumber, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the duration for all sub-songs
		/// </summary>
		/********************************************************************/
		public override DurationInfo[] CalculateDuration()
		{
			SubSongInfo subs = SubSongs;

			// Initialize some of the temp variables
			Pdb[] pattInfo = Helpers.InitializeArray<Pdb>(8);
			List<DurationInfo> durationList = new List<DurationInfo>();

			// Take each sub-song
			for (int i = 0; i < subs.Number; i++)
			{
				// Get the start and end positions of the sub-song
				ushort startPos = songStart[i];
				ushort endPos = songEnd[i];

				// Calculate the start tempo
				ushort cia, spd;

				if (tempo[i] >= 0x10)
				{
					cia = (ushort)(0x1b51f8 / tempo[i]);
					spd = 5;
				}
				else
				{
					cia = 14318;
					spd = tempo[i];
				}

				short trackLoop = -1;
				float total = 0.0f;
				List<PositionInfo> posInfoList = new List<PositionInfo>();

				// Now take each position
				for (int j = startPos; j <= endPos; j++)
				{
					PositionInfo posInfo = new PositionInfo((byte)spd, cia, new TimeSpan((long)total * TimeSpan.TicksPerMillisecond));

					while ((j - startPos) >= posInfoList.Count)
						posInfoList.Add(posInfo);

Loop:
					// Get the offset to the position data
					int posData = trackStart + (j * 16);

					if (BitConverter.ToUInt16(musicData, posData) == 0xeffe)
					{
						switch (BitConverter.ToUInt16(musicData, posData + 1 * 2))
						{
							// Stop
							case 0:
							{
								j = endPos;
								break;
							}

							// Loop
							case 1:
							{
								if (trackLoop-- == 0)
								{
									trackLoop = -1;
									break;
								}
								else
								{
									if (trackLoop < 0)
									{
										trackLoop = BitConverter.ToInt16(musicData, posData + 3 * 2);
										if ((trackLoop == 0) || (trackLoop > 1000))
										{
											j = endPos;
											break;
										}
									}
								}

								// If we jump back, the module has probably ended
								ushort l2 = BitConverter.ToUInt16(musicData, posData + 2 * 2);
								if ((l2 < j) && (trackLoop < 0))
								{
									j = endPos;
									break;
								}

								j = l2;
								goto Loop;
							}

							// Speed
							case 2:
							{
								spd = BitConverter.ToUInt16(musicData, posData + 2 * 2);

								int x;
								ushort l3 = BitConverter.ToUInt16(musicData, posData + 3 * 2);
								if (((l3 & 0xf200) == 0) && ((x = (l3 & 0x1ff)) > 0xf))
									cia = (ushort)(0x1b51f8 / x);

								break;
							}

							// Time share
							case 3:
							{
								int x = BitConverter.ToUInt16(musicData, posData + 3 * 2);
								if ((x & 0x8000) == 0)
								{
									sbyte xx = (sbyte)x;
									xx = xx < -0x20 ? (sbyte)-0x20 : xx;
									cia = (ushort)((14318 * (xx + 100)) / 100);
								}
								break;
							}
						}
					}
					else
					{
						for (int k = 0; k < 8; k++)
						{
							ushort lv = BitConverter.ToUInt16(musicData, posData + k * 2);

							byte y = pattInfo[k].PNum = (byte)(lv >> 8);
							if (y < 0x80)
							{
								pattInfo[k].PStep = 0;
								pattInfo[k].PWait = 0;
								pattInfo[k].PLoop = 0xffff;
								pattInfo[k].PAddr = BitConverter.ToUInt32(musicData, patternStart + y * 4);
								pattInfo[k].Looped = false;
							}
						}

						// Take each row in the pattern
						bool endPatt = false;

						do
						{
							for (int k = 0; k < 8; k++)
							{
								if (DoTrack(pattInfo, pattInfo[k], true))
									endPatt = true;
							}

							// Add the time it takes to play the current row
							if (!endPatt)
							{
								total += ((cia * 1.3968255f) / 1000.0f) * (spd + 1);

								// Check to see if all the channels are stopped.
								// If so, the pattern ends
								ushort l = 0;
								for (int k = 0; k < 8; k++)
								{
									if (pattInfo[k].PNum >= 0x90)
										l++;
								}

								if (l == 8)
									endPatt = true;
							}
						}
						while (!endPatt);
					}
				}

				// Set the total time
				durationList.Add(new DurationInfo(new TimeSpan((long)total * TimeSpan.TicksPerMillisecond), posInfoList.ToArray()));
			}

			return durationList.ToArray();
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

				if (mdb.PlayerEnable)
				{
					DoAllMacros();

					if (currentSong >= 0)
						DoTracks();

					numBytes = (int)(eClocks * (outRate >> 1));
					eRem += (numBytes % 357955);
					numBytes /= 357955;

					if (eRem > 357955)
					{
						numBytes++;
						eRem -= 357955;
					}
				}
				else
					bytesDone = BufSize;
			}

			Conv16(tBuf[0], outBuf[0], bytesDone);
			Conv16(tBuf[1], outBuf[1], bytesDone);
			Conv16(tBuf[2], outBuf[2], bytesDone);
			Conv16(tBuf[3], outBuf[3], bytesDone);

			if (multiMode)
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

			if (multiMode)
			{
				SetupChannel(4);
				SetupChannel(5);
				SetupChannel(6);
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
		/// Return the length of the current song
		/// </summary>
		/********************************************************************/
		public override int SongLength => pdb.LastPos - pdb.FirstPos + 1;



		/********************************************************************/
		/// <summary>
		/// Return the current position of the song
		/// </summary>
		/********************************************************************/
		public override int GetSongPosition()
		{
			return pdb.CurrPos - pdb.FirstPos;
		}



		/********************************************************************/
		/// <summary>
		/// Set a new position of the song
		/// </summary>
		/********************************************************************/
		public override void SetSongPosition(int position, PositionInfo positionInfo)
		{
			// Set the speed and tempo
			mdb.CiaSave = positionInfo.Bpm;
			eClocks = mdb.CiaSave;
			mdb.SpeedCnt = pdb.PreScale = positionInfo.Speed;

			// Change the position
			pdb.CurrPos = (ushort)(pdb.FirstPos + position);
			if (pdb.CurrPos == pdb.FirstPos)
				firstTime = true;

			GetTrackStep();

			// Change the module info
			OnModuleInfoChanged(InfoSpeedLine, pdb.PreScale.ToString());
			OnModuleInfoChanged(InfoTempoLine, (0x1b51f8 / mdb.CiaSave).ToString());
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
		/// Tests the module to see which type of module it is
		/// </summary>
		/********************************************************************/
		private ModuleType TestModule(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 512)
				return ModuleType.Unknown;

			int startOffset = 0;

			// First check for one-file format
			if (IsOneFile(moduleStream))
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
									break;;
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
		private bool IsOneFile(ModuleStream moduleStream)
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

				header = oneFile;

				return true;
			}

			return false;
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
				hdb[x].C = cdb[x];
				pdb.p[x].PNum = 0xff;
				pdb.p[x].PAddr = 0;

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
			mdb.PlayerEnable = false;		// Sort of locking mechanism

			mdb.MasterVol = 0x40;
			mdb.FadeSlope = 0;
			mdb.TrackLoop = -1;
			mdb.PlayPattFlag = false;
			mdb.CiaSave = 14318;			// Assume 125 BPM, NTSC timing

			if (mode != 2)
			{
				pdb.CurrPos = pdb.FirstPos = songStart[song];
				pdb.LastPos = songEnd[song];

				ushort x = tempo[song];
				if (x >= 0x10)
				{
					mdb.CiaSave = (ushort)(0x1b51f8 / x);
					pdb.PreScale = 5;		// Fix by Thomas Neumann
				}
				else
					pdb.PreScale = x;

				// Change the module info
				OnModuleInfoChanged(InfoSpeedLine, pdb.PreScale.ToString());
			}

			// Change the module info
			OnModuleInfoChanged(InfoTempoLine, (0x1b51f8 / mdb.CiaSave).ToString());

			eClocks = mdb.CiaSave;

			for (int x = 0; x < 8; x++)
			{
				pdb.p[x].PAddr = 0;
				pdb.p[x].PNum = 0xff;
				pdb.p[x].PxPose = 0;
				pdb.p[x].PStep = 0;
			}

			if (mode != 2)
				GetTrackStep();

			if (startPat != -1)
			{
				pdb.CurrPos = pdb.FirstPos = (ushort)startPat;
				GetTrackStep();
				startPat = -1;
			}

			mdb.SpeedCnt = 0;
			mdb.EndFlag = false;

			mdb.PlayerEnable = true;
		}



		/********************************************************************/
		/// <summary>
		/// Setup the note period
		/// </summary>
		/********************************************************************/
		private void NotePort(uint i)
		{
			Uni x = new Uni(i);
			Cdb c = cdb[x.b2 & (multiMode ? 7 : 3)];

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
					c.DestPeriod = (ushort)(noteVals[c.CurrNote = (byte)(x.b0 & 0x3f)]);
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

						a = (noteVals[a + x.b1 & 0x3f] * (0x100 + c.FineTune + (sbyte)x.b3)) >> 8;
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

						a = (noteVals[a + x.b1 & 0x3f] * (0x100 + c.FineTune + (sbyte)x.b3)) >> 8;
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

						a = (noteVals[a + x.b1 & 0x3f] * (0x100 + c.FineTune + (sbyte)x.b3)) >> 8;
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
						idb.Cue[x.b1 & 0x03] = x.w1;
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

			if ((mdb.FadeSlope != 0) && ((--mdb.FadeTime) == 0))
			{
				mdb.FadeTime = mdb.FadeReset;
				mdb.MasterVol += mdb.FadeSlope;

				if (mdb.FadeDest == mdb.MasterVol)
					mdb.FadeSlope = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Run the macro on one channel
		/// </summary>
		/********************************************************************/
		private void DoMacro(int cc)
		{
			Cdb c = cdb[cc];

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
			c.hw.Delta = c.CurPeriod != 0 ? (3579545 << 9) / (c.CurPeriod * outRate >> 5) : 0;
			c.hw.SampleStart = (int)c.SaveAddr;
			c.hw.SampleLength = (ushort)(c.SaveLen != 0 ? c.SaveLen << 1 : 131072);

			if ((c.hw.Mode & 3) == 1)
			{
				c.hw.SBeg = c.hw.SampleStart;
				c.hw.SLen = c.hw.SampleLength;
			}

			c.hw.Vol = (byte)((c.CurVol * mdb.MasterVol) >> 6);
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

			if (multiMode)
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
			Cdb c = cdb[i & 0xf];
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
			mdb.FadeDest = dv;

			if (((mdb.FadeTime = mdb.FadeReset = sp) == 0) || (mdb.MasterVol == sp))
			{
				mdb.MasterVol = dv;
				mdb.FadeSlope = 0;
				return;
			}

			mdb.FadeSlope = (sbyte)((mdb.MasterVol > mdb.FadeDest) ? -1 : 1);
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
				if (pdb.CurrPos == pdb.FirstPos)
				{
					if (firstTime)
						firstTime = false;
					else
						OnEndReached();
				}

				int l = trackStart + (pdb.CurrPos * 16);

				jiffies = 0;
				if (BitConverter.ToUInt16(musicData, l) == 0xeffe)
				{
					switch (BitConverter.ToUInt16(musicData, l + 1 * 2))
					{
						// Stop
						case 0:
						{
							OnEndReached();
							return;
						}

						// Loop
						case 1:
						{
							if (loops != 0)
							{
								if ((--loops) == 0)
								{
									OnEndReached();
									return;
								}
							}

							if ((mdb.TrackLoop--) == 0)
							{
								mdb.TrackLoop = -1;
								pdb.CurrPos++;

								OnPositionChanged();
								break;
							}

							if (mdb.TrackLoop < 0)
							{
								mdb.TrackLoop = BitConverter.ToInt16(musicData, l + 3 * 2);
								if ((mdb.TrackLoop == 0) || (mdb.TrackLoop > 1000))		// Fix for Rames - Title song 0
									OnEndReached();
							}

							// If we jump back, the module has probably ended
							ushort l2 = BitConverter.ToUInt16(musicData, l + 2 * 2);

							// Fix for PP_Hammer - Ingame song 8 by Thomas Neumann
							if (l2 < pdb.FirstPos)
								l2 = pdb.FirstPos;

							if ((l2 < pdb.CurrPos) && (mdb.TrackLoop < 0))
								OnEndReached();

							pdb.CurrPos = l2;
							OnPositionChanged();
							break;
						}

						// Speed
						case 2:
						{
							mdb.SpeedCnt = pdb.PreScale = BitConverter.ToUInt16(musicData, l + 2 * 2);

							// Change the module info
							OnModuleInfoChanged(InfoSpeedLine, pdb.PreScale.ToString());

							int x;
							ushort l3 = BitConverter.ToUInt16(musicData, l + 3 * 2);
							if (((l3 & 0xf200) == 0) && ((x = (l3 & 0x1ff)) > 0xf))
							{
								mdb.CiaSave = (ushort)(0x1b51f8 / x);
								eClocks = mdb.CiaSave;

								// Change the module info
								OnModuleInfoChanged(InfoTempoLine, (0x1b51f8 / mdb.CiaSave).ToString());
							}

							pdb.CurrPos++;
							OnPositionChanged();
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
								mdb.CiaSave = (ushort)((14318 * (xx + 100)) / 100);
								eClocks = mdb.CiaSave;
								multiMode = true;

								// Change the module info
								OnModuleInfoChanged(InfoTempoLine, (0x1b51f8 / mdb.CiaSave).ToString());
							}

							pdb.CurrPos++;
							OnPositionChanged();
							break;
						}

						// Fade
						case 4:
						{
							ushort l2 = BitConverter.ToUInt16(musicData, l + 2 * 2);
							ushort l3 = BitConverter.ToUInt16(musicData, l + 3 * 2);

							DoFade((sbyte)(l2 & 0xff), (sbyte)(l3 & 0xff));
							pdb.CurrPos++;
							OnPositionChanged();
							break;
						}

						default:
						{
							pdb.CurrPos++;
							OnPositionChanged();
							break;
						}
					}
				}
				else
				{
					for (int x = 0; x < 8; x++)
					{
						ushort lv = BitConverter.ToUInt16(musicData, l + x * 2);

						pdb.p[x].PxPose = (sbyte)(lv & 0xff);

						byte y = pdb.p[x].PNum = (byte)(lv >> 8);
						if (y < 0x80)
						{
							pdb.p[x].PStep = 0;
							pdb.p[x].PWait = 0;
							pdb.p[x].PLoop = 0xffff;
							pdb.p[x].PAddr = BitConverter.ToUInt32(musicData, patternStart + y * 4);
							pdb.p[x].Looped = false;
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
								if (pdb.CurrPos == pdb.LastPos)
								{
									pdb.CurrPos = pdb.FirstPos;
									OnPositionChanged();
									OnEndReached();
								}
								else
								{
									pdb.CurrPos++;
									OnPositionChanged();
								}

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

									OnEndReached();
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
							mdb.PlayPattFlag = false;
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
								idb.Cue[x.b1 & 0x3] = x.w1;

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
			jiffies++;

			if (mdb.SpeedCnt-- == 0)
			{
				mdb.SpeedCnt = pdb.PreScale;

				for (int x = 0; x < 8; x++)
				{
					if (DoTrack(pdb.p, pdb.p[x], false))
						x = -1;
				}

				// Extra check for "end of module" added by Thomas Neumann
				int y = 0;
				for (int x = 0; x < 8; x++)
				{
					if (pdb.p[x].PNum >= 0x90)
						y++;
				}

				if (y == 8)
				{
					if (pdb.CurrPos == pdb.LastPos)
					{
						pdb.CurrPos = pdb.FirstPos;
						OnPositionChanged();
						OnEndReached();
					}
					else
					{
						pdb.CurrPos++;
						OnPositionChanged();
					}

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
			mdb.PlayerEnable = false;

			for (int x = 0; x < 8; x++)
			{
				Cdb c = cdb[x];
				c.hw = hdb[x];
				c.hw.C = c;			// Wait on DMA

				hdb[x].Mode = 0;
				c.MacroWait = 0;
				c.MacroRun = c.CurVol = 0;
				c.SfxFlag = 0;
				c.SfxCode = c.SaveAddr = 0;
				hdb[x].Vol = 0;
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
			if (multiMode)
			{
				MixAdd(hdb[4], n, tBuf[3], b);
				MixAdd(hdb[5], n, tBuf[4], b);
				MixAdd(hdb[6], n, tBuf[5], b);
				MixAdd(hdb[7], n, tBuf[6], b);
			}
			else
				MixAdd(hdb[3], n, tBuf[3], b);

			MixAdd(hdb[0], n, tBuf[0], b);
			MixAdd(hdb[1], n, tBuf[1], b);
			MixAdd(hdb[2], n, tBuf[2], b);
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

			channel.PlaySample(outBuf[chan], 0, BufSize, 16);
			channel.SetFrequency(outRate);
			channel.SetVolume(256);

			if (multiMode)
				channel.SetPanning((ushort)pan7[chan]);
			else
				channel.SetPanning((ushort)pan4[chan]);
		}
		#endregion
	}
}
