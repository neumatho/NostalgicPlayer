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
	internal static class LPC_Fit
	{
		/********************************************************************/
		/// <summary>
		/// Convert int32 coefficients to int16 coefs and make sure there's
		/// no wrap-around. This logic is reused in _celt_lpc(). Any bug
		/// fixes should also be applied there
		/// </summary>
		/********************************************************************/
		public static void Silk_LPC_Fit(CPointer<opus_int16> a_QOUT, CPointer<opus_int32> a_QIN, opus_int QOUT, opus_int QIN, opus_int d)
		{
			opus_int i, idx = 0;

			// Limit the maximum absolute value of the prediction coefficients, so that they'll fit in int16
			for (i = 0; i < 10; i++)
			{
				// Find maximum absolute value and its index
				opus_int32 maxabs = 0;

				for (opus_int k = 0; k < d; k++)
				{
					opus_int32 absval = SigProc_Fix.Silk_Abs(a_QIN[k]);

					if (absval > maxabs)
					{
						maxabs = absval;
						idx = k;
					}
				}

				maxabs = SigProc_Fix.Silk_RSHIFT_ROUND(maxabs, QIN - QOUT);

				if (maxabs > opus_int16.MaxValue)
				{
					// Reduce magnitude of prediction coefficients
					maxabs = SigProc_Fix.Silk_Min(maxabs, 163838);	// (silk_int32_MAX >> 14) + silk_int16_MAX = 163838
					opus_int32 chirp_Q16 = SigProc_Fix.Silk_FIX_CONST(0.999f, 16) - SigProc_Fix.Silk_DIV32(SigProc_Fix.Silk_LSHIFT(maxabs - opus_int16.MaxValue, 14), 
						SigProc_Fix.Silk_RSHIFT32(SigProc_Fix.Silk_MUL(maxabs, idx + 1), 2));
					Bwexpander_32.Silk_Bwexpander_32(a_QIN, d, chirp_Q16);
				}
				else
					break;
			}

			if (i == 10)
			{
				// Reached the last iteration, clip the coefficients
				for (opus_int k = 0; k < d; k++)
				{
					a_QOUT[k] = (opus_int16)SigProc_Fix.Silk_SAT16(SigProc_Fix.Silk_RSHIFT_ROUND(a_QIN[k], QIN - QOUT));
					a_QIN[k] = SigProc_Fix.Silk_LSHIFT(a_QOUT[k], QIN - QOUT);
				}
			}
			else
			{
				for (opus_int k = 0; k < d; k++)
					a_QOUT[k] = (opus_int16)SigProc_Fix.Silk_RSHIFT_ROUND(a_QIN[k], QIN - QOUT);
			}
		}
	}
}
