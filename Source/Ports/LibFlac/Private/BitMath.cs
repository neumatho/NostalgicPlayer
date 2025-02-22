/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibFlac.Private
{
	/// <summary>
	/// 
	/// </summary>
	internal static class BitMath
	{
		private static readonly uint8_t[] byte_To_Unary_Table =
		[
			8, 7, 6, 6, 5, 5, 5, 5, 4, 4, 4, 4, 4, 4, 4, 4,
			3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
			2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
			2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		];

		private static readonly uint8_t[] debruijn_Idx64 =
		[
			 0,  1,  2,  7,  3, 13,  8, 19,  4, 25, 14, 28,  9, 34, 20, 40,
			 5, 17, 26, 38, 15, 46, 29, 48, 10, 31, 35, 54, 21, 50, 41, 57,
			63,  6, 12, 18, 24, 27, 33, 39, 16, 37, 45, 47, 30, 53, 49, 56,
			62, 11, 23, 32, 36, 44, 52, 55, 61, 22, 43, 51, 60, 42, 59, 58
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Flac__uint32 Flac__Clz_UInt32(Flac__uint32 v)
		{
			// Never used with input 0
			Debug.Assert(v > 0);

			return Flac__Clz_Soft_UInt32(v);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Flac__uint32 Flac__Clz_UInt64(Flac__uint64 v)
		{
			// Never used with input 0
			Debug.Assert(v > 0);

			return Flac__Clz_Soft_UInt64(v);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Flac__uint32 Flac__Clz2_UInt64(Flac__uint64 v)
		{
			if (v == 0)
				return 64;

			return Flac__Clz_UInt64(v);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Flac__uint32 Flac__BitMath_ILog2(Flac__uint32 v)
		{
			Debug.Assert(v > 0);

			return Flac__Clz_UInt32(v) ^ 31U;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint32_t Flac__BitMath_ILog2_Wide(Flac__uint64 v)
		{
			Debug.Assert(v > 0);

			// de Bruijn sequences (http://supertech.csail.mit.edu/papers/debruijn.pdf)
			// (C) Timothy B. Terriberry (tterribe@xiph.org) 2001-2009 CC0 (Public domain)
			v |= v >> 1;
			v |= v >> 2;
			v |= v >> 4;
			v |= v >> 8;
			v |= v >> 16;
			v |= v >> 32;
			v = (v >> 1) + 1;

			return debruijn_Idx64[v * 0x218A392CD3D5DBF >> 58 & 0x3f];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint32_t Flac__BitMath_SiLog2(Flac__int64 v)
		{
			if (v == 0)
				return 0;

			if (v == -1)
				return 2;

			v = (v < 0) ? (-(v + 1)) : v;

			return Flac__BitMath_ILog2_Wide((Flac__uint64)v) + 2;
		}



		/********************************************************************/
		/// <summary>
		/// The intent of this is to calculate how many extra bits
		/// multiplication by a certain number requires. So, if a signal fits
		/// in a certain number of bits (for example 16) than multiplying by
		/// a number (for example 1024) grows that storage requirement (to
		/// 26 in this example). In effect this is the log2 rounded up
		/// </summary>
		/********************************************************************/
		public static uint32_t Flac__BitMath_Extra_Mulbits_Unsigned(Flac__uint32 v)
		{
			if (v == 0)
				return 0;

			uint32_t ilog2 = Flac__BitMath_ILog2(v);
			if (((v >> (int)ilog2) << (int)ilog2) == v)
			{
				// v is power of 2
				return ilog2;
			}
			else
			{
				// v is not a power of 2, return one higher
				return ilog2 + 1;
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Flac__uint32 Flac__Clz_Soft_UInt32(Flac__uint32 word)
		{
			return (Flac__uint32)(word > 0xffffff ? byte_To_Unary_Table[word >> 24] :
				word > 0xffff ? byte_To_Unary_Table[word >> 16] + 8 :
				word > 0xff ? byte_To_Unary_Table[word >> 8] + 16 :
				byte_To_Unary_Table[word] + 24);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Flac__uint32 Flac__Clz_Soft_UInt64(Flac__uint64 word)
		{
			return ((word >> 32) != 0) ? Flac__Clz_UInt32((Flac__uint32)(word >> 32)) : Flac__Clz_UInt32((Flac__uint32)word) + 32;
		}
		#endregion
	}
}
