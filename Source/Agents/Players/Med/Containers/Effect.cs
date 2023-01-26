/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Med.Containers
{
	/// <summary>
	/// Different effects
	/// </summary>
	internal enum Effect : byte
	{
		None = 0,
		Arpeggio = 0,						// 0
		SlideUp,							// 1
		SlideDown,							// 2
		Vibrato,							// 3
		SetVolume = 0xc,					// C
		Crescendo,							// D
		Filter,								// E
		SetTempo							// F
	}
}
