/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
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
		/// [first, last) for which the given predicate returns true, or last
		/// if no such element is found
		/// (C++ find_if(InputIt first, InputIt last, UnaryPred p)).
		///
		/// The returned iterator has the same type as the ones passed in.
		/// Note that the predicate must have an explicitly typed parameter
		/// (for example (int x) => ...) so the element type T can be inferred
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
	}
}
