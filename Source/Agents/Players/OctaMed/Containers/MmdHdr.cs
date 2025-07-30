/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers
{
	/// <summary>
	/// Module header structure
	/// </summary>
	internal class MmdHdr
	{
		public uint Id { get; set; }
		public uint ModLen { get; set; }
		public uint SongOffs { get; set; }
		public ushort PSecNum { get; set; }
		public ushort PSeq { get; set; }
		public uint BlocksOffs { get; set; }
		public byte MmdFlags { get; set; }
		public uint SamplesOffs { get; set; }
		public uint ExpDataOffs { get; set; }
		public ushort PState { get; set; }
		public ushort PBlock { get; set; }
		public ushort PLine { get; set; }
		public ushort PSeqNum { get; set; }
		public short ActPlayLine { get; set; }
		public byte Counter { get; set; }
		public byte ExtraSongs { get; set; }
	}
}
