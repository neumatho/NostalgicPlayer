/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Resampler_Private_Up2_HQ
	{
		/********************************************************************/
		/// <summary>
		/// Upsample by a factor 2, high quality.
		/// Uses 2nd order allpass filters for the 2x upsampling, followed
		/// by a notch filter just above Nyquist
		/// </summary>
		/********************************************************************/
		public static void Silk_Resampler_Private_Up2_HQ(CPointer<opus_int32> S, CPointer<opus_int16> _out, CPointer<opus_int16> _in, opus_int32 len)
		{
			// Internal variables and state are in Q10 format
			for (opus_int32 k = 0; k < len; k++)
			{
				// Convert to Q10
				opus_int32 in32 = SigProc_Fix.Silk_LSHIFT(_in[k], 10);

				// First all-pass section for even output sample
				opus_int32 Y = SigProc_Fix.Silk_SUB32(in32, S[0]);
				opus_int32 X = Macros.Silk_SMULWB(Y, Resampler_Rom.Silk_Resampler_Up2_Hq_0[0]);
				opus_int32 out32_1 = SigProc_Fix.Silk_ADD32(S[0], X);
				S[0] = SigProc_Fix.Silk_ADD32(in32, X);

				// Second all-pass section for even output sample
				Y = SigProc_Fix.Silk_SUB32(out32_1, S[1]);
				X = Macros.Silk_SMULWB(Y, Resampler_Rom.Silk_Resampler_Up2_Hq_0[1]);
				opus_int32 out32_2 = SigProc_Fix.Silk_ADD32(S[1], X);
				S[1] = SigProc_Fix.Silk_ADD32(out32_1, X);

				// Third all-pass section for even output sample
				Y = SigProc_Fix.Silk_SUB32(out32_2, S[2]);
				X = Macros.Silk_SMLAWB(Y, Y, Resampler_Rom.Silk_Resampler_Up2_Hq_0[2]);
				out32_1 = SigProc_Fix.Silk_ADD32(S[2], X);
				S[2] = SigProc_Fix.Silk_ADD32(out32_2, X);

				// Apply gain in Q15, convert back to int16 and store to output
				_out[2 * k] = (opus_int16)SigProc_Fix.Silk_SAT16(SigProc_Fix.Silk_RSHIFT_ROUND(out32_1, 10));

				// First all-pass section for odd output sample
				Y = SigProc_Fix.Silk_SUB32(in32, S[3]);
				X = Macros.Silk_SMULWB(Y, Resampler_Rom.Silk_Resampler_Up2_Hq_1[0]);
				out32_1 = SigProc_Fix.Silk_ADD32(S[3], X);
				S[3] = SigProc_Fix.Silk_ADD32(in32, X);

				// Second all-pass section for odd output sample
				Y = SigProc_Fix.Silk_SUB32(out32_1, S[4]);
				X = Macros.Silk_SMULWB(Y, Resampler_Rom.Silk_Resampler_Up2_Hq_1[1]);
				out32_2 = SigProc_Fix.Silk_ADD32(S[4], X);
				S[4] = SigProc_Fix.Silk_ADD32(out32_1, X);

				// Third all-pass section for odd output sample
				Y = SigProc_Fix.Silk_SUB32(out32_2, S[5]);
				X = Macros.Silk_SMLAWB(Y, Y, Resampler_Rom.Silk_Resampler_Up2_Hq_1[2]);
				out32_1 = SigProc_Fix.Silk_ADD32(S[5], X);
				S[5] = SigProc_Fix.Silk_ADD32(out32_2, X);

				// Apply gain in Q15, convert back to int16 and store to output
				_out[2 * k + 1] = (opus_int16)SigProc_Fix.Silk_SAT16(SigProc_Fix.Silk_RSHIFT_ROUND(out32_1, 10));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Silk_Resampler_Private_Up2_HQ_Wrapper(Silk_Resampler_State_Struct SS, CPointer<opus_int16> _out, CPointer<opus_int16> _in, opus_int32 len)
		{
			Silk_Resampler_State_Struct S = SS;
			Silk_Resampler_Private_Up2_HQ(S.sIIR, _out, _in, len);
		}
	}
}
