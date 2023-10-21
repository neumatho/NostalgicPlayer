/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Fuzzer
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Fuzzer
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Asy_Invalid_Samples()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// The ASYLUM format only supports up to 64 samples, reject higher counts
			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Asy_Invalid_Samples.amf", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (1)");

			// Despite using 32-bit sample length fields, the ASYLUM format is
			// converted from MOD and should never have samples longer than 128k.
			// (All of the original ASYLUM module samples seem to be <64k)
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Asy_Invalid_Samples2.amf", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (2)");

			opaque.Xmp_Free_Context();
		}
	}
}
