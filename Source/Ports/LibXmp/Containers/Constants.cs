/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers
{
	/// <summary>
	/// Different constants
	/// </summary>
	internal static class Constants
	{
		public const c_int EOF = -1;

		// Common
		public const c_double Pal_Rate = 250.0;			// 1 / (50Hz * 80us)
		public const c_double Ntsc_Rate = 208.0;		// 1 / (60Hz * 80us)
		public const c_int C4_Pal_Rate = 8287;			// 7093789.2 / period (C4) * 2
		public const c_int C4_Ntsc_Rate = 8363;			// 7159090.5 / period (C4) * 2

		public const c_int Default_Amplify = 1;
		public const c_int Default_Mix = 100;

		public const c_double Default_Time_Factor = 10.0;
		public const c_double Far_Time_Factor = 4.01373;// See Far_Extras

		public const c_int Max_Sequences = 255;
		public const c_int Max_Sample_Size = 0x10000000;
		public const c_int Max_Samples = 1024;
		public const c_int Max_Instruments = 255;

		// Mixer
		public const c_double C4_Period = 428.0;

		public const c_int SMix_NumVoc = 128;			// Default number of softmixer voices
		public const c_int SMix_Shift = 16;
		public const c_int SMix_Mask = 0xffff;

		public const c_int Filter_Shift = 22;
		public const c_int AntiClick_Shift = 3;

		public const c_int Pan_Surround = 0x8000;

		// Period
		public const c_double Period_Base = 13696.0;	// C0 period
		public const c_double Min_Period_L = 0x0000;
		public const c_double Max_Period_L = 0x1e00;
		public const c_int Min_Note_Mod = 48;
		public const c_int Max_Note_Mod = 83;

		// Xmp
		public const c_int Xmp_Name_Size = 64;			// Size of module name and type

		public const uint8 Xmp_Key_Off = 0x81;			// Note number for key off event
		public const uint8 Xmp_Key_Cut = 0x82;			// Note number for key cut event
		public const uint8 Xmp_Key_Fade = 0x83;			// Note number for fade event

		public const c_int Xmp_Max_Keys = 121;			// Number of valid keys
		public const c_int Xmp_Max_Env_Points = 32;		// Max number of envelope points
		public const c_int Xmp_Max_Mod_Length = 256;	// Max number of patterns in module
		public const c_int Xmp_Max_Channels = 64;		// Max number of channels in module
		public const c_int Xmp_Max_SRate = 49170;		// Max sampling rate (Hz)
		public const c_int Xmp_Min_SRate = 4000;		// Min sampling rate (Hz)
		public const c_int Xmp_Min_Bpm = 20;			// Min BPM

		// frame rate = (50 * bpm / 125) Hz
		// frame size = (sampling rate * channels * size) / frame rate
		public const c_int Xmp_Max_FrameSize = 5 * Xmp_Max_SRate * 2 / Xmp_Min_Bpm;
	}
}
