/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Ports.OpusFile;
using Polycode.NostalgicPlayer.Ports.OpusFile.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.Opus
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class OpusWorker : SamplePlayerWithDurationAgentBase
	{
		private OpusFile opusFile;

		private int channels;

		private float[] inputBuffer;

		private string songName;
		private string artist;
		private string trackNum;
		private string album;
		private string genre;
		private string organization;
		private string copyright;
		private string descrip;
		private string vendor;
		private PictureInfo[] pictures;
		private int bitRate;

		private const int InfoBitRateLine = 7;

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => new [] { "opus" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 47)
				return AgentResult.Unknown;

			// Load first 256 bytes of file to check on
			byte[] buf = new byte[256];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.ReadInto(buf, 0, 256);

			if (OpusFile.Op_Test(null, buf, (ulong)buf.Length) == 0)
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
					description = Resources.IDS_OPUS_INFODESCLINE0;
					value = trackNum;
					break;
				}

				// Album
				case 1:
				{
					description = Resources.IDS_OPUS_INFODESCLINE1;
					value = album;
					break;
				}

				// Genre
				case 2:
				{
					description = Resources.IDS_OPUS_INFODESCLINE2;
					value = genre;
					break;
				}

				// Organization
				case 3:
				{
					description = Resources.IDS_OPUS_INFODESCLINE3;
					value = organization;
					break;
				}

				// Copyright
				case 4:
				{
					description = Resources.IDS_OPUS_INFODESCLINE4;
					value = copyright;
					break;
				}

				// Description
				case 5:
				{
					description = Resources.IDS_OPUS_INFODESCLINE5;
					value = descrip;
					break;
				}

				// Vendor
				case 6:
				{
					description = Resources.IDS_OPUS_INFODESCLINE6;
					value = vendor;
					break;
				}

				// Bit rate
				case 7:
				{
					description = Resources.IDS_OPUS_INFODESCLINE7;
					value = bitRate.ToString();
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

			// Get a handle, which is used on all other calls
			opusFile = OpusFile.Op_Open_Stream(moduleStream, true, out OpusFileError error);
			if (error != OpusFileError.Ok)
			{
				errorMessage = GetErrorString(error);
				return false;
			}

			// Get player data
			channels = opusFile.Op_Channel_Count(0);
			if (channels > 2)
			{
				errorMessage = string.Format(Resources.IDS_OPUS_ERR_ILLEGAL_CHANNELS, channels);
				return false;
			}

			// Allocate buffers
			inputBuffer = new float[channels * (120 * 48000 / 1000 * 2)];		// 120 * 2 = 240 ms per channel at 48 kHz

			// Get meta data
			OpusTags tags = opusFile.Op_Tags(0);

			vendor = Encoding.UTF8.GetString(tags.Vendor.Buffer, tags.Vendor.Offset, tags.Vendor.Length - 1);
			if (string.IsNullOrEmpty(vendor))
				vendor = Resources.IDS_OPUS_INFO_UNKNOWN;

			trackNum = Resources.IDS_OPUS_INFO_UNKNOWN;
			album = Resources.IDS_OPUS_INFO_UNKNOWN;
			genre = Resources.IDS_OPUS_INFO_UNKNOWN;
			organization = Resources.IDS_OPUS_INFO_UNKNOWN;
			copyright = Resources.IDS_OPUS_INFO_UNKNOWN;
			descrip = Resources.IDS_OPUS_INFO_NONE;

			List<PictureInfo> collectedPictures = new List<PictureInfo>();

			foreach (var tag in ParseTags(tags))
			{
				switch (tag.tagName)
				{
					case "TITLE":
					{
						songName = tag.tagValue;
						break;
					}

					case "ARTIST":
					{
						artist = tag.tagValue;
						break;
					}

					case "TRACKNUMBER":
					{
						trackNum = tag.tagValue;
						break;
					}

					case "ALBUM":
					{
						album = tag.tagValue;
						break;
					}

					case "GENRE":
					{
						genre = tag.tagValue;
						break;
					}

					case "ORGANIZATION":
					{
						organization = tag.tagValue;
						break;
					}

					case "COPYRIGHT":
					{
						copyright = tag.tagValue;
						break;
					}

					case "DESCRIPTION":
					{
						descrip = tag.tagValue;
						break;
					}

					case "METADATA_BLOCK_PICTURE":
					{
						int err = Info.Opus_Picture_Tag_Parse(out OpusPictureTag pic, tag.tagValue);
						if (err >= 0)
						{
							PictureInfo info = ParsePicture(pic);
							if (info != null)
								collectedPictures.Add(info);

							Info.Opus_Picture_Tag_Clear(pic);
						}
						break;
					}
				}
			}

			pictures = collectedPictures.Count > 0 ? collectedPictures.ToArray() : null;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public override void CleanupPlayer()
		{
			inputBuffer = null;

			if (opusFile != null)
			{
				opusFile.Op_Free();
				opusFile = null;
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

			bitRate = 0;

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

			// Has the bit rate changed
			int newBitRate = opusFile.Op_Bitrate_Instant() / 1000;
			if ((newBitRate >= 0) && (newBitRate != bitRate))
			{
				bitRate = newBitRate;

				OnModuleInfoChanged(InfoBitRateLine, bitRate.ToString());
			}

			return filled;
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the sample uses
		/// </summary>
		/********************************************************************/
		public override int ChannelCount => channels;



		/********************************************************************/
		/// <summary>
		/// Return the frequency the sample is stored with
		/// </summary>
		/********************************************************************/
		public override int Frequency => 48000;
		#endregion

		#region SamplePlayerWithDurationAgentBase
		/********************************************************************/
		/// <summary>
		/// Return the total time of the sample
		/// </summary>
		/********************************************************************/
		protected override TimeSpan GetTotalDuration()
		{
			long numberOfSamples = opusFile.Op_Pcm_Total(0);
			double totalTime = (double)numberOfSamples / 48000;

			return new TimeSpan((long)(totalTime * TimeSpan.TicksPerSecond));
		}



		/********************************************************************/
		/// <summary>
		/// Set the position in the playing sample to the time given
		/// </summary>
		/********************************************************************/
		protected override void SetPosition(TimeSpan time)
		{
			opusFile.Op_Pcm_Seek((long)(time.TotalMilliseconds * 48000 / 1000));
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Helper method to get the string of an error
		/// </summary>
		/********************************************************************/
		private string GetErrorString(OpusFileError error)
		{
			switch (error)
			{
				case OpusFileError.Hole:
					return Resources.IDS_OPUS_ERR_HOLE;

				case OpusFileError.Read:
					return Resources.IDS_OPUS_ERR_READ;

				case OpusFileError.Fault:
					return Resources.IDS_OPUS_ERR_FAULT;

				case OpusFileError.Impl:
					return Resources.IDS_OPUS_ERR_IMPL;

				case OpusFileError.Inval:
					return Resources.IDS_OPUS_ERR_INVAL;

				case OpusFileError.NotFormat:
					return Resources.IDS_OPUS_ERR_NOT_FORMAT;

				case OpusFileError.BadHeader:
					return Resources.IDS_OPUS_ERR_BAD_HEADER;

				case OpusFileError.Version:
					return Resources.IDS_OPUS_ERR_VERSION;

				case OpusFileError.BadPacket:
					return Resources.IDS_OPUS_ERR_BAD_PACKET;

				case OpusFileError.BadLink:
					return Resources.IDS_OPUS_ERR_BAD_LINK;

				case OpusFileError.NoSeek:
					return Resources.IDS_OPUS_ERR_NO_SEEK;

				case OpusFileError.BadTimestamp:
					return Resources.IDS_OPUS_ERR_BAD_TIMESTAMP;

				default:
					return Resources.IDS_OPUS_ERR_UNKNOWN;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Read next block of data
		/// </summary>
		/********************************************************************/
		private int LoadData(int[] outputBuffer, int count)
		{
			int offset = 0;
			int total = 0;

			while (count > 0)
			{
				int todo = Math.Min(count, inputBuffer.Length);
				int done = opusFile.Op_Read_Float(inputBuffer, todo, out _);
				if (done == (int)OpusFileError.Hole)
					continue;

				if (done <= 0)
					break;

				done *= channels;

				// Convert the floats into 32-bit integers
				for (int i = 0; i < done; i++)
					outputBuffer[offset++] = Math.Clamp((int)(inputBuffer[i] * 0x8000000), -0x8000000, 0x7ffffff) << 4;

				count -= done;
				total += done;
			}

			return total;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the tags to something that is easier to read
		/// </summary>
		/********************************************************************/
		private IEnumerable<(string tagName, string tagValue)> ParseTags(OpusTags tags)
		{
			Encoding encoder = Encoding.UTF8;

			for (int i = 0; i < tags.Comments; i++)
			{
				int length = tags.Comment_Lengths[i];
				string tag = encoder.GetString(tags.User_Comments[i].Buffer, tags.User_Comments[i].Offset, length);

				int index = tag.IndexOf('=');
				if (index > 0)
					yield return (tag.Substring(0, index).ToUpper(), tag.Substring(index + 1));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse the Vorbis picture metadata block
		/// </summary>
		/********************************************************************/
		private PictureInfo ParsePicture(OpusPictureTag tag)
		{
			try
			{
				if (tag.Format == PictureFormat.Url)	// URL, we do not support that
					return null;

				string type = GetTypeDescription(tag.Type);
				string description;

				if (tag.Description.IsNull || (tag.Description.Length == 1))
					description = type;
				else
					description = $"{type}: {Encoding.UTF8.GetString(tag.Description.Buffer, tag.Description.Offset, tag.Description.Length - 1)}";

				return new PictureInfo(tag.Data.AsSpan().Slice(0, (int)tag.Data_Length).ToArray(), description);
			}
			catch (Exception)
			{
				return null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Find the picture type string
		/// </summary>
		/********************************************************************/
		private string GetTypeDescription(PictureType type)
		{
			switch (type)
			{
				case PictureType.Other:
					return Resources.IDS_OPUS_PICTURE_TYPE_OTHER;

				case PictureType.File_Icon_Standard:
					return Resources.IDS_OPUS_PICTURE_TYPE_FILE_ICON_STANDARD;

				case PictureType.File_Icon:
					return Resources.IDS_OPUS_PICTURE_TYPE_FILE_ICON;

				case PictureType.Front_Cover:
					return Resources.IDS_OPUS_PICTURE_TYPE_FRONT_COVER;

				case PictureType.Back_Cover:
					return Resources.IDS_OPUS_PICTURE_TYPE_BACK_COVER;

				case PictureType.Leaflet_Page:
					return Resources.IDS_OPUS_PICTURE_TYPE_LEAFLET_PAGE;

				case PictureType.Media:
					return Resources.IDS_OPUS_PICTURE_TYPE_MEDIA;

				case PictureType.Lead_Artist:
					return Resources.IDS_OPUS_PICTURE_TYPE_LEAD_ARTIST;

				case PictureType.Artist:
					return Resources.IDS_OPUS_PICTURE_TYPE_ARTIST;

				case PictureType.Conductor:
					return Resources.IDS_OPUS_PICTURE_TYPE_CONDUCTOR;

				case PictureType.Band:
					return Resources.IDS_OPUS_PICTURE_TYPE_BAND;

				case PictureType.Composer:
					return Resources.IDS_OPUS_PICTURE_TYPE_COMPOSER;

				case PictureType.Lyricist:
					return Resources.IDS_OPUS_PICTURE_TYPE_LYRICIST;

				case PictureType.Recording_Location:
					return Resources.IDS_OPUS_PICTURE_TYPE_RECORDING_LOCATION;

				case PictureType.During_Recoding:
					return Resources.IDS_OPUS_PICTURE_TYPE_DURING_RECORDING;

				case PictureType.During_Performance:
					return Resources.IDS_OPUS_PICTURE_TYPE_DURING_PERFORMANCE;

				case PictureType.Video_Screen_Capture:
					return Resources.IDS_OPUS_PICTURE_TYPE_VIDEO_SCREEN_CAPTURE;

				case PictureType.Fish:
					return Resources.IDS_OPUS_PICTURE_TYPE_FISH;

				case PictureType.Illustration:
					return Resources.IDS_OPUS_PICTURE_TYPE_ILLUSTRATION;

				case PictureType.Band_LogoType:
					return Resources.IDS_OPUS_PICTURE_TYPE_BAND_LOGO;

				case PictureType.Publisher_LogoType:
					return Resources.IDS_OPUS_PICTURE_TYPE_PUBLISHER_LOGO;
			}

			return string.Empty;
		}
		#endregion
	}
}
