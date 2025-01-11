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
		private static readonly c_int[] vals_S3M_Bpm =
		[
			80, 80, 80,		// Set tempo
			20, 20, 20,		// Set tempo 0x02
			20, 20, 20,		// Set tempo 0x11
			20, 20, 20,		// Nothing
			32, 32, 32,		// Set tempo 0x20
			20, 20, 20,		// Set tempo 0x01
			255, 255, 255,	// Set tempo 0xff
			20, 20, 20		// Set tempo 0x11
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_S3M_Bpm()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Frame_Info info = null;

			Create_Simple_Module(opaque, 2, 2);

			New_Event(opaque, 0, 0, 0, 0, 0, 0, Effects.Fx_S3M_Bpm, 0x50, Effects.Fx_Speed, 3);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_S3M_Bpm, 0x02, 0, 0);
			New_Event(opaque, 0, 2, 0, 0, 0, 0, Effects.Fx_S3M_Bpm, 0x11, 0, 0);
			New_Event(opaque, 0, 4, 0, 0, 0, 0, Effects.Fx_S3M_Bpm, 0x20, 0, 0);
			New_Event(opaque, 0, 5, 0, 0, 0, 0, Effects.Fx_S3M_Bpm, 0x01, 0, 0);
			New_Event(opaque, 0, 6, 0, 0, 0, 0, Effects.Fx_S3M_Bpm, 0xff, 0, 0);
			New_Event(opaque, 0, 7, 0, 0, 0, 0, Effects.Fx_S3M_Bpm, 0x11, 0, 0);

			opaque.scan.LibXmp_Scan_Sequences();

			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 8 * 3; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				Assert.AreEqual(vals_S3M_Bpm[i], info.Bpm, "Tempo setting error");
			}

			Assert.AreEqual(4431, info!.Total_Time, "Total time error");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
