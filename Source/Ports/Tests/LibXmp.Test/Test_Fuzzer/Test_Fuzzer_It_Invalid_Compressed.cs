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
		/// Inputs that caused issues in IT sample depacking
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_It_Invalid_Compressed()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// This input caused hangs and high RAM consumption in the IT loader
			// due to allocating large buffers for invalid compressed samples
			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_It_Invalid_Compressed.it", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load");

			// This input resulted in invalid shift exponents in read_bits
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_It_Invalid_Compressed2.it", opaque);
			Assert.AreEqual(0, ret, "Module load");

			// This input sets the word length to 32, which can cause invalid shift
			// exponents if not carefully implemented
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_It_Invalid_Compressed3.it", opaque);
			Assert.AreEqual(0, ret, "Module load");

			// This input sets the word length to 31, which can cause signed int
			// overflows if not carefully implemented
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_It_Invalid_Compressed4.it", opaque);
			Assert.AreEqual(0, ret, "Module load");

			opaque.Xmp_Free_Context();
		}
	}
}
