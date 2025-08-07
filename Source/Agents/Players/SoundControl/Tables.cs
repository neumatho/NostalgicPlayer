/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundControl
{
	/// <summary>
	/// Different tables needed
	/// </summary>
	internal static class Tables
	{
		/********************************************************************/
		/// <summary>
		/// Base numbers needed to calculate the period table
		/// </summary>
		/********************************************************************/
		public static readonly ushort[] BasePeriod =
		[
			0xd600, 0xca00, 0xbe80, 0xb400, 0xa980, 0xa000, 0x9700, 0x8e80,
			0x8680, 0x7f00, 0x7800, 0x7100, 0x6b00, 0x0000, 0x0000, 0x0000
		];
	}
}
