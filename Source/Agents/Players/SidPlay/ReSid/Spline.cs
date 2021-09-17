/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSid
{
	/// <summary>
	/// Interpolation helper methods
	/// </summary>
	internal static class Spline
	{
		// Our objective is to construct a smooth interpolating single-valued function
		// y = f(x).
		//
		// Catmull-Rom splines are widely used for interpolation, however these are
		// parametric curves [x(t) y(t) ...] and can not be used to directly calculate
		// y = f(x).
		// For a discussion of Catmull-Rom splines see Catmull, E., and R. Rom,
		// "A Class of Local Interpolating Splines", Computer Aided Geometric Design.
		//
		// Natural cubic splines are single-valued functions, and have been used in
		// several applications e.g. to specify gamma curves for image display.
		// These splines do not afford local control, and a set of linear equations
		// including all interpolation points must be solved before any point on the
		// curve can be calculated. The lack of local control makes the splines
		// more difficult to handle than e.g. Catmull-Rom splines, and real-time
		// interpolation of a stream of data points is not possible.
		// For a discussion of natural cubic splines, see e.g. Kreyszig, E., "Advanced
		// Engineering Mathematics".
		//
		// Our approach is to approximate the properties of Catmull-Rom splines for
		// piecewice cubic polynomials f(x) = ax^3 + bx^2 + cx + d as follows:
		// Each curve segment is specified by four interpolation points,
		// p0, p1, p2, p3.
		// The curve between p1 and p2 must interpolate both p1 and p2, and in addition
		//   f'(p1.x) = k1 = (p2.y - p0.y)/(p2.x - p0.x) and
		//   f'(p2.x) = k2 = (p3.y - p1.y)/(p3.x - p1.x).
		//
		// The constraints are expressed by the following system of linear equations
		//
		//   [ 1  xi    xi^2    xi^3 ]   [ d ]    [ yi ]
		//   [     1  2*xi    3*xi^2 ] * [ c ] =  [ ki ]
		//   [ 1  xj    xj^2    xj^3 ]   [ b ]    [ yj ]
		//   [     1  2*xj    3*xj^2 ]   [ a ]    [ kj ]
		//
		// Solving using Gaussian elimination and back substitution, setting
		// dy = yj - yi, dx = xj - xi, we get
		// 
		//   a = ((ki + kj) - 2*dy/dx)/(dx*dx);
		//   b = ((kj - ki)/dx - 3*(xi + xj)*a)/2;
		//   c = ki - (3*xi*a + 2*b)*xi;
		//   d = yi - ((xi*a + b)*xi + c)*xi;
		//
		// Having calculated the coefficients of the cubic polynomial we have the
		// choice of evaluation by brute force
		//
		//   for (x = x1; x <= x2; x += res) {
		//     y = ((a*x + b)*x + c)*x + d;
		//     plot(x, y);
		//   }
		//
		// or by forward differencing
		//
		//   y = ((a*x1 + b)*x1 + c)*x1 + d;
		//   dy = (3*a*(x1 + res) + 2*b)*x1*res + ((a*res + b)*res + c)*res;
		//   d2y = (6*a*(x1 + res) + 2*b)*res*res;
		//   d3y = 6*a*res*res*res;
		//     
		//   for (x = x1; x <= x2; x += res) {
		//     plot(x, y);
		//     y += dy; dy += d2y; d2y += d3y;
		//   }
		//
		// See Foley, Van Dam, Feiner, Hughes, "Computer Graphics, Principles and
		// Practice" for a discussion of forward differencing.
		//
		// If we have a set of interpolation points p0, ..., pn, we may specify
		// curve segments between p0 and p1, and between pn-1 and pn by using the
		// following constraints:
		//   f''(p0.x) = 0 and
		//   f''(pn.x) = 0.
		//
		// Substituting the results for a and b in
		//
		//   2*b + 6*a*xi = 0
		//
		// we get
		//
		//   ki = (3*dy/dx - kj)/2;
		//
		// or by substituting the results for a and b in
		//
		//   2*b + 6*a*xj = 0
		//
		// we get
		//
		//   kj = (3*dy/dx - ki)/2;
		//
		// Finally, if we have only two interpolation points, the cubic polynomial
		// will degenerate to a straight line if we set
		//
		//   ki = kj = dy/dx;
		//

		#region FCPoint structure
		public struct FCPoint
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public FCPoint(int x, int y)
			{
				X = x;
				Y = y;
			}

			public int X;
			public int Y;
		}
		#endregion

		#region PointPlotter class
		public class PointPlotter
		{
			private readonly int[] f;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public PointPlotter(int[] arr)
			{
				f = arr;
			}



			/********************************************************************/
			/// <summary>
			/// Plot new point
			/// </summary>
			/********************************************************************/
			public void Plot(double x, double y)
			{
				// Clamp negative values to zero
				if (y < 0)
					y = 0;

				f[(int)x] = (int)y;
			}
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Evaluation of complete interpolating function.
		/// 
		/// Note that since each curve segment is controlled by four points,
		/// the end points will not be interpolated. If extra control points
		/// are not desirable, the end points can simply be repeated to
		/// ensure interpolation.
		/// 
		/// Note also that points of non-differentiability and discontinuity
		/// can be introduced by repeating points
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Interpolate(FCPoint[] p, int p0, int pn, PointPlotter plot, double res)
		{
			// Set up points for first curve segment
			int p1 = p0; ++p1;
			int p2 = p1; ++p2;
			int p3 = p2; ++p3;

			// Draw each curve segment
			for (; p2 != pn; ++p0, ++p1, ++p2, ++p3)
			{
				// p1 and p2 equal; single point
				if (p[p1].X == p[p2].X)
					continue;

				double k1, k2;

				// Both end points repeated; straight line
				if ((p[p0].X == p[p1].X) && (p[p2].X == p[p3].X))
					k1 = k2 = (p[p2].Y - p[p1].Y) / (double)(p[p2].X - p[p1].X);
				else
				{
					// p0 and p1 equal; use f''(x1) = 0
					if ((p[p0].X == p[p1].X))
					{
						k2 = (p[p3].Y - p[p1].Y) / (double)(p[p3].X - p[p1].X);
						k1 = (3 * (p[p2].Y - p[p1].Y) / (double)(p[p2].X - p[p1].X) - k2) / 2;
					}
					else
					{
						if (p[p2].X == p[p3].X)
						{
							// p2 and p3 equal; use f''(x2) = 0
							k1 = (p[p2].Y - p[p0].Y) / (double)(p[p2].X - p[p0].X);
							k2 = (3 * (p[p2].Y - p[p1].Y) / (double)(p[p2].X - p[p1].X) - k1) / 2;
						}
						else
						{
							// Normal curve
							k1 = (p[p2].Y - p[p0].Y) / (double)(p[p2].X - p[p0].X);
							k2 = (p[p3].Y - p[p1].Y) / (double)(p[p3].X - p[p1].X);
						}
					}
				}

				InterpolateForwardDifference(p[p1].X, p[p1].Y, p[p2].X, p[p2].Y, k1, k2, plot, res);
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Evaluation of cubic polynomial by forward differencing
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void InterpolateForwardDifference(double x1, double y1, double x2, double y2, double k1, double k2, PointPlotter plot, double res)
		{
			CubicCoefficients(x1, y1, x2, y2, k1, k2, out double a, out double b, out double c, out double d);

			double y = ((a * x1 + b) * x1 + c) * x1 + d;
			double dy = (3 * a * (x1 + res) + 2 * b) * x1 * res + ((a * res + b) * res + c) * res;
			double d2y = (6 * a * (x1 + res) + 2 * b) * res * res;
			double d3y = 6 * a * res * res * res;

			// Calculate each point
			for (double x = x1; x <= x2; x += res)
			{
				plot.Plot(x, y);
				y += dy;
				dy += d2y;
				d2y += d3y;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Calculation of coefficients
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void CubicCoefficients(double x1, double y1, double x2, double y2, double k1, double k2, out double a, out double b, out double c, out double d)
		{
			double dx = x2 - x1;
			double dy = y2 - y1;

			a = ((k1 + k2) - 2 * dy / dx) / (dx * dx);
			b = ((k2 - k1) / dx - 3 * (x1 + x2) * a) / 2;
			c = k1 - (3 * x1 * a + 2 * b) * x1;
			d = y1 - ((x1 * a + b) * x1 + c) * x1;
		}
		#endregion
	}
}
