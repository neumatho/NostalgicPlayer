/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C.Std.Exceptions;
using Polycode.NostalgicPlayer.Kit.C.Std.Iterators;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.C.Std
{
	/// <summary>
	/// C# port of the C++ standard library std::array
	/// (see the C++ standard, [array]).
	///
	/// A container that encapsulates a fixed size contiguous array. The
	/// number of elements (N) is chosen when the container is constructed and
	/// never changes, so there are no operations that add or remove elements.
	/// The elements are stored in a single T[] buffer, so that a pointer into
	/// the buffer (see <see cref="begin"/>, <see cref="end"/> and
	/// <see cref="data"/>) can be used as a random access iterator, just like
	/// a pointer into the underlying array of a C++ std::array.
	///
	/// In C++, N is a compile time template argument (std::array‹T, N›). C#
	/// generics cannot take such a non-type argument, so here N is passed to
	/// the constructor instead and stored as the length of the buffer.
	///
	/// Unlike a C++ std::array, this is a reference type (a C# class), so
	/// assigning one array‹T› variable to another makes both refer to the
	/// same container. To make an independent copy (the behavior of C++ copy
	/// construction and copy assignment), use the copy constructor
	/// array‹T›(array‹T›).
	///
	/// The bulk operations that copy more than one element (copy construction
	/// and the fill overload of the constructor and <see cref="fill"/>) make
	/// an independent copy of each source element. If the element type
	/// implements IDeepCloneable‹T›, its MakeDeepClone() is used to obtain
	/// that copy, so that mutable reference type elements do not become
	/// shared between containers or between elements
	/// </summary>
#pragma warning disable CS8981
	public class array<T> : IEquatable<array<T>>
	{
#pragma warning restore CS8981
		private readonly T[] buffer;

		/********************************************************************/
		/// <summary>
		/// Constructs a container holding the given number of default
		/// initialized elements (C++ std::array‹T, N›).
		///
		/// Note that, unlike C++, the elements are value initialized to
		/// default(T). For a reference type this means null, where C++ would
		/// have default constructed an object
		/// </summary>
		/********************************************************************/
		public array(size_t count)
		{
			int n = (int)count;

			buffer = n > 0 ? new T[n] : Array.Empty<T>();
		}



		/********************************************************************/
		/// <summary>
		/// Constructs a container holding a copy of the given items, so that
		/// N is the number of items (C++ aggregate initialization
		/// std::array‹T, N› a = { ... })
		/// </summary>
		/********************************************************************/
		public array(T[] items)
		{
			int n = items?.Length ?? 0;

			buffer = n > 0 ? new T[n] : Array.Empty<T>();

			if (n > 0)
				Copy_Cloned(items, buffer, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Copy constructor. Constructs an independent container with a copy
		/// of the contents of the given container (C++ array(const array＆)).
		/// Elements that implement IDeepCloneable‹T› are deep cloned, so that
		/// the two containers do not share element instances
		/// </summary>
		/********************************************************************/
		public array(array<T> other)
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));

			int n = other.buffer.Length;
			buffer = n > 0 ? new T[n] : Array.Empty<T>();

			if (n > 0)
				Copy_Cloned(other.buffer, buffer, 0);
		}

		#region Element access
		/********************************************************************/
		/// <summary>
		/// Returns a reference to the element at the given position, with
		/// bounds checking (C++ at(size_type pos)). Throws out_of_range if
		/// the position is not within the range of the container
		/// </summary>
		/********************************************************************/
		public ref T at(size_t pos)
		{
			if (pos >= (size_t)buffer.Length)
				throw new out_of_range($"array.at: pos (which is {pos}) >= this->size() (which is {buffer.Length})");

			return ref buffer[(int)pos];
		}



		/********************************************************************/
		/// <summary>
		/// Returns a reference to the element at the given position, with
		/// bounds checking (C++ at(size_type pos)). Throws out_of_range if
		/// the position is not within the range of the container
		/// </summary>
		/********************************************************************/
		public ref T at(int pos)
		{
			if ((pos < 0) || (pos >= buffer.Length))
				throw new out_of_range($"array.at: pos (which is {pos}) >= this->size() (which is {buffer.Length})");

			return ref buffer[pos];
		}



		/********************************************************************/
		/// <summary>
		/// Returns a reference to the element at the given position, without
		/// bounds checking (C++ operator[](size_type pos))
		/// </summary>
		/********************************************************************/
		public ref T this[size_t pos]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref buffer[(int)pos];
		}



		/********************************************************************/
		/// <summary>
		/// Returns a reference to the element at the given position, without
		/// bounds checking (C++ operator[](size_type pos))
		/// </summary>
		/********************************************************************/
		public ref T this[int pos]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref buffer[pos];
		}



		/********************************************************************/
		/// <summary>
		/// Returns a reference to the first element in the container
		/// (C++ front()). Calling this on an empty container is undefined
		/// </summary>
		/********************************************************************/
		public ref T front()
		{
			return ref buffer[0];
		}



		/********************************************************************/
		/// <summary>
		/// Returns a reference to the last element in the container
		/// (C++ back()). Calling this on an empty container is undefined
		/// </summary>
		/********************************************************************/
		public ref T back()
		{
			return ref buffer[buffer.Length - 1];
		}



		/********************************************************************/
		/// <summary>
		/// Returns a pointer to the first element of the underlying buffer
		/// (C++ data()). The returned pointer can be used as a random access
		/// iterator over the [0, size()) range
		/// </summary>
		/********************************************************************/
		public CPointer<T> data()
		{
			return new CPointer<T>(buffer, 0);
		}
		#endregion

		#region Iterators
		/********************************************************************/
		/// <summary>
		/// Returns an iterator to the first element of the container
		/// (C++ begin())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public forward_iterator<T> begin()
		{
			return new forward_iterator<T>(new CPointer<T>(buffer, 0));
		}



		/********************************************************************/
		/// <summary>
		/// Returns an iterator to just after the last element of the
		/// container (C++ end())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public forward_iterator<T> end()
		{
			return new forward_iterator<T>(new CPointer<T>(buffer, buffer.Length));
		}



		/********************************************************************/
		/// <summary>
		/// Returns a reverse iterator to the first element of the reversed
		/// container. It corresponds to the last element of the non-reversed
		/// container (C++ rbegin())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public reverse_iterator<T> rbegin()
		{
			return new reverse_iterator<T>(end());
		}



		/********************************************************************/
		/// <summary>
		/// Returns a reverse iterator to the element following the last
		/// element of the reversed container. It corresponds to the element
		/// preceding the first element of the non-reversed container
		/// (C++ rend())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public reverse_iterator<T> rend()
		{
			return new reverse_iterator<T>(begin());
		}



		/********************************************************************/
		/// <summary>
		/// Returns an enumerator over the elements of the container, so that
		/// it can be used in a C# foreach loop. The enumerator gives access
		/// to each element by reference (see <see cref="Buffer_Enumerator{T}"/>)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Buffer_Enumerator<T> GetEnumerator()
		{
			return new Buffer_Enumerator<T>(buffer, buffer.Length);
		}
		#endregion

		#region Capacity
		/********************************************************************/
		/// <summary>
		/// Checks if the container has no elements, which is the case only
		/// when it was constructed with N equal to zero (C++ empty())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool empty()
		{
			return buffer.Length == 0;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of elements in the container, which is always
		/// N (C++ size())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t size()
		{
			return (size_t)buffer.Length;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the maximum number of elements the container is able to
		/// hold. For a fixed size container this is always N, the same value
		/// as <see cref="size"/> (C++ max_size())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t max_size()
		{
			return (size_t)buffer.Length;
		}
		#endregion

		#region Operations
		/********************************************************************/
		/// <summary>
		/// Assigns the given value to every element in the container
		/// (C++ fill(const T＆ value)). Each element is an independent copy
		/// of the value (see <see cref="Clone_Value"/>)
		/// </summary>
		/********************************************************************/
		public void fill(T value)
		{
			for (int i = 0; i < buffer.Length; i++)
				buffer[i] = Clone_Value(value);
		}



		/********************************************************************/
		/// <summary>
		/// Exchanges the contents of the container with those of the other
		/// container (C++ swap(array＆ other)). Both containers must have the
		/// same size. Unlike vector, the elements are swapped in place, so
		/// the buffers themselves are not exchanged and pointers keep
		/// referring to the same container
		/// </summary>
		/********************************************************************/
		public void swap(array<T> other)
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));

			if (other.buffer.Length != buffer.Length)
				throw new ArgumentException("array.swap: the two containers have different sizes");

			for (int i = 0; i < buffer.Length; i++)
				(buffer[i], other.buffer[i]) = (other.buffer[i], buffer[i]);
		}
		#endregion

		#region Comparison operators
		/********************************************************************/
		/// <summary>
		/// Checks if the contents of the two containers are equal
		/// (C++ operator==)
		/// </summary>
		/********************************************************************/
		public static bool operator ==(array<T> left, array<T> right)
		{
			if (ReferenceEquals(left, right))
				return true;

			if ((left is null) || (right is null))
				return false;

			return left.Equals(right);
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the contents of the two containers are not equal
		/// (C++ operator!=)
		/// </summary>
		/********************************************************************/
		public static bool operator !=(array<T> left, array<T> right)
		{
			return !(left == right);
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically
		/// (C++ operator‹)
		/// </summary>
		/********************************************************************/
		public static bool operator <(array<T> left, array<T> right)
		{
			return Compare(left, right) < 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically
		/// (C++ operator‹=)
		/// </summary>
		/********************************************************************/
		public static bool operator <=(array<T> left, array<T> right)
		{
			return Compare(left, right) <= 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically
		/// (C++ operator›)
		/// </summary>
		/********************************************************************/
		public static bool operator >(array<T> left, array<T> right)
		{
			return Compare(left, right) > 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically
		/// (C++ operator›=)
		/// </summary>
		/********************************************************************/
		public static bool operator >=(array<T> left, array<T> right)
		{
			return Compare(left, right) >= 0;
		}
		#endregion

		#region IEquatable implementation
		/********************************************************************/
		/// <summary>
		/// Checks if the contents of this container are equal to the contents
		/// of the other container
		/// </summary>
		/********************************************************************/
		public bool Equals(array<T> other)
		{
			if (other is null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			if (buffer.Length != other.buffer.Length)
				return false;

			EqualityComparer<T> comparer = EqualityComparer<T>.Default;

			for (int i = 0; i < buffer.Length; i++)
			{
				if (!comparer.Equals(buffer[i], other.buffer[i]))
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the contents of this container are equal to the contents
		/// of the other container
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return Equals(obj as array<T>);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a hash code based on the contents of the container
		/// </summary>
		/********************************************************************/
		public override int GetHashCode()
		{
			HashCode hash = new HashCode();

			for (int i = 0; i < buffer.Length; i++)
				hash.Add(buffer[i]);

			return hash.ToHashCode();
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Returns an independent copy of the given value. If the value
		/// implements IDeepCloneable‹T›, its MakeDeepClone() is used to
		/// obtain a new instance. Otherwise the value is returned unchanged,
		/// which is the correct behavior for value types and immutable
		/// reference types
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static T Clone_Value(T value)
		{
			return value is IDeepCloneable<T> cloneable ? cloneable.MakeDeepClone() : value;
		}



		/********************************************************************/
		/// <summary>
		/// Copies the source elements into the destination buffer starting at
		/// the given index, making an independent copy of each element (see
		/// <see cref="Clone_Value"/>)
		/// </summary>
		/********************************************************************/
		private static void Copy_Cloned(ReadOnlySpan<T> source, T[] destination, int destinationIndex)
		{
			for (int i = 0; i < source.Length; i++)
				destination[destinationIndex + i] = Clone_Value(source[i]);
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically,
		/// returning a negative value, zero or a positive value
		/// </summary>
		/********************************************************************/
		private static int Compare(array<T> left, array<T> right)
		{
			if (ReferenceEquals(left, right))
				return 0;

			if (left is null)
				return -1;

			if (right is null)
				return 1;

			Comparer<T> comparer = Comparer<T>.Default;
			int n = Math.Min(left.buffer.Length, right.buffer.Length);

			for (int i = 0; i < n; i++)
			{
				int c = comparer.Compare(left.buffer[i], right.buffer[i]);
				if (c != 0)
					return c;
			}

			return left.buffer.Length.CompareTo(right.buffer.Length);
		}
		#endregion
	}
}
