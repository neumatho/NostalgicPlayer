/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// Simple math operations
	/// </summary>
	public static class MathOps
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Sign_Extend(c_int val, c_uint bits)
		{
			c_uint shift = (8 * sizeof(c_int)) - bits;
			c_uint v = (c_uint)val << (c_int)shift;

			return (c_int)v >> (c_int)shift;
		}
	}
}
