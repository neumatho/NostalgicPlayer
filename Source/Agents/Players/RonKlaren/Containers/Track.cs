/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.RonKlaren.Containers
{
	/// <summary>
	/// Holds information about a track
	/// </summary>
	internal class Track
	{
		public int TrackNumber { get; set; }
		public short Transpose { get; set; }
		public ushort NumberOfRepeatTimes { get; set; }
	}
}
