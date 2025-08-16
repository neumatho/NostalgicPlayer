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
	internal static class Decode_Core
	{
		/********************************************************************/
		/// <summary>
		/// Core decoder. Performs inverse NSQ operation LTP + LPC
		/// </summary>
		/********************************************************************/
		public static void Silk_Decode_Core(Silk_Decoder_State psDec, Silk_Decoder_Control psDecCtrl, CPointer<opus_int16> xq, CPointer<opus_int16> pulses, c_int arch)
		{
			opus_int lag = 0;
			opus_int16[] A_Q12_tmp = new opus_int16[Constants.Max_Lpc_Order];

			CPointer<opus_int16> sLTP = new CPointer<opus_int16>(psDec.ltp_mem_length);
			CPointer<opus_int32> sLTP_Q15 = new CPointer<opus_int32>(psDec.ltp_mem_length + psDec.frame_length);
			opus_int32[] res_Q14 = new opus_int32[psDec.subfr_length];
			CPointer<opus_int32> sLPC_Q14 = new CPointer<opus_int32>(psDec.subfr_length + Constants.Max_Lpc_Order);

			opus_int32 offset_Q10 = Tables_Other.Silk_Quantization_Offsets_Q10[(int)psDec.indices.signalType >> 1][psDec.indices.quantOffsetType];

			bool NLSF_interpolation_flag;

			if (psDec.indices.NLSFInterpCoef_Q2 < (1 << 2))
				NLSF_interpolation_flag = true;
			else
				NLSF_interpolation_flag = false;

			// Decode excitation
			opus_int32 rand_seed = psDec.indices.Seed;

			for (opus_int i = 0; i < psDec.frame_length; i++)
			{
				rand_seed = SigProc_Fix.Silk_RAND(rand_seed);
				psDec.exc_Q14[i] = SigProc_Fix.Silk_LSHIFT(pulses[i], 14);

				if (psDec.exc_Q14[i] > 0)
					psDec.exc_Q14[i] -= Constants.Quant_Level_Adjust_Q10 << 4;
				else
				{
					if (psDec.exc_Q14[i] < 0)
						psDec.exc_Q14[i] += Constants.Quant_Level_Adjust_Q10 << 4;
				}

				psDec.exc_Q14[i] += offset_Q10 << 4;

				if (rand_seed < 0)
					psDec.exc_Q14[i] = -psDec.exc_Q14[i];

				rand_seed = SigProc_Fix.Silk_ADD32_ovflw((opus_uint32)rand_seed, (opus_uint32)pulses[i]);
			}

			// Copy LPC state
			SigProc_Fix.Silk_MemCpy(sLPC_Q14, psDec.sLpc_Q14_buf, Constants.Max_Lpc_Order);

			CPointer<opus_int32> pexc_Q14 = psDec.exc_Q14;
			CPointer<opus_int16> pxq = xq;
			opus_int sLTP_buf_idx = psDec.ltp_mem_length;

			// Loop over subframes
			for (opus_int k = 0; k < psDec.nb_subfr; k++)
			{
				CPointer<opus_int32> pres_Q14 = res_Q14;
				CPointer<opus_int16> A_Q12 = psDecCtrl.PredCoef_Q12[k >> 1];

				// Preload LPC coeficients to array on stack. Gives small performance gain
				SigProc_Fix.Silk_MemCpy(A_Q12_tmp, A_Q12, psDec.LPC_Order);

				CPointer<opus_int16> B_Q14 = psDecCtrl.LTPCoef_Q14 + k * Constants.Ltp_Order;
				SignalType signalType = psDec.indices.signalType;

				opus_int32 Gain_Q10 = SigProc_Fix.Silk_RSHIFT(psDecCtrl.Gains_Q16[k], 6);
				opus_int32 inv_gain_Q31 = Inlines.Silk_INVERSE32_VarQ(psDecCtrl.Gains_Q16[k], 47);

				// Calculate gain adjustment factor
				opus_int32 gain_adj_Q16;

				if (psDecCtrl.Gains_Q16[k] != psDec.prev_gain_Q16)
				{
					gain_adj_Q16 = Inlines.Silk_DIV32_VarQ(psDec.prev_gain_Q16, psDecCtrl.Gains_Q16[k], 16);

					// Scale short term state
					for (opus_int i = 0; i < Constants.Max_Lpc_Order; i++)
						sLPC_Q14[i] = Macros.Silk_SMULWW(gain_adj_Q16, sLPC_Q14[i]);
				}
				else
					gain_adj_Q16 = 1 << 16;

				// Save inv_gain
				psDec.prev_gain_Q16 = psDecCtrl.Gains_Q16[k];

				// Avoid abrupt transition from voiced PLC to unvoiced normal decoding
				if ((psDec.lossCnt != 0) && (psDec.prevSignalType == SignalType.Voiced) && (psDec.indices.signalType != SignalType.Voiced) && (k < (Constants.Max_Nb_Subfr / 2)))
				{
					SigProc_Fix.Silk_MemSet<opus_int16>(B_Q14, 0, Constants.Ltp_Order);
					B_Q14[Constants.Ltp_Order / 2] = (opus_int16)SigProc_Fix.Silk_FIX_CONST(0.25f, 14);

					signalType = SignalType.Voiced;
					psDecCtrl.pitchL[k] = psDec.lagPrev;
				}

				if (signalType == SignalType.Voiced)
				{
					// Voiced
					lag = psDecCtrl.pitchL[k];

					// Re-whitening
					if ((k == 0) || ((k == 2) && NLSF_interpolation_flag))
					{
						// Rewhiten with new A coefs
						opus_int start_idx = psDec.ltp_mem_length - lag - psDec.LPC_Order - Constants.Ltp_Order / 2;

						if (k == 2)
							SigProc_Fix.Silk_MemCpy(psDec.outBuf + psDec.ltp_mem_length, xq, 2 * psDec.subfr_length);

						LPC_Analysis_Filter.Silk_LPC_Analysis_Filter(sLTP + start_idx, psDec.outBuf + start_idx + k * psDec.subfr_length, A_Q12, psDec.ltp_mem_length - start_idx, psDec.LPC_Order, arch);

						// After rewhitening the LTP state is unscaled
						if (k == 0)
						{
							// Do LTP downscaling to reduce inter-packet dependency
							inv_gain_Q31 = SigProc_Fix.Silk_LSHIFT(Macros.Silk_SMULWB(inv_gain_Q31, psDecCtrl.LTP_scale_Q14), 2);
						}

						for (opus_int i = 0; i < (lag + Constants.Ltp_Order / 2); i++)
							sLTP_Q15[sLTP_buf_idx - i - 1] = Macros.Silk_SMULWB(inv_gain_Q31, sLTP[psDec.ltp_mem_length - i - 1]);
					}
					else
					{
						// Update LTP state when Gain changes
						if (gain_adj_Q16 != (1 << 16))
						{
							for (opus_int i = 0; i < (lag + Constants.Ltp_Order / 2); i++)
								sLTP_Q15[sLTP_buf_idx - i - 1] = Macros.Silk_SMULWW(gain_adj_Q16, sLTP_Q15[sLTP_buf_idx - i - 1]);
						}
					}
				}

				// Long-term prediction
				if (signalType == SignalType.Voiced)
				{
					// Set up pointer
					CPointer<opus_int32> pred_lag_ptr = sLTP_Q15 + sLTP_buf_idx - lag + Constants.Ltp_Order / 2;

					for (opus_int i = 0; i < psDec.subfr_length; i++)
					{
						// Unrolled loop
						// Avoids introduction a bias because silk_SMLAWB() always rounds to -inf
						opus_int32 LTP_pred_Q13 = 2;

						LTP_pred_Q13 = Macros.Silk_SMLAWB(LTP_pred_Q13, pred_lag_ptr[0], B_Q14[0]);
						LTP_pred_Q13 = Macros.Silk_SMLAWB(LTP_pred_Q13, pred_lag_ptr[-1], B_Q14[1]);
						LTP_pred_Q13 = Macros.Silk_SMLAWB(LTP_pred_Q13, pred_lag_ptr[-2], B_Q14[2]);
						LTP_pred_Q13 = Macros.Silk_SMLAWB(LTP_pred_Q13, pred_lag_ptr[-3], B_Q14[3]);
						LTP_pred_Q13 = Macros.Silk_SMLAWB(LTP_pred_Q13, pred_lag_ptr[-4], B_Q14[4]);

						pred_lag_ptr++;

						// Generate LPC excitation
						pres_Q14[i] = SigProc_Fix.Silk_ADD_LSHIFT32(pexc_Q14[i], LTP_pred_Q13, 1);

						// Update states
						sLTP_Q15[sLTP_buf_idx] = SigProc_Fix.Silk_LSHIFT(pres_Q14[i], 1);
						sLTP_buf_idx++;
					}
				}
				else
					pres_Q14 = pexc_Q14;

				for (opus_int i = 0; i < psDec.subfr_length; i++)
				{
					// Short-term prediction
					// Avoids introduction a bias because silk_SMLAWB() always rounds to -inf
					opus_int32 LPC_pred_Q10 = SigProc_Fix.Silk_RSHIFT(psDec.LPC_Order, 1);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, sLPC_Q14[Constants.Max_Lpc_Order + i - 1], A_Q12_tmp[0]);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, sLPC_Q14[Constants.Max_Lpc_Order + i - 2], A_Q12_tmp[1]);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, sLPC_Q14[Constants.Max_Lpc_Order + i - 3], A_Q12_tmp[2]);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, sLPC_Q14[Constants.Max_Lpc_Order + i - 4], A_Q12_tmp[3]);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, sLPC_Q14[Constants.Max_Lpc_Order + i - 5], A_Q12_tmp[4]);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, sLPC_Q14[Constants.Max_Lpc_Order + i - 6], A_Q12_tmp[5]);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, sLPC_Q14[Constants.Max_Lpc_Order + i - 7], A_Q12_tmp[6]);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, sLPC_Q14[Constants.Max_Lpc_Order + i - 8], A_Q12_tmp[7]);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, sLPC_Q14[Constants.Max_Lpc_Order + i - 9], A_Q12_tmp[8]);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, sLPC_Q14[Constants.Max_Lpc_Order + i - 10], A_Q12_tmp[9]);

					if (psDec.LPC_Order == 16)
					{
						LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, sLPC_Q14[Constants.Max_Lpc_Order + i - 11], A_Q12_tmp[10]);
						LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, sLPC_Q14[Constants.Max_Lpc_Order + i - 12], A_Q12_tmp[11]);
						LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, sLPC_Q14[Constants.Max_Lpc_Order + i - 13], A_Q12_tmp[12]);
						LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, sLPC_Q14[Constants.Max_Lpc_Order + i - 14], A_Q12_tmp[13]);
						LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, sLPC_Q14[Constants.Max_Lpc_Order + i - 15], A_Q12_tmp[14]);
						LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, sLPC_Q14[Constants.Max_Lpc_Order + i - 16], A_Q12_tmp[15]);
					}

					// Add prediction to LPC excitation
					sLPC_Q14[Constants.Max_Lpc_Order + i] = Macros.Silk_ADD_SAT32(pres_Q14[i], SigProc_Fix.Silk_LSHIFT_SAT32(LPC_pred_Q10, 4));

					// Scale with gain
					pxq[i] = (opus_int16)SigProc_Fix.Silk_SAT16(SigProc_Fix.Silk_RSHIFT_ROUND(Macros.Silk_SMULWW(sLPC_Q14[Constants.Max_Lpc_Order + i], Gain_Q10), 8));
				}

				// Update LPC filter state
				SigProc_Fix.Silk_MemCpy(sLPC_Q14, sLPC_Q14 + psDec.subfr_length, Constants.Max_Lpc_Order);

				pexc_Q14 += psDec.subfr_length;
				pxq += psDec.subfr_length;
			}

			// Save LPC state
			SigProc_Fix.Silk_MemCpy(psDec.sLpc_Q14_buf, sLPC_Q14, Constants.Max_Lpc_Order);
		}
	}
}
