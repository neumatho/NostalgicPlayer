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
	public class Lrint : TestMathBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Exact_Integers()
		{
			Assert.AreEqual(0, CMath.lrint(0.0));
			Assert.AreEqual(1, CMath.lrint(1.0));
			Assert.AreEqual(-1, CMath.lrint(-1.0));
			Assert.AreEqual(42, CMath.lrint(42.0));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Halfway_Cases()
		{
			Assert.AreEqual(0, CMath.lrint(0.5));
			Assert.AreEqual(2, CMath.lrint(1.5));
			Assert.AreEqual(2, CMath.lrint(2.5));
			Assert.AreEqual(4, CMath.lrint(3.5));
			Assert.AreEqual(0, CMath.lrint(-0.5));
			Assert.AreEqual(-2, CMath.lrint(-1.5));
			Assert.AreEqual(-2, CMath.lrint(-2.5));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fractional_Values()
		{
			Assert.AreEqual(0, CMath.lrint(0.1));
			Assert.AreEqual(1, CMath.lrint(0.9));
			Assert.AreEqual(-1, CMath.lrint(-0.9));
			Assert.AreEqual(1234, CMath.lrint(1234.4));
			Assert.AreEqual(1235, CMath.lrint(1234.6));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Large_Values()
		{
			Assert.AreEqual(2147483647, CMath.lrint(2147483647.0));
			Assert.AreEqual(-2147483648, CMath.lrint(-2147483648.0));
			Assert.AreEqual(1000000, CMath.lrint(1000000.0));
			Assert.AreEqual(-1000000, CMath.lrint(-1000000.0));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Special_Values()
		{
			Assert.AreEqual(0, CMath.lrint(c_double.PositiveInfinity));
			Assert.AreEqual(0, CMath.lrint(c_double.NegativeInfinity));
			Assert.AreEqual(0, CMath.lrint(c_double.NaN));
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
				c_int r = CMath.lrint(x);
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
				c_double x = ((int32_t)(s >> 32)) / 2147483648.0f;
				x *= 1e6f;

				c_int r = CMath.lrint(x);
				c_double diff = Math.Abs(x - r);

				Assert.IsLessThanOrEqualTo(0.5, diff);
			}
		}
	}
}
