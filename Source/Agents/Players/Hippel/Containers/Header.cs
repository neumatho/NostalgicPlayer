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
		public ushort NumberOfFrequencies;
		public ushort NumberOfEnvelopes;
		public ushort NumberOfTracks;
		public ushort NumberOfPositions;
		public ushort BytesPerTrack;
		public ushort NumberOfSubSongs;
		public ushort NumberOfSamples;
	}
}
