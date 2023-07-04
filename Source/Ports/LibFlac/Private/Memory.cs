/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Diagnostics;
using Polycode.NostalgicPlayer.Ports.LibFlac.Share;

namespace Polycode.NostalgicPlayer.Ports.LibFlac.Private
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Memory
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Memory_Alloc_Aligned_Int32_Array(size_t elements, ref Flac__int32[] unaligned_Pointer, ref Flac__int32[] aligned_Pointer)
		{
			Debug.Assert(elements > 0);

			Flac__int32[] pu = Flac__Memory_Alloc_Aligned(elements, out Flac__int32[] _);
			if (pu == null)
				return false;
			else
			{
				unaligned_Pointer = pu;
				aligned_Pointer = pu;

				return true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Memory_Alloc_Aligned_UInt64_Array(size_t elements, ref Flac__uint64[] unaligned_Pointer, ref Flac__uint64[] aligned_Pointer)
		{
			Debug.Assert(elements > 0);

			Flac__uint64[] pu = Flac__Memory_Alloc_Aligned(elements, out Flac__uint64[] _);
			if (pu == null)
				return false;
			else
			{
				unaligned_Pointer = pu;
				aligned_Pointer = pu;

				return true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Memory_Alloc_Aligned_Real_Array(size_t elements, ref Flac__real[] unaligned_Pointer, ref Flac__real[] aligned_Pointer)
		{
			Debug.Assert(elements > 0);

			Flac__real[] pu = Flac__Memory_Alloc_Aligned(elements, out Flac__real[] _);
			if (pu == null)
				return false;
			else
			{
				unaligned_Pointer = pu;
				aligned_Pointer = pu;

				return true;
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static T[] Flac__Memory_Alloc_Aligned<T>(size_t elements, out T[] aligned_Address) where T : new()
		{
			T[] x = Alloc.Safe_MAlloc<T>(elements);
			aligned_Address = x;

			return x;
		}
		#endregion
	}
}
