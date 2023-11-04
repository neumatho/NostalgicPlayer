/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
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
		public void Test_Loader_Stm_V1()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			c_int ret = LoadModule(Path.Combine(dataDirectory, "M"), "FCSlide1.sts", opaque);
			Assert.AreEqual(0, ret, "Module load");

			opaque.Xmp_Get_Module_Info(out Xmp_Module_Info info);

			ret = Util.Compare_Module(info.Mod, OpenStream(dataDirectory, "Format_Stm_V1.data"));
			Assert.AreEqual(0, ret, "Format not correctly loaded");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
