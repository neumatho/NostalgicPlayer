/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Decode_Pulses
	{
		/********************************************************************/
		/// <summary>
		/// Decode quantization indices of excitation
		/// </summary>
		/********************************************************************/
		public static void Silk_Decode_Pulses(Ec_Dec psRangeDec, Pointer<opus_int16> pulses, SignalType signalType, opus_int quantOffsetType, opus_int frame_length)
		{
			opus_int[] sum_pulses = new opus_int[Constants.Max_Nb_Shell_Blocks];
			opus_int[] nLshifts = new opus_int[Constants.Max_Nb_Shell_Blocks];

			/*********************/
			/* Decode rate level */
			/*********************/
			opus_int RateLevelIndex = EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Pulses_Per_Block.Silk_Rate_Levels_iCDF[(int)signalType >> 1], 8);

			// Calculate number of shell blocks
			opus_int iter = SigProc_Fix.Silk_RSHIFT(frame_length, Constants.Log2_Shell_Codec_Frame_Length);

			if ((iter * Constants.Shell_Codec_Frame_Length) < frame_length)
				iter++;

			/***************************************************/
			/* Sum-Weighted-Pulses Decoding                    */
			/***************************************************/
			opus_uint8[] cdf_ptr = Tables_Pulses_Per_Block.Silk_Pulses_Per_Block_iCDF[RateLevelIndex];

			for (opus_int i = 0; i < iter; i++)
			{
				nLshifts[i] = 0;
				sum_pulses[i] = EntDec.Ec_Dec_Icdf(psRangeDec, cdf_ptr, 8);

				// LSB indication
				while (sum_pulses[i] == (Constants.Silk_Max_Pulses + 1))
				{
					nLshifts[i]++;

					// When we've already got 10 LSBs, we shift the table to not allow (SILK_MAX_PULSES + 1)
					sum_pulses[i] = EntDec.Ec_Dec_Icdf(psRangeDec, new Pointer<opus_uint8>(Tables_Pulses_Per_Block.Silk_Pulses_Per_Block_iCDF[Constants.N_Rate_Levels - 1], nLshifts[i] == 10 ? 1 : 0), 8);
				}
			}

			/***************************************************/
			/* Shell decoding                                  */
			/***************************************************/
			for (opus_int i = 0; i < iter; i++)
			{
				if (sum_pulses[i] > 0)
					Shell_Coder.Silk_Shell_Decoder(pulses + Macros.Silk_SMULBB(i, Constants.Shell_Codec_Frame_Length), psRangeDec, sum_pulses[i]);
				else
					SigProc_Fix.Silk_MemSet<opus_int16>(pulses + Macros.Silk_SMULBB(i, Constants.Shell_Codec_Frame_Length), 0, Constants.Shell_Codec_Frame_Length);
			}

			/***************************************************/
			/* LSB Decoding                                    */
			/***************************************************/
			for (opus_int i = 0; i < iter; i++)
			{
				if (nLshifts[i] > 0)
				{
					opus_int nLS = nLshifts[i];
					Pointer<opus_int16> pulses_ptr = pulses + Macros.Silk_SMULBB(i, Constants.Shell_Codec_Frame_Length);

					for (opus_int k = 0; k < Constants.Shell_Codec_Frame_Length; k++)
					{
						opus_int abs_q = pulses_ptr[k];

						for (opus_int j = 0; j < nLS; j++)
						{
							abs_q = SigProc_Fix.Silk_LSHIFT(abs_q, 1);
							abs_q += EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Other.Silk_Lsb_iCDF, 8);
						}

						pulses_ptr[k] = (opus_int16)abs_q;
					}

					// Mark the number of pulses non-zero for sign decoding
					sum_pulses[i] |= nLS << 5;
				}
			}

			/****************************************/
			/* Decode and add signs to pulse signal */
			/****************************************/
			Code_Signs.Silk_Decode_Signs(psRangeDec, pulses, frame_length, signalType, quantOffsetType, sum_pulses);
		}
	}
}
