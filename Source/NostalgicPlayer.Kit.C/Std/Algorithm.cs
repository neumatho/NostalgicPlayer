/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C.Std.Iterators;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.C.Std
{
	/// <summary>
	/// C# port of the non-member algorithms that C++ places in the
	/// ‹algorithm› header (see the C++ standard, [algorithms]).
	///
	/// The range based algorithms operate on a half open range of elements
	/// [first, last), expressed as a pair of iterators, just like their C++
	/// counterparts. The iterators are any type implementing
	/// <see cref="Iterators.IIterator{TSelf, T}"/> (for example a
	/// CPointer‹T›, a <see cref="Iterators.forward_iterator{T}"/> or a
	/// <see cref="Iterators.reverse_iterator{T}"/>), and both must refer into
	/// the same buffer
	/// </summary>
	public static class Algorithm
	{
		/********************************************************************/
		/// <summary>
		/// Returns an iterator to the first element in the range
		/// [first, last) that equals the given value, or last if no such
		/// element is found
		/// (C++ find(InputIt first, InputIt last, const T＆ value)).
		///
		/// The returned iterator has the same type as the ones passed in.
		/// Elements are compared with EqualityComparer‹T›.Default (which
		/// uses IEquatable‹T› or Object.Equals), the C# equivalent of the
		/// C++ operator== that find uses
		/// </summary>
		/********************************************************************/
		public static TIt find<TIt, T>(TIt first, TIt last, T value) where TIt : IIterator<TIt, T>
		{
			for (; !first.Equals(last); first = first.Next())
			{
				if (EqualityComparer<T>.Default.Equals(first.Value, value))
					return first;
			}

			return last;
		}



		/********************************************************************/
		/// <summary>
		/// Returns an iterator to the first element in the range
		/// [first, last) for which the given predicate returns true, or last
		/// if no such element is found
		/// (C++ find_if(InputIt first, InputIt last, UnaryPred p)).
		///
		/// The returned iterator has the same type as the ones passed in.
		/// Note that the predicate must have an explicitly typed parameter
		/// (for example (int x) => ...) so the element type T can be
		/// inferred
		/// </summary>
		/********************************************************************/
		public static TIt find_if<TIt, T>(TIt first, TIt last, Func<T, bool> pred) where TIt : IIterator<TIt, T>
		{
			for (; !first.Equals(last); first = first.Next())
			{
				if (pred(first.Value))
					return first;
			}

			return last;
		}



		/********************************************************************/
		/// <summary>
		/// Assigns the given value to every element in the range
		/// [first, last)
		/// (C++ fill(ForwardIt first, ForwardIt last, const T＆ value)).
		///
		/// Each element is assigned an independent copy of the value: if T
		/// implements IDeepCloneable‹T›, its MakeDeepClone() is used, so
		/// that mutable reference type elements do not become shared. This
		/// matches the copy assignment that C++ fill performs on each
		/// element
		/// </summary>
		/********************************************************************/
		public static void fill<TIt, T>(TIt first, TIt last, T value) where TIt : IIterator<TIt, T>
		{
			for (; !first.Equals(last); first = first.Next())
				first.Value = Clone_Value(value);
		}



		/********************************************************************/
		/// <summary>
		/// Copies the elements in the range (first, last) to another range
		/// beginning at d_first, and returns the destination iterator one
		/// past the last element copied
		/// (C++ copy(InputIt first, InputIt last, OutputIt d_first)).
		///
		/// The source and the destination iterators are independent types,
		/// both implementing <see cref="Iterators.IIterator{TSelf, T}"/>
		/// (for example a CPointer‹T›, a
		/// <see cref="Iterators.forward_iterator{T}"/> or a
		/// <see cref="Iterators.reverse_iterator{T}"/>). The two ranges must
		/// not overlap. Each element is an independent copy (see
		/// <see cref="Clone_Value"/>), matching the copy assignment that C++
		/// copy performs on each element.
		///
		/// Note that C# cannot infer the element type T from the iterator
		/// constraints alone, so the type arguments must be given explicitly
		/// (for example copy‹CPointer‹char›, forward_iterator‹char›, char›)
		/// </summary>
		/********************************************************************/
		public static TDstIt copy<TSrcIt, TDstIt, T>(TSrcIt first, TSrcIt last, TDstIt d_first) where TSrcIt : IIterator<TSrcIt, T> where TDstIt : IIterator<TDstIt, T>
		{
			for (; !first.Equals(last); first = first.Next(), d_first = d_first.Next())
				d_first.Value = Clone_Value(first.Value);

			return d_first;
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the range (first1, last1) and the range beginning
		/// at first2 (which must be at least as long) compare element-wise
		/// equal, and false otherwise
		/// (C++ equal(InputIt1 first1, InputIt1 last1, InputIt2 first2)).
		///
		/// The two ranges may use different iterator types, both implementing
		/// <see cref="Iterators.IIterator{TSelf, T}"/>. Elements are compared
		/// with EqualityComparer‹T›.Default (the C# equivalent of the C++
		/// operator== that equal uses). As with <see cref="copy"/>, C# cannot
		/// infer the element type T from the iterator constraints alone, so
		/// the type arguments must be given explicitly
		/// </summary>
		/********************************************************************/
		public static bool equal<TIt1, TIt2, T>(TIt1 first1, TIt1 last1, TIt2 first2) where TIt1 : IIterator<TIt1, T> where TIt2 : IIterator<TIt2, T>
		{
			for (; !first1.Equals(last1); first1 = first1.Next(), first2 = first2.Next())
			{
				if (!EqualityComparer<T>.Default.Equals(first1.Value, first2.Value))
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the range (first1, last1) and the range
		/// [first2, last2) have the same length and compare element-wise
		/// equal, and false otherwise
		/// (C++ equal(InputIt1 first1, InputIt1 last1, InputIt2 first2,
		/// InputIt2 last2)).
		///
		/// The two ranges may use different iterator types, both implementing
		/// <see cref="Iterators.IIterator{TSelf, T}"/>. Elements are compared
		/// with EqualityComparer‹T›.Default (the C# equivalent of the C++
		/// operator== that equal uses). As with <see cref="copy"/>, C# cannot
		/// infer the element type T from the iterator constraints alone, so
		/// the type arguments must be given explicitly
		/// </summary>
		/********************************************************************/
		public static bool equal<TIt1, TIt2, T>(TIt1 first1, TIt1 last1, TIt2 first2, TIt2 last2) where TIt1 : IIterator<TIt1, T> where TIt2 : IIterator<TIt2, T>
		{
			for (; !first1.Equals(last1) && !first2.Equals(last2); first1 = first1.Next(), first2 = first2.Next())
			{
				if (!EqualityComparer<T>.Default.Equals(first1.Value, first2.Value))
					return false;
			}

			return first1.Equals(last1) && first2.Equals(last2);
		}

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
		private static T Clone_Value<T>(T value)
		{
			return value is IDeepCloneable<T> cloneable ? cloneable.MakeDeepClone() : value;
		}
		#endregion
	}
}
