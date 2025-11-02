/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Silk
{
	/// <summary>
	/// 
	/// </summary>
	internal static class SigProc_Fix
	{
		private const int Rand_Multiplier = 196314165;
		private const int Rand_Increment = 907633515;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_ROR32(opus_int32 a32, opus_int rot)
		{
			opus_uint32 x = (opus_uint32)a32;
			opus_int32 r = rot;
			opus_int32 m = -rot;

			if (rot == 0)
				return a32;
			else if (rot < 0)
				return (opus_int32)((x << m) | (x >> (32 - m)));
			else
				return (opus_int32)((x << (32 - r)) | (x >> r));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Silk_MemCpy<T>(CPointer<T> dest, CPointer<T> src, int size)
		{
			CMemory.memcpy(dest, src, (size_t)size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Silk_MemCpy_Span<T>(Span<T> dest, Span<T> src, int size)
		{
			src.Slice(0, size).CopyTo(dest);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Silk_MemSet<T>(CPointer<T> ptr, T value, int length)
		{
			CMemory.memset(ptr, value, (size_t)length);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Silk_MemMove<T>(CPointer<T> dest, CPointer<T> src, int size)
		{
			CMemory.memmove(dest, src, (size_t)size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_MUL(opus_int32 a32, opus_int32 b32)
		{
			return a32 * b32;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_MLA(opus_int32 a32, opus_int32 b32, opus_int32 c32)
		{
			return Silk_ADD32(a32, b32 * c32);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_SMULTT(opus_int32 a32, opus_int32 b32)
		{
			return (a32 >> 16) * (b32 >> 16);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int64 Silk_SMULL(opus_int32 a32, opus_int32 b32)
		{
			return (opus_int64)a32 * b32;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_ADD32_ovflw(opus_uint32 a, opus_uint32 b)
		{
			return (opus_int32)(a + b);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_SUB32_ovflw(opus_uint32 a, opus_uint32 b)
		{
			return (opus_int32)(a - b);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_MLA_ovflw(opus_uint32 a32, opus_uint32 b32, opus_uint32 c32)
		{
			return Silk_ADD32_ovflw(a32, (b32 * c32));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_SMLABB_ovflw(opus_uint32 a32, opus_uint32 b32, opus_uint32 c32)
		{
			return Silk_ADD32_ovflw(a32, ((opus_uint32)((opus_int16)b32)) * (opus_uint32)((opus_int16)c32));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_DIV32_16(opus_int32 a32, opus_int16 b16)
		{
			return a32 / b16;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_DIV32(opus_int32 a32, opus_int32 b32)
		{
			return a32 / b32;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_ADD16(opus_int a, opus_int b)
		{
			return a + b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_ADD32(opus_int32 a, opus_int32 b)
		{
			return a + b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_SUB16(opus_int a, opus_int b)
		{
			return a - b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_SUB32(opus_int32 a, opus_int32 b)
		{
			return a - b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int Silk_SAT16(opus_int a)
		{
			return a > opus_int16.MaxValue ? opus_int16.MaxValue : a < opus_int16.MinValue ? opus_int16.MinValue : a;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int16 Silk_ADD_SAT16(opus_int32 a, opus_int32 b)
		{
			return (opus_int16)Silk_SAT16(Silk_ADD32(a, b));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_LSHIFT32(opus_int32 a, int shift)
		{
			return a << shift;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_LSHIFT(int a, int shift)
		{
			return Silk_LSHIFT32(a, shift);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_RSHIFT32(opus_int32 a, int shift)
		{
			return a >> shift;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int64 Silk_RSHIFT64(opus_int64 a, int shift)
		{
			return a >> shift;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_RSHIFT(int a, int shift)
		{
			return Silk_RSHIFT32(a, shift);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_LSHIFT_SAT32(opus_int32 a, int shift)
		{
			return Silk_LSHIFT(Silk_LIMIT(a, Silk_RSHIFT32(opus_int32.MinValue, shift), Silk_RSHIFT32(opus_int32.MaxValue, shift)), shift);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_LSHIFT_ovflw(opus_uint32 a, int shift)
		{
			return (opus_int32)(a << shift);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_uint32 Silk_RSHIFT_uint(opus_uint32 a, int shift)
		{
			return a >> shift;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_ADD_LSHIFT(int a, int b, int shift)
		{
			return a + Silk_LSHIFT(b, shift);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_ADD_LSHIFT32(opus_int32 a, opus_int32 b, int shift)
		{
			return Silk_ADD32(a, Silk_LSHIFT32(b, shift));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_ADD_RSHIFT32(opus_int32 a, opus_int32 b, int shift)
		{
			return Silk_ADD32(a, Silk_RSHIFT32(b, shift));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_uint32 Silk_ADD_RSHIFT_uint(opus_uint32 a, opus_uint32 b, int shift)
		{
			return a + Silk_RSHIFT_uint(b, shift);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_SUB_LSHIFT32(opus_int32 a, opus_int32 b, int shift)
		{
			return Silk_SUB32(a, Silk_LSHIFT32(b, shift));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_RSHIFT_ROUND(opus_int32 a, int shift)
		{
			return shift == 1 ? (a >> 1) + (a & 1) : ((a >> (shift - 1)) + 1) >> 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int64 Silk_RSHIFT_ROUND64(opus_int64 a, int shift)
		{
			return shift == 1 ? (a >> 1) + (a & 1) : ((a >> (shift - 1)) + 1) >> 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_Min(opus_int32 a, opus_int32 b)
		{
			return a < b ? a : b;
		}



		/********************************************************************/
		/// <summary>
		/// Convert floating-point constants to fixed-point
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_FIX_CONST(c_float c, opus_int32 q)
		{
			return (opus_int32)(c * ((opus_int64)1 << q) + 0.5);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int Silk_Max(opus_int a, opus_int b)
		{
			return a > b ? a : b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int Silk_Min_Int(opus_int a, opus_int b)
		{
			return a < b ? a : b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_Min_32(opus_int32 a, opus_int32 b)
		{
			return a < b ? a : b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int Silk_Max_Int(opus_int a, opus_int b)
		{
			return a > b ? a : b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int16 Silk_Max_16(opus_int16 a, opus_int16 b)
		{
			return a > b ? a : b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_Max_32(opus_int32 a, opus_int32 b)
		{
			return a > b ? a : b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int Silk_LIMIT(opus_int a, opus_int limit1, opus_int limit2)
		{
			return limit1 > limit2 ? a > limit1 ? limit1 : a < limit2 ? limit2 : a
									: a > limit2 ? limit2 : a < limit1 ? limit1 : a;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int Silk_LIMIT_Int(opus_int a, opus_int limit1, opus_int limit2)
		{
			return Silk_LIMIT(a, limit1, limit2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int Silk_LIMIT_32(opus_int a, opus_int limit1, opus_int limit2)
		{
			return Silk_LIMIT(a, limit1, limit2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int Silk_Abs(opus_int a)
		{
			return a > 0 ? a : -a;	
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int Silk_RAND(opus_int seed)
		{
			return Silk_MLA_ovflw(Rand_Increment, (opus_uint32)seed, Rand_Multiplier);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Silk_SMMUL(opus_int32 a32, opus_int32 b32)
		{
			return (opus_int32)Silk_RSHIFT64(Silk_SMULL(a32, b32), 32);
		}
	}
}
