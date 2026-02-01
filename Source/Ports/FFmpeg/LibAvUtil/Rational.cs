/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// Rational numbers
	/// </summary>
	public static class Rational
	{
		/********************************************************************/
		/// <summary>
		/// Create an AVRational.
		///
		/// Useful for compilers that do not support compound literals
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static AvRational Av_Make_Q(c_int num, c_int den)
		{
			AvRational r = new AvRational(num, den);

			return r;
		}



		/********************************************************************/
		/// <summary>
		/// Compare two rationals
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Av_Cmp_Q(AvRational a, AvRational b)
		{
			int64_t tmp = (a.Num * (int64_t)b.Den) - (b.Num * (int64_t)a.Den);

			if (tmp != 0)
				return (c_int)((tmp ^ a.Den ^ b.Den) >> 63) | 1;
			else if ((b.Den != 0) && (a.Den != 0))
				return 0;
			else if ((a.Num != 0) && (b.Num != 0))
				return (a.Num >> 31) - (b.Num >> 31);
			else
				return c_int.MinValue;
		}



		/********************************************************************/
		/// <summary>
		/// Convert an AVRational to a `double`
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double Av_Q2D(AvRational a)
		{
			return a.Num / (c_double)a.Den;
		}



		/********************************************************************/
		/// <summary>
		/// Invert a rational
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static AvRational Av_Inv_Q(AvRational q)
		{
			AvRational r = new AvRational(q.Den, q.Num);

			return r;
		}



		/********************************************************************/
		/// <summary>
		/// Reduce a fraction.
		///
		/// This is useful for framerate calculations
		/// </summary>
		/********************************************************************/
		public static c_int Av_Reduce(out c_int dst_Num, out c_int dst_Den, int64_t num, int64_t den, int64_t max)//XX 35
		{
			AvRational a0 = new AvRational(0, 1);
			AvRational a1 = new AvRational(1, 0);
			c_int sign = (num < 0 ? 1 : 0) ^ (den < 0 ? 1 : 0);
			int64_t gcd = Mathematics.Av_Gcd(Common.FFAbs(num), Common.FFAbs(den));

			if (gcd != 0)
			{
				num = Common.FFAbs(num) / gcd;
				den = Common.FFAbs(den) / gcd;
			}

			if ((num <= max) && (den <= max))
			{
				a1 = new AvRational((c_int)num, (c_int)den);
				den = 0;
			}

			while (den != 0)
			{
				int64_t x = num / den;
				int64_t next_Den = num - (den * x);
				int64_t a2n = (x * a1.Num) + a0.Num;
				int64_t a2d = (x * a1.Den) + a0.Den;

				if ((a2n > max) || (a2d > max))
				{
					if (a1.Num != 0)
						x = (max - a0.Num) / a1.Num;

					if (a1.Den != 0)
						x = Macros.FFMin(x, (max - a0.Den) / a1.Den);

					if ((den * ((2 * x * a1.Den) + a0.Den)) > (num * a1.Den))
						a1 = new AvRational((c_int)((x * a1.Num) + a0.Num), (c_int)((x * a1.Den) + a0.Den));

					break;
				}

				a0 = a1;
				a1 = new AvRational((c_int)a2n, (c_int)a2d);
				num = den;
				den = next_Den;
			}

			dst_Num = sign != 0 ? -a1.Num : a1.Num;
			dst_Den = a1.Den;

			return den == 0 ? 1 : 0;
		}



		/********************************************************************/
		/// <summary>
		/// Multiply two rationals
		/// </summary>
		/********************************************************************/
		public static AvRational Av_Mul_Q(AvRational b, AvRational c)//XX 80
		{
			Av_Reduce(out b.Num, out b.Den, b.Num * (int64_t)c.Num, b.Den * (int64_t)c.Den, c_int.MaxValue);

			return b;
		}



		/********************************************************************/
		/// <summary>
		/// Add two rationals
		/// </summary>
		/********************************************************************/
		public static AvRational Av_Add_Q(AvRational b, AvRational c)//XX 93
		{
			Av_Reduce(out b.Num, out b.Den, (b.Num * (int64_t)c.Den) + (c.Num * (int64_t)b.Den), b.Den * (int64_t)c.Den, c_int.MaxValue);

			return b;
		}



		/********************************************************************/
		/// <summary>
		/// Subtract one rational from another
		/// </summary>
		/********************************************************************/
		public static AvRational Av_Sub_Q(AvRational b, AvRational c)//XX 101
		{
			return Av_Add_Q(b, new AvRational(-c.Num, c.Den));
		}



		/********************************************************************/
		/// <summary>
		/// Convert a double precision floating point number to a rational.
		///
		/// In case of infinity, the returned value is expressed as `{1, 0}`
		/// or `{-1, 0}` depending on the sign.
		///
		/// In general rational numbers with |num| ‹= 1‹‹26 ＆＆ |den| ‹= 1‹‹26
		/// can be recovered exactly from their double representation.
		/// (no exceptions were found within 1B random ones)
		/// </summary>
		/********************************************************************/
		public static AvRational Av_D2Q(c_double d, c_int max)//XX 106
		{
			if (c_double.IsNaN(d))
				return new AvRational(0, 0);

			if (Math.Abs(d) > (c_int.MaxValue + 3L))
				return new AvRational(d < 0 ? -1 : 1, 0);

			CMath.frexp(d, out c_int exponent);
			exponent = Macros.FFMax(exponent - 1, 0);
			int64_t den = 1L << (62 - exponent);

			AvRational a = new AvRational();

			Av_Reduce(out a.Num, out a.Den, (int64_t)Math.Floor((d * den) + 0.5), den, max);

			return a;
		}



		/********************************************************************/
		/// <summary>
		/// Convert an AVRational to a IEEE 32-bit `float` expressed in
		/// fixed-point format
		/// </summary>
		/********************************************************************/
		public static uint32_t Av_Q2IntFloat(AvRational q)//XX 150
		{
			c_int sign = 0;

			if (q.Den < 0)
			{
				q.Den *= -1;
				q.Num *= -1;
			}

			if (q.Num < 0)
			{
				q.Num *= -1;
				sign = 1;
			}

			if ((q.Num == 0) && (q.Den == 0))
				return 0xffc00000;

			if (q.Num == 0)
				return 0;

			if (q.Den == 0)
				return (uint32_t)(0x7f800000 | (q.Num & 0x80000000));

			int64_t n;
			c_int shift = 23 + IntMath.Av_Log2((c_uint)q.Den) - IntMath.Av_Log2((c_uint)q.Num);

			if (shift >= 0)
				n = Mathematics.Av_Rescale(q.Num, 1L << shift, q.Den);
			else
				n = Mathematics.Av_Rescale(q.Num, 1, ((int64_t)q.Den) << -shift);

			shift -= (n >= (1 << 24) ? 1 : 0);
			shift += (n < (1 << 23) ? 1 : 0);

			if (shift >= 0)
				n = Mathematics.Av_Rescale(q.Num, 1L << shift, q.Den);
			else
				n = Mathematics.Av_Rescale(q.Num, 1, ((int64_t)q.Den) << -shift);

			return (uint32_t)(sign << 31) | (uint32_t)((150 - shift) << 23) | (uint32_t)(n - (1 << 23));
		}
	}
}
