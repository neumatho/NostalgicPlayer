/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class LPC_Inv_Pred_Gain
	{
		private const int QA = 24;
		private static readonly int A_Limit = SigProc_Fix.Silk_FIX_CONST(0.99975f, QA);

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static opus_int32 MUL32_FRAC_Q(opus_int32 a32, opus_int32 b32, opus_int Q)
		{
			return (opus_int32)SigProc_Fix.Silk_RSHIFT_ROUND64(SigProc_Fix.Silk_SMULL(a32, b32), Q);
		}



		/********************************************************************/
		/// <summary>
		/// Compute inverse of LPC prediction gain, and test if LPC
		/// coefficients are stable (all poles within unit circle)
		/// </summary>
		/********************************************************************/
		private static opus_int32 LPC_Inverse_Pred_Gain_QA_C(Pointer<opus_int32> A_QA, opus_int order)
		{
			opus_int k;
			opus_int32 rc_Q31, rc_mult1_Q30;
			opus_int32 invGain_Q30 = SigProc_Fix.Silk_FIX_CONST(1, 30);

			for (k = order - 1; k > 0; k--)
			{
				// Check for stability
				if ((A_QA[k] > A_Limit) || (A_QA[k] < -A_Limit))
					return 0;

				// Set RC equal to negated AR coef
				rc_Q31 = -SigProc_Fix.Silk_LSHIFT(A_QA[k], 31 - QA);

				// rc_mult1_Q30 range: [ 1 : 2^30 ]
				rc_mult1_Q30 = SigProc_Fix.Silk_SUB32(SigProc_Fix.Silk_FIX_CONST(1, 30), SigProc_Fix.Silk_SMMUL(rc_Q31, rc_Q31));

				// Update inverse gain
				// invGain_Q30 range: [ 0 : 2^30 ]
				invGain_Q30 = SigProc_Fix.Silk_LSHIFT(SigProc_Fix.Silk_SMMUL(invGain_Q30, rc_mult1_Q30), 2);

				if (invGain_Q30 < SigProc_Fix.Silk_FIX_CONST(1.0f / Constants.Max_Prediction_Power_Gain, 30))
					return 0;

				// rc_mult2 range: [ 2^30 : silk_int32_MAX ]
				opus_int mult2Q = 32 - Macros.Silk_CLZ32(SigProc_Fix.Silk_Abs(rc_mult1_Q30));
				opus_int32 rc_mult2 = Inlines.Silk_INVERSE32_VarQ(rc_mult1_Q30, mult2Q + 30);

				// Update AR coefficient
				for (opus_int n = 0; n < ((k + 1) >> 1); n++)
				{
					opus_int32 tmp1 = A_QA[n];
					opus_int32 tmp2 = A_QA[k - n - 1];
					opus_int64 tmp64 = SigProc_Fix.Silk_RSHIFT_ROUND64(SigProc_Fix.Silk_SMULL(Macros.Silk_SUB_SAT32((opus_uint32)tmp1, (opus_uint32)MUL32_FRAC_Q(tmp2, rc_Q31, 31)), rc_mult2), mult2Q);

					if ((tmp64 > opus_int32.MaxValue) || (tmp64 < opus_int32.MinValue))
						return 0;

					A_QA[n] = (opus_int32)tmp64;

					tmp64 = SigProc_Fix.Silk_RSHIFT_ROUND64(SigProc_Fix.Silk_SMULL(Macros.Silk_SUB_SAT32((opus_uint32)tmp2, (opus_uint32)MUL32_FRAC_Q(tmp1, rc_Q31, 31)), rc_mult2), mult2Q);

					if ((tmp64 > opus_int32.MaxValue) || (tmp64 < opus_int32.MinValue))
						return 0;

					A_QA[k - n - 1] = (opus_int32)tmp64;
				}
			}

			// Check for stability
			if ((A_QA[k] > A_Limit) || (A_QA[k] < -A_Limit))
				return 0;

			// Set RC equal to negated AR coef
			rc_Q31 = -SigProc_Fix.Silk_LSHIFT(A_QA[0], 31 - QA);

			// rc_mult1_Q30 range: [ 1 : 2^30 ]
			rc_mult1_Q30 = SigProc_Fix.Silk_SUB32(SigProc_Fix.Silk_FIX_CONST(1, 30), SigProc_Fix.Silk_SMMUL(rc_Q31, rc_Q31));

			// Update inverse gain
			// invGain_Q30 range: [ 0 : 2^30 ]
			invGain_Q30 = SigProc_Fix.Silk_LSHIFT(SigProc_Fix.Silk_SMMUL(invGain_Q30, rc_mult1_Q30), 2);

			if (invGain_Q30 < SigProc_Fix.Silk_FIX_CONST(1.0f / Constants.Max_Prediction_Power_Gain, 30))
				return 0;

			return invGain_Q30;
		}



		/********************************************************************/
		/// <summary>
		/// For input in Q12 domain
		/// </summary>
		/********************************************************************/
		private static opus_int32 LPC_Inverse_Pred_Gain_C(Pointer<opus_int16> A_Q12, opus_int order)
		{
			opus_int32[] Atmp_QA = new opus_int32[Constants.Silk_Max_Order_Lpc];
			opus_int32 DC_resp = 0;

			// Increase Q domain of the AR coefficients
			for (opus_int k = 0; k < order; k++)
			{
				DC_resp += A_Q12[k];
				Atmp_QA[k] = SigProc_Fix.Silk_LSHIFT32(A_Q12[k], QA - 12);
			}

			// If the DC is unstable, we don't even need to do the full calculations
			if (DC_resp >= 4096)
				return 0;

			return LPC_Inverse_Pred_Gain_QA_C(Atmp_QA, order);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_LPC_Inverse_Pred_Gain(Pointer<opus_int16> A_Q12, opus_int order, c_int arch)
		{
			return LPC_Inverse_Pred_Gain_C(A_Q12, order);
		}
	}
}
