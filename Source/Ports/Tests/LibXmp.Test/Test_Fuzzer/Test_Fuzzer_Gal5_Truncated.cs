/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
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
		public void Test_Fuzzer_Gal5_Truncated()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// This input caused UMRs in the Galaxy 5.0 loader due to not
			// checking the hio_read return value for the module title
			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Gal5_Truncated.gal5", opaque);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load");

			// This input caused uninitialized reads in the Galaxy 5.0 loader due to
			// a missing return value check when reading the channel array in the
			// INIT chunk. Requires testing using xmp_load_module_from_memory
			Read_File_To_Memory(Path.Combine(dataDirectory, "F"), "Load_Gal5_Truncated_Init.gal5", out CPointer<byte> buffer, out c_long size);
			Assert.IsTrue(buffer.IsNotNull, "Read file to memory");

			ret = opaque.Xmp_Load_Module_From_Memory(buffer, size);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load (init)");
			CMemory.free(buffer);

			// This input caused UMRs in the Galaxy 5.0 loader due to
			// reading uninitialized channel panning data after failing to read
			// the INIT chunk (in this example, due to an invalid size).
			// This edge case relies on quirks of the IFF loader
			ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Gal5_Truncated_Init_2.gal5", opaque);
			Assert.AreEqual(0, ret, "Module load (init_2)");

			opaque.Xmp_Free_Context();
		}
	}
}
