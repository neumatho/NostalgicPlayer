/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player
{
	[Flags]
	internal enum Channel_Flag : c_int
	{
		// ReSharper disable InconsistentNaming
		None = 0,
		Vol_Slide = 1 << 0,
		Pan_Slide = 1 << 1,
		TonePorta = 1 << 2,
		PitchBend = 1 << 3,
		Vibrato = 1 << 4,
		Tremolo = 1 << 5,
		Fine_Vols = 1 << 6,
		Fine_Bend = 1 << 7,
		Offset = 1 << 8,
		Trk_VSlide = 1 << 9,
		Trk_FVSlide = 1 << 10,
		New_Ins = 1 << 11,
		New_Vol = 1 << 12,
		Vol_Slide_2 = 1 << 13,
		Note_Slide = 1 << 14,
		Fine_NSlide = 1 << 15,
		New_Note = 1 << 16,
		Fine_TPorta = 1 << 17,
		Retrig = 1 << 18,
		Panbrello = 1 << 19,
		GVol_Slide = 1 << 20,
		Tempo_Slide = 1 << 21,
		VEnv_Pause = 1 << 22,
		PEnv_Pause = 1 << 23,
		FEnv_Pause = 1 << 24,
		Fine_Vols_2 = 1 << 25,
		Key_Off = 1 << 26,				// For IT release on envloop end
		Tremor = 1 << 27,				// For XM tremor
		Midi_Macro = 1 << 28			// IT midi macro
		// ReSharper restore InconsistentNaming
	}
}
