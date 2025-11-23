/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.Utility;
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
	public struct CPointer<T> : IEquatable<CPointer<T>>, IComparable<CPointer<T>>, IClearable, IDeepCloneable<CPointer<T>>
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CPointer(T[] buffer, int offset)
		{
			Buffer = buffer;
			Offset = offset;
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
		/// Return the whole buffer
		/// </summary>
		/********************************************************************/
		public T[] Buffer { get; private set; }



		/********************************************************************/
		/// <summary>
		/// Return the offset in the buffer where the first item starts
		/// </summary>
		/********************************************************************/
		public int Offset { get; private set; }



		/********************************************************************/
		/// <summary>
		/// Return the length of the buffer
		/// </summary>
		/********************************************************************/
		public int Length => Buffer.Length - Offset;



		/********************************************************************/
		/// <summary>
		/// Clear the pointer
		/// </summary>
		/********************************************************************/
		public void SetToNull()
		{
			Buffer = null;
			Offset = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Check to see if the pointer is null
		/// </summary>
		/********************************************************************/
		public bool IsNull => Buffer == null;



		/********************************************************************/
		/// <summary>
		/// Check to see if the pointer is not null
		/// </summary>
		/********************************************************************/
		public bool IsNotNull => Buffer != null;



		/********************************************************************/
		/// <summary>
		/// Convert the pointer to a span
		/// </summary>
		/********************************************************************/
		public Span<T> AsSpan()
		{
			return new Span<T>(Buffer, Offset, Length);
		}



		/********************************************************************/
		/// <summary>
		/// Clear the array
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Array.Clear(Buffer, Offset, Length);
		}



		/********************************************************************/
		/// <summary>
		/// Clear some part of the array
		/// </summary>
		/********************************************************************/
		public void Clear(int length)
		{
			Array.Clear(Buffer, Offset, length);
		}



		/********************************************************************/
		/// <summary>
		/// Return or set the item at the index given
		/// </summary>
		/********************************************************************/
		public ref T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref Buffer[Offset + index];
		}



		/********************************************************************/
		/// <summary>
		/// Return or set the item at the index given
		/// </summary>
		/********************************************************************/
		public ref T this[uint index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref Buffer[Offset + index];
		}



		/********************************************************************/
		/// <summary>
		/// Return or set the item at the index given
		/// </summary>
		/********************************************************************/
		public ref T this[long index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref Buffer[Offset + index];
		}



		/********************************************************************/
		/// <summary>
		/// Return or set the item at the index given
		/// </summary>
		/********************************************************************/
		public ref T this[ulong index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref Buffer[Offset + (long)index];
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
				T val = Buffer[Offset + index];
				Offset += increment;

				return val;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				Buffer[Offset + index] = value;
				Offset += increment;
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
			return new CPointer<T>(ptr.Buffer, ptr.Offset + increment);
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
			return new CPointer<T>(ptr.Buffer, ptr.Offset + (int)increment);
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
			return new CPointer<T>(ptr.Buffer, (int)(ptr.Offset + increment));
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
			return new CPointer<T>(ptr.Buffer, (int)(ptr.Offset + (long)increment));
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
			return new CPointer<T>(ptr.Buffer, ptr.Offset - decrement);
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
			return new CPointer<T>(ptr.Buffer, ptr.Offset - (int)decrement);
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
			return new CPointer<T>(ptr.Buffer, (int)(ptr.Offset - decrement));
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
			return new CPointer<T>(ptr.Buffer, (int)(ptr.Offset - (long)decrement));
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

			if (ptr1.IsNotNull && ptr2.IsNotNull && (ptr1.Buffer != ptr2.Buffer))
				throw new ArgumentException("Both pointers need to use the same buffer");

			return ptr1.Offset - ptr2.Offset;
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
		/// 
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			if (typeof(T) == typeof(char))
			{
				if (Buffer == null)
					return string.Empty;

				CPointer<char> ptr = new CPointer<char>(Buffer as char[], Offset);
				c_int len = (c_int)CString.strlen(ptr);

				return (len > 0) ? new string(ptr.AsSpan().Slice(0, len).ToArray()) : string.Empty;
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
			return (Buffer == other.Buffer) && (Offset == other.Offset);
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

			if (Buffer != other.Buffer)
				throw new ArgumentException("Both pointers need to use the same buffer");

			return Offset.CompareTo(other.Offset);
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
				return null;

			return new CPointer<T>(ArrayHelper.CloneArray(Buffer), Offset);
		}
		#endregion
	}
}
