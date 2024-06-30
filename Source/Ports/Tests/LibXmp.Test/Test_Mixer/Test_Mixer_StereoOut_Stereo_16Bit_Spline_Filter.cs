/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Mixer
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Mixer
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Mixer_StereoOut_Stereo_16Bit_Spline_Filter()
		{
			Compare_Mixer_Samples(dataDirectory, "Mixer_StereoOut_Stereo_16Bit_Spline_Filter.data", "Test.it", 22050, Xmp_Format.Default, Xmp_Interp.Spline, Test_Xm_Sample_16Bit_Stereo, true);
		}
	}
}
