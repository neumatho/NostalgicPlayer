/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.C
{
	/// <summary>
	/// C like array methods
	/// </summary>
	public static class CArray
	{
		/// <summary></summary>
		public delegate c_int Compare_Delegate<E, K>(K key, E elem);

		/********************************************************************/
		/// <summary>
		/// Perform a binary search on a sorted array to search for an
		/// element
		/// </summary>
		/********************************************************************/
		public static CPointer<E> bsearch<E, K>(K key, CPointer<E> @base, size_t nItems, Compare_Delegate<E, K> compar)
		{
			c_int left = 0;
			c_int right = (c_int)nItems - 1;

			while (left <= right)
			{
				c_int mid = left + ((right - left) / 2);
				E elem = @base[mid];

				c_int compareResult = compar(key, elem);

				if (compareResult == 0)
					return @base + mid;

				if (compareResult > 0)
					left = mid + 1;
				else
					right = mid - 1;
			}

			return null;
		}
	}
}
