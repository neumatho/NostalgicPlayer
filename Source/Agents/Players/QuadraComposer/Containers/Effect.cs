/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.QuadraComposer.Containers
{
	/// <summary>
	/// The different effects
	/// </summary>
	internal enum Effect : byte
	{
		Arpeggio = 0x0,						// 0
		SlideUp,							// 1
		SlideDown,							// 2
		TonePortamento,						// 3
		Vibrato,							// 4
		TonePortamentoAndVolumeSlide,		// 5
		VibratoAndVolumeSlide,				// 6
		Tremolo,							// 7
		SetSampleOffset = 0x9,				// 9
		VolumeSlide,						// A
		PositionJump,						// B
		SetVolume,							// C
		PatternBreak,						// D
		ExtraEffects,						// E
		SetSpeed							// F
	}
}
