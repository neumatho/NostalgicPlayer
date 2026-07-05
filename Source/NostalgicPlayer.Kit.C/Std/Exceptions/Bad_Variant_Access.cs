/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.C.Std.Exceptions
{
	/// <summary>
	/// C# port of the C++ standard library std::bad_variant_access
	/// (see the C++ standard, [variant.bad.access]).
	///
	/// The type of the exception thrown when a
	/// <see cref="variant{T0,T1}.get{T}"/> is called with a type or index that
	/// does not match the alternative currently held by the variant
	/// </summary>
#pragma warning disable CS8981
	public class bad_variant_access : Exception
	{
#pragma warning restore CS8981
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public bad_variant_access() : base("bad variant access")
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public bad_variant_access(string message) : base(message)
		{
		}
	}
}
