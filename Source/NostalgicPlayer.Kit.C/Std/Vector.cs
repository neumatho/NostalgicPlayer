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
	/// C# port of the C++ standard library std::vector
	/// (see the C++ standard, [vector]).
	///
	/// A sequence container that encapsulates a dynamically sized contiguous
	/// array. The elements are stored in a single T[] buffer, so that a
	/// pointer into the buffer (see <see cref="begin"/>, <see cref="end"/>
	/// and <see cref="data"/>) can be used as a random access iterator, just
	/// like a pointer into the underlying array of a C++ std::vector.
	///
	/// Unlike a C++ std::vector, this is a reference type (a C# class), so
	/// assigning one vector‹T› variable to another makes both refer to the
	/// same container. To make an independent copy (the behavior of C++ copy
	/// construction and copy assignment), use the copy constructor
	/// vector‹T›(vector‹T›).
	///
	/// As with a C++ std::vector, any operation that changes the capacity (a
	/// reallocation) invalidates all pointers, iterators and references that
	/// refer to elements of the container.
	///
	/// The bulk operations that insert more than one element (copy
	/// construction; the fill, range and initializer forms of the
	/// constructor and assign; the fill and range overloads of insert; and
	/// the fill overload of resize) make an independent copy of each source
	/// element. If the element type implements IDeepCloneable‹T›, its
	/// MakeDeepClone() is used to obtain that copy, so that mutable reference
	/// type elements do not become shared between containers or between
	/// elements. The single element operations (push_back, the single value
	/// overload of insert, emplace and emplace_back) store the given instance
	/// directly
	/// </summary>
