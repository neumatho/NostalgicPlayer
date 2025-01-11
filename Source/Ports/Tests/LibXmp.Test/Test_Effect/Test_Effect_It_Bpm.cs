/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Effect
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Effect
	{
		private static readonly c_int[] vals_It_Bpm =
		[
			80, 80, 80,		// Set tempo
			80, 78, 76,		// Slide down 2
			76, 77, 78,		// Slide up 1
			78, 78, 78,		// Nothing
			32, 32, 32,		// Set as 0x20
			32, 32, 32,		// Slide down
			255, 255, 255,	// Set as 0xff
			255, 255, 255,	// Slide up

			125, 125, 125,	// Set as 0x7d
			125, 127, 129,	// Slide up 2
			129, 131, 133,	// Slide up (T00)
			133, 131, 129,	// Slide down 2
			129, 127, 125,	// Slide down (T00)
			255, 255, 255,	// Set as 0xff
			255, 253, 251,	// Slide down (T00)
			251, 253, 255,	// Slide up 2
			252, 252, 252,	// Set as 0xfc
			252, 254, 255	// Slide up (T00)
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_It_Bpm()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			Create_Simple_Module(opaque, 2, 2);

			New_Event(opaque, 0, 0, 0, 0, 0, 0, Effects.Fx_It_Bpm, 0x50, Effects.Fx_Speed, 3);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_It_Bpm, 0x02, 0, 0);
			New_Event(opaque, 0, 2, 0, 0, 0, 0, Effects.Fx_It_Bpm, 0x11, 0, 0);
			New_Event(opaque, 0, 4, 0, 0, 0, 0, Effects.Fx_It_Bpm, 0x20, 0, 0);
			New_Event(opaque, 0, 5, 0, 0, 0, 0, Effects.Fx_It_Bpm, 0x01, 0, 0);
			New_Event(opaque, 0, 6, 0, 0, 0, 0, Effects.Fx_It_Bpm, 0xff, 0, 0);
			New_Event(opaque, 0, 7, 0, 0, 0, 0, Effects.Fx_It_Bpm, 0x11, 0, 0);

			New_Event(opaque, 0, 8, 0, 0, 0, 0, Effects.Fx_It_Bpm, 0x7d, 0, 0);
			New_Event(opaque, 0, 9, 0, 0, 0, 0, Effects.Fx_It_Bpm, 0x12, 0, 0);
			New_Event(opaque, 0, 10, 0, 0, 0, 0, Effects.Fx_It_Bpm, 0x00, 0, 0);
			New_Event(opaque, 0, 11, 0, 0, 0, 0, Effects.Fx_It_Bpm, 0x02, 0, 0);
			New_Event(opaque, 0, 12, 0, 0, 0, 0, Effects.Fx_It_Bpm, 0x00, 0, 0);
			New_Event(opaque, 0, 13, 0, 0, 0, 0, Effects.Fx_It_Bpm, 0xff, 0, 0);
			New_Event(opaque, 0, 14, 0, 0, 0, 0, Effects.Fx_It_Bpm, 0x00, 0, 0);
			New_Event(opaque, 0, 15, 0, 0, 0, 0, Effects.Fx_It_Bpm, 0x12, 0, 0);
			New_Event(opaque, 0, 16, 0, 0, 0, 0, Effects.Fx_It_Bpm, 0xfc, 0, 0);
			New_Event(opaque, 0, 17, 0, 0, 0, 0, Effects.Fx_It_Bpm, 0x00, 0, 0);

			opaque.scan.LibXmp_Scan_Sequences();

			opaque.Xmp_Start_Player(44100, 0);

			opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);
			Assert.AreEqual(4628, info!.Total_Time, "Total time error");

			for (c_int i = 0; i < 18 * 3; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				Assert.AreEqual(vals_It_Bpm[i], info.Bpm, "Tempo setting error");
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
