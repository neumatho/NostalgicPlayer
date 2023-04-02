/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Share
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Alloc
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] Safe_MAlloc<T>(size_t size) where T : new()
		{
			if (size == 0)
				size++;

			return ArrayHelper.InitializeArray<T>((int)size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] Safe_CAlloc<T>(size_t size) where T : new()
		{
			if (size == 0)
				size++;

			return ArrayHelper.InitializeArray<T>((int)size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] Safe_MAlloc_Add_2Op<T>(size_t size1, size_t size2) where T : new()
		{
			size2 += size1;
			if (size2 < size1)
				return null;

			return Safe_MAlloc<T>(size2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] Safe_MAlloc_Add_4Op<T>(size_t size1, size_t size2, size_t size3, size_t size4) where T : new()
		{
			size2 += size1;
			if (size2 < size1)
				return null;

			size3 += size2;
			if (size3 < size2)
				return null;

			size4 += size3;
			if (size4 < size3)
				return null;

			return Safe_MAlloc<T>(size4);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] Safe_MAlloc_Mul_2Op<T>(size_t size1, size_t size2) where T : new()
		{
			if ((size1 == 0) || (size2 == 0))
				return ArrayHelper.InitializeArray<T>(1);

			if (size1 > (size_t.MaxValue / size2))
				return null;

			return ArrayHelper.InitializeArray<T>((int)(size1 * size2));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] Safe_Realloc<T>(T[] ptr, size_t size) where T : new()
		{
			T[] oldPtr = ptr;
			T[] newPtr = ptr;

			Array.Resize(ref newPtr, (int)size);

			if ((oldPtr == null) || (size > oldPtr.Length))
			{
				for (int i = oldPtr?.Length ?? 0; i < size; i++)
					newPtr[i] = new T();
			}

			return newPtr;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] Safe_Realloc_Add_2Op<T>(T[] ptr, size_t size1, size_t size2) where T : new()
		{
			size2 += size1;
			if (size2 < size1)
				return null;

			return Safe_Realloc(ptr, size2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] Safe_Realloc_Mul_2Op<T>(T[] ptr, size_t size1, size_t size2) where T : new()
		{
			if ((size1 == 0) || (size2 == 0))
			{
				Array.Resize(ref ptr, 0);
				return ptr;
			}

			return Safe_Realloc(ptr, size1 * size2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T[] Safe_MAlloc_Mul_2Op_P<T>(size_t size1, size_t size2) where T : new()
		{
			if ((size1 == 0) || (size2 == 0))
				return ArrayHelper.InitializeArray<T>(1);

			if (size1 > (size_t.MaxValue / size2))
				return null;

			return ArrayHelper.InitializeArray<T>((int)(size1 * size2));
		}
	}
}
