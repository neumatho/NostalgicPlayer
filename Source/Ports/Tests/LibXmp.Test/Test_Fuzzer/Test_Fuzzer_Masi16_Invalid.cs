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
		public void Test_Fuzzer_Masi16_Invalid()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// This input crashed the PSM loader due to containing a samples
			// count over 64, which overflowed the sample offsets array
			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Masi16_Invalid.psm", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (1)");

			// The maximum sample length supported in PS16 PSMs is 64k
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Masi16_Invalid2.psm", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (2)");

			// Contains a duplicate sample that causes an uninitialized read
			// of the sample offsets array
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Masi16_Invalid3.psm", opaque);
			Assert.AreEqual(0, ret, "Module load (3)");

			opaque.Xmp_Free_Context();
		}
	}
}
