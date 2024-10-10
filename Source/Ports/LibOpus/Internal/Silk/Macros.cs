/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Macros
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_SMULWB(opus_int32 a32, opus_int32 b32)
		{
			return (opus_int32)(((a32 * (opus_int64)((opus_int16)b32))) >> 16);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_SMLAWB(opus_int32 a32, opus_int32 b32, opus_int32 c32)
		{
			return (opus_int32)(a32 + ((b32 * (opus_int64)((opus_int16)c32)) >> 16));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_SMULBB(opus_int32 a32, opus_int32 b32)
		{
			return (opus_int32)((opus_int16)a32) * (opus_int32)((opus_int16)b32);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_SMLABB(opus_int32 a32, opus_int32 b32, opus_int32 c32)
		{
			return a32 + ((opus_int32)((opus_int16)b32)) * (opus_int32)((opus_int16)c32);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_SMULWW(opus_int32 a32, opus_int32 b32)
		{
			return (opus_int32)(((opus_int64)a32 * b32) >> 16);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_SMLAWW(opus_int32 a32, opus_int32 b32, opus_int32 c32)
		{
			return (opus_int32)(a32 + (((opus_int64)b32 * c32) >> 16));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_ADD_SAT32(opus_uint32 a, opus_uint32 b)
		{
			return ((a + b) & 0x80000000) == 0 ?
				(a & (b ^ 0x80000000) & 0x80000000) != 0 ? opus_int32.MinValue : (opus_int32)(a + b) :
				((a | b) & 0x80000000) == 0 ? opus_int32.MaxValue : (opus_int32)(a + b);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_SUB_SAT32(opus_uint32 a, opus_uint32 b)
		{
			return ((a - b) & 0x80000000) == 0 ?
				(a & (b ^ 0x80000000) & 0x80000000) != 0 ? opus_int32.MinValue : (opus_int32)(a - b) :
				((a ^ 0x80000000) & b & 0x80000000) != 0 ? opus_int32.MaxValue : (opus_int32)(a - b);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_CLZ32(opus_int32 in32)
		{
			return in32 != 0 ? 32 - EntCode.Ec_Ilog((opus_uint32)in32) : 32;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Matrix_Ptr<T>(Pointer<T> Matrix_base_adr, opus_int row, opus_int column, opus_int N)
		{
			return Matrix_base_adr[row * N + column];
		}
	}
}
