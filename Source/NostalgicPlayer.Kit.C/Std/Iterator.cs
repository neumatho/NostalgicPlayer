/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C.Std.Iterators;

namespace Polycode.NostalgicPlayer.Kit.C.Std
{
	/// <summary>
	/// C# port of the iterator operations that C++ places in the ‹iterator›
	/// header (see the C++ standard, [iterator.operations]).
	///
	/// The iterators used throughout the ports are random access iterators,
	/// so the operations are computed directly from the iterator offsets
	/// </summary>
	public static class Iterator
	{
		/********************************************************************/
		/// <summary>
		/// Returns the number of elements between first and last. If first
		/// is not reachable from last, the result is negative. Both
		/// iterators must refer into the same buffer
		/// (C++ distance(InputIt first, InputIt last))
		/// </summary>
		/********************************************************************/
		public static ptrdiff_t distance<TIt>(TIt first, TIt last) where TIt : IRandom_Access_Iterator<TIt>
		{
			return last.DistanceFrom(first);
		}



		/********************************************************************/
		/// <summary>
		/// Returns an iterator to the first element of the given array. Like
		/// C++ begin(T (＆array)[N]), which yields a plain pointer, this
		/// returns a CPointer‹T› (which is itself a random access iterator)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> begin<T>(T[] array)
		{
			return new CPointer<T>(array, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Returns an iterator to just after the last element of the given
		/// array. Like C++ end(T (＆array)[N]), which yields a plain
		/// pointer, this returns a CPointer‹T› (which is itself a random
		/// access iterator)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<T> end<T>(T[] array)
		{
			return new CPointer<T>(array, array.Length);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of elements in the given array
		/// (C++ size(const T (＆array)[N]), which returns N)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static size_t size<T>(T[] array)
		{
			return (size_t)array.Length;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the number of elements in the given container
		/// (C++ size(const C＆ c), which returns c.size()). C# cannot
		/// express the single C++ template that works on any container with
		/// a size(), so this overload is provided for the array container
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static size_t size<T>(array<T> array)
		{
			return array.size();
		}
	}
}
