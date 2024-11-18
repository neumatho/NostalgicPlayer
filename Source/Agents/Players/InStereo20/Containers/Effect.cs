/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.InStereo20.Containers
{
	/// <summary>
	/// All the different effects
	/// </summary>
	internal enum Effect
	{
		Arpeggio = 0x0,						// 0
		SetSlideSpeed,						// 1
		RestartAdsr,						// 2
		SetVibrato = 4,						// 4
		SetPortamento = 7,					// 7
		SkipPortamento,						// 8
		SetTrackLen,						// 9
		SetVolumeIncrement,					// A
		PositionJump,						// B
		SetVolume,							// C
		TrackBreak,							// D
		SetFilter,							// E
		SetSpeed							// F
	}
}
