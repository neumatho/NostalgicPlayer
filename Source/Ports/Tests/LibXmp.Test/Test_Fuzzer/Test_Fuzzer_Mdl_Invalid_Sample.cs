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
		/// These inputs caused high amounts of RAM usage, hangs, and
		/// undefined behavior in the MDL loader instrument and sample chunk
		/// handlers
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Mdl_Invalid_Sample()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// Contains an invalid pack type of 3, but the loader would allocate
			// a giant buffer for it before rejecting it
			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Mdl_Invalid_Sample_Pack.mdl", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (pack)");

			// Invalid sample size for a pack type 0 sample
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Mdl_Invalid_Sample_Size.mdl", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (size)");

			// Invalid sample size for a pack type 2 sample
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Mdl_Invalid_Sample_Size2.mdl", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (size2)");

			// Negative sample length (caused crashes)
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Mdl_Invalid_Sample_Size3.mdl", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (size3)");

			// Negative loop start (caused overflows)
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Mdl_Invalid_Sample_Loop.mdl", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (loop)");

			// Loop start past sample length (caused overflows)
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Mdl_Invalid_Sample_Loop2.mdl", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (loop2)");

			// Loop length past sample end (caused overflows)
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Mdl_Invalid_Sample_Loop3.mdl", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (loop3)");

			opaque.Xmp_Free_Context();
		}
	}
}
