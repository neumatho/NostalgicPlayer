/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Util
	{
		/********************************************************************/
		/// <summary>
		/// Multiply two 32-bit integers, receive 64-bit result
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int64 Mul32To64(int32 a, int32 b)
		{
			return (int64)a * b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint64 Mul32To64_Unsigned(uint32 a, uint32 b)
		{
			return (uint64)a * b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int32 MulDiv(int32 a, int32 b, int32 c)
		{
			return int32.CreateSaturating(Mul32To64(a, b) / c);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int32 MulDivR(int32 a, int32 b, int32 c)
		{
			return int32.CreateSaturating((Mul32To64(a, b) + (c / 2)) / c);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint32 MulDiv_Unsigned(uint32 a, uint32 b, uint32 c)
		{
			return uint32.CreateSaturating(Mul32To64_Unsigned(a, b) / c);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint32 MulDivR_Unsigned(uint32 a, uint32 b, uint32 c)
		{
			return uint32.CreateSaturating((Mul32To64_Unsigned(a, b) + (c / 2U)) / c);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int32 MulDivRFloor(int64 a, uint32 b, uint32 c)
		{
			a *= b;
			a += c / 2U;

			return a >= 0 ? int32.CreateSaturating(a / c) : int32.CreateSaturating((a - (c - 1)) / c);
		}
	}
}
