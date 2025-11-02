/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace NostalgicPlayer.Kit.C.Test.Math_
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class Hypot : TestMathBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Basic_Pythagoras()
		{
			c_double r = CMath.hypot(3.0, 4.0);
			Assert.IsTrue((r > 4.999999) && (r < 5.000001));

			r = CMath.hypot(-3.0, -4.0);
			Assert.IsTrue((r > 4.999999) && (r < 5.000001));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Symmetry()
		{
			c_double[] xs = [ 1.0, 2.0, -3.0, 0.0 ];
			c_double[] ys = [ 4.0, -5.0, 6.0, 0.0 ];

			for (c_int i = 0; i < 4; i++)
			{
				c_double a = CMath.hypot(xs[i], ys[i]);
				c_double b = CMath.hypot(ys[i], xs[i]);

				Assert.IsLessThan(1e-12, Math.Abs(a - b));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Zero_And_NaN_Inf()
		{
			c_double r = CMath.hypot(0.0, 0.0);
			Assert.AreEqual(0.0, r);

			r = CMath.hypot(c_double.PositiveInfinity, 5.0);
			Assert.IsTrue(c_double.IsPositiveInfinity(r));

			r = CMath.hypot(c_double.NegativeInfinity, 5.0);
			Assert.IsTrue(c_double.IsPositiveInfinity(r));

			r = CMath.hypot(5.0, c_double.PositiveInfinity);
			Assert.IsTrue(c_double.IsPositiveInfinity(r));

			r = CMath.hypot(c_double.NaN, 2.0);
			Assert.IsTrue(c_double.IsNaN(r));

			r = CMath.hypot(2.0, c_double.NaN);
			Assert.IsTrue(c_double.IsNaN(r));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Large_Scale_Consistency()
		{
			c_double h_Small = CMath.hypot(1.0, 1.0);
			c_double h_Large = CMath.hypot(1e300, 1e300);
			c_double scaled = 1e300 * h_Small;

			c_double denon = Math.Abs(h_Large) + 1e-300;
			c_double relErr = Math.Abs(h_Large - scaled) / denon;
			Assert.IsLessThan(1e-12, relErr);

			c_double h_Mix = CMath.hypot(1e300, 1e-300);
			relErr = Math.Abs(h_Mix - 1e300) / 1e300;
			Assert.IsLessThan(1e-15, relErr);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Underflow_Range()
		{
			c_double tiny = BitConverter.UInt64BitsToDouble(1UL);

			c_double r = CMath.hypot(tiny, tiny);
			Assert.IsTrue((r > 0.0) && !c_double.IsInfinity(r));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Triangle_Inequality()
		{
			c_double[][] pairs =
			[
				[ 3.0, 4.0 ],
				[ -3.0, 4.0 ],
				[ 5.0, 0.1 ],
				[ 1e-10, 1e10 ]
			];

			for (c_int i = 0; i < 4; i++)
			{
				c_double x = pairs[i][0];
				c_double y = pairs[i][1];

				c_double r = CMath.hypot(x, y);
				Assert.IsTrue((r >= Math.Abs(x)) && (r >= Math.Abs(y)));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Scaling_Property()
		{
			c_double[] xs = [ 1.0, -2.0, 3.5 ];
			c_double[] ys = [ 4.0, 5.0, -6.5 ];
			c_double[] ks = [ 2.0, 3.0, 1e-5 ];

			for (c_int i = 0; i < 3; i++)
			{
				c_double x = xs[i];
				c_double y = ys[i];
				c_double k = ks[i];

				c_double h1 = CMath.hypot(k * x, k * y);
				c_double h2 = Math.Abs(k) * CMath.hypot(x, y);
				c_double relErr = Math.Abs(h1 - h2) / (Math.Abs(h2) + 1e-300);
				Assert.IsLessThan(1e-12, relErr);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Random_Consistency()
		{
			uint64_t s = 0x12345678abcdef00UL;

			for (c_int i = 0; i < 1000; i++)
			{
				s = s * 6364136223846793005UL + 1;
				c_double x = ((int32_t)(s >> 32)) / 2147483648.0;

				s = s * 6364136223846793005UL + 1;
				c_double y = ((int32_t)(s >> 32)) / 2147483648.0;

				x *= 1e3;
				y *= 1e3;

				c_double h = CMath.hypot(x, y);

				c_double diff = (h * h) - ((x * x) + (y * y));
				c_double rel = Math.Abs(diff) / ((x * x) + (y * y) + 1e-300);
				Assert.IsLessThan(1e-12, rel);
			}
		}
	}
}
