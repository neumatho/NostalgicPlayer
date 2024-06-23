/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers
{
	internal enum Effect : byte
	{
		Note = 0x01,
		Instrument = 0x02,
		Arpeggio = 0x03,					// (IT, XM, S3M, MOD, MTM)
		PortamentoUp1 = 0x04,				// (XM, MOD, MTM) [for formats with separate fine slides]
		PortamentoDown1 = 0x05,				// (XM, MOD, MTM) [for formats with separate fine slides]
		TonePortamento = 0x06,				// (IT, XM, S3M, MOD, MTM) / Vol_TonePortamento (IT, XM)
		Vibrato = 0x07,						// (IT, XM, S3M, MOD, MTM) / Vol_VibratoDepth (IT)
		TonePortaVol = 0x08,				// (XM, MOD, MTM)
		VibratoVol = 0x09,					// (XM, MOD. MTM)
		Tremolo = 0x0a,						// (IT, XM, S3M, MOD, MTM)
		Panning8 = 0x0b,					// (IT, XM, S3M, MOD, MTM) / Vol_Panning (IT, XM)
		Offset = 0x0c,						// (IT, XM, S3M, MOD, MTM)
		VolumeSlide1 = 0x0d,				// (XM, MOD, MTM)
		PositionJump = 0x0e,				// (IT, XM, S3M, MOD, MTM)
		Volume = 0x0f,						// (XM, MOD, MTM) / Vol_Volume (IT, XM, S3M)
		PatternBreak = 0x10,				// (IT, XM, MOD, MTM) - BCD-encoded in MOD/XM/S3M/MTM
		ModCmdEx = 0x11,					// (XM, MOD, MTM)
		Tempo1 = 0x12,						// (XM, MOD, MTM) / Speed (XM, MOD, MTM)
		Tremor1 = 0x13,						// (XM)
		Vol_VolSlide1 = 0x14,				// Up x=X0 (XM) / Down x=0X (XM)
		Vol_FineVol = 0x15,					// Up x=X0 (XM) / Down x=0X (XM)
		GlobalVolume = 0x16,				// (IT, XM, S3M)
		GlobalVolSlide1 = 0x17,				// (XM)
		KeyOff = 0x18,						// (XM)
		SetEnvPosition = 0x19,				// (XM)
		PanningSlide1 = 0x1a,				// (XM)
		Vol_PanSlide = 0x1b,				// Left x=0X (XM) / Right x=X0 (XM)
		Retrig1 = 0x1c,						// (XM)
		XFinePortaUp = 0x1d,				// X1x (XM)
		XFinePortaDown = 0x1e,				// X2x (XM)
		Vol_VibratoSpeed = 0x1f,			// (XM)
		Vol_VibratoDepth = 0x20,			// (XM)
		Speed = 0x21,						// (IT, S3M)
		VolumeSlide2 = 0x22,				// (IT, S3M)
		PortamentoDown2 = 0x23,				// (IT, S3M) [for formats without separate fine slides]
		PortamentoUp2 = 0x24,				// (IT, S3M) [for formats without separate fine slides]
		Tremor2 = 0x25,						// (IT, S3M)
		Retrig2 = 0x26,						// (IT, S3M)
		FineVibrato = 0x27,					// (IT, S3M)
		ChannelVolume = 0x28,				// (IT, S3M)
		ChannelVolSlide = 0x29,				// (IT, S3M)
		PanningSlide2 = 0x2a,				// (IT, S3M)
		S3MCmdEx = 0x2b,					// (IT, S3M)
		Tempo2 = 0x2c,						// (IT, S3M)
		GlobalVolSlide2 = 0x2d,				// (IT, S3M)
		Panbrello = 0x2e,					// (IT, XM, S3M)
		Midi = 0x2f,						// (IT, XM, S3M)
		Vol_VolSlide2 = 0x30,				// Fine up x=0...9 (IT) / Fine down x=10...19 (IT) / Up x=20...29 (IT) / Down x=30...39 (IT)
		Vol_PortaDown = 0x31,				// (IT)
		Vol_PortaUp = 0x32,					// (IT)
		UnusedW = 0x33,						// Unused XM command "W" (XM)
		Vol_ItOther = 0x34,					// Any other IT volume column command to support OpenMPT extensions (IT)
		XParam = 0x35,						// (IT)
		SmoothMidi = 0x36,					// (IT)
		DelayCut = 0x37,					// (IT)
		FineTune = 0x38,					// (MPTM)
		FineTuneSmooth = 0x39				// (MPTM)
	}
}
