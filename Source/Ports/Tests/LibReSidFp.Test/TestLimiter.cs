/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibReSidFp.Resample;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibReSidFp.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class TestLimiter
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestSoftClip()
		{
			// We assume values stay below this peak
			const int peak = Limiter.threshold + (Limiter.threshold / 2);

			// Values within threshold should pass unchanged
			for (int i = -Limiter.threshold; i <= Limiter.threshold; i++)
				Assert.AreEqual(i, Limiter.SoftClipImpl(i));

			// Values above threshold should be compressed
			for (int i = Limiter.threshold; i <= peak; i++)
			{
				int x = Limiter.SoftClipImpl(i);
				Assert.IsLessThanOrEqualTo(i, x);
				Assert.IsLessThanOrEqualTo(short.MaxValue, x);
			}

			for (int i = -Limiter.threshold; i <= -peak; i--)
			{
				int x = Limiter.SoftClipImpl(i);
				Assert.IsGreaterThanOrEqualTo(i, x);
				Assert.IsGreaterThanOrEqualTo(short.MinValue, x);
			}

			// Check the extremes too
			Assert.IsLessThanOrEqualTo(short.MaxValue, Limiter.SoftClipImpl(int.MaxValue));
			Assert.IsGreaterThanOrEqualTo(short.MinValue, Limiter.SoftClipImpl(int.MinValue + 1));
		}
	}
}
