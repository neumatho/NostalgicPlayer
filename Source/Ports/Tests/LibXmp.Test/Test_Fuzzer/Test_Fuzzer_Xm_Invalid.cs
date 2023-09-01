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
		public void Test_Fuzzer_Xm_Invalid()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// This input contains some unusual/invalid patterns that originally
			// caused undefined behavior in the OXM test function. It now covers
			// related pattern checks in the XM loader
			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Xm_Invalid_Pattern_Length.xm", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Depacking (invalid_pattern_length)");

			// This input caused undefined behavior in the XM loader due to
			// having a broken instrument header size bounds check
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Xm_Invalid_InstSize.xm", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (invalid_instsize)");

			opaque.Xmp_Free_Context();
		}
	}
}
