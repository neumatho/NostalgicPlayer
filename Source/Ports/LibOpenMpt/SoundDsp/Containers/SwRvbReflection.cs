/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundDsp.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class SwRvbReflection
	{
		/// <summary>
		/// 
		/// </summary>
		public uint32 Delay;

		/// <summary>
		/// 
		/// </summary>
		public uint32 DelayDest;

		/// <summary>
		/// g_ll, g_rl, g_lr, g_rr
		/// </summary>
		public readonly LR16[] Gains = new LR16[2];
	}
}
