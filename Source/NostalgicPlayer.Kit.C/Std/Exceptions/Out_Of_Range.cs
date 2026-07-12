/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.C.Std.Exceptions
{
	/// <summary>
	/// C# port of the C++ standard library std::out_of_range
	/// (see the C++ standard, [out.of.range]).
	///
	/// The type of the exception thrown when an argument value is not in the
	/// expected range, e.g. when vector‹T›.at() is called with an index that is
	/// outside the valid range of the container
	/// </summary>
#pragma warning disable CS8981
	public class out_of_range : Exception
	{
#pragma warning restore CS8981
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public out_of_range() : base("out of range")
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public out_of_range(string message) : base(message)
		{
		}
	}
}
