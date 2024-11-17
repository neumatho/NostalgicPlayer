/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.InStereo10.Containers
{
	/// <summary>
	/// All the different effects
	/// </summary>
	internal enum Effect
	{
		None = 0x0,							// 0
		SetSlideSpeed,						// 1
		RestartAdsr,						// 2
		RestartEgc,							// 3
		SetSlideIncrement,					// 4
		SetVibratoDelay,					// 5
		SetVibratoPosition,					// 6
		SetVolume,							// 7
		SkipNt,								// 8
		SkipSt,								// 9
		SetTrackLen,						// A
		SkipPortamento,						// B
		EffC,								// C
		EffD,								// D
		SetFilter,							// E
		SetSpeed							// F
	}
}
