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
	public class Fmod : TestMathBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Basic_Remainder()
		{
			Assert.IsTrue((CMath.fmod(5.3, 2.0) > 1.29) && (CMath.fmod(5.3, 2.0) < 1.31));
			Assert.IsTrue((CMath.fmod(-5.3, 2.0) > -1.31) && (CMath.fmod(-5.3, 2.0) < -1.29));
			Assert.IsTrue((CMath.fmod(5.3, -2.0) > 1.29) && (CMath.fmod(5.3, -2.0) < 1.31));
			Assert.IsTrue((CMath.fmod(-5.3, -2.0) > -1.31) && (CMath.fmod(-5.3, -2.0) < -1.29));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Y_Divides_X_Exact()
		{
			Assert.AreEqual(0.0, CMath.fmod(4.0, 2.0));
			Assert.IsTrue(c_double.IsNegative(CMath.fmod(-4.0, 2.0)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_X_Smaller_Than_Y()
		{
			Assert.AreEqual(1.5, CMath.fmod(1.5, 2.0));
			Assert.AreEqual(-1.5, CMath.fmod(-1.5, 2.0));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Zero_Cases()
		{
			c_double r = CMath.fmod(0.0, 3.0);
			Assert.IsTrue(c_double.IsPositive(r));
			Assert.AreEqual(0.0, r);

			r = CMath.fmod(-0.0, 3.0);
			Assert.IsTrue(c_double.IsNegative(r));

			r = CMath.fmod(2.5, c_double.PositiveInfinity);
			Assert.AreEqual(2.5, r);

			r = CMath.fmod(-2.5, c_double.PositiveInfinity);
			Assert.AreEqual(-2.5, r);

			r = CMath.fmod(2.5, 0.0);
			Assert.IsTrue(c_double.IsNaN(r));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Inf_Cases()
		{
			c_double r = CMath.fmod(c_double.PositiveInfinity, 2.0);
			Assert.IsTrue(c_double.IsNaN(r));

			r = CMath.fmod(c_double.NegativeInfinity, 2.0);
			Assert.IsTrue(c_double.IsNaN(r));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Magnitude_Bound()
		{
			c_double[] xs = [ 5.3, -5.3, 1.25, -7.75, 0.0, -0.0 ];
			c_double[] ys = [ 2.0, -2.0, 1.0, 3.5, 4.0, 4.0 ];

			for (c_int i = 0; i < xs.Length; i++)
			{
				c_double x = xs[i];
				c_double y = ys[i];
				c_double r = CMath.fmod(x, y);

				// If y == 0 or NaN, it is handled in other tests
				if ((y != 0.0) && !c_double.IsNaN(y))
					Assert.IsLessThan(Math.Abs(y), Math.Abs(r));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Recomposition_Identity()
		{
			c_double[] xs = [ 5.3, -5.3, 1.25, -7.75, 8.0, -8.0, 1.5, -1.5 ];
			c_double[] ys = [ 2.0, -2.0, 1.0, 3.5, 2.0, 2.0, 4.0, 4.0 ];

			for (c_int i = 0; i < xs.Length; i++)
			{
				c_double x = xs[i];
				c_double y = ys[i];

				c_double qd = x / y;
				int64_t qi = (int64_t)qd;

				c_double r = CMath.fmod(x, y);
				c_double recombined = (qi * y) + r;

				c_double err = recombined - x;
				Assert.IsTrue((err < 1e-12) && (err > -1e-12));

				if (r == 0.0)
				{
					if (x == 0.0)
					{
					}
					else
						Assert.IsTrue(((x >= 0.0) && c_double.IsPositive(r)) || ((x <= 0.0) && c_double.IsNegative(x)));
				}
				else
					Assert.IsTrue(((r > 0.0) && (x > 0.0)) || ((r < 0.0) && (x < 0.0)));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Subnormal_Behavior()
		{
			c_double tiny = BitConverter.UInt64BitsToDouble(1UL);

			c_double r = CMath.fmod(tiny, 1.0);
			Assert.AreEqual(tiny, r);
		}
	}
}
