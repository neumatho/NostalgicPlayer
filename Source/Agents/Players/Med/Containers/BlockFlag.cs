/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Med.Containers
{
	/// <summary>
	/// Packed block flags
	/// </summary>
	[Flags]
	internal enum BlockFlag : byte
	{
		FirstHalfLineAll = 0x01,
		SecondHalfLineAll = 0x02,
		FirstHalfEffectAll = 0x04,
		SecondHalfEffectAll = 0x08,
		FirstHalfLineNone = 0x10,
		SecondHalfLineNone = 0x20,
		FirstHalfEffectNone = 0x40,
		SecondHalfEffectNone = 0x80
	}
}
