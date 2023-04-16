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

		private static readonly double[] bs = { 0.0, 384.0, 1152.0, 1152.0 };

		private const int CheckBufSize = 8 * 1024;
		private const int CheckFrames = 10;

		private const int InputBufSize = 32 * 1024;
		private const int OutputBufSize = 128 * 1024;

		private readonly ModuleType currentModuleType;

		private long firstFramePosition;
		private Frame firstFrame;

		private long calculatedFileLength;
		private int numberOfFrames;

		private bool isVbr;
		private int vbrFrames;
		private int vbrTotalBytes;
		private int vbrScale;
		private byte[] vbrToc;

		private int frequency;
		private int bitRateMode;

		private int oldBitRate;

		private string songName;
		private string artist;
		private string album;
		private string year;
		private string comment;
		private string genre;
		private byte trackNum;

		private IntPtr mpg123Handle;
		private byte[] inputBuffer;
		private byte[] tempOutputBuffer;

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

			mpg123Handle = IntPtr.Zero;
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
					value = firstFrame.Lay.ToString();
					break;
				}

				// Bit rate
				case 6:
				{
					description = Resources.IDS_MPG_INFODESCLINE06;
					value = LookupTables.TabSel123[firstFrame.Lsf, firstFrame.Lay - 1, firstFrame.BitRateIndex].ToString();
					break;
				}

				// Frequency
				case 7:
				{
					description = Resources.IDS_MPG_INFODESCLINE07;
					value = frequency.ToString();
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

					switch (firstFrame.Mode)
					{
						default:
						case Mode.Mono:
						{
							value = Resources.IDS_MPG_INFO_CHANNEL_MONO;
							break;
						}

						case Mode.Stereo:
						{
							value = Resources.IDS_MPG_INFO_CHANNEL_STEREO;
							break;
						}

						case Mode.JointStereo:
						{
							value = Resources.IDS_MPG_INFO_CHANNEL_JOINT;
							break;
						}

						case Mode.DualChannel:
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
					value = firstFrame.Extension != 0 ? Resources.IDS_MPG_INFO_YES : Resources.IDS_MPG_INFO_NO;
					break;
				}

				// CRCs
				case 12:
				{
					description = Resources.IDS_MPG_INFODESCLINE12;
					value = firstFrame.ErrorProtection != 0 ? Resources.IDS_MPG_INFO_YES : Resources.IDS_MPG_INFO_NO;
					break;
				}

				// Copyrighted
				case 13:
				{
					description = Resources.IDS_MPG_INFODESCLINE13;
					value = firstFrame.Copyright != 0 ? Resources.IDS_MPG_INFO_YES : Resources.IDS_MPG_INFO_NO;
					break;
				}

				// Original
				case 14:
				{
					description = Resources.IDS_MPG_INFODESCLINE14;
					value = firstFrame.Original != 0 ? Resources.IDS_MPG_INFO_YES : Resources.IDS_MPG_INFO_NO;
					break;
				}

				// Emphasis
				case 15:
				{
					description = Resources.IDS_MPG_INFODESCLINE15;

					switch (firstFrame.Emphasis)
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

			// Load ID3 tags if any
			if (firstFramePosition > 0)
			{
				// If non-zero, an ID3v2 header could have been skipped
				// if the identifying, so check for that
				if (!GetId3V2Tags(moduleStream))
				{
					// Could not find ID3v2 tag, so check for v1
					GetId3V1Tags(moduleStream);
				}
			}
			else
				GetId3V1Tags(moduleStream);

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
			mpg123Handle = Native.mpg123_new(null, out int error);
			if (error != Native.mpg123_errors.MPG123_OK)
			{
				errorMessage = Native.GetErrorString(mpg123Handle, error);
				return false;
			}

			// Make sure, that the output is always in 32-bit for every sample rate
			int result = Native.mpg123_format_none(mpg123Handle);
			if (result != Native.mpg123_errors.MPG123_OK)
			{
				errorMessage = Native.GetErrorString(mpg123Handle, error);
				return false;
			}

			Native.mpg123_rates(out IntPtr list, out uint number);
			if (number > 0)
			{
				// Copy the unmanaged array into a managed array
				int[] supportedRates = new int[number];
				Marshal.Copy(list, supportedRates, 0, supportedRates.Length);

				// Set the output format to 32-bit on every rate
				foreach (int rate in supportedRates)
				{
					result = Native.mpg123_format(mpg123Handle, rate, Native.mpg123_channelcount.MPG123_MONO | Native.mpg123_channelcount.MPG123_STEREO, Native.mpg123_enc_enum.MPG123_ENC_SIGNED_32);
					if (result != Native.mpg123_errors.MPG123_OK)
					{
						errorMessage = Native.GetErrorString(mpg123Handle, error);
						return false;
					}
				}
			}

			// Remember the frequency to play with
			frequency = LookupTables.Freqs[firstFrame.SamplingFrequency];

			// Allocate buffers
			inputBuffer = new byte[InputBufSize];
			tempOutputBuffer = new byte[OutputBufSize];

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public override void CleanupPlayer()
		{
			firstFrame = null;

			inputBuffer = null;
			tempOutputBuffer = null;

			if (mpg123Handle != IntPtr.Zero)
			{
				Native.mpg123_delete(mpg123Handle);
				mpg123Handle = IntPtr.Zero;
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

			// Reset the sample position
			modStream.Seek(firstFramePosition, SeekOrigin.Begin);

			// Initialize Mpg123
			int result = Native.mpg123_open_feed(mpg123Handle);
			if (result != Native.mpg123_errors.MPG123_OK)
			{
				errorMessage = Native.GetErrorString(mpg123Handle, result);
				return false;
			}

			// Tell Mpg123 about the total file size
			result = Native.mpg123_set_filesize(mpg123Handle, (int)calculatedFileLength);
			if (result != Native.mpg123_errors.MPG123_OK)
			{
				errorMessage = Native.GetErrorString(mpg123Handle, result);
				return false;
			}

			// Calculate the number of frames
			numberOfFrames = isVbr ? vbrFrames : (int)(calculatedFileLength / (firstFrame.FrameSize + 4));

			// Initialize some variables
			oldBitRate = -1;
			bitRateMode = -1;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the current song
		/// </summary>
		/********************************************************************/
		public override void CleanupSound()
		{
			Native.mpg123_close(mpg123Handle);

			base.CleanupSound();
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
			if (Native.mpg123_info2(mpg123Handle, out Native.mpg123_frameinfo2 frameInfo) == Native.mpg123_errors.MPG123_OK)
			{
				if ((frameInfo.bitrate != oldBitRate) || (bitRateMode == -1))
				{
					oldBitRate = frameInfo.bitrate;
					bitRateMode = frameInfo.vbr;

					OnModuleInfoChanged(InfoBitRateLine, frameInfo.bitrate.ToString());
				}
			}

			return filled;
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the sample uses
		/// </summary>
		/********************************************************************/
		public override int ChannelCount => firstFrame.Mode == Mode.Mono ? 1 : 2;



		/********************************************************************/
		/// <summary>
		/// Return the frequency the sample is stored with
		/// </summary>
		/********************************************************************/
		public override int Frequency => frequency;
		#endregion

		#region SamplePlayerWithDurationAgentBase
		/********************************************************************/
		/// <summary>
		/// Return the total time of the sample
		/// </summary>
		/********************************************************************/
		protected override TimeSpan GetTotalDuration()
		{
			double totalTime;

			if (isVbr)
				totalTime = bs[firstFrame.Lay] / LookupTables.Freqs[firstFrame.SamplingFrequency] * vbrFrames;
			else
				totalTime = (double)calculatedFileLength / ((LookupTables.TabSel123[firstFrame.Lsf, firstFrame.Lay - 1, firstFrame.BitRateIndex] / 8) * 1000);

			if (totalTime == 0.0f)
			{
				// Could not calculate the duration
				return TimeSpan.Zero;

				// TODO: Fix this when mpg123 has been converted to C#. Then I can use a different approch than
				// feeding, e.g. open from a C# stream directly. Then I can do a mpg123_scan() here to find the time
			}

			return new TimeSpan((long)(totalTime * TimeSpan.TicksPerSecond));
		}



		/********************************************************************/
		/// <summary>
		/// Set the position in the playing sample to the time given
		/// </summary>
		/********************************************************************/
		protected override void SetPosition(TimeSpan time)
		{
			int newPos = (int)(frequency * time.TotalSeconds);
			int result = Native.mpg123_feedseek(mpg123Handle, newPos, 0/*SEEK_SET*/, out int inputOffset);
			if (result >= 0)
				modStream.Seek(firstFramePosition + inputOffset, SeekOrigin.Begin);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Tests the module to see which type of module it is
		/// </summary>
		/********************************************************************/
		private ModuleType TestModule(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Start at the beginning of the file
			moduleStream.Seek(0, SeekOrigin.Begin);
			firstFramePosition = 0;

			// Is the file an ID3 file, then skip it
			if ((moduleStream.Read_UINT8() == 0x49) && (moduleStream.Read_UINT8() == 0x44) && (moduleStream.Read_UINT8() == 0x33))		// ID3
			{
				byte[] buf = new byte[4];

				// Skip the version and flag byte
				moduleStream.Seek(3, SeekOrigin.Current);

				// Read the tag size
				moduleStream.Read(buf, 0, 4);

				// Remove bit 7 from all the bytes and calculate the size value
				firstFramePosition = ((buf[0] & 0x7f) << 21) | ((buf[1] & 0x7f) << 14) | ((buf[2] & 0x7f) << 7) | (buf[3] & 0x7f);
				firstFramePosition += 10;		// Add the bytes we already have read
			}

			// Well, some MP3's have a huge block of zeros in the beginning.
			// We try to find the end of this block first and then scan
			byte[] chkBuf = new byte[CheckBufSize];

			// Seek to first frame position
			moduleStream.Seek(firstFramePosition, SeekOrigin.Begin);

			for (;;)
			{
				// Read one block
				int bytesRead = moduleStream.Read(chkBuf, 0, CheckBufSize);
				if (bytesRead == 0)
					return ModuleType.Unknown;

				// Check the read block to see if it only contains zeros
				for (int i = 0; i < bytesRead; i++)
				{
					if (chkBuf[i] != 0)
					{
						// Found a block which has other values than zero
						goto exitLoop;
					}
				}

				// Well, increment the frame position
				firstFramePosition += bytesRead;
			}
			exitLoop:

			// Read the first 4 bytes into the check variable
			moduleStream.Seek(firstFramePosition, SeekOrigin.Begin);
			uint header = moduleStream.Read_B_UINT32();
			int pos = 4;
			bool found = false;

			for (;;)
			{
				// Try to find a header in the next 8 Kb
				for (; pos < CheckBufSize; pos++)
				{
					if (HeadCheck(header))
					{
						found = true;
						break;
					}

					// Read the next byte and shift it into the check variable
					header <<= 8;
					header &= 0xffffff00;
					header |= moduleStream.Read_UINT8();
				}

				// We didn't find a header, so we give up
				if (!found)
					return ModuleType.Unknown;

				Frame frame = new Frame();

				// Decode the header
				DecodeHeader(frame, header);

				// Ok, it seems we found a header. Check if it is a Xing header (VBR from LAME)
				if (!DecodeVbrHeader(moduleStream, frame))
				{
					// It is not. We will then try to find some more headers with the right
					// space between them
					for (int i = 0; i < CheckFrames; i++)
					{
						// Seek to the next frame header
						moduleStream.Seek(frame.FrameSize, SeekOrigin.Current);

						// Read the header
						header = moduleStream.Read_B_UINT32();
						if (moduleStream.EndOfStream)
							return ModuleType.Unknown;

						// Check the new header
						if (!HeadCheck(header))
						{
							found = false;

							// Set the file position back
							moduleStream.Seek(firstFramePosition + pos - 3, SeekOrigin.Begin);

							// Make sure we don't make a double check
							header = moduleStream.Read_B_UINT32();
							pos++;
							break;
						}

						// Decode the header
						DecodeHeader(frame, header);

						if (frame.FrameSize == 0)
							i--;	// Do not count empty frames
					}
				}

				if (found)
				{
					firstFramePosition += (pos - 4);
					firstFrame = frame;

					// Calculate the file length
					calculatedFileLength = moduleStream.Length - firstFramePosition;
					if (HasId3V1Tags(moduleStream))
						calculatedFileLength -= 128;

					// Yeah, we found a mpeg file, now find out which kind
					if (frame.Mpeg25)
						return ModuleType.Mpeg25;

					if (frame.Lsf == 0)
						return ModuleType.Mpeg10;

					return ModuleType.Mpeg20;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will check the header to see if it's valid
		/// </summary>
		/********************************************************************/
		private bool HeadCheck(uint head)
		{
			// Check sync
			if ((head & 0xffe00000) != 0xffe00000)
				return false;

			// Check version
			if (((head >> 19) & 3) == 1)
				return false;

			// Check layer
			if (((head >> 17) & 3) == 0)
				return false;

			// Check bit rate
			if (((head >> 12) & 0xf) == 0xf)
				return false;

			// Check frequency
			if (((head >> 10) & 0x3) == 0x3)
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Will read the rest of the frame header and decode it
		/// </summary>
		/********************************************************************/
		private void DecodeHeader(Frame frame, uint newHead)
		{
			if ((newHead & (1 << 20)) != 0)
			{
				frame.Lsf = (newHead & (1 << 19)) != 0 ? 0 : 1;
				frame.Mpeg25 = false;
			}
			else
			{
				frame.Lsf = 1;
				frame.Mpeg25 = true;
			}

			frame.Lay = (int)(4 - ((newHead >> 17) & 3));

			if (frame.Mpeg25)
				frame.SamplingFrequency = (int)(6 + ((newHead >> 10) & 0x3));
			else
				frame.SamplingFrequency = (int)(((newHead >> 10) & 0x3) + (frame.Lsf * 3));

			frame.ErrorProtection = (int)(((newHead >> 16) & 0x1) ^ 0x1);

			frame.BitRateIndex = (int)((newHead >> 12) & 0xf);
			frame.Padding = (int)((newHead >> 9) & 0x1);
			frame.Extension = (int)((newHead >> 8) & 0x1);
			frame.Mode = (Mode)((newHead >> 6) & 0x3);
			frame.ModeExt = (int)((newHead >> 4) & 0x3);
			frame.Copyright = (int)((newHead >> 3) & 0x1);
			frame.Original = (int)((newHead >> 2) & 0x1);
			frame.Emphasis = (int)(newHead & 0x3);

			frame.Stereo = frame.Mode == Mode.Mono ? 1 : 2;

			switch (frame.Lay)
			{
				case 1:
				{
					frame.FrameSize = LookupTables.TabSel123[frame.Lsf, 0, frame.BitRateIndex] * 12000;
					frame.FrameSize /= LookupTables.Freqs[frame.SamplingFrequency];
					frame.FrameSize = ((frame.FrameSize + frame.Padding) << 2) - 4;
					frame.SideInfoSize = 0;
					frame.PadSize = frame.Padding << 2;
					break;
				}

				case 2:
				{
					frame.FrameSize = LookupTables.TabSel123[frame.Lsf, 1, frame.BitRateIndex] * 144000;
					frame.FrameSize /= LookupTables.Freqs[frame.SamplingFrequency];
					frame.FrameSize += frame.Padding - 4;
					frame.SideInfoSize = 0;
					frame.PadSize = frame.Padding;
					break;
				}

				case 3:
				{
					if (frame.Lsf != 0)
						frame.SideInfoSize = (frame.Stereo == 1) ? 9 : 17;
					else
						frame.SideInfoSize = (frame.Stereo == 1) ? 17 : 32;

					if (frame.ErrorProtection != 0)
						frame.SideInfoSize += 2;

					frame.FrameSize = LookupTables.TabSel123[frame.Lsf, 2, frame.BitRateIndex] * 144000;
					frame.FrameSize /= LookupTables.Freqs[frame.SamplingFrequency] << frame.Lsf;
					frame.FrameSize = frame.FrameSize + frame.Padding - 4;
					frame.PadSize = frame.Padding;
					break;
				}
			}

			if (frame.BitRateIndex == 0)
				frame.FrameSize = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Checks the given frame for a VBR frame and if so, decode it
		/// </summary>
		/********************************************************************/
		private bool DecodeVbrHeader(ModuleStream moduleStream, Frame frame)
		{
			// First of all, we don't know if we have a VBR file yet
			isVbr = false;
			vbrFrames = 0;
			vbrTotalBytes = 0;
			vbrScale = -1;
			vbrToc = null;

			// Only layer 3 have VBR support
			if (frame.Lay == 3)
			{
				// Get the start offset of the VBR info
				int offset;

				if (frame.Lsf != 0)
					offset = frame.Stereo == 1 ? 9 : 17;
				else
					offset = frame.Stereo == 1 ? 17 : 32;

				// Is the frame a VBR frame?
				long filePos = moduleStream.Position;

				try
				{
					moduleStream.Seek(offset, SeekOrigin.Current);

					if (moduleStream.Read_B_UINT32() != 0x58696e67)		// Xing
						return false;

					// Okay, the frame is a VBR frame, so start decode it
					isVbr = true;

					// Get the flags
					uint flags = moduleStream.Read_B_UINT32();

					// Any frames?
					if ((flags & 1) != 0)
						vbrFrames = (int)moduleStream.Read_B_UINT32();

					// Any bytes?
					if ((flags & 2) != 0)
						vbrTotalBytes = (int)moduleStream.Read_B_UINT32();

					// Any TOC?
					if ((flags & 4) != 0)
					{
						vbrToc = new byte[100];
						moduleStream.Read(vbrToc, 0, 100);
					}

					// Any scale?
					if ((flags & 8) != 0)
						vbrScale = (int)moduleStream.Read_B_UINT32();

					return true;
				}
				finally
				{
					moduleStream.Seek(filePos, SeekOrigin.Begin);
				}
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Checks the file for ID3v1 tags
		/// </summary>
		/********************************************************************/
		private bool HasId3V1Tags(ModuleStream moduleStream)
		{
			// Seek to the end of the file - 128 bytes
			moduleStream.Seek(-128, SeekOrigin.End);

			// Do the file have the MP3 tag?
			if ((moduleStream.Read_UINT8() == 0x54) && (moduleStream.Read_UINT8() == 0x41) && (moduleStream.Read_UINT8() == 0x47))		// TAG
				return true;

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Checks the file for ID3v1 tags and if available, read it
		/// </summary>
		/********************************************************************/
		private void GetId3V1Tags(ModuleStream moduleStream)
		{
			// Do the file have the MP3 tag?
			if (HasId3V1Tags(moduleStream))
			{
				// Yes, read it
				Encoding encoder = EncoderCollection.Win1252;
				byte[] buffer = new byte[32];
				bool track;

				// Song name
				moduleStream.ReadString(buffer, 30);
				songName = encoder.GetString(buffer).Trim();

				// Artist
				moduleStream.ReadString(buffer, 30);
				artist = encoder.GetString(buffer).Trim();

				// Album
				moduleStream.ReadString(buffer, 30);
				album = encoder.GetString(buffer).Trim();

				// Year
				moduleStream.ReadString(buffer, 4);
				year = encoder.GetString(buffer).Trim();

				// Comment
				moduleStream.ReadString(buffer, 29);
				if (buffer[28] != 0x00)
				{
					moduleStream.Read(buffer, 29, 1);
					buffer[30] = 0x00;
					track = false;
				}
				else
				{
					buffer[29] = 0x00;
					track = true;
				}

				comment = encoder.GetString(buffer).Trim();

				// Track number
				if (track)
					trackNum = moduleStream.Read_UINT8();
				else
					trackNum = 0;

				// Genre
				short genreNum = moduleStream.Read_UINT8();
				if ((genreNum < 192) && (genreNum >= 0))
					genre = LookupTables.Genre[genreNum];
			}
			else
			{
				// Clear the values
				songName = string.Empty;
				artist = string.Empty;
				album = string.Empty;
				year = string.Empty;
				comment = string.Empty;
				genre = string.Empty;
				trackNum = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Checks the file for ID3v2 tags and if available, read it
		/// </summary>
		/********************************************************************/
		private bool GetId3V2Tags(ModuleStream moduleStream)
		{
			// Seek to the beginning of the file
			moduleStream.Seek(0, SeekOrigin.Begin);

			if ((moduleStream.Read_UINT8() == 0x49) && (moduleStream.Read_UINT8() == 0x44) && (moduleStream.Read_UINT8() == 0x33))		// ID3
			{
				// Skip version and flags
				moduleStream.Seek(3, SeekOrigin.Current);

				// Read the tag size
				byte[] buf = new byte[4];
				moduleStream.Read(buf, 0, 4);

				// Remove bit 7 from all the bytes and calculate the size value
				int totalSize = ((buf[0] & 0x7f) << 21) | ((buf[1] & 0x7f) << 14) | ((buf[2] & 0x7f) << 7) | (buf[3] & 0x7f);

				// Use Mpg123 to read the header
				IntPtr handle = Native.mpg123_new(null, out int error);
				if (error == Native.mpg123_errors.MPG123_OK)
				{
					try
					{
						error = Native.mpg123_open_feed(handle);
						if (error != Native.mpg123_errors.MPG123_OK)
							return false;

						try
						{
							// Feed Mpg123 with the whole ID3v2 tag
							buf = new byte[totalSize + 200];	// Add a little bit more to read, so we're sure the beginning of the next frame is in the buffer

							moduleStream.Seek(0, SeekOrigin.Begin);
							int read = moduleStream.Read(buf, 0, buf.Length);
							if (read != buf.Length)
								return false;

							GCHandle bufferHandle = GCHandle.Alloc(buf, GCHandleType.Pinned);

							try
							{
								error = Native.mpg123_feed(handle, bufferHandle.AddrOfPinnedObject(), buf.Length);
								if (error != Native.mpg123_errors.MPG123_OK)
									return false;

								// Need to do this, which trigger the parsing of the ID3v2 frame
								Native.mpg123_feedseek(handle, 0, 0/*SEEK_SET*/, out _);
							}
							finally
							{
								bufferHandle.Free();
							}

							// Check if there is some meta data
							int meta = Native.mpg123_meta_check(handle);
							if ((meta & Native.MPG123_ID3) != 0)
							{
								// Read the meta data
								error = Native.mpg123_id3(handle, out _, out IntPtr v2);
								if (error != Native.mpg123_errors.MPG123_OK)
									return false;

								Native.mpg123_id3v2 id3v2 = Marshal.PtrToStructure<Native.mpg123_id3v2>(v2);

								songName = GetId3v2String(id3v2.title);
								artist = GetId3v2String(id3v2.artist);
								album = GetId3v2String(id3v2.album);
								year = GetId3v2String(id3v2.year);
								comment = GetId3v2String(id3v2.comment);
								genre = ProcessGenre(GetId3v2String(id3v2.genre));

								return true;
							}
						}
						finally
						{
							Native.mpg123_close(handle);
						}
					}
					finally
					{
						Native.mpg123_delete(handle);
					}
				}
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Parse the given mpg123_string and return it as a managed string
		/// </summary>
		/********************************************************************/
		private string GetId3v2String(IntPtr mpg123String)
		{
			StringBuilder sb = new StringBuilder();

			if (mpg123String != IntPtr.Zero)
			{
				Native.mpg123_string str = Marshal.PtrToStructure<Native.mpg123_string>(mpg123String);
				Encoding encoder = Encoding.UTF8;

				if (str.fill.ToUInt32() != 0)
				{
					byte[] lines = new byte[str.size.ToUInt32()];
					Marshal.Copy(str.p, lines, 0, lines.Length);
					int len = (int)str.fill.ToUInt32();
					const string newLine = " * ";

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
								sb.Append(newLine);
							else
							{
								if (i > startOffset)
									sb.Append(encoder.GetString(lines, startOffset, i - startOffset) + newLine);
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
		/// Feed Mpg123 with a block of data
		/// </summary>
		/********************************************************************/
		private bool FeedWithData()
		{
			int count = modStream.Read(inputBuffer, 0, InputBufSize);
			if (count == 0)
				return false;

			// Pin the input buffer, so the garbage collector won't move it
			// while we're feeding Mpg123
			GCHandle handle = GCHandle.Alloc(inputBuffer, GCHandleType.Pinned);

			try
			{
				int result = Native.mpg123_feed(mpg123Handle, handle.AddrOfPinnedObject(), count);
				if (result != Native.mpg123_errors.MPG123_OK)
					return false;
			}
			finally
			{
				// Done with the input buffer
				handle.Free();
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read next block of data
		/// </summary>
		/********************************************************************/
		private int LoadData(int[] outputBuffer, int count)
		{
			// Pin the output buffer, so the garbage collector won't move it
			// while reading data from Mpg123
			GCHandle handle = GCHandle.Alloc(tempOutputBuffer, GCHandleType.Pinned);

			try
			{
				int offset = 0;
				int total = 0;

				while (count > 0)
				{
					int todo = Math.Min(count * 4, OutputBufSize);	// Multiply with 4 to get number of bytes
					int result = Native.mpg123_read(mpg123Handle, handle.AddrOfPinnedObject(), todo, out int done);
					if ((result == Native.mpg123_errors.MPG123_OK) || (result == Native.mpg123_errors.MPG123_NEED_MORE))
					{
						// Copy the data into the output buffer
						if (done > 0)
						{
							Buffer.BlockCopy(tempOutputBuffer, 0, outputBuffer, offset, done);

							offset += done;
							done /= 4;
							count -= done;
							total += done;
						}

						if (result == Native.mpg123_errors.MPG123_NEED_MORE)
						{
							if (!FeedWithData())
								break;		// No more data to read
						}
					}
					else
					{
						if (result == Native.mpg123_errors.MPG123_NEW_FORMAT)
						{
							// Just ignore this one, since we don't need the new info
						}
						else if (result == Native.mpg123_errors.MPG123_DONE)
						{
							// Done with the stream
							break;
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
			finally
			{
				// Done with the input buffer
				handle.Free();
			}
		}
		#endregion
	}
}
