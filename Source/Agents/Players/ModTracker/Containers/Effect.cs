/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker.Containers
{
	/// <summary>
	/// The different effects
	/// </summary>
	internal enum Effect : byte
	{
		Arpeggio = 0x00,			// 0x00
		SlideUp,					// 0x01
		SlideDown,					// 0x02
		TonePortamento,				// 0x03
		Vibrato,					// 0x04
		TonePort_VolSlide,			// 0x05
		Vibrato_VolSlide,			// 0x06
		Tremolo,					// 0x07
		MegaArp = 0x07,				// 0x07 (Only used in His Master's Noise)
		SetPanning,					// 0x08 (Only for FastTracker and up)
		SampleOffset,				// 0x09
		VolumeSlide,				// 0x0a
		PosJump,					// 0x0b
		SetVolume,					// 0x0c
		PatternBreak,				// 0x0d
		ExtraEffect,				// 0x0e
		SetSpeed					// 0x0f
	}
}
