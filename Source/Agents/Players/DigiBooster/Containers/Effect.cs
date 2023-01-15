/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBooster.Containers
{
	/// <summary>
	/// Different effects
	/// </summary>
	internal enum Effect : byte
	{
		None = 0,
		Arpeggio = 0,						// 0
		PortamentoUp,						// 1
		PortamentoDown,						// 2
		Glissando,							// 3
		Vibrato,							// 4
		GlissandoVolumeSlide,				// 5
		VibratoVolumeSlide,					// 6
		Robot = 8,							// 8
		SampleOffset,						// 9
		VolumeSlide,						// A
		SongRepeat,							// B
		SetVolume,							// C
		PatternBreak,						// D
		Extra,								// E
		SetSpeed							// F
	}
}
