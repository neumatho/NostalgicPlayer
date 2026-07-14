/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.C.Std.Exceptions
{
	/// <summary>
	/// C# port of the C++ standard library std::overflow_error
	/// (see the C++ standard, [overflow.error]).
	///
	/// The type of the exception thrown to report an arithmetic overflow,
	/// e.g. when bitset.to_ulong() is called on a value that does not fit in
	/// the result type
	/// </summary>
#pragma warning disable CS8981
	public class overflow_error : Exception
	{
#pragma warning restore CS8981
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public overflow_error() : base("overflow error")
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public overflow_error(string message) : base(message)
		{
		}
	}
}
