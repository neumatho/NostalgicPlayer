/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.HivelyTracker.Containers
{
	/// <summary>
	/// Holds information about a single track step
	/// </summary>
	internal class HvlStep
	{
		public int Note { get; set; }
		public int Instrument { get; set; }
		public int Fx { get; set; }
		public int FxParam { get; set; }
		public int FxB { get; set; }
		public int FxBParam { get; set; }
	}
}
