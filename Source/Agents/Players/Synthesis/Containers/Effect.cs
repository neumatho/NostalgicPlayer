/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Synthesis.Containers
{
	/// <summary>
	/// All the different effects
	/// </summary>
	internal enum Effect
	{
		None = 0x0,							// 0
		Slide,								// 1
		RestartAdsr,						// 2
		RestartEgc,							// 3
		SetTrackLen,						// 4
		SkipStNt,							// 5
		SyncMark,							// 6
		SetFilter,							// 7
		SetSpeed,							// 8
		EnableFx,							// 9
		ChangeFx,							// A
		ChangeArg1,							// B
		ChangeArg2,							// C
		ChangeArg3,							// D
		EgcOff,								// E
		SetVolume							// F
	}
}
