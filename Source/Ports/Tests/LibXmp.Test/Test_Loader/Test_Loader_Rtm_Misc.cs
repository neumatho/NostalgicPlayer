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
		/// Tests for misc. RTM properties fixed around the same time:
		/// 
		/// 1) Patterns can have 999 rows
		/// 2) Fixed loading of effects 8/a/d/e/f/k
		/// 3) Stripping of fine effects interpretations of 1/2/A
		///    (TODO should be ignored at runtime instead)
		/// 4) Instrument mute samples flag
		/// 5) Sample default panning (and lack thereof)
		/// 6) Sample base volume (equivalent to S3M/IT global volume)
		///
		/// This module doesn't actually play properly due to the coverage
		/// for effects 1/2/e/f and A/d relying on separate effects memory,
		/// which libxmp doesn't currently support
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Loader_Rtm_Misc()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			c_int ret = LoadModule(dataDirectory, "Rtm_Misc.rtm", opaque);
			Assert.AreEqual(0, ret, "Module load");

			opaque.Xmp_Get_Module_Info(out Xmp_Module_Info info);

			ret = Compare_Module(info.Mod, OpenStream(dataDirectory, "Format_Rtm_Misc.data"));
			Assert.AreEqual(0, ret, "Format not correctly loaded");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
