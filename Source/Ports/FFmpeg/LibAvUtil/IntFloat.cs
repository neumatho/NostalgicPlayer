/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class IntFloat
	{
		/********************************************************************/
		/// <summary>
		/// Reinterpret a 32-bit integer as a float
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_float Av_Int2Float(uint32_t i)
		{
			return BitConverter.UInt32BitsToSingle(i);
		}
	}
}
