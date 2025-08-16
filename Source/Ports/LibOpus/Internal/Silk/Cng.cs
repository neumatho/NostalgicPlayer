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
	internal static class Cng
	{
		/********************************************************************/
		/// <summary>
		/// Generates excitation for CNG LPC synthesis
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Silk_CNG_Exc(CPointer<opus_int32> exc_Q14, CPointer<opus_int32> exc_buf_Q14, opus_int length, ref opus_int32 rand_seed)
		{
			opus_int exc_mask = Constants.Cng_Buf_Mask_Max;

			while (exc_mask > length)
				exc_mask = SigProc_Fix.Silk_RSHIFT(exc_mask, 1);

			opus_int32 seed = rand_seed;

			for (opus_int i = 0; i < length; i++)
			{
				seed = SigProc_Fix.Silk_RAND(seed);
				opus_int idx = SigProc_Fix.Silk_RSHIFT(seed, 24) & exc_mask;

				exc_Q14[i]= exc_buf_Q14[idx];
			}

			rand_seed = seed;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Silk_CNG_Reset(Silk_Decoder_State psDec)
		{
			opus_int NLSF_step_Q15 = SigProc_Fix.Silk_DIV32_16(opus_int16.MaxValue, (opus_int16)(psDec.LPC_Order + 1));
			opus_int NLSF_acc_Q15 = 0;

			for (opus_int i = 0; i < psDec.LPC_Order; i++)
			{
				NLSF_acc_Q15 += NLSF_step_Q15;
				psDec.sCNG.CNG_smth_NLSF_Q15[i] = (opus_int16)NLSF_acc_Q15;
			}

			psDec.sCNG.CNG_smth_Gain_Q16 = 0;
			psDec.sCNG.rand_seed = 3176576;
		}



		/********************************************************************/
		/// <summary>
		/// Updates CNG estimate, and applies the CNG when packet was lost
		/// </summary>
		/********************************************************************/
		public static void Silk_CNG(Silk_Decoder_State psDec, Silk_Decoder_Control psDecCtrl, CPointer<opus_int16> frame, opus_int length)
		{
			opus_int16[] A_Q12 = new opus_int16[Constants.Max_Lpc_Order];
			Silk_CNG_Struct psCNG = psDec.sCNG;

			if (psDec.fs_kHz != psCNG.fs_kHz)
			{
				// Reset state
				Silk_CNG_Reset(psDec);

				psCNG.fs_kHz = psDec.fs_kHz;
			}

			if ((psDec.lossCnt == 0) && (psDec.prevSignalType == SignalType.No_Voice_Activity))
			{
				// Update CNG parameters

				// Smoothing of LSF's
				for (opus_int i = 0; i < psDec.LPC_Order; i++)
					psCNG.CNG_smth_NLSF_Q15[i] += (opus_int16)Macros.Silk_SMULWB(psDec.prevNLSF_Q15[i] - psCNG.CNG_smth_NLSF_Q15[i], Constants.Cng_Nlsf_Smth_Q16);

				// Find the subframe with the highest gain
				opus_int32 max_Gain_Q16 = 0;
				opus_int subfr = 0;

				for (opus_int i = 0; i < psDec.nb_subfr; i++)
				{
					if (psDecCtrl.Gains_Q16[i] > max_Gain_Q16)
					{
						max_Gain_Q16 = psDecCtrl.Gains_Q16[i];
						subfr = i;
					}
				}

				// Update CNG excitation buffer with excitation from this subframe
				SigProc_Fix.Silk_MemMove(psCNG.CNG_exc_buf_Q14 + psDec.subfr_length, psCNG.CNG_exc_buf_Q14, (psDec.nb_subfr - 1) * psDec.subfr_length);
				SigProc_Fix.Silk_MemCpy(psCNG.CNG_exc_buf_Q14, psDec.exc_Q14 + subfr * psDec.subfr_length, psDec.subfr_length);

				// Smooth gains
				for (opus_int i = 0; i < psDec.nb_subfr; i++)
				{
					psCNG.CNG_smth_Gain_Q16 += Macros.Silk_SMULWB(psDecCtrl.Gains_Q16[i] - psCNG.CNG_smth_Gain_Q16, Constants.Cng_Gain_Smth_Q16);

					// If the smoothed gain is 3 dB greater than this subframe's gain, use this subframe's gain to adapt faster
					if (Macros.Silk_SMULWW(psCNG.CNG_smth_Gain_Q16, Constants.Cng_Gain_Smth_Threshold_Q16) > psDecCtrl.Gains_Q16[i])
						psCNG.CNG_smth_Gain_Q16 = psDecCtrl.Gains_Q16[i];
				}
			}

			// Add CNG when packet is lost or during DTX
			if (psDec.lossCnt != 0)
			{
				CPointer<opus_int32> CNG_sig_Q14 = new CPointer<opus_int32>(length + Constants.Max_Lpc_Order);

				// Generate CNG excitation
				opus_int32 gain_Q16 = Macros.Silk_SMULWW(psDec.sPLC.randScale_Q14, psDec.sPLC.prevGain_Q16[1]);

				if ((gain_Q16 >= (1 << 21)) || (psCNG.CNG_smth_Gain_Q16 > (1 << 23)))
				{
					gain_Q16 = SigProc_Fix.Silk_SMULTT(gain_Q16, gain_Q16);
					gain_Q16 = SigProc_Fix.Silk_SUB_LSHIFT32(SigProc_Fix.Silk_SMULTT(psCNG.CNG_smth_Gain_Q16, psCNG.CNG_smth_Gain_Q16), gain_Q16, 5);
					gain_Q16 = SigProc_Fix.Silk_LSHIFT32(Inlines.Silk_SQRT_APPROX(gain_Q16), 16);
				}
				else
				{
					gain_Q16 = Macros.Silk_SMULWW(gain_Q16, gain_Q16);
					gain_Q16 = SigProc_Fix.Silk_SUB_LSHIFT32(Macros.Silk_SMULWW(psCNG.CNG_smth_Gain_Q16, psCNG.CNG_smth_Gain_Q16), gain_Q16, 5);
					gain_Q16 = SigProc_Fix.Silk_LSHIFT32(Inlines.Silk_SQRT_APPROX(gain_Q16), 8);
				}

				opus_int32 gain_Q10 = SigProc_Fix.Silk_RSHIFT(gain_Q16, 6);

				Silk_CNG_Exc(CNG_sig_Q14 + Constants.Max_Lpc_Order, psCNG.CNG_exc_buf_Q14, length, ref psCNG.rand_seed);

				// Convert CNG NLSF to filter representation
				NLSF2A.Silk_NLSF2A(A_Q12, psCNG.CNG_smth_NLSF_Q15, psDec.LPC_Order, psDec.arch);

				// Generate CNG signal, by synthesis filtering
				SigProc_Fix.Silk_MemCpy(CNG_sig_Q14, psCNG.CNG_synth_state, Constants.Max_Lpc_Order);

				for (opus_int i = 0; i < length; i++)
				{
					// Avoids introducing a bias because silk_SMLAWB() always rounds to -inf
					opus_int32 LPC_pred_Q10 = SigProc_Fix.Silk_RSHIFT(psDec.LPC_Order, 1);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, CNG_sig_Q14[Constants.Max_Lpc_Order + i - 1], A_Q12[0]);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, CNG_sig_Q14[Constants.Max_Lpc_Order + i - 2], A_Q12[1]);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, CNG_sig_Q14[Constants.Max_Lpc_Order + i - 3], A_Q12[2]);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, CNG_sig_Q14[Constants.Max_Lpc_Order + i - 4], A_Q12[3]);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, CNG_sig_Q14[Constants.Max_Lpc_Order + i - 5], A_Q12[4]);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, CNG_sig_Q14[Constants.Max_Lpc_Order + i - 6], A_Q12[5]);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, CNG_sig_Q14[Constants.Max_Lpc_Order + i - 7], A_Q12[6]);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, CNG_sig_Q14[Constants.Max_Lpc_Order + i - 8], A_Q12[7]);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, CNG_sig_Q14[Constants.Max_Lpc_Order + i - 9], A_Q12[8]);
					LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, CNG_sig_Q14[Constants.Max_Lpc_Order + i - 10], A_Q12[9]);

					if (psDec.LPC_Order == 16)
					{
						LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, CNG_sig_Q14[Constants.Max_Lpc_Order + i - 11], A_Q12[10]);
						LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, CNG_sig_Q14[Constants.Max_Lpc_Order + i - 12], A_Q12[11]);
						LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, CNG_sig_Q14[Constants.Max_Lpc_Order + i - 13], A_Q12[12]);
						LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, CNG_sig_Q14[Constants.Max_Lpc_Order + i - 14], A_Q12[13]);
						LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, CNG_sig_Q14[Constants.Max_Lpc_Order + i - 15], A_Q12[14]);
						LPC_pred_Q10 = Macros.Silk_SMLAWB(LPC_pred_Q10, CNG_sig_Q14[Constants.Max_Lpc_Order + i - 16], A_Q12[15]);
					}

					// Update states
					CNG_sig_Q14[Constants.Max_Lpc_Order + i] = Macros.Silk_ADD_SAT32(CNG_sig_Q14[Constants.Max_Lpc_Order + i], SigProc_Fix.Silk_LSHIFT_SAT32(LPC_pred_Q10, 4));

					// Scale with Gain and add to input signal
					frame[i] = SigProc_Fix.Silk_ADD_SAT16(frame[i], SigProc_Fix.Silk_SAT16(SigProc_Fix.Silk_RSHIFT_ROUND(Macros.Silk_SMULWW(CNG_sig_Q14[Constants.Max_Lpc_Order + i], gain_Q10), 8)));
				}

				SigProc_Fix.Silk_MemCpy(psCNG.CNG_synth_state, CNG_sig_Q14 + length, Constants.Max_Lpc_Order);
			}
			else
				SigProc_Fix.Silk_MemSet(psCNG.CNG_synth_state, 0, psDec.LPC_Order);
		}
	}
}
