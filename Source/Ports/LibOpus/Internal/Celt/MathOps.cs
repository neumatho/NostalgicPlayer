/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt
{
	/// <summary>
	/// 
	/// </summary>
	internal class MathOps
	{
		/********************************************************************/
		/// <summary>
		/// Multiplies two 16-bit fractional values. Bit-exactness is
		/// important
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_int32 Frac_MUL16(opus_int16 a, opus_int16 b)
		{
			return (16384 + (a * b)) >> 15;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private const c_float cA = 0.43157974f;
		private const c_float cB = 0.67848403f;
		private const c_float cC = 0.08595542f;
		private const c_float cE = (c_float)(Math.PI / 2);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_float Fast_Atan2F(c_float y, c_float x)
		{
			c_float x2 = x * x;
			c_float y2 = y * y;

			// For very small values, we don't care about the answer, so
			// we can just return 0
			if ((x2 + y2) < 1e-18f)
				return 0;

			if (x2 < y2)
			{
				c_float den = (y2 + cB * x2) * (y2 + cC * x2);
				return -x * y* (y2 + cA * x2) / den + (y < 0 ? -cE : cE);
			}
			else
			{
				c_float den = (x2 + cB * y2) * (x2 + cC * y2);
				return x * y* (x2 + cA * y2) / den + (y < 0 ? -cE : cE) - (x * y < 0 ? -cE : cE);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the arctangent of x using a Remez approximation of
		/// order 15, incorporating only odd-powered terms
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_float Celt_Atan_Norm(c_float x)
		{
			const c_float ATAN2_2_OVER_PI = 0.636619772367581f;
			c_float x_sq = x * x;

			// Polynomial coefficients approximated in the [0, 1] range.
			// Lolremez command: lolremez --degree 6 --range "0:1"
			//                   "(atan(sqrt(x))-sqrt(x))/(x*sqrt(x))" "1/(sqrt(x)*x)"
			// Please note that ATAN2_COEFF_A01 is fixed to 1.0f
			const c_float ATAN2_COEFF_A03 = -3.3331659436225891113281250000e-01f;
			const c_float ATAN2_COEFF_A05 = 1.99627041816711425781250000000e-01f;
			const c_float ATAN2_COEFF_A07 = -1.3976582884788513183593750000e-01f;
			const c_float ATAN2_COEFF_A09 = 9.79423448443412780761718750000e-02f;
			const c_float ATAN2_COEFF_A11 = -5.7773590087890625000000000000e-02f;
			const c_float ATAN2_COEFF_A13 = 2.30401363223791122436523437500e-02f;
			const c_float ATAN2_COEFF_A15 = -4.3554059229791164398193359375e-03f;

			return ATAN2_2_OVER_PI * (x + (x * x_sq * (ATAN2_COEFF_A03
			+ (x_sq * (ATAN2_COEFF_A05
			+ (x_sq * (ATAN2_COEFF_A07
			+ (x_sq * (ATAN2_COEFF_A09
			+ (x_sq * (ATAN2_COEFF_A11
			+ (x_sq * (ATAN2_COEFF_A13
			+ (x_sq * (ATAN2_COEFF_A15)))))))))))))));
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the arctangent of y/x, returning an approximate value
		/// in radians.
		/// Please refer to the linked wiki page (https://en.wikipedia.org/wiki/Atan2)
		/// to learn how atan2 results are computed
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_float Celt_Atanp_Norm(c_float y, c_float x)
		{
			// For very small values, we don't care about the answer
			if (((x * x) + (y * y)) < 1e-18f)
				return 0;

			if (y < x)
				return Celt_Atan_Norm(y / x);
			else
				return 1.0f - Celt_Atan_Norm(x / y);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_val32 Celt_Sqrt(opus_val32 x)
		{
			return (c_float)Math.Sqrt(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_val32 Celt_Sqrt32(opus_val32 x)
		{
			return (c_float)Math.Sqrt(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_val32 Celt_Rsqrt(opus_val32 x)
		{
			return 1.0f / Celt_Sqrt(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_val32 Celt_Rsqrt_Norm(opus_val32 x)
		{
			return Celt_Rsqrt(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_val32 Celt_Rsqrt_Norm32(opus_val32 x)
		{
			return Celt_Rsqrt(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_val32 Celt_Cos_Norm(opus_val32 x)
		{
			return (c_float)Math.Cos((0.5 * Math.PI) * x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_val32 Celt_Rcp(opus_val32 x)
		{
			return 1.0f / x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static opus_val16 Celt_Div(opus_val32 a, opus_val32 b)
		{
			return a / b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_float Frac_Div32(c_float a, c_float b)
		{
			return a / b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_float Celt_Log2(c_float x)
		{
			return (c_float)(1.442695040888963387 * Math.Log(x));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_float Celt_Exp2(c_float x)
		{
			return (c_float)Math.Exp(0.6931471805599453094 * x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_float Celt_Exp2_Db(c_float x)
		{
			return Celt_Exp2(x);
		}



		/********************************************************************/
		/// <summary>
		/// Compute floor(sqrt(val)) with exact arithmetic
		/// </summary>
		/********************************************************************/
		public static c_uint Isqrt32(opus_uint32 _val)
		{
			// Uses the second method from
			//  http://www.azillionmonkeys.com/qed/sqroot.html
			// The main idea is to search for the largest binary digit b such that
			//  (g+b)*(g+b) <= _val, and add it to the solution g
			c_uint g = 0;
			c_int bshift = (EntCode.Ec_Ilog(_val) - 1) >> 1;
			c_uint b = 1U << bshift;

			do
			{
				opus_uint32 t = ((g << 1) + b) << bshift;
				if (t <= _val)
				{
					g += b;
					_val -= t;
				}

				b >>= 1;
				bshift--;
			}
			while (bshift >= 0);

			return g;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Celt_Float2Int16(CPointer<c_float> _in, CPointer<c_short> _out, c_int cnt, c_int arch)
		{
			Celt_Float2Int16_C(_in, _out, cnt);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Celt_Float2Int16_C(CPointer<c_float> _in, CPointer<c_short> _out, c_int cnt)
		{
			for (c_int i = 0; i < cnt; i++)
				_out[i] = Float_Cast.Float2Int16(_in[i]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Opus_Limit2_CheckWithin1(CPointer<c_float> samples, c_int cnt, c_int arch)
		{
			return Opus_Limit2_CheckWithin1_C(samples, cnt);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Opus_Limit2_CheckWithin1_C(CPointer<c_float> samples, c_int cnt)
		{
			if (cnt <= 0)
				return 1;

			for (c_int i = 0; i < cnt; i++)
			{
				c_float clippedVal = samples[i];

				clippedVal = Arch.FMAX(-2.0f, clippedVal);
				clippedVal = Arch.FMIN(2.0f, clippedVal);

				samples[i] = clippedVal;
			}

			return 0;
		}
	}
}
