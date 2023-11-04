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
		/// These inputs caused hangs and UMRs due to a missing EOF checks
		/// in the STMIK loader
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Stx_Truncated()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// Truncated pattern (hang)
			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Stx_Truncated.stx", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load");

			// Truncated instrument pattern (UMR)
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Stx_Truncated2.stx", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load");

			opaque.Xmp_Free_Context();
		}
	}
}
