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
		/// This input caused scan_module to attempt to endlessly scan a
		/// pattern
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Mod_Scan_Row_Limit()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Mod_Scan_Row_Limit.mod", opaque);
			Assert.AreEqual(0, ret, "Module load");

			opaque.Xmp_Free_Context();
		}
	}
}
