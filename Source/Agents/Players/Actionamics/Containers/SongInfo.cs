/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Actionamics.Containers
{
	/// <summary>
	/// Contains information about a single sub-song
	/// </summary>
	internal class SongInfo
	{
		public byte StartPosition { get; set; }
		public byte EndPosition { get; set; }
		public byte LoopPosition { get; set; }
		public byte Speed { get; set; }
	}
}
