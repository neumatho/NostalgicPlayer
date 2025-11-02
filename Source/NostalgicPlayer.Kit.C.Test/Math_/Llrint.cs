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
	public class Llrint : TestMathBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Exact_Integers()
		{
			Assert.AreEqual(0, CMath.llrint(0.0));
			Assert.AreEqual(1, CMath.llrint(1.0));
			Assert.AreEqual(-1, CMath.llrint(-1.0));
			Assert.AreEqual(42, CMath.llrint(42.0));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Halfway_Cases()
		{
			Assert.AreEqual(0, CMath.llrint(0.5));
			Assert.AreEqual(2, CMath.llrint(1.5));
			Assert.AreEqual(2, CMath.llrint(2.5));
			Assert.AreEqual(4, CMath.llrint(3.5));
			Assert.AreEqual(0, CMath.llrint(-0.5));
			Assert.AreEqual(-2, CMath.llrint(-1.5));
			Assert.AreEqual(-2, CMath.llrint(-2.5));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fractional_Values()
		{
			Assert.AreEqual(0, CMath.llrint(0.1));
			Assert.AreEqual(1, CMath.llrint(0.9));
			Assert.AreEqual(-1, CMath.llrint(-0.9));
			Assert.AreEqual(1234, CMath.llrint(1234.4));
			Assert.AreEqual(1235, CMath.llrint(1234.6));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Large_Values()
		{
			Assert.AreEqual(9000000000000000000L, CMath.llrint(9e18));
			Assert.AreEqual(-9000000000000000000L, CMath.llrint(-9e18));

			c_double big = 1.0e20;
			c_long_long r = CMath.llrint(big);
			Assert.IsFalse(c_double.IsNaN(r));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Special_Values()
		{
			Assert.AreEqual(0, CMath.llrint(c_double.PositiveInfinity));
			Assert.AreEqual(0, CMath.llrint(c_double.NegativeInfinity));
			Assert.AreEqual(0, CMath.llrint(c_double.NaN));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Roundtrip_Identity()
		{
			c_double[] vals = [ -3.7, -2.3, -1.6, -0.5, -0.49, 0.49, 0.5, 1.4, 1.5, 2.6, 10.9, -9.9 ];

			for (c_int i = 0; i < vals.Length; i++)
			{
				c_double x = vals[i];
				c_long_long r = CMath.llrint(x);
				c_double diff = Math.Abs(x - r);

				Assert.IsLessThanOrEqualTo(0.5, diff);
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

			for (c_int i = 0; i < 5000; i++)
			{
				s = s * 6364136223846793005UL + 1;
				c_double x = ((int32_t)(s >> 32)) / 2147483648.0;
				x *= 1e6;

				c_long_long r = CMath.llrint(x);
				c_double diff = Math.Abs(x - r);

				Assert.IsLessThanOrEqualTo(0.5, diff);
			}
		}
	}
}
