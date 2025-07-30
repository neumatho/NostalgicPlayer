/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// Complete pattern
	/// </summary>
	internal class DB3ModulePattern
	{
		public uint16_t NumberOfRows { get; set; }
		public DB3ModuleEntry[] Pattern { get; set; }	// A table
	}
}
