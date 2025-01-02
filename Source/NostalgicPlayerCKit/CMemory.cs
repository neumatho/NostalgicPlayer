/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using System.Text;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.CKit
{
	/// <summary>
	/// C like memory methods
	/// </summary>
	public static class CMemory
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> MAlloc<T>(int size) where T : struct
		{
			return CAlloc<T>(size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> MAllocObj<T>(int size) where T : new()
		{
			return CAllocObj<T>(size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> CAlloc<T>(int size) where T : struct
		{
			return new CPointer<T>(size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> CAllocObj<T>(int size) where T : new()
		{
			T[] array = ArrayHelper.InitializeArray<T>(size);

			return new CPointer<T>(array);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> Realloc<T>(CPointer<T> ptr, int newSize) where T : struct
		{
			if (ptr.IsNull)
				return MAlloc<T>(newSize);

			if (newSize <= ptr.Length)
				return ptr;

			T[] newArray = new T[newSize];
			Array.Copy(ptr.Buffer, ptr.Offset, newArray, 0, Math.Min(newSize, ptr.Length));

			return new CPointer<T>(newArray);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> ReallocObj<T>(CPointer<T> ptr, int newSize) where T : new()
		{
			if (ptr.IsNull)
				return MAllocObj<T>(newSize);

			if (newSize <= ptr.Length)
				return ptr;

			T[] newArray = new T[newSize];
			int copyLength = Math.Min(newSize, ptr.Length);
			Array.Copy(ptr.Buffer, ptr.Offset, newArray, 0, copyLength);

			for (int i = copyLength; i < newSize; i++)
				newArray[i] = new T();

			return new CPointer<T>(newArray);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Free<T>(CPointer<T> ptr)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemMove<T>(CPointer<T> dest, CPointer<T> source, int length)
		{
			if (length > 0)
				Array.Copy(source.Buffer, source.Offset, dest.Buffer, dest.Offset, length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemCpy<T>(CPointer<T> dest, CPointer<T> source, int length)
		{
			if (length > 0)
				Array.Copy(source.Buffer, source.Offset, dest.Buffer, dest.Offset, length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemCpy<T>(T[] dest, string str, int length)
		{
			if (length > 0)
				Array.Copy(Encoding.Latin1.GetBytes(str), dest, length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int MemCmp(CPointer<byte> ptr1, CPointer<byte> ptr2, int length)
		{
			for (int i = 0; i < length; i++)
			{
				if (ptr1[i] < ptr2[i])
					return -1;

				if (ptr1[i] > ptr2[i])
					return 1;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int MemCmp(CPointer<byte> ptr, string compareString, int length)
		{
			return MemCmp(ptr, Encoding.Latin1.GetBytes(compareString), length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int StrCmp(CPointer<byte> ptr, string compareString)
		{
			byte[] strBuf = Encoding.Latin1.GetBytes(compareString);
			return MemCmp(ptr, strBuf, strBuf.Length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemSet<T>(CPointer<T> ptr, T value, int length)
		{
			ptr.Buffer.AsSpan(ptr.Offset, length).Fill(value);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> MemChr<T>(CPointer<T> ptr, T ch, int count)
		{
			int searchLength = Math.Min(count, ptr.Length);

			for (int i = 0; i < searchLength; i++)
			{
				if (ptr[i].Equals(ch))
					return new CPointer<T>(ptr.Buffer, ptr.Offset + i);
			}

			return null;
		}
	}
}
