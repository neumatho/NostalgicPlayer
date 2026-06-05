/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers
{
	/// <summary>
	/// Holds all the part effects
	/// </summary>
	internal enum PartEffect : byte
	{
		None = 0x00,

		// Pitch

		SlideUp = 0x01,
		SlideDown = 0x02,
		Portamento = 0x03,
		InitInstrumentPortamento = 0x04,
		PitchUp = 0x05,
		PitchDown = 0x06,
		VibratoSpeed = 0x07,
		VibratoUp = 0x08,
		VibratoDown = 0x09,
		VibratoWave = 0x0a,
		SetFineTune = 0x0b,

		// Instrument volume

		SetInstrumentVolume = 0x10,
		InstrumentVolumeSlideUp = 0x11,
		InstrumentVolumeSlideDown = 0x12,
		SetSlideInstrumentVolume = 0x13,
		SlideToInstrumentVolume = 0x14,
		InstrumentVolumeAdd = 0x15,
		InstrumentVolumeSub = 0x16,
		TremoloSpeed = 0x17,
		TremoloUp = 0x18,
		TremoloDown = 0x19,
		TremoloWave = 0x1a,

		// Channel volume

		SetChannelVolume = 0x20,
		ChannelVolumeSlideUp = 0x21,
		ChannelVolumeSlideDown = 0x22,
		SetSlideChannelVolume = 0x23,
		SlideToChannelVolume = 0x24,
		ChannelVolumeAdd = 0x25,
		ChannelVolumeSub = 0x26,
		SetVolumeAllChannels = 0x27,

		// Master volume

		SetMasterVolume = 0x30,
		MasterVolumeSlideUp = 0x31,
		MasterVolumeSlideDown = 0x32,
		SetSlideMasterVolume = 0x33,
		SlideToMasterVolume = 0x34,
		MasterVolumeAdd = 0x35,
		MasterVolumeSub = 0x36,

		// Other

		SpeedOneChannel = 0x40,
		GrooveOneChannel = 0x41,
		SpeedAllChannels = 0x42,
		GrooveAllChannels = 0x43,
		ArpeggioTable = 0x44,
		ArpeggioTableOneStep = 0x45,
		HoldSustain = 0x46,
		Filter = 0x47,
		SampleOffset = 0x48,
		RestartNoVolume = 0x49,
		WaveSample = 0x4a,
		InitInstrument = 0x4b,

		// ProTracker

		PtSlideUp = 0xe1,
		PtSlideDown = 0xe2,
		PtPortamento = 0xe3,
		PtFineSlideUp = 0xe4,
		PtFineSlideDown = 0xe5,
		PtVolumeSlideUp = 0xe6,
		PtVolumeSlideDown = 0xe7,
		PtTremolo = 0xe8,
		PtTremoloWave = 0xe9,
		PtVibrato = 0xea,
		PtVibratoWave = 0xeb
	}
}
