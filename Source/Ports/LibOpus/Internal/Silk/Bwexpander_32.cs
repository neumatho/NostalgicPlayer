/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Bwexpander_32
	{
		/********************************************************************/
		/// <summary>
		/// Chirp (bandwidth expand) LP AR filter.
		/// This logic is reused in _celt_lpc(). Any bug fixes should also
		/// be applied there
		/// </summary>
		/********************************************************************/
		public static void Silk_Bwexpander_32(CPointer<opus_int32> ar, opus_int d, opus_int32 chirp_Q16)
		{
			opus_int32 chirp_minus_one_Q16 = chirp_Q16 - 65536;

			for (opus_int i = 0; i < (d - 1); i++)
			{
				ar[i] = Macros.Silk_SMULWW(chirp_Q16, ar[i]);
				chirp_Q16 += SigProc_Fix.Silk_RSHIFT_ROUND(SigProc_Fix.Silk_MUL(chirp_Q16, chirp_minus_one_Q16), 16);
			}

			ar[d - 1] = Macros.Silk_SMULWW(chirp_Q16, ar[d - 1]);
		}
	}
}
