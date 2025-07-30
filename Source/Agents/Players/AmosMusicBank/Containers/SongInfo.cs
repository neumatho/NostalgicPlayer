/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.AmosMusicBank.Containers
{
	/// <summary>
	/// Contains information about a single sub-song
	/// </summary>
	internal class SongInfo
	{
		public string Name { get; set; }
		public PositionList[] PositionLists { get; set; }
	}
}
