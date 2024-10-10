/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// NLSF stabilizer:
	///
	/// - Moves NLSFs further apart if they are too close
	/// - Moves NLSFs away from borders if they are too close
	/// - High effort to achieve a modification with minimum
	///     Euclidean distance to input vector
	/// - Output are sorted NLSF coefficients
	/// </summary>
	internal class NLSF_Stabilize
	{
		private const int Max_Loops = 20;

		/********************************************************************/
		/// <summary>
		/// NLSF stabilizer, for a single input data vector
		/// </summary>
		/********************************************************************/
		public static void Silk_NLSF_Stabilize(Pointer<opus_int16> NLSF_Q15, Pointer<opus_int16> NDeltaMin_Q15, opus_int L)
		{
			opus_int I = 0, loops;

			for (loops = 0; loops < Max_Loops; loops++)
			{
				/**************************/
				/* Find smallest distance */
				/**************************/
				// First element
				opus_int32 min_diff_Q15 = NLSF_Q15[0] - NDeltaMin_Q15[0];
				I = 0;

				// Middle elements
				opus_int32 diff_Q15;
				for (opus_int i = 1; i <= (L - 1); i++)
				{
					diff_Q15 = NLSF_Q15[i] - (NLSF_Q15[i - 1] + NDeltaMin_Q15[i]);

					if (diff_Q15 < min_diff_Q15)
					{
						min_diff_Q15 = diff_Q15;
						I = i;
					}
				}

				// Last element
				diff_Q15 = (1 << 15) - (NLSF_Q15[L - 1] + NDeltaMin_Q15[L]);

				if (diff_Q15 < min_diff_Q15)
				{
					min_diff_Q15 = diff_Q15;
					I = L;
				}

				/***************************************************/
				/* Now check if the smallest distance non-negative */
				/***************************************************/
				if (min_diff_Q15 >= 0)
					return;

				if (I == 0)
				{
					// Move away from lower limit
					NLSF_Q15[0] = NDeltaMin_Q15[0];
				}
				else if (I == L)
				{
					// Move away from higher limit
					NLSF_Q15[L - 1] = (opus_int16)((1 << 15) - NDeltaMin_Q15[L]);
				}
				else
				{
					// Find the lower extreme for the location of the current center frequency
					opus_int32 min_center_Q15 = 0;

					for (opus_int k = 0; k < I; k++)
						min_center_Q15 += NDeltaMin_Q15[k];

					min_center_Q15 += SigProc_Fix.Silk_RSHIFT(NDeltaMin_Q15[I], 1);

					// Find the upper extreme for the location of the current center frequency
					opus_int32 max_center_Q15 = 1 << 15;

					for (opus_int k = L; k > I; k--)
						max_center_Q15 -= NDeltaMin_Q15[k];

					min_center_Q15 -= SigProc_Fix.Silk_RSHIFT(NDeltaMin_Q15[I], 1);

					// Move apart, sorted by value, keeping the same center frequency
					opus_int16 center_freq_Q15 = (opus_int16)SigProc_Fix.Silk_LIMIT_32(SigProc_Fix.Silk_RSHIFT_ROUND(NLSF_Q15[I - 1] + NLSF_Q15[I], 1), min_center_Q15, max_center_Q15);
					NLSF_Q15[I - 1] = (opus_int16)(center_freq_Q15 - SigProc_Fix.Silk_RSHIFT(NDeltaMin_Q15[I], 1));
					NLSF_Q15[I] = (opus_int16)(NLSF_Q15[I - 1] + NDeltaMin_Q15[I]);
				}
			}

			// Safe and simple fall back method, which is less ideal than the above
			if (loops == Max_Loops)
			{
				// Insertion sort (fast for already almost sorted arrays):
				// Best case:  O(n)   for an already sorted array
				// Worst case: O(n^2) for an inversely sorted array
				Sort.Silk_Insertion_Sort_Increasing_All_Values_Int16(NLSF_Q15, L);

				// First NLSF should be no less than NDeltaMin[0]
				NLSF_Q15[0] = (opus_int16)SigProc_Fix.Silk_Max_Int(NLSF_Q15[0], NDeltaMin_Q15[0]);

				// Keep delta_min distance between the NLSFs
				for (opus_int i = 1; i < L; i++)
					NLSF_Q15[i] = (opus_int16)SigProc_Fix.Silk_Max_Int(NLSF_Q15[i], SigProc_Fix.Silk_ADD_SAT16(NLSF_Q15[i - 1], NDeltaMin_Q15[i]));

				// Last NLSF should be no higher than 1 - NDeltaMin[L]
				NLSF_Q15[L - 1] = (opus_int16)SigProc_Fix.Silk_Min_Int(NLSF_Q15[L - 1], (1 << 15) - NDeltaMin_Q15[L]);

				// Keep NDeltaMin distance between the NLSFs
				for (opus_int i = L - 2; i >= 0; i--)
					NLSF_Q15[i] = (opus_int16)SigProc_Fix.Silk_Min_Int(NLSF_Q15[i], NLSF_Q15[i + 1] - NDeltaMin_Q15[i + 1]);
			}
		}
	}
}
