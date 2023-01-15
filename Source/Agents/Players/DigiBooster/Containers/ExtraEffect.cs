/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBooster.Containers
{
	/// <summary>
	/// The different effects
	/// </summary>
	internal enum ExtraEffect : byte
	{
		Filter = 0x00,						// E0
		FineSlideUp = 0x10,					// E1
		FineSlideDown = 0x20,				// E2
		BackwardPlay = 0x30,				// E3
		StopPlaying = 0x40,					// E4
		ChannelOnOff = 0x50,				// E5
		Loop = 0x60,						// E6
		SampleOffset = 0x80,				// E8
		Retrace = 0x90,						// E9
		FineVolumeUp = 0xa0,				// EA
		FineVolumeDown = 0xb0,				// EB
		CutSample = 0xc0,					// EC
		PatternDelay = 0xe0					// EE
	}
}
