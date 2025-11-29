/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Streamer.OggStreamer.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Ports.LibOpusFile;
using Polycode.NostalgicPlayer.Ports.LibOpusFile.Containers;

namespace Polycode.NostalgicPlayer.Agent.Streamer.OggStreamer
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class OpusStreamerWorker : StreamerBase
	{
		private OpusFile opusFile;

		private float[] inputBuffer;

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
			// Load first block of data to check on
			byte[] buf = new byte[256];

			streamingStream.ReadExactly(buf, 0, buf.Length);

			if (OpusFile.Op_Test(null, buf, (ulong)buf.Length) == 0)
				return AgentResult.Ok;

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
			opusFile = OpusFile.Op_Open_Stream(streamingStream, true, out OpusFileError error);
			if (error != OpusFileError.Ok)
			{
				errorMessage = GetErrorString(error);
				return false;
			}

			// Get player data
			channels = opusFile.Op_Channel_Count(0);
			if (channels > 2)
			{
				errorMessage = string.Format(Resources.IDS_OGG_ERR_ILLEGAL_CHANNELS, channels);
				return false;
			}

			// Allocate buffers
			inputBuffer = new float[channels * (120 * 48000 / 1000 * 2)];		// 120 * 2 = 240 ms per channel at 48 kHz

			// Opus always use 48 kHz
			frequency = 48000;

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
			inputBuffer = null;

			if (opusFile != null)
			{
				opusFile.Op_Free();
				opusFile = null;
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
			int newBitRate = opusFile.Op_Bitrate_Instant();
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
		private string GetErrorString(OpusFileError error)
		{
			switch (error)
			{
				case OpusFileError.Hole:
					return Resources.IDS_OGG_ERR_HOLE;

				case OpusFileError.Read:
					return Resources.IDS_OGG_ERR_READ;

				case OpusFileError.Fault:
					return Resources.IDS_OGG_ERR_FAULT;

				case OpusFileError.Impl:
					return Resources.IDS_OGG_ERR_IMPL;

				case OpusFileError.Inval:
					return Resources.IDS_OGG_ERR_INVAL;

				case OpusFileError.NotFormat:
					return Resources.IDS_OGG_ERR_NOT_FORMAT;

				case OpusFileError.BadHeader:
					return Resources.IDS_OGG_ERR_BAD_HEADER;

				case OpusFileError.Version:
					return Resources.IDS_OGG_ERR_VERSION;

				case OpusFileError.BadPacket:
					return Resources.IDS_OGG_ERR_BAD_PACKET;

				case OpusFileError.BadLink:
					return Resources.IDS_OGG_ERR_BAD_LINK;

				case OpusFileError.NoSeek:
					return Resources.IDS_OGG_ERR_NO_SEEK;

				case OpusFileError.BadTimestamp:
					return Resources.IDS_OGG_ERR_BAD_TIMESTAMP;

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
			OpusTags tags = opusFile.Op_Tags(0);

			string vendor = Encoding.UTF8.GetString(tags.Vendor.Buffer, tags.Vendor.Offset, tags.Vendor.Length - 1);
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

			foreach (var tag in ParseTags(tags))
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
		/// Read next block of data
		/// </summary>
		/********************************************************************/
		private int LoadData(int[][] outputBuffer, int countInFrames)
		{
			int offset = 0;
			int totalInFrames = 0;

			while (countInFrames > 0)
			{
				int todoInFrames = Math.Min(countInFrames, inputBuffer.Length);
				int done = opusFile.Op_Read_Float(inputBuffer, todoInFrames, out _);
				if (done == (int)OpusFileError.Hole)
					continue;

				if (done <= 0)
					break;

				// Convert the floats into 32-bit integers
				for (int i = 0; i < channels; i++)
				{
					int[] outBuffer = outputBuffer[i];

					for (int j = 0, inOffset = i; j < done; j++, inOffset += channels)
						outBuffer[offset + j] = Math.Clamp((int)(inputBuffer[inOffset] * 0x8000000), -0x8000000, 0x7ffffff) << 4;
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
