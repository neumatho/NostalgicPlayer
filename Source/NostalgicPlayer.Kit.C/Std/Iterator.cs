/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
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
		/// is not reachable from last, the result is negative. Both iterators
		/// must refer into the same buffer
		/// (C++ distance(InputIt first, InputIt last))
		/// </summary>
		/********************************************************************/
		public static ptrdiff_t distance<TIt>(TIt first, TIt last) where TIt : IRandom_Access_Iterator<TIt>
		{
			return last.DistanceFrom(first);
		}
	}
}
