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
		{
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
		};

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
