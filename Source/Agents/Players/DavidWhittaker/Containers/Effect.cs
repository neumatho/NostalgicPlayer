/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DavidWhittaker.Containers
{
	/// <summary>
	/// Different effects
	/// </summary>
	internal enum Effect : byte
	{
		EndOfTrack = 0,						// 0
		Slide,								// 1
		Mute,								// 2
		WaitUntilNextRow,					// 3
		StopSong,							// 4
		GlobalTranspose,					// 5
		StartVibrato,						// 6
		StopVibrato,						// 7
		Effect8,							// 8
		Effect9,							// 9
		SetSpeed,							// A
		GlobalVolumeFade,					// B
		SetGlobalVolume,					// C
		StartOrStopSoundFx,					// D
		StopSoundFx							// E
	}
}