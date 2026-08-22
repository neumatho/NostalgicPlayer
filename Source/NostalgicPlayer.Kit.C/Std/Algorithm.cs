/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C.Std.Iterators;

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
		/// implements IDeepCloneable‹T›, the copy is a clone, and if it
		/// implements ICopyTo‹T›, the value is copied into the instance the
		/// element already holds, so that mutable reference type elements do
		/// not become shared. This matches the copy assignment that C++ fill
		/// performs on each element
		/// </summary>
		/********************************************************************/
		public static void fill<TIt, T>(TIt first, TIt last, T value) where TIt : IIterator<TIt, T>
		{
			for (; !first.Equals(last); first = first.Next())
				first.Value = Copy_Value(first.Value, value);
		}



		/********************************************************************/
		/// <summary>
		/// Replaces every element in the range [first, last) that equals
		/// old_value with new_value
		/// (C++ replace(ForwardIt first, ForwardIt last, const T＆ old_value,
		/// const T＆ new_value)).
		///
		/// Elements are compared with EqualityComparer‹T›.Default (which
		/// uses IEquatable‹T› or Object.Equals), the C# equivalent of the
		/// C++ operator== that replace uses. Each replaced element is
		/// assigned an independent copy of the new value: if T implements
		/// IDeepCloneable‹T›, the copy is a clone, and if it implements
		/// ICopyTo‹T›, the new value is copied into the instance the element
		/// already holds, so that mutable reference type elements do not
		/// become shared. This matches the copy assignment that C++ replace
		/// performs on each replaced element
		/// </summary>
		/********************************************************************/
		public static void replace<TIt, T>(TIt first, TIt last, T old_value, T new_value) where TIt : IIterator<TIt, T>
		{
			for (; !first.Equals(last); first = first.Next())
			{
				if (EqualityComparer<T>.Default.Equals(first.Value, old_value))
					first.Value = Copy_Value(first.Value, new_value);
			}
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
		/// <see cref="Copy_Value"/>), matching the copy assignment that C++
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
				d_first.Value = Copy_Value(d_first.Value, first.Value);

			return d_first;
		}



		/********************************************************************/
		/// <summary>
		/// Applies the given operation to every element in the range
		/// (first1, last1), stores each result in the range beginning at
		/// d_first, and returns the destination iterator one past the last
		/// element written
		/// (C++ transform(InputIt first1, InputIt last1, OutputIt d_first,
		/// UnaryOp unary_op)).
		///
		/// The source and the destination are independent iterator types
		/// with independent element types, so the operation may change the
		/// element type. The two ranges may only overlap if they begin at
		/// the same element. Each result is stored as an independent copy
		/// (see <see cref="Copy_Value"/>), matching the copy assignment
		/// that C++ transform performs on each element.
		///
		/// Note that the operation must have an explicitly typed parameter
		/// (for example (int x) => ...) so the source element type can be
		/// inferred
		/// </summary>
		/********************************************************************/
		public static TDstIt transform<TSrcIt, TDstIt, TSrc, TDst>(TSrcIt first1, TSrcIt last1, TDstIt d_first, Func<TSrc, TDst> unary_op) where TSrcIt : IIterator<TSrcIt, TSrc> where TDstIt : IIterator<TDstIt, TDst>
		{
			for (; !first1.Equals(last1); first1 = first1.Next(), d_first = d_first.Next())
				d_first.Value = Copy_Value(d_first.Value, unary_op(first1.Value));

			return d_first;
		}



		/********************************************************************/
		/// <summary>
		/// Applies the given operation to every pair of elements taken from
		/// the range (first1, last1) and the range beginning at first2
		/// (which must be at least as long), stores each result in the range
		/// beginning at d_first, and returns the destination iterator one
		/// past the last element written
		/// (C++ transform(InputIt1 first1, InputIt1 last1, InputIt2 first2,
		/// OutputIt d_first, BinaryOp binary_op)).
		///
		/// The three ranges use independent iterator types with independent
		/// element types. Each result is stored as an independent copy (see
		/// <see cref="Copy_Value"/>), matching the copy assignment that C++
		/// transform performs on each element.
		///
		/// Note that the operation must have explicitly typed parameters
		/// (for example (int x, int y) => ...) so the source element types
		/// can be inferred
		/// </summary>
		/********************************************************************/
		public static TDstIt transform<TSrcIt1, TSrcIt2, TDstIt, TSrc1, TSrc2, TDst>(TSrcIt1 first1, TSrcIt1 last1, TSrcIt2 first2, TDstIt d_first, Func<TSrc1, TSrc2, TDst> binary_op) where TSrcIt1 : IIterator<TSrcIt1, TSrc1> where TSrcIt2 : IIterator<TSrcIt2, TSrc2> where TDstIt : IIterator<TDstIt, TDst>
		{
			for (; !first1.Equals(last1); first1 = first1.Next(), first2 = first2.Next(), d_first = d_first.Next())
				d_first.Value = Copy_Value(d_first.Value, binary_op(first1.Value, first2.Value));

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



		/********************************************************************/
		/// <summary>
		/// Rearranges the elements in the range [first, last) so that
		/// [first, middle) holds the smallest elements of the whole range
		/// in ascending order, while the order of the elements left in
		/// [middle, last) is unspecified
		/// (C++ partial_sort(RandomIt first, RandomIt middle,
		/// RandomIt last)).
		///
		/// Elements are ordered with Comparer‹T›.Default (which uses
		/// IComparable‹T›), the C# equivalent of the C++ operator‹ that
		/// partial_sort uses.
		///
		/// C# cannot infer the element type T from the iterator constraints
		/// alone, so the type arguments must be given explicitly (for
		/// example partial_sort‹forward_iterator‹int›, int›)
		/// </summary>
		/********************************************************************/
		public static void partial_sort<TIt, T>(TIt first, TIt middle, TIt last) where TIt : IRandom_Access_Iterator<TIt, T>
		{
			partial_sort(first, middle, last, (T a, T b) => Comparer<T>.Default.Compare(a, b) < 0);
		}



		/********************************************************************/
		/// <summary>
		/// Rearranges the elements in the range [first, last) so that
		/// [first, middle) holds the smallest elements of the whole range
		/// in ascending order, while the order of the elements left in
		/// [middle, last) is unspecified
		/// (C++ partial_sort(RandomIt first, RandomIt middle,
		/// RandomIt last, Compare comp)).
		///
		/// The comparer must return true if its first argument is ordered
		/// before its second. Elements that are ordered neither before nor
		/// after each other come out in an unspecified order, as
		/// partial_sort is not stable.
		///
		/// The elements are picked and sorted through a heap, just like the
		/// C++ one does, so the number of comparisons is at most N*log(M),
		/// where N is the length of [first, last) and M the length of
		/// [first, middle).
		///
		/// Note that the comparer must have explicitly typed parameters (for
		/// example (int a, int b) => ...) so the element type T can be
		/// inferred
		/// </summary>
		/********************************************************************/
		public static void partial_sort<TIt, T>(TIt first, TIt middle, TIt last, Func<T, T, bool> comp) where TIt : IRandom_Access_Iterator<TIt, T>
		{
			ptrdiff_t sorted = middle.DistanceFrom(first);
			if (sorted <= 0)
				return;

			ptrdiff_t count = last.DistanceFrom(first);

			// Make a max heap out of the elements to be sorted
			for (ptrdiff_t i = sorted / 2; i > 0; i--)
				Sift_Down(first, i - 1, sorted, comp);

			// Let every one of the remaining elements that is smaller than
			// the largest element of the heap take its place, so that the
			// heap ends up holding the smallest elements of the whole range
			for (ptrdiff_t i = sorted; i < count; i++)
			{
				if (comp(first[i], first[0]))
				{
					Swap<TIt, T>(first, 0, i);
					Sift_Down(first, 0, sorted, comp);
				}
			}

			// Sort the heap by moving its largest element to the end of it,
			// over and over
			for (ptrdiff_t i = sorted - 1; i > 0; i--)
			{
				Swap<TIt, T>(first, 0, i);
				Sift_Down(first, 0, i, comp);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if an element equal to the given value is found in
		/// the range [first, last), and false otherwise
		/// (C++ binary_search(ForwardIt first, ForwardIt last,
		/// const T＆ value)).
		///
		/// The range must already be partitioned with respect to the value,
		/// which a range sorted in ascending order is. Elements are ordered
		/// with Comparer‹T›.Default (which uses IComparable‹T›), the C#
		/// equivalent of the C++ operator‹ that binary_search uses. Two
		/// elements count as equal when neither is ordered before the other
		/// </summary>
		/********************************************************************/
		public static bool binary_search<TIt, T>(TIt first, TIt last, T value) where TIt : IIterator<TIt, T>
		{
			return binary_search(first, last, value, (T a, T b) => Comparer<T>.Default.Compare(a, b) < 0);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if an element equal to the given value is found in
		/// the range [first, last), and false otherwise
		/// (C++ binary_search(ForwardIt first, ForwardIt last,
		/// const T＆ value, Compare comp)).
		///
		/// The comparer must return true if its first argument is ordered
		/// before its second, and the range must already be partitioned with
		/// respect to the value using that same ordering. Two elements count
		/// as equal when neither is ordered before the other.
		///
		/// Note that the comparer must have explicitly typed parameters (for
		/// example (int a, int b) => ...) so the element type T can be
		/// inferred
		/// </summary>
		/********************************************************************/
		public static bool binary_search<TIt, T>(TIt first, TIt last, T value, Func<T, T, bool> comp) where TIt : IIterator<TIt, T>
		{
			first = Lower_Bound(first, last, value, comp);

			return !first.Equals(last) && !comp(value, first.Value);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Returns an iterator to the first element in the range
		/// [first, last) which is not ordered before the given value, or
		/// last if no such element is found (C++ lower_bound).
		///
		/// The number of comparisons is logarithmic, as in C++. The
		/// iterators are only stepped forward one element at a time, since
		/// that is all <see cref="Iterators.IIterator{TSelf}"/> offers,
		/// which matches what C++ does for a forward iterator
		/// </summary>
		/********************************************************************/
		private static TIt Lower_Bound<TIt, T>(TIt first, TIt last, T value, Func<T, T, bool> comp) where TIt : IIterator<TIt, T>
		{
			ptrdiff_t count = 0;

			for (TIt it = first; !it.Equals(last); it = it.Next())
				count++;

			while (count > 0)
			{
				ptrdiff_t step = count / 2;
				TIt middle = first;

				for (ptrdiff_t i = 0; i < step; i++)
					middle = middle.Next();

				if (comp(middle.Value, value))
				{
					first = middle.Next();
					count -= step + 1;
				}
				else
					count = step;
			}

			return first;
		}



		/********************************************************************/
		/// <summary>
		/// Moves the element at the given index down into the max heap that
		/// begins at first and holds count elements, until the element has
		/// found the place where the heap condition holds again (C++ does
		/// the same in its pop_heap and make_heap)
		/// </summary>
		/********************************************************************/
		private static void Sift_Down<TIt, T>(TIt first, ptrdiff_t index, ptrdiff_t count, Func<T, T, bool> comp) where TIt : IRandom_Access_Iterator<TIt, T>
		{
			for (;;)
			{
				ptrdiff_t child = (index * 2) + 1;
				if (child >= count)
					break;

				// Take the larger of the two children
				if (((child + 1) < count) && comp(first[child], first[child + 1]))
					child++;

				if (!comp(first[index], first[child]))
					break;

				Swap<TIt, T>(first, index, child);
				index = child;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Exchanges the two elements lying at the given distances from
		/// first. The elements are exchanged just as they are, which is
		/// what the C++ swap() used by the sorting algorithms does
		/// </summary>
		/********************************************************************/
		private static void Swap<TIt, T>(TIt first, ptrdiff_t index1, ptrdiff_t index2) where TIt : IRandom_Access_Iterator<TIt, T>
		{
			T temp = first[index1];

			first[index1] = first[index2];
			first[index2] = temp;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the value to store in the given element. If the value
		/// implements IDeepCloneable‹T›, its MakeDeepClone() is used to
		/// obtain a new instance, and if it implements ICopyTo‹T›, it is
		/// copied into the instance the element already holds, just as C++
		/// copy assignment of an element does. Otherwise the value is
		/// returned unchanged, which is the correct behavior for value types
		/// and immutable reference types (see <see cref="Copy_Insert{T}"/>)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static T Copy_Value<T>(T destination, T value)
		{
			return Copy_Insert<T>.Assign(destination, value);
		}
		#endregion
	}
}
