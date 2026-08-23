/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.C.Std.Iterators
{
	/// <summary>
	/// An <see cref="IIterator{TSelf}"/> that can also be moved towards the
	/// beginning of the range, so that a range can be traversed backwards.
	/// This is the interface the algorithms that work from the end of a
	/// range (like <see cref="Algorithm.move_backward{TSrcIt, TDstIt, T}"/>)
	/// operate on.
	///
	/// It matches the C++ bidirectional iterator category. Iterator types
	/// that can only be moved towards the end of the range (for example a
	/// future forward list iterator) should implement only
	/// <see cref="IIterator{TSelf}"/>
	/// </summary>
	public interface IBidirectional_Iterator<TSelf> : IIterator<TSelf> where TSelf : IBidirectional_Iterator<TSelf>
	{
		/// <summary>
		/// Returns a copy of the iterator moved one element towards the
		/// beginning of the range (C++ --it)
		/// </summary>
		TSelf Prev();
	}



	/// <summary>
	/// An <see cref="IBidirectional_Iterator{TSelf}"/> that also gives
	/// access to the element it refers to. This is the interface the
	/// element based algorithms that traverse a range backwards (like
	/// <see cref="Algorithm.move_backward{TSrcIt, TDstIt, T}"/>) operate on
	/// </summary>
	public interface IBidirectional_Iterator<TSelf, T> : IBidirectional_Iterator<TSelf>, IIterator<TSelf, T> where TSelf : IBidirectional_Iterator<TSelf, T>
	{
	}
}
