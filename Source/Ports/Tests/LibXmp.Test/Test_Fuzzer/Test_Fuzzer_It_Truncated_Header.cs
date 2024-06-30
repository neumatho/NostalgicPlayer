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
		/// This input caused uninitialized reads in the IT loader due to not
		/// checking for EOFs when loading the IT channel tables from the
		/// header. This needs to be tested using xmp_load_module_from_memory
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_It_Truncated_Header()
		{
			Read_File_To_Memory(Path.Combine(dataDirectory, "F"), "Load_It_Truncated_Header.it", out byte[] buffer, out c_long size);
			Assert.IsNotNull(buffer, "Read file to memory");

			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			c_int ret = opaque.Xmp_Load_Module_From_Memory(buffer, size);
			Assert.AreEqual(-(c_int)Xmp_Error.Load, ret, "Module load");

			opaque.Xmp_Free_Context();
		}
	}
}
