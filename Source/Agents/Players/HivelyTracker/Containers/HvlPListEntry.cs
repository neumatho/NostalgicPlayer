/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.HivelyTracker.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class HvlPListEntry
	{
		public int Note { get; set; }
		public bool Fixed { get; set; }
		public int Waveform { get; set; }
		public int[] Fx { get; set; }
		public int[] FxParam { get; set; }
	}
}
