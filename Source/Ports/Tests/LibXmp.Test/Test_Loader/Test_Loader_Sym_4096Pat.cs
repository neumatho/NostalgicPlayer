/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Loader
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Loader
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Loader_Sym_4096Pat()
		{
			c_int[] idx = [ 0, 0xff9, 0xffa, 0xffb, 0xffc, 0xffd, 0xffe, 0xfff ];

			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			c_int ret = LoadModule(dataDirectory, "4096_Patterns.dsym", opaque);
			Assert.AreEqual(0, ret, "Module load");

			opaque.Xmp_Get_Module_Info(out Xmp_Module_Info info);

			// Data file is too big so just test the relevant portions
			Assert.AreEqual(1, info.Mod.Pat, "Patterns mismatch");
			Assert.AreEqual(8, info.Mod.Chn, "Channels mismatch");

			for (c_int i = 0; i < 8; i++)
				Assert.AreEqual(idx[i], info.Mod.Xxp[0].Index[i], "Tracks mismatch");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
