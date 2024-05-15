/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Polycode.NostalgicPlayer.Agent.Player.Mpg123.Containers;
using Polycode.NostalgicPlayer.Ports.LibMpg123.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Ports.LibMpg123;

namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class Mpg123Worker : SamplePlayerWithDurationAgentBase
	{
		private static readonly Dictionary<Guid, ModuleType> moduleTypeLookup = new Dictionary<Guid, ModuleType>
		{
			{ Mpg123.Agent1Id, ModuleType.Mpeg10 },
			{ Mpg123.Agent2Id, ModuleType.Mpeg20 },
			{ Mpg123.Agent3Id, ModuleType.Mpeg25 }
		};

		private readonly ModuleType currentModuleType;

		private int oldBitRate;

		private Mpg123_FrameInfo frameInfo;
		private long numberOfFrames;
		private long firstFramePosition;

		private string songName;
		private string artist;
		private string album;
		private string year;
		private string comment;
		private string genre;
		private byte trackNum;
		private PictureInfo[] pictures;

		private LibMpg123 mpg123Handle;

		private const int InfoBitRateLine = 6;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Mpg123Worker(Guid typeId)
		{
			if (!moduleTypeLookup.TryGetValue(typeId, out currentModuleType))
				currentModuleType = ModuleType.Unknown;
		}

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "mp1", "mp2", "mp3", "m2a", "mpg" };



		/********************************************************************/
		/// <summary>
		/// Return the identity priority number. Players with the lowest
		/// numbers will be called first.
		///
		/// Normally, you should not change this, but make your Identify()
		/// method to be aware of similar formats
		/// </summary>
		/********************************************************************/
		public override int IdentifyPriority => int.MaxValue;



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
		/// Return the name of the module
		/// </summary>
		/********************************************************************/
		public override string ModuleName => songName;



		/********************************************************************/
		/// <summary>
		/// Return the name of the author
		/// </summary>
		/********************************************************************/
		public override string Author => artist;



		/********************************************************************/
		/// <summary>
		/// Return all pictures available
		/// </summary>
		/********************************************************************/
		public override PictureInfo[] Pictures => pictures;



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
				// Track number
				case 0:
				{
					description = Resources.IDS_MPG_INFODESCLINE00;
					value = trackNum == 0 ? Resources.IDS_MPG_INFO_UNKNOWN : trackNum.ToString();
					break;
				}

				// Album
				case 1:
				{
					description = Resources.IDS_MPG_INFODESCLINE01;
					value = string.IsNullOrEmpty(album) ? Resources.IDS_MPG_INFO_UNKNOWN : album;
					break;
				}

				// Year
				case 2:
				{
					description = Resources.IDS_MPG_INFODESCLINE02;
					value = string.IsNullOrEmpty(year) ? Resources.IDS_MPG_INFO_UNKNOWN : year;
					break;
				}

				// Genre
				case 3:
				{
					description = Resources.IDS_MPG_INFODESCLINE03;
					value = string.IsNullOrEmpty(genre) ? Resources.IDS_MPG_INFO_UNKNOWN : genre;
					break;
				}

				// Comment
				case 4:
				{
					description = Resources.IDS_MPG_INFODESCLINE04;
					value = string.IsNullOrEmpty(comment) ? Resources.IDS_MPG_INFO_NONE : comment;
					break;
				}

				// Layer
				case 5:
				{
					description = Resources.IDS_MPG_INFODESCLINE05;
					value = frameInfo.Layer.ToString();
					break;
				}

				// Bit rate
				case 6:
				{
					description = Resources.IDS_MPG_INFODESCLINE06;
					value = frameInfo.BitRate == 0 ? Resources.IDS_MPG_INFO_FREE_FORMAT : frameInfo.BitRate.ToString();
					break;
				}

				// Frequency
				case 7:
				{
					description = Resources.IDS_MPG_INFODESCLINE07;
					value = frameInfo.Rate.ToString();
					break;
				}

				// Offset to first header
				case 8:
				{
					description = Resources.IDS_MPG_INFODESCLINE08;
					value = firstFramePosition.ToString();
					break;
				}

				// Frames
				case 9:
				{
					description = Resources.IDS_MPG_INFODESCLINE09;
					value = numberOfFrames == 0 ? Resources.IDS_MPG_INFO_UNKNOWN : numberOfFrames.ToString();
					break;
				}

				// Channel mode
				case 10:
				{
					description = Resources.IDS_MPG_INFODESCLINE10;

					switch (frameInfo.Mode)
					{
						default:
						case Mpg123_Mode.Mono:
						{
							value = Resources.IDS_MPG_INFO_CHANNEL_MONO;
							break;
						}

						case Mpg123_Mode.Stereo:
						{
							value = Resources.IDS_MPG_INFO_CHANNEL_STEREO;
							break;
						}

						case Mpg123_Mode.Joint:
						{
							value = Resources.IDS_MPG_INFO_CHANNEL_JOINT;
							break;
						}

						case Mpg123_Mode.Dual:
						{
							value = Resources.IDS_MPG_INFO_CHANNEL_DUAL;
							break;
						}
					}
					break;
				}

				// Private
				case 11:
				{
					description = Resources.IDS_MPG_INFODESCLINE11;
					value = (frameInfo.Flags & Mpg123_Flags.Private) != 0 ? Resources.IDS_MPG_INFO_YES : Resources.IDS_MPG_INFO_NO;
					break;
				}

				// CRCs
				case 12:
				{
					description = Resources.IDS_MPG_INFODESCLINE12;
					value = (frameInfo.Flags & Mpg123_Flags.Crc) != 0 ? Resources.IDS_MPG_INFO_YES : Resources.IDS_MPG_INFO_NO;
					break;
				}

				// Copyrighted
				case 13:
				{
					description = Resources.IDS_MPG_INFODESCLINE13;
					value = (frameInfo.Flags & Mpg123_Flags.Copyright) != 0 ? Resources.IDS_MPG_INFO_YES : Resources.IDS_MPG_INFO_NO;
					break;
				}

				// Original
				case 14:
				{
					description = Resources.IDS_MPG_INFODESCLINE14;
					value = (frameInfo.Flags & Mpg123_Flags.Original) != 0 ? Resources.IDS_MPG_INFO_YES : Resources.IDS_MPG_INFO_NO;
					break;
				}

				// Emphasis
				case 15:
				{
					description = Resources.IDS_MPG_INFODESCLINE15;

					switch (frameInfo.Emphasis)
					{
						default:
						case 0:
						{
							value = Resources.IDS_MPG_INFO_EMPHASIS0;
							break;
						}

						case 1:
						{
							value = Resources.IDS_MPG_INFO_EMPHASIS1;
							break;
						}

						case 3:
						{
							value = Resources.IDS_MPG_INFO_EMPHASIS3;
							break;
						}
					}
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

		#region ISamplePlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Will load the header information from the file
		/// </summary>
		/********************************************************************/
		public override AgentResult LoadHeaderInfo(ModuleStream moduleStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer(ModuleStream moduleStream, out string errorMessage)
		{
			if (!base.InitPlayer(moduleStream, out errorMessage))
				return false;

			// Get a Mpg123 handle, which is used on all other calls
			mpg123Handle = LibMpg123.Mpg123_New(null, out Mpg123_Errors error);
			if (error != Mpg123_Errors.Ok)
			{
				errorMessage = GetErrorString(error);
				return false;
			}

			// Make sure, that the output is always in 32-bit for every sample rate
			Mpg123_Errors result = mpg123Handle.Mpg123_Format_None();
			if (result != Mpg123_Errors.Ok)
			{
				errorMessage = GetErrorString(result);
				return false;
			}

			mpg123Handle.Mpg123_Rates(out int[] supportedRates, out ulong number);
			if (number > 0)
			{
				// Set the output format to 32-bit on every rate
				foreach (int rate in supportedRates)
				{
					result = mpg123Handle.Mpg123_Format(rate, Mpg123_ChannelCount.Mono | Mpg123_ChannelCount.Stereo, Mpg123_Enc_Enum.Enc_Signed_32);
					if (result != Mpg123_Errors.Ok)
					{
						errorMessage = GetErrorString(result);
						return false;
					}
				}
			}

			// We want to include pictures in the scan
			result = mpg123Handle.Mpg123_Param(Mpg123_Parms.Add_Flags, (int)Mpg123_Param_Flags.Picture, 0);
			if (result != Mpg123_Errors.Ok)
			{
				errorMessage = GetErrorString(result);
				return false;
			}

			// Increase the resync limit
			result = mpg123Handle.Mpg123_Param(Mpg123_Parms.Resync_Limit, 16 * 1024, 0);
			if (result != Mpg123_Errors.Ok)
			{
				errorMessage = GetErrorString(result);
				return false;
			}

			// Open the stream and scan it to find all tags, etc.
			result = OpenFile(moduleStream);
			if (result != Mpg123_Errors.Ok)
			{
				errorMessage = GetErrorString(result);
				return false;
			}

			// Scan the whole file to find meta data etc.
			result = mpg123Handle.Mpg123_Scan();
			if (result != Mpg123_Errors.Ok)
			{
				errorMessage = GetErrorString(result);
				return false;
			}

			// Extract all the information to show
			if (!ExtractMetaInformation(out errorMessage))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public override void CleanupPlayer()
		{
			frameInfo = null;

			if (mpg123Handle != null)
			{
				mpg123Handle.Mpg123_Close();

				mpg123Handle.Mpg123_Delete();
				mpg123Handle = null;
			}

			base.CleanupPlayer();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the player to start the sample from start
		/// </summary>
		/********************************************************************/
		public override bool InitSound(out string errorMessage)
		{
			if (!base.InitSound(out errorMessage))
				return false;

			// Initialize some variables
			oldBitRate = 0;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Will load and decode a data block and store it in the buffer
		/// given
		/// </summary>
		/********************************************************************/
		public override int LoadDataBlock(int[] outputBuffer, int count)
		{
			// Load the next block of data
			int filled = LoadData(outputBuffer, count);

			if (filled == 0)
			{
				OnEndReached();

				// Loop the sample
				CleanupSound();
				InitSound(out _);
			}

			// Has the bit rate changed (mostly on VBR streams)
			if (mpg123Handle.Mpg123_Info(out Mpg123_FrameInfo newFrameInfo) == Mpg123_Errors.Ok)
			{
				if (newFrameInfo.BitRate != oldBitRate)
				{
					oldBitRate = newFrameInfo.BitRate;

					OnModuleInfoChanged(InfoBitRateLine, newFrameInfo.BitRate.ToString());
				}
			}

			return filled;
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the sample uses
		/// </summary>
		/********************************************************************/
		public override int ChannelCount => frameInfo.Mode == Mpg123_Mode.Mono ? 1 : 2;



		/********************************************************************/
		/// <summary>
		/// Return the frequency the sample is stored with
		/// </summary>
		/********************************************************************/
		public override int Frequency => frameInfo.Rate;
		#endregion

		#region SamplePlayerWithDurationAgentBase
		/********************************************************************/
		/// <summary>
		/// Return the total time of the sample
		/// </summary>
		/********************************************************************/
		protected override TimeSpan GetTotalDuration()
		{
			long numberOfSamples = mpg123Handle.Mpg123_Length64();
			double totalTime = (double)numberOfSamples / frameInfo.Rate;

			return new TimeSpan((long)(totalTime * TimeSpan.TicksPerSecond));
		}



		/********************************************************************/
		/// <summary>
		/// Set the position in the playing sample to the time given
		/// </summary>
		/********************************************************************/
		protected override void SetPosition(TimeSpan time)
		{
			long newPos = (long)(frameInfo.Rate * time.TotalSeconds);
			mpg123Handle.Mpg123_Seek64(newPos, SeekOrigin.Begin);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Helper method to get the string of an error
		/// </summary>
		/********************************************************************/
		private string GetErrorString(Mpg123_Errors error)
		{
			if (error == Mpg123_Errors.Err)
				return mpg123Handle.Mpg123_StrError();

			return mpg123Handle.Mpg123_Plain_StrError(error);
		}



		/********************************************************************/
		/// <summary>
		/// Skip blocks of zeros
		/// </summary>
		/********************************************************************/
		private long SkipZeroes(ModuleStream moduleStream)
		{
			byte[] buf = new byte[4096];

			moduleStream.Seek(0, SeekOrigin.Begin);
			long bufferStartPosition = 0;

			while (!moduleStream.EndOfStream)
			{
				int read = moduleStream.Read(buf, 0, buf.Length);

				for (int i = 0; i < read; i++)
				{
					if (buf[i] != 0)
					{
						moduleStream.Seek(bufferStartPosition + i, SeekOrigin.Begin);
						return moduleStream.Position;
					}
				}

				bufferStartPosition += read;
			}

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// Open the file
		/// </summary>
		/********************************************************************/
		private Mpg123_Errors OpenFile(ModuleStream moduleStream)
		{
			// Skip blocks of zeros before checking
			long startPosition = SkipZeroes(moduleStream);
			if (startPosition < 0)
				return Mpg123_Errors.Err;

			Stream stream = startPosition == 0 ? moduleStream : new SliceStream(moduleStream, true, startPosition, moduleStream.Length - startPosition);

			Mpg123_Errors result = mpg123Handle.Mpg123_Reader64(Read, Seek, Close);
			if (result != Mpg123_Errors.Ok)
				return result;

			return mpg123Handle.Mpg123_Open_Handle(stream);
		}



		/********************************************************************/
		/// <summary>
		/// Read from the file
		/// </summary>
		/********************************************************************/
		private int Read(object handle, Memory<byte> buf, ulong count, out ulong readCount)
		{
			Stream stream = (Stream)handle;

			readCount = (ulong)stream.Read(buf.Span.Slice(0, (int)count));

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Seek in the file
		/// </summary>
		/********************************************************************/
		private long Seek(object handle, long offset, SeekOrigin whence)
		{
			Stream stream = (Stream)handle;

			return stream.Seek(offset, whence);
		}



		/********************************************************************/
		/// <summary>
		/// Close the file
		/// </summary>
		/********************************************************************/
		private void Close(object handle)
		{
			if (handle is SliceStream sliceStream)
				sliceStream.Dispose();
		}



		/********************************************************************/
		/// <summary>
		/// Tests the module to see which type of module it is
		/// </summary>
		/********************************************************************/
		private ModuleType TestModule(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Some other module formats have a lot of data that can be parsed as a MPEG file.
			// To prevent false positives, we check some for these module formats and will
			// then skip the MPEG test below if such a module is found
			if (CheckModuleFormats(moduleStream))
				return ModuleType.Unknown;

			mpg123Handle = LibMpg123.Mpg123_New(null, out Mpg123_Errors error);
			if (error != Mpg123_Errors.Ok)
				return ModuleType.Unknown;

			try
			{
				Mpg123_Errors result = mpg123Handle.Mpg123_Param(Mpg123_Parms.Add_Flags, (int)(Mpg123_Param_Flags.No_Frankenstein | Mpg123_Param_Flags.No_Resync), 0);
				if (result != Mpg123_Errors.Ok)
					return ModuleType.Unknown;

				// Open the stream and scan it to find all tags, etc.
				result = OpenFile(moduleStream);
				if (result != Mpg123_Errors.Ok)
					return ModuleType.Unknown;

				// Read first frame
				result = mpg123Handle.Mpg123_FrameByFrame_Next();
				if (result != Mpg123_Errors.New_Format)
					return ModuleType.Unknown;

				// Get frame information
				result = mpg123Handle.Mpg123_Info(out Mpg123_FrameInfo fi);
				if (result != Mpg123_Errors.Ok)
					return ModuleType.Unknown;

				// We need ok frames in the first Kb and 20 frames after that
				long minPosition = moduleStream.Position + 1024;
				int foundFrames = 0;

				for (int i = 0; i < 8; i++)
				{
					result = mpg123Handle.Mpg123_FrameByFrame_Next();
					if (result == Mpg123_Errors.Done)
						break;

					if (result != Mpg123_Errors.Ok)
						return ModuleType.Unknown;

					if (fi.BitRate == 0)	// Free format
					{
						result = mpg123Handle.Mpg123_Info(out Mpg123_FrameInfo info);
						if (result != Mpg123_Errors.Ok)
							return ModuleType.Unknown;

						if (info.FrameSize < 32)
							return ModuleType.Unknown;
					}

					if (moduleStream.Position < minPosition)
						i--;

					foundFrames++;
				}

				if (foundFrames < 5)
					return ModuleType.Unknown;

				switch (fi.Version)
				{
					case Mpg123_Version._1_0:
						return ModuleType.Mpeg10;

					case Mpg123_Version._2_0:
						return ModuleType.Mpeg20;

					case Mpg123_Version._2_5:
						return ModuleType.Mpeg25;
				}

				return ModuleType.Unknown;
			}
			finally
			{
				mpg123Handle.Mpg123_Close();
				mpg123Handle.Mpg123_Delete();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Check for different module formats that should be ignored
		/// </summary>
		/********************************************************************/
		private bool CheckModuleFormats(ModuleStream moduleStream)
		{
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint id1 = moduleStream.Read_B_UINT32();
			uint id2 = moduleStream.Read_B_UINT32();

			// If starts with ID3, then it is probably a valid file
			if ((id1 & 0xffffff00) == 0x49443300)
				return false;

			// Beepola
			if ((id1 == 0x4242534f) && ((id2 & 0xffff0000) == 0x4e470000))	// BBSONG
				return true;

			// Funktracker
			if (id1 == 0x46756e6b)											// Funk
				return true;

			// Nintendo Sound Format
			if (id1 == 0x4e45534d)											// NESM
				return true;

			// Nintendo SPC
			if (id1 == 0x534e4553)											// SNES
				return true;

			// SC68
			if (id1 == 0x53433638)											// SC68
				return true;

			// Impulse Tracker
			if (id1 == 0x494d504d)											// IMPM
				return true;

			// Hippel COSO
			if (id1 == 0x434f534f)											// COSO
				return true;

			// Gameboy Sound System
			uint maskedId = id1 & 0xffffff00;

			if (maskedId == 0x47425300)										// GBS
				return true;

			// Graoumf Tracker
			if (maskedId == 0x47544b00)										// GTK
				return true;

			// Graoumf Tracker 2
			if (maskedId == 0x47543200)										// GT2
				return true;

			// MO3
			if (maskedId == 0x4d4f3300)										// MO3
				return true;

			// YM
			if (maskedId == 0x594d3500)										// YM5
				return true;

			// Delitracker custom
			if (id1 == 0x000003f3)
			{
				moduleStream.Seek(8, SeekOrigin.Begin);

				id1 = moduleStream.Read_B_UINT32();
				moduleStream.Seek((id1 + 1) * 4 + 16, SeekOrigin.Current);

				id1 = moduleStream.Read_B_UINT32();
				id2 = moduleStream.Read_B_UINT32();

				if ((id1 == 0x44454c49) && (id2 == 0x5249554d))				// DELIRIUM
					return true;
			}

			// The Musical Enlightenment
			if (moduleStream.Length > (0x1a8a + 32))
			{
				if ((id2 & 0x000000ff) == 0x1f)
				{
					moduleStream.Seek(0x1a8a, SeekOrigin.Begin);

					byte[] buf = new byte[32];
					moduleStream.Read(buf, 0, 32);

					if (buf[0] != 0x00)
					{
						bool invalid = false;
						int uCount = 0;

						for (int i = buf.Length - 1; i > 0; i--)
						{
							if ((buf[i] != 0x00) && ((buf[i] < 0x20) || (buf[i] > 0x7f)))
							{
								invalid = true;
								break;
							}

							if (buf[i] == 0x55)
								uCount++;
						}

						if (!invalid && (uCount != (buf.Length - 1)))
							return true;
					}
				}
			}

			// Maniacs of Noise
			if (moduleStream.Length > 1024)
			{
				moduleStream.Seek(0, SeekOrigin.Begin);

				byte[] buf = new byte[1024];
				moduleStream.Read(buf, 0, 1024);

				byte[] compareBuf = "Maniacs of Noise"u8.ToArray();

				int i, j;
				for (i = 0, j = 0; i < buf.Length - compareBuf.Length; i++)
				{
					if (buf[i] == compareBuf[j])
					{
						j++;
						if (j == compareBuf.Length)
							return true;
					}
					else
						j = 0;
				}
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Will extract all meta information if any
		/// </summary>
		/********************************************************************/
		private bool ExtractMetaInformation(out string errorMessage)
		{
			errorMessage = string.Empty;

			Mpg123_Errors result = mpg123Handle.Mpg123_Info(out frameInfo);
			if (result != Mpg123_Errors.Ok)
			{
				errorMessage = GetErrorString(result);
				return false;
			}

			numberOfFrames = mpg123Handle.Mpg123_FrameLength64();
			if (numberOfFrames < 0)
				numberOfFrames = 0;

			result = mpg123Handle.Mpg123_Index64(out long[] offsets, out _, out _);
			if (result == Mpg123_Errors.Ok)
				firstFramePosition = offsets[0];
			else
				firstFramePosition = 0;

			// Clear ID3 values first
			songName = string.Empty;
			artist = string.Empty;
			album = string.Empty;
			year = string.Empty;
			comment = string.Empty;
			genre = string.Empty;
			trackNum = 0;

			result = mpg123Handle.Mpg123_Id3(out Mpg123_Id3V1 v1, out Mpg123_Id3V2 v2);
			if (result == Mpg123_Errors.Ok)
			{
				if (v2 != null)
					ExtractId3V2Tags(v2);
				else if (v1 != null)
					ExtractId3V1Tags(v1);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Copy the ID3v1 tags
		/// </summary>
		/********************************************************************/
		private void ExtractId3V1Tags(Mpg123_Id3V1 v1)
		{
			Encoding encoder = EncoderCollection.Win1252;

			// Song name
			songName = encoder.GetString(v1.Title).Trim();

			// Artist
			artist = encoder.GetString(v1.Artist).Trim();

			// Album
			album = encoder.GetString(v1.Album).Trim();

			// Year
			year = encoder.GetString(v1.Year).Trim();

			// Track number
			if (v1.Comment[28] == 0x00)
				trackNum = v1.Comment[29];
			else
				trackNum = 0;

			// Comment
			comment = encoder.GetString(v1.Comment).Trim();

			// Genre
			if (v1.Genre < 192)
				genre = LookupTables.Genre[v1.Genre];
		}



		/********************************************************************/
		/// <summary>
		/// Copy the ID3v2 tags
		/// </summary>
		/********************************************************************/
		private void ExtractId3V2Tags(Mpg123_Id3V2 v2)
		{
			songName = GetId3V2String(v2.Title);
			artist = GetId3V2String(v2.Artist);
			album = GetId3V2String(v2.Album);
			year = GetId3V2String(v2.Year);
			comment = GetId3V2String(v2.Comment);
			genre = ProcessGenre(GetId3V2String(v2.Genre));
			pictures = ProcessPictures(v2.Picture, v2.Pictures);
		}



		/********************************************************************/
		/// <summary>
		/// Parse the given Mpg123_String and return it as a string
		/// </summary>
		/********************************************************************/
		private string GetId3V2String(Mpg123_String mpg123String)
		{
			StringBuilder sb = new StringBuilder();

			if (mpg123String != null)
			{
				Encoding encoder = Encoding.UTF8;

				if (mpg123String.Fill != 0)
				{
					byte[] lines = mpg123String.P;
					int len = (int)mpg123String.Fill;
					const string NewLine = " * ";

					int hadCr = 0;
					int hadLf = 0;
					int startOffset = 0;

					for (int i = 0; i < len; i++)
					{
						if ((lines[i] == '\n') || (lines[i] == '\r') || (lines[i] == 0))
						{
							if (lines[i] == '\n')
								hadLf++;

							if (lines[i] == '\r')
								hadCr++;

							if (((hadLf != 0) || (hadCr != 0)) && (hadLf % 2 == 0) && (hadCr % 2 == 0))
								sb.Append(NewLine);
							else
							{
								if (i > startOffset)
									sb.Append(encoder.GetString(lines, startOffset, i - startOffset) + NewLine);
							}

							startOffset = i + 1;
						}
						else
							hadLf = hadCr = 0;
					}
				}
			}

			return sb.Length == 0 ? string.Empty : sb.ToString(0, sb.Length - 3);	// 3 -> size of newLine
		}



		/********************************************************************/
		/// <summary>
		/// Process the genre string to generate the real genre
		/// </summary>
		/********************************************************************/
		private string ProcessGenre(string id3Genre)
		{
			if (string.IsNullOrEmpty(id3Genre))
				return string.Empty;

			Regex regex = new Regex(@"(\(\d+\))");
			MatchCollection matches = regex.Matches(id3Genre);
			if (matches.Count == 0)
				return id3Genre;

			StringBuilder sb = new StringBuilder();

			int startOffset = 0;
			foreach (Match match in matches)
			{
				sb.Append(id3Genre.Substring(startOffset, match.Index - startOffset));

				int genre = int.Parse(id3Genre.Substring(match.Index + 1, match.Length - 2));
				if (genre < LookupTables.Genre.Length)
				{
					sb.Append(' ');
					sb.Append(LookupTables.Genre[genre]);
					sb.Append(' ');
				}

				startOffset = match.Index + match.Length;
			}

			return sb.ToString().Trim();
		}



		/********************************************************************/
		/// <summary>
		/// Process the pictures
		/// </summary>
		/********************************************************************/
		private PictureInfo[] ProcessPictures(Mpg123_Picture[] allPictures, ulong count)
		{
			List<PictureInfo> result = new List<PictureInfo>((int)count);

			for (ulong i = 0; i < count; i++)
			{
				Mpg123_Picture picture = allPictures[i];

				string mimeType = GetId3V2String(picture.Mime_Type);
				if (mimeType == "-->")	// URL, we do not support that
					continue;

				string type = GetTypeDescription(picture.Type);

				string description = GetId3V2String(picture.Description);
				if (string.IsNullOrEmpty(description))
					description = type;
				else
					description = $"{type}: {description}";

				result.Add(new PictureInfo(picture.Data, description));
			}

			return result.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Find the picture type string
		/// </summary>
		/********************************************************************/
		private string GetTypeDescription(Mpg123_Id3_Pic_Type type)
		{
			switch (type)
			{
				case Mpg123_Id3_Pic_Type.Other:
					return Resources.IDS_MPG_PICTURE_TYPE_OTHER;

				case Mpg123_Id3_Pic_Type.Icon:
					return Resources.IDS_MPG_PICTURE_TYPE_ICON;

				case Mpg123_Id3_Pic_Type.Other_Icon:
					return Resources.IDS_MPG_PICTURE_TYPE_OTHER_ICON;

				case Mpg123_Id3_Pic_Type.Front_Cover:
					return Resources.IDS_MPG_PICTURE_TYPE_FRONT_COVER;

				case Mpg123_Id3_Pic_Type.Back_Cover:
					return Resources.IDS_MPG_PICTURE_TYPE_BACK_COVER;

				case Mpg123_Id3_Pic_Type.Leaflet:
					return Resources.IDS_MPG_PICTURE_TYPE_LEAFLET;

				case Mpg123_Id3_Pic_Type.Media:
					return Resources.IDS_MPG_PICTURE_TYPE_MEDIA;

				case Mpg123_Id3_Pic_Type.Lead:
					return Resources.IDS_MPG_PICTURE_TYPE_LEAD;

				case Mpg123_Id3_Pic_Type.Artist:
					return Resources.IDS_MPG_PICTURE_TYPE_ARTIST;

				case Mpg123_Id3_Pic_Type.Conductor:
					return Resources.IDS_MPG_PICTURE_TYPE_CONDUCTOR;

				case Mpg123_Id3_Pic_Type.Orchestra:
					return Resources.IDS_MPG_PICTURE_TYPE_ORCHESTRA;

				case Mpg123_Id3_Pic_Type.Composer:
					return Resources.IDS_MPG_PICTURE_TYPE_COMPOSER;

				case Mpg123_Id3_Pic_Type.Lyricist:
					return Resources.IDS_MPG_PICTURE_TYPE_LYRICIST;

				case Mpg123_Id3_Pic_Type.Location:
					return Resources.IDS_MPG_PICTURE_TYPE_LOCATION;

				case Mpg123_Id3_Pic_Type.Recording:
					return Resources.IDS_MPG_PICTURE_TYPE_RECORDING;

				case Mpg123_Id3_Pic_Type.Performance:
					return Resources.IDS_MPG_PICTURE_TYPE_PERFORMANCE;

				case Mpg123_Id3_Pic_Type.Video:
					return Resources.IDS_MPG_PICTURE_TYPE_VIDEO;

				case Mpg123_Id3_Pic_Type.Fish:
					return Resources.IDS_MPG_PICTURE_TYPE_FISH;

				case Mpg123_Id3_Pic_Type.Illustration:
					return Resources.IDS_MPG_PICTURE_TYPE_ILLUSTRATION;

				case Mpg123_Id3_Pic_Type.Artist_Logo:
					return Resources.IDS_MPG_PICTURE_TYPE_ARTIST_LOGO;

				case Mpg123_Id3_Pic_Type.Publisher_Logo:
					return Resources.IDS_MPG_PICTURE_TYPE_PUBLISHER_LOGO;
			}

			return string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Read next block of data
		/// </summary>
		/********************************************************************/
		private int LoadData(int[] outputBuffer, int count)
		{
			Span<byte> outBuf = MemoryMarshal.Cast<int, byte>(outputBuffer);
			int total = 0;
			int todo = count;

			while (todo > 0)
			{
				Mpg123_Errors result = mpg123Handle.Mpg123_Read(outBuf, (ulong)todo * 4, out ulong done);
				if ((result == Mpg123_Errors.Ok) || (result == Mpg123_Errors.Done))
				{
					if (done == 0)
					{
						// Done with the stream
						break;
					}

					outBuf = outBuf.Slice((int)done);
					done /= 4;
					todo -= (int)done;
					total += (int)done;
				}
				else
				{
					if (result == Mpg123_Errors.New_Format)
					{
						// Just ignore this one, since we don't need the new info
					}
					else
					{
						// Some error occurred, so stop
						break;
					}
				}
			}

			return total;
		}
		#endregion
	}
}
