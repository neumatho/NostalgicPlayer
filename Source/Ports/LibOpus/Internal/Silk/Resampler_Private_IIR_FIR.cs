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
	internal static class Resampler_Private_IIR_FIR
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static CPointer<opus_int16> Silk_Resampler_Private_IIR_FIR_INTERPOL(CPointer<opus_int16> _out, CPointer<opus_int16> buf, opus_int32 max_index_Q16, opus_int32 index_increment_Q16)
		{
			// Interpolate upsampled signal and store in output array
			for (opus_int32 index_Q16 = 0; index_Q16 < max_index_Q16; index_Q16 += index_increment_Q16)
			{
				opus_int32 table_index = Macros.Silk_SMULWB(index_Q16 & 0xffff, 12);
				CPointer<opus_int16> buf_ptr = buf + (index_Q16 >> 16);

				opus_int32 res_Q15 = Macros.Silk_SMULBB(buf_ptr[0], Resampler_Rom.Silk_Resampler_Frac_FIR_12[table_index][0]);
				res_Q15 = Macros.Silk_SMLABB(res_Q15, buf_ptr[1], Resampler_Rom.Silk_Resampler_Frac_FIR_12[table_index][1]);
				res_Q15 = Macros.Silk_SMLABB(res_Q15, buf_ptr[2], Resampler_Rom.Silk_Resampler_Frac_FIR_12[table_index][2]);
				res_Q15 = Macros.Silk_SMLABB(res_Q15, buf_ptr[3], Resampler_Rom.Silk_Resampler_Frac_FIR_12[table_index][3]);
				res_Q15 = Macros.Silk_SMLABB(res_Q15, buf_ptr[4], Resampler_Rom.Silk_Resampler_Frac_FIR_12[11 - table_index][3]);
				res_Q15 = Macros.Silk_SMLABB(res_Q15, buf_ptr[5], Resampler_Rom.Silk_Resampler_Frac_FIR_12[11 - table_index][2]);
				res_Q15 = Macros.Silk_SMLABB(res_Q15, buf_ptr[6], Resampler_Rom.Silk_Resampler_Frac_FIR_12[11 - table_index][1]);
				res_Q15 = Macros.Silk_SMLABB(res_Q15, buf_ptr[7], Resampler_Rom.Silk_Resampler_Frac_FIR_12[11 - table_index][0]);

				_out[0, 1] = (opus_int16)SigProc_Fix.Silk_SAT16(SigProc_Fix.Silk_RSHIFT_ROUND(res_Q15, 15));
			}

			return _out;
		}



		/********************************************************************/
		/// <summary>
		/// Upsample using a combination of allpass-based 2x upsampling and
		/// FIR interpolation
		/// </summary>
		/********************************************************************/
		public static void Silk_Resampler_Private_IIR_FIR(Silk_Resampler_State_Struct SS, CPointer<opus_int16> _out, CPointer<opus_int16> _in, opus_int32 inLen)
		{
			Silk_Resampler_State_Struct S = SS;

			CPointer<opus_int16> buf = new CPointer<opus_int16>(2 * S.batchSize + Constants.Resampler_Order_Fir_12);

			// Copy buffered samples to start of buffer
			SigProc_Fix.Silk_MemCpy_Span(buf.AsSpan(), S.sFIR.i16, Constants.Resampler_Order_Fir_12);

			// Iterate over blocks of frameSizeIn input samples
			opus_int32 index_increment_Q16 = S.invRatio_Q16;
			opus_int32 nSamplesIn;

			while (true)
			{
				nSamplesIn = SigProc_Fix.Silk_Min(inLen, S.batchSize);

				// Upsample 2x
				Resampler_Private_Up2_HQ.Silk_Resampler_Private_Up2_HQ(S.sIIR, buf + Constants.Resampler_Order_Fir_12, _in, nSamplesIn);

				opus_int32 max_index_Q16 = SigProc_Fix.Silk_LSHIFT32(nSamplesIn, 16 + 1);		// + 1 because 2x upsampling
				_out = Silk_Resampler_Private_IIR_FIR_INTERPOL(_out, buf, max_index_Q16, index_increment_Q16);

				_in += nSamplesIn;
				inLen -= nSamplesIn;

				if (inLen > 0)
				{
					// More iterations to do; copy last part of filtered signal to beginning of buffer
					SigProc_Fix.Silk_MemCpy(buf, buf + (nSamplesIn << 1), Constants.Resampler_Order_Fir_12);
				}
				else
					break;
			}

			// Copy last part of filtered signal to the state for the next call
			SigProc_Fix.Silk_MemCpy_Span(S.sFIR.i16, (buf + (nSamplesIn << 1)).AsSpan(), Constants.Resampler_Order_Fir_12);
		}
	}
}
