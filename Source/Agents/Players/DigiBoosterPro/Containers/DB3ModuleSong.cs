/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// Complete song
	/// </summary>
	internal class DB3ModuleSong
	{
		public string Name;
		public uint16_t NumberOfOrders;			// Play list length
		public uint16_t[] PlayList;				// Play list table
	}
}
