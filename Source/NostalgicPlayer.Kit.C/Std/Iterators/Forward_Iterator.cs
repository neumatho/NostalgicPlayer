/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Kit.C.Std.Iterators
{
	/// <summary>
	/// The iterator type returned by the begin()/end() of the containers in
	/// this namespace. It is a thin wrapper around a CPointer‹T›, which is the
	/// underlying random access iterator used throughout the ports.
	///
	/// It converts implicitly to and from CPointer‹T›, so it can be passed to
	/// any code that expects a plain pointer, while still being a distinct
	/// named type that mirrors <see cref="reverse_iterator{T}"/>
	/// </summary>
	public struct forward_iterator<T> : IIterator<forward_iterator<T>, T>, IRandom_Access_Iterator<forward_iterator<T>>, IEquatable<forward_iterator<T>>
	{
		private CPointer<T> current;

		/********************************************************************/
		/// <summary>
		/// Constructs a forward_iterator from the given pointer
		/// </summary>
		/********************************************************************/
		public forward_iterator(CPointer<T> it)
		{
			current = it;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the element at the given distance from the one this
		/// iterator refers to (C++ operator[]). Index 0 is the element the
		/// iterator itself refers to
		/// </summary>
		/********************************************************************/
		public ref T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref current[index];
		}



		/********************************************************************/
		/// <summary>
		/// Return a new iterator moved one element towards the end of the
		/// range
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static forward_iterator<T> operator ++ (forward_iterator<T> it)
		{
			return new forward_iterator<T>(it.current + 1);
		}



		/********************************************************************/
		/// <summary>
		/// Return a new iterator moved one element towards the beginning of
		/// the range
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static forward_iterator<T> operator -- (forward_iterator<T> it)
		{
			return new forward_iterator<T>(it.current - 1);
		}



		/********************************************************************/
		/// <summary>
		/// Return a new iterator moved the given number of elements towards
		/// the end of the range
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static forward_iterator<T> operator + (forward_iterator<T> it, int n)
		{
			return new forward_iterator<T>(it.current + n);
		}



		/********************************************************************/
		/// <summary>
		/// Return a new iterator moved the given number of elements towards
		/// the beginning of the range
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static forward_iterator<T> operator - (forward_iterator<T> it, int n)
		{
			return new forward_iterator<T>(it.current - n);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of elements between the two iterators. Both
		/// iterators must refer into the same buffer
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int operator - (forward_iterator<T> it1, forward_iterator<T> it2)
		{
			return it1.current - it2.current;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two iterators to see if they refer to the same element
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator == (forward_iterator<T> it1, forward_iterator<T> it2)
		{
			return it1.current == it2.current;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two iterators to see if they refer to different elements
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator != (forward_iterator<T> it1, forward_iterator<T> it2)
		{
			return it1.current != it2.current;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two iterators
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator < (forward_iterator<T> it1, forward_iterator<T> it2)
		{
			return it1.current < it2.current;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two iterators
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator > (forward_iterator<T> it1, forward_iterator<T> it2)
		{
			return it1.current > it2.current;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two iterators
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <= (forward_iterator<T> it1, forward_iterator<T> it2)
		{
			return it1.current <= it2.current;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two iterators
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >= (forward_iterator<T> it1, forward_iterator<T> it2)
		{
			return it1.current >= it2.current;
		}



		/********************************************************************/
		/// <summary>
		/// Convert a forward_iterator to the pointer it wraps. The conversion
		/// is one way (there is no implicit conversion back), so that a
		/// forward_iterator and a CPointer can be compared without the
		/// operators becoming ambiguous. Use the constructor to go the other
		/// way
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator CPointer<T>(forward_iterator<T> it)
		{
			return it.current;
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

		#region IIterator implementation
		/********************************************************************/
		/// <summary>
		/// The element the iterator currently refers to (C++ *it)
		/// </summary>
		/********************************************************************/
		ref T IIterator<forward_iterator<T>, T>.Value
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref current[0];
		}



		/********************************************************************/
		/// <summary>
		/// Returns a copy of the iterator advanced one element (C++ ++it)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		forward_iterator<T> IIterator<forward_iterator<T>>.Next()
		{
			return new forward_iterator<T>(current + 1);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of elements between other and this iterator
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		ptrdiff_t IRandom_Access_Iterator<forward_iterator<T>>.DistanceFrom(forward_iterator<T> other)
		{
			return current - other.current;
		}
		#endregion

		#region IEquatable implementation
		/********************************************************************/
		/// <summary>
		/// Compare two iterators
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(forward_iterator<T> other)
		{
			return current == other.current;
		}
		#endregion
	}
}
