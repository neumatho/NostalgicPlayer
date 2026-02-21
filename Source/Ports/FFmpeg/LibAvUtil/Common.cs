/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class Common
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Av_Ceil_RShift<T>(T a, c_int b) where T : INumber<T>, IShiftOperators<T, c_int, T>
		{
			return -(-a) >> b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T FFAbs<T>(T a) where T : INumber<T>
		{
			return a >= T.Zero ? a : -a;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Av_Clip(c_int a, c_int aMin, c_int aMax)
		{
			return Av_Clip_C(a, aMin, aMax);
		}



		/********************************************************************/
		/// <summary>
		/// Clip a signed integer value into the amin-amax range
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Av_Clip_C(c_int a, c_int aMin, c_int aMax)
		{
			if (a < aMin)
				return aMin;
			else if (a > aMax)
				return aMax;
			else
				return a;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int64_t Av_Clip64(int64_t a, int64_t aMin, int64_t aMax)
		{
			return Av_Clip64_C(a, aMin, aMax);
		}



		/********************************************************************/
		/// <summary>
		/// Clip a signed 64-bit integer value into the amin-amax range
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int64_t Av_Clip64_C(int64_t a, int64_t aMin, int64_t aMax)
		{
			if (a < aMin)
				return aMin;
			else if (a > aMax)
				return aMax;
			else
				return a;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint8_t Av_Clip_UInt8(c_int a)
		{
			return Av_Clip_UInt8_C(a);
		}



		/********************************************************************/
		/// <summary>
		/// Clip a signed integer value into the -128,127 range
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint8_t Av_Clip_UInt8_C(c_int a)
		{
			if ((a & ~0xff) != 0)
				return (uint8_t)((~a) >> 31);
			else
				return (uint8_t)a;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int16_t Av_Clip_Int16(c_int a)
		{
			return Av_Clip_Int16_C(a);
		}



		/********************************************************************/
		/// <summary>
		/// Clip a signed integer value into the -32768,32767 range
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int16_t Av_Clip_Int16_C(c_int a)
		{
			if (((a + 0x8000U) & ~0xffff) != 0)
				return (int16_t)((a >> 31) ^ 0x7fff);
			else
				return (int16_t)a;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int32_t Av_ClipL_Int32(int64_t a)
		{
			return Av_ClipL_Int32_C(a);
		}



		/********************************************************************/
		/// <summary>
		/// Clip a signed 64-bit integer value into the
		/// -2147483648,2147483647 range
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int32_t Av_ClipL_Int32_C(int64_t a)
		{
			if ((((uint64_t)a + 0x80000000) & ~(uint64_t)0xffffffff) != 0)
				return (int32_t)((a >> 63) ^ 0x7fffffff);
			else
				return (int32_t)a;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int64_t Av_Sat_Sub64(int64_t a, int64_t b)
		{
			return Av_Sat_Sub64_C(a, b);
		}



		/********************************************************************/
		/// <summary>
		/// Subtract two signed 64-bit values with saturation
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int64_t Av_Sat_Sub64_C(int64_t a, int64_t b)
		{
			if ((b <= 0) && (a >= (int64_t.MaxValue + b)))
				return int64_t.MaxValue;

			if ((b >= 0) && (a <= (int64_t.MinValue + b)))
				return int64_t.MinValue;

			return a - b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double Av_ClipD(c_double a, c_double aMin, c_double aMax)
		{
			return Av_ClipD_C(a, aMin, aMax);
		}



		/********************************************************************/
		/// <summary>
		/// Clip a double value into the amin-amax range.
		/// If a is nan or -inf amin will be returned.
		/// If a is +inf amax will be returned.
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double Av_ClipD_C(c_double a, c_double aMin, c_double aMax)
		{
			return Macros.FFMin(Macros.FFMax(a, aMin), aMax);
		}



		/********************************************************************/
		/// <summary>
		/// Count number of bits set to one in x
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Av_PopCount(uint32_t x)
		{
			return Av_PopCount_C(x);
		}



		/********************************************************************/
		/// <summary>
		/// Count number of bits set to one in x
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Av_PopCount_C(uint32_t x)
		{
			x -= (x >> 1) & 0x55555555;
			x = (x & 0x33333333) + ((x >> 2) & 0x33333333);
			x = (x + (x >> 4)) & 0x0F0F0F0F;
			x += x >> 8;

			return (c_int)((x + (x >> 16)) & 0x3f);
		}



		/********************************************************************/
		/// <summary>
		/// Count number of bits set to one in x
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Av_PopCount64(uint64_t x)
		{
			return Av_PopCount64_C(x);
		}



		/********************************************************************/
		/// <summary>
		/// Count number of bits set to one in x
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Av_PopCount64_C(uint64_t x)
		{
			return Av_PopCount((uint32_t)x) + Av_PopCount((uint32_t)(x >> 32));
		}



		/********************************************************************/
		/// <summary>
		/// Convert a UTF-16 character (2 or 4 bytes) to its 32-bit UCS-4
		/// encoded form
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint32_t Get_Utf16(UtilFunc.Utf8_Get16_Delegate get_16Bit, out bool error)
		{
			error = false;

			uint32_t val = get_16Bit();

			c_uint hi = val - 0xd800;

			if (hi < 0x800)
			{
				val = (uint32_t)get_16Bit() - 0xdc00;

				if ((val > 0x3ff) || (hi > 0x3ff))
				{
					error = true;
					return 0;
				}

				val += (hi << 10) + 0x10000;
			}

			return val;
		}



		/********************************************************************/
		/// <summary>
		/// Convert a 32-bit Unicode character to its UTF-8 encoded form
		/// (up to 4 bytes long)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Put_Utf8(uint32_t val, UtilFunc.Utf8_Write_Delegate put_Byte)
		{
			uint32_t @in = val;
			uint8_t tmp;

			if (@in < 0x80)
			{
				tmp = (uint8_t)@in;
				put_Byte(tmp);
			}
			else
			{
				c_int bytes = (IntMath.Av_Log2(@in) + 4) / 5;
				c_int shift = (bytes - 1) * 6;

				tmp = (uint8_t)((uint8_t)(256 - (256 >> bytes)) | (uint8_t)(@in >> shift));
				put_Byte(tmp);

				while (shift >= 6)
				{
					shift -= 6;

					tmp = (uint8_t)(0x80 | ((@in >> shift) & 0x3f));
					put_Byte(tmp);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Add two signed 64-bit values with saturation
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int64_t Av_Sat_Add64(int64_t a, int64_t b)
		{
			return Av_Sat_Add64_C(a, b);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Add two signed 64-bit values with saturation
		/// </summary>
		/********************************************************************/
		private static int64_t Av_Sat_Add64_C(int64_t a, int64_t b)
		{
			int64_t s = (int64_t)((uint64_t)a + (uint64_t)b);

			if ((a ^ b | ~s ^ b) >= 0)
				return int64_t.MaxValue ^ (b >> 63);

			return s;
		}
		#endregion
	}
}
