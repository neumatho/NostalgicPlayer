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
	internal static class Gain_Quant
	{
		private const int Offset = (Constants.Min_QGain_dB * 128) / 6 + 16 * 128;
		private const int Inv_Scale_Q16 = (65536 * (((Constants.Max_QGain_dB - Constants.Min_QGain_dB) * 128) / 6)) / (Constants.N_Levels_QGain - 1);

		/********************************************************************/
		/// <summary>
		/// Gains scalar dequantization, uniform on log scale
		/// </summary>
		/********************************************************************/
		public static void Silk_Gains_Dequant(CPointer<opus_int32> gain_Q16, CPointer<opus_int8> ind, ref opus_int8 prev_ind, bool conditional, opus_int nb_subfr)
		{
			for (opus_int k = 0; k < nb_subfr; k++)
			{
				if ((k == 0) && (conditional == false))
				{
					// Gain index is not allowed to go down more than 16 steps (~21.8 dB)
					prev_ind = (opus_int8)SigProc_Fix.Silk_Max_Int(ind[k], prev_ind - 16);
				}
				else
				{
					// Delta index
					opus_int ind_tmp = ind[k] + Constants.Min_Delta_Gain_Quant;

					// Accumulate deltas
					opus_int double_step_size_threshold = 2 * Constants.Max_Delta_Gain_Quant - Constants.N_Levels_QGain + prev_ind;

					if (ind_tmp > double_step_size_threshold)
						prev_ind += (opus_int8)(SigProc_Fix.Silk_LSHIFT(ind_tmp, 1) - double_step_size_threshold);
					else
						prev_ind += (opus_int8)ind_tmp;
				}

				prev_ind = (opus_int8)SigProc_Fix.Silk_LIMIT_Int(prev_ind, 0, Constants.N_Levels_QGain - 1);

				// Scale and convert to linear scale
				gain_Q16[k] = Log2Lin.Silk_Log2Lin(SigProc_Fix.Silk_Min_32(Macros.Silk_SMULWB(Inv_Scale_Q16, prev_ind) + Offset, 3967));	// 3967 = 31 in Q7
			}
		}
	}
}
