/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp
{
	/// <summary>
	/// Opamp input -> output voltage conversion
	/// </summary>
	internal class OpAmp
	{
		// Find output voltage in inverting gain and inverting summer SID op-amp
		// circuits, using a combination of Newton-Raphson and bisection.
		//
		//               +---R2--+
		//               |       |
		//     vi ---R1--o--[A>--o-- vo
		//               vx
		//
		// From Kirchoff's current law it follows that
		//
		//     IR1f + IR2r = 0
		//
		// Substituting the triode mode transistor model K*W/L*(Vgst^2 - Vgdt^2)
		// for the currents, we get:
		//
		//     n*((Vddt - vx)^2 - (Vddt - vi)^2) + (Vddt - vx)^2 - (Vddt - vo)^2 = 0
		//
		// Our root function f can thus be written as:
		//
		//     f = (n + 1)*(Vddt - vx)^2 - n*(Vddt - vi)^2 - (Vddt - vo)^2 = 0
		//
		// Using substitution constants
		//
		//     a = n + 1
		//     b = Vddt
		//     c = n*(Vddt - vi)^2
		//
		// the equations for the root function and its derivative can be written as:
		//
		//     f = a*(b - vx)^2 - c - (b - vo)^2
		//     df = 2*((b - vo)*dvo - a*(b - vx))

		private const double EPSILON = 1e-8;

		/// <summary>
		/// Current root position
		/// </summary>
		private double x;

		private readonly double vddt;
		private readonly double vMin;
		private readonly double vMax;

		private readonly Spline opamp;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OpAmp(List<Spline.Point> opamp, double vddt)
		{
			x = 0.0;
			this.vddt = vddt;
			vMin = opamp.First().x;
			vMax = opamp.Last().x;
			this.opamp = new Spline(opamp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			x = vMin;
		}



		/********************************************************************/
		/// <summary>
		/// Solve the opamp equation for input vi in loading context n
		/// </summary>
		/********************************************************************/
		public double Solve(double n, double vi)
		{
			// Start off with an estimate of x and a root bracket [ak, bk].
			// f is decreasing, so that f(ak) > 0 and f(bk) < 0
			double ak = vMin;
			double bk = vMax;

			double a = n + 1.0;
			double b = vddt;
			double b_vi = b > vi ? b - vi : 0.0;
			double c = n * (b_vi * b_vi);

			for (;;)
			{
				double xk = x;

				// Calculate f and df
				Spline.Point @out = opamp.Evaluate(x);
				double vo = @out.x;
				double dvo = @out.y;

				double b_vx = b > x ? b - x : 0.0;
				double b_vo = b > vo ? b - vo : 0.0;

				// f = a*(b - vx)^2 - c - (b - vo)^2
				double f = a * (b_vx * b_vx) - c - (b_vo * b_vo);

				// df = 2*((b - vo)*dvo - a*(b - vx))
				double df = 2.0 * (b_vo * dvo - a * b_vx);

				// Newton-Raphson step: xk1 = xk - f(xk)/f'(xk)
				x -= f / df;

				if (Math.Abs(x - xk) < EPSILON)
				{
					@out = opamp.Evaluate(x);
					return @out.x;
				}

				// Narrow down root bracket
				if (f < 0.0)
					bk = xk;
				else
					ak = xk;

				if ((x <= ak) || (x >= bk))
				{
					// Bisection step (ala Dekker's method)
					x = (ak + bk) * 0.5;
				}
			}
		}
	}
}
