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
using Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

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
		private off_t numberOfFrames;
		private long firstFramePosition;

		private string songName;
		private string artist;
		private string album;
		private string year;
		private string comment;
		private string genre;
		private byte trackNum;

		private LibMpg123.LibMpg123 mpg123Handle;

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
			mpg123Handle = LibMpg123.LibMpg123.Mpg123_New(null, out Mpg123_Errors error);
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

			mpg123Handle.Mpg123_Rates(out c_long[] supportedRates, out size_t number);
			if (number > 0)
			{
				// Set the output format to 32-bit on every rate
				foreach (c_long rate in supportedRates)
				{
					result = mpg123Handle.Mpg123_Format(rate, Mpg123_ChannelCount.Mono | Mpg123_ChannelCount.Stereo, Mpg123_Enc_Enum.Enc_Signed_32);
					if (result != Mpg123_Errors.Ok)
					{
						errorMessage = GetErrorString(result);
						return false;
					}
				}
			}

			// Reset the stream to the beginning
			moduleStream.Seek(0, SeekOrigin.Begin);

			// Open the stream and scan it to find all tags, etc.
			result = mpg123Handle.Mpg123_Open_Fd(moduleStream);
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
			off_t numberOfSamples = mpg123Handle.Mpg123_Length();
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
			int newPos = (int)(frameInfo.Rate * time.TotalSeconds);
			mpg123Handle.Mpg123_Seek(newPos, SeekOrigin.Begin);
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
		/// Tests the module to see which type of module it is
		/// </summary>
		/********************************************************************/
		private ModuleType TestModule(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			mpg123Handle = LibMpg123.LibMpg123.Mpg123_New(null, out Mpg123_Errors error);
			if (error != Mpg123_Errors.Ok)
				return ModuleType.Unknown;

			try
			{
				moduleStream.Seek(0, SeekOrigin.Begin);

				// Open the stream and scan it to find all tags, etc.
				Mpg123_Errors result = mpg123Handle.Mpg123_Open_Fd(moduleStream);
				if (result != Mpg123_Errors.Ok)
					return ModuleType.Unknown;

				// We need 10 ok frames
				for (int i = 0; i < 10; i++)
				{
					result = mpg123Handle.Mpg123_FrameByFrame_Next();
					if ((result != Mpg123_Errors.Ok) && (result != Mpg123_Errors.New_Format))
						return ModuleType.Unknown;
				}

				result = mpg123Handle.Mpg123_Info(out Mpg123_FrameInfo fi);
				if (result != Mpg123_Errors.Ok)
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

			numberOfFrames = mpg123Handle.Mpg123_FrameLength();
			if (numberOfFrames < 0)
				numberOfFrames = 0;

			result = mpg123Handle.Mpg123_Index(out off_t[] offsets, out _, out _);
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
				Mpg123_Errors result = mpg123Handle.Mpg123_Read(outBuf, (size_t)todo * 4, out size_t done);
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
