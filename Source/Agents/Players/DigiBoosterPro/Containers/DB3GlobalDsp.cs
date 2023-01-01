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
		public uint32_t[] EffectMask;			// One longword per track, bit 0 is for global echo
		public uint8_t EchoDelay;				// 2 ms units
		public uint8_t EchoFeedback;
		public uint8_t EchoMix;
		public uint8_t EchoCross;
	}
}
