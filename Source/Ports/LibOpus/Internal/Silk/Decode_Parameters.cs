/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Decode_Parameters
	{
		/********************************************************************/
		/// <summary>
		/// Decode parameters from payload
		/// </summary>
		/********************************************************************/
		public static void Silk_Decode_Parameters(Silk_Decoder_State psDec, Silk_Decoder_Control psDecCtrl, CodeType condCoding)
		{
			opus_int16[] pNLSF_Q15 = new opus_int16[Constants.Max_Lpc_Order];
			opus_int16[] pNLSF0_Q15 = new opus_int16[Constants.Max_Lpc_Order];

			// Dequant Gains
			Gain_Quant.Silk_Gains_Dequant(psDecCtrl.Gains_Q16, psDec.indices.GainsIndicies, ref psDec.LastGainIndex, condCoding == CodeType.Conditionally, psDec.nb_subfr);

			/****************/
			/* Decode NLSFs */
			/****************/
			NLSF_Decode.Silk_NLSF_Decode(pNLSF_Q15, psDec.indices.NLSFIndices, psDec.psNLSF_CB);

			// Convert NLSF parameters to AR prediction filter coefficients
			NLSF2A.Silk_NLSF2A(psDecCtrl.PredCoef_Q12[1], pNLSF_Q15, psDec.LPC_Order, psDec.arch);

			// If just reset, e.g., because internal Fs changed, do not allow interpolation
			// improves the case of packet loss in the first frame after a switch
			if (psDec.first_frame_after_reset == true)
				psDec.indices.NLSFInterpCoef_Q2 = 4;

			if (psDec.indices.NLSFInterpCoef_Q2 < 4)
			{
				// Calculation of the interpolated NLSF0 vector from the interpolation factor,
				// the previous NLSF1, and the current NLSF1
				for (opus_int i = 0; i < psDec.LPC_Order; i++)
					pNLSF_Q15[i] = (opus_int16)(psDec.prevNLSF_Q15[i] + SigProc_Fix.Silk_RSHIFT(SigProc_Fix.Silk_MUL(psDec.indices.NLSFInterpCoef_Q2, pNLSF_Q15[i] - psDec.prevNLSF_Q15[i]), 2));

				// Convert NLSF parameters to AR prediction filter coefficients
				NLSF2A.Silk_NLSF2A(psDecCtrl.PredCoef_Q12[0], pNLSF0_Q15, psDec.LPC_Order, psDec.arch);
			}
			else
			{
				// Copy LPC coefficients for first half from second half
				SigProc_Fix.Silk_MemCpy<opus_int16>(psDecCtrl.PredCoef_Q12[0], psDecCtrl.PredCoef_Q12[1], psDec.LPC_Order);
			}

			SigProc_Fix.Silk_MemCpy<opus_int16>(psDec.prevNLSF_Q15, pNLSF_Q15, psDec.LPC_Order);

			// After a packet loss do BWE of LPC coefs
			if (psDec.lossCnt != 0)
			{
				Bwexpander.Silk_Bwexpander(psDecCtrl.PredCoef_Q12[0], psDec.LPC_Order, Constants.Bwe_After_Loss_Q16);
				Bwexpander.Silk_Bwexpander(psDecCtrl.PredCoef_Q12[1], psDec.LPC_Order, Constants.Bwe_After_Loss_Q16);
			}

			if (psDec.indices.signalType == SignalType.Voiced)
			{
				/*********************/
				/* Decode pitch lags */
				/*********************/

				// Decode pitch values
				Decode_Pitch.Silk_Decode_Pitch(psDec.indices.lagIndex, psDec.indices.contourIndex, psDecCtrl.pitchL, psDec.fs_kHz, psDec.nb_subfr);

				// Decode Codebook Index
				CPointer<opus_int8> cbk_ptr_Q7 = Tables_LTP.Silk_LTP_Vq_Ptrs_Q7[psDec.indices.PERIndex];	// Set pointer to start of codebook
				opus_int Ix;

				for (opus_int k = 0; k < psDec.nb_subfr; k++)
				{
					Ix = psDec.indices.LTPIndex[k];

					for (opus_int i = 0; i < Constants.Ltp_Order; i++)
						psDecCtrl.LTPCoef_Q14[k * Constants.Ltp_Order + i] = (opus_int16)SigProc_Fix.Silk_LSHIFT(cbk_ptr_Q7[Ix * Constants.Ltp_Order + i], 7);
				}

				/**********************/
				/* Decode LTP scaling */
				/**********************/
				Ix = psDec.indices.LTP_scaleIndex;
				psDecCtrl.LTP_scale_Q14 = Tables_Other.Silk_LTPScales_Table_Q14[Ix];
			}
			else
			{
				SigProc_Fix.Silk_MemSet(psDecCtrl.pitchL, 0, psDec.nb_subfr);
				SigProc_Fix.Silk_MemSet<opus_int16>(psDecCtrl.LTPCoef_Q14, 0, Constants.Ltp_Order);

				psDec.indices.PERIndex = 0;
				psDecCtrl.LTP_scale_Q14 = 0;
			}
		}
	}
}
