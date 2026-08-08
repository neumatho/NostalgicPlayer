/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.C.Std
{
	/// <summary>
	/// C# port of the non-member helpers that C++ places in the ‹utility›
	/// header (see the C++ standard, [utility])
	/// </summary>
	public static class Utility
	{
		/********************************************************************/
		/// <summary>
		/// Hands the data of the given value over to the caller, leaving
		/// the given value in a moved-from state
		/// (C++ move(T＆＆ t)).
		///
		/// In C++ this is only a cast to an rvalue reference: the actual
		/// transfer of the data is done by the move constructor or the
		/// move assignment operator of the type that receives the result.
		/// C# has neither rvalue references nor move constructors, so the
		/// transfer is done here instead: if the given value implements
		/// IMoveable‹T›, its MoveFrom() hands the data over to a new
		/// instance and leaves the given one empty, which is the valid
		/// but unspecified state that a moved-from object has in C++.
		///
		/// Otherwise the value is returned unchanged and the source is
		/// left untouched, exactly like a C++ move of a type that has no
		/// move constructor, where the move degrades to a copy.
		///
		/// The value is taken by value, so that it can be applied to any
		/// expression just like the C++ one. This means that only a
		/// reference type can be left in a moved-from state: a value type
		/// implementing IMoveable‹T› would be moved from through a boxed
		/// copy, leaving the caller's variable unchanged
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T move<T>(T value)
		{
			return value is IMoveable<T> moveable ? moveable.MoveFrom() : value;
		}
	}
}
