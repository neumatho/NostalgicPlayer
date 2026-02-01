/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// Internal math functions
	/// </summary>
	public static class FFMath
	{
		/********************************************************************/
		/// <summary>
		/// Compute 10^x for floating point values. Note: this function is by
		/// no means "correctly rounded", and is meant as a fast, reasonably
		/// accurate approximation.
		/// For instance, maximum relative error for the double precision
		/// variant is ~ 1e-13 for very small and very large values.
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double FF_Exp10(c_double x)
		{
			return CMath.exp2(Mathematics.M_Log2_10 * x);
		}
	}
}
