/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Decoder_Set_Fs
	{
		/********************************************************************/
		/// <summary>
		/// Set decoder sampling rate
		/// </summary>
		/********************************************************************/
		public static SilkError Silk_Decoder_Set_Fs(Silk_Decoder_State psDec, opus_int fs_kHz, opus_int32 fs_API_Hz)
		{
			SilkError ret = SilkError.No_Error;
			SilkError resultRet;

			// New (sub)frame length
			psDec.subfr_length = Macros.Silk_SMULBB(Constants.Sub_Frame_Length_Ms, fs_kHz);
			opus_int frame_length = Macros.Silk_SMULBB(psDec.nb_subfr, psDec.subfr_length);

			// Initialize resampler when switching internal or external sampling frequency
			if ((psDec.fs_kHz != fs_kHz) || (psDec.fs_API_hz != fs_API_Hz))
			{
				// Initialize the resampler for dec_API.c preparing resampling from fs_kHz to API_fs_Hz
				resultRet = Resampler.Silk_Resampler_Init(psDec.resampler_state, Macros.Silk_SMULBB(fs_kHz, 1000), fs_API_Hz, false);
				ret = resultRet != SilkError.No_Error ? resultRet : ret;

				psDec.fs_API_hz = fs_API_Hz;
			}

			if ((psDec.fs_kHz != fs_kHz) || (frame_length != psDec.frame_length))
			{
				if (fs_kHz == 8)
				{
					if (psDec.nb_subfr == Constants.Max_Nb_Subfr)
						psDec.pitch_contour_iCDF = Tables_Pitch_Lag.Silk_Pitch_Contour_NB_iCDF;
					else
						psDec.pitch_contour_iCDF = Tables_Pitch_Lag.Silk_Pitch_Contour_10_Ms_NB_iCDF;
				}
				else
				{
					if (psDec.nb_subfr == Constants.Max_Nb_Subfr)
						psDec.pitch_contour_iCDF = Tables_Pitch_Lag.Silk_Pitch_Contour_iCDF;
					else
						psDec.pitch_contour_iCDF = Tables_Pitch_Lag.Silk_Pitch_Contour_10_Ms_iCDF;
				}

				if (psDec.fs_kHz != fs_kHz)
				{
					psDec.ltp_mem_length = Macros.Silk_SMULBB(Constants.Ltp_Mem_Length_Ms, fs_kHz);

					if ((fs_kHz == 8) || (fs_kHz == 12))
					{
						psDec.LPC_Order = Constants.Min_Lpc_Order;
						psDec.psNLSF_CB = Tables_NLSF_CB_NB_MB.Silk_NLSF_CB_NB_MB;
					}
					else
					{
						psDec.LPC_Order = Constants.Max_Lpc_Order;
						psDec.psNLSF_CB = Tables_NLSF_CB_WB.Silk_NLSF_CB_WB;
					}

					if (fs_kHz == 16)
						psDec.pitch_lag_low_bits_iCDF = Tables_Other.Silk_Uniform8_iCDF;
					else if (fs_kHz == 12)
						psDec.pitch_lag_low_bits_iCDF = Tables_Other.Silk_Uniform6_iCDF;
					else if (fs_kHz == 8)
						psDec.pitch_lag_low_bits_iCDF = Tables_Other.Silk_Uniform4_iCDF;
					else
					{
						// Unsupported sampling rate
						throw new NotSupportedException("Unsupported sampling rate");
					}

					psDec.first_frame_after_reset = true;
					psDec.lagPrev = 100;
					psDec.LastGainIndex = 10;
					psDec.prevSignalType = SignalType.No_Voice_Activity;

					psDec.outBuf.Clear();
					SigProc_Fix.Silk_MemSet(psDec.sLpc_Q14_buf, 0, psDec.sLpc_Q14_buf.Length);
				}

				psDec.fs_kHz = fs_kHz;
				psDec.frame_length = frame_length;
			}

			return ret;
		}
	}
}
