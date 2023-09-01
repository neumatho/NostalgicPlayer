/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Linq;
using System.Text;
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
		/// Misc. fuzzer input tests that don't really fit anywhere else
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Misc()
		{
			// 0x84 is specifically to check for a Coconizer bug
			byte[] buf = new byte[] { 0x84 }.Union(Encoding.ASCII.GetBytes("ZCDEFGH")).ToArray();

			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// Zero-length inputs previously caused out-of-bounds reads
			c_int ret = opaque.Xmp_Load_Module_From_Memory(Array.Empty<uint8>(), 0);
			Assert.AreEqual(-(c_int)Xmp_Error.Invalid, ret, "Module load (0 length)");

			// Tiny inputs caused uninitialized reads in some test functions
			for (c_int i = 1; i < buf.Length; i++)
			{
				ret = opaque.Xmp_Load_Module_From_Memory(buf, i);
				Assert.AreEqual(-(c_int)Xmp_Error.Format, ret, $"Module load ({i} length)");
			}

			opaque.Xmp_Free_Context();
		}
	}
}
