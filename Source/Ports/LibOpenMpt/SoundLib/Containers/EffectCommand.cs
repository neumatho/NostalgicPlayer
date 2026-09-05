/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Effect column commands
	/// </summary>
	internal enum EffectCommand : uint8
	{
		None = 0,
		Arpeggio = 1,
		PortamentoUp = 2,
		PortamentoDown = 3,
		TonePortamento = 4,
		Vibrato = 5,
		TonePortaVol = 6,
		VibratoVol = 7,
		Tremolo = 8,
		Panning8 = 9,
		Offset = 10,
		VolumeSlide = 11,
		PositionJump = 12,
		Volume = 13,
		PatternBreak = 14,
		Retrig = 15,
		Speed = 16,
		Tempo = 17,
		Tremor = 18,
		ModCmdEx = 19,
		S3MCmdEx = 20,
		ChannelVolume = 21,
		ChannelVolSlide = 22,
		GlobalVolume = 23,
		GlobalVolSlide = 24,
		KeyOff = 25,
		FineVibrato = 26,
		Panbrello = 27,
		XFinePortaUpDown = 28,
		PanningSlide = 29,
		SetEnvPosition = 30,
		Midi = 31,
		SmoothMidi = 32,
		DelayCut = 33,
		XParam = 34,
		FineTune = 35,
		FineTune_Smooth = 36,
		Dummy = 37,
		NoteSlideUp = 38,				// IMF Gxy / PTM Jxy (Slide y notes up every x ticks)
		NoteSlideDown = 39,				// IMF Hxy / PTM Kxy (Slide y notes down every x ticks)
		NoteSlideUpRetrig = 40,			// PTM Lxy (Slide y notes up every x ticks + retrigger note)
		NoteSlideDownRetrig = 41,		// PTM Mxy (Slide y notes down every x ticks + retrigger note)
		ReverseOffset = 42,				// PTM Nxx Revert sample + offset
		DbmEcho = 43,					// DBM enable/disable echo
		OffsetPercentage = 44,			// PLM Percentage Offset
		DigiReverseSample = 45,			// DIGI reverse sample
		Volume8 = 46,					// 8-bit volume
		Hmn_Mega_Arp = 47,				// His Master's Noise "mega-arp"
		Med_Synth_Jump = 48,			// MED synth jump / MIDI panning
		Auto_VolumeSlide = 49,
		Auto_PortaUp = 50,
		Auto_PortaDown = 51,
		Auto_PortaUp_Fine = 52,
		Auto_PortaDown_Fine = 53,
		Auto_Portamento_FC = 54,		// Future Composer
		TonePorta_Duration = 55,		// Parameter = how many rows the slide should last
		VolumeDown_Duration = 56,		// Parameter = how many rows the slide should last
		VolumeDown_Etx = 57,			// EasyTrax fade-out (parameter = speed, independent of song tempo)

		Max_Effects
	}
}
