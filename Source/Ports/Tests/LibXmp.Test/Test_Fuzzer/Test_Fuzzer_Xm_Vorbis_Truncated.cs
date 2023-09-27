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
		public void Test_Fuzzer_Xm_Vorbis_Truncated()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// These inputs caused hangs, leaks, or crashes in stb_vorbis due
			// to missing or broken EOF checks in start decoder
			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Xm_Vorbis_Truncated.oxm", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Depacking (truncated)");

			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Xm_Vorbis_Truncated2.oxm", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Depacking (truncated2)");

			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Xm_Vorbis_Truncated3.oxm", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Depacking (truncated3)");

			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Xm_Vorbis_Truncated4.oxm", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Depacking (truncated4)");

			opaque.Xmp_Free_Context();
		}
	}
}
