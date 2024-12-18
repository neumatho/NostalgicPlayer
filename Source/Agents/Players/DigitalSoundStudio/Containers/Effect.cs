/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigitalSoundStudio.Containers
{
	/// <summary>
	/// The different effects
	/// </summary>
	internal enum Effect : byte
	{
		Arpeggio = 0x0,						//  0
		SlideUp,							//  1
		SlideDown,							//  2
		SetVolume,							//  3
		SetMasterVolume,					//  4
		SetSongSpeed,						//  5
		PositionJump,						//  6
		SetFilter,							//  7
		PitchUp,							//  8
		PitchDown,							//  9
		PitchControl,						//  A
		SetSongTempo,						//  B
		VolumeUp,							//  C
		VolumeDown,							//  D
		VolumeSlideUp,						//  E
		VolumeSlideDown,					//  F
		MasterVolumeUp,						// 10
		MasterVolumeDown,					// 11
		MasterVolumeSlideUp,				// 12
		MasterVolumeSlideDown,				// 13
		SetLoopStart,						// 14
		JumpToLoop,							// 15
		RetrigNote,							// 16
		NoteDelay,							// 17
		NoteCut,							// 18
		SetSampleOffset,					// 19
		SetFineTune,						// 1A
		Portamento,							// 1B
		PortamentoVolumeSlideUp,			// 1C
		PortamentoVolumeSlideDown,			// 1D
		PortamentoControl					// 1E
	}
}
