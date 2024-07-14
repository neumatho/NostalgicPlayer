/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.ArtOfNoise.Containers
{
	/// <summary>
	/// The different extra effects
	/// </summary>
	internal enum ExtraEffect : byte
	{
		SetFilter = 0x00,					// E0
		FineSlideUp = 0x10,					// E1
		FineSlideDown = 0x20,				// E2
		SetVibratoWaveform = 0x40,			// E4
		SetLoop = 0x50,						// E5
		PatternLoop = 0x60,					// E6
		RetrigNote = 0x90,					// E9
		FineVolumeSlideUp = 0xa0,			// EA
		FineVolumeSlideDown = 0xb0,			// EB
		NoteCut = 0xc0,						// EC
		NoteDelay = 0xd0,					// ED
		PatternDelay = 0xe0					// EE
	}
}
