/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class NLSF_Decode
	{
		/********************************************************************/
		/// <summary>
		/// Predictive dequantizer for NLSF residuals
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Silk_NLSF_Residual_Dequant(CPointer<opus_int16> x_Q10, CPointer<opus_int8> indices, CPointer<opus_uint8> pred_coef_Q8, opus_int quant_step_size_Q16, opus_int16 order)
		{
			opus_int out_Q10 = 0;

			for (opus_int i = order - 1; i >= 0; i--)
			{
				opus_int pred_Q10 = SigProc_Fix.Silk_RSHIFT(Macros.Silk_SMULBB(out_Q10, pred_coef_Q8[i]), 8);
				out_Q10 = SigProc_Fix.Silk_LSHIFT(indices[i], 10);

				if (out_Q10 > 0)
					out_Q10 = SigProc_Fix.Silk_SUB16(out_Q10, SigProc_Fix.Silk_FIX_CONST(Constants.Nlsf_Quant_Level_Adj, 10));
				else if (out_Q10 < 0)
					out_Q10 = SigProc_Fix.Silk_ADD16(out_Q10, SigProc_Fix.Silk_FIX_CONST(Constants.Nlsf_Quant_Level_Adj, 10));

				out_Q10 = Macros.Silk_SMLAWB(pred_Q10, out_Q10, quant_step_size_Q16);
				x_Q10[i] = (opus_int16)out_Q10;
			}
		}



		/********************************************************************/
		/// <summary>
		/// NLSF vector decoder
		/// </summary>
		/********************************************************************/
		public static void Silk_NLSF_Decode(CPointer<opus_int16> pNLSF_Q15, CPointer<opus_int8> NLSFIndicies, Silk_NLSF_CB_Struct psNLSF_CB)
		{
			opus_uint8[] pred_Q8 = new opus_uint8[Constants.Max_Lpc_Order];
			opus_int16[] ec_ix = new opus_int16[Constants.Max_Lpc_Order];
			opus_int16[] res_Q10 = new opus_int16[Constants.Max_Lpc_Order];

			// Unpack entropy table indices and predictor for current CB1 index
			NLSF_Unpack.Silk_NLSF_Unpack(ec_ix, pred_Q8, psNLSF_CB, NLSFIndicies[0]);

			// Predictive residual dequantizer
			Silk_NLSF_Residual_Dequant(res_Q10, NLSFIndicies + 1, pred_Q8, psNLSF_CB.quantStepSize_Q16, psNLSF_CB.order);

			// Apply inverse square-rooted weights to first stage and add to output
			CPointer<opus_uint8> pCB_element = psNLSF_CB.CB1_NLSF_Q8 + NLSFIndicies[0] * psNLSF_CB.order;
			CPointer<opus_int16> pCB_Wght_Q9 = psNLSF_CB.CB1_Wght_Q9 + NLSFIndicies[0] * psNLSF_CB.order;

			for (opus_int i = 0; i < psNLSF_CB.order; i++)
			{
				opus_int32 NLSF_Q15_tmp = SigProc_Fix.Silk_ADD_LSHIFT32(SigProc_Fix.Silk_DIV32_16(SigProc_Fix.Silk_LSHIFT(res_Q10[i], 14), pCB_Wght_Q9[i]), pCB_element[i], 7);
				pNLSF_Q15[i] = (opus_int16)SigProc_Fix.Silk_LIMIT(NLSF_Q15_tmp, 0, 32767);
			}

			// NLSF stabilization
			NLSF_Stabilize.Silk_NLSF_Stabilize(pNLSF_Q15, psNLSF_CB.deltaMin_Q15, psNLSF_CB.order);
		}
	}
}
