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
	public static class BSwap
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint16_t Av_BSwap16C(uint16_t x)
		{
			return (uint16_t)(((x << 8) & 0xff00) | ((x >> 8) & 0x00ff));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint32_t Av_BSwap32C(uint32_t x)
		{
			return (uint32_t)(Av_BSwap16C((uint16_t)x) << 16) | Av_BSwap16C((uint16_t)(x >> 16));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint32_t Av_BSwap32(uint32_t x)
		{
			return Av_BSwap32C(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint64_t Av_BSwap64(uint64_t x)
		{
			return (uint64_t)Av_BSwap32((uint32_t)x) << 32 | Av_BSwap32((uint32_t)(x >> 32));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint64_t Av_Be2Ne64(uint64_t x)
		{
			if (BitConverter.IsLittleEndian)
				return Av_BSwap64(x);

			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint64_t Av_Le2Ne64(uint64_t x)
		{
			if (!BitConverter.IsLittleEndian)
				return Av_BSwap64(x);

			return x;
		}
	}
}
