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
	/// C# port of the C++ standard library std::reverse_iterator
	/// (see the C++ standard, [reverse.iterator]).
	///
	/// An iterator adaptor that reverses the direction of a given
	/// <see cref="forward_iterator{T}"/>. Incrementing a reverse_iterator
	/// moves it towards the beginning of the range, so iterating from
	/// rbegin() to rend() visits the elements in reverse order.
	///
	/// The stored base iterator points to the element after the one the
	/// reverse_iterator refers to. In other words, ＆*rit == ＆*(rit.base() -
	/// 1), just like in C++
	/// </summary>
	public struct reverse_iterator<T> : IIterator<reverse_iterator<T>, T>, IRandom_Access_Iterator<reverse_iterator<T>>, IEquatable<reverse_iterator<T>>
	{
		private forward_iterator<T> current;

		/********************************************************************/
		/// <summary>
		/// Constructs a reverse_iterator from the given base iterator
		/// (C++ explicit reverse_iterator(Iterator x))
		/// </summary>
		/********************************************************************/
		public reverse_iterator(forward_iterator<T> it)
		{
			current = it;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the underlying base iterator. It points to the element
		/// after the one this reverse_iterator refers to (C++ base()).
		///
		/// Named @base because base is a reserved keyword in C#
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public forward_iterator<T> @base()
		{
			return current;
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
			get => ref current[-index - 1];
		}



		/********************************************************************/
		/// <summary>
		/// Return a new iterator moved one element towards the beginning of
		/// the range
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static reverse_iterator<T> operator ++ (reverse_iterator<T> it)
		{
			return new reverse_iterator<T>(it.current - 1);
		}



		/********************************************************************/
		/// <summary>
		/// Return a new iterator moved one element towards the end of the
		/// range
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static reverse_iterator<T> operator -- (reverse_iterator<T> it)
		{
			return new reverse_iterator<T>(it.current + 1);
		}



		/********************************************************************/
		/// <summary>
		/// Return a new iterator moved the given number of elements towards
		/// the beginning of the range
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static reverse_iterator<T> operator + (reverse_iterator<T> it, int n)
		{
			return new reverse_iterator<T>(it.current - n);
		}



		/********************************************************************/
		/// <summary>
		/// Return a new iterator moved the given number of elements towards
		/// the end of the range
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static reverse_iterator<T> operator - (reverse_iterator<T> it, int n)
		{
			return new reverse_iterator<T>(it.current + n);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of elements between the two iterators. Both
		/// iterators must refer into the same buffer
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int operator - (reverse_iterator<T> it1, reverse_iterator<T> it2)
		{
			return it2.current - it1.current;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two iterators to see if they refer to the same element
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator == (reverse_iterator<T> it1, reverse_iterator<T> it2)
		{
			return it1.current == it2.current;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two iterators to see if they refer to different elements
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator != (reverse_iterator<T> it1, reverse_iterator<T> it2)
		{
			return it1.current != it2.current;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two iterators
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator < (reverse_iterator<T> it1, reverse_iterator<T> it2)
		{
			return it1.current > it2.current;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two iterators
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator > (reverse_iterator<T> it1, reverse_iterator<T> it2)
		{
			return it1.current < it2.current;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two iterators
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <= (reverse_iterator<T> it1, reverse_iterator<T> it2)
		{
			return it1.current >= it2.current;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two iterators
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >= (reverse_iterator<T> it1, reverse_iterator<T> it2)
		{
			return it1.current <= it2.current;
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
		ref T IIterator<reverse_iterator<T>, T>.Value
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref current[-1];
		}



		/********************************************************************/
		/// <summary>
		/// Returns a copy of the iterator advanced one element (C++ ++it)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		reverse_iterator<T> IIterator<reverse_iterator<T>>.Next()
		{
			return new reverse_iterator<T>(current - 1);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of elements between other and this iterator
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		ptrdiff_t IRandom_Access_Iterator<reverse_iterator<T>>.DistanceFrom(reverse_iterator<T> other)
		{
			return other.current - current;
		}
		#endregion

		#region IEquatable implementation
		/********************************************************************/
		/// <summary>
		/// Compare two iterators
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(reverse_iterator<T> other)
		{
			return current == other.current;
		}
		#endregion
	}
}
