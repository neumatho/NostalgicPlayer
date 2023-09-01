/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Effects
	{
		/// <summary>
		/// ProTracker effects
		/// </summary>
		public const uint8 Fx_Arpeggio = 0x00;
		public const uint8 Fx_Porta_Up = 0x01;
		public const uint8 Fx_Porta_Dn = 0x02;
		public const uint8 Fx_TonePorta = 0x03;
		public const uint8 Fx_Vibrato = 0x04;
		public const uint8 Fx_Tone_VSlide = 0x05;
		public const uint8 Fx_Vibra_VSlide = 0x06;
		public const uint8 Fx_Tremolo = 0x07;
		public const uint8 Fx_Offset = 0x09;
		public const uint8 Fx_VolSlide = 0x0a;
		public const uint8 Fx_Jump = 0x0b;
		public const uint8 Fx_VolSet = 0x0c;
		public const uint8 Fx_Break = 0x0d;
		public const uint8 Fx_Extended = 0x0e;
		public const uint8 Fx_Speed = 0x0f;

		/// <summary>
		/// Fast Tracker effects
		/// </summary>
		public const uint8 Fx_SetPan = 0x08;

		/// <summary>
		/// Fast Tracker II effects
		/// </summary>
		public const uint8 Fx_GlobalVol = 0x10;
		public const uint8 Fx_GVol_Slide = 0x11;
		public const uint8 Fx_Keyoff = 0x14;
		public const uint8 Fx_EnvPos = 0x15;
		public const uint8 Fx_PanSlide = 0x19;
		public const uint8 Fx_Multi_Retrig = 0x1b;
		public const uint8 Fx_Tremor = 0x1d;
		public const uint8 Fx_Xf_Porta = 0x21;

		/// <summary>
		/// ProTracker extended effects
		/// </summary>
		public const uint8 Ex_Filter = 0x00;
		public const uint8 Ex_F_Porta_Up = 0x01;
		public const uint8 Ex_F_Porta_Dn = 0x02;
		public const uint8 Ex_Gliss = 0x03;
		public const uint8 Ex_Vibrato_Wf = 0x04;
		public const uint8 Ex_FineTune = 0x05;
		public const uint8 Ex_Pattern_Loop = 0x06;
		public const uint8 Ex_Tremolo_Wf = 0x07;
		public const uint8 Ex_SetPan = 0x08;
		public const uint8 Ex_Retrig = 0x09;
		public const uint8 Ex_F_VSlide_Up = 0x0a;
		public const uint8 Ex_F_VSlide_Dn = 0x0b;
		public const uint8 Ex_Cut = 0x0c;
		public const uint8 Ex_Delay = 0x0d;
		public const uint8 Ex_Patt_Delay = 0x0e;
		public const uint8 Ex_InvLoop = 0x0f;

		/// <summary>
		/// Oktalyzer effects
		/// </summary>
		public const uint8 Fx_Okt_Arp3 = 0x70;
		public const uint8 Fx_Okt_Arp4 = 0x71;
		public const uint8 Fx_Okt_Arp5 = 0x72;
		public const uint8 Fx_NSlide2_Dn = 0x73;
		public const uint8 Fx_NSlide2_Up = 0x74;
		public const uint8 Fx_F_NSlide_Dn = 0x75;
		public const uint8 Fx_F_NSlide_Up = 0x76;

		/// <summary>
		/// Persistent effects -- for FNK
		/// </summary>
		public const uint8 Fx_Per_Porta_Dn = 0x78;
		public const uint8 Fx_Per_Porta_Up = 0x79;
		public const uint8 Fx_Per_TPorta = 0x7a;
		public const uint8 Fx_Per_Vibrato = 0x7b;
		public const uint8 Fx_Per_VSld_Up = 0x7c;
		public const uint8 Fx_Per_VSld_Dn = 0x7d;
		public const uint8 Fx_Speed_Cp = 0x7e;
		public const uint8 Fx_Per_Cancel = 0x7f;

		/// <summary>
		/// 669 frequency based effects
		/// </summary>
		public const uint8 Fx_669_Porta_Up = 0x60;
		public const uint8 Fx_669_Porta_Dn = 0x61;
		public const uint8 Fx_669_TPorta = 0x62;
		public const uint8 Fx_669_FineTune = 0x63;
		public const uint8 Fx_669_Vibrato = 0x64;

		/// <summary>
		/// FAR effects
		/// </summary>
		public const uint8 Fx_Far_Porta_Up = 0x65;		// FAR pitch offset up
		public const uint8 Fx_Far_Porta_Dn = 0x66;		// FAR pitch offset down
		public const uint8 Fx_Far_TPorta = 0x67;		// FAR persistent tone portamento
		public const uint8 Fx_Far_Tempo = 0x68;			// FAR coarse tempo and tempo mode
		public const uint8 Fx_Far_F_Tempo = 0x69;		// FAR fine tempo slide up/down
		public const uint8 Fx_Far_VibDepth = 0x6a;		// FAR set vibrato depth
		public const uint8 Fx_Far_Vibrato = 0x6b;		// FAR persistent vibrato
		public const uint8 Fx_Far_SlideVol = 0x6c;		// FAR persistent slide-to-volume
		public const uint8 Fx_Far_Retrig = 0x6d;		// FAR retrigger
		public const uint8 Fx_Far_Delay = 0x6e;			// FAR note offset

		/// <summary>
		/// Other frequency based effects (ULT, etc)
		/// </summary>
		public const uint8 Fx_Ult_TPorta = 0x6f;

		/// <summary>
		/// IT effects
		/// </summary>
		public const uint8 Fx_Trk_Vol = 0x80;
		public const uint8 Fx_Trk_VSlide = 0x81;
		public const uint8 Fx_Trk_FVSlide = 0x82;
		public const uint8 Fx_It_InstFunc = 0x83;
		public const uint8 Fx_Flt_CutOff = 0x84;
		public const uint8 Fx_Flt_Resn = 0x85;
		public const uint8 Fx_It_Bpm = 0x87;
		public const uint8 Fx_It_RowDelay = 0x88;
		public const uint8 Fx_It_PanSlide = 0x89;
		public const uint8 Fx_Panbrello = 0x8a;
		public const uint8 Fx_Panbrello_Wf = 0x8b;
		public const uint8 Fx_HiOffset = 0x8c;
		public const uint8 Fx_It_Break = 0x8e;			// Like Fx_Break with hex parameter
		public const uint8 Fx_Macro_Set = 0xbd;			// Set active IT parametered MIDI macro
		public const uint8 Fx_Macro = 0xbe;				// Execute IT MIDI macro
		public const uint8 Fx_MacroSmooth = 0xbf;		// Execute IT MIDI macro slide

		/// <summary>
		/// MED effects
		/// </summary>
		public const uint8 Fx_Hold_Decay = 0x90;
		public const uint8 Fx_SetPitch = 0x91;
		public const uint8 Fx_Vibrato2 = 0x92;

		/// <summary>
		/// PTM effects
		/// </summary>
		public const uint8 Fx_NSlide_Dn = 0x9c;			// IMF/PTM note slide down
		public const uint8 Fx_NSlide_Up = 0x9d;			// IMF/PTM note slide up
		public const uint8 Fx_NSlide_R_Up = 0x9e;		// PTM note slide down with retrigger
		public const uint8 Fx_NSlide_R_Dn = 0x9f;		// PTM note slide up with retrigger

		/// <summary>
		/// Extra effects
		/// </summary>
		public const uint8 Fx_VolSlide_Up = 0xa0;		// SFX, MDL
		public const uint8 Fx_VolSlide_Dn = 0xa1;
		public const uint8 Fx_F_VSlide = 0xa5;			// IMF/MDL
		public const uint8 Fx_Chorus = 0xa9;			// IMF
		public const uint8 Fx_Ice_Speed = 0xa2;
		public const uint8 Fx_Reverb = 0xaa;			// IMF
		public const uint8 Fx_Med_Hold = 0xb1;			// MMD hold/decay
		public const uint8 Fx_MegaArp = 0xb2;			// Smaksak effect 7: MegaArp
		public const uint8 Fx_Vol_Add = 0xb6;			// SFX change volume up
		public const uint8 Fx_Vol_Sub = 0xb7;			// SFX change volume down
		public const uint8 Fx_Pitch_Add = 0xb8;			// SFX add steps to current note
		public const uint8 Fx_Pitch_Sub = 0xb9;			// SFX add steps to current note
		public const uint8 Fx_Line_Jump = 0xba;			// Archimedes jump to line in current order

		public const uint8 Fx_Surround = 0x8d;			// S3M/IT
		public const uint8 Fx_Reverse = 0x8f;			// XM/IT/others: play forward/reverse
		public const uint8 Fx_S3M_Speed = 0xa3;			// S3M
		public const uint8 Fx_VolSlide_2 = 0xa4;
		public const uint8 Fx_FineTune = 0xa6;
		public const uint8 Fx_S3M_Bpm = 0xab;			// S3M
		public const uint8 Fx_Fine_Vibrato = 0xac;		// S3M/PTM/IMF/LIQ
		public const uint8 Fx_F_VSlide_Up = 0xad;		// MMD
		public const uint8 Fx_F_VSlide_Dn = 0xae;		// MMD
		public const uint8 Fx_F_Porta_Up = 0xaf;		// MMD
		public const uint8 Fx_F_Porta_Dn = 0xb0;		// MMD
		public const uint8 Fx_Patt_Delay = 0xb3;		// MMD
		public const uint8 Fx_S3M_Arpeggio = 0xb4;
		public const uint8 Fx_PanSl_NoMem = 0xb5;		// XM volume column

		public const uint8 Fx_VSlide_Up_2 = 0xc0;		// IT volume column volume slide
		public const uint8 Fx_VSlide_Dn_2 = 0xc1;
		public const uint8 Fx_F_VSlide_Up_2 = 0xc2;
		public const uint8 Fx_F_VSlide_Dn_2 = 0xc3;
	}
}