#pragma warning disable CS8981
	public class vector<T> : IEquatable<vector<T>>
	{
#pragma warning restore CS8981
		private T[] buffer;
		private int count;

		/********************************************************************/
		/// <summary>
		/// Constructs an empty container (C++ vector())
		/// </summary>
		/********************************************************************/
		public vector()
		{
			buffer = Array.Empty<T>();
			count = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the container with the given number of default inserted
		/// elements (C++ vector(size_type count)).
		///
		/// Note that, unlike C++, the elements are value initialized to
		/// default(T). For a reference type this means null, where C++ would
		/// have default constructed an object
		/// </summary>
		/********************************************************************/
		public vector(size_t count)
		{
			int n = (int)count;

			buffer = n > 0 ? new T[n] : Array.Empty<T>();
			this.count = n;
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the container with the given number of copies of the
		/// given value (C++ vector(size_type count, const T＆ value))
		/// </summary>
		/********************************************************************/
		public vector(size_t count, T value)
		{
			int n = (int)count;

			buffer = n > 0 ? new T[n] : Array.Empty<T>();
			this.count = n;

			for (int i = 0; i < n; i++)
				buffer[i] = Clone_Value(value);
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the container with the contents of the range
		/// [first, last) (C++ vector(InputIt first, InputIt last)).
		/// Both pointers need to use the same buffer
		/// </summary>
		/********************************************************************/
		public vector(CPointer<T> first, CPointer<T> last)
		{
			int n = last - first;

			if (n < 0)
				throw new ArgumentException("last is before first");

			buffer = n > 0 ? new T[n] : Array.Empty<T>();
			count = n;

			if (n > 0)
				Copy_Cloned(first.AsSpan(n), buffer, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the container with the contents of the given items
		/// (C++ vector(std::initializer_list‹T›))
		/// </summary>
		/********************************************************************/
		public vector(T[] items)
		{
			int n = items?.Length ?? 0;

			buffer = n > 0 ? new T[n] : Array.Empty<T>();
			count = n;

			if (n > 0)
				Copy_Cloned(items, buffer, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Copy constructor. Constructs an independent container with a copy
		/// of the contents of the given container (C++ vector(const
		/// vector＆)). Elements that implement IDeepCloneable‹T› are deep
		/// cloned, so that the two containers do not share element instances
		/// </summary>
		/********************************************************************/
		public vector(vector<T> other)
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));

			count = other.count;
			buffer = count > 0 ? new T[count] : Array.Empty<T>();

			if (count > 0)
				Copy_Cloned(other.buffer.AsSpan(0, count), buffer, 0);
		}

		#region assign
		/********************************************************************/
		/// <summary>
		/// Replaces the contents with the given number of copies of the given
		/// value (C++ assign(size_type count, const T＆ value))
		/// </summary>
		/********************************************************************/
		public void assign(size_t count, T value)
		{
			int n = (int)count;

			Ensure_Capacity(n);

			for (int i = 0; i < n; i++)
				buffer[i] = Clone_Value(value);

			if (n < this.count)
				Array.Clear(buffer, n, this.count - n);

			this.count = n;
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the contents with the contents of the range [first, last)
		/// (C++ assign(InputIt first, InputIt last)). Both pointers need to
		/// use the same buffer
		/// </summary>
		/********************************************************************/
		public void assign(CPointer<T> first, CPointer<T> last)
		{
			int n = last - first;

			if (n < 0)
				throw new ArgumentException("last is before first");

			// Snapshot the source before any reallocation, in case it refers
			// into this container's own buffer
			T[] tmp = new T[n];

			if (n > 0)
				first.AsSpan(n).CopyTo(tmp);

			Ensure_Capacity(n);
			Copy_Cloned(tmp, buffer, 0);

			if (n < count)
				Array.Clear(buffer, n, count - n);

			count = n;
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the contents with the given items
		/// (C++ assign(std::initializer_list‹T›))
		/// </summary>
		/********************************************************************/
		public void assign(T[] items)
		{
			int n = items?.Length ?? 0;

			Ensure_Capacity(n);

			if (n > 0)
				Copy_Cloned(items, buffer, 0);

			if (n < count)
				Array.Clear(buffer, n, count - n);

			count = n;
		}
		#endregion

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
			if (pos >= (size_t)count)
				throw new out_of_range($"vector.at: pos (which is {pos}) >= this->size() (which is {count})");

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
			if ((pos < 0) || (pos >= count))
				throw new out_of_range($"vector.at: pos (which is {pos}) >= this->size() (which is {count})");

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
			return ref buffer[count - 1];
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
			return new forward_iterator<T>(new CPointer<T>(buffer, count));
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
		#endregion

		#region Capacity
		/********************************************************************/
		/// <summary>
		/// Checks if the container has no elements (C++ empty())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool empty()
		{
			return count == 0;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of elements in the container (C++ size())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t size()
		{
			return (size_t)count;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the maximum number of elements the container is able to
		/// hold (C++ max_size())
		/// </summary>
		/********************************************************************/
		public size_t max_size()
		{
			return (size_t)Array.MaxLength;
		}



		/********************************************************************/
		/// <summary>
		/// Increases the capacity of the container to a value that is greater
		/// than or equal to the given capacity (C++ reserve(size_type)). If
		/// the given capacity is greater than max_size(), length_error is
		/// thrown. If it is less than the current capacity, nothing happens
		/// </summary>
		/********************************************************************/
		public void reserve(size_t newCapacity)
		{
			if (newCapacity > max_size())
				throw new length_error("vector.reserve: requested capacity is larger than max_size()");

			int n = (int)newCapacity;

			if (n > buffer.Length)
				Reallocate(n);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of elements that the container has currently
		/// allocated space for (C++ capacity())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t capacity()
		{
			return (size_t)buffer.Length;
		}



		/********************************************************************/
		/// <summary>
		/// Requests the removal of unused capacity, so that capacity()
		/// becomes equal to size() (C++ shrink_to_fit())
		/// </summary>
		/********************************************************************/
		public void shrink_to_fit()
		{
			if (buffer.Length > count)
				Reallocate(count);
		}
		#endregion

		#region Modifiers
		/********************************************************************/
		/// <summary>
		/// Erases all elements from the container. After this call, size() is
		/// zero. The capacity is unchanged (C++ clear())
		/// </summary>
		/********************************************************************/
		public void clear()
		{
			if (count > 0)
			{
				Array.Clear(buffer, 0, count);
				count = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Inserts the given value before the given position. Returns a
		/// pointer to the inserted element
		/// (C++ insert(const_iterator pos, const T＆ value))
		/// </summary>
		/********************************************************************/
		public CPointer<T> insert(CPointer<T> pos, T value)
		{
			int index = To_Index(pos);

			Make_Room(index, 1);
			buffer[index] = value;
			count++;

			return new CPointer<T>(buffer, index);
		}



		/********************************************************************/
		/// <summary>
		/// Inserts the given number of copies of the given value before the
		/// given position. Returns a pointer to the first inserted element,
		/// or to the given position if count is zero
		/// (C++ insert(const_iterator pos, size_type count, const T＆ value))
		/// </summary>
		/********************************************************************/
		public CPointer<T> insert(CPointer<T> pos, size_t count, T value)
		{
			int index = To_Index(pos);
			int n = (int)count;

			if (n > 0)
			{
				Make_Room(index, n);

				for (int i = 0; i < n; i++)
					buffer[index + i] = Clone_Value(value);

				this.count += n;
			}

			return new CPointer<T>(buffer, index);
		}



		/********************************************************************/
		/// <summary>
		/// Inserts the elements of the range [first, last) before the given
		/// position. Returns a pointer to the first inserted element, or to
		/// the given position if the range is empty
		/// (C++ insert(const_iterator pos, InputIt first, InputIt last))
		/// </summary>
		/********************************************************************/
		public CPointer<T> insert(CPointer<T> pos, CPointer<T> first, CPointer<T> last)
		{
			int index = To_Index(pos);
			int n = last - first;

			if (n < 0)
				throw new ArgumentException("last is before first");

			if (n > 0)
			{
				// Snapshot the source before any reallocation, in case it
				// refers into this container's own buffer
				T[] tmp = new T[n];
				first.AsSpan(n).CopyTo(tmp);

				Make_Room(index, n);
				Copy_Cloned(tmp, buffer, index);
				count += n;
			}

			return new CPointer<T>(buffer, index);
		}



		/********************************************************************/
		/// <summary>
		/// Inserts a new element before the given position, constructed from
		/// the given value. Returns a pointer to the inserted element
		/// (C++ emplace(const_iterator pos, Args＆＆... args))
		/// </summary>
		/********************************************************************/
		public CPointer<T> emplace(CPointer<T> pos, T value)
		{
			return insert(pos, value);
		}



		/********************************************************************/
		/// <summary>
		/// Removes the element at the given position. Returns a pointer to
		/// the element that followed the removed element
		/// (C++ erase(const_iterator pos))
		/// </summary>
		/********************************************************************/
		public CPointer<T> erase(CPointer<T> pos)
		{
			int index = To_Index(pos);

			Array.Copy(buffer, index + 1, buffer, index, count - index - 1);
			count--;
			buffer[count] = default;

			return new CPointer<T>(buffer, index);
		}



		/********************************************************************/
		/// <summary>
		/// Removes the elements of the range [first, last). Returns a pointer
		/// to the element that followed the last removed element
		/// (C++ erase(const_iterator first, const_iterator last))
		/// </summary>
		/********************************************************************/
		public CPointer<T> erase(CPointer<T> first, CPointer<T> last)
		{
			int start = To_Index(first);
			int n = last - first;

			if (n < 0)
				throw new ArgumentException("last is before first");

			if (n > 0)
			{
				Array.Copy(buffer, start + n, buffer, start, count - start - n);

				int newCount = count - n;
				Array.Clear(buffer, newCount, n);
				count = newCount;
			}

			return new CPointer<T>(buffer, start);
		}



		/********************************************************************/
		/// <summary>
		/// Appends the given value to the end of the container
		/// (C++ push_back(const T＆ value))
		/// </summary>
		/********************************************************************/
		public void push_back(T value)
		{
			Ensure_Capacity(count + 1);
			buffer[count++] = value;
		}



		/********************************************************************/
		/// <summary>
		/// Appends a new element to the end of the container, constructed
		/// from the given value. Returns a reference to the appended element
		/// (C++ emplace_back(Args＆＆... args))
		/// </summary>
		/********************************************************************/
		public ref T emplace_back(T value)
		{
			Ensure_Capacity(count + 1);
			buffer[count] = value;

			return ref buffer[count++];
		}



		/********************************************************************/
		/// <summary>
		/// Removes the last element of the container (C++ pop_back()).
		/// Calling this on an empty container is undefined
		/// </summary>
		/********************************************************************/
		public void pop_back()
		{
			if (count > 0)
			{
				count--;
				buffer[count] = default;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Resizes the container to contain the given number of elements. If
		/// the container is grown, the new elements are value initialized to
		/// default(T) (C++ resize(size_type count))
		/// </summary>
		/********************************************************************/
		public void resize(size_t count)
		{
			int n = (int)count;

			if (n < this.count)
			{
				Array.Clear(buffer, n, this.count - n);
				this.count = n;
			}
			else if (n > this.count)
			{
				Ensure_Capacity(n);
				Array.Clear(buffer, this.count, n - this.count);
				this.count = n;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Resizes the container to contain the given number of elements. If
		/// the container is grown, the new elements are copies of the given
		/// value (C++ resize(size_type count, const T＆ value))
		/// </summary>
		/********************************************************************/
		public void resize(size_t count, T value)
		{
			int n = (int)count;

			if (n < this.count)
			{
				Array.Clear(buffer, n, this.count - n);
				this.count = n;
			}
			else if (n > this.count)
			{
				Ensure_Capacity(n);

				for (int i = this.count; i < n; i++)
					buffer[i] = Clone_Value(value);

				this.count = n;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Exchanges the contents of the container with those of the other
		/// container (C++ swap(vector＆ other))
		/// </summary>
		/********************************************************************/
		public void swap(vector<T> other)
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));

			(buffer, other.buffer) = (other.buffer, buffer);
			(count, other.count) = (other.count, count);
		}
		#endregion

		#region Comparison operators
		/********************************************************************/
		/// <summary>
		/// Checks if the contents of the two containers are equal
		/// (C++ operator==)
		/// </summary>
		/********************************************************************/
		public static bool operator ==(vector<T> left, vector<T> right)
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
		public static bool operator !=(vector<T> left, vector<T> right)
		{
			return !(left == right);
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically
		/// (C++ operator‹)
		/// </summary>
		/********************************************************************/
		public static bool operator <(vector<T> left, vector<T> right)
		{
			return Compare(left, right) < 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically
		/// (C++ operator‹=)
		/// </summary>
		/********************************************************************/
		public static bool operator <=(vector<T> left, vector<T> right)
		{
			return Compare(left, right) <= 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically
		/// (C++ operator›)
		/// </summary>
		/********************************************************************/
		public static bool operator >(vector<T> left, vector<T> right)
		{
			return Compare(left, right) > 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically
		/// (C++ operator›=)
		/// </summary>
		/********************************************************************/
		public static bool operator >=(vector<T> left, vector<T> right)
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
		public bool Equals(vector<T> other)
		{
			if (other is null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			if (count != other.count)
				return false;

			EqualityComparer<T> comparer = EqualityComparer<T>.Default;

			for (int i = 0; i < count; i++)
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
			return Equals(obj as vector<T>);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a hash code based on the contents of the container
		/// </summary>
		/********************************************************************/
		public override int GetHashCode()
		{
			HashCode hash = new HashCode();

			for (int i = 0; i < count; i++)
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
		/// Returns the element index that the given pointer refers to,
		/// validating that it points into this container's buffer
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int To_Index(CPointer<T> pos)
		{
			return pos - new CPointer<T>(buffer, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Makes room for the given number of elements at the given index by
		/// growing the buffer if needed and shifting the following elements
		/// to the right. The count field is not changed
		/// </summary>
		/********************************************************************/
		private void Make_Room(int index, int n)
		{
			Ensure_Capacity(count + n);
			Array.Copy(buffer, index, buffer, index + n, count - index);
		}



		/********************************************************************/
		/// <summary>
		/// Ensures that the buffer can hold at least the given number of
		/// elements, growing it geometrically if needed
		/// </summary>
		/********************************************************************/
		private void Ensure_Capacity(int needed)
		{
			if (needed <= buffer.Length)
				return;

			int newCapacity = buffer.Length == 0 ? 1 : buffer.Length * 2;

			if (newCapacity < needed)
				newCapacity = needed;

			Reallocate(newCapacity);
		}



		/********************************************************************/
		/// <summary>
		/// Allocates a new buffer of the given capacity and copies the
		/// current elements into it
		/// </summary>
		/********************************************************************/
		private void Reallocate(int newCapacity)
		{
			T[] newBuffer = newCapacity > 0 ? new T[newCapacity] : Array.Empty<T>();

			if (count > 0)
				Array.Copy(buffer, newBuffer, count);

			buffer = newBuffer;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the contents of the two containers lexicographically,
		/// returning a negative value, zero or a positive value
		/// </summary>
		/********************************************************************/
		private static int Compare(vector<T> left, vector<T> right)
		{
			if (ReferenceEquals(left, right))
				return 0;

			if (left is null)
				return -1;

			if (right is null)
				return 1;

			Comparer<T> comparer = Comparer<T>.Default;
			int n = Math.Min(left.count, right.count);

			for (int i = 0; i < n; i++)
			{
				int c = comparer.Compare(left.buffer[i], right.buffer[i]);
				if (c != 0)
					return c;
			}

			return left.count.CompareTo(right.count);
		}
		#endregion
	}
}
