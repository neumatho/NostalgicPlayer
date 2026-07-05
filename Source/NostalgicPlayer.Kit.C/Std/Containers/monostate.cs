/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.C.Std.Containers
{
	/// <summary>
	/// C# port of the C++ standard library std::monostate
	/// (see the C++ standard, [variant.monostate]).
	///
	/// A unit type intended for use as a well-behaved empty alternative in a
	/// <see cref="variant{T0,T1}"/>. It is typically used as the first
	/// alternative so that the variant can be default constructed even when the
	/// other alternatives are not default constructible
	/// </summary>
#pragma warning disable CS8981
	public readonly struct monostate : IEquatable<monostate>
	{
#pragma warning restore CS8981
		/********************************************************************/
		/// <summary>
		/// All instances of monostate are equal (C++ operator==)
		/// </summary>
		/********************************************************************/
		public bool Equals(monostate other)
		{
			return true;
		}



		/********************************************************************/
		/// <summary>
		/// All instances of monostate are equal
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return obj is monostate;
		}



		/********************************************************************/
		/// <summary>
		/// All instances of monostate hash to the same value
		/// </summary>
		/********************************************************************/
		public override int GetHashCode()
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Two monostate values are always equal (C++ operator==)
		/// </summary>
		/********************************************************************/
		public static bool operator ==(monostate left, monostate right)
		{
			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Two monostate values are never unequal (C++ operator!=)
		/// </summary>
		/********************************************************************/
		public static bool operator !=(monostate left, monostate right)
		{
			return false;
		}
	}
}
