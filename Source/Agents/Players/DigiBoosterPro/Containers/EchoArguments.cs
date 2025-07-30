/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// Holds the different arguments to make the echo
	/// </summary>
	internal struct EchoArguments
	{
		public int EchoDelay { get; set; }				// 0 to 255, 2 ms (tracker units)
		public int EchoFeedback { get; set; }			// 0 to 255
		public int EchoMix { get; set; }				// 0 dry, 255 wet
		public int EchoCross { get; set; }				// 0 to 255
	}
}
