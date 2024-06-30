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
		/// Test loading 8-bit and 16-bit stereo XM samples (OggMod)
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Loader_Xm_Stereo_Vorbis()
		{
			Ports.LibXmp.LibXmp ctx = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			c_int ret = LoadModule(dataDirectory, "Stereo.oxm", ctx);
			Assert.AreEqual(0, ret, "Module load");

			ctx.Xmp_Get_Module_Info(out Xmp_Module_Info info);

			ret = Compare_Module(info.Mod, OpenStream(dataDirectory, "Stereo_Oxm.data"));
			Assert.AreEqual(0, ret, "Format not correctly loaded");

			ctx.Xmp_Release_Module();
			ctx.Xmp_Free_Context();
		}
	}
}
