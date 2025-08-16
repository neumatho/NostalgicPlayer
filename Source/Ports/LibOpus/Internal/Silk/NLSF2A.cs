/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// Conversion between prediction filter coefficients and LSFs
	/// order should be even
	/// a piecewise linear approximation maps LSF - cos(LSF)
	/// therefore the result is not accurate LSFs, but the two
	/// functions are accurate inverses of each other
	/// </summary>
	internal static class NLSF2A
	{
		// This ordering was found to maximize quality. It improves numerical accuracy of
		// silk_NLSF2A_find_poly() compared to "standard" ordering
		private static readonly byte[] ordering16 =
		[
			0, 15, 8, 7, 4, 11, 12, 3, 2, 13, 10, 5, 6, 9, 14, 1
		];

		private static readonly byte[] ordering10 =
		[
			0, 9, 6, 3, 4, 5, 8, 1, 2, 7
		];

		private const int QA = 16;

		/********************************************************************/
		/// <summary>
		/// Helper function for NLSF2A
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Silk_NLSF2A_Find_Poly(CPointer<opus_int32> _out, CPointer<opus_int32> cLSF, opus_int dd)
		{
			_out[0] = SigProc_Fix.Silk_LSHIFT(1, QA);
			_out[1] = -cLSF[0];

			for (opus_int k = 1; k < dd; k++)
			{
				opus_int32 ftmp = cLSF[2 * k];  // QA
				_out[k + 1] = SigProc_Fix.Silk_LSHIFT(_out[k - 1], 1) - (opus_int32)SigProc_Fix.Silk_RSHIFT_ROUND64(SigProc_Fix.Silk_SMULL(ftmp, _out[k]), QA);

				for (opus_int n = k; n > 1; n--)
					_out[n] += _out[n - 2] - (opus_int32)SigProc_Fix.Silk_RSHIFT_ROUND64(SigProc_Fix.Silk_SMULL(ftmp, _out[n - 1]), QA);

				_out[1] -= ftmp;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Compute whitening filter coefficients from normalized line
		/// spectral frequencies
		/// </summary>
		/********************************************************************/
		public static void Silk_NLSF2A(CPointer<opus_int16> a_Q12, CPointer<opus_int16> NLSF, opus_int d, c_int arch)
		{
			CPointer<opus_int32> cos_LSF_QA = new CPointer<opus_int32>(Constants.Silk_Max_Order_Lpc);
			opus_int32[] P = new opus_int32[Constants.Silk_Max_Order_Lpc / 2 + 1];
			opus_int32[] Q = new opus_int32[Constants.Silk_Max_Order_Lpc / 2 + 1];
			opus_int32[] a32_QA1 = new opus_int32[Constants.Silk_Max_Order_Lpc];

			// Convert LSFs to 2*cos(LSF), using piecewise linear curve from table
			byte[] ordering = d == 16 ? ordering16 : ordering10;

			for (opus_int k = 0; k < d; k++)
			{
				// f_int on a scale 0-127 (rounded down)
				opus_int32 f_int = SigProc_Fix.Silk_RSHIFT(NLSF[k], 15 - 7);

				// f_frac, range: 0..255
				opus_int32 f_frac = NLSF[k] - SigProc_Fix.Silk_LSHIFT(f_int, 15 - 7);

				// Read start and end value from table
				opus_int32 cos_val = Table_LSF_Cos.Silk_LSFCosTab_FIX_Q12[f_int];				// Q12
				opus_int32 delta = Table_LSF_Cos.Silk_LSFCosTab_FIX_Q12[f_int + 1] - cos_val;	// Q12, with a range of 0..200

				// Linear interpolation
				cos_LSF_QA[ordering[k]] = SigProc_Fix.Silk_RSHIFT_ROUND(SigProc_Fix.Silk_LSHIFT(cos_val, 8) + SigProc_Fix.Silk_MUL(delta, f_frac), 20 - QA);		// QA
			}

			opus_int dd = SigProc_Fix.Silk_RSHIFT(d, 1);

			// Generate even and odd polynomials using convolution
			Silk_NLSF2A_Find_Poly(P, cos_LSF_QA, dd);
			Silk_NLSF2A_Find_Poly(Q, cos_LSF_QA + 1, dd);

			// Convert even and odd polynomials to opus_int32 Q12 filter coefs
			for (opus_int k = 0; k < dd; k++)
			{
				opus_int32 Ptmp = P[k + 1] + P[k];
				opus_int32 Qtmp = Q[k + 1] - Q[k];

				// The Ptmp and Qtmp values at this stage need to fit in int32
				a32_QA1[k] = -Qtmp - Ptmp;			// QA+1
				a32_QA1[d - k - 1] = Qtmp - Ptmp;	// QA+1
			}

			// Convert int32 coefficients to Q12 int16 coefs
			LPC_Fit.Silk_LPC_Fit(a_Q12, a32_QA1, 12, QA + 1, d);

			for (opus_int i = 0; (LPC_Inv_Pred_Gain.Silk_LPC_Inverse_Pred_Gain(a_Q12, d, arch) == 0) && (i < Constants.Max_Lpc_Stabilize_Iterations); i++)
			{
				// Prediction coefficients are (too close to) unstable; apply bandwidth expansion
				// on the unscaled coefficients, convert to Q12 and measure again
				Bwexpander_32.Silk_Bwexpander_32(a32_QA1, d, 65536 - SigProc_Fix.Silk_LSHIFT(2, i));

				for (opus_int k = 0; k < d; k++)
					a_Q12[k] = (opus_int16)SigProc_Fix.Silk_RSHIFT_ROUND(a32_QA1[k], QA + 1 - 12);		// QA+1 -> Q12
			}
		}
	}
}
