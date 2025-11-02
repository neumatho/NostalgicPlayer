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
	public class Exp2 : TestMathBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Special_Values()
		{
			c_double r = CMath.exp2(0.0);
			Assert.AreEqual(1.0, r);

			r = CMath.exp2(-0.0);
			Assert.AreEqual(1.0, r);

			r = CMath.exp2(c_double.NaN);
			Assert.IsTrue(c_double.IsNaN(r));

			r = CMath.exp2(c_double.PositiveInfinity);
			Assert.IsTrue(c_double.IsPositiveInfinity(r));

			r = CMath.exp2(c_double.NegativeInfinity);
			Assert.AreEqual(0.0, r);
			Assert.IsTrue(c_double.IsPositive(r));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Overflow_Boundary()
		{
			c_double r = CMath.exp2(1024.0);
			Assert.IsTrue(c_double.IsPositiveInfinity(r));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Top_Finite_1023()
		{
			c_double two_pow_1023 = 1.0;

			for (c_int i = 0; i < 1023; i++)
				two_pow_1023 += two_pow_1023;

			c_double r = CMath.exp2(1023.0);
			Assert.IsTrue(Nearly_Equal_Ulps(r, two_pow_1023, 1));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Underflow_To_Zero()
		{
			c_double r = CMath.exp2(-1075.0);
			Assert.IsTrue(c_double.IsPositive(r));
			Assert.AreEqual(0.0, r);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_True_Min_And_Min_Normal()
		{
			c_double tMin = BitConverter.UInt64BitsToDouble(1UL);
			c_double dMin = BitConverter.UInt64BitsToDouble(0x0010000000000000UL);

			c_double r = CMath.exp2(-1074.0);
			Assert.IsTrue(Nearly_Equal_Ulps(r, tMin, 0));

			r = CMath.exp2(-1022.0);
			Assert.IsTrue(Nearly_Equal_Ulps(r, dMin, 0));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Integer_Exponents_Exactness()
		{
			for (c_int e = -100; e <= 100; e++)
			{
				c_double @ref = 1.0;

				if (e >= 0)
				{
					for (c_int i = 0; i < e; i++)
						@ref += @ref;
				}
				else
				{
					for (c_int i = 0; i < -e; i++)
						@ref *= 0.5;
				}

				c_double r = CMath.exp2(e);
				Assert.IsTrue(Nearly_Equal_Ulps(r, @ref, 1));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Functional_Equation_Shift()
		{
			c_double[] samples =
			[
				-800.0, -300.0, -150.0, -20.0, -10.0, -1.0, -0.75, -0.5, -0.25,
				0.0, 0.25, 0.5, 0.75, 1.0, 5.0, 10.0, 20.0, 100.0
			];

			c_int n = samples.Length;

			for (c_int i = 0; i < n; i++)
			{
				c_double x = samples[i];
				c_double lhs = CMath.exp2(x + 1.0);
				c_double rhs = CMath.exp2(x) + CMath.exp2(x);

				Assert.IsTrue(Nearly_Equal_Ulps(lhs, rhs, 4) || (Ulp_Diff(lhs, rhs) <= 8));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Additivity_Subset()
		{
			c_double[] a = [ -3.5, -1.25, -0.5, 0.0, 0.5, 1.25, 3.5 ];
			c_double[] b = [ -2.0, -0.75, 0.25, 0.0, 0.75, 2.0, 4.0 ];

			for (c_int i = 0; i < a.Length; i++)
			{
				c_double _a = a[i];
				c_double _b = b[i];

				c_double r_ab = CMath.exp2(_a + _b);
				c_double prod = CMath.exp2(_a) * CMath.exp2(_b);

				Assert.IsTrue(Nearly_Equal_Ulps(r_ab, prod, 8));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Monotonicity_Dense_Scan()
		{
			c_double prev = CMath.exp2(-4.0);

			for (c_int i = 1; i <= 160; i++)
			{
				c_double x = -4.0 + (i * 0.05);
				c_double cur = CMath.exp2(x);

				Assert.IsTrue((cur >= prev) || c_double.IsNaN(cur));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fractional_Compositions()
		{
			c_double e_Half = CMath.exp2(0.5);
			c_double e_Quarter = CMath.exp2(0.25);
			c_double e_Three_Quarters = CMath.exp2(0.75);

			c_double two_From_Half = e_Half * e_Half;
			c_double two_From_Quarter = e_Quarter * e_Quarter;
			two_From_Quarter = two_From_Quarter * two_From_Quarter;

			Assert.IsTrue(Nearly_Equal_Ulps(two_From_Half, 2.0, 32));
			Assert.IsTrue(Nearly_Equal_Ulps(two_From_Quarter, 2.0, 32));

			c_double combo = e_Half * e_Quarter;
			Assert.IsTrue(Nearly_Equal_Ulps(e_Three_Quarters, combo, 8));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Very_Negative_Is_PosZero()
		{
			c_double r = CMath.exp2(-2000.0);
			Assert.IsTrue(c_double.IsPositive(r));
			Assert.AreEqual(0.0, r);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Random_Functional_Identity()
		{
			uint64_t state = 0x12345678ABCDEF00UL;

			for (c_int i = 0; i < 5000; i++)
			{
				state = (6364136223846793005UL * state) + 1;
				c_double x = ((int32_t)(state >> 32)) / 2147483648.0;
				x *= 20.0;

				c_double lhs = CMath.exp2(x + 1.0);
				c_double rhs = CMath.exp2(x) + CMath.exp2(x);

				Assert.IsTrue(Nearly_Equal_Ulps(lhs, rhs, 4));
			}
		}
	}
}
