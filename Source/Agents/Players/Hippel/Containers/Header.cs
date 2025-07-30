/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Hippel.Containers
{
	/// <summary>
	/// Header information. Only used by the loader
	/// </summary>
	internal class Header
	{
		public ushort NumberOfFrequencies { get; set; }
		public ushort NumberOfEnvelopes { get; set; }
		public ushort NumberOfTracks { get; set; }
		public ushort NumberOfPositions { get; set; }
		public ushort BytesPerTrack { get; set; }
		public ushort NumberOfSubSongs { get; set; }
		public ushort NumberOfSamples { get; set; }
	}
}
