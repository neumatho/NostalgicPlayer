/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// Global DSP echo parameters
	/// </summary>
	internal class DB3GlobalDsp
	{
		public uint32_t[] EffectMask { get; set; }		// One longword per track, bit 0 is for global echo
		public uint8_t EchoDelay { get; set; }			// 2 ms units
		public uint8_t EchoFeedback { get; set; }
		public uint8_t EchoMix { get; set; }
		public uint8_t EchoCross { get; set; }
	}
}
