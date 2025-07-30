/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.PumaTracker.Containers
{
	/// <summary>
	/// Holds information about a single position
	/// </summary>
	internal class Position
	{
		public VoicePosition[] VoicePosition { get; } = new VoicePosition[4];
		public byte Speed { get; set; }
	}
}
