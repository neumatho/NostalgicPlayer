/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Polycode.NostalgicPlayer.Kit.C.Std.Exceptions;

namespace Polycode.NostalgicPlayer.Kit.C.Std
{
	/// <summary>
	/// C# port of the C++ standard library std::bitset
	/// (see the C++ standard, [template.bitset]).
	///
	/// A fixed size sequence of N bits. Each bit can be accessed and
	/// individually toggled, and the whole sequence can be combined with the
	/// usual bitwise operators. The bits are packed into an array of 64 bit
	/// words, so that operations work on many bits at a time.
	///
	/// In C++, N is a compile time template argument (std::bitset‹N›). C#
	/// generics cannot take such a non-type argument, so here N is passed to
	/// the constructor instead (just like <see cref="array{T}"/>) and stored.
	///
	/// Unlike a C++ std::bitset, this is a reference type (a C# class), so
	/// assigning one bitset variable to another makes both refer to the same
	/// container. To make an independent copy (the behavior of C++ copy
	/// construction), use the copy constructor bitset(bitset).
	///
	/// Notable differences from C++:
	/// - The reference proxy returned by the non-const operator[] is not
	///   modelled. The indexer get returns the bit value and the indexer set
	///   assigns it, which covers the common uses.
	/// - The compound operators (＆=, |=, ^=, ‹‹=, ››=) cannot be overloaded
	///   in C#. The binary operators (＆, |, ^, ~, ‹‹, ››) are provided
	///   instead, from which C# derives the compound forms
	/// </summary>
#pragma warning disable CS8981
	public class bitset : IEquatable<bitset>
	{
#pragma warning restore CS8981
		private const int Bits_Per_Word = 64;

		private readonly int nBits;
		private readonly ulong[] words;

		/********************************************************************/
		/// <summary>
		/// Constructs a bitset of the given number of bits, with every bit
		/// set to zero (C++ std::bitset‹N›())
		/// </summary>
		/********************************************************************/
		public bitset(size_t nBits)
		{
			this.nBits = (int)nBits;
			words = Make_Words(this.nBits);
		}



