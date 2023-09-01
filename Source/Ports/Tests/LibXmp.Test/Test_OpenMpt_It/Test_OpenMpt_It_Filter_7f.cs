/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_It
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_It
	{
		/********************************************************************/
		/// <summary>
		/// A small test case that demonstrates that full cutoff should not
		/// enable the filter if no resonance is applied. Resonance is only
		/// ever applied if the cutoff is not full or the resonance is not
		/// zero
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_Filter_7f()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "Filter-7f.it", "Filter-7f.data");
		}
	}
}
