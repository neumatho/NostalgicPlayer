/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Streamer.OggStreamer.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Streamer.OggStreamer
{
	/// <summary>
	/// Base class for the different Ogg streamers
	/// </summary>
	internal abstract class StreamerBase : StreamerAgentBase
	{
		protected int channels;
		protected int frequency;

		private TagInformation currentTagInfo = null;
		private int bitRate = 0;

		private const int InfoTrackNumberLine = 0;
		private const int InfoAlbumLine = 1;
		private const int InfoGenreLine = 2;
		private const int InfoOrganizationLine = 3;
		private const int InfoCopyrightLine = 4;
		private const int InfoDescriptionLine = 5;
		private const int InfoVendorLine = 6;
		private const int InfoBitRateLine = 7;

		/********************************************************************/
		/// <summary>
		/// Update the current bit rate
		/// </summary>
		/********************************************************************/
		protected void UpdateBitRate(int newBitRate)
		{
			if (newBitRate > 0)
			{
				newBitRate /= 1000;

				if (newBitRate != bitRate)
				{
					bitRate = newBitRate;

					OnModuleInfoChanged(InfoBitRateLine, bitRate.ToString());
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Update the current tag information
		/// </summary>
		/********************************************************************/
		protected void UpdateTagInformation(TagInformation tagInfo)
		{
			if (currentTagInfo != null)
			{
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
			}

			currentTagInfo = tagInfo;
		}

		#region Information
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
	}
}
