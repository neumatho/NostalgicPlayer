/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.FaceTheMusic.Containers
{
	/// <summary>
	/// Holds information about a single track
	/// </summary>
	internal class Track
	{
		public ushort DefaultSpacing { get; set; }
		public TrackLine[] Lines { get; set; }
	}
}
