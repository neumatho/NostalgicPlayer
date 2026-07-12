/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.C.Std.Exceptions
{
	/// <summary>
	/// C# port of the C++ standard library std::length_error
	/// (see the C++ standard, [length.error]).
	///
	/// The type of the exception thrown when an attempt is made to exceed an
	/// implementation defined length limit, e.g. when vector‹T›.reserve() is
	/// called with a value larger than vector‹T›.max_size()
	/// </summary>
#pragma warning disable CS8981
	public class length_error : Exception
	{
#pragma warning restore CS8981
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public length_error() : base("length error")
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public length_error(string message) : base(message)
		{
		}
	}
}
