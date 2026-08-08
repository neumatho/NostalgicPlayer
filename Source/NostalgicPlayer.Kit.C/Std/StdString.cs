/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using System.Text;
using Polycode.NostalgicPlayer.Kit.C.Std.Exceptions;
using Polycode.NostalgicPlayer.Kit.C.Std.Iterators;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.C.Std
{
	/// <summary>
	/// C# port of the C++ standard library std::string, which is the
	/// std::basic_string‹char› instantiation (see the C++ standard,
	/// [basic.string]).
	///
	/// A sequence container that holds a contiguous run of characters. As in
	/// C++, a character is a single byte and carries no encoding: the
	/// container stores the raw bytes it is given and never interprets them,
	/// which is why the element type is uint8_t and not the C# char. Any
	/// conversion to or from encoded text is up to the caller, apart from
	/// the constructor <see cref="StdString(string)"/>, which encodes the
	/// characters of a C# string as UTF-8.
	///
	/// The characters are stored in a single uint8_t[] buffer, so that a
	/// pointer into the buffer (see <see cref="begin"/>, <see cref="end"/>,
	/// <see cref="data"/> and <see cref="c_str"/>) can be used as a random
	/// access iterator, just like a pointer into the underlying array of a
	/// C++ std::string. The buffer always holds a null character just after
	/// the last one, so the pointer can also be handed to any code that
	/// expects a null terminated C string.
	///
	/// Unlike a C++ std::string, this is a reference type (a C# class), so
	/// assigning one StdString variable to another makes both refer to the
	/// same container. To make an independent copy (the behavior of C++ copy
	/// construction and copy assignment), use the copy constructor
	/// StdString(StdString) or <see cref="MakeDeepClone"/>. To copy the
	/// characters into an already existing container, use
	/// <see cref="CopyTo"/>. To hand the characters over to a new container,
	/// leaving this one empty (the behavior of C++ move construction), use
	/// <see cref="MoveFrom"/> or Utility.move.
	///
	/// As with a C++ std::string, any operation that changes the capacity (a
	/// reallocation) invalidates all pointers, iterators and references that
	/// refer to characters of the container.
	///
	/// Notable differences from C++:
	/// - The allocator and the char_traits template parameters are not
	///   modelled. Characters are compared as unsigned bytes, which is what
	///   std::char_traits‹char› does.
	/// - C# cannot overload +=, =, or the conversion to a string_view. The
	///   binary + operators are provided, from which C# derives +=, and the
	///   named methods (<see cref="assign(StdString)"/>,
	///   <see cref="append(StdString)"/>) cover the rest.
	/// - The comparison operators are only defined between two containers.
	///   Comparing against a C string is done with the compare method,
	///   as an operator taking a pointer would make a comparison against
	///   null ambiguous
	/// </summary>
	public class StdString : IEquatable<StdString>, IDeepCloneable<StdString>, ICopyTo<StdString>, IMoveable<StdString>
	{
		/// <summary>
		/// The special value meaning "until the end of the container" when
		/// given as a length, and "not found" when returned by one of the
		/// search methods (C++ npos)
		/// </summary>
		public const size_t npos = size_t.MaxValue;

		private uint8_t[] buffer;
		private int count;

		/********************************************************************/
		/// <summary>
		/// Constructs an empty container (C++ basic_string())
		/// </summary>
		/********************************************************************/
		public StdString()
		{
			buffer = new uint8_t[1];
			count = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the container with the given number of copies of the
		/// given character
		/// (C++ basic_string(size_type count, CharT ch))
		/// </summary>
		/********************************************************************/
		public StdString(size_t count, uint8_t ch)
		{
			int n = Check_Length(count);

			buffer = new uint8_t[n + 1];
			this.count = n;

			buffer.AsSpan(0, n).Fill(ch);
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the container with the characters of the given
		/// container from the given position and to its end
		/// (C++ basic_string(const basic_string＆ other, size_type pos))
		/// </summary>
		/********************************************************************/
		public StdString(StdString other, size_t pos) : this(other, pos, npos)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the container with the given number of characters of
		/// the given container, starting at the given position (C++
		/// basic_string(const basic_string＆, size_type pos, size_type count))
		/// </summary>
		/********************************************************************/
		public StdString(StdString other, size_t pos, size_t count) : this()
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));

			int start = other.Check_Pos(pos, "basic_string");
			int n = other.Limit(start, count);

			Replace_Span(0, 0, other.buffer.AsSpan(start, n));
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the container with the first given number of
		/// characters of the given character array. The array may contain
		/// null characters
		/// (C++ basic_string(const CharT* s, size_type count))
		/// </summary>
		/********************************************************************/
		public StdString(CPointer<uint8_t> s, size_t count) : this()
		{
			Replace_Span(0, 0, To_Span(s, count));
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the container with the characters of the given null
		/// terminated character array, up to but not including the null
		/// character (C++ basic_string(const CharT* s))
		/// </summary>
		/********************************************************************/
		public StdString(CPointer<uint8_t> s) : this()
		{
			Replace_Span(0, 0, To_Span(s));
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the container with the characters of the range
		/// [first, last) (C++ basic_string(InputIt first, InputIt last)).
		/// Both pointers need to use the same buffer
		/// </summary>
		/********************************************************************/
		public StdString(CPointer<uint8_t> first, CPointer<uint8_t> last) : this()
		{
			Replace_Span(0, 0, To_Span(first, last));
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the container with the given characters
		/// (C++ basic_string(std::initializer_list‹CharT›))
		/// </summary>
		/********************************************************************/
		public StdString(uint8_t[] items) : this()
		{
			Replace_Span(0, 0, items == null ? ReadOnlySpan<uint8_t>.Empty : items.AsSpan());
		}



		/********************************************************************/
		/// <summary>
		/// Copy constructor. Constructs an independent container with a copy
		/// of the characters of the given container
		/// (C++ basic_string(const basic_string＆))
		/// </summary>
		/********************************************************************/
		public StdString(StdString other) : this()
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));

			Replace_Span(0, 0, other.Span());
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the container with the characters of the given C#
		/// string, encoded as UTF-8. There is no C++ counterpart to this
		/// one, as a std::string never knows the encoding of the characters
		/// it holds. No byte order mark is stored, and the encoder replaces
		/// any character that cannot be encoded (an unpaired surrogate)
		/// with U+FFFD
		/// </summary>
		/********************************************************************/
		public StdString(string str) : this()
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			Replace_Span(0, 0, Encoding.UTF8.GetBytes(str));
		}

		#region assign
		/********************************************************************/
		/// <summary>
		/// Replaces the characters with the given number of copies of the
		/// given character (C++ assign(size_type count, CharT ch))
		/// </summary>
		/********************************************************************/
		public StdString assign(size_t count, uint8_t ch)
		{
			Replace_Fill(0, this.count, Check_Length(count), ch);

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the characters with a copy of the characters of the
		/// given container (C++ assign(const basic_string＆ str))
		/// </summary>
		/********************************************************************/
		public StdString assign(StdString str)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			if (!ReferenceEquals(str, this))
				Replace_Span(0, count, str.Span());

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the characters with the characters of the given
		/// container from the given position and to its end
		/// (C++ assign(const basic_string＆ str, size_type pos))
		/// </summary>
		/********************************************************************/
		public StdString assign(StdString str, size_t pos)
		{
			return assign(str, pos, npos);
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the characters with the given number of characters of
		/// the given container, starting at the given position (C++
		/// assign(const basic_string＆ str, size_type pos, size_type count))
		/// </summary>
		/********************************************************************/
		public StdString assign(StdString str, size_t pos, size_t count)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			int start = str.Check_Pos(pos, "assign");
			int n = str.Limit(start, count);

			Replace_Span(0, this.count, str.buffer.AsSpan(start, n));

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the characters with the first given number of characters
		/// of the given character array. The array may contain null
		/// characters (C++ assign(const CharT* s, size_type count))
		/// </summary>
		/********************************************************************/
		public StdString assign(CPointer<uint8_t> s, size_t count)
		{
			Replace_Span(0, this.count, To_Span(s, count));

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the characters with the characters of the given null
		/// terminated character array (C++ assign(const CharT* s))
		/// </summary>
		/********************************************************************/
		public StdString assign(CPointer<uint8_t> s)
		{
			Replace_Span(0, count, To_Span(s));

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the characters with the characters of the range
		/// [first, last) (C++ assign(InputIt first, InputIt last)). Both
		/// pointers need to use the same buffer
		/// </summary>
		/********************************************************************/
		public StdString assign(CPointer<uint8_t> first, CPointer<uint8_t> last)
		{
			Replace_Span(0, count, To_Span(first, last));

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the characters with the given characters
		/// (C++ assign(std::initializer_list‹CharT›))
		/// </summary>
		/********************************************************************/
		public StdString assign(uint8_t[] items)
		{
			Replace_Span(0, count, items == null ? ReadOnlySpan<uint8_t>.Empty : items.AsSpan());

			return this;
		}
		#endregion

		#region Element access
		/********************************************************************/
		/// <summary>
		/// Returns a reference to the character at the given position, with
		/// bounds checking (C++ at(size_type pos)). Throws out_of_range if
		/// the position is not within the range of the container
		/// </summary>
		/********************************************************************/
		public ref uint8_t at(size_t pos)
		{
			if (pos >= (size_t)count)
				throw new out_of_range($"basic_string.at: pos (which is {pos}) >= this->size() (which is {count})");

			return ref buffer[(int)pos];
		}



		/********************************************************************/
		/// <summary>
		/// Returns a reference to the character at the given position, with
		/// bounds checking (C++ at(size_type pos)). Throws out_of_range if
		/// the position is not within the range of the container
		/// </summary>
		/********************************************************************/
		public ref uint8_t at(int pos)
		{
			if ((pos < 0) || (pos >= count))
				throw new out_of_range($"basic_string.at: pos (which is {pos}) >= this->size() (which is {count})");

			return ref buffer[pos];
		}



		/********************************************************************/
		/// <summary>
		/// Returns a reference to the character at the given position,
		/// without bounds checking (C++ operator[](size_type pos)). As in
		/// C++, the position may be size(), which gives the null character
		/// just after the last one
		/// </summary>
		/********************************************************************/
		public ref uint8_t this[size_t pos]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref buffer[(int)pos];
		}



		/********************************************************************/
		/// <summary>
		/// Returns a reference to the character at the given position,
		/// without bounds checking (C++ operator[](size_type pos)). As in
		/// C++, the position may be size(), which gives the null character
		/// just after the last one
		/// </summary>
		/********************************************************************/
		public ref uint8_t this[int pos]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref buffer[pos];
		}



		/********************************************************************/
		/// <summary>
		/// Returns a reference to the first character of the container
		/// (C++ front()). Calling this on an empty container is undefined
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref uint8_t front()
		{
			return ref buffer[0];
		}



		/********************************************************************/
		/// <summary>
		/// Returns a reference to the last character of the container
		/// (C++ back()). Calling this on an empty container is undefined
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref uint8_t back()
		{
			return ref buffer[count - 1];
		}



		/********************************************************************/
		/// <summary>
		/// Returns a pointer to the first character of the underlying
		/// buffer (C++ data()). The buffer holds a null character at index
		/// size(), so the pointer can be used both as a random access
		/// iterator over the [0, size()) range and as a C string
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CPointer<uint8_t> data()
		{
			return new CPointer<uint8_t>(buffer, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a pointer to a null terminated character array holding
		/// the characters of the container (C++ c_str()). This is the same
		/// pointer as the one <see cref="data"/> returns
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CPointer<uint8_t> c_str()
		{
			return new CPointer<uint8_t>(buffer, 0);
		}
		#endregion

		#region Iterators
		/********************************************************************/
		/// <summary>
		/// Returns an iterator to the first character of the container
		/// (C++ begin())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public forward_iterator<uint8_t> begin()
		{
			return new forward_iterator<uint8_t>(new CPointer<uint8_t>(buffer, 0));
		}



		/********************************************************************/
		/// <summary>
		/// Returns an iterator to just after the last character of the
		/// container (C++ end())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public forward_iterator<uint8_t> end()
		{
			return new forward_iterator<uint8_t>(new CPointer<uint8_t>(buffer, count));
		}



		/********************************************************************/
		/// <summary>
		/// Returns a reverse iterator to the first character of the
		/// reversed container. It corresponds to the last character of the
		/// non-reversed container (C++ rbegin())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public reverse_iterator<uint8_t> rbegin()
		{
			return new reverse_iterator<uint8_t>(end());
		}



		/********************************************************************/
		/// <summary>
		/// Returns a reverse iterator to the character following the last
		/// character of the reversed container. It corresponds to the
		/// character preceding the first character of the non-reversed
		/// container (C++ rend())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public reverse_iterator<uint8_t> rend()
		{
			return new reverse_iterator<uint8_t>(begin());
		}



		/********************************************************************/
		/// <summary>
		/// Returns an enumerator over the characters of the container, so
		/// that it can be used in a C# foreach loop. The enumerator gives
		/// access to each character by reference (see
		/// <see cref="Buffer_Enumerator{T}"/>)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Buffer_Enumerator<uint8_t> GetEnumerator()
		{
			return new Buffer_Enumerator<uint8_t>(buffer, count);
		}
		#endregion

		#region Capacity
		/********************************************************************/
		/// <summary>
		/// Checks if the container has no characters (C++ empty())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool empty()
		{
			return count == 0;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of characters in the container (C++ size())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t size()
		{
			return (size_t)count;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of characters in the container (C++ length()).
		/// This is the same as <see cref="size"/>
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t length()
		{
			return (size_t)count;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the maximum number of characters the container is able
		/// to hold (C++ max_size())
		/// </summary>
		/********************************************************************/
		public size_t max_size()
		{
			return (size_t)(Array.MaxLength - 1);
		}



		/********************************************************************/
		/// <summary>
		/// Increases the capacity of the container to a value that is
		/// greater than or equal to the given capacity
		/// (C++ reserve(size_type)). If the given capacity is greater than
		/// max_size(), length_error is thrown. If it is less than the
		/// current capacity, nothing happens
		/// </summary>
		/********************************************************************/
		public void reserve(size_t newCapacity)
		{
			if (newCapacity > max_size())
				throw new length_error("basic_string.reserve: requested capacity is larger than max_size()");

			int n = (int)newCapacity;

			if ((size_t)n > capacity())
				Reallocate(n);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of characters that the container has
		/// currently allocated space for, not counting the null character
		/// (C++ capacity())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t capacity()
		{
			return (size_t)(buffer.Length - 1);
		}



		/********************************************************************/
		/// <summary>
		/// Requests the removal of unused capacity, so that capacity()
		/// becomes equal to size() (C++ shrink_to_fit())
		/// </summary>
		/********************************************************************/
		public void shrink_to_fit()
		{
			if (capacity() > (size_t)count)
				Reallocate(count);
		}
		#endregion

		#region Modifiers
		/********************************************************************/
		/// <summary>
		/// Removes all characters from the container. After this call,
		/// size() is zero. The capacity is unchanged (C++ clear())
		/// </summary>
		/********************************************************************/
		public void clear()
		{
			Set_Size(0);
		}



		/********************************************************************/
		/// <summary>
		/// Inserts the given number of copies of the given character at the
		/// given position
		/// (C++ insert(size_type index, size_type count, CharT ch))
		/// </summary>
		/********************************************************************/
		public StdString insert(size_t index, size_t count, uint8_t ch)
		{
			int at = Check_Pos(index, "insert");

			Replace_Fill(at, 0, Check_Length(count), ch);

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Inserts the characters of the given null terminated character
		/// array at the given position
		/// (C++ insert(size_type index, const CharT* s))
		/// </summary>
		/********************************************************************/
		public StdString insert(size_t index, CPointer<uint8_t> s)
		{
			int at = Check_Pos(index, "insert");

			Replace_Span(at, 0, To_Span(s));

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Inserts the first given number of characters of the given
		/// character array at the given position. The array may contain
		/// null characters
		/// (C++ insert(size_type index, const CharT* s, size_type count))
		/// </summary>
		/********************************************************************/
		public StdString insert(size_t index, CPointer<uint8_t> s, size_t count)
		{
			int at = Check_Pos(index, "insert");

			Replace_Span(at, 0, To_Span(s, count));

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Inserts the characters of the given container at the given
		/// position
		/// (C++ insert(size_type index, const basic_string＆ str))
		/// </summary>
		/********************************************************************/
		public StdString insert(size_t index, StdString str)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			int at = Check_Pos(index, "insert");

			Replace_Span(at, 0, str.Span());

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Inserts the characters of the given container from the given
		/// position and to its end, at the given position (C++
		/// insert(size_type index, const basic_string＆ str, size_type
		/// s_index))
		/// </summary>
		/********************************************************************/
		public StdString insert(size_t index, StdString str, size_t s_index)
		{
			return insert(index, str, s_index, npos);
		}



		/********************************************************************/
		/// <summary>
		/// Inserts the given number of characters of the given container,
		/// starting at the given position, at the given position (C++
		/// insert(size_type index, const basic_string＆ str, size_type
		/// s_index, size_type count))
		/// </summary>
		/********************************************************************/
		public StdString insert(size_t index, StdString str, size_t s_index, size_t count)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			int at = Check_Pos(index, "insert");
			int start = str.Check_Pos(s_index, "insert");
			int n = str.Limit(start, count);

			Replace_Span(at, 0, str.buffer.AsSpan(start, n));

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Inserts the given character before the given position. Returns a
		/// pointer to the inserted character
		/// (C++ insert(const_iterator pos, CharT ch))
		/// </summary>
		/********************************************************************/
		public CPointer<uint8_t> insert(CPointer<uint8_t> pos, uint8_t ch)
		{
			int at = To_Index(pos);

			Replace_Fill(at, 0, 1, ch);

			return new CPointer<uint8_t>(buffer, at);
		}



		/********************************************************************/
		/// <summary>
		/// Inserts the given number of copies of the given character before
		/// the given position. Returns a pointer to the first inserted
		/// character, or to the given position if count is zero
		/// (C++ insert(const_iterator pos, size_type count, CharT ch))
		/// </summary>
		/********************************************************************/
		public CPointer<uint8_t> insert(CPointer<uint8_t> pos, size_t count, uint8_t ch)
		{
			int at = To_Index(pos);

			Replace_Fill(at, 0, Check_Length(count), ch);

			return new CPointer<uint8_t>(buffer, at);
		}



		/********************************************************************/
		/// <summary>
		/// Inserts the characters of the range [first, last) before the
		/// given position. Returns a pointer to the first inserted
		/// character, or to the given position if the range is empty
		/// (C++ insert(const_iterator pos, InputIt first, InputIt last))
		/// </summary>
		/********************************************************************/
		public CPointer<uint8_t> insert(CPointer<uint8_t> pos, CPointer<uint8_t> first, CPointer<uint8_t> last)
		{
			int at = To_Index(pos);

			Replace_Span(at, 0, To_Span(first, last));

			return new CPointer<uint8_t>(buffer, at);
		}



		/********************************************************************/
		/// <summary>
		/// Removes all the characters of the container
		/// (C++ erase(size_type index, size_type count))
		/// </summary>
		/********************************************************************/
		public StdString erase()
		{
			return erase(0, npos);
		}



		/********************************************************************/
		/// <summary>
		/// Removes the characters from the given position and to the end of
		/// the container (C++ erase(size_type index, size_type count))
		/// </summary>
		/********************************************************************/
		public StdString erase(size_t index)
		{
			return erase(index, npos);
		}



		/********************************************************************/
		/// <summary>
		/// Removes the given number of characters, starting at the given
		/// position (C++ erase(size_type index, size_type count))
		/// </summary>
		/********************************************************************/
		public StdString erase(size_t index, size_t count)
		{
			int at = Check_Pos(index, "erase");
			int n = Limit(at, count);

			Replace_Fill(at, n, 0, 0);

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Removes the character at the given position. Returns a pointer
		/// to the character that followed the removed character
		/// (C++ erase(const_iterator position))
		/// </summary>
		/********************************************************************/
		public CPointer<uint8_t> erase(CPointer<uint8_t> position)
		{
			int at = To_Index(position);

			Replace_Fill(at, 1, 0, 0);

			return new CPointer<uint8_t>(buffer, at);
		}



		/********************************************************************/
		/// <summary>
		/// Removes the characters of the range [first, last). Returns a
		/// pointer to the character that followed the last removed
		/// character (C++ erase(const_iterator first, const_iterator last))
		/// </summary>
		/********************************************************************/
		public CPointer<uint8_t> erase(CPointer<uint8_t> first, CPointer<uint8_t> last)
		{
			int at = To_Index(first);
			int n = last - first;

			if (n < 0)
				throw new ArgumentException("last is before first");

			Replace_Fill(at, n, 0, 0);

			return new CPointer<uint8_t>(buffer, at);
		}



		/********************************************************************/
		/// <summary>
		/// Appends the given character to the end of the container
		/// (C++ push_back(CharT ch))
		/// </summary>
		/********************************************************************/
		public void push_back(uint8_t ch)
		{
			Replace_Fill(count, 0, 1, ch);
		}



		/********************************************************************/
		/// <summary>
		/// Removes the last character of the container (C++ pop_back()).
		/// Calling this on an empty container is undefined
		/// </summary>
		/********************************************************************/
		public void pop_back()
		{
			if (count > 0)
				Set_Size(count - 1);
		}



		/********************************************************************/
		/// <summary>
		/// Appends the given number of copies of the given character
		/// (C++ append(size_type count, CharT ch))
		/// </summary>
		/********************************************************************/
		public StdString append(size_t count, uint8_t ch)
		{
			Replace_Fill(this.count, 0, Check_Length(count), ch);

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Appends the characters of the given container
		/// (C++ append(const basic_string＆ str))
		/// </summary>
		/********************************************************************/
		public StdString append(StdString str)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			Replace_Span(count, 0, str.Span());

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Appends the characters of the given container from the given
		/// position and to its end
		/// (C++ append(const basic_string＆ str, size_type pos))
		/// </summary>
		/********************************************************************/
		public StdString append(StdString str, size_t pos)
		{
			return append(str, pos, npos);
		}



		/********************************************************************/
		/// <summary>
		/// Appends the given number of characters of the given container,
		/// starting at the given position (C++ append(const basic_string＆
		/// str, size_type pos, size_type count))
		/// </summary>
		/********************************************************************/
		public StdString append(StdString str, size_t pos, size_t count)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			int start = str.Check_Pos(pos, "append");
			int n = str.Limit(start, count);

			Replace_Span(this.count, 0, str.buffer.AsSpan(start, n));

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Appends the first given number of characters of the given
		/// character array. The array may contain null characters
		/// (C++ append(const CharT* s, size_type count))
		/// </summary>
		/********************************************************************/
		public StdString append(CPointer<uint8_t> s, size_t count)
		{
			Replace_Span(this.count, 0, To_Span(s, count));

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Appends the characters of the given null terminated character
		/// array (C++ append(const CharT* s))
		/// </summary>
		/********************************************************************/
		public StdString append(CPointer<uint8_t> s)
		{
			Replace_Span(count, 0, To_Span(s));

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Appends the characters of the range [first, last)
		/// (C++ append(InputIt first, InputIt last)). Both pointers need to
		/// use the same buffer
		/// </summary>
		/********************************************************************/
		public StdString append(CPointer<uint8_t> first, CPointer<uint8_t> last)
		{
			Replace_Span(count, 0, To_Span(first, last));

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Appends the given characters
		/// (C++ append(std::initializer_list‹CharT›))
		/// </summary>
		/********************************************************************/
		public StdString append(uint8_t[] items)
		{
			Replace_Span(count, 0, items == null ? ReadOnlySpan<uint8_t>.Empty : items.AsSpan());

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the given number of characters, starting at the given
		/// position, with the characters of the given container (C++
		/// replace(size_type pos, size_type count, const basic_string＆ str))
		/// </summary>
		/********************************************************************/
		public StdString replace(size_t pos, size_t count, StdString str)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			int at = Check_Pos(pos, "replace");

			Replace_Span(at, Limit(at, count), str.Span());

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the characters of the range [first, last) with the
		/// characters of the given container (C++ replace(const_iterator
		/// first, const_iterator last, const basic_string＆ str))
		/// </summary>
		/********************************************************************/
		public StdString replace(CPointer<uint8_t> first, CPointer<uint8_t> last, StdString str)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			int at = To_Index(first);

			Replace_Span(at, Range_Length(first, last), str.Span());

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the given number of characters, starting at the given
		/// position, with the given number of characters of the given
		/// container, starting at the given position of it (C++
		/// replace(size_type pos, size_type count, const basic_string＆ str,
		/// size_type pos2, size_type count2))
		/// </summary>
		/********************************************************************/
		public StdString replace(size_t pos, size_t count, StdString str, size_t pos2, size_t count2)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			int at = Check_Pos(pos, "replace");
			int start = str.Check_Pos(pos2, "replace");

			Replace_Span(at, Limit(at, count), str.buffer.AsSpan(start, str.Limit(start, count2)));

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the given number of characters, starting at the given
		/// position, with the characters of the given container from the
		/// given position of it and to its end (C++ replace(size_type pos,
		/// size_type count, const basic_string＆ str, size_type pos2,
		/// size_type count2))
		/// </summary>
		/********************************************************************/
		public StdString replace(size_t pos, size_t count, StdString str, size_t pos2)
		{
			return replace(pos, count, str, pos2, npos);
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the given number of characters, starting at the given
		/// position, with the first given number of characters of the given
		/// character array (C++ replace(size_type pos, size_type count,
		/// const CharT* cstr, size_type count2))
		/// </summary>
		/********************************************************************/
		public StdString replace(size_t pos, size_t count, CPointer<uint8_t> cstr, size_t count2)
		{
			int at = Check_Pos(pos, "replace");

			Replace_Span(at, Limit(at, count), To_Span(cstr, count2));

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the characters of the range [first, last) with the
		/// first given number of characters of the given character array
		/// (C++ replace(const_iterator first, const_iterator last, const
		/// CharT* cstr, size_type count2))
		/// </summary>
		/********************************************************************/
		public StdString replace(CPointer<uint8_t> first, CPointer<uint8_t> last, CPointer<uint8_t> cstr, size_t count2)
		{
			int at = To_Index(first);

			Replace_Span(at, Range_Length(first, last), To_Span(cstr, count2));

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the given number of characters, starting at the given
		/// position, with the characters of the given null terminated
		/// character array (C++ replace(size_type pos, size_type count,
		/// const CharT* cstr))
		/// </summary>
		/********************************************************************/
		public StdString replace(size_t pos, size_t count, CPointer<uint8_t> cstr)
		{
			int at = Check_Pos(pos, "replace");

			Replace_Span(at, Limit(at, count), To_Span(cstr));

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the characters of the range [first, last) with the
		/// characters of the given null terminated character array (C++
		/// replace(const_iterator first, const_iterator last, const CharT*
		/// cstr))
		/// </summary>
		/********************************************************************/
		public StdString replace(CPointer<uint8_t> first, CPointer<uint8_t> last, CPointer<uint8_t> cstr)
		{
			int at = To_Index(first);

			Replace_Span(at, Range_Length(first, last), To_Span(cstr));

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the given number of characters, starting at the given
		/// position, with the given number of copies of the given character
		/// (C++ replace(size_type pos, size_type count, size_type count2,
		/// CharT ch))
		/// </summary>
		/********************************************************************/
		public StdString replace(size_t pos, size_t count, size_t count2, uint8_t ch)
		{
			int at = Check_Pos(pos, "replace");

			Replace_Fill(at, Limit(at, count), Check_Length(count2), ch);

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the characters of the range [first, last) with the
		/// given number of copies of the given character (C++
		/// replace(const_iterator first, const_iterator last, size_type
		/// count2, CharT ch))
		/// </summary>
		/********************************************************************/
		public StdString replace(CPointer<uint8_t> first, CPointer<uint8_t> last, size_t count2, uint8_t ch)
		{
			int at = To_Index(first);

			Replace_Fill(at, Range_Length(first, last), Check_Length(count2), ch);

			return this;
		}
		#endregion

		#region Operations
		/********************************************************************/
		/// <summary>
		/// Compares the characters of the container with the characters of
		/// the given container (C++ compare(const basic_string＆ str)).
		/// Returns a negative value if this one is less, zero if they are
		/// equal and a positive value if this one is greater
		/// </summary>
		/********************************************************************/
		public c_int compare(StdString str)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			return Compare_Spans(Span(), str.Span());
		}



		/********************************************************************/
		/// <summary>
		/// Compares the given number of characters, starting at the given
		/// position, with the characters of the given container (C++
		/// compare(size_type pos1, size_type count1, const basic_string＆
		/// str))
		/// </summary>
		/********************************************************************/
		public c_int compare(size_t pos1, size_t count1, StdString str)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			int at = Check_Pos(pos1, "compare");

			return Compare_Spans(buffer.AsSpan(at, Limit(at, count1)), str.Span());
		}



		/********************************************************************/
		/// <summary>
		/// Compares the given number of characters, starting at the given
		/// position, with the characters of the given container from the
		/// given position of it and to its end (C++ compare(size_type pos1,
		/// size_type count1, const basic_string＆ str, size_type pos2,
		/// size_type count2))
		/// </summary>
		/********************************************************************/
		public c_int compare(size_t pos1, size_t count1, StdString str, size_t pos2)
		{
			return compare(pos1, count1, str, pos2, npos);
		}



		/********************************************************************/
		/// <summary>
		/// Compares the given number of characters, starting at the given
		/// position, with the given number of characters of the given
		/// container, starting at the given position of it (C++
		/// compare(size_type pos1, size_type count1, const basic_string＆
		/// str, size_type pos2, size_type count2))
		/// </summary>
		/********************************************************************/
		public c_int compare(size_t pos1, size_t count1, StdString str, size_t pos2, size_t count2)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			int at = Check_Pos(pos1, "compare");
			int start = str.Check_Pos(pos2, "compare");

			return Compare_Spans(buffer.AsSpan(at, Limit(at, count1)), str.buffer.AsSpan(start, str.Limit(start, count2)));
		}



		/********************************************************************/
		/// <summary>
		/// Compares the characters of the container with the characters of
		/// the given null terminated character array
		/// (C++ compare(const CharT* s))
		/// </summary>
		/********************************************************************/
		public c_int compare(CPointer<uint8_t> s)
		{
			return Compare_Spans(Span(), To_Span(s));
		}



		/********************************************************************/
		/// <summary>
		/// Compares the given number of characters, starting at the given
		/// position, with the characters of the given null terminated
		/// character array (C++ compare(size_type pos1, size_type count1,
		/// const CharT* s))
		/// </summary>
		/********************************************************************/
		public c_int compare(size_t pos1, size_t count1, CPointer<uint8_t> s)
		{
			int at = Check_Pos(pos1, "compare");

			return Compare_Spans(buffer.AsSpan(at, Limit(at, count1)), To_Span(s));
		}



		/********************************************************************/
		/// <summary>
		/// Compares the given number of characters, starting at the given
		/// position, with the first given number of characters of the given
		/// character array (C++ compare(size_type pos1, size_type count1,
		/// const CharT* s, size_type count2))
		/// </summary>
		/********************************************************************/
		public c_int compare(size_t pos1, size_t count1, CPointer<uint8_t> s, size_t count2)
		{
			int at = Check_Pos(pos1, "compare");

			return Compare_Spans(buffer.AsSpan(at, Limit(at, count1)), To_Span(s, count2));
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the container begins with the characters of the given
		/// container (C++ starts_with(basic_string_view sv))
		/// </summary>
		/********************************************************************/
		public bool starts_with(StdString str)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			return Span().StartsWith(str.Span());
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the container begins with the given character
		/// (C++ starts_with(CharT ch))
		/// </summary>
		/********************************************************************/
		public bool starts_with(uint8_t ch)
		{
			return (count > 0) && (buffer[0] == ch);
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the container begins with the characters of the given
		/// null terminated character array
		/// (C++ starts_with(const CharT* s))
		/// </summary>
		/********************************************************************/
		public bool starts_with(CPointer<uint8_t> s)
		{
			return Span().StartsWith(To_Span(s));
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the container ends with the characters of the given
		/// container (C++ ends_with(basic_string_view sv))
		/// </summary>
		/********************************************************************/
		public bool ends_with(StdString str)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			return Span().EndsWith(str.Span());
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the container ends with the given character
		/// (C++ ends_with(CharT ch))
		/// </summary>
		/********************************************************************/
		public bool ends_with(uint8_t ch)
		{
			return (count > 0) && (buffer[count - 1] == ch);
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the container ends with the characters of the given
		/// null terminated character array
		/// (C++ ends_with(const CharT* s))
		/// </summary>
		/********************************************************************/
		public bool ends_with(CPointer<uint8_t> s)
		{
			return Span().EndsWith(To_Span(s));
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the container holds the characters of the given
		/// container (C++ contains(basic_string_view sv))
		/// </summary>
		/********************************************************************/
		public bool contains(StdString str)
		{
			return find(str) != npos;
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the container holds the given character
		/// (C++ contains(CharT ch))
		/// </summary>
		/********************************************************************/
		public bool contains(uint8_t ch)
		{
			return find(ch) != npos;
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the container holds the characters of the given null
		/// terminated character array (C++ contains(const CharT* s))
		/// </summary>
		/********************************************************************/
		public bool contains(CPointer<uint8_t> s)
		{
			return find(s) != npos;
		}



		/********************************************************************/
		/// <summary>
		/// Returns a container holding all the characters of this one
		/// (C++ substr(size_type pos, size_type count))
		/// </summary>
		/********************************************************************/
		public StdString substr()
		{
			return substr(0, npos);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a container holding the characters from the given
		/// position and to the end of this one
		/// (C++ substr(size_type pos, size_type count))
		/// </summary>
		/********************************************************************/
		public StdString substr(size_t pos)
		{
			return substr(pos, npos);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a container holding the given number of characters of
		/// this one, starting at the given position
		/// (C++ substr(size_type pos, size_type count))
		/// </summary>
		/********************************************************************/
		public StdString substr(size_t pos, size_t count)
		{
			int at = Check_Pos(pos, "substr");

			StdString result = new StdString();
			result.Replace_Span(0, 0, buffer.AsSpan(at, Limit(at, count)));

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Copies the given number of characters, starting at the given
		/// position, to the given character array. The copy is not null
		/// terminated. Returns the number of characters copied
		/// (C++ copy(CharT* dest, size_type count, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t copy(CPointer<uint8_t> dest, size_t count, size_t pos)
		{
			if (dest.IsNull)
				throw new ArgumentNullException(nameof(dest));

			int at = Check_Pos(pos, "copy");
			int n = Limit(at, count);

			buffer.AsSpan(at, n).CopyTo(dest.AsSpan(n));

			return (size_t)n;
		}



		/********************************************************************/
		/// <summary>
		/// Copies the given number of characters, starting at the first
		/// one, to the given character array. The copy is not null
		/// terminated. Returns the number of characters copied
		/// (C++ copy(CharT* dest, size_type count, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t copy(CPointer<uint8_t> dest, size_t count)
		{
			return copy(dest, count, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Resizes the container to hold the given number of characters. If
		/// the container is grown, the new characters are null characters
		/// (C++ resize(size_type count))
		/// </summary>
		/********************************************************************/
		public void resize(size_t count)
		{
			resize(count, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Resizes the container to hold the given number of characters. If
		/// the container is grown, the new characters are copies of the
		/// given character (C++ resize(size_type count, CharT ch))
		/// </summary>
		/********************************************************************/
		public void resize(size_t count, uint8_t ch)
		{
			int n = Check_Length(count);

			if (n < this.count)
				Set_Size(n);
			else if (n > this.count)
				Replace_Fill(this.count, 0, n - this.count, ch);
		}



		/********************************************************************/
		/// <summary>
		/// Exchanges the characters of the container with those of the
		/// other container (C++ swap(basic_string＆ other))
		/// </summary>
		/********************************************************************/
		public void swap(StdString other)
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));

			(buffer, other.buffer) = (other.buffer, buffer);
			(count, other.count) = (other.count, count);
		}
		#endregion

		#region Search
		/********************************************************************/
		/// <summary>
		/// Finds the first occurrence of the characters of the given
		/// container, at or after the given position. Returns npos if there
		/// is no such occurrence
		/// (C++ find(const basic_string＆ str, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find(StdString str, size_t pos)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			return Find_Span(str.Span(), pos);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first occurrence of the characters of the given
		/// container. Returns npos if there is no such occurrence
		/// (C++ find(const basic_string＆ str, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find(StdString str)
		{
			return find(str, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first occurrence of the first given number of
		/// characters of the given character array, at or after the given
		/// position. The array may contain null characters. Returns npos if
		/// there is no such occurrence
		/// (C++ find(const CharT* s, size_type pos, size_type count))
		/// </summary>
		/********************************************************************/
		public size_t find(CPointer<uint8_t> s, size_t pos, size_t count)
		{
			return Find_Span(To_Span(s, count), pos);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first occurrence of the characters of the given null
		/// terminated character array, at or after the given position.
		/// Returns npos if there is no such occurrence
		/// (C++ find(const CharT* s, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find(CPointer<uint8_t> s, size_t pos)
		{
			return Find_Span(To_Span(s), pos);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first occurrence of the characters of the given null
		/// terminated character array. Returns npos if there is no such
		/// occurrence (C++ find(const CharT* s, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find(CPointer<uint8_t> s)
		{
			return Find_Span(To_Span(s), 0);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first occurrence of the given character, at or after
		/// the given position. Returns npos if there is no such occurrence
		/// (C++ find(CharT ch, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find(uint8_t ch, size_t pos)
		{
			ReadOnlySpan<uint8_t> pattern = [ ch ];

			return Find_Span(pattern, pos);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first occurrence of the given character. Returns npos
		/// if there is no such occurrence
		/// (C++ find(CharT ch, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find(uint8_t ch)
		{
			return find(ch, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last occurrence of the characters of the given
		/// container, beginning at or before the given position. Returns
		/// npos if there is no such occurrence
		/// (C++ rfind(const basic_string＆ str, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t rfind(StdString str, size_t pos)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			return RFind_Span(str.Span(), pos);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last occurrence of the characters of the given
		/// container. Returns npos if there is no such occurrence
		/// (C++ rfind(const basic_string＆ str, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t rfind(StdString str)
		{
			return rfind(str, npos);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last occurrence of the first given number of
		/// characters of the given character array, beginning at or before
		/// the given position. The array may contain null characters.
		/// Returns npos if there is no such occurrence
		/// (C++ rfind(const CharT* s, size_type pos, size_type count))
		/// </summary>
		/********************************************************************/
		public size_t rfind(CPointer<uint8_t> s, size_t pos, size_t count)
		{
			return RFind_Span(To_Span(s, count), pos);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last occurrence of the characters of the given null
		/// terminated character array, beginning at or before the given
		/// position. Returns npos if there is no such occurrence
		/// (C++ rfind(const CharT* s, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t rfind(CPointer<uint8_t> s, size_t pos)
		{
			return RFind_Span(To_Span(s), pos);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last occurrence of the characters of the given null
		/// terminated character array. Returns npos if there is no such
		/// occurrence (C++ rfind(const CharT* s, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t rfind(CPointer<uint8_t> s)
		{
			return RFind_Span(To_Span(s), npos);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last occurrence of the given character, at or before
		/// the given position. Returns npos if there is no such occurrence
		/// (C++ rfind(CharT ch, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t rfind(uint8_t ch, size_t pos)
		{
			ReadOnlySpan<uint8_t> pattern = [ ch ];

			return RFind_Span(pattern, pos);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last occurrence of the given character. Returns npos
		/// if there is no such occurrence
		/// (C++ rfind(CharT ch, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t rfind(uint8_t ch)
		{
			return rfind(ch, npos);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first character, at or after the given position, that
		/// is one of the characters of the given container. Returns npos if
		/// there is no such character
		/// (C++ find_first_of(const basic_string＆ str, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_first_of(StdString str, size_t pos)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			return Find_Of(str.Span(), pos, true);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first character that is one of the characters of the
		/// given container. Returns npos if there is no such character
		/// (C++ find_first_of(const basic_string＆ str, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_first_of(StdString str)
		{
			return find_first_of(str, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first character, at or after the given position, that
		/// is one of the first given number of characters of the given
		/// character array. Returns npos if there is no such character (C++
		/// find_first_of(const CharT* s, size_type pos, size_type count))
		/// </summary>
		/********************************************************************/
		public size_t find_first_of(CPointer<uint8_t> s, size_t pos, size_t count)
		{
			return Find_Of(To_Span(s, count), pos, true);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first character, at or after the given position, that
		/// is one of the characters of the given null terminated character
		/// array. Returns npos if there is no such character
		/// (C++ find_first_of(const CharT* s, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_first_of(CPointer<uint8_t> s, size_t pos)
		{
			return Find_Of(To_Span(s), pos, true);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first character that is one of the characters of the
		/// given null terminated character array. Returns npos if there is
		/// no such character
		/// (C++ find_first_of(const CharT* s, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_first_of(CPointer<uint8_t> s)
		{
			return Find_Of(To_Span(s), 0, true);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first occurrence of the given character, at or after
		/// the given position. Returns npos if there is no such character
		/// (C++ find_first_of(CharT ch, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_first_of(uint8_t ch, size_t pos)
		{
			ReadOnlySpan<uint8_t> chars = [ ch ];

			return Find_Of(chars, pos, true);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first occurrence of the given character. Returns npos
		/// if there is no such character
		/// (C++ find_first_of(CharT ch, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_first_of(uint8_t ch)
		{
			return find_first_of(ch, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first character, at or after the given position, that
		/// is not one of the characters of the given container. Returns
		/// npos if there is no such character
		/// (C++ find_first_not_of(const basic_string＆ str, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_first_not_of(StdString str, size_t pos)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			return Find_Of(str.Span(), pos, false);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first character that is not one of the characters of
		/// the given container. Returns npos if there is no such character
		/// (C++ find_first_not_of(const basic_string＆ str, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_first_not_of(StdString str)
		{
			return find_first_not_of(str, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first character, at or after the given position, that
		/// is not one of the first given number of characters of the given
		/// character array. Returns npos if there is no such character (C++
		/// find_first_not_of(const CharT* s, size_type pos, size_type
		/// count))
		/// </summary>
		/********************************************************************/
		public size_t find_first_not_of(CPointer<uint8_t> s, size_t pos, size_t count)
		{
			return Find_Of(To_Span(s, count), pos, false);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first character, at or after the given position, that
		/// is not one of the characters of the given null terminated
		/// character array. Returns npos if there is no such character
		/// (C++ find_first_not_of(const CharT* s, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_first_not_of(CPointer<uint8_t> s, size_t pos)
		{
			return Find_Of(To_Span(s), pos, false);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first character that is not one of the characters of
		/// the given null terminated character array. Returns npos if there
		/// is no such character
		/// (C++ find_first_not_of(const CharT* s, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_first_not_of(CPointer<uint8_t> s)
		{
			return Find_Of(To_Span(s), 0, false);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first character, at or after the given position, that
		/// is not the given character. Returns npos if there is no such
		/// character (C++ find_first_not_of(CharT ch, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_first_not_of(uint8_t ch, size_t pos)
		{
			ReadOnlySpan<uint8_t> chars = [ ch ];

			return Find_Of(chars, pos, false);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first character that is not the given character.
		/// Returns npos if there is no such character
		/// (C++ find_first_not_of(CharT ch, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_first_not_of(uint8_t ch)
		{
			return find_first_not_of(ch, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last character, at or before the given position, that
		/// is one of the characters of the given container. Returns npos if
		/// there is no such character
		/// (C++ find_last_of(const basic_string＆ str, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_last_of(StdString str, size_t pos)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			return Find_Last_Of(str.Span(), pos, true);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last character that is one of the characters of the
		/// given container. Returns npos if there is no such character
		/// (C++ find_last_of(const basic_string＆ str, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_last_of(StdString str)
		{
			return find_last_of(str, npos);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last character, at or before the given position, that
		/// is one of the first given number of characters of the given
		/// character array. Returns npos if there is no such character (C++
		/// find_last_of(const CharT* s, size_type pos, size_type count))
		/// </summary>
		/********************************************************************/
		public size_t find_last_of(CPointer<uint8_t> s, size_t pos, size_t count)
		{
			return Find_Last_Of(To_Span(s, count), pos, true);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last character, at or before the given position, that
		/// is one of the characters of the given null terminated character
		/// array. Returns npos if there is no such character
		/// (C++ find_last_of(const CharT* s, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_last_of(CPointer<uint8_t> s, size_t pos)
		{
			return Find_Last_Of(To_Span(s), pos, true);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last character that is one of the characters of the
		/// given null terminated character array. Returns npos if there is
		/// no such character
		/// (C++ find_last_of(const CharT* s, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_last_of(CPointer<uint8_t> s)
		{
			return Find_Last_Of(To_Span(s), npos, true);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last occurrence of the given character, at or before
		/// the given position. Returns npos if there is no such character
		/// (C++ find_last_of(CharT ch, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_last_of(uint8_t ch, size_t pos)
		{
			ReadOnlySpan<uint8_t> chars = [ ch ];

			return Find_Last_Of(chars, pos, true);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last occurrence of the given character. Returns npos
		/// if there is no such character
		/// (C++ find_last_of(CharT ch, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_last_of(uint8_t ch)
		{
			return find_last_of(ch, npos);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last character, at or before the given position, that
		/// is not one of the characters of the given container. Returns
		/// npos if there is no such character
		/// (C++ find_last_not_of(const basic_string＆ str, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_last_not_of(StdString str, size_t pos)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			return Find_Last_Of(str.Span(), pos, false);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last character that is not one of the characters of
		/// the given container. Returns npos if there is no such character
		/// (C++ find_last_not_of(const basic_string＆ str, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_last_not_of(StdString str)
		{
			return find_last_not_of(str, npos);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last character, at or before the given position, that
		/// is not one of the first given number of characters of the given
		/// character array. Returns npos if there is no such character (C++
		/// find_last_not_of(const CharT* s, size_type pos, size_type count))
		/// </summary>
		/********************************************************************/
		public size_t find_last_not_of(CPointer<uint8_t> s, size_t pos, size_t count)
		{
			return Find_Last_Of(To_Span(s, count), pos, false);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last character, at or before the given position, that
		/// is not one of the characters of the given null terminated
		/// character array. Returns npos if there is no such character
		/// (C++ find_last_not_of(const CharT* s, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_last_not_of(CPointer<uint8_t> s, size_t pos)
		{
			return Find_Last_Of(To_Span(s), pos, false);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last character that is not one of the characters of
		/// the given null terminated character array. Returns npos if there
		/// is no such character
		/// (C++ find_last_not_of(const CharT* s, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_last_not_of(CPointer<uint8_t> s)
		{
			return Find_Last_Of(To_Span(s), npos, false);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last character, at or before the given position, that
		/// is not the given character. Returns npos if there is no such
		/// character (C++ find_last_not_of(CharT ch, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_last_not_of(uint8_t ch, size_t pos)
		{
			ReadOnlySpan<uint8_t> chars = [ ch ];

			return Find_Last_Of(chars, pos, false);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last character that is not the given character.
		/// Returns npos if there is no such character
		/// (C++ find_last_not_of(CharT ch, size_type pos))
		/// </summary>
		/********************************************************************/
		public size_t find_last_not_of(uint8_t ch)
		{
			return find_last_not_of(ch, npos);
		}
		#endregion

		#region Concatenation operators
		/********************************************************************/
		/// <summary>
		/// Returns a container holding the characters of the two given
		/// containers, one after the other
		/// (C++ operator+(const basic_string＆, const basic_string＆))
		/// </summary>
		/********************************************************************/
		public static StdString operator +(StdString left, StdString right)
		{
			if (left == null)
				throw new ArgumentNullException(nameof(left));

			return new StdString(left).append(right);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a container holding the characters of the given
		/// container followed by the characters of the given null
		/// terminated character array
		/// (C++ operator+(const basic_string＆, const CharT*))
		/// </summary>
		/********************************************************************/
		public static StdString operator +(StdString left, CPointer<uint8_t> right)
		{
			if (left == null)
				throw new ArgumentNullException(nameof(left));

			return new StdString(left).append(right);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a container holding the characters of the given null
		/// terminated character array followed by the characters of the
		/// given container
		/// (C++ operator+(const CharT*, const basic_string＆))
		/// </summary>
		/********************************************************************/
		public static StdString operator +(CPointer<uint8_t> left, StdString right)
		{
			if (right == null)
				throw new ArgumentNullException(nameof(right));

			return new StdString(left).append(right);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a container holding the characters of the given
		/// container followed by the given character
		/// (C++ operator+(const basic_string＆, CharT))
		/// </summary>
		/********************************************************************/
		public static StdString operator +(StdString left, uint8_t right)
		{
			if (left == null)
				throw new ArgumentNullException(nameof(left));

			StdString result = new StdString(left);
			result.push_back(right);

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Returns a container holding the given character followed by the
		/// characters of the given container
		/// (C++ operator+(CharT, const basic_string＆))
		/// </summary>
		/********************************************************************/
		public static StdString operator +(uint8_t left, StdString right)
		{
			if (right == null)
				throw new ArgumentNullException(nameof(right));

			StdString result = new StdString((size_t)1, left);

			return result.append(right);
		}
		#endregion

		#region Comparison operators
		/********************************************************************/
		/// <summary>
		/// Checks if the characters of the two containers are equal
		/// (C++ operator==)
		/// </summary>
		/********************************************************************/
		public static bool operator ==(StdString left, StdString right)
		{
			if (ReferenceEquals(left, right))
				return true;

			if ((left is null) || (right is null))
				return false;

			return left.Equals(right);
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the characters of the two containers are not equal
		/// (C++ operator!=)
		/// </summary>
		/********************************************************************/
		public static bool operator !=(StdString left, StdString right)
		{
			return !(left == right);
		}



		/********************************************************************/
		/// <summary>
		/// Compares the characters of the two containers
		/// lexicographically (C++ operator‹)
		/// </summary>
		/********************************************************************/
		public static bool operator <(StdString left, StdString right)
		{
			return Compare(left, right) < 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the characters of the two containers
		/// lexicographically (C++ operator‹=)
		/// </summary>
		/********************************************************************/
		public static bool operator <=(StdString left, StdString right)
		{
			return Compare(left, right) <= 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the characters of the two containers
		/// lexicographically (C++ operator›)
		/// </summary>
		/********************************************************************/
		public static bool operator >(StdString left, StdString right)
		{
			return Compare(left, right) > 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the characters of the two containers
		/// lexicographically (C++ operator›=)
		/// </summary>
		/********************************************************************/
		public static bool operator >=(StdString left, StdString right)
		{
			return Compare(left, right) >= 0;
		}
		#endregion

		#region IEquatable implementation
		/********************************************************************/
		/// <summary>
		/// Checks if the characters of this container are equal to the
		/// characters of the other container
		/// </summary>
		/********************************************************************/
		public bool Equals(StdString other)
		{
			if (other is null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			return Span().SequenceEqual(other.Span());
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the characters of this container are equal to the
		/// characters of the other container
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return Equals(obj as StdString);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a hash code based on the characters of the container
		/// </summary>
		/********************************************************************/
		public override int GetHashCode()
		{
			HashCode hash = new HashCode();

			hash.AddBytes(Span());

			return hash.ToHashCode();
		}
		#endregion

		#region IDeepCloneable implementation
		/********************************************************************/
		/// <summary>
		/// Returns an independent copy of the container, holding a copy of
		/// the characters. This is the same as using the copy constructor
		/// StdString(StdString)
		/// </summary>
		/********************************************************************/
		public virtual StdString MakeDeepClone()
		{
			return new StdString(this);
		}
		#endregion

		#region ICopyTo implementation
		/********************************************************************/
		/// <summary>
		/// Replaces the characters of the given container with a copy of
		/// the characters of this one. This is the same as C++ copy
		/// assignment (destination = *this), so it invalidates all
		/// pointers, iterators and references that refer to characters of
		/// the destination
		/// </summary>
		/********************************************************************/
		public void CopyTo(StdString destination)
		{
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));

			if (ReferenceEquals(destination, this))
				return;

			destination.Replace_Span(0, destination.count, Span());
		}
		#endregion

		#region IMoveable implementation
		/********************************************************************/
		/// <summary>
		/// Returns a container that has taken over the characters of this
		/// one, leaving this one empty. This is what C++ move construction
		/// (basic_string(basic_string＆＆)) does: the characters are handed
		/// over as they are and not copied, so the pointers, iterators and
		/// references that refer to them stay valid and now refer to
		/// characters of the returned container
		/// </summary>
		/********************************************************************/
		public virtual StdString MoveFrom()
		{
			StdString moved = new StdString();

			moved.buffer = buffer;
			moved.count = count;

			buffer = new uint8_t[1];
			count = 0;

			return moved;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Returns the characters of the container as a C# string, decoded
		/// as Latin1, so that every one of the size() characters becomes the
		/// character with the same value. Null characters are kept, as they
		/// are ordinary characters of the container
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return Encoding.Latin1.GetString(Span());
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Returns the characters of the container as a span
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ReadOnlySpan<uint8_t> Span()
		{
			return buffer.AsSpan(0, count);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the characters of the given null terminated character
		/// array as a span, not including the null character
		/// </summary>
		/********************************************************************/
		private static ReadOnlySpan<uint8_t> To_Span(CPointer<uint8_t> s)
		{
			if (s.IsNull)
				throw new ArgumentNullException(nameof(s));

			int len = 0;
			int maxLength = s.Length;

			while ((len < maxLength) && (s[len] != 0))
				len++;

			return s.AsSpan(len);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the first given number of characters of the given
		/// character array as a span
		/// </summary>
		/********************************************************************/
		private static ReadOnlySpan<uint8_t> To_Span(CPointer<uint8_t> s, size_t count)
		{
			int n = Check_Length(count);

			if (s.IsNull && (n > 0))
				throw new ArgumentNullException(nameof(s));

			return n == 0 ? ReadOnlySpan<uint8_t>.Empty : s.AsSpan(n);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the characters of the range [first, last) as a span.
		/// Both pointers need to use the same buffer
		/// </summary>
		/********************************************************************/
		private static ReadOnlySpan<uint8_t> To_Span(CPointer<uint8_t> first, CPointer<uint8_t> last)
		{
			int n = last - first;

			if (n < 0)
				throw new ArgumentException("last is before first");

			return n == 0 ? ReadOnlySpan<uint8_t>.Empty : first.AsSpan(n);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of characters of the range [first, last)
		/// </summary>
		/********************************************************************/
		private static int Range_Length(CPointer<uint8_t> first, CPointer<uint8_t> last)
		{
			int n = last - first;

			if (n < 0)
				throw new ArgumentException("last is before first");

			return n;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the character index that the given pointer refers to
		/// </summary>
		/********************************************************************/
		private int To_Index(CPointer<uint8_t> pos)
		{
			return pos - new CPointer<uint8_t>(buffer, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Checks that the given position is one that a character may be
		/// inserted at or read from, and returns it as an index. Throws
		/// out_of_range if it is greater than size()
		/// </summary>
		/********************************************************************/
		private int Check_Pos(size_t pos, string methodName)
		{
			if (pos > (size_t)count)
				throw new out_of_range($"basic_string.{methodName}: pos (which is {pos}) > this->size() (which is {count})");

			return (int)pos;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the given number of characters, limited to the number of
		/// characters left after the given index. This is what the C++
		/// standard calls rlen
		/// </summary>
		/********************************************************************/
		private int Limit(int pos, size_t count)
		{
			int available = this.count - pos;

			return count > (size_t)available ? available : (int)count;
		}



		/********************************************************************/
		/// <summary>
		/// Checks that the given number of characters can be held by a
		/// container, and returns it as an int. Throws length_error if it
		/// is greater than max_size()
		/// </summary>
		/********************************************************************/
		private static int Check_Length(size_t count)
		{
			if (count > (size_t)(Array.MaxLength - 1))
				throw new length_error("basic_string: requested size is larger than max_size()");

			return (int)count;
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the given number of characters, starting at the given
		/// index, with the given characters. The source is copied before
		/// anything is changed, so it may refer into this container's own
		/// buffer
		/// </summary>
		/********************************************************************/
		private void Replace_Span(int index, int n, ReadOnlySpan<uint8_t> source)
		{
			uint8_t[] tmp = source.ToArray();
			int newCount = Check_Length((size_t)((long)count - n + tmp.Length));

			Ensure_Capacity(newCount);

			if (count - index - n > 0)
				Array.Copy(buffer, index + n, buffer, index + tmp.Length, count - index - n);

			tmp.CopyTo(buffer, index);

			Set_Size(newCount);
		}



		/********************************************************************/
		/// <summary>
		/// Replaces the given number of characters, starting at the given
		/// index, with the given number of copies of the given character
		/// </summary>
		/********************************************************************/
		private void Replace_Fill(int index, int n, int fillCount, uint8_t ch)
		{
			int newCount = Check_Length((size_t)((long)count - n + fillCount));

			Ensure_Capacity(newCount);

			if (count - index - n > 0)
				Array.Copy(buffer, index + n, buffer, index + fillCount, count - index - n);

			if (fillCount > 0)
				buffer.AsSpan(index, fillCount).Fill(ch);

			Set_Size(newCount);
		}



		/********************************************************************/
		/// <summary>
		/// Sets the number of characters in the container and writes the
		/// null character that always follows the last one
		/// </summary>
		/********************************************************************/
		private void Set_Size(int newCount)
		{
			count = newCount;
			buffer[newCount] = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Ensures that the buffer can hold at least the given number of
		/// characters plus the null character, growing it geometrically if
		/// needed
		/// </summary>
		/********************************************************************/
		private void Ensure_Capacity(int needed)
		{
			int currentCapacity = buffer.Length - 1;

			if (needed <= currentCapacity)
				return;

			int newCapacity = currentCapacity == 0 ? 1 : currentCapacity * 2;

			if (newCapacity < needed)
				newCapacity = needed;

			Reallocate(newCapacity);
		}



		/********************************************************************/
		/// <summary>
		/// Allocates a new buffer that can hold the given number of
		/// characters plus the null character, and copies the current
		/// characters into it
		/// </summary>
		/********************************************************************/
		private void Reallocate(int newCapacity)
		{
			uint8_t[] newBuffer = new uint8_t[newCapacity + 1];

			if (count > 0)
				Array.Copy(buffer, newBuffer, count);

			buffer = newBuffer;
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first occurrence of the given characters, at or after
		/// the given position
		/// </summary>
		/********************************************************************/
		private size_t Find_Span(ReadOnlySpan<uint8_t> pattern, size_t pos)
		{
			if (pos > (size_t)count)
				return npos;

			int start = (int)pos;
			int last = count - pattern.Length;

			if (start > last)
				return npos;

			int index = buffer.AsSpan(start, count - start).IndexOf(pattern);

			return index < 0 ? npos : (size_t)(start + index);
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last occurrence of the given characters, beginning at
		/// or before the given position
		/// </summary>
		/********************************************************************/
		private size_t RFind_Span(ReadOnlySpan<uint8_t> pattern, size_t pos)
		{
			int n = pattern.Length;

			if (n > count)
				return npos;

			int maxStart = count - n;
			int start = pos > (size_t)maxStart ? maxStart : (int)pos;

			if (n == 0)
				return (size_t)start;

			int index = buffer.AsSpan(0, start + n).LastIndexOf(pattern);

			return index < 0 ? npos : (size_t)index;
		}



		/********************************************************************/
		/// <summary>
		/// Finds the first character, at or after the given position, that
		/// is one of the given characters (wanted is true) or that is not
		/// one of them (wanted is false)
		/// </summary>
		/********************************************************************/
		private size_t Find_Of(ReadOnlySpan<uint8_t> chars, size_t pos, bool wanted)
		{
			if (pos >= (size_t)count)
				return npos;

			for (int i = (int)pos; i < count; i++)
			{
				if ((chars.IndexOf(buffer[i]) >= 0) == wanted)
					return (size_t)i;
			}

			return npos;
		}



		/********************************************************************/
		/// <summary>
		/// Finds the last character, at or before the given position, that
		/// is one of the given characters (wanted is true) or that is not
		/// one of them (wanted is false)
		/// </summary>
		/********************************************************************/
		private size_t Find_Last_Of(ReadOnlySpan<uint8_t> chars, size_t pos, bool wanted)
		{
			if (count == 0)
				return npos;

			int start = pos >= (size_t)count ? count - 1 : (int)pos;

			for (int i = start; i >= 0; i--)
			{
				if ((chars.IndexOf(buffer[i]) >= 0) == wanted)
					return (size_t)i;
			}

			return npos;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the two given character sequences lexicographically,
		/// returning a negative value, zero or a positive value
		/// </summary>
		/********************************************************************/
		private static c_int Compare_Spans(ReadOnlySpan<uint8_t> left, ReadOnlySpan<uint8_t> right)
		{
			int n = Math.Min(left.Length, right.Length);

			for (int i = 0; i < n; i++)
			{
				if (left[i] != right[i])
					return left[i] - right[i];
			}

			return left.Length.CompareTo(right.Length);
		}



		/********************************************************************/
		/// <summary>
		/// Compares the characters of the two containers lexicographically,
		/// returning a negative value, zero or a positive value
		/// </summary>
		/********************************************************************/
		private static c_int Compare(StdString left, StdString right)
		{
			if (ReferenceEquals(left, right))
				return 0;

			if (left is null)
				return -1;

			if (right is null)
				return 1;

			return Compare_Spans(left.Span(), right.Span());
		}
		#endregion
	}
}
