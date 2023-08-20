/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using NUnit.Framework;
using Polycode.NostalgicPlayer.Ports.ReSidFp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibSidPlayFp.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestFixture]
	public class TestSpline
	{
		private const int OpAmpSize = 33;

		private static readonly Spline.Point[] opamp_voltage =
		{
			new ( 0.81, 10.31),		// Approximate start of actual range
			new ( 2.40, 10.31),
			new ( 2.60, 10.30),
			new ( 2.70, 10.29),
			new ( 2.80, 10.26),
			new ( 2.90, 10.17),
			new ( 3.00, 10.04),
			new ( 3.10,  9.83),
			new ( 3.20,  9.58),
			new ( 3.30,  9.32),
			new ( 3.50,  8.69),
			new ( 3.70,  8.00),
			new ( 4.00,  6.89),
			new ( 4.40,  5.21),
			new ( 4.54,  4.54),		// Working point (vi = vo)
			new ( 4.60,  4.19),
			new ( 4.80,  3.00),
			new ( 4.90,  2.30),		// Change of curvature
			new ( 4.95,  2.03),
			new ( 5.00,  1.88),
			new ( 5.05,  1.77),
			new ( 5.10,  1.69),
			new ( 5.20,  1.58),
			new ( 5.40,  1.44),
			new ( 5.60,  1.33),
			new ( 5.80,  1.26),
			new ( 6.00,  1.21),
			new ( 6.40,  1.12),
			new ( 7.00,  1.02),
			new ( 7.50,  0.97),
			new ( 8.50,  0.89),
			new ( 10.00,  0.81),
			new ( 10.31,  0.81)		// Approximate end of actual range
		};

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[Test]
		public void TestMonotonicity()
		{
			Spline s = new Spline(new List<Spline.Point>(opamp_voltage));

			double old = double.MaxValue;
			for (double x = 0.0; x < 12.0; x += 0.01)
			{
				Spline.Point o = s.Evaluate(x);
				Assert.IsTrue(o.x <= old);

				old = o.x;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[Test]
		public void TestPoints()
		{
			Spline s = new Spline(new List<Spline.Point>(opamp_voltage));

			for (int i = 0; i < OpAmpSize; i++)
			{
				Spline.Point o = s.Evaluate(opamp_voltage[i].x);
				Assert.AreEqual(opamp_voltage[i].y, o.x);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[Test]
		public void TestInterpolateOutsideBounds()
		{
			Spline.Point[] values =
			{
				new ( 10, 15 ),
				new ( 15, 20 ),
				new ( 20, 30 ),
				new ( 25, 40 ),
				new ( 30, 45 )
			};

			Spline s = new Spline(new List<Spline.Point>(opamp_voltage));

			Spline.Point o = s.Evaluate(5);
			CheckClose(6.66667, o.x, 0.00001);

			o = s.Evaluate(40);
			CheckClose(75.0, o.x, 0.00001);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool CheckClose(double expected, double actual, double tolerance)
		{
			return (actual >= (expected - tolerance)) && (actual <= (expected + tolerance));
		}
		#endregion
	}
}
