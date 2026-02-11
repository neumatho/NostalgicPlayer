/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpus.Test.Celt
{
	/// <summary>
	/// </summary>
	[TestClass]
	public class Test_Unit_MathOps
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestDiv()
		{
			for (opus_int32 i = 1; i <= 327670; i++)
			{
				opus_val32 val = MathOps.Celt_Rcp(i);
				c_double prod = val * i;

				if (Math.Abs(prod - 1) > 0.00025)
					Assert.Fail($"div failed: 1/{i}={val} (product = {prod})");
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestSqrt()
		{
			for (opus_int32 i = 1; i <= 1000000000; i++)
			{
				opus_val16 val = MathOps.Celt_Sqrt(i);
				c_double ratio = val / Math.Sqrt(i);

				if ((Math.Abs(ratio - 1) > 0.0005) && (Math.Abs(val - Math.Sqrt(i)) > 2))
					Assert.Fail($"sqrt failed: sqrt({i})={val} (ratio = {ratio})");

				i += i >> 10;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestBitexactCos()
		{
			opus_int32 chk = 0, max_d = 0;
			opus_int32 last = 32767, min_d = 32767;

			for (c_int i = 64; i <= 16320; i++)
			{
				opus_int32 q = Bands.Bitexact_Cos((opus_int16)i);
				chk ^= q * i;

				opus_int32 d = last - q;

				if (d > max_d)
					max_d = d;

				if (d < min_d)
					min_d = d;

				last = q;
			}

			if ((chk != 89408644) || (max_d != 5) || (min_d != 0) || (Bands.Bitexact_Cos(64) != 32767) || (Bands.Bitexact_Cos(16320) != 200) || (Bands.Bitexact_Cos(8192) != 23171))
				Assert.Fail("Bitexact_Cos failed");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestBitexactLog2Tan()
		{
			bool fail = false;
			opus_int32 chk = 0, max_d = 0;
			opus_int32 last = 15059, min_d = 15059;

			for (c_int i = 64; i < 8193; i++)
			{
				opus_int32 mid = Bands.Bitexact_Cos((opus_int16)i);
				opus_int32 side = Bands.Bitexact_Cos((opus_int16)(16384 - i));
				opus_int32 q = Bands.Bitexact_Log2Tan(mid, side);
				chk ^= q * i;

				opus_int32 d = last - q;

				if (q != (-1 * Bands.Bitexact_Log2Tan(side, mid)))
					fail = true;

				if (d > max_d)
					max_d = d;

				if (d < min_d)
					min_d = d;

				last = q;
			}

			if ((chk != 15821257) || (max_d != 61) || (min_d != -2) || fail || (Bands.Bitexact_Log2Tan(32767, 200) != 15059) || (Bands.Bitexact_Log2Tan(30274, 12540) != 2611) || (Bands.Bitexact_Log2Tan(23171, 23171) != 0))
				Assert.Fail("Bitexact_Log2Tan failed");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestLog2()
		{
			for (c_float x = 0.001f; x < 1677700.0f; x += (x / 8.0f))
			{
				c_float error = (c_float)Math.Abs((1.442695040888963387 * Math.Log(x)) - MathOps.Celt_Log2(x));
				if (error > 0.0009)
					Assert.Fail($"celt_log2 failed: x = {x}, error = {error}");
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestExp2()
		{
			for (c_float x = -11.0f; x < 24.0f; x += 0.0007f)
			{
				c_float error = (c_float)Math.Abs(x - (1.442695040888963387 * Math.Log(MathOps.Celt_Exp2(x))));
				if (error > 0.0002)
					Assert.Fail($"celt_exp2 failed: x = {x}, error = {error}");
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestExp2Log2()
		{
			for (c_float x = -11.0f; x < 24.0f; x += 0.0007f)
			{
				c_float error = Math.Abs(x - (MathOps.Celt_Log2(MathOps.Celt_Exp2(x))));
				if (error > 0.001)
					Assert.Fail($"celt_log2/celt_exp2 failed: x = {x}, error = {error}");
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Atan2()
		{
			c_float error_threshold = 1.5e-07f;
			c_float max_error = 0;

			for (c_float x = 0.0f; x < 1.0f; x += 0.007f)
			{
				for (c_float y = 0.0f; y < 1.0f; y += 0.07f)
				{
					if ((x == 0) && (y == 0))
					{
						// atan2(0,0) is undefined behavior
						continue;
					}

					c_float error = (c_float)CMath.fabs((0.636619772367581f * CMath.atan2(y, x)) - MathOps.Celt_Atanp_Norm(y, x));
					if (max_error < error)
						max_error = error;

					if (error > error_threshold)
						Assert.Fail($"celt_atan2p_norm failed: (fabs)(2/pi*atan2(y,x) - celt_atan2p_norm(y,x))>{error_threshold} (x = {x}, y = {y}, error = {error})");
				}
			}
		}
	}
}
