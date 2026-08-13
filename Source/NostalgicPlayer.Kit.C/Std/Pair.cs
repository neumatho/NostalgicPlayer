/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Kit.C.Std
{
	/// <summary>
	/// Holds the free (non-member) helpers that C++ places in the std
	/// namespace and that do not need to be tied to a specific pair type
	/// </summary>
#pragma warning disable CS8981
	public static class pair
	{
		/********************************************************************/
		/// <summary>
		/// Creates a pair‹T1, T2› holding the given values
		/// (C++ std::make_pair)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static pair<T1, T2> make_pair<T1, T2>(T1 x, T2 y)
		{
			return new pair<T1, T2>(x, y);
		}
	}



	/// <summary>
	/// C# port of the C++ standard library std::pair
	/// (see the C++ standard, [pairs]).
	///
	/// A structure that couples together a pair of values, which may be of
	/// different types (T1 and T2). The two values are exposed through the
	/// public <see cref="first"/> and <see cref="second"/> fields, just like
	/// the data members of a C++ std::pair.
	///
	/// Like a C++ std::pair, this is a value type (a C# struct), so assigning
	/// one pair‹T1, T2› variable to another copies the two values. That copy
	/// is shallow: if an element is itself a mutable reference type, both
	/// pairs end up referring to the same element object. Use the copy
	/// constructor pair‹T1, T2›(pair‹T1, T2›) to make an independent copy of
	/// the elements as well. Constructing a pair from two values stores the
	/// given instances directly, as it builds the pair from its elements
	/// rather than copying an existing pair.
	///
	/// Notable differences from C++:
	/// - The elements are value initialized to default(T). For a reference
	///   type this means null, where C++ would have default constructed an
	///   object.
	/// - The index based std::get‹I›(pair) is not available, as C# does not
	///   support non-type (value) template arguments. Access the elements
	///   through the <see cref="first"/> and <see cref="second"/> fields
	///   instead
	/// </summary>
	public struct pair<T1, T2> : IEquatable<pair<T1, T2>>, IComparable<pair<T1, T2>>
	{
#pragma warning restore CS8981
		/// <summary>
		/// The first stored value (C++ pair::first)
		/// </summary>
		public T1 first;

		/// <summary>
		/// The second stored value (C++ pair::second)
		/// </summary>
		public T2 second;

		/********************************************************************/
		/// <summary>
		/// Constructs a pair with both elements value initialized to their
		/// default (C++ pair()).
		///
		/// Note that, unlike C++, the elements are value initialized to
		/// default(T). For a reference type this means null, where C++ would
		/// have default constructed an object
		/// </summary>
		/********************************************************************/
		public pair()
		{
			first = default;
			second = default;
		}



		/********************************************************************/
		/// <summary>
		/// Constructs a pair holding the given values
		/// (C++ pair(const T1＆ x, const T2＆ y))
		/// </summary>
		/********************************************************************/
		public pair(T1 x, T2 y)
		{
			first = x;
			second = y;
		}



		/********************************************************************/
		/// <summary>
		/// Copy constructor. Constructs an independent pair with a copy of
		/// the contents of the given pair (C++ pair(const pair＆)). Elements
		/// that implement IDeepCloneable‹T› or ICopyTo‹T› are copied, so that
		/// the two pairs do not share element instances
		/// </summary>
		/********************************************************************/
		public pair(pair<T1, T2> other)
		{
			first = Create_Value(other.first);
			second = Create_Value(other.second);
		}



		/********************************************************************/
		/// <summary>
		/// Exchanges the contents of this pair with those of the other pair
		/// (C++ swap(pair＆ other))
		/// </summary>
		/********************************************************************/
		public void swap(ref pair<T1, T2> other)
		{
			(first, other.first) = (other.first, first);
			(second, other.second) = (other.second, second);
		}

		#region Comparison operators
		/********************************************************************/
		/// <summary>
		/// Checks if both elements of the two pairs are equal
		/// (C++ operator==)
		/// </summary>
		/********************************************************************/
		public static bool operator ==(pair<T1, T2> left, pair<T1, T2> right)
		{
			return left.Equals(right);
		}



		/********************************************************************/
		/// <summary>
		/// Checks if any element of the two pairs differs (C++ operator!=)
		/// </summary>
		/********************************************************************/
		public static bool operator !=(pair<T1, T2> left, pair<T1, T2> right)
		{
			return !left.Equals(right);
		}



		/********************************************************************/
		/// <summary>
		/// Compares the two pairs lexicographically, comparing the first
		/// elements and, if they are equal, the second elements
		/// (C++ operator‹)
		/// </summary>
		/********************************************************************/
		public static bool operator <(pair<T1, T2> left, pair<T1, T2> right)
		{
			return left.CompareTo(right) < 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the two pairs lexicographically (C++ operator‹=)
		/// </summary>
		/********************************************************************/
		public static bool operator <=(pair<T1, T2> left, pair<T1, T2> right)
		{
			return left.CompareTo(right) <= 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the two pairs lexicographically (C++ operator›)
		/// </summary>
		/********************************************************************/
		public static bool operator >(pair<T1, T2> left, pair<T1, T2> right)
		{
			return left.CompareTo(right) > 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compares the two pairs lexicographically (C++ operator›=)
		/// </summary>
		/********************************************************************/
		public static bool operator >=(pair<T1, T2> left, pair<T1, T2> right)
		{
			return left.CompareTo(right) >= 0;
		}
		#endregion

		#region IEquatable implementation
		/********************************************************************/
		/// <summary>
		/// Checks if both elements of this pair are equal to the corresponding
		/// elements of the other pair
		/// </summary>
		/********************************************************************/
		public bool Equals(pair<T1, T2> other)
		{
			return EqualityComparer<T1>.Default.Equals(first, other.first) &&
			       EqualityComparer<T2>.Default.Equals(second, other.second);
		}



		/********************************************************************/
		/// <summary>
		/// Checks if the given object is a pair that is equal to this one
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return (obj is pair<T1, T2> other) && Equals(other);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a hash code based on both elements of the pair
		/// </summary>
		/********************************************************************/
		public override int GetHashCode()
		{
			return HashCode.Combine(first, second);
		}
		#endregion

		#region IComparable implementation
		/********************************************************************/
		/// <summary>
		/// Compares this pair with the other pair lexicographically, first
		/// elements first and, if they are equal, the second elements, and
		/// returns a negative value, zero or a positive value. The elements
		/// are ordered with Comparer‹T›.Default (which uses IComparable‹T›),
		/// the C# equivalent of the element comparisons that the C++
		/// operator‹=› does
		/// </summary>
		/********************************************************************/
		public int CompareTo(pair<T1, T2> other)
		{
			int c = Comparer<T1>.Default.Compare(first, other.first);
			if (c != 0)
				return c;

			return Comparer<T2>.Default.Compare(second, other.second);
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Returns a string representation of both elements of the pair
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return $"({first}, {second})";
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Returns the value to store in a new element, which is an
		/// independent copy of the given value. If the value implements
		/// IDeepCloneable‹U›, its MakeDeepClone() is used to obtain a new
		/// instance, and if it implements ICopyTo‹U›, it is copied into a new
		/// instance of its own type. Otherwise the value is returned
		/// unchanged, which is the correct behavior for value types and
		/// immutable reference types (see <see cref="Copy_Insert{T}"/>)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static U Create_Value<U>(U value)
		{
			return Copy_Insert<U>.Create(value);
		}
		#endregion
	}
}
