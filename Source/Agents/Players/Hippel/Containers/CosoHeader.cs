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
		public uint FrequenciesOffset;
		public uint EnvelopesOffset;
		public uint TracksOffset;
		public uint PositionListOffset;
		public uint SubSongsOffset;
		public uint SampleInfoOffset;
		public int SampleDataOffset;
	}
}
