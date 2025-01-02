/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.CKit;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Bwexpander
	{
		/********************************************************************/
		/// <summary>
		/// Chirp (bandwidth expand) LP AR filter
		/// </summary>
		/********************************************************************/
		public static void Silk_Bwexpander(CPointer<opus_int16> ar, opus_int d, opus_int32 chirp_Q16)
		{
			opus_int32 chirp_minus_one_Q16 = chirp_Q16 - 65536;

			// NB: Dont use silk_SMULWB, instead of silk_RSHIFT_ROUND( silk_MUL(), 16 ), below.
			// Bias in silk_SMULWB can lead to unstable filters
			for (opus_int i = 0; i < (d - 1); i++)
			{
				ar[i] = (opus_int16)SigProc_Fix.Silk_RSHIFT_ROUND(SigProc_Fix.Silk_MUL(chirp_Q16, ar[i]), 16);
				chirp_Q16 += SigProc_Fix.Silk_RSHIFT_ROUND(SigProc_Fix.Silk_MUL(chirp_Q16, chirp_minus_one_Q16), 16);
			}

			ar[d - 1] = (opus_int16)SigProc_Fix.Silk_RSHIFT_ROUND(SigProc_Fix.Silk_MUL(chirp_Q16, ar[d - 1]), 16);
		}
	}
}
