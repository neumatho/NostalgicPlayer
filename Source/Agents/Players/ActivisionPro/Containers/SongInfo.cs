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
//		public readonly Position[][] PositionLists = new Position[4][];
		public readonly byte[][] PositionLists = new byte[4][];
		public readonly sbyte[] SpeedVariation = new sbyte[8];
	}
}
