/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// Miscellaneous math routines and tables
	/// </summary>
	public static class Mathematics
	{
		/// <summary></summary>
		public const c_double M_Log2_10 = 3.32192809488736234787;	// log_2 10
		/// <summary></summary>
		public const c_double M_Phi = 1.61803398874989484820;		// phi / golden ratio

		/********************************************************************/
		/// <summary>
		/// Stein's binary GCD algorithm:
		/// https://en.wikipedia.org/wiki/Binary_GCD_algorithm
		/// </summary>
		/********************************************************************/
		public static int64_t Av_Gcd(int64_t a, int64_t b)//XX 37
		{
			if (a == 0)
				return b;

			if (b == 0)
				return a;

			c_int za = IntMath.FF_Ctzll(a);
			c_int zb = IntMath.FF_Ctzll(b);
			c_int k = Macros.FFMin(za, zb);
			int64_t u = Math.Abs(a >> za);
			int64_t v = Math.Abs(b >> zb);

			while (u != v)
			{
				if (u > v)
					Macros.FFSwap(ref v, ref u);

				v -= u;
				v >>= IntMath.FF_Ctzll(v);
			}

			return (int64_t)((uint64_t)u << k);
		}



		/********************************************************************/
		/// <summary>
		/// Rescale a 64-bit integer with specified rounding.
		///
		/// The operation is mathematically equivalent to `a * b / c`, but
		/// writing that directly can overflow, and does not support
		/// different rounding methods. If the result is not representable
		/// then INT64_MIN is returned.
		///
		/// See av_rescale(), av_rescale_q(), av_rescale_q_rnd()
		/// </summary>
		/********************************************************************/
		public static int64_t Av_Rescale_Rnd(int64_t a, int64_t b, int64_t c, AvRounding rnd)//XX 58
		{
			int64_t r = 0;

			if ((c <= 0) || (b < 0) || !(((c_uint)(rnd & ~AvRounding.Pass_MinMax) <= 5) && ((rnd & ~AvRounding.Pass_MinMax) != (AvRounding)4)))
				return int64_t.MinValue;

			if ((rnd & AvRounding.Pass_MinMax) != 0)
			{
				if ((a == int64_t.MinValue) || (a == int64_t.MaxValue))
					return a;

				rnd -= AvRounding.Pass_MinMax;
			}

			if (a < 0)
				return -Av_Rescale_Rnd(-Macros.FFMax(a, -int64_t.MaxValue), b, c, (AvRounding)((c_int)rnd ^ (((c_int)rnd >> 1) & 1)));

			if (rnd == AvRounding.Near_Inf)
				r = c / 2;
			else if ((rnd & AvRounding.Inf) != 0)
				r = c - 1;

			if ((b <= c_int.MaxValue) && (c <= c_int.MaxValue))
			{
				if (a <= c_int.MaxValue)
					return ((a * b) + r) / c;
				else
				{
					int64_t ad = a / c;
					int64_t a2 = ((a % c * b) + r) / c;

					if ((ad >= c_int.MaxValue) && (b != 0) && (ad > ((int64_t.MaxValue - a2) / b)))
						return int64_t.MinValue;

					return (ad * b) + a2;
				}
			}
			else
			{
				uint64_t a0 = (uint64_t)a & 0xffffffff;
				uint64_t a1 = (uint64_t)a >> 32;
				uint64_t b0 = (uint64_t)b & 0xffffffff;
				uint64_t b1 = (uint64_t)b >> 32;
				uint64_t t1 = (a0 * b1) + (a1 * b0);
				uint64_t t1a = t1 << 32;

				a0 = (a0 * b0) + t1a;
				a1 = (a1 * b1) + (t1 >> 32) + (a0 < t1a ? 1U : 0U);
				a0 = (uint64_t)((int64_t)a0 + r);

				for (c_int i = 63; i >= 0; i--)
				{
					a1 += a1 + ((a0 >> i) & 1);
					t1 += t1;

					if ((uint64_t)c <= a1)
					{
						a1 = (uint64_t)((int64_t)a1 - c);
						t1++;
					}
				}

				if (t1 > int64_t.MaxValue)
					return int64_t.MinValue;

				return (int64_t)t1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Rescale a 64-bit integer with rounding to nearest.
		///
		/// The operation is mathematically equivalent to `a * b / c`, but
		/// writing that directly can overflow.
		///
		/// This function is equivalent to av_rescale_rnd() with
		/// AV_ROUND_NEAR_INF.
		///
		/// See av_rescale_rnd(), av_rescale_q(), av_rescale_q_rnd()
		/// </summary>
		/********************************************************************/
		public static int64_t Av_Rescale(int64_t a, int64_t b, int64_t c)//XX 129
		{
			return Av_Rescale_Rnd(a, b, c, AvRounding.Near_Inf);
		}



		/********************************************************************/
		/// <summary>
		/// Rescale a 64-bit integer with specified rounding.
		///
		/// The operation is mathematically equivalent to `a * b / c`, but
		/// writing that directly can overflow, and does not support
		/// different rounding methods. If the result is not representable
		/// then INT64_MIN is returned.
		///
		/// See av_rescale(), av_rescale_q(), av_rescale_q_rnd()
		/// </summary>
		/********************************************************************/
		public static int64_t Av_Rescale_Q_Rnd(int64_t a, AvRational bq, AvRational cq, AvRounding rnd)//XX 134
		{
			int64_t b = bq.Num * (int64_t)cq.Den;
			int64_t c = cq.Num * (int64_t)bq.Den;

			return Av_Rescale_Rnd(a, b, c, rnd);
		}



		/********************************************************************/
		/// <summary>
		/// Rescale a 64-bit integer by 2 rational numbers.
		///
		/// The operation is mathematically equivalent to `a * bq / cq`.
		///
		/// This function is equivalent to av_rescale_q_rnd() with
		/// AV_ROUND_NEAR_INF.
		///
		/// See av_rescale(), av_rescale_rnd(), av_rescale_q_rnd()
		/// </summary>
		/********************************************************************/
		public static int64_t Av_Rescale_Q(int64_t a, AvRational bq, AvRational cq)//XX 142
		{
			return Av_Rescale_Q_Rnd(a, bq, cq, AvRounding.Near_Inf);
		}



		/********************************************************************/
		/// <summary>
		/// Compare two timestamps each in its own time base
		/// </summary>
		/********************************************************************/
		public static c_int Av_Compare_Ts(int64_t ts_A, AvRational tb_A, int64_t ts_B, AvRational tb_B)//XX 147
		{
			int64_t a = tb_A.Num * (int64_t)tb_B.Den;
			int64_t b = tb_B.Num * (int64_t)tb_A.Den;

			if ((Common.FFAbs(ts_A) | a | Common.FFAbs(ts_B) | b) <= c_int.MaxValue)
				return (((ts_A * a) > (ts_B * b)) ? 1 : 0) - (((ts_A * a) < (ts_B * b)) ? 1 : 0);

			if (Av_Rescale_Rnd(ts_A, a, b, AvRounding.Down) < ts_B)
				return -1;

			if (Av_Rescale_Rnd(ts_B, b, a, AvRounding.Down) < ts_A)
				return 1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Compare the remainders of two integer operands divided by a
		/// common divisor.
		///
		/// In other words, compare the least significant `log2(mod)` bits of
		/// integers `a` and `b`
		/// </summary>
		/********************************************************************/
		public static int64_t Av_Compare_Mod(uint64_t a, uint64_t b, uint64_t mod)//XX 160
		{
			int64_t c = (int64_t)((a - b) & (mod - 1));

			if (c > (int64_t)(mod >> 1))
				c -= (int64_t)mod;

			return c;
		}



		/********************************************************************/
		/// <summary>
		/// Add a value to a timestamp.
		///
		/// This function guarantees that when the same value is repeatedly
		/// added that no accumulation of rounding errors occurs
		/// </summary>
		/********************************************************************/
		public static int64_t Av_Add_Stable(AvRational ts_tb, int64_t ts, AvRational inc_tb, int64_t inc)//XX 191
		{
			if (inc != 1)
				inc_tb = Rational.Av_Mul_Q(inc_tb, new AvRational((c_int)inc, 1));

			int64_t m = inc_tb.Num * (int64_t)ts_tb.Den;
			int64_t d = inc_tb.Den * (int64_t)ts_tb.Num;

			if (((m % d) == 0) && (ts <= (int64_t.MaxValue - (m / d))))
				return ts + (m / d);

			if (m < d)
				return ts;

			{
				int64_t old = Av_Rescale_Q(ts, ts_tb, inc_tb);
				int64_t old_ts = Av_Rescale_Q(old, inc_tb, ts_tb);

				if ((old == int64_t.MaxValue) || (old == UtilConstants.Av_NoPts_Value) || (old_ts == UtilConstants.Av_NoPts_Value))
					return ts;

				return Common.Av_Sat_Add64(Av_Rescale_Q(old + 1, inc_tb, ts_tb), ts - old_ts);
			}
		}
	}
}
