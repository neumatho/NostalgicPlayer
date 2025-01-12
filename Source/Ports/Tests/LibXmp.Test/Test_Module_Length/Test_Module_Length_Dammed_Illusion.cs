/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Module_Length
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Module_Length
	{
		/********************************************************************/
		/// <summary>
		/// This Octalyser CD81 module relies on being played in Octalyser
		/// 4 channel mode or 6 channel mode, as there are duplicate E6x
		/// effects in the last two channels. This information is NOT
		/// stored in the module.
		///
		/// As of writing this comment, libxmp supports this module by
		/// enabling an inaccurate loop mode quirk for Octalyser MODs,
		/// but this module could also be fixed by checking for its MD5.
		///
		/// In addition to this, it relies on global loop target/count,
		/// and found a bug in how pattern delay was being scanned.
		///
		/// TODO: not clear why the scan and real time are >200ms apart
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Module_Length_Dammed_Illusion()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			c_int time = 0;

			c_int ret = LoadModule(Path.Combine(dataDirectory, "P"), "Dammed_Illusion.mod", opaque);
			Assert.AreEqual(0, ret, "Module load");

			opaque.Xmp_Get_Module_Info(out Xmp_Module_Info mi);
			opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info fi);

			Assert.AreEqual(96, mi.Mod.Len, "Module length");
			Assert.AreEqual(354200, fi.Total_Time, "Estimated time");

			opaque.Xmp_Start_Player(Constants.Xmp_Min_SRate, 0);

			while (opaque.Xmp_Play_Frame() == 0)
			{
				opaque.Xmp_Get_Frame_Info(out fi);

				if (fi.Loop_Count > 0)
					break;

				time += fi.Frame_Time;
			}

			opaque.Xmp_End_Player();

			Assert.AreEqual(354435, time / 1000, "Elapsed time");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
