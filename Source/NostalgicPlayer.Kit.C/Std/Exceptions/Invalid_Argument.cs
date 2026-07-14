/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.C.Std.Exceptions
{
	/// <summary>
	/// C# port of the C++ standard library std::invalid_argument
	/// (see the C++ standard, [invalid.argument]).
	///
	/// The type of the exception thrown to report an argument whose value is
	/// not accepted, e.g. when a bitset is constructed from a string that
	/// contains a character other than the zero and one characters
	/// </summary>
#pragma warning disable CS8981
	public class invalid_argument : Exception
	{
#pragma warning restore CS8981
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public invalid_argument() : base("invalid argument")
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public invalid_argument(string message) : base(message)
		{
		}
	}
}
