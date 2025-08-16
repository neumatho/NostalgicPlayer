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
	internal static class Stereo_MS_To_LR
	{
		/********************************************************************/
		/// <summary>
		/// Convert adaptive Mid/Side representation to Left/Right stereo
		/// signal
		/// </summary>
		/********************************************************************/
		public static void Silk_Stereo_MS_To_LR(Stereo_Dec_State state, CPointer<opus_int16> x1, CPointer<opus_int16> x2, CPointer<opus_int32> pred_Q13, opus_int fs_kHz, opus_int frame_length)
		{
			// Buffering
			SigProc_Fix.Silk_MemCpy(x1, state.sMid, 2);
			SigProc_Fix.Silk_MemCpy(x2, state.sSide, 2);
			SigProc_Fix.Silk_MemCpy(state.sMid, x1 + frame_length, 2);
			SigProc_Fix.Silk_MemCpy(state.sSide, x2 + frame_length, 2);

			// Interpolate predictors and add prediction to side channel
			opus_int32 pred0_Q13 = state.pred_prev_Q13[0];
			opus_int32 pred1_Q13 = state.pred_prev_Q13[1];
			opus_int denom_Q16 = SigProc_Fix.Silk_DIV32_16(1 << 16, (opus_int16)(Constants.Stereo_Interp_Len_Ms * fs_kHz));
			opus_int delta0_Q13 = SigProc_Fix.Silk_RSHIFT_ROUND(Macros.Silk_SMULBB(pred_Q13[0] - state.pred_prev_Q13[0], denom_Q16), 16);
			opus_int delta1_Q13 = SigProc_Fix.Silk_RSHIFT_ROUND(Macros.Silk_SMULBB(pred_Q13[1] - state.pred_prev_Q13[1], denom_Q16), 16);

			for (opus_int n = 0; n < (Constants.Stereo_Interp_Len_Ms * fs_kHz); n++)
			{
				pred0_Q13 += delta0_Q13;
				pred1_Q13 += delta1_Q13;

				opus_int32 sum = SigProc_Fix.Silk_LSHIFT(SigProc_Fix.Silk_ADD_LSHIFT32(x1[n] + x1[n + 2], x1[n + 1], 1), 9);	// Q11
				sum = Macros.Silk_SMLAWB(SigProc_Fix.Silk_LSHIFT(x2[n + 1], 8), sum, pred0_Q13);		// Q8
				sum = Macros.Silk_SMLAWB(sum, SigProc_Fix.Silk_LSHIFT(x1[n + 1], 11), pred1_Q13);   // Q8
				x2[n + 1] = (opus_int16)SigProc_Fix.Silk_SAT16(SigProc_Fix.Silk_RSHIFT_ROUND(sum, 8));
			}

			state.pred_prev_Q13[0] = (opus_int16)pred_Q13[0];
			state.pred_prev_Q13[1] = (opus_int16)pred_Q13[1];

			// Convert to left/right signals
			for (opus_int n = 0; n < frame_length; n++)
			{
				opus_int32 sum = x1[n + 1] + x2[n + 1];
				opus_int32 diff = x1[n + 1] - x2[n + 1];

				x1[n + 1] = (opus_int16)SigProc_Fix.Silk_SAT16(sum);
				x2[n + 1] = (opus_int16)SigProc_Fix.Silk_SAT16(diff);
			}
		}
	}
}
