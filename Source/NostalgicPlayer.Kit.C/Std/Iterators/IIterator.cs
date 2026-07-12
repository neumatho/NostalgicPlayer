/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.C.Std.Iterators
{
	/// <summary>
	/// Common interface implemented by the iterator types in this namespace,
	/// covering the operations needed to traverse and compare a range.
	///
	/// This has no direct equivalent in C++, where iterators conform to the
	/// iterator named requirements and are used via templates (duck typing).
	/// In C# it is needed so that the algorithms in the Std namespace can be
	/// written once, against a generic type constraint, instead of once per
	/// iterator type.
	///
	/// TSelf is the implementing type itself (a self-referencing generic), so
	/// that operations return the concrete iterator type and can be used
	/// through a generic constraint without boxing
	/// </summary>
	public interface IIterator<TSelf> : IEquatable<TSelf> where TSelf : IIterator<TSelf>
	{
		/// <summary>
		/// Returns a copy of the iterator advanced one element towards the
		/// end of the range (C++ ++it)
		/// </summary>
		TSelf Next();
	}



	/// <summary>
	/// An <see cref="IIterator{TSelf}"/> that also gives access to the element
	/// it refers to. This is the interface the element based algorithms (like
	/// <see cref="Algorithm.find_if{TIt, T}"/>) operate on
	/// </summary>
	public interface IIterator<TSelf, T> : IIterator<TSelf> where TSelf : IIterator<TSelf, T>
	{
		/// <summary>
		/// The element the iterator currently refers to (C++ *it)
		/// </summary>
		ref T Value { get; }
	}
}
