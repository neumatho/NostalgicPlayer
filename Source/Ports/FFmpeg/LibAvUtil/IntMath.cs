/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class IntMath
	{
		private static readonly uint8_t[] debruijn_ctz32 =
		[
			0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
			31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
		];

		private static readonly uint8_t[] debruijn_ctz64 =
		[
			0, 1, 2, 53, 3, 7, 54, 27, 4, 38, 41, 8, 34, 55, 48, 28,
			62, 5, 39, 46, 44, 42, 22, 9, 24, 35, 59, 56, 49, 18, 29, 11,
			63, 52, 6, 26, 37, 40, 33, 47, 61, 45, 43, 21, 23, 58, 17, 10,
			51, 25, 36, 32, 60, 20, 57, 16, 50, 31, 19, 15, 30, 14, 13, 12
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Av_Log2(c_uint v)
		{
			return FF_Log2(v);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Av_Log2_16Bit(c_uint v)
		{
			return FF_Log2_16Bit(v);
		}



		/********************************************************************/
		/// <summary>
		/// Trailing zero bit count.
		///
		/// We use the De-Bruijn method outlined in:
		/// http://supertech.csail.mit.edu/papers/debruijn.pdf
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int FF_Ctz(c_int v)
		{
			return debruijn_ctz32[(uint32_t)((v & -(uint32_t)v) * 0x077cb531U) >> 27];
		}



		/********************************************************************/
		/// <summary>
		/// Trailing zero bit count.
		///
		/// We use the De-Bruijn method outlined in:
		/// http://supertech.csail.mit.edu/papers/debruijn.pdf
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int FF_Ctzll(c_long_long v)
		{
			return debruijn_ctz64[(uint64_t)(((uint64_t)v & (uint64_t)(-v)) * 0x022fdd63cc95386dU) >> 58];
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_int FF_Log2(c_uint v)
		{
			c_int n = 0;

			if ((v & 0xffff0000) != 0)
			{
				v >>= 16;
				n += 16;
			}

			if ((v & 0xff00) != 0)
			{
				v >>= 8;
				n += 8;
			}

			n += Log2_Tab.FF_Log2_Tab[v];

			return n;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_int FF_Log2_16Bit(c_uint v)
		{
			c_int n = 0;

			if ((v & 0xff00) != 0)
			{
				v >>= 8;
				n += 8;
			}

			n += Log2_Tab.FF_Log2_Tab[v];

			return n;
		}
		#endregion
	}
}
