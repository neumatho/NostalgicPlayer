/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class DynArray
	{
		/********************************************************************/
		/// <summary>
		/// Add an element to a dynamic array.
		///
		/// The array is reallocated when its number of elements reaches
		/// powers of 2. Therefore, the amortized cost of adding an element
		/// is constant.
		///
		/// In case of success, the pointer to the array is updated in order
		/// to point to the new grown array, and the size is incremented
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FF_DynArray_Add<T>(size_t av_Size_Max, ref CPointer<T> av_Array, ref size_t av_Size, UtilFunc.Success_Delegate<T> av_Success, UtilFunc.Failure_Delegate<T> av_Failure) where T : class, new()
		{
			size_t av_Size_New = av_Size;

			if ((av_Size & (av_Size - 1)) == 0)
			{
				av_Size_New = av_Size != 0 ? av_Size << 1 : 1;

				if (av_Size_New > av_Size_Max)
					av_Size_New = 0;
				else
				{
					CPointer<T> av_Array_New = Mem.Av_ReallocObj(av_Array, av_Size_New);
					if (av_Array_New.IsNull)
						av_Size_New = 0;
					else
						av_Array = av_Array_New;
				}
			}

			if (av_Size_New != 0)
			{
				av_Success(av_Array, av_Size);
				av_Size++;
			}
			else
				av_Failure(ref av_Array, ref av_Size);
		}
	}
}
