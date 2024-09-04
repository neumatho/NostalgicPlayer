/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.ReSidFp.Resample;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibSidPlayFp.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class TestResampler
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void TestSoftClip()
		{
			Assert.IsTrue(Resampler.SoftClipImpl(0) == 0);
			Assert.IsTrue(Resampler.SoftClipImpl(28000) == 28000);
			Assert.IsTrue(Resampler.SoftClipImpl(int.MaxValue) <= 32767);
			Assert.IsTrue(Resampler.SoftClipImpl(-28000) == -28000);
			Assert.IsTrue(Resampler.SoftClipImpl(int.MinValue + 1) >= -32768);
		}
	}
}
