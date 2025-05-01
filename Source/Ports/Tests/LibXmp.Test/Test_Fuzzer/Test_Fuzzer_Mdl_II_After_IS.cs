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
		/// This input caused leaks in the MDL loader due to duplicate SA
		/// chunks being allowed
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Mdl_II_After_IS()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Mdl_II_After_IS.mdl", opaque);
			Assert.AreEqual(0, ret, "Module load");

			opaque.Xmp_Free_Context();
		}
	}
}
