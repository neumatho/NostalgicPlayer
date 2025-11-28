/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Streamer.OggStreamer.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Ports.LibVorbis;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Containers;
using Polycode.NostalgicPlayer.Ports.LibVorbisFile;

namespace Polycode.NostalgicPlayer.Agent.Streamer.OggStreamer
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class OggVorbisStreamerWorker : StreamerAgentBase
	{
		private VorbisFile vorbisFile;

		private int channels;
		private int frequency;

		private TagInformation currentTagInfo;
		private int bitRate;

		private const int InfoTrackNumberLine = 0;
		private const int InfoAlbumLine = 1;
		private const int InfoGenreLine = 2;
		private const int InfoOrganizationLine = 3;
		private const int InfoCopyrightLine = 4;
		private const int InfoDescriptionLine = 5;
		private const int InfoVendorLine = 6;
		private const int InfoBitRateLine = 7;

		#region Identify
		/********************************************************************/
		/// <summary>
		/// Return an array of mime types that this agent can handle
		/// </summary>
		/********************************************************************/
		public override string[] PlayableMimeTypes => [ "application/ogg" ];



		/********************************************************************/
		/// <summary>
		/// Try to identify the format
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(StreamingStream streamingStream)
		{
			byte[] buffer = new byte[4];

			streamingStream.ReadExactly(buffer, 0, 4);

			if ((buffer[0] != 'O') || (buffer[1] != 'g') || (buffer[2] != 'g') || (buffer[3] != 'S'))
				return AgentResult.Unknown;

			if (VorbisFile.Ov_Test(streamingStream, true, out VorbisFile testVorbisFile, buffer, buffer.Length) == VorbisError.Ok)
			{
				testVorbisFile.Ov_Clear();

				return AgentResult.Ok;
			}

			return AgentResult.Unknown;
		}
		#endregion

		#region Initialization and cleanup
		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer(StreamingStream streamingStream, IMetadata metadata, out string errorMessage)
		{
			if (!base.InitPlayer(streamingStream, metadata, out errorMessage))
				return false;

			// Get a handle, which is used on all other calls
			VorbisError result = VorbisFile.Ov_Open(streamingStream, true, out vorbisFile, null, 0);
			if (result != VorbisError.Ok)
			{
				errorMessage = GetErrorString(result);
				return false;
			}

			// Get player data
			VorbisInfo info = vorbisFile.Ov_Info(0);

			frequency = (int)info.rate;
			channels = info.channels;

			if (channels > 2)
			{
				errorMessage = string.Format(Resources.IDS_OGG_ERR_ILLEGAL_CHANNELS, channels);
				return false;
			}

			// Get meta data
			currentTagInfo = RetrieveTagInfo();

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

			VorbisInfo info = vorbisFile.Ov_Info(0);
			bitRate = (int)(info.bitrate_nominal / 1000);

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
			int newBitRate = (int)vorbisFile.Ov_Bitrate_Instant();
			if (newBitRate > 0)
			{
				newBitRate /= 1000;

				if (newBitRate != bitRate)
				{
					bitRate = newBitRate;

					OnModuleInfoChanged(InfoBitRateLine, bitRate.ToString());
				}
			}

			// Has the meta information changed
			TagInformation tagInfo = RetrieveTagInfo();

			if (tagInfo.SongName != currentTagInfo.SongName)
				OnModuleInfoChanged(ModuleInfoChanged.ModuleNameChanged, tagInfo.SongName);

			if (tagInfo.Artist != currentTagInfo.Artist)
				OnModuleInfoChanged(ModuleInfoChanged.AuthorChanged, tagInfo.Artist);

			if (tagInfo.TrackNum != currentTagInfo.TrackNum)
				OnModuleInfoChanged(InfoTrackNumberLine, tagInfo.TrackNum);

			if (tagInfo.Album != currentTagInfo.Album)
				OnModuleInfoChanged(InfoAlbumLine, tagInfo.Album);

			if (tagInfo.Genre != currentTagInfo.Genre)
				OnModuleInfoChanged(InfoGenreLine, tagInfo.Genre);

			if (tagInfo.Organization != currentTagInfo.Organization)
				OnModuleInfoChanged(InfoOrganizationLine, tagInfo.Organization);

			if (tagInfo.Copyright != currentTagInfo.Copyright)
				OnModuleInfoChanged(InfoCopyrightLine, tagInfo.Copyright);

			if (tagInfo.Description != currentTagInfo.Description)
				OnModuleInfoChanged(InfoDescriptionLine, tagInfo.Description);

			if (tagInfo.Vendor != currentTagInfo.Vendor)
				OnModuleInfoChanged(InfoVendorLine, tagInfo.Vendor);

			currentTagInfo = tagInfo;

			return filledInFrames;
		}
		#endregion

		#region Information
		/********************************************************************/
		/// <summary>
		/// Return the title
		/// </summary>
		/********************************************************************/
		public override string Title => currentTagInfo.SongName;



		/********************************************************************/
		/// <summary>
		/// Return the name of the author
		/// </summary>
		/********************************************************************/
		public override string Author => currentTagInfo.Artist;



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
					value = currentTagInfo.TrackNum;
					break;
				}

				// Album
				case 1:
				{
					description = Resources.IDS_OGG_INFODESCLINE1;
					value = currentTagInfo.Album;
					break;
				}

				// Genre
				case 2:
				{
					description = Resources.IDS_OGG_INFODESCLINE2;
					value = currentTagInfo.Genre;
					break;
				}

				// Organization
				case 3:
				{
					description = Resources.IDS_OGG_INFODESCLINE3;
					value = currentTagInfo.Organization;
					break;
				}

				// Copyright
				case 4:
				{
					description = Resources.IDS_OGG_INFODESCLINE4;
					value = currentTagInfo.Copyright;
					break;
				}

				// Description
				case 5:
				{
					description = Resources.IDS_OGG_INFODESCLINE5;
					value = currentTagInfo.Description;
					break;
				}

				// Vendor
				case 6:
				{
					description = Resources.IDS_OGG_INFODESCLINE6;
					value = currentTagInfo.Vendor;
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
		/// Retrieve and parse all tags
		/// </summary>
		/********************************************************************/
		private TagInformation RetrieveTagInfo()
		{
			VorbisComment comment = vorbisFile.Ov_Comment(0);

			string vendor = Encoding.UTF8.GetString(comment.vendor.Buffer, comment.vendor.Offset, comment.vendor.Length - 1);
			if (string.IsNullOrEmpty(vendor))
				vendor = Resources.IDS_OGG_INFO_UNKNOWN;

			TagInformation tagInfo = new TagInformation
			{
				Vendor = vendor,
				TrackNum = Resources.IDS_OGG_INFO_UNKNOWN,
				Album = Resources.IDS_OGG_INFO_UNKNOWN,
				Genre = Resources.IDS_OGG_INFO_UNKNOWN,
				Organization = Resources.IDS_OGG_INFO_UNKNOWN,
				Copyright = Resources.IDS_OGG_INFO_UNKNOWN,
				Description = Resources.IDS_OGG_INFO_NONE
			};

			foreach ((string tagName, string tagValue) tag in ParseTags(comment))
			{
				switch (tag.tagName)
				{
					case "TITLE":
					{
						tagInfo.SongName = tag.tagValue;
						break;
					}

					case "ARTIST":
					{
						tagInfo.Artist = tag.tagValue;
						break;
					}

					case "TRACKNUMBER":
					{
						tagInfo.TrackNum = tag.tagValue;
						break;
					}

					case "ALBUM":
					{
						tagInfo.Album = tag.tagValue;
						break;
					}

					case "GENRE":
					{
						tagInfo.Genre = tag.tagValue;
						break;
					}

					case "ORGANIZATION":
					{
						tagInfo.Organization = tag.tagValue;
						break;
					}

					case "COPYRIGHT":
					{
						tagInfo.Copyright = tag.tagValue;
						break;
					}

					case "DESCRIPTION":
					{
						tagInfo.Description = tag.tagValue;
						break;
					}
				}
			}

			return tagInfo;
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
		/// Read next block of data
		/// </summary>
		/********************************************************************/
		private int LoadData(int[][] outputBuffer, int countInFrames)
		{
			int offset = 0;
			int totalInFrames = 0;

			while (countInFrames > 0)
			{
				int done = (int)vorbisFile.Ov_Read_Float(out CPointer<float>[] buffer, countInFrames, out _);
				if (done == (int)VorbisError.Hole)
					continue;

				if (done <= 0)
					break;

				// Convert the floats into 32-bit integers
				for (int i = 0; i < channels; i++)
				{
					CPointer<float> inBuffer = buffer[i];
					int[] outBuffer = outputBuffer[i];

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
