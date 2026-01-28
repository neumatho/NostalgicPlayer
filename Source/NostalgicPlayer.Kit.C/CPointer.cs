/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.C
{
	/// <summary>
	/// This holds a buffer and a start offset. Can be used in C ports, where
	/// there are a lot of pointer calculations. By using this, you don't need
	/// to hold the offset by yourself and pass it around.
	///
	/// It is almost similar to Span, except that with this, you can also use
	/// negative indexes to retrieve the data, which is used by some C programs
	/// </summary>
	public struct CPointer<T> : IPointer, IEquatable<CPointer<T>>, IComparable<CPointer<T>>, IClearable, IDeepCloneable<CPointer<T>>
	{
		#region CastMemoryManager class
		private sealed class CastMemoryManager<TFrom, TTo> : MemoryManager<TTo> where TFrom : unmanaged where TTo : unmanaged
		{
			private readonly Memory<TFrom> fromMemory;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public CastMemoryManager(Memory<TFrom> from)
			{
				fromMemory = from;
			}



			/********************************************************************/
			/// <summary>
			/// Do the casting
			/// </summary>
			/********************************************************************/
			public override Span<TTo> GetSpan()
			{
				return MemoryMarshal.Cast<TFrom, TTo>(fromMemory.Span);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			protected override void Dispose(bool disposing)
			{
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public override MemoryHandle Pin(int elementIndex = 0)
			{
				throw new NotSupportedException();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public override void Unpin()
			{
				throw new NotSupportedException();
			}
		}
		#endregion

		private Memory<T> internalBuffer;
		private int bufferOffset;
		private bool bufferSet;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CPointer()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CPointer(T[] buffer, int offset)
		{
			if (offset < 0)
				throw new ArgumentOutOfRangeException(nameof(offset), "Offset has to be a positive number");

			internalBuffer = new Memory<T>(buffer);
			bufferOffset = buffer != null ? offset : 0;
			bufferSet = buffer != null;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CPointer(T[] buffer) : this(buffer, 0)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CPointer(int length) : this(new T[length], 0)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CPointer(size_t length) : this(new T[length], 0)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CPointer(CPointer<T> pointer, int offset)
		{
			if (pointer.IsNotNull)
			{
				internalBuffer = pointer.internalBuffer;
				bufferOffset = pointer.bufferOffset + offset;
				bufferSet = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private CPointer(Memory<T> buffer, int offset)
		{
			internalBuffer = buffer;
			bufferOffset = offset;
			bufferSet = true;
		}



		/********************************************************************/
		/// <summary>
		/// Return the offset in the buffer where the first item starts.
		/// Only used by unit tests
		/// </summary>
		/********************************************************************/
		internal int Offset => bufferOffset;



		/********************************************************************/
		/// <summary>
		/// Return the length of the buffer
		/// </summary>
		/********************************************************************/
		public int Length => internalBuffer.Length - bufferOffset;



		/********************************************************************/
		/// <summary>
		/// Clear the pointer
		/// </summary>
		/********************************************************************/
		public void SetToNull()
		{
			internalBuffer = null;
			bufferOffset = 0;
			bufferSet = false;
		}



		/********************************************************************/
		/// <summary>
		/// Check to see if the pointer is null
		/// </summary>
		/********************************************************************/
		public bool IsNull => !bufferSet;



		/********************************************************************/
		/// <summary>
		/// Check to see if the pointer is not null
		/// </summary>
		/********************************************************************/
		public bool IsNotNull => bufferSet;



		/********************************************************************/
		/// <summary>
		/// Convert the pointer to a span
		/// </summary>
		/********************************************************************/
		public Span<T> AsSpan()
		{
			return internalBuffer.Slice(bufferOffset).Span;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the pointer to a span
		/// </summary>
		/********************************************************************/
		public Span<T> AsSpan(int length)
		{
			return internalBuffer.Slice(bufferOffset, length).Span;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the pointer to a span
		/// </summary>
		/********************************************************************/
		public Span<T> AsSpan(int offset, int length)
		{
			return internalBuffer.Slice(bufferOffset + offset, length).Span;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the pointer to a memory
		/// </summary>
		/********************************************************************/
		public Memory<T> AsMemory()
		{
			return internalBuffer.Slice(bufferOffset);
		}



		/********************************************************************/
		/// <summary>
		/// Convert the pointer to a memory
		/// </summary>
		/********************************************************************/
		public Memory<T> AsMemory(int length)
		{
			return internalBuffer.Slice(bufferOffset, length);
		}



		/********************************************************************/
		/// <summary>
		/// Convert the pointer to a memory
		/// </summary>
		/********************************************************************/
		public Memory<T> AsMemory(int offset, int length)
		{
			return internalBuffer.Slice(bufferOffset + offset, length);
		}



		/********************************************************************/
		/// <summary>
		/// Clear the array
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			internalBuffer.Slice(bufferOffset).Span.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// Clear some part of the array
		/// </summary>
		/********************************************************************/
		public void Clear(int length)
		{
			internalBuffer.Slice(bufferOffset, length).Span.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// Return or set the item at the index given
		/// </summary>
		/********************************************************************/
		public ref T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref internalBuffer.Span[bufferOffset + index];
		}



		/********************************************************************/
		/// <summary>
		/// Return or set the item at the index given
		/// </summary>
		/********************************************************************/
		public ref T this[uint index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref internalBuffer.Span[bufferOffset + (int)index];
		}



		/********************************************************************/
		/// <summary>
		/// Return or set the item at the index given
		/// </summary>
		/********************************************************************/
		public ref T this[long index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref internalBuffer.Span[bufferOffset + (int)index];
		}



		/********************************************************************/
		/// <summary>
		/// Return or set the item at the index given
		/// </summary>
		/********************************************************************/
		public ref T this[ulong index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref internalBuffer.Span[bufferOffset + (int)index];
		}



		/********************************************************************/
		/// <summary>
		/// Return or set the item at the index given and increment the
		/// pointer afterwards
		///
		/// Examples of usage:
		///
		/// a = *p++ → a = p[0, 1]
		/// *p++ = a → p[0, 1] = a
		///	*p++ += a → p[0] += a; p++
		/// </summary>
		/********************************************************************/
		public T this[int index, int increment]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				T val = internalBuffer.Span[bufferOffset + index];
				bufferOffset += increment;

				return val;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				internalBuffer.Span[bufferOffset + index] = value;
				bufferOffset += increment;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return a new pointer where the original pointer is incremented by
		/// the value given
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> operator + (CPointer<T> ptr, int increment)
		{
			return new CPointer<T>(ptr.internalBuffer, ptr.bufferOffset + increment);
		}



		/********************************************************************/
		/// <summary>
		/// Return a new pointer where the original pointer is incremented by
		/// the value given
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> operator + (CPointer<T> ptr, uint increment)
		{
			return new CPointer<T>(ptr.internalBuffer, ptr.bufferOffset + (int)increment);
		}



		/********************************************************************/
		/// <summary>
		/// Return a new pointer where the original pointer is incremented by
		/// the value given
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> operator + (CPointer<T> ptr, long increment)
		{
			return new CPointer<T>(ptr.internalBuffer, (int)(ptr.bufferOffset + increment));
		}



		/********************************************************************/
		/// <summary>
		/// Return a new pointer where the original pointer is incremented by
		/// the value given
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> operator + (CPointer<T> ptr, ulong increment)
		{
			return new CPointer<T>(ptr.internalBuffer, (int)(ptr.bufferOffset + (long)increment));
		}



		/********************************************************************/
		/// <summary>
		/// Return a new pointer where the original pointer is decremented by
		/// the value given
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> operator - (CPointer<T> ptr, int decrement)
		{
			return new CPointer<T>(ptr.internalBuffer, ptr.bufferOffset - decrement);
		}



		/********************************************************************/
		/// <summary>
		/// Return a new pointer where the original pointer is decremented by
		/// the value given
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> operator - (CPointer<T> ptr, uint decrement)
		{
			return new CPointer<T>(ptr.internalBuffer, ptr.bufferOffset - (int)decrement);
		}



		/********************************************************************/
		/// <summary>
		/// Return a new pointer where the original pointer is decremented by
		/// the value given
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> operator - (CPointer<T> ptr, long decrement)
		{
			return new CPointer<T>(ptr.internalBuffer, (int)(ptr.bufferOffset - decrement));
		}



		/********************************************************************/
		/// <summary>
		/// Return a new pointer where the original pointer is decremented by
		/// the value given
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> operator - (CPointer<T> ptr, ulong decrement)
		{
			return new CPointer<T>(ptr.internalBuffer, (int)(ptr.bufferOffset - (long)decrement));
		}



		/********************************************************************/
		/// <summary>
		/// Return a new pointer where the original pointer is incremented by
		/// one
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> operator ++ (CPointer<T> ptr)
		{
			return ptr + 1;
		}



		/********************************************************************/
		/// <summary>
		/// Return a new pointer where the original pointer is decremented by
		/// one
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> operator -- (CPointer<T> ptr)
		{
			return ptr - 1;
		}



		/********************************************************************/
		/// <summary>
		/// Will calculate the difference between the two pointers. Both
		/// pointers need to use the same buffer
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int operator - (CPointer<T> ptr1, CPointer<T> ptr2)
		{
			if (ptr1.IsNull && ptr2.IsNotNull)
				return -0x12345678;

			if (ptr1.IsNotNull && ptr2.IsNotNull && (ptr1.GetOriginalArray() != ptr2.GetOriginalArray()))
				throw new ArgumentException("Both pointers need to use the same buffer");

			return ptr1.bufferOffset - ptr2.bufferOffset;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two pointers to see if they point to the same place
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator == (CPointer<T> ptr1, CPointer<T> ptr2)
		{
			return ptr1.Equals(ptr2);
		}



		/********************************************************************/
		/// <summary>
		/// Compare two pointers to see if they don't point to the same place
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator != (CPointer<T> ptr1, CPointer<T> ptr2)
		{
			return !ptr1.Equals(ptr2);
		}



		/********************************************************************/
		/// <summary>
		/// Compare two pointers
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator > (CPointer<T> ptr1, CPointer<T> ptr2)
		{
			return ptr1.CompareTo(ptr2) > 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two pointers
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator < (CPointer<T> ptr1, CPointer<T> ptr2)
		{
			return ptr1.CompareTo(ptr2) < 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two pointers
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >= (CPointer<T> ptr1, CPointer<T> ptr2)
		{
			return ptr1.CompareTo(ptr2) >= 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two pointers
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <= (CPointer<T> ptr1, CPointer<T> ptr2)
		{
			return ptr1.CompareTo(ptr2) <= 0;
		}



		/********************************************************************/
		/// <summary>
		/// Convert an array to a pointer
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator CPointer<T>(T[] array)
		{
			return new CPointer<T>(array);
		}



		/********************************************************************/
		/// <summary>
		/// Case a pointer from one type to another
		/// </summary>
		/********************************************************************/
		public CPointer<TTo> Cast<TFrom, TTo>() where TFrom : unmanaged where TTo : unmanaged
		{
			if (internalBuffer.GetType() == typeof(Memory<TTo>))
				return new CPointer<TTo>((Memory<TTo>)(object)internalBuffer, bufferOffset);

			int newOffset = (int)(((float)Unsafe.SizeOf<TFrom>() / Unsafe.SizeOf<TTo>()) * bufferOffset);

			return new CPointer<TTo>(new CastMemoryManager<TFrom, TTo>((Memory<TFrom>)(object)internalBuffer).Memory, newOffset);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			if (typeof(T) == typeof(char))
			{
				if (IsNull)
					return string.Empty;

				Memory<char> charBuffer = (Memory<char>)(object)internalBuffer;
				CPointer<char> ptr = new CPointer<char>(charBuffer, bufferOffset);
				c_int len = (c_int)CString.strlen(ptr);

				return (len > 0) ? new string(ptr.AsSpan(len).ToArray()) : string.Empty;
			}

			return base.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			throw new NotSupportedException();
		}



		/********************************************************************/
		/// <summary>
		/// This method is not supported as spans cannot be boxed.
		/// </summary>
		/********************************************************************/
		public override int GetHashCode()
		{
			throw new NotSupportedException();
		}

		#region IEquatable implementation
		/********************************************************************/
		/// <summary>
		/// Compare two pointers
		/// </summary>
		/********************************************************************/
		public bool Equals(CPointer<T> other)
		{
			return (GetOriginalArray() == other.GetOriginalArray()) && (bufferOffset == other.bufferOffset);
		}
		#endregion

		#region IComparable implementation
		/********************************************************************/
		/// <summary>
		/// Compare two pointers
		/// </summary>
		/********************************************************************/
		public int CompareTo(CPointer<T> other)
		{
			if (other.IsNull)
				return 1;

			if (IsNull)
				return -1;

			if (GetOriginalArray() != other.GetOriginalArray())
				throw new ArgumentException("Both pointers need to use the same buffer");

			return bufferOffset.CompareTo(other.bufferOffset);
		}
		#endregion

		#region IDeepCloneable implementation
		/********************************************************************/
		/// <summary>
		/// Clone the buffer and pointer
		/// </summary>
		/********************************************************************/
		public CPointer<T> MakeDeepClone()
		{
			if (IsNull)
				return new CPointer<T>();

			return new CPointer<T>(internalBuffer.ToArray(), bufferOffset);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal T[] GetOriginalArray()
		{
			if (MemoryMarshal.TryGetArray(internalBuffer, out ArraySegment<T> segment))
				return segment.Array;

			return null;
		}
		#endregion
	}
}
