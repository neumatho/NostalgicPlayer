/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Hippel.Containers
{
	/// <summary>
	/// COSO header information. Only used by the loader
	/// </summary>
	internal class CosoHeader
	{
		public uint FrequenciesOffset { get; set; }
		public uint EnvelopesOffset { get; set; }
		public uint TracksOffset { get; set; }
		public uint PositionListOffset { get; set; }
		public uint SubSongsOffset { get; set; }
		public uint SampleInfoOffset { get; set; }
		public int SampleDataOffset { get; set; }
	}
}
