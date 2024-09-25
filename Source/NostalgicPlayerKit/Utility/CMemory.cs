/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Polycode.NostalgicPlayer.Kit.Utility
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
		public static Pointer<T> MAlloc<T>(int size)
		{
			return new Pointer<T>(size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Pointer<T> Realloc<T>(Pointer<T> ptr, int newSize)
		{
			T[] newArray = new T[newSize];
			Array.Copy(ptr.Buffer, ptr.Offset, newArray, 0, Math.Min(newSize, ptr.Buffer.Length - ptr.Offset));

			return new Pointer<T>(newArray);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Free<T>(Pointer<T> ptr)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemMove<T>(Pointer<T> dest, Pointer<T> source, int length)
		{
			Array.Copy(source.Buffer, source.Offset, dest.Buffer, dest.Offset, length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemCpy<T>(Pointer<T> dest, Pointer<T> source, int length)
		{
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
			Array.Copy(Encoding.ASCII.GetBytes(str), dest, length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int MemCmp(Pointer<byte> ptr1, Pointer<byte> ptr2, int length)
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
		public static int MemCmp(Pointer<byte> ptr1, string compareString, int length)
		{
			return MemCmp(ptr1, Encoding.ASCII.GetBytes(compareString), length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MemSet<T>(Pointer<T> ptr, T value, int length)
		{
			ptr.Buffer.AsSpan(ptr.Offset, length).Fill(value);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Pointer<T> MemChr<T>(Pointer<T> ptr, T ch, int count)
		{
			int searchLength = Math.Min(count, ptr.Buffer.Length - ptr.Offset);

			for (int i = 0; i < searchLength; i++)
			{
				if (ptr[i].Equals(ch))
					return new Pointer<T>(ptr.Buffer, ptr.Offset + i);
			}

			return null;
		}
	}
}
