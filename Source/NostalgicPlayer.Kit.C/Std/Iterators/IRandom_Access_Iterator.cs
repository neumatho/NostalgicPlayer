/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.C.Std.Iterators
{
	/// <summary>
	/// An <see cref="IIterator{TSelf}"/> whose elements are stored
	/// contiguously, so that the distance between two iterators can be
	/// computed in constant time. This is the interface
	/// <see cref="Iterator.distance{TIt}"/> operates on.
	///
	/// It matches the C++ random access iterator category. Iterator types
	/// that are not random access (for example a future map iterator) should
	/// implement only <see cref="IIterator{TSelf}"/>
	/// </summary>
	public interface IRandom_Access_Iterator<TSelf> : IIterator<TSelf> where TSelf : IRandom_Access_Iterator<TSelf>
	{
		/// <summary>
		/// Returns the number of elements between other and this iterator,
		/// i.e. this - other. The result is negative when this comes before
		/// other. Both iterators must refer into the same buffer
		/// </summary>
		ptrdiff_t DistanceFrom(TSelf other);
	}
}
