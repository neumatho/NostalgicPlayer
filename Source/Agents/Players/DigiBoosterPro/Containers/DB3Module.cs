/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// Complete module
	/// </summary>
	internal class DB3Module
	{
		public string Name;
		public uint16_t CreatorVersion;
		public uint16_t CreatorRevision;
		public uint16_t NumberOfInstruments;
		public uint16_t NumberOfSamples;
		public uint16_t NumberOfSongs;
		public uint16_t NumberOfPatterns;
		public uint16_t NumberOfTracks;
		public uint16_t NumberOfVolumeEnvelopes;
		public uint16_t NumberOfPanningEnvelopes;
		public DB3ModuleInstrument[] Instruments;
		public DB3ModuleSample[] Samples;
		public DB3ModuleSong[] Songs;
		public DB3ModulePattern[] Patterns;
		public DB3ModuleEnvelope[] VolumeEnvelopes;
		public DB3ModuleEnvelope[] PanningEnvelopes;
		public DB3GlobalDsp DspDefaults;
	}
}
