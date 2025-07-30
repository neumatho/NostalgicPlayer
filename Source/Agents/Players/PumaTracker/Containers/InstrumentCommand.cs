/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.PumaTracker.Containers
{
	/// <summary>
	/// Holds information about a single command (both volume and frequency)
	/// </summary>
	internal class InstrumentCommand
	{
		public byte Command { get; set; }
		public byte Argument1 { get; set; }
		public byte Argument2 { get; set; }
		public byte Argument3 { get; set; }
	}
}
