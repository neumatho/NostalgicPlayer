/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Share
{
	/// <summary>
	/// Helper class to work on numbers
	/// </summary>
	internal static class EndSwap
	{
		/********************************************************************/
		/// <summary>
		/// Swap the bytes in a 16-bit number
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int16_t EndSwap_16(int16_t x)
		{
			return (int16_t)(((x >> 8) & 0xff) | ((x & 0xff) << 8));
		}



		/********************************************************************/
		/// <summary>
		/// Swap the bytes in a 16-bit number
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint16_t EndSwap_16(uint16_t x)
		{
			return (uint16_t)(((x >> 8) & 0xff) | ((x & 0xff) << 8));
		}



		/********************************************************************/
		/// <summary>
		/// Swap the bytes in a 32-bit number
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int32_t EndSwap_32(int32_t x)
		{
			return ((x >> 24) & 0xff) | ((x >> 8) & 0xff00) | ((x & 0xff00) << 8) | ((x & 0xff) << 24);
		}



		/********************************************************************/
		/// <summary>
		/// Swap the bytes in a 32-bit number
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint32_t EndSwap_32(uint32_t x)
		{
			return ((x >> 24) & 0xff) | ((x >> 8) & 0xff00) | ((x & 0xff00) << 8) | ((x & 0xff) << 24);
		}



		/********************************************************************/
		/// <summary>
		/// Swap the bytes in a 64-bit number
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int64_t EndSwap_64(int64_t x)
		{
			return (int64_t)((uint32_t)(EndSwap_32((int32_t)((x >> 32) & 0xffffffff))) | ((uint64_t)EndSwap_32((int32_t)(x & 0xffffffff)) << 32));
		}



		/********************************************************************/
		/// <summary>
		/// Swap the bytes in a 64-bit number
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint64_t EndSwap_64(uint64_t x)
		{
			return EndSwap_32((uint32_t)((x >> 32) & 0xffffffff)) | ((uint64_t)EndSwap_32((uint32_t)(x & 0xffffffff)) << 32);
		}



		/********************************************************************/
		/// <summary>
		/// Host to little-endian byte swapping
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int16_t H2LE_16(int16_t x)
		{
			if (BitConverter.IsLittleEndian)
				return x;

			return EndSwap_16(x);
		}



		/********************************************************************/
		/// <summary>
		/// Host to little-endian byte swapping
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int32_t H2LE_32(int32_t x)
		{
			if (BitConverter.IsLittleEndian)
				return x;

			return EndSwap_32(x);
		}
	}
}
