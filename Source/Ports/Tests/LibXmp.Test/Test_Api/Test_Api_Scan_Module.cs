/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Api
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Api
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Api_Scan_Module()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// Try to scan before loading
			opaque.Xmp_Scan_Module();

			Create_Simple_Module(opaque, 2, 2);

			New_Event(opaque, 0, 0, 0, 0, 0, 0, Effects.Fx_Speed, 0x03, 0, 0);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_Speed, 0x1f, 0, 0);
			New_Event(opaque, 0, 2, 0, 0, 0, 0, Effects.Fx_Speed, 0x02, 0, 0);
			New_Event(opaque, 0, 3, 0, 0, 0, 0, Effects.Fx_Speed, 0x20, 0, 0);
			New_Event(opaque, 0, 4, 0, 0, 0, 0, Effects.Fx_Speed, 0x80, 0, 0);

			opaque.Xmp_Scan_Module();

			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < (3 + 0x1f + 3 * 2); i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);
				Assert.AreEqual(5720, info.Total_Time, "Total time error");
			}

			opaque.Xmp_Release_Module();

			// Load something with an absurd number of sequences
			c_int ret = LoadModule(dataDirectory, "Scan_240_Seq.it", opaque);
			Assert.AreEqual(0, ret, "Load module");

			opaque.Xmp_Scan_Module();

			opaque.Xmp_Get_Module_Info(out Xmp_Module_Info mInfo);
			Assert.AreEqual(240, mInfo.Num_Sequences, "Should have 240 sequences");

			for (c_int i = 0; i < mInfo.Num_Sequences; i++)
				Assert.AreEqual(i, mInfo.Seq_Data[i].Entry_Point, "Entry point");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
