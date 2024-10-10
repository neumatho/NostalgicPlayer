/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// Insertion sort (fast for already almost sorted arrays):
	/// Best case:  O(n)   for an already sorted array
	/// Worst case: O(n^2) for an inversely sorted array
	///
	/// Shell short: https://en.wikipedia.org/wiki/Shell_sort
	/// </summary>
	internal static class Sort
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Silk_Insertion_Sort_Increasing_All_Values_Int16(Pointer<opus_int16> a, opus_int L)
		{
			// Sort vector elements by value, increasing order
			for (opus_int i = 1; i < L; i++)
			{
				opus_int j;
				opus_int value = a[i];

				for (j = i - 1; (j >= 0) && (value < a[j]); j--)
					a[j + 1] = a[j];    // Shift value

				a[j + 1] = (opus_int16)value;	// Write value
			}
		}
	}
}
