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
	internal static class NLSF_Unpack
	{
		/********************************************************************/
		/// <summary>
		/// Unpack predictor values and indicies for entropy coding tables
		/// </summary>
		/********************************************************************/
		public static void Silk_NLSF_Unpack(CPointer<opus_int16> ec_ix, CPointer<opus_uint8> pred_Q8, Silk_NLSF_CB_Struct psNLSF_CB, opus_int CB1_index)
		{
			CPointer<opus_uint8> ec_sel_ptr = psNLSF_CB.ec_sel + CB1_index * psNLSF_CB.order / 2;

			for (opus_int i = 0; i < psNLSF_CB.order; i += 2)
			{
				opus_uint8 entry = ec_sel_ptr[0, 1];

				ec_ix[i] = (opus_int16)Macros.Silk_SMULBB(SigProc_Fix.Silk_RSHIFT(entry, 1) & 7, 2 * Constants.Nlsf_Quant_Max_Amplitude + 1);
				pred_Q8[i] = psNLSF_CB.pred_Q8[i + (entry & 1) * (psNLSF_CB.order - 1)];

				ec_ix[i + 1] = (opus_int16)Macros.Silk_SMULBB(SigProc_Fix.Silk_RSHIFT(entry, 5) & 7, 2 * Constants.Nlsf_Quant_Max_Amplitude + 1);
				pred_Q8[i + 1] = psNLSF_CB.pred_Q8[i + (SigProc_Fix.Silk_RSHIFT(entry, 4) & 1) * (psNLSF_CB.order - 1) + 1];
			}
		}
	}
}
