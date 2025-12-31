/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.C
{
	/// <summary>
	/// Different sort algorithms
	/// </summary>
	public static class CSort
	{
		/// <summary>
		/// 
		/// </summary>
		public delegate c_int Comp_Delegate<T>(T a, T b);

		/********************************************************************/
		/// <summary>
		/// Sorts an array using the quicksort algorithm. This mimics the
		/// behavior of the C standard library qsort function
		/// </summary>
		/********************************************************************/
		public static void qsort<T>(CPointer<T> ptr, size_t count, Comp_Delegate<T> comp)
		{
			if (ptr.IsNull || (count <= 1))
				return;

			QuickSort(ptr, 0, (c_int)count - 1, comp);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Recursive quicksort implementation
		/// </summary>
		/********************************************************************/
		private static void QuickSort<T>(CPointer<T> ptr, c_int low, c_int high, Comp_Delegate<T> comp)
		{
			if (low < high)
			{
				c_int pivotIndex = Partition(ptr, low, high, comp);

				QuickSort(ptr, low, pivotIndex - 1, comp);
				QuickSort(ptr, pivotIndex + 1, high, comp);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Partition the array around a pivot element
		/// </summary>
		/********************************************************************/
		private static c_int Partition<T>(CPointer<T> ptr, c_int low, c_int high, Comp_Delegate<T> comp)
		{
			T pivot = ptr[high];
			c_int i = low - 1;

			for (c_int j = low; j < high; j++)
			{
				if (comp(ptr[j], pivot) <= 0)
				{
					i++;
					Swap(ptr, i, j);
				}
			}

			Swap(ptr, i + 1, high);

			return i + 1;
		}



		/********************************************************************/
		/// <summary>
		/// Swap two elements in the array
		/// </summary>
		/********************************************************************/
		private static void Swap<T>(CPointer<T> ptr, c_int i, c_int j)
		{
			if (i != j)
				(ptr[i], ptr[j]) = (ptr[j], ptr[i]);
		}
		#endregion
	}
}
