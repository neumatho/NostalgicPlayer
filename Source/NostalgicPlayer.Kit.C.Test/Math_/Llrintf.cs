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
	public class Llrintf : TestMathBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Exact_Integers()
		{
			Assert.AreEqual(0, CMath.llrintf(0.0f));
			Assert.AreEqual(1, CMath.llrintf(1.0f));
			Assert.AreEqual(-1, CMath.llrintf(-1.0f));
			Assert.AreEqual(42, CMath.llrintf(42.0f));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Halfway_Cases()
		{
			Assert.AreEqual(0, CMath.llrintf(0.5f));
			Assert.AreEqual(2, CMath.llrintf(1.5f));
			Assert.AreEqual(2, CMath.llrintf(2.5f));
			Assert.AreEqual(4, CMath.llrintf(3.5f));
			Assert.AreEqual(0, CMath.llrintf(-0.5f));
			Assert.AreEqual(-2, CMath.llrintf(-1.5f));
			Assert.AreEqual(-2, CMath.llrintf(-2.5f));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fractional_Values()
		{
			Assert.AreEqual(0, CMath.llrintf(0.1f));
			Assert.AreEqual(1, CMath.llrintf(0.9f));
			Assert.AreEqual(-1, CMath.llrintf(-0.9f));
			Assert.AreEqual(1234, CMath.llrintf(1234.4f));
			Assert.AreEqual(1235, CMath.llrintf(1234.6f));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Large_Values()
		{
			Assert.AreEqual(1000000000, CMath.llrintf(1000000000.0f));
			Assert.AreEqual(-1000000000, CMath.llrintf(-1000000000.0f));

			c_float big = 1.0e20f;
			c_long_long r = CMath.llrintf(big);
			Assert.IsFalse(c_float.IsNaN(r));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Special_Values()
		{
			Assert.AreEqual(0, CMath.llrintf(c_float.PositiveInfinity));
			Assert.AreEqual(0, CMath.llrintf(c_float.NegativeInfinity));
			Assert.AreEqual(0, CMath.llrintf(c_float.NaN));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Roundtrip_Identity()
		{
			c_float[] vals = [ -3.7f, -2.3f, -1.6f, -0.5f, -0.49f, 0.49f, 0.5f, 1.4f, 1.5f, 2.6f, 10.9f, -9.9f ];

			for (c_int i = 0; i < vals.Length; i++)
			{
				c_float x = vals[i];
				c_long_long r = CMath.llrintf(x);
				c_float diff = Math.Abs(x - r);

				Assert.IsLessThanOrEqualTo(0.5f, diff);
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
				c_float x = ((int32_t)(s >> 32)) / 2147483648.0f;
				x *= 1e6f;

				c_long_long r = CMath.llrintf(x);
				c_float diff = Math.Abs(x - r);

				Assert.IsLessThanOrEqualTo(0.5f, diff);
			}
		}
	}
}
