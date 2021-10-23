/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.SidPlayFp
{
	/// <summary>
	/// This interface is used to get values from SidTune objects.
	///
	/// You must read (i.e. activate) sub-song specific information
	/// via:
	///	        const SidTuneInfo* tuneInfo = SidTune.getInfo();
	///	        const SidTuneInfo* tuneInfo = SidTune.getInfo(songNumber);
	/// </summary>
	internal abstract class SidTuneInfo
	{
		/// <summary>
		/// Different clock types
		/// </summary>
		public enum clock_t
		{
			/// <summary></summary>
			CLOCK_UNKNOWN,
			/// <summary></summary>
			CLOCK_PAL,
			/// <summary></summary>
			CLOCK_NTSC,
			/// <summary></summary>
			CLOCK_ANY
		}

		/// <summary>
		/// Different SID models
		/// </summary>
		public enum model_t
		{
			SIDMODEL_UNKNOWN,
			SIDMODEL_6581,
			SIDMODEL_8580,
			SIDMODEL_ANY
		}

		/// <summary>
		/// Compatibility modes
		/// </summary>
		public enum compatibility_t
		{
			/// <summary>
			/// File is C64 compatible
			/// </summary>
			COMPATIBILITY_C64,

			/// <summary>
			/// File is PSID specific
			/// </summary>
			COMPATIBILITY_PSID,

			/// <summary>
			/// File is Real C64 only
			/// </summary>
			COMPATIBILITY_R64,

			/// <summary>
			/// File requires C64 Basic
			/// </summary>
			COMPATIBILITY_BASIC
		}

		/// <summary>
		/// Vertical-Blanking-Interrupt
		/// </summary>
		public const int SPEED_VBI = 0;

		/// <summary>
		/// CIA 1 Timer A
		/// </summary>
		public const int SPEED_CIA_1A = 60;

		/********************************************************************/
		/// <summary>
		/// Load address
		/// </summary>
		/********************************************************************/
		public uint_least16_t LoadAddr()
		{
			return GetLoadAddr();
		}



		/********************************************************************/
		/// <summary>
		/// Init address
		/// </summary>
		/********************************************************************/
		public uint_least16_t InitAddr()
		{
			return GetInitAddr();
		}



		/********************************************************************/
		/// <summary>
		/// Play address
		/// </summary>
		/********************************************************************/
		public uint_least16_t PlayAddr()
		{
			return GetPlayAddr();
		}



		/********************************************************************/
		/// <summary>
		/// The number of songs
		/// </summary>
		/********************************************************************/
		public uint Songs()
		{
			return GetSongs();
		}



		/********************************************************************/
		/// <summary>
		/// The default starting song
		/// </summary>
		/********************************************************************/
		public uint StartSong()
		{
			return GetStartSong();
		}



		/********************************************************************/
		/// <summary>
		/// The tune that has been initialized
		/// </summary>
		/********************************************************************/
		public uint CurrentSong()
		{
			return GetCurrentSong();
		}



		/********************************************************************/
		/// <summary>
		/// The SID chip base address(es) used by the sid tune
		/// - 0xD400 for the 1st SID
		/// - 0 if the nth SID is not required
		/// </summary>
		/********************************************************************/
		public uint_least16_t SidChipBase(uint i)
		{
			return GetSidChipBase(i);
		}



		/********************************************************************/
		/// <summary>
		/// The number of SID chips required by the tune
		/// </summary>
		/********************************************************************/
		public int SidChips()
		{
			return GetSidChips();
		}



		/********************************************************************/
		/// <summary>
		/// Intended speed
		/// </summary>
		/********************************************************************/
		public int SongSpeed()
		{
			return GetSongSpeed();
		}



		/********************************************************************/
		/// <summary>
		/// First available page for relocation
		/// </summary>
		/********************************************************************/
		public uint_least8_t RelocStartPage()
		{
			return GetRelocStartPage();
		}



		/********************************************************************/
		/// <summary>
		/// Number of pages available for relocation
		/// </summary>
		/********************************************************************/
		public uint_least8_t RelocPages()
		{
			return GetRelocPages();
		}



		/********************************************************************/
		/// <summary>
		/// The SID chip model(s) requested by the sid tune
		/// </summary>
		/********************************************************************/
		public model_t SidModel(uint i)
		{
			return GetSidModel(i);
		}



		/********************************************************************/
		/// <summary>
		/// Compatibility requirements
		/// </summary>
		/********************************************************************/
		public compatibility_t Compatibility()
		{
			return GetCompatibility();
		}



		/********************************************************************/
		/// <summary>
		/// The number of available text info lines
		/// </summary>
		/********************************************************************/
		public uint NumberOfInfoStrings()
		{
			return GetNumberOfInfoStrings();
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
		public string InfoString(uint i)
		{
			return GetInfoString(i);
		}



		/********************************************************************/
		/// <summary>
		/// The number of available comment lines
		/// </summary>
		/********************************************************************/
		public uint NumberOfCommentStrings()
		{
			return GetNumberOfCommentStrings();
		}



		/********************************************************************/
		/// <summary>
		/// Comments
		/// </summary>
		/********************************************************************/
		public string CommentString(uint i)
		{
			return GetCommentString(i);
		}



		/********************************************************************/
		/// <summary>
		/// The number of available lyrics lines
		/// </summary>
		/********************************************************************/
		public uint NumberOfLyricsStrings()
		{
			return GetNumberOfLyricsStrings();
		}



		/********************************************************************/
		/// <summary>
		/// Lyrics
		/// </summary>
		/********************************************************************/
		public string LyricsString(uint i)
		{
			return GetLyricsString(i);
		}



		/********************************************************************/
		/// <summary>
		/// Length of single-file sid tune file
		/// </summary>
		/********************************************************************/
		public uint_least32_t DataFileLen()
		{
			return GetDataFileLen();
		}



		/********************************************************************/
		/// <summary>
		/// Length of raw C64 data without load address
		/// </summary>
		/********************************************************************/
		public uint_least32_t C64DataLen()
		{
			return GetC64DataLen();
		}



		/********************************************************************/
		/// <summary>
		/// The tune clock speed
		/// </summary>
		/********************************************************************/
		public clock_t ClockSpeed()
		{
			return GetClockSpeed();
		}



		/********************************************************************/
		/// <summary>
		/// The name of the identified file format
		/// </summary>
		/********************************************************************/
		public string FormatString()
		{
			return GetFormatString();
		}



		/********************************************************************/
		/// <summary>
		/// Whether load address might be duplicate
		/// </summary>
		/********************************************************************/
		public bool FixLoad()
		{
			return GetFixLoad();
		}



		/********************************************************************/
		/// <summary>
		/// Tells if the current tune is in MUS format
		/// </summary>
		/********************************************************************/
		public bool IsMusFormat()
		{
			return GetIsMusFormat();
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Load address
		/// </summary>
		/********************************************************************/
		protected abstract uint_least16_t GetLoadAddr();



		/********************************************************************/
		/// <summary>
		/// Init address
		/// </summary>
		/********************************************************************/
		protected abstract uint_least16_t GetInitAddr();



		/********************************************************************/
		/// <summary>
		/// Play address
		/// </summary>
		/********************************************************************/
		protected abstract uint_least16_t GetPlayAddr();



		/********************************************************************/
		/// <summary>
		/// The number of songs
		/// </summary>
		/********************************************************************/
		protected abstract uint GetSongs();



		/********************************************************************/
		/// <summary>
		/// The default starting song
		/// </summary>
		/********************************************************************/
		protected abstract uint GetStartSong();



		/********************************************************************/
		/// <summary>
		/// The tune that has been initialized
		/// </summary>
		/********************************************************************/
		protected abstract uint GetCurrentSong();



		/********************************************************************/
		/// <summary>
		/// The SID chip base address(es) used by the sid tune
		/// - 0xD400 for the 1st SID
		/// - 0 if the nth SID is not required
		/// </summary>
		/********************************************************************/
		protected abstract uint_least16_t GetSidChipBase(uint i);



		/********************************************************************/
		/// <summary>
		/// The number of SID chips required by the tune
		/// </summary>
		/********************************************************************/
		protected abstract int GetSidChips();



		/********************************************************************/
		/// <summary>
		/// Intended speed
		/// </summary>
		/********************************************************************/
		protected abstract int GetSongSpeed();



		/********************************************************************/
		/// <summary>
		/// First available page for relocation
		/// </summary>
		/********************************************************************/
		protected abstract uint_least8_t GetRelocStartPage();



		/********************************************************************/
		/// <summary>
		/// Number of pages available for relocation
		/// </summary>
		/********************************************************************/
		protected abstract uint_least8_t GetRelocPages();



		/********************************************************************/
		/// <summary>
		/// The SID chip model(s) requested by the sid tune
		/// </summary>
		/********************************************************************/
		protected abstract model_t GetSidModel(uint i);



		/********************************************************************/
		/// <summary>
		/// Compatibility requirements
		/// </summary>
		/********************************************************************/
		protected abstract compatibility_t GetCompatibility();



		/********************************************************************/
		/// <summary>
		/// The number of available text info lines
		/// </summary>
		/********************************************************************/
		protected abstract uint GetNumberOfInfoStrings();



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
		protected abstract string GetInfoString(uint i);



		/********************************************************************/
		/// <summary>
		/// The number of available comment lines
		/// </summary>
		/********************************************************************/
		protected abstract uint GetNumberOfCommentStrings();



		/********************************************************************/
		/// <summary>
		/// Comments
		/// </summary>
		/********************************************************************/
		protected abstract string GetCommentString(uint i);



		/********************************************************************/
		/// <summary>
		/// The number of available lyrics lines
		/// </summary>
		/********************************************************************/
		protected abstract uint GetNumberOfLyricsStrings();



		/********************************************************************/
		/// <summary>
		/// Lyrics
		/// </summary>
		/********************************************************************/
		protected abstract string GetLyricsString(uint i);



		/********************************************************************/
		/// <summary>
		/// Length of single-file sid tune file
		/// </summary>
		/********************************************************************/
		protected abstract uint_least32_t GetDataFileLen();



		/********************************************************************/
		/// <summary>
		/// Length of raw C64 data without load address
		/// </summary>
		/********************************************************************/
		protected abstract uint_least32_t GetC64DataLen();



		/********************************************************************/
		/// <summary>
		/// The tune clock speed
		/// </summary>
		/********************************************************************/
		protected abstract clock_t GetClockSpeed();



		/********************************************************************/
		/// <summary>
		/// The name of the identified file format
		/// </summary>
		/********************************************************************/
		protected abstract string GetFormatString();



		/********************************************************************/
		/// <summary>
		/// Whether load address might be duplicate
		/// </summary>
		/********************************************************************/
		protected abstract bool GetFixLoad();



		/********************************************************************/
		/// <summary>
		/// Tells if the current tune is in MUS format
		/// </summary>
		/********************************************************************/
		protected abstract bool GetIsMusFormat();
		#endregion
	}
}
