/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NVorbis;
using NVorbis.Contracts;
using Polycode.NostalgicPlayer.Agent.Player.OggVorbis.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.OggVorbis
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class OggVorbisWorker : SamplePlayerWithDurationAgentBase
	{
		private const int InputBufSize = 32 * 1024;

		private VorbisReader reader;

		private int channels;
		private int frequency;

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
		public override string[] FileExtensions => new [] { "ogg", "oga" };



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 27)
				return AgentResult.Unknown;

			// Check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);

			if (moduleStream.Read_B_UINT32() != 0x4f676753)		// OggS
				return AgentResult.Unknown;

			// Check the stream structure version
			if (moduleStream.Read_UINT8() != 0x00)
				return AgentResult.Unknown;

			// Check the header type flag
			if ((moduleStream.Read_UINT8() & 0xf8) != 0x00)
				return AgentResult.Unknown;

			// Check the second mark
			byte[] buf = new byte[6];

			moduleStream.Seek(29, SeekOrigin.Begin);
			moduleStream.Read(buf, 0, 6);

			if ((buf[0] != 0x76) || (buf[1] != 0x6f) || (buf[2] != 0x72) || (buf[3] != 0x62) || (buf[4] != 0x69) || (buf[5] != 0x73))	// vorbis
				return AgentResult.Unknown;

			return AgentResult.Ok;
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

			// Initialize the reader
			reader = new VorbisReader(moduleStream);

			// Remember the frequency to play with
			frequency = reader.SampleRate;
			channels = reader.Channels;

			if (channels > 2)
			{
				errorMessage = string.Format(Resources.IDS_OGG_ERR_ILLEGAL_CHANNELS, channels);
				return false;
			}

			// Allocate buffers
			inputBuffer = new float[InputBufSize];

			// Get meta data
			ITagData tags = reader.Tags;

			songName = tags.Title;
			artist = tags.Artist;
			trackNum = string.IsNullOrEmpty(tags.TrackNumber) ? Resources.IDS_OGG_INFO_UNKNOWN : tags.TrackNumber;
			album = string.IsNullOrEmpty(tags.Album) ? Resources.IDS_OGG_INFO_UNKNOWN : tags.Album;
			genre = tags.Genres.Count == 0 ? Resources.IDS_OGG_INFO_UNKNOWN : string.Join(", ", tags.Genres);
			organization = string.IsNullOrEmpty(tags.Organization) ? Resources.IDS_OGG_INFO_UNKNOWN : tags.Organization;
			copyright = string.IsNullOrEmpty(tags.Copyright) ? Resources.IDS_OGG_INFO_UNKNOWN : tags.Copyright;
			descrip = string.IsNullOrEmpty(tags.Description) ? Resources.IDS_OGG_INFO_NONE : tags.Description;
			vendor = string.IsNullOrEmpty(tags.EncoderVendor) ? Resources.IDS_OGG_INFO_UNKNOWN : tags.EncoderVendor;
			pictures = ParsePictures(tags);

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

			if (reader != null)
			{
				reader.Dispose();
				reader = null;
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

			// Reset the sample position
			// (need this check, else NVorbis crashes with an "index out of range" exception)
			if (reader.TimePosition != TimeSpan.Zero)
				reader.TimePosition = TimeSpan.Zero;

			bitRate = reader.NominalBitrate / 1000;

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
			int newBitRate = reader.Streams[0].Stats.InstantBitRate / 1000;
			if (newBitRate != bitRate)
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
			return reader.TotalTime;
		}



		/********************************************************************/
		/// <summary>
		/// Set the position in the playing sample to the time given
		/// </summary>
		/********************************************************************/
		protected override void SetPosition(TimeSpan time)
		{
			reader.TimePosition = time;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Parse the Vorbis picture metadata block
		/// </summary>
		/********************************************************************/
		private PictureInfo[] ParsePictures(ITagData tags)
		{
			try
			{
				IReadOnlyList<string> allPictures = tags.GetTagMulti("METADATA_BLOCK_PICTURE");
				if (allPictures.Count == 0)
					return null;

				List<PictureInfo> pictureList = new List<PictureInfo>();

				foreach (string base64 in allPictures)
				{
					try
					{
						using (ReaderStream rs = new ReaderStream(new MemoryStream(Convert.FromBase64String(base64))))
						{
							PictureType pictureType = (PictureType)rs.Read_B_INT32();

							int length = rs.Read_B_INT32();
							byte[] bytes = new byte[length];

							if (rs.Read(bytes, 0, length) != length)
								continue;

							string mimeType = Encoding.ASCII.GetString(bytes);
							if (mimeType == "-->")	// URL, we do not support that
								continue;

							length = rs.Read_B_INT32();
							bytes = new byte[length];

							if (rs.Read(bytes, 0, length) != length)
								continue;

							string description = Encoding.UTF8.GetString(bytes);

							// Skip width, height, color depth and number of colors
							rs.Seek(4 * 4, SeekOrigin.Current);

							// Get the picture data
							length = rs.Read_B_INT32();
							bytes = new byte[length];

							if (rs.Read(bytes, 0, length) != length)
								continue;

							string type = GetTypeDescription(pictureType);

							if (string.IsNullOrEmpty(description))
								description = type;
							else
								description = $"{type}: {description}";

							pictureList.Add(new PictureInfo(bytes, description));
						}
					}
					catch (Exception)
					{
						// Ignore exception
					}
				}

				return pictureList.Count > 0 ? pictureList.ToArray() : null;
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
		private int LoadData(int[] outputBuffer, int count)
		{
			int offset = 0;
			int total = 0;

			while (count > 0)
			{
				int todo = Math.Min(count, InputBufSize);
				int done = reader.ReadSamples(inputBuffer, 0, todo);
				if (done == 0)
					break;

				// Convert the floats into 32-bit integers
				for (int i = 0; i < done; i++)
					outputBuffer[offset++] = (int)(inputBuffer[i] * 2147483647.0f);

				count -= done;
				total += done;
			}

			return total;
		}
		#endregion
	}
}
