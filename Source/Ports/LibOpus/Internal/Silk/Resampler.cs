/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Resampler
	{
		// Matrix of resampling methods used:
		//                                 Fs_out (kHz)
		//                        8      12     16     24     48
		//
		//               8        C      UF     U      UF     UF
		//              12        AF     C      UF     U      UF
		// Fs_in (kHz)  16        D      AF     C      UF     UF
		//              24        AF     D      AF     C      U
		//              48        AF     AF     AF     D      C
		//
		// C   -> Copy (no resampling)
		// D   -> Allpass-based 2x downsampling
		// U   -> Allpass-based 2x upsampling
		// UF  -> Allpass-based 2x upsampling followed by FIR interpolation
		// AF  -> AR2 filter followed by FIR interpolation

		// Tables with delay compensation values to equalize total delay for different modes
		private static readonly opus_int8[][] delay_matrix_enc =
		[
			// in \ out 8  12  16
			/*  8 */ [  6,  0,  3 ],
			/* 12 */ [  0,  7,  3 ],
			/* 16 */ [  0,  1, 10 ],
			/* 24 */ [  0,  2,  6 ],
			/* 48 */ [ 18, 10, 12 ]
		];

		private static readonly opus_int8[][] delay_matrix_dec =
		[
			// in \ out 8  12  16  24  48
			/*  8 */ [  4,  0,  2,  0,  0 ],
			/* 12 */ [  0,  9,  4,  7,  4 ],
			/* 16 */ [  0,  3, 12,  7,  7 ]
		];

		// Number of input samples to process in the inner loop
		private const int Resampler_Max_Batch_Size_Ms = 10;

		/********************************************************************/
		/// <summary>
		/// Simple way to make [8000, 12000, 16000, 24000, 48000] to
		/// [0, 1, 2, 3, 4]
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static opus_int32 RateID(opus_int32 R)
		{
			return (((R >> 12) - (R > 16000 ? 1 : 0)) >> (R > 24000 ? 1 : 0)) - 1;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize/reset the resampler state for a given pair of
		/// input/output sampling rates
		/// </summary>
		/********************************************************************/
		public static SilkError Silk_Resampler_Init(Silk_Resampler_State_Struct S, opus_int32 Fs_Hz_in, opus_int32 Fs_Hz_out, bool forEnc)
		{
			// Clear state
			S.Clear();

			// Input checking
			if (forEnc)
			{
				if (((Fs_Hz_in != 8000) && (Fs_Hz_in != 12000) && (Fs_Hz_in != 16000) && (Fs_Hz_in != 24000) && (Fs_Hz_in != 48000)) ||
				    ((Fs_Hz_out != 8000) && (Fs_Hz_out != 12000) && (Fs_Hz_out != 16000)))
				{
					return SilkError.Error;
				}

				S.inputDelay = delay_matrix_enc[RateID(Fs_Hz_in)][RateID(Fs_Hz_out)];
			}
			else
			{
				if (((Fs_Hz_in != 8000) && (Fs_Hz_in != 12000) && (Fs_Hz_in != 16000)) ||
				    ((Fs_Hz_out != 8000) && (Fs_Hz_out != 12000) && (Fs_Hz_out != 16000) && (Fs_Hz_out != 24000) && (Fs_Hz_out != 48000)))
				{
					return SilkError.Error;
				}

				S.inputDelay = delay_matrix_dec[RateID(Fs_Hz_in)][RateID(Fs_Hz_out)];
			}

			S.Fs_in_kHz = SigProc_Fix.Silk_DIV32_16(Fs_Hz_in, 1000);
			S.Fs_out_kHz = SigProc_Fix.Silk_DIV32_16(Fs_Hz_out, 1000);

			// Number of samples processed per batch
			S.batchSize = S.Fs_in_kHz * Resampler_Max_Batch_Size_Ms;

			// Find resampler with the right sampling ratio
			opus_int up2x = 0;

			if (Fs_Hz_out > Fs_Hz_in)
			{
				// Upsample
				if (Fs_Hz_out == SigProc_Fix.Silk_MUL(Fs_Hz_in, 2))
				{
					// Fs_out : Fs_in = 2 : 1
					// Special case: directly use 2x upsampler
					S.resampler_function = ResamplerType.Private_Up2_HQ_Wrapper;
				}
				else
				{
					// Default resampler
					S.resampler_function = ResamplerType.Private_IIR_FIR;
					up2x = 1;
				}
			}
			else if (Fs_Hz_out < Fs_Hz_in)
			{
				// Downsample
				S.resampler_function = ResamplerType.Private_Down_FIR;

				if (SigProc_Fix.Silk_MUL(Fs_Hz_out, 4) == SigProc_Fix.Silk_MUL(Fs_Hz_in, 3))
				{
					// Fs_out : Fs_in = 3 : 4
					S.FIR_Fracs = 3;
					S.FIR_Order = Constants.Resampler_Down_Order_Fir0;
					S.Coefs = Resampler_Rom.Silk_Resampler_3_4_COEFS;
				}
				else if (SigProc_Fix.Silk_MUL(Fs_Hz_out, 3) == SigProc_Fix.Silk_MUL(Fs_Hz_in, 2))
				{
					// Fs_out : Fs_in = 2 : 3
					S.FIR_Fracs = 2;
					S.FIR_Order = Constants.Resampler_Down_Order_Fir0;
					S.Coefs = Resampler_Rom.Silk_Resampler_2_3_COEFS;
				}
				else if (SigProc_Fix.Silk_MUL(Fs_Hz_out, 2) == Fs_Hz_in)
				{
					// Fs_out : Fs_in = 1 : 2
					S.FIR_Fracs = 1;
					S.FIR_Order = Constants.Resampler_Down_Order_Fir1;
					S.Coefs = Resampler_Rom.Silk_Resampler_1_2_COEFS;
				}
				else if (SigProc_Fix.Silk_MUL(Fs_Hz_out, 3) == Fs_Hz_in)
				{
					// Fs_out : Fs_in = 1 : 3
					S.FIR_Fracs = 1;
					S.FIR_Order = Constants.Resampler_Down_Order_Fir2;
					S.Coefs = Resampler_Rom.Silk_Resampler_1_3_COEFS;
				}
				else if (SigProc_Fix.Silk_MUL(Fs_Hz_out, 4) == Fs_Hz_in)
				{
					// Fs_out : Fs_in = 1 : 4
					S.FIR_Fracs = 1;
					S.FIR_Order = Constants.Resampler_Down_Order_Fir2;
					S.Coefs = Resampler_Rom.Silk_Resampler_1_4_COEFS;
				}
				else if (SigProc_Fix.Silk_MUL(Fs_Hz_out, 6) == Fs_Hz_in)
				{
					// Fs_out : Fs_in = 1 : 6
					S.FIR_Fracs = 1;
					S.FIR_Order = Constants.Resampler_Down_Order_Fir2;
					S.Coefs = Resampler_Rom.Silk_Resampler_1_6_COEFS;
				}
				else
				{
					// None available
					return SilkError.Error;
				}
			}
			else
			{
				// Input and output sampling rates are equal: copy
				S.resampler_function = ResamplerType.Copy;
			}

			// Ratio of input/output samples
			S.invRatio_Q16 = SigProc_Fix.Silk_LSHIFT32(SigProc_Fix.Silk_DIV32(SigProc_Fix.Silk_LSHIFT32(Fs_Hz_in, 14 + up2x), Fs_Hz_out), 2);

			// Make sure the ratio is rounded up
			while (Macros.Silk_SMULWW(S.invRatio_Q16, Fs_Hz_out) < SigProc_Fix.Silk_LSHIFT32(Fs_Hz_in, up2x))
			{
				S.invRatio_Q16++;
			}

			return SilkError.No_Error;
		}



		/********************************************************************/
		/// <summary>
		/// Convert from one sampling rate to another.
		/// Input and output sampling rate are at most 48000 Hz
		/// </summary>
		/********************************************************************/
		public static SilkError Silk_Resampler(Silk_Resampler_State_Struct S, CPointer<opus_int16> _out, CPointer<opus_int16> _in, opus_int32 inLen)
		{
			opus_int nSamples = S.Fs_in_kHz - S.inputDelay;

			// Copy to delay buffer
			SigProc_Fix.Silk_MemCpy(S.delayBuf + S.inputDelay, _in, nSamples);

			switch (S.resampler_function)
			{
				case ResamplerType.Private_Up2_HQ_Wrapper:
				{
					Resampler_Private_Up2_HQ.Silk_Resampler_Private_Up2_HQ_Wrapper(S, _out, S.delayBuf, S.Fs_in_kHz);
					Resampler_Private_Up2_HQ.Silk_Resampler_Private_Up2_HQ_Wrapper(S, _out + S.Fs_out_kHz, _in + nSamples, inLen - S.Fs_in_kHz);
					break;
				}

				case ResamplerType.Private_IIR_FIR:
				{
					Resampler_Private_IIR_FIR.Silk_Resampler_Private_IIR_FIR(S, _out, S.delayBuf, S.Fs_in_kHz);
					Resampler_Private_IIR_FIR.Silk_Resampler_Private_IIR_FIR(S, _out + S.Fs_out_kHz, _in + nSamples, inLen - S.Fs_in_kHz);
					break;
				}

				case ResamplerType.Private_Down_FIR:
				{
					Resampler_Private_Down_FIR.Silk_Resampler_Private_Down_FIR(S, _out, S.delayBuf, S.Fs_in_kHz);
					Resampler_Private_Down_FIR.Silk_Resampler_Private_Down_FIR(S, _out + S.Fs_out_kHz, _in + nSamples, inLen - S.Fs_in_kHz);
					break;
				}

				default:
				{
					SigProc_Fix.Silk_MemCpy(_out, S.delayBuf, S.Fs_in_kHz);
					SigProc_Fix.Silk_MemCpy(_out + S.Fs_out_kHz, _in + nSamples, inLen - S.Fs_in_kHz);
					break;
				}
			}

			// Copy to delay buffer
			SigProc_Fix.Silk_MemCpy(S.delayBuf, _in + inLen - S.inputDelay, S.inputDelay);

			return SilkError.No_Error;
		}
	}
}
