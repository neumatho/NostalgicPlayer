/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player
{
	/// <summary>
	/// Holds all the information about the player state at a specific time
	/// </summary>
	internal class Snapshot : ISnapshot
	{
		public Player_Data PlayerData;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Snapshot(Player_Data playerData)
		{
			PlayerData = playerData.MakeDeepClone();
		}
	}
}
