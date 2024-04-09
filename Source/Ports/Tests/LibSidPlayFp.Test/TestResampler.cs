/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using NUnit.Framework;
using Polycode.NostalgicPlayer.Ports.ReSidFp.Resample;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibSidPlayFp.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestFixture]
	public class TestResampler
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[Test]
		public void TestSoftClip()
		{
			Assert.That(Resampler.SoftClipImpl(0) == 0);
			Assert.That(Resampler.SoftClipImpl(28000) == 28000);
			Assert.That(Resampler.SoftClipImpl(int.MaxValue) <= 32767);
			Assert.That(Resampler.SoftClipImpl(-28000) == -28000);
			Assert.That(Resampler.SoftClipImpl(int.MinValue + 1) >= -32768);
		}
	}
}
