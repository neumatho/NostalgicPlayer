/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Tremor_Flag
	{
		public const c_int Suppress = 0x40;		// Ignore tremor state until next update (FT2)
		public const c_int On = 0x80;
	}
}
