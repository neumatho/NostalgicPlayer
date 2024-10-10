/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Resampler_Private_Down_FIR
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Pointer<opus_int16> Silk_Resampler_Private_Down_FIR_INTERPOL(Pointer<opus_int16> _out, Pointer<opus_int32> buf, Pointer<opus_int16> FIR_Coefs, opus_int FIR_Order, opus_int FIR_Fracs, opus_int32 max_index_Q16, opus_int32 index_increment_Q16)
		{
			switch (FIR_Order)
			{
				case Constants.Resampler_Down_Order_Fir0:
				{
					for (opus_int32 index_Q16 = 0; index_Q16 < max_index_Q16; index_Q16 += index_increment_Q16)
					{
						// Integer part gives pointer to buffered output
						Pointer<opus_int32> buf_ptr = buf + SigProc_Fix.Silk_RSHIFT(index_Q16, 16);

						// Fractional part gives interpolation coefficients
						opus_int32 interpol_ind = Macros.Silk_SMULWB(index_Q16 & 0xffff, FIR_Fracs);

						// Inner product
						Pointer<opus_int16> interpol_ptr = FIR_Coefs + Constants.Resampler_Down_Order_Fir0 / 2 * interpol_ind;

						opus_int32 res_Q6 = Macros.Silk_SMULWB(buf_ptr[0], interpol_ptr[0]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, buf_ptr[1], interpol_ptr[1]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, buf_ptr[2], interpol_ptr[2]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, buf_ptr[3], interpol_ptr[3]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, buf_ptr[4], interpol_ptr[4]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, buf_ptr[5], interpol_ptr[5]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, buf_ptr[6], interpol_ptr[6]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, buf_ptr[7], interpol_ptr[7]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, buf_ptr[8], interpol_ptr[8]);

						interpol_ptr = FIR_Coefs + Constants.Resampler_Down_Order_Fir0 / 2 * (FIR_Fracs - 1 - interpol_ind);

						res_Q6 = Macros.Silk_SMLAWB(res_Q6, buf_ptr[17], interpol_ptr[0]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, buf_ptr[16], interpol_ptr[1]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, buf_ptr[15], interpol_ptr[2]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, buf_ptr[14], interpol_ptr[3]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, buf_ptr[13], interpol_ptr[4]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, buf_ptr[12], interpol_ptr[5]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, buf_ptr[11], interpol_ptr[6]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, buf_ptr[10], interpol_ptr[7]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, buf_ptr[9], interpol_ptr[8]);

						// Scale down, saturate and store in output array
						_out[0, 1] = (opus_int16)SigProc_Fix.Silk_SAT16(SigProc_Fix.Silk_RSHIFT_ROUND(res_Q6, 6));
					}
					break;
				}

				case Constants.Resampler_Down_Order_Fir1:
				{
					for (opus_int32 index_Q16 = 0; index_Q16 < max_index_Q16; index_Q16 += index_increment_Q16)
					{
						// Integer part gives pointer to buffered output
						Pointer<opus_int32> buf_ptr = buf + SigProc_Fix.Silk_RSHIFT(index_Q16, 16);

						// Inner product
						opus_int32 res_Q6 = Macros.Silk_SMULWB(SigProc_Fix.Silk_ADD32(buf_ptr[0], buf_ptr[23]), FIR_Coefs[0]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[1], buf_ptr[22]), FIR_Coefs[1]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[2], buf_ptr[21]), FIR_Coefs[2]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[3], buf_ptr[20]), FIR_Coefs[3]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[4], buf_ptr[19]), FIR_Coefs[4]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[5], buf_ptr[18]), FIR_Coefs[5]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[6], buf_ptr[17]), FIR_Coefs[6]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[7], buf_ptr[16]), FIR_Coefs[7]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[8], buf_ptr[15]), FIR_Coefs[8]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[9], buf_ptr[14]), FIR_Coefs[9]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[10], buf_ptr[13]), FIR_Coefs[10]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[11], buf_ptr[12]), FIR_Coefs[11]);

						// Scale down, saturate and store in output array
						_out[0, 1] = (opus_int16)SigProc_Fix.Silk_SAT16(SigProc_Fix.Silk_RSHIFT_ROUND(res_Q6, 6));
					}
					break;
				}

				case Constants.Resampler_Down_Order_Fir2:
				{
					for (opus_int32 index_Q16 = 0; index_Q16 < max_index_Q16; index_Q16 += index_increment_Q16)
					{
						// Integer part gives pointer to buffered output
						Pointer<opus_int32> buf_ptr = buf + SigProc_Fix.Silk_RSHIFT(index_Q16, 16);

						// Inner product
						opus_int32 res_Q6 = Macros.Silk_SMULWB(SigProc_Fix.Silk_ADD32(buf_ptr[0], buf_ptr[35]), FIR_Coefs[0]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[1], buf_ptr[34]), FIR_Coefs[1]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[2], buf_ptr[33]), FIR_Coefs[2]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[3], buf_ptr[32]), FIR_Coefs[3]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[4], buf_ptr[31]), FIR_Coefs[4]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[5], buf_ptr[30]), FIR_Coefs[5]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[6], buf_ptr[29]), FIR_Coefs[6]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[7], buf_ptr[28]), FIR_Coefs[7]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[8], buf_ptr[27]), FIR_Coefs[8]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[9], buf_ptr[26]), FIR_Coefs[9]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[10], buf_ptr[25]), FIR_Coefs[10]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[11], buf_ptr[24]), FIR_Coefs[11]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[12], buf_ptr[23]), FIR_Coefs[12]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[13], buf_ptr[22]), FIR_Coefs[13]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[14], buf_ptr[21]), FIR_Coefs[14]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[15], buf_ptr[20]), FIR_Coefs[15]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[16], buf_ptr[19]), FIR_Coefs[16]);
						res_Q6 = Macros.Silk_SMLAWB(res_Q6, SigProc_Fix.Silk_ADD32(buf_ptr[17], buf_ptr[18]), FIR_Coefs[17]);

						// Scale down, saturate and store in output array
						_out[0, 1] = (opus_int16)SigProc_Fix.Silk_SAT16(SigProc_Fix.Silk_RSHIFT_ROUND(res_Q6, 6));
					}
					break;
				}
			}

			return _out;
		}



		/********************************************************************/
		/// <summary>
		/// Resample with a 2nd order AR filter followed by FIR interpolation
		/// </summary>
		/********************************************************************/
		public static void Silk_Resampler_Private_Down_FIR(Silk_Resampler_State_Struct SS, Pointer<opus_int16> _out, Pointer<opus_int16> _in, opus_int32 inLen)
		{
			Silk_Resampler_State_Struct S = SS;

			Pointer<opus_int32> buf = new Pointer<opus_int32>(S.batchSize + S.FIR_Order);

			// Copy buffered samples to start of buffer
			SigProc_Fix.Silk_MemCpy_Span(buf.AsSpan(), S.sFIR.i32, S.FIR_Order);

			Pointer<opus_int16> FIR_Coefs = S.Coefs + 2;

			// Iterate over blocks of frameSizeIn input samples
			opus_int32 index_increment_Q16 = S.invRatio_Q16;
			opus_int32 nSamplesIn;

			while (true)
			{
				nSamplesIn = SigProc_Fix.Silk_Min(inLen, S.batchSize);

				// Second-order AR filter (output in Q8)
				Resampler_Private_AR2.Silk_Resampler_Private_AR2(S.sIIR, buf + S.FIR_Order, _in, S.Coefs, nSamplesIn);

				opus_int32 max_index_Q16 = SigProc_Fix.Silk_LSHIFT32(nSamplesIn, 16);

				// Interpolate filtered signal
				_out = Silk_Resampler_Private_Down_FIR_INTERPOL(_out, buf, FIR_Coefs, S.FIR_Order, S.FIR_Fracs, max_index_Q16, index_increment_Q16);

				_in += nSamplesIn;
				inLen -= nSamplesIn;

				if (inLen > 1)
				{
					// More iterations to do; copy last part of filtered signal to beginning of buffer
					SigProc_Fix.Silk_MemCpy(buf, buf + nSamplesIn, S.FIR_Order);
				}
				else
					break;
			}

			// Copy last part of filtered signal to the state for the next call
			SigProc_Fix.Silk_MemCpy_Span(S.sFIR.i32, (buf + nSamplesIn).AsSpan(), S.FIR_Order);
		}
	}
}
