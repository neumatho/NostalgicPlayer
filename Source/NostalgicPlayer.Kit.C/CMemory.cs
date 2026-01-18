/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Kit.C
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
		public static CPointer<T> malloc<T>(size_t size) where T : struct
		{
			return calloc<T>(size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> mallocObj<T>(size_t size) where T : class, new()
		{
			return callocObj<T>(size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> calloc<T>(size_t size) where T : struct
		{
			return new CPointer<T>(size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> callocObj<T>(size_t size) where T : class, new()
		{
			T[] array = ArrayHelper.InitializeArray<T>((int)size);

			return new CPointer<T>(array);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> realloc<T>(CPointer<T> ptr, size_t newSize) where T : struct
		{
			if (ptr.IsNull)
				return malloc<T>(newSize);

			if (newSize <= (size_t)ptr.Length)
				return ptr;

			T[] newArray = new T[newSize];
			Array.Copy(ptr.Buffer, ptr.Offset, newArray, 0, Math.Min((int)newSize, ptr.Length));

			return new CPointer<T>(newArray);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> reallocObj<T>(CPointer<T> ptr, size_t newSize) where T : class, new()
		{
			if (ptr.IsNull)
				return mallocObj<T>(newSize);

			if (newSize <= (size_t)ptr.Length)
				return ptr;

			T[] newArray = new T[newSize];
			size_t copyLength = Math.Min(newSize, (size_t)ptr.Length);
			Array.Copy(ptr.Buffer, ptr.Offset, newArray, 0, (int)copyLength);

			for (size_t i = copyLength; i < newSize; i++)
				newArray[i] = new T();

			return new CPointer<T>(newArray);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void free<T>(CPointer<T> ptr)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void memmove<T>(CPointer<T> dest, CPointer<T> source, size_t length)
		{
			if (length > 0)
				Array.Copy(source.Buffer, source.Offset, dest.Buffer, dest.Offset, (int)length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void memcpy<T>(CPointer<T> dest, CPointer<T> source, size_t length)
		{
			if (length > 0)
				Array.Copy(source.Buffer, source.Offset, dest.Buffer, dest.Offset, (int)length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void memcpy<T>(CPointer<T> dest, T[] source, size_t length)
		{
			if (length > 0)
				Array.Copy(source, 0, dest.Buffer, dest.Offset, (int)length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void memcpy<T>(CPointer<T> dest, Span<T> source, size_t length)
		{
			if (length > 0)
				source.Slice((int)length).CopyTo(dest.AsSpan());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void memcpyCast<T1, T2>(CPointer<T1> dest, CPointer<T2> source, size_t length) where T1 : struct where T2 : struct
		{
			if (length > 0)
				source.AsSpan().Slice(0, (c_int)length).CopyTo(MemoryMarshal.Cast<T1, T2>(dest.AsSpan()));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void memcpy<T>(T[] dest, string str, size_t length)
		{
			if (length > 0)
				Array.Copy(Encoding.Latin1.GetBytes(str), dest, (int)length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int memcmp<T>(CPointer<T> ptr1, CPointer<T> ptr2, size_t length) where T : struct, IComparable
		{
			for (size_t i = 0; i < length; i++)
			{
				if (ptr1[i].CompareTo(ptr2[i]) < 0)
					return -1;

				if (ptr1[i].CompareTo(ptr2[i]) > 0)
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
		public static int memcmp<T>(CPointer<T> ptr1, T[] ptr2, size_t length) where T : struct, IComparable
		{
			for (size_t i = 0; i < length; i++)
			{
				if (ptr1[i].CompareTo(ptr2[i]) < 0)
					return -1;

				if (ptr1[i].CompareTo(ptr2[i]) > 0)
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
		public static int memcmp(CPointer<byte> ptr, string compareString, size_t length)
		{
			return memcmp(ptr, compareString.ToPointer(), length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int memcmp(string compareString, CPointer<byte> ptr, size_t length)
		{
			return memcmp(compareString.ToPointer(), ptr, length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int strcmp(CPointer<byte> ptr, string compareString)
		{
			CPointer<byte> comparePtr = compareString.ToPointer();
			return memcmp(ptr, comparePtr, (size_t)comparePtr.Length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int strncmp(CPointer<byte> ptr, string compareString, size_t length)
		{
			return memcmp(ptr, compareString.ToPointer(), length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void memset<T>(CPointer<T> ptr, T value, size_t length)
		{
			ptr.Buffer.AsSpan(ptr.Offset, (int)length).Fill(value);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> memchr<T>(CPointer<T> ptr, T ch, size_t count)
		{
			size_t searchLength = Math.Min(count, (size_t)ptr.Length);

			for (size_t i = 0; i < searchLength; i++)
			{
				if (ptr[i].Equals(ch))
					return ptr + i;
			}

			return null;
		}
	}
}
