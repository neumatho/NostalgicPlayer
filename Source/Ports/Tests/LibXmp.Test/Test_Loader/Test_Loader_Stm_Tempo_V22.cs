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
		/// NOTE: ST2 has very bizarre tempo handling. libxmp currently plays
		/// this module incorrectly due to various limitations, hence this is
		/// a loader test to verify approximate behavior instead of a player
		/// test
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Loader_Stm_Tempo_V22()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			c_int ret = LoadModule(dataDirectory, "St22_Tempo.stm", opaque);
			Assert.AreEqual(0, ret, "Module load");

			opaque.Xmp_Get_Module_Info(out Xmp_Module_Info info);

			ret = Compare_Module(info.Mod, OpenStream(dataDirectory, "Format_Stm_Tempo_V22.data"));
			Assert.AreEqual(0, ret, "Format not correctly loaded");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
