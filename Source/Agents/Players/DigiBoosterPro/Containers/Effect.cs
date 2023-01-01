/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// The different effects
	/// </summary>
	internal enum Effect : uint8_t
	{
		None = 0,
		Arpreggio = 0,						// 00
		PortamentoUp,						// 01
		PortamentoDown,						// 02
		PortamentoToNote,					// 03
		Vibrato,							// 04
		PortamentoToNoteVolumeSlide,		// 05
		VibratoVolumeSlide,					// 06
		SetPanning = 8,						// 08
		SampleOffset,						// 09
		VolumeSlide,						// 0A
		PositionJump,						// 0B
		SetVolume,							// 0C
		PatternBreak,						// 0D
		ExtraEffects,						// 0E
		SetTempo,							// 0F
		SetGlobalVolume,					// 0G
		GlobalVolumeSlide,					// 0H
		PanningSlide = 0x19,				// 0P
		EchoSwitch = 0x1f,					// 0V
		EchoDelay,							// 0W
		EchoFeedback,						// 0X
		EchoMix,							// 0Y
		EchoCross							// 0Z
	}
}
