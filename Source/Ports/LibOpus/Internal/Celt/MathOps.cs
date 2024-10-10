/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;

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
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_float Celt_Sqrt(c_float x)
		{
			return (c_float)Math.Sqrt(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_float Celt_Rsqrt(c_float x)
		{
			return 1.0f / Celt_Sqrt(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_float Celt_Rsqrt_Norm(c_float x)
		{
			return Celt_Rsqrt(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_float Celt_Cos_Norm(c_float x)
		{
			return (c_float)Math.Cos((0.5 * Math.PI) * x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_float Celt_Rcp(c_float x)
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
	}
}
