/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers
{
	/// <summary>
	/// An instance of this structure is used to transport values to
	/// and from SidTune objects
	/// </summary>
	internal class SidTuneInfo
	{
		public string FormatString;				// The name of the identified file format
		public string SpeedString;				// Describing the speed a song is running at

		public List<string> Comment;			// Information from MUS files
		public List<string> Lyrics;

		public ushort LoadAddr;
		public ushort InitAddr;
		public ushort PlayAddr;

		public ushort Songs;
		public ushort StartSong;

		// The SID chip base address(es) used by the sid tune
		public ushort SidChipBase1;				// 0xD400 (normal, 1st SID)
		public ushort SidChipBase2;				// 0xD?00 (2nd SID) or 0 (no 2nd SID)

		// Available after song initialization
		public ushort CurrentSong;				// The one that has been initialized
		public Speed SongSpeed;					// Intended speed

		public Clock ClockSpeed;
		public byte RelocStartPage;				// First available page for relocation
		public byte RelocPages;					// Number of pages available for relocation
		public bool MusPlayer;					// Whether Sidplayer routine has been installed
		public SidModel SidModel1;				// Sid Model required for first sid
		public SidModel SidModel2;				// Sid Model required for second sid

		public Compatibility Compatibility;		// Compatibility requirements
		public bool FixLoad;					// Whether load address might be duplicate

		public string Title;
		public string Author;
		public string Released;

		public uint C64DataLen;					// Length of raw C64 data without load address
	}
}
