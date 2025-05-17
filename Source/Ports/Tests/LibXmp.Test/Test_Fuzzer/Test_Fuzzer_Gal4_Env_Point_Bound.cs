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
		/// This input caused out-of-bounds reads in the Galaxy 4.0 loader
		/// due to incorrectly bounded envelope point counts
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Gal4_Env_Point_Bound()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			c_int ret = LoadModule(Path.Combine(dataDirectory, "F"), "Load_Gal4_Env_Point_Bound.gal4", opaque);
			Assert.AreEqual(0, ret, "Module load");

			opaque.Xmp_Get_Module_Info(out Xmp_Module_Info info);

			ret = Compare_Module(info.Mod, OpenStream(Path.Combine(dataDirectory, "F"), "Load_Gal4_Env_Point_Bound.data"));
			Assert.AreEqual(0, ret, "Format not correctly loaded");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
