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
	internal class Mmd0SongData
	{
		public ushort NumBlocks { get; set; }
		public ushort SongLen { get; set; }
		public byte[] PlaySeq { get; } = new byte[256];
		public ushort DefTempo { get; set; }
		public sbyte PlayTransp { get; set; }
		public MmdFlag Flags { get; set; }
		public MmdFlag2 Flags2 { get; set; }
		public byte Tempo2 { get; set; }
		public byte[] TrkVol { get; } = new byte[16];
		public byte MasterVol { get; set; }
		public byte NumSamples { get; set; }
	}
}
