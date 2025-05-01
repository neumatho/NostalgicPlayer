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
		/// Tests for truncated MDL modules that caused problems in the MDL
		/// loader
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Mdl_Truncated()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// This input caused UMRs in the MDL loader due to not checking
			// the hio_read return value for a truncated instrument name
			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Mdl_Truncated.mdl", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load");

			// This input caused UMRs in the MDL loader due to not checking
			// the hio_read return value for the format version
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Mdl_Truncated2.mdl", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load");

			opaque.Xmp_Free_Context();
		}
	}
}
