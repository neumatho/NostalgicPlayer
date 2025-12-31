/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Kit.C
{
	/// <summary>
	/// Extra math functions that are not part of the standard CMath class
	/// </summary>
	public static partial class CMath
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double abs(c_double x)
		{
			return Math.Abs(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double acos(c_double x)
		{
			return Math.Acos(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double asin(c_double x)
		{
			return Math.Asin(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double atan(c_double x)
		{
			return Math.Atan(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double atan2(c_double y, c_double x)
		{
			return Math.Atan2(y, x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double ceil(c_double x)
		{
			return Math.Ceiling(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double cos(c_double x)
		{
			return Math.Cos(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double cosh(c_double x)
		{
			return Math.Cosh(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double exp(c_double x)
		{
			return Math.Exp(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_double exp2(c_double x)
		{
			if (c_double.IsNaN(x))
				return double.NaN;

			if (c_double.IsPositiveInfinity(x))
				return double.PositiveInfinity;

			if (c_double.IsNegativeInfinity(x))
				return 0.0;

			// Theoretical limits for double:
			// 2^1024 -> +∞ (overflow), 2^-1075 -> 0 (underflow to 0)
			if (x >= 1024.0)
				return double.PositiveInfinity;

			if (x <= -1075.0)
				return 0.0;

			// Split x = i + f, where i is an integer and f i [-0.5, 0.5) for good precision
			c_int i = (c_int)Math.Round(x);
			c_double f = x - i;

			// exp2(f) = exp(f * ln(2))
			const c_double LN2 = 0.693147180559945309417232121458176568; 
			c_double frac = Math.Exp(f * LN2);

			// 2^x = 2^i * 2^f  =>  ScaleB(frac, i) = frac * 2^i
			return Math.ScaleB(frac, i);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double fabs(c_double x)
		{
			return Math.Abs(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double floor(c_double x)
		{
			return Math.Floor(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double fmod(c_double x, c_double y)
		{
			return x % y;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double hypot(c_double x, c_double y)
		{
			// Handle special values first (matching C behavior)
			if (c_double.IsInfinity(x) || c_double.IsInfinity(y))
				return c_double.PositiveInfinity;

			if (c_double.IsNaN(x) || c_double.IsNaN(y))
				return c_double.NaN;

			x = Math.Abs(x);
			y = Math.Abs(y);

			// Ensure x >= y
			if (x < y)
				(x, y) = (y, x);

			// If the smaller is 0, result is the larger
			if (y == 0.0)
				return x;

			// Fast path for very different magnitudes (y negligible)
			c_int ex = Math.ILogB(x);
			c_int ey = Math.ILogB(y);
			c_int diff = ex - ey;

			if (diff > 30)	// 2^-30 precision is enough; y contribution negligible
				return x;

			// Scale to reduce risk of overflow/underflow
			// Scale both numbers by 2^-ex so largest becomes ~1.
			c_double scaledX = Math.ScaleB(x, -ex);
			c_double scaledY = Math.ScaleB(y, -ex);
			c_double r = Math.Sqrt((scaledX * scaledX) + (scaledY * scaledY));

			return Math.ScaleB(r, ex);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long llrint(c_double x)
		{
			if (c_double.IsInfinity(x) || c_double.IsNaN(x))
				return 0;

			return (long)Math.Round(x, MidpointRounding.ToEven);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double log(c_double x)
		{
			return Math.Log(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double log10(c_double x)
		{
			return Math.Log10(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double pow(c_double x, c_double y)
		{
			return Math.Pow(x, y);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double round(c_double x)
		{
			return Math.Round(x, MidpointRounding.AwayFromZero);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double sin(c_double x)
		{
			return Math.Sin(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double sinh(c_double x)
		{
			return Math.Sinh(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double sqrt(c_double x)
		{
			return Math.Sqrt(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double tan(c_double x)
		{
			return Math.Tan(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double tanh(c_double x)
		{
			return Math.Tanh(x);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_double trunc(c_double x)
		{
			return Math.Truncate(x);
		}
	}
}
