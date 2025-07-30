/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.ActivisionPro.Containers
{
	/// <summary>
	/// Contains information about a single sub-song
	/// </summary>
	internal class SongInfo
	{
		public byte[][] PositionLists { get; } = new byte[4][];
		public sbyte[] SpeedVariation { get; } = new sbyte[8];
	}
}