		/********************************************************************/
		/// <summary>
		/// Constructs a bitset of the given number of bits, initializing the
		/// low order bits from the given value (C++ bitset(unsigned long long
		/// val)). Bits at positions beyond the value width are set to zero
		/// </summary>
		/********************************************************************/
		public bitset(size_t nBits, c_ulong_long val)
		{
			this.nBits = (int)nBits;
			words = Make_Words(this.nBits);

			if (words.Length > 0)
			{
				words[0] = val;
				Trim();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Constructs a bitset of the given number of bits from a string of
		/// zero and one characters (C++ bitset(basic_string, pos, n, zero,
		/// one)). The character at position pos is the most significant bit
		/// used. Throws out_of_range if pos is past the end of the string and
		/// invalid_argument if a used character is neither zero nor one
		/// </summary>
		/********************************************************************/
		public bitset(size_t nBits, string str, size_t pos = 0, size_t n = size_t.MaxValue, char zero = '0', char one = '1')
		{
			this.nBits = (int)nBits;
			words = Make_Words(this.nBits);

			if (str == null)
				throw new ArgumentNullException(nameof(str));

			if (pos > (size_t)str.Length)
				throw new out_of_range($"bitset: pos (which is {pos}) > str.size() (which is {str.Length})");

			// The number of characters actually available from pos
			size_t rlen = Math.Min(n, (size_t)str.Length - pos);

			// Validate every available character first
			for (size_t i = 0; i < rlen; i++)
			{
				char c = str[(int)(pos + i)];

				if ((c != zero) && (c != one))
					throw new invalid_argument($"bitset: character '{c}' is neither '{zero}' nor '{one}'");
			}

			// Only the first min(rlen, N) characters end up as bits. The
			// character at pos + m - 1 is bit 0
			int m = (int)Math.Min(rlen, (size_t)this.nBits);

			for (int i = 0; i < m; i++)
			{
				if (str[(int)pos + m - 1 - i] == one)
					Set_Bit(i);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Copy constructor. Constructs an independent bitset with a copy of
		/// the bits of the given bitset (C++ bitset(const bitset＆))
		/// </summary>
		/********************************************************************/
		public bitset(bitset other)
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));

			nBits = other.nBits;
			words = (ulong[])other.words.Clone();
		}

		#region Element access
		/********************************************************************/
		/// <summary>
		/// Gets or sets the bit at the given position, without bounds
		/// checking (C++ operator[](size_t pos))
		/// </summary>
		/********************************************************************/
		public bool this[size_t pos]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Get_Bit((int)pos);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				if (value)
					Set_Bit((int)pos);
				else
					Clear_Bit((int)pos);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Returns the value of the bit at the given position, with bounds
		/// checking (C++ test(size_t pos)). Throws out_of_range if the
		/// position is not within the range of the bitset
		/// </summary>
		/********************************************************************/
		public bool test(size_t pos)
		{
			if (pos >= (size_t)nBits)
				throw new out_of_range($"bitset.test: pos (which is {pos}) >= this->size() (which is {nBits})");

			return Get_Bit((int)pos);
		}
		#endregion

		#region Capacity
		/********************************************************************/
		/// <summary>
		/// Returns the number of bits in the bitset, which is always N
		/// (C++ size())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t size()
		{
			return (size_t)nBits;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of bits that are set to one (C++ count())
		/// </summary>
		/********************************************************************/
		public size_t count()
		{
			int result = 0;

			for (int i = 0; i < words.Length; i++)
				result += BitOperations.PopCount(words[i]);

			return (size_t)result;
		}



		/********************************************************************/
		/// <summary>
		/// Checks if all the bits are set to one (C++ all()). A bitset of
		/// zero bits is considered to have all its bits set
		/// </summary>
		/********************************************************************/
		public bool all()
		{
			return count() == (size_t)nBits;
		}



		/********************************************************************/
		/// <summary>
		/// Checks if any of the bits is set to one (C++ any())
		/// </summary>
		/********************************************************************/
		public bool any()
		{
			for (int i = 0; i < words.Length; i++)
			{
				if (words[i] != 0)
					return true;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Checks if none of the bits is set to one (C++ none())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool none()
		{
			return !any();
		}
		#endregion

		#region Modifiers
		/********************************************************************/
		/// <summary>
		/// Sets every bit to one (C++ set())
		/// </summary>
		/********************************************************************/
		public bitset set()
		{
			for (int i = 0; i < words.Length; i++)
				words[i] = ulong.MaxValue;

			Trim();

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Sets the bit at the given position to the given value
		/// (C++ set(size_t pos, bool value = true)). Throws out_of_range if
		/// the position is not within the range of the bitset
		/// </summary>
		/********************************************************************/
		public bitset set(size_t pos, bool value = true)
		{
			if (pos >= (size_t)nBits)
				throw new out_of_range($"bitset.set: pos (which is {pos}) >= this->size() (which is {nBits})");

			if (value)
				Set_Bit((int)pos);
			else
				Clear_Bit((int)pos);

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Sets every bit to zero (C++ reset())
		/// </summary>
		/********************************************************************/
		public bitset reset()
		{
			Array.Clear(words);

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Sets the bit at the given position to zero (C++ reset(size_t
		/// pos)). Throws out_of_range if the position is not within the range
		/// of the bitset
		/// </summary>
		/********************************************************************/
		public bitset reset(size_t pos)
		{
			if (pos >= (size_t)nBits)
				throw new out_of_range($"bitset.reset: pos (which is {pos}) >= this->size() (which is {nBits})");

			Clear_Bit((int)pos);

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Flips every bit, turning ones into zeros and zeros into ones
		/// (C++ flip())
		/// </summary>
		/********************************************************************/
		public bitset flip()
		{
			for (int i = 0; i < words.Length; i++)
				words[i] = ~words[i];

			Trim();

			return this;
		}



		/********************************************************************/
		/// <summary>
		/// Flips the bit at the given position (C++ flip(size_t pos)). Throws
		/// out_of_range if the position is not within the range of the bitset
		/// </summary>
		/********************************************************************/
		public bitset flip(size_t pos)
		{
			if (pos >= (size_t)nBits)
				throw new out_of_range($"bitset.flip: pos (which is {pos}) >= this->size() (which is {nBits})");

			int p = (int)pos;
			words[p / Bits_Per_Word] ^= 1UL << (p % Bits_Per_Word);

			return this;
		}
		#endregion

		#region Bitwise operators
		/********************************************************************/
		/// <summary>
		/// Returns a bitset with the bitwise AND of the two bitsets
		/// (C++ operator＆). Both bitsets must have the same size
		/// </summary>
		/********************************************************************/
		public static bitset operator &(bitset left, bitset right)
		{
			Check_Same_Size(left, right);

			bitset result = new bitset(left.size());

			for (int i = 0; i < result.words.Length; i++)
				result.words[i] = left.words[i] & right.words[i];

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Returns a bitset with the bitwise OR of the two bitsets
		/// (C++ operator|). Both bitsets must have the same size
		/// </summary>
		/********************************************************************/
		public static bitset operator |(bitset left, bitset right)
		{
			Check_Same_Size(left, right);

			bitset result = new bitset(left.size());

			for (int i = 0; i < result.words.Length; i++)
				result.words[i] = left.words[i] | right.words[i];

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Returns a bitset with the bitwise XOR of the two bitsets
		/// (C++ operator^). Both bitsets must have the same size
		/// </summary>
		/********************************************************************/
		public static bitset operator ^(bitset left, bitset right)
		{
			Check_Same_Size(left, right);

			bitset result = new bitset(left.size());

			for (int i = 0; i < result.words.Length; i++)
				result.words[i] = left.words[i] ^ right.words[i];

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Returns a bitset with every bit of the given bitset flipped
		/// (C++ operator~)
		/// </summary>
		/********************************************************************/
		public static bitset operator ~(bitset value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return new bitset(value).flip();
		}



		/********************************************************************/
		/// <summary>
		/// Returns a bitset with the bits shifted towards higher positions
		/// (C++ operator‹‹(size_t)). Vacated low positions are filled with
		/// zeros and bits shifted past the end are discarded
		/// </summary>
		/********************************************************************/
		public static bitset operator <<(bitset value, int count)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));

			bitset result = new bitset(value.size());

			for (int i = value.nBits - 1; i >= count; i--)
			{
				if (value.Get_Bit(i - count))
					result.Set_Bit(i);
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Returns a bitset with the bits shifted towards lower positions
		/// (C++ operator››(size_t)). Vacated high positions are filled with
		/// zeros and bits shifted past position zero are discarded
		/// </summary>
		/********************************************************************/
		public static bitset operator >>(bitset value, int count)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));

			bitset result = new bitset(value.size());

			for (int i = 0; (i + count) < value.nBits; i++)
			{
				if (value.Get_Bit(i + count))
					result.Set_Bit(i);
			}

			return result;
		}
		#endregion

		#region Comparison operators
		/********************************************************************/
		/// <summary>
		/// Checks if the two bitsets have the same bits (C++ operator==)
		/// </summary>
		/********************************************************************/
		public static bool operator ==(bitset left, bitset right)
		{
			if (ReferenceEquals(left, right))
				return true;

			if ((left is null) || (right is null))
				return false;

			return left.Equals(right);
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the two bitsets differ in any bit (C++ operator!=)
		/// </summary>
		/********************************************************************/
		public static bool operator !=(bitset left, bitset right)
		{
			return !(left == right);
		}
		#endregion

		#region Conversions
		/********************************************************************/
		/// <summary>
		/// Returns a string representation of the bitset, with the most
		/// significant bit first (C++ to_string(zero, one))
		/// </summary>
		/********************************************************************/
		public string to_string(char zero = '0', char one = '1')
		{
			StringBuilder sb = new StringBuilder(nBits);

			for (int i = nBits - 1; i >= 0; i--)
				sb.Append(Get_Bit(i) ? one : zero);

			return sb.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Returns the value of the bitset as an unsigned long (C++
		/// to_ulong()). Throws overflow_error if the value has any bit set
		/// that does not fit in the result type
		/// </summary>
		/********************************************************************/
		public c_ulong to_ulong()
		{
			return Fits_In_First_Word() ? First_Word() : throw new overflow_error("bitset.to_ulong: value too large to fit in an unsigned long");
		}



		/********************************************************************/
		/// <summary>
		/// Returns the value of the bitset as an unsigned long long (C++
		/// to_ullong()). Throws overflow_error if the value has any bit set
		/// that does not fit in the result type
		/// </summary>
		/********************************************************************/
		public c_ulong_long to_ullong()
		{
			return Fits_In_First_Word() ? First_Word() : throw new overflow_error("bitset.to_ullong: value too large to fit in an unsigned long long");
		}
		#endregion

		#region IEquatable implementation
		/********************************************************************/
		/// <summary>
		/// Checks if this bitset has the same size and bits as the other
		/// bitset
		/// </summary>
		/********************************************************************/
		public bool Equals(bitset other)
		{
			if (other is null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			if (nBits != other.nBits)
				return false;

			for (int i = 0; i < words.Length; i++)
			{
				if (words[i] != other.words[i])
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the given object is a bitset that is equal to this one
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return Equals(obj as bitset);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a hash code based on the size and bits of the bitset
		/// </summary>
		/********************************************************************/
		public override int GetHashCode()
		{
			HashCode hash = new HashCode();

			hash.Add(nBits);

			for (int i = 0; i < words.Length; i++)
				hash.Add(words[i]);

			return hash.ToHashCode();
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Returns a string representation of the bitset, with the most
		/// significant bit first
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return to_string();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Allocates the word buffer needed to hold the given number of bits
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong[] Make_Words(int nBits)
		{
			int count = (nBits + Bits_Per_Word - 1) / Bits_Per_Word;

			return count > 0 ? new ulong[count] : Array.Empty<ulong>();
		}



		/********************************************************************/
		/// <summary>
		/// Returns the value of the bit at the given position
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool Get_Bit(int pos)
		{
			return (words[pos / Bits_Per_Word] & (1UL << (pos % Bits_Per_Word))) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// Sets the bit at the given position to one
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Set_Bit(int pos)
		{
			words[pos / Bits_Per_Word] |= 1UL << (pos % Bits_Per_Word);
		}



		/********************************************************************/
		/// <summary>
		/// Sets the bit at the given position to zero
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Clear_Bit(int pos)
		{
			words[pos / Bits_Per_Word] &= ~(1UL << (pos % Bits_Per_Word));
		}



		/********************************************************************/
		/// <summary>
		/// Clears the unused high bits of the last word, so that they do not
		/// affect count(), any() and the conversions
		/// </summary>
		/********************************************************************/
		private void Trim()
		{
			int used = nBits % Bits_Per_Word;

			if ((used != 0) && (words.Length > 0))
				words[^1] &= (1UL << used) - 1;
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the value of the bitset fits in a single 64 bit word,
		/// which is the case only when no bit at position 64 or higher is set
		/// </summary>
		/********************************************************************/
		private bool Fits_In_First_Word()
		{
			for (int i = 1; i < words.Length; i++)
			{
				if (words[i] != 0)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the low order 64 bits of the bitset value
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ulong First_Word()
		{
			return words.Length > 0 ? words[0] : 0;
		}



		/********************************************************************/
		/// <summary>
		/// Throws if either bitset is null or the two have different sizes
		/// </summary>
		/********************************************************************/
		private static void Check_Same_Size(bitset left, bitset right)
		{
			if (left == null)
				throw new ArgumentNullException(nameof(left));

			if (right == null)
				throw new ArgumentNullException(nameof(right));

			if (left.nBits != right.nBits)
				throw new ArgumentException("bitset: the two bitsets have different sizes");
		}
		#endregion
	}
}
