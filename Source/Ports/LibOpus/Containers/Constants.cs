/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// Different constants
	/// </summary>
	internal static class Constants
	{
		public const int Char_Bit = 8;

		// Not defined constants
		public const int Sig_Sat = 0;
		public const int Sig_Shift = 0;

		//
		// Arch
		//
		public const float Celt_Sig_Scale = 32768.0f;

		public const int Db_Shift = 10;

		public const c_float Q15One = 1.0f;

		public const c_float Norm_Scaling = 1.0f;

		public const c_float Epsilon = 1e-15f;
		public const c_float Very_Small = 1e-30f;

		//
		// Celt
		//
		public const int CombFilter_MinPeriod = 15;

		//
		// Celt_lpc
		//
		public const int Celt_Lpc_Order = 24;

		//
		// Define
		//
		// Number of decoder channels (1/2)
		public const int Decoder_Num_Channels = 2;

		public const int Max_Frames_Per_Packet = 3;

		// Maximum sampling frequency
		public const int Max_Fs_KHz = 16;
		public const int Max_Api_Fs_KHz = 48;

		// Settings for stereo processing
		public const int Stereo_Quant_Sub_Steps = 5;
		public const int Stereo_Interp_Len_Ms = 8;		// Must be even

		// Maximum number of subframes
		public const int Max_Nb_Subfr = 4;

		// Number of samples per frame
		public const int Ltp_Mem_Length_Ms = 20;
		public const int Sub_Frame_Length_Ms = 5;
		public const int Max_Sub_Frame_Length = Sub_Frame_Length_Ms * Max_Fs_KHz;
		public const int Max_Frame_Length_Ms = Sub_Frame_Length_Ms * Max_Nb_Subfr;
		public const int Max_Frame_Length = Max_Frame_Length_Ms * Max_Fs_KHz;

		// dB level of lowest gain quantization level
		public const int Min_QGain_dB = 2;

		// dB level of highest gain quantization level
		public const int Max_QGain_dB = 88;

		// Number of gain quantization levels
		public const int N_Levels_QGain = 64;

		// Max increase in gain quantization index
		public const int Max_Delta_Gain_Quant = 36;

		// Max decrease in gain quantization index
		public const int Min_Delta_Gain_Quant = -4;

		public const opus_int16 Offset_Vl_Q10 = 32;
		public const opus_int16 Offset_Vh_Q10 = 100;
		public const opus_int16 Offset_Uvl_Q10 = 100;
		public const opus_int16 Offset_Uvh_Q10 = 240;

		public const int Quant_Level_Adjust_Q10 = 80;

		// Maximum numbers of iterations used to stabilize an LPC vector
		public const int Max_Lpc_Stabilize_Iterations = 16;
		public const float Max_Prediction_Power_Gain = 1e4f;

		public const int Max_Lpc_Order = 16;
		public const int Min_Lpc_Order = 10;

		// Find Pred Coef defines
		public const int Ltp_Order = 5;

		// Number of subframes for excitation entropy coding
		public const int Shell_Codec_Frame_Length = 16;
		public const int Log2_Shell_Codec_Frame_Length = 4;
		public const int Max_Nb_Shell_Blocks = Max_Frame_Length / Shell_Codec_Frame_Length;

		// Number of rate levels, for entropy coding of excitation
		public const int N_Rate_Levels = 10;

		// Maximum sum of pulses per shell coding frame
		public const int Silk_Max_Pulses = 16;

		// NLSF quantizer
		public const int Nlsf_Quant_Max_Amplitude = 4;
		public const float Nlsf_Quant_Level_Adj = 0.1f;

		// BWE factors to apply after packet loss
		public const int Bwe_After_Loss_Q16 = 63570;

		// Defines for CN generation
		public const int Cng_Buf_Mask_Max = 255;				// 2^floor(log2(MAX_FRAME_LENGTH))-1
		public const int Cng_Gain_Smth_Q16 = 4634;				// 0.25^(1/4)
		public const int Cng_Gain_Smth_Threshold_Q16 = 46396;	// -3 dB
		public const int Cng_Nlsf_Smth_Q16 = 16348;				// 0.25

		//
		// Entcode
		//
		public const int Ec_Window_Size = sizeof(ec_window) * Char_Bit;

		// The number of bits to use for the range-coded part of unsigned integers
		public const int Ec_UInt_Bits = 8;

		// The resolution of fractional-precision bit usage measurements, i.e., 3 => 1/8th bits
		public const int BitRes = 3;

		//
		// Kiss_fft
		//
		// E.g. an fft of length 128 has 4 factors
		// as far as kissfft is concerned
		// 4*4*4*2
		public const int MaxFactors = 8;

		//
		// Mfrngcod
		//
		// Constants used by the entropy encoder/decoder

		// The number of bits to output at a time
		public const int Ec_Sym_Bits = 8;

		// The total number of bits in each of the state registers
		public const int Ec_Code_Bits = 32;

		// The maximum symbol value
		public const uint Ec_Sym_Max = (1U << Ec_Sym_Bits) - 1;

		// Bits to shift by to move a symbol into the high-order position
		public const int Ec_Code_Shift = Ec_Code_Bits - Ec_Sym_Bits - 1;

		// Carry bit of the high-order range symbol
		public const uint Ec_Code_Top = 1U << (Ec_Code_Bits - 1);

		// Low-order bit of the high-order range symbol
		public const uint Ec_Code_Bot = Ec_Code_Top >> Ec_Sym_Bits;

		// The number of bits available for the last, partial symbol in the code field
		public const int Ec_Code_Extra = (Ec_Code_Bits - 2) % Ec_Sym_Bits + 1;

		//
		// Modes
		//
		public const int Max_Period = 1024;

		//
		// Pitch_est_defines
		//
		public const int Pe_Max_Nb_Subfr = 4;

		public const int Pe_Max_Lag_Ms = 18;			// 18 ms -> 56 Hz
		public const int Pe_Min_Lag_Ms = 2;				// 2 ms -> 500 Hz

		public const int Pe_Nb_Cbks_Stage2_Ext = 11;

		public const int Pe_Nb_Cbks_Stage3_Max = 34;

		public const int Pe_Nb_Cbks_Stage3_10ms = 12;
		public const int Pe_Nb_Cbks_Stage2_10ms = 3;

		//
		// PLC
		//
		public const float Bwe_Coef = 0.99f;
		public const int V_Pitch_Gain_Start_Min_Q14 = 11469;	// 0.7 in Q14
		public const int V_Pitch_Gain_Start_Max_Q14 = 15565;	// 0.95 in Q14
		public const int Max_Pitch_Lag_Ms = 18;
		public const int Rand_Buf_Size = 128;
		public const int Rand_Buf_Mask = Rand_Buf_Size - 1;
		public const int Log2_Inv_Lpc_Gain_High_Thres = 3;		// 2^3 = 8 dB LPC gain
		public const int Log2_Inv_Lpc_Gain_Low_Thres = 8;		// 2^8 = 24 dB LPC gain
		public const int Pitch_Drift_Fac_Q16 = 655;				// 0.01 in Q16

		//
		// Rate
		//
		public const int Log_Max_Pseudo = 6;

		public const int Max_Fine_Bits = 8;

		public const int Fine_Offset = 21;
		public const int QTheta_Offset = 4;
		public const int QTheta_Offset_TwoPhase = 16;

		//
		// Resampler_rom
		//
		public const int Resampler_Down_Order_Fir0 = 18;
		public const int Resampler_Down_Order_Fir1 = 24;
		public const int Resampler_Down_Order_Fir2 = 36;

		public const int Resampler_Order_Fir_12 = 8;

		//
		// Resampler_structs
		//
		public const int Silk_Resampler_Max_Fir_Order = 36;
		public const int Silk_Resampler_Max_Iir_Order = 6;

		//
		// SigProc_FIX
		//
		// Max order of the LPC analysis in schur() and k2a()
		public const int Silk_Max_Order_Lpc = 24;

		//
		// Stack_alloc
		//
		public const int Alloc_None = 0;

		//
		// Static_modes_float
		//
		public const int Total_Modes = 1;
	}
}
