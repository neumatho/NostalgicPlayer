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
		/// This input caused an out of bounds read due to the Ultra Tracker
		/// loader allowing "MAS_UTrack_V000" as an ID. This doesn't seem to
		/// be an actual magic string used by any ULT modules
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Ult_V000()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Ult_V000.ult", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Format, ret, "Module load");

			opaque.Xmp_Free_Context();
		}
	}
}
