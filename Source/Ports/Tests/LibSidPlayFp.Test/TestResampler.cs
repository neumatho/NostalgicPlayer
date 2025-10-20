/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibReSidFp.Resample;

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
			Assert.AreEqual(0, Resampler.SoftClipImpl(0));
			Assert.AreEqual(28000, Resampler.SoftClipImpl(28000));
			Assert.IsLessThanOrEqualTo(32767, Resampler.SoftClipImpl(int.MaxValue));
			Assert.AreEqual(-28000, Resampler.SoftClipImpl(-28000));
			Assert.IsGreaterThanOrEqualTo(-32768, Resampler.SoftClipImpl(int.MinValue + 1));
		}
	}
}
