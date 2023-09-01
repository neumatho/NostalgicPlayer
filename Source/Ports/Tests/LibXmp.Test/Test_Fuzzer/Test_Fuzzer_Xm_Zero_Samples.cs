/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Fuzzer
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Fuzzer
	{
		/********************************************************************/
		/// <summary>
		/// This input caused leaks in the XM loader due to containing zero
		/// samples and libxmp_realloc_samples ignoring m.Xtra
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Xm_Zero_Samples()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Xm_Zero_Samples.xm", opaque);
			Assert.AreEqual(0, ret, "Module load");

			opaque.Xmp_Free_Context();
		}
	}
}
