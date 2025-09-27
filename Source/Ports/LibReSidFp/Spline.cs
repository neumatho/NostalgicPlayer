/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibReSidFp
{
	/// <summary>
	/// Fritsch-Carlson monotone cubic spline interpolation.
	///
	/// Based on the implementation from the [Monotone cubic interpolation] wikipedia page.
	///
	/// [Monotone cubic interpolation]: https://en.wikipedia.org/wiki/Monotone_cubic_interpolation
	/// </summary>
	internal class Spline
	{
		#region Point structure
		public struct Point
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Point(double x, double y)
			{
				this.x = x;
				this.y = y;
			}

			public double x;
			public double y;
		}
		#endregion

		#region Param structure
		private class Param
		{
			public double x1;
			public double x2;
			public double a;
			public double b;
			public double c;
			public double d;
		}
		#endregion

		// Interpolation parameters
		private readonly Param[] @params;

		// Last used parameters, cached for speed up
		private Param c;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Spline(List<Point> input)
		{
			@params = ArrayHelper.InitializeArray<Param>(input.Count);
			c = @params[0];

			uint coeffLength = (uint)input.Count - 1;

			double[] dxs = new double[coeffLength];
			double[] ms = new double[coeffLength];

			// Get consecutive differences and slopes
			for (int i = 0; i < coeffLength; i++)
			{
				double dx = input[i + 1].x - input[i].x;
				double dy = input[i + 1].y - input[i].y;

				dxs[i] = dx;
				ms[i] = dy / dx;
			}

			// Get degree-1 coefficients
			@params[0].c = ms[0];

			for (uint i = 1; i < coeffLength; i++)
			{
				double m = ms[i - 1];
				double mNext = ms[i];

				if (m * mNext <= 0)
					@params[i].c = 0.0;
				else
				{
					double dx = dxs[i - 1];
					double dxNext = dxs[i];
					double common = dx + dxNext;
					@params[i].c = 3.0 * common / ((common + dxNext) / m + (common + dx) / mNext);
				}
			}

			@params[coeffLength].c = ms[coeffLength - 1];

			// Get degree-2 and degree-3 coefficients
			for (int i = 0; i < coeffLength; i++)
			{
				@params[i].x1 = input[i].x;
				@params[i].x2 = input[i + 1].x;
				@params[i].d = input[i].y;

				double c1 = @params[i].c;
				double m = ms[i];
				double invDx = 1.0 / dxs[i];
				double common = c1 + @params[i + 1].c - m - m;

				@params[i].b = (m - c1 - common) * invDx;
				@params[i].a = common * invDx * invDx;
			}

			// Fix the upper range, because we interpolate outside original bounds if necessary
			@params[coeffLength - 1].x2 = double.MaxValue;
		}



		/********************************************************************/
		/// <summary>
		/// Evaluate y and its derivative at given point x
		/// </summary>
		/********************************************************************/
		public Point Evaluate(double x)
		{
			if ((x < c.x1) || (x > c.x2))
			{
				foreach (Param param in @params)
				{
					if (x <= param.x2)
					{
						c = param;
						break;
					}
				}
			}

			// Interpolate
			double diff = x - c.x1;

			Point @out;

			// y = a*x^3 + b*x^2 + c*x + d
			@out.x = ((c.a * diff + c.b) * diff + c.c) * diff + c.d;

			// dy = 3*a*x^2 + 2*b*x + c
			@out.y = (3.0 * c.a * diff + 2.0 * c.b) * diff + c.c;

			return @out;
		}
	}
}
