/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.OggVorbis.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Ports.LibVorbis;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Containers;
using Polycode.NostalgicPlayer.Ports.LibVorbisFile;

namespace Polycode.NostalgicPlayer.Agent.Player.OggVorbis
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class OggVorbisWorker : SamplePlayerWithDurationAgentBase
	{
		private VorbisFile vorbisFile;

		private int channels;
		private int frequency;

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

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "ogg", "oga" ];



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

			moduleStream.Seek(0, SeekOrigin.Begin);

			// Check the mark
			string mark = moduleStream.ReadMark();
			if (mark != "OggS")
				return AgentResult.Unknown;

			moduleStream.Seek(0, SeekOrigin.Begin);

			if (VorbisFile.Ov_Test(moduleStream, true, out VorbisFile testVorbisFile, null, 0) == VorbisError.Ok)
			{
				testVorbisFile.Ov_Clear();

				return AgentResult.Ok;
			}

			return AgentResult.Unknown;
		}
		#endregion

		#region Loading
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
		#endregion

		#region Initialization and cleanup
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
			VorbisError result = VorbisFile.Ov_Open(moduleStream, true, out vorbisFile, null, 0);
			if (result != VorbisError.Ok)
			{
				errorMessage = GetErrorString(result);
				return false;
			}

			// Get player data
			VorbisInfo info = vorbisFile.Ov_Info(0);

			frequency = info.rate;
			channels = info.channels;

			if (channels > 8)
			{
				errorMessage = string.Format(Resources.IDS_OGG_ERR_ILLEGAL_CHANNELS, channels);
				return false;
			}

			// Get meta data
			VorbisComment comment = vorbisFile.Ov_Comment(0);

			vendor = Encoding.UTF8.GetString(comment.vendor.Buffer, comment.vendor.Offset, comment.vendor.Length - 1);
			if (string.IsNullOrEmpty(vendor))
				vendor = Resources.IDS_OGG_INFO_UNKNOWN;

			trackNum = Resources.IDS_OGG_INFO_UNKNOWN;
			album = Resources.IDS_OGG_INFO_UNKNOWN;
			genre = Resources.IDS_OGG_INFO_UNKNOWN;
			organization = Resources.IDS_OGG_INFO_UNKNOWN;
			copyright = Resources.IDS_OGG_INFO_UNKNOWN;
			descrip = Resources.IDS_OGG_INFO_NONE;

			List<PictureInfo> collectedPictures = new List<PictureInfo>();

			foreach (var tag in ParseTags(comment))
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
						PictureInfo pictureInfo = ParsePicture(tag.tagValue);
						if (pictureInfo != null)
							collectedPictures.Add(pictureInfo);

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
			if (vorbisFile != null)
			{
				vorbisFile.Ov_Clear();
				vorbisFile = null;
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

			errorMessage = string.Empty;

			VorbisInfo info = vorbisFile.Ov_Info(0);
			bitRate = info.bitrate_nominal / 1000;

			return true;
		}
		#endregion

		#region Playing
		/********************************************************************/
		/// <summary>
		/// Will load and decode a data block and store it in the buffer
		/// given
		/// </summary>
		/********************************************************************/
		public override int LoadDataBlock(int[][] outputBuffer, int countInFrames)
		{
			// Load the next block of data
			int filledInFrames = LoadData(outputBuffer, countInFrames);

			if (filledInFrames == 0)
			{
				OnEndReached();

				// Loop the sample
				CleanupSound();
				InitSound(out _);
			}

			// Has the bit rate changed
			int newBitRate = vorbisFile.Ov_Bitrate_Instant();
			if (newBitRate > 0)
			{
				newBitRate /= 1000;

				if (newBitRate != bitRate)
				{
					bitRate = newBitRate;

					OnModuleInfoChanged(InfoBitRateLine, bitRate.ToString());
				}
			}

			return filledInFrames;
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
		/// Return which speakers the player uses.
		/// 
		/// Note that the outputBuffer in LoadDataBlock match the defined
		/// order in SpeakerFlag enum
		/// </summary>
		/********************************************************************/
		public override SpeakerFlag SpeakerFlags => Tables.ChannelToSpeaker[channels - 1];



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
		public override int Frequency => frequency;



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
					description = Resources.IDS_OGG_INFODESCLINE0;
					value = trackNum;
					break;
				}

				// Album
				case 1:
				{
					description = Resources.IDS_OGG_INFODESCLINE1;
					value = album;
					break;
				}

				// Genre
				case 2:
				{
					description = Resources.IDS_OGG_INFODESCLINE2;
					value = genre;
					break;
				}

				// Organization
				case 3:
				{
					description = Resources.IDS_OGG_INFODESCLINE3;
					value = organization;
					break;
				}

				// Copyright
				case 4:
				{
					description = Resources.IDS_OGG_INFODESCLINE4;
					value = copyright;
					break;
				}

				// Description
				case 5:
				{
					description = Resources.IDS_OGG_INFODESCLINE5;
					value = descrip;
					break;
				}

				// Vendor
				case 6:
				{
					description = Resources.IDS_OGG_INFODESCLINE6;
					value = vendor;
					break;
				}

				// Bit rate
				case 7:
				{
					description = Resources.IDS_OGG_INFODESCLINE7;
					value = bitRate.ToString();
					break;
				}

				// Frequency
				case 8:
				{
					description = Resources.IDS_OGG_INFODESCLINE8;
					value = frequency.ToString();
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
		/// Return the total time of the sample
		/// </summary>
		/********************************************************************/
		protected override TimeSpan GetTotalDuration()
		{
			double totalTime = vorbisFile.Ov_Time_Total(-1);

			return TimeSpan.FromSeconds(totalTime);
		}



		/********************************************************************/
		/// <summary>
		/// Set the position in the playing sample to the time given
		/// </summary>
		/********************************************************************/
		protected override void SetPosition(TimeSpan time)
		{
			vorbisFile.Ov_Time_Seek(time.TotalSeconds);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Helper method to get the string of an error
		/// </summary>
		/********************************************************************/
		private string GetErrorString(VorbisError error)
		{
			switch (error)
			{
				case VorbisError.Hole:
					return Resources.IDS_OGG_ERR_HOLE;

				case VorbisError.Read:
					return Resources.IDS_OGG_ERR_READ;

				case VorbisError.Fault:
					return Resources.IDS_OGG_ERR_FAULT;

				case VorbisError.Impl:
					return Resources.IDS_OGG_ERR_IMPL;

				case VorbisError.Inval:
					return Resources.IDS_OGG_ERR_INVAL;

				case VorbisError.NotVorbis:
					return Resources.IDS_OGG_ERR_NOT_FORMAT;

				case VorbisError.BadHeader:
					return Resources.IDS_OGG_ERR_BAD_HEADER;

				case VorbisError.Version:
					return Resources.IDS_OGG_ERR_VERSION;

				case VorbisError.NotAudio:
					return Resources.IDS_OGG_ERR_NOT_AUDIO;

				case VorbisError.BadPacket:
					return Resources.IDS_OGG_ERR_BAD_PACKET;

				case VorbisError.BadLink:
					return Resources.IDS_OGG_ERR_BAD_LINK;

				case VorbisError.NoSeek:
					return Resources.IDS_OGG_ERR_NO_SEEK;

				default:
					return Resources.IDS_OGG_ERR_UNKNOWN;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse a Vorbis picture metadata block
		/// </summary>
		/********************************************************************/
		private PictureInfo ParsePicture(string pictureValue)
		{
			try
			{
				using (ReaderStream rs = new ReaderStream(new MemoryStream(Convert.FromBase64String(pictureValue))))
				{
					PictureType pictureType = (PictureType)rs.Read_B_INT32();

					int length = rs.Read_B_INT32();
					byte[] bytes = new byte[length];

					if (rs.Read(bytes, 0, length) != length)
						return null;

					string mimeType = Encoding.Latin1.GetString(bytes);
					if (mimeType == "-->")	// URL, we do not support that
						return null;

					length = rs.Read_B_INT32();
					bytes = new byte[length];

					if (rs.Read(bytes, 0, length) != length)
						return null;

					string description = Encoding.UTF8.GetString(bytes);

					// Skip width, height, color depth and number of colors
					rs.Seek(4 * 4, SeekOrigin.Current);

					// Get the picture data
					length = rs.Read_B_INT32();
					bytes = new byte[length];

					if (rs.Read(bytes, 0, length) != length)
						return null;

					string type = GetTypeDescription(pictureType);

					if (string.IsNullOrEmpty(description))
						description = type;
					else
						description = $"{type}: {description}";

					return new PictureInfo(bytes, description);
				}
			}
			catch (Exception)
			{
				return null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert the tags to something that is easier to read
		/// </summary>
		/********************************************************************/
		private IEnumerable<(string tagName, string tagValue)> ParseTags(VorbisComment comment)
		{
			Encoding encoder = Encoding.UTF8;

			for (int i = 0; i < comment.comments; i++)
			{
				int length = comment.comment_lengths[i];
				string tag = encoder.GetString(comment.user_comments[i].Buffer, comment.user_comments[i].Offset, length);

				int index = tag.IndexOf('=');
				if (index > 0)
					yield return (tag.Substring(0, index).ToUpper(), tag.Substring(index + 1));
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
					return Resources.IDS_OGG_PICTURE_TYPE_OTHER;

				case PictureType.File_Icon_Standard:
					return Resources.IDS_OGG_PICTURE_TYPE_FILE_ICON_STANDARD;

				case PictureType.File_Icon:
					return Resources.IDS_OGG_PICTURE_TYPE_FILE_ICON;

				case PictureType.Front_Cover:
					return Resources.IDS_OGG_PICTURE_TYPE_FRONT_COVER;

				case PictureType.Back_Cover:
					return Resources.IDS_OGG_PICTURE_TYPE_BACK_COVER;

				case PictureType.Leaflet_Page:
					return Resources.IDS_OGG_PICTURE_TYPE_LEAFLET_PAGE;

				case PictureType.Media:
					return Resources.IDS_OGG_PICTURE_TYPE_MEDIA;

				case PictureType.Lead_Artist:
					return Resources.IDS_OGG_PICTURE_TYPE_LEAD_ARTIST;

				case PictureType.Artist:
					return Resources.IDS_OGG_PICTURE_TYPE_ARTIST;

				case PictureType.Conductor:
					return Resources.IDS_OGG_PICTURE_TYPE_CONDUCTOR;

				case PictureType.Band:
					return Resources.IDS_OGG_PICTURE_TYPE_BAND;

				case PictureType.Composer:
					return Resources.IDS_OGG_PICTURE_TYPE_COMPOSER;

				case PictureType.Lyricist:
					return Resources.IDS_OGG_PICTURE_TYPE_LYRICIST;

				case PictureType.Recording_Location:
					return Resources.IDS_OGG_PICTURE_TYPE_RECORDING_LOCATION;

				case PictureType.During_Recoding:
					return Resources.IDS_OGG_PICTURE_TYPE_DURING_RECORDING;

				case PictureType.During_Performance:
					return Resources.IDS_OGG_PICTURE_TYPE_DURING_PERFORMANCE;

				case PictureType.Video_Screen_Capture:
					return Resources.IDS_OGG_PICTURE_TYPE_VIDEO_SCREEN_CAPTURE;

				case PictureType.Fish:
					return Resources.IDS_OGG_PICTURE_TYPE_FISH;

				case PictureType.Illustration:
					return Resources.IDS_OGG_PICTURE_TYPE_ILLUSTRATION;

				case PictureType.Band_LogoType:
					return Resources.IDS_OGG_PICTURE_TYPE_BAND_LOGO;

				case PictureType.Publisher_LogoType:
					return Resources.IDS_OGG_PICTURE_TYPE_PUBLISHER_LOGO;
			}

			return string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Read next block of data
		/// </summary>
		/********************************************************************/
		private int LoadData(int[][] outputBuffer, int countInFrames)
		{
			int offset = 0;
			int totalInFrames = 0;

			while (countInFrames > 0)
			{
				int done = vorbisFile.Ov_Read_Float(out CPointer<float>[] buffer, countInFrames, out _);
				if (done == (int)VorbisError.Hole)
					continue;

				if (done <= 0)
					break;

				// Convert the floats into 32-bit integers
				int[] channelMapping = Tables.InputToOutput[channels - 1];

				for (int i = 0; i < channels; i++)
				{
					CPointer<float> inBuffer = buffer[i];
					int[] outBuffer = outputBuffer[channelMapping[i]];

					for (int j = 0; j < done; j++)
						outBuffer[offset + j] = Math.Clamp((int)(inBuffer[j] * 0x8000000), -0x8000000, 0x7ffffff) << 4;
				}

				offset += done;
				countInFrames -= done;
				totalInFrames += done;
			}

			return totalInFrames;
		}
		#endregion
	}
}
