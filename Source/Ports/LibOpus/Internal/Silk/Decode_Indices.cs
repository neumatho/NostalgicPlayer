/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Decode_Indices
	{
		/********************************************************************/
		/// <summary>
		/// Decode side-information parameters from payload
		/// </summary>
		/********************************************************************/
		public static void Silk_Decode_Indices(Silk_Decoder_State psDec, Ec_Dec psRangeDec, opus_int FrameIndex, bool decode_LBRR, CodeType condCoding)
		{
			opus_int Ix;
			opus_int16[] ec_ix = new opus_int16[Constants.Max_Lpc_Order];
			opus_uint8[] pred_Q8 = new opus_uint8[Constants.Max_Lpc_Order];

			/*******************************************/
			/* Decode signal type and quantizer offset */
			/*******************************************/
			if (decode_LBRR || psDec.VAD_flags[FrameIndex])
				Ix = EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Other.Silk_Type_Offset_VAD_iCDF, 8) + 2;
			else
				Ix = EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Other.Silk_Type_Offset_No_VAD_iCDF, 8);

			psDec.indices.signalType = (SignalType)(opus_int8)SigProc_Fix.Silk_RSHIFT(Ix, 1);
			psDec.indices.quantOffsetType = (opus_int8)(Ix & 1);

			/****************/
			/* Decode gains */
			/****************/
			// First subframe
			if (condCoding == CodeType.Conditionally)
			{
				// Conditional coding
				psDec.indices.GainsIndicies[0] = (opus_int8)EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Gain.Silk_Delta_Gain_iCDF, 8);
			}
			else
			{
				// Independent coding, in two stages: MSB bits followed by 3 LSBs
				psDec.indices.GainsIndicies[0] = (opus_int8)SigProc_Fix.Silk_LSHIFT(EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Gain.Silk_Gain_iCDF[(int)psDec.indices.signalType], 8), 3);
				psDec.indices.GainsIndicies[0] += (opus_int8)EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Other.Silk_Uniform8_iCDF, 8);
			}

			// Remaining subframes
			for (opus_int i = 1; i < psDec.nb_subfr; i++)
				psDec.indices.GainsIndicies[i] = (opus_int8)EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Gain.Silk_Delta_Gain_iCDF, 8);

			/**********************/
			/* Decode LSF Indices */
			/**********************/
			psDec.indices.NLSFIndices[0] = (opus_int8)EntDec.Ec_Dec_Icdf(psRangeDec, psDec.psNLSF_CB.CB1_iCDF + ((int)psDec.indices.signalType >> 1) * psDec.psNLSF_CB.nVectors, 8);
			NLSF_Unpack.Silk_NLSF_Unpack(ec_ix, pred_Q8, psDec.psNLSF_CB, psDec.indices.NLSFIndices[0]);

			for (opus_int i = 0; i < psDec.psNLSF_CB.order; i++)
			{
				Ix = EntDec.Ec_Dec_Icdf(psRangeDec, psDec.psNLSF_CB.ec_iCDF + ec_ix[i], 8);

				if (Ix == 0)
					Ix -= EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Other.Silk_NLSF_EXT_iCDF, 8);
				else if (Ix == (2 * Constants.Nlsf_Quant_Max_Amplitude))
					Ix += EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Other.Silk_NLSF_EXT_iCDF, 8);

				psDec.indices.NLSFIndices[i + 1] = (opus_int8)(Ix - Constants.Nlsf_Quant_Max_Amplitude);
			}

			// Decode LSF interpolation factor
			if (psDec.nb_subfr == Constants.Max_Nb_Subfr)
				psDec.indices.NLSFInterpCoef_Q2 = (opus_int8)EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Other.Silk_NLSF_Interpolation_Factor_iCDF, 8);
			else
				psDec.indices.NLSFInterpCoef_Q2 = 4;

			if (psDec.indices.signalType == SignalType.Voiced)
			{
				/*********************/
				/* Decode pitch lags */
				/*********************/
				// Get lag index
				bool decode_absolute_lagIndex = true;

				if ((condCoding == CodeType.Conditionally) && (psDec.ec_prevSignalType == SignalType.Voiced))
				{
					// Decode delta index
					opus_int delta_lagIndex = (opus_int16)EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Pitch_Lag.Silk_Pitch_Delta_iCDF, 8);

					if (delta_lagIndex > 0)
					{
						delta_lagIndex = delta_lagIndex - 9;
						psDec.indices.lagIndex = (opus_int16)(psDec.ec_prevLagIndex + delta_lagIndex);
						decode_absolute_lagIndex = false;
					}
				}

				if (decode_absolute_lagIndex)
				{
					// Absolute coding
					psDec.indices.lagIndex = (opus_int16)(EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Pitch_Lag.Silk_Pitch_Lag_iCDF, 8) * SigProc_Fix.Silk_RSHIFT(psDec.fs_kHz, 1));
					psDec.indices.lagIndex += (opus_int16)EntDec.Ec_Dec_Icdf(psRangeDec, psDec.pitch_lag_low_bits_iCDF, 8);
				}

				psDec.ec_prevLagIndex = psDec.indices.lagIndex;

				// Get countour index
				psDec.indices.contourIndex = (opus_int8)EntDec.Ec_Dec_Icdf(psRangeDec, psDec.pitch_contour_iCDF, 8);

				/********************/
				/* Decode LTP gains */
				/********************/
				// Decode PERIndex value
				psDec.indices.PERIndex = (opus_int8)EntDec.Ec_Dec_Icdf(psRangeDec, Tables_LTP.Silk_LTP_Per_Index_iCDF, 8);

				for (opus_int k = 0; k < psDec.nb_subfr; k++)
					psDec.indices.LTPIndex[k] = (opus_int8)EntDec.Ec_Dec_Icdf(psRangeDec, Tables_LTP.Silk_LTP_Gain_iCDF_Ptrs[psDec.indices.PERIndex], 8);

				/**********************/
				/* Decode LTP scaling */
				/**********************/
				if (condCoding == CodeType.Independently)
					psDec.indices.LTP_scaleIndex = (opus_int8)EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Other.Silk_LTPscale_iCDF, 8);
				else
					psDec.indices.LTP_scaleIndex = 0;
			}

			psDec.ec_prevSignalType = psDec.indices.signalType;

			/***************/
			/* Decode seed */
			/***************/
			psDec.indices.Seed = (opus_int8)EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Other.Silk_Uniform4_iCDF, 8);
		}
	}
}
