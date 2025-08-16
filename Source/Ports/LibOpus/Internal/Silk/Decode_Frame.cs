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
	internal static class Decode_Frame
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static SilkError Silk_Decode_Frame(Silk_Decoder_State psDec, Ec_Dec psRangeDec, CPointer<opus_int16> pOut, out opus_int32 pN, LostFlag lostFlag, CodeType condCoding, c_int arch)
		{
			SilkError ret = SilkError.No_Error;

			opus_int L = psDec.frame_length;
			Silk_Decoder_Control psDecCtrl = new Silk_Decoder_Control();

			psDecCtrl.LTP_scale_Q14 = 0;

			if ((lostFlag == LostFlag.Decode_Normal) || ((lostFlag == LostFlag.Decode_LBRR) && (psDec.LBRR_flags[psDec.nFramesDecoded] == true)))
			{
				opus_int16[] pulses = new opus_int16[(L + Constants.Shell_Codec_Frame_Length - 1) & ~(Constants.Shell_Codec_Frame_Length - 1)];

				/*********************************************/
				/* Decode quantization indices of side info  */
				/*********************************************/
				Decode_Indices.Silk_Decode_Indices(psDec, psRangeDec, psDec.nFramesDecoded, lostFlag != LostFlag.Decode_Normal, condCoding);

				/*********************************************/
				/* Decode quantization indices of excitation */
				/*********************************************/
				Decode_Pulses.Silk_Decode_Pulses(psRangeDec, pulses, psDec.indices.signalType, psDec.indices.quantOffsetType, psDec.frame_length);

				/********************************************/
				/* Decode parameters and pulse signal       */
				/********************************************/
				Decode_Parameters.Silk_Decode_Parameters(psDec, psDecCtrl, condCoding);

				/********************************************************/
				/* Run inverse NSQ                                      */
				/********************************************************/
				Decode_Core.Silk_Decode_Core(psDec, psDecCtrl, pOut, pulses, arch);

				/*************************/
				/* Update output buffer  */
				/*************************/
				opus_int mv_len = psDec.ltp_mem_length - psDec.frame_length;
				SigProc_Fix.Silk_MemMove(psDec.outBuf, psDec.outBuf + psDec.frame_length, mv_len);
				SigProc_Fix.Silk_MemCpy(psDec.outBuf + mv_len, pOut, psDec.frame_length);

				/********************************************************/
				/* Update PLC state                                     */
				/********************************************************/
				Plc.Silk_PLC(psDec, psDecCtrl, pOut, false, arch);

				psDec.lossCnt = 0;
				psDec.prevSignalType = psDec.indices.signalType;

				// A frame has been decoded without errors
				psDec.first_frame_after_reset = false;
			}
			else
			{
				// Handle packet loss by extrapolation
				Plc.Silk_PLC(psDec, psDecCtrl, pOut, true, arch);

				/*************************/
				/* Update output buffer  */
				/*************************/
				opus_int mv_len = psDec.ltp_mem_length - psDec.frame_length;
				SigProc_Fix.Silk_MemMove(psDec.outBuf, psDec.outBuf + psDec.frame_length, mv_len);
				SigProc_Fix.Silk_MemCpy(psDec.outBuf + mv_len, pOut, psDec.frame_length);
			}

			/************************************************/
			/* Comfort noise generation / estimation        */
			/************************************************/
			Cng.Silk_CNG(psDec, psDecCtrl, pOut, L);

			/****************************************************************/
			/* Ensure smooth connection of extrapolated and good frames     */
			/****************************************************************/
			Plc.Silk_PLC_Glue_Frames(psDec, pOut, L);

			// Update some decoder state variables
			psDec.lagPrev = psDecCtrl.pitchL[psDec.nb_subfr - 1];

			// Set output frame length
			pN = L;

			return ret;
		}
	}
}
