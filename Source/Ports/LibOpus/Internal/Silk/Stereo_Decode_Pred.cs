/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Stereo_Decode_Pred
	{
		/********************************************************************/
		/// <summary>
		/// Decode mid/side predictors
		/// </summary>
		/********************************************************************/
		public static void Silk_Stereo_Decode_Pred(Ec_Dec psRangeDec, CPointer<opus_int32> pred_Q13)
		{
			opus_int[][] ix = ArrayHelper.Initialize2Arrays<opus_int>(2, 3);

			// Entropy decoding
			opus_int n = EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Other.Silk_Stereo_Pred_Joint_iCDF, 8);
			ix[0][2] = SigProc_Fix.Silk_DIV32_16(n, 5);
			ix[1][2] = n - 5 * ix[0][2];

			for (n = 0; n < 2; n++)
			{
				ix[n][0] = EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Other.Silk_Uniform3_iCDF, 8);
				ix[n][1] = EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Other.Silk_Uniform5_iCDF, 8);
			}

			// Dequantize
			for (n = 0; n < 2 ; n++)
			{
				ix[n][0] += 3 * ix[n][2];

				opus_int32 low_Q13 = Tables_Other.Silk_Stereo_Pred_Quant_Q13[ix[n][0]];
				opus_int32 step_Q13 = Macros.Silk_SMULWB(Tables_Other.Silk_Stereo_Pred_Quant_Q13[ix[n][0] + 1] - low_Q13, SigProc_Fix.Silk_FIX_CONST(0.5f / Constants.Stereo_Quant_Sub_Steps, 16));

				pred_Q13[n] = Macros.Silk_SMLABB(low_Q13, step_Q13, 2 * ix[n][1] + 1);
			}

			// Subtract second from first predictor (helps when actually applying these)
			pred_Q13[0] -= pred_Q13[1];
		}



		/********************************************************************/
		/// <summary>
		/// Decode mid-only flag
		/// </summary>
		/********************************************************************/
		public static void Silk_Stereo_Decode_Mid_Only(Ec_Dec psRangeDec, out bool decode_only_mid)
		{
			// Decode flag that only mid channel is coded
			decode_only_mid = EntDec.Ec_Dec_Icdf(psRangeDec, Tables_Other.Silk_Stereo_Only_Code_Mid_iCDF, 8) != 0;
		}
	}
}
