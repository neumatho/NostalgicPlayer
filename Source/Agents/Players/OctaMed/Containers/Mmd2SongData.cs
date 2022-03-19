/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers
{
	/// <summary>
	/// MMD0 song data structure
	/// </summary>
	internal class Mmd2SongData
	{
		public ushort NumBlocks;
		public ushort NumSections;
		public uint PlaySeqTableOffs;
		public uint SectionTableOffs;
		public uint TrackVolsOffs;
		public ushort NumTracks;
		public ushort NumPlaySeqs;
		public uint TrackPansOffs;
		public MmdFlag3 Flags3;
		public ushort VolAdj;
		public ushort Channels;
		public byte MixEchoType;
		public byte MixEchoDepth;
		public ushort MixEchoLen;
		public sbyte MixStereoSep;
		public ushort DefTempo;
		public sbyte PlayTransp;
		public MmdFlag Flags;
		public MmdFlag2 Flags2;
		public byte Tempo2;
		public byte MasterVol;
		public byte NumSamples;
	}
}