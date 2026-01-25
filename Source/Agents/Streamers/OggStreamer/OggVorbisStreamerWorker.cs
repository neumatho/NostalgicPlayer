/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Streamer.OggStreamer.Containers;
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
	internal class OggVorbisStreamerWorker : StreamerBase
	{
		private VorbisFile vorbisFile;

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
			TagInformation tagInfo = RetrieveTagInfo();
			UpdateTagInformation(tagInfo);

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
			UpdateBitRate(newBitRate);

			// Has the meta information changed
			TagInformation tagInfo = RetrieveTagInfo();
			UpdateTagInformation(tagInfo);

			return filledInFrames;
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

			string vendor = Encoding.UTF8.GetString(comment.vendor.AsSpan(comment.vendor.Length - 1));
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
				string tag = encoder.GetString(comment.user_comments[i].AsSpan(length));

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
