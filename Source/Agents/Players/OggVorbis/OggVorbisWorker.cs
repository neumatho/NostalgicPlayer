/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using NVorbis;
using NVorbis.Contracts;
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
			// Initialize the reader
			reader = new VorbisReader(moduleStream);

			// Remember the frequency to play with
			channels = reader.Channels;
			frequency = reader.SampleRate;

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

			return base.InitPlayer(moduleStream, out errorMessage);
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
