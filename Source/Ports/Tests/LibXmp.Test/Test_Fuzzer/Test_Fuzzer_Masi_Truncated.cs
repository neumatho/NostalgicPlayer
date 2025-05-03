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
		public void Test_Fuzzer_Masi_Truncated()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// This input caused uninitialized reads in the MASI loader
			// due to a missing EOF check when checking the pattern count
			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Masi_Truncated.psm", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (1)");

			// This input caused uninitialized reads in the MASI loader
			// due to a missing EOF check when reading the TITL chunk
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Masi_Truncated2.psm", opaque);
			Assert.AreEqual(0, ret, "Module load (2)");

			opaque.Xmp_Free_Context();
		}
	}
}
