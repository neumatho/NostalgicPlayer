/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class IntReadWrite
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint32_t Av_RB16(CPointer<char> x)
		{
			return ((uint32_t)x[0] << 8) | x[1];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Av_WB16(CPointer<uint8_t> p, uint16_t val)
		{
			p[1] = (uint8_t)(val);
			p[0] = (uint8_t)(val >> 8);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint32_t Av_RL16(CPointer<uint8_t> x)
		{
			return ((uint32_t)x[1] << 8) | x[0];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Av_WL16(CPointer<uint8_t> p, uint16_t val)
		{
			p[0] = (uint8_t)(val);
			p[1] = (uint8_t)(val >> 8);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint32_t Av_RB32(CPointer<uint8_t> x)
		{
			return ((uint32_t)x[0] << 24) | ((uint32_t)x[1] << 16) | ((uint32_t)x[2] << 8) | x[3];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint32_t Av_RB32(CPointer<char> x)
		{
			return ((uint32_t)x[0] << 24) | ((uint32_t)x[1] << 16) | ((uint32_t)x[2] << 8) | x[3];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Av_WB32(CPointer<uint8_t> p, uint32_t val)
		{
			p[3] = (uint8_t)(val);
			p[2] = (uint8_t)(val >> 8);
			p[1] = (uint8_t)(val >> 16);
			p[0] = (uint8_t)(val >> 24);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint32_t Av_RL32(CPointer<uint8_t> x)
		{
			return ((uint32_t)x[3] << 24) | ((uint32_t)x[2] << 16) | ((uint32_t)x[1] << 8) | x[0];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Av_WL32(CPointer<uint8_t> p, uint32_t val)
		{
			p[0] = (uint8_t)(val);
			p[1] = (uint8_t)(val >> 8);
			p[2] = (uint8_t)(val >> 16);
			p[3] = (uint8_t)(val >> 24);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint64_t Av_RB64(CPointer<uint8_t> x)
		{
			return ((uint64_t)x[0] << 56) | ((uint64_t)x[1] << 48) | ((uint64_t)x[2] << 40) | ((uint64_t)x[3] << 32) | ((uint64_t)x[4] << 24) | ((uint64_t)x[5] << 16) | ((uint64_t)x[6] << 8) | x[7];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Av_WB64(CPointer<uint8_t> p, uint64_t val)
		{
			p[7] = (uint8_t)(val);
			p[6] = (uint8_t)(val >> 8);
			p[5] = (uint8_t)(val >> 16);
			p[4] = (uint8_t)(val >> 24);
			p[3] = (uint8_t)(val >> 32);
			p[2] = (uint8_t)(val >> 40);
			p[1] = (uint8_t)(val >> 48);
			p[0] = (uint8_t)(val >> 56);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint64_t Av_RL64(CPointer<uint8_t> x)
		{
			return ((uint64_t)x[7] << 56) | ((uint64_t)x[6] << 48) | ((uint64_t)x[5] << 40) | ((uint64_t)x[4] << 32) | ((uint64_t)x[3] << 24) | ((uint64_t)x[2] << 16) | ((uint64_t)x[1] << 8) | x[0];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Av_WL64(CPointer<uint8_t> p, uint64_t val)
		{
			p[0] = (uint8_t)(val);
			p[1] = (uint8_t)(val >> 8);
			p[2] = (uint8_t)(val >> 16);
			p[3] = (uint8_t)(val >> 24);
			p[4] = (uint8_t)(val >> 32);
			p[5] = (uint8_t)(val >> 40);
			p[6] = (uint8_t)(val >> 48);
			p[7] = (uint8_t)(val >> 56);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint8_t Av_RB8(CPointer<uint8_t> x)
		{
			return x[0];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Av_WB8(CPointer<uint8_t> x, uint8_t val)
		{
			x[0] = val;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint8_t Av_RL8(CPointer<uint8_t> x)
		{
			return Av_RB8(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Av_WL8(CPointer<uint8_t> x, uint8_t val)
		{
			Av_WB8(x, val);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Av_WN16(CPointer<uint8_t> p, uint16_t val)
		{
			if (BitConverter.IsLittleEndian)
				Av_WL16(p, val);
			else
				Av_WB16(p, val);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Av_WN32(CPointer<uint8_t> p, uint32_t val)
		{
			if (BitConverter.IsLittleEndian)
				Av_WL32(p, val);
			else
				Av_WB32(p, val);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint64_t Av_RN64(CPointer<uint8_t> x)
		{
			return BitConverter.IsLittleEndian ? Av_RL64(x) : Av_RB64(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Av_WN64(CPointer<uint8_t> p, uint64_t val)
		{
			if (BitConverter.IsLittleEndian)
				Av_WL64(p, val);
			else
				Av_WB64(p, val);
		}
	}
}
