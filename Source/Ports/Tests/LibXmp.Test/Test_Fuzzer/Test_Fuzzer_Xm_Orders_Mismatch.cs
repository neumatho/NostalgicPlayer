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
		/// This input caused UMRs in the XM loader due to the song length
		/// and order list header size being different
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Xm_Orders_Mismatch()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Xm_Orders_Mismatch.xm", opaque);
			Assert.AreEqual(0, ret, "Module load");

			opaque.Xmp_Free_Context();
		}
	}
}
