/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers
{
	/// <summary>
	/// MMD0 song data structure
	/// </summary>
	internal class Mmd2SongData
	{
		public ushort NumBlocks { get; set; }
		public ushort NumSections { get; set; }
		public uint PlaySeqTableOffs { get; set; }
		public uint SectionTableOffs { get; set; }
		public uint TrackVolsOffs { get; set; }
		public ushort NumTracks { get; set; }
		public ushort NumPlaySeqs { get; set; }
		public uint TrackPansOffs { get; set; }
		public MmdFlag3 Flags3 { get; set; }
		public ushort VolAdj { get; set; }
		public ushort Channels { get; set; }
		public byte MixEchoType { get; set; }
		public byte MixEchoDepth { get; set; }
		public ushort MixEchoLen { get; set; }
		public sbyte MixStereoSep { get; set; }
		public ushort DefTempo { get; set; }
		public sbyte PlayTransp { get; set; }
		public MmdFlag Flags { get; set; }
		public MmdFlag2 Flags2 { get; set; }
		public byte Tempo2 { get; set; }
		public byte MasterVol { get; set; }
		public byte NumSamples { get; set; }
	}
}