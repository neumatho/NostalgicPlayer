/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class QSort
	{
		/// <summary>
		/// 
		/// </summary>
		public delegate c_int Sort_Delegate<T>(T a, T b);

		/********************************************************************/
		/// <summary>
		/// Quicksort
		/// This sort is fast, and fully inplace but not stable and it is
		/// possible to construct input that requires O(n^2) time but this is
		/// very unlikely to happen with non constructed input
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Av_QSort<T>(CPointer<T> p, c_int num, Sort_Delegate<T> cmp)//XX 33
		{
			CPointer<T>[][] stack = ArrayHelper.Initialize2Arrays<CPointer<T>>(64, 2);
			c_int sp = 1;

			stack[0][0] = p;
			stack[0][1] = p + num - 1;

			while (sp != 0)
			{
				CPointer<T> start = stack[--sp][0];
				CPointer<T> end = stack[sp][1];

				while (start < end)
				{
					if (start < (end - 1))
					{
						c_int checkSort = 0;
						CPointer<T> right = end - 2;
						CPointer<T> left = start + 1;
						CPointer<T> mid = start + ((end - start) >> 1);

						if (cmp(start[0], end[0]) > 0)
						{
							if (cmp(end[0], mid[0]) > 0)
								Macros.FFSwap(ref start[0], ref mid[0]);
							else
								Macros.FFSwap(ref start[0], ref end[0]);
						}
						else
						{
							if (cmp(start[0], mid[0]) > 0)
								Macros.FFSwap(ref start[0], ref mid[0]);
							else
								checkSort = 1;
						}

						if (cmp(mid[0], end[0]) > 0)
						{
							Macros.FFSwap(ref mid[0], ref end[0]);
							checkSort = 0;
						}

						if (start == (end - 2))
							break;

						Macros.FFSwap(ref end[-1], ref mid[0]);

						while (left <= right)
						{
							while ((left <= right) && (cmp(left[0], end[-1]) < 0))
								left++;

							while ((left <= right) && (cmp(right[0], end[-1]) > 0))
								right--;

							if (left <= right)
							{
								Macros.FFSwap(ref left[0], ref right[0]);
								left++;
								right--;
							}
						}

						Macros.FFSwap(ref end[-1], ref left[0]);

						if ((checkSort != 0) && ((mid == (left - 1)) || (mid == left)))
						{
							mid = start;

							while ((mid < end) && (cmp(mid[0], mid[1]) <= 0))
								mid++;

							if (mid == end)
								break;
						}

						if ((end - left) < (left - start))
						{
							stack[sp][0] = start;
							stack[sp++][1] = right;

							start = left + 1;
						}
						else
						{
							stack[sp][0] = left + 1;
							stack[sp++][1] = end;

							end = right;
						}
					}
					else
					{
						if (cmp(start[0], end[0]) > 0)
							Macros.FFSwap(ref start[0], ref end[0]);

						break;
					}
				}
			}
		}
	}
}
