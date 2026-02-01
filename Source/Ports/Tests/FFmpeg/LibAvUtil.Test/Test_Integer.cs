/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.LibAvUtil.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class Test_Integer
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Integer_()
		{
			for (int64_t a = 7; a < 256 * 256 * 256; a += 13215)
			{
				for (int64_t b = 3 ; b < 256 * 256 * 256; b += 27118)
				{
					AvInteger ai = Integer.Av_Int2I(a);
					AvInteger bi = Integer.Av_Int2I(b);

					Assert.AreEqual(a, Integer.Av_I2Int(ai));
					Assert.AreEqual(b, Integer.Av_I2Int(bi));
					Assert.AreEqual(a + b, Integer.Av_I2Int(Integer.Av_Add_I(ai, bi)));
					Assert.AreEqual(a - b, Integer.Av_I2Int(Integer.Av_Sub_I(ai, bi)));
					Assert.AreEqual(a * b, Integer.Av_I2Int(Integer.Av_Mul_I(ai, bi)));
					Assert.AreEqual(a >> 9, Integer.Av_I2Int(Integer.Av_Shr_I(ai, 9)));
					Assert.AreEqual(a << 9, Integer.Av_I2Int(Integer.Av_Shr_I(ai, -9)));
					Assert.AreEqual(a >> 17, Integer.Av_I2Int(Integer.Av_Shr_I(ai, 17)));
					Assert.AreEqual(a << 17, Integer.Av_I2Int(Integer.Av_Shr_I(ai, -17)));
					Assert.AreEqual(IntMath.Av_Log2((c_uint)a), Integer.Av_Log2_I(ai));
					Assert.AreEqual(a / b, Integer.Av_I2Int(Integer.Av_Div_I(ai, bi)));
				}
			}
		}
	}
}
