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
		/// This input caused hangs and excessive memory consumption in the
		/// GDM loader due to invalid sample sizes
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Gdm_Invalid_Sample_Size()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Gdm_Invalid_Sample_Size.gdm", opaque);
			Assert.AreEqual(0, ret, "Module load");

			opaque.Xmp_Free_Context();
		}
	}
}
