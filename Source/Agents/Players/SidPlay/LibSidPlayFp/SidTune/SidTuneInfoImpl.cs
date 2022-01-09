/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.SidPlayFp;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.SidTune
{
	/// <summary>
	/// The implementation of the SidTuneInfo interface
	/// </summary>
	internal sealed class SidTuneInfoImpl : SidTuneInfo
	{
		public string formatString;

		public uint songs;
		public uint startSong;
		public uint currentSong;

		public int songSpeed;

		public clock_t clockSpeed;

		public compatibility_t compatibility;

		public uint_least32_t dataFileLen;

		public uint_least32_t c64DataLen;

		public uint_least16_t loadAddr;
		public uint_least16_t initAddr;
		public uint_least16_t playAddr;

		public uint_least8_t relocStartPage;

		public uint_least8_t relocPages;

		public readonly List<model_t> sidModels = new List<model_t>();

		public readonly List<uint_least16_t> sidChipAddresses = new List<uint_least16_t>();

		public readonly List<string> infoString = new List<string>();
		public readonly List<string> commentString = new List<string>();
		public readonly List<string> lyricsString = new List<string>();

		public bool fixLoad;
		public bool isMusFormat;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SidTuneInfoImpl()
		{
			formatString = Resources.IDS_SID_NA;
			songs = 0;
			startSong = 0;
			currentSong = 0;
			songSpeed = SPEED_VBI;
			clockSpeed = clock_t.CLOCK_UNKNOWN;
			compatibility = compatibility_t.COMPATIBILITY_C64;
			dataFileLen = 0;
			c64DataLen = 0;
			loadAddr = 0;
			initAddr = 0;
			playAddr = 0;
			relocStartPage = 0;
			relocPages = 0;
			fixLoad = false;
			isMusFormat = false;

			sidModels.Add(model_t.SIDMODEL_UNKNOWN);
			sidChipAddresses.Add(0xd400);
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Load address
		/// </summary>
		/********************************************************************/
		protected override uint_least16_t GetLoadAddr()
		{
			return loadAddr;
		}



		/********************************************************************/
		/// <summary>
		/// Init address
		/// </summary>
		/********************************************************************/
		protected override uint_least16_t GetInitAddr()
		{
			return initAddr;
		}



		/********************************************************************/
		/// <summary>
		/// Play address
		/// </summary>
		/********************************************************************/
		protected override uint_least16_t GetPlayAddr()
		{
			return playAddr;
		}



		/********************************************************************/
		/// <summary>
		/// The number of songs
		/// </summary>
		/********************************************************************/
		protected override uint GetSongs()
		{
			return songs;
		}



		/********************************************************************/
		/// <summary>
		/// The default starting song
		/// </summary>
		/********************************************************************/
		protected override uint GetStartSong()
		{
			return startSong;
		}



		/********************************************************************/
		/// <summary>
		/// The tune that has been initialized
		/// </summary>
		/********************************************************************/
		protected override uint GetCurrentSong()
		{
			return currentSong;
		}



		/********************************************************************/
		/// <summary>
		/// The SID chip base address(es) used by the sid tune
		/// - 0xD400 for the 1st SID
		/// - 0 if the nth SID is not required
		/// </summary>
		/********************************************************************/
		protected override uint_least16_t GetSidChipBase(uint i)
		{
			return i < sidChipAddresses.Count ? sidChipAddresses[(int)i] : (uint_least16_t)0;
		}



		/********************************************************************/
		/// <summary>
		/// The number of SID chips required by the tune
		/// </summary>
		/********************************************************************/
		protected override int GetSidChips()
		{
			return sidChipAddresses.Count;
		}



		/********************************************************************/
		/// <summary>
		/// Intended speed
		/// </summary>
		/********************************************************************/
		protected override int GetSongSpeed()
		{
			return songSpeed;
		}



		/********************************************************************/
		/// <summary>
		/// First available page for relocation
		/// </summary>
		/********************************************************************/
		protected override uint_least8_t GetRelocStartPage()
		{
			return relocStartPage;
		}



		/********************************************************************/
		/// <summary>
		/// Number of pages available for relocation
		/// </summary>
		/********************************************************************/
		protected override uint_least8_t GetRelocPages()
		{
			return relocPages;
		}



		/********************************************************************/
		/// <summary>
		/// The SID chip model(s) requested by the sid tune
		/// </summary>
		/********************************************************************/
		protected override model_t GetSidModel(uint i)
		{
			return i < sidModels.Count ? sidModels[(int)i] : model_t.SIDMODEL_UNKNOWN;
		}



		/********************************************************************/
		/// <summary>
		/// Compatibility requirements
		/// </summary>
		/********************************************************************/
		protected override compatibility_t GetCompatibility()
		{
			return compatibility;
		}



		/********************************************************************/
		/// <summary>
		/// The number of available text info lines
		/// </summary>
		/********************************************************************/
		protected override uint GetNumberOfInfoStrings()
		{
			return (uint)infoString.Count;
		}



		/********************************************************************/
		/// <summary>
		/// Text info from the format headers etc.
		///
		/// - 0 = Title
		/// - 1 = Author
		/// - 2 = Released
		/// - ....
		/// </summary>
		/********************************************************************/
		protected override string GetInfoString(uint i)
		{
			return i < GetNumberOfInfoStrings() ? infoString[(int)i] : string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// The number of available comment lines
		/// </summary>
		/********************************************************************/
		protected override uint GetNumberOfCommentStrings()
		{
			return (uint)commentString.Count;
		}



		/********************************************************************/
		/// <summary>
		/// Comments
		/// </summary>
		/********************************************************************/
		protected override string GetCommentString(uint i)
		{
			return i < GetNumberOfCommentStrings() ? commentString[(int)i] : string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// The number of available lyrics lines
		/// </summary>
		/********************************************************************/
		protected override uint GetNumberOfLyricsStrings()
		{
			return (uint)lyricsString.Count;
		}



		/********************************************************************/
		/// <summary>
		/// Lyrics
		/// </summary>
		/********************************************************************/
		protected override string GetLyricsString(uint i)
		{
			return i < GetNumberOfLyricsStrings() ? lyricsString[(int)i] : string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Length of single-file sid tune file
		/// </summary>
		/********************************************************************/
		protected override uint_least32_t GetDataFileLen()
		{
			return dataFileLen;
		}



		/********************************************************************/
		/// <summary>
		/// Length of raw C64 data without load address
		/// </summary>
		/********************************************************************/
		protected override uint_least32_t GetC64DataLen()
		{
			return c64DataLen;
		}



		/********************************************************************/
		/// <summary>
		/// The tune clock speed
		/// </summary>
		/********************************************************************/
		protected override clock_t GetClockSpeed()
		{
			return clockSpeed;
		}



		/********************************************************************/
		/// <summary>
		/// The name of the identified file format
		/// </summary>
		/********************************************************************/
		protected override string GetFormatString()
		{
			return formatString;
		}



		/********************************************************************/
		/// <summary>
		/// Whether load address might be duplicate
		/// </summary>
		/********************************************************************/
		protected override bool GetFixLoad()
		{
			return fixLoad;
		}



		/********************************************************************/
		/// <summary>
		/// Tells if the current tune is in MUS format
		/// </summary>
		/********************************************************************/
		protected override bool GetIsMusFormat()
		{
			return isMusFormat;
		}
		#endregion
	}
}
