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
		private static readonly c_int[] vals_F =
		[
			125, 3, 125, 3, 125, 3,		// Set speed to 0x03
			125, 31, 125, 31, 125, 31,
			125, 31, 125, 31, 125, 31,
			125, 31, 125, 31, 125, 31,
			125, 31, 125, 31, 125, 31,
			125, 31, 125, 31, 125, 31,
			125, 31, 125, 31, 125, 31,
			125, 31, 125, 31, 125, 31,
			125, 31, 125, 31, 125, 31,
			125, 31, 125, 31, 125, 31,
			125, 31, 125, 31, 125, 31,
			125, 31,					// Set speed to 0x1f
			125, 2, 125, 2,				// Set speed to 0x02
			32, 2, 32, 2,				// Set speed to 0x20
			128, 2, 128, 2				// Set speed to 0x80
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_F_Set_Speed()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			Create_Simple_Module(opaque, 2, 2);

			New_Event(opaque, 0, 0, 0, 0, 0, 0, Effects.Fx_Speed, 0x03, 0, 0);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_Speed, 0x1f, 0, 0);
			New_Event(opaque, 0, 2, 0, 0, 0, 0, Effects.Fx_Speed, 0x02, 0, 0);
			New_Event(opaque, 0, 3, 0, 0, 0, 0, Effects.Fx_Speed, 0x20, 0, 0);
			New_Event(opaque, 0, 4, 0, 0, 0, 0, Effects.Fx_Speed, 0x80, 0, 0);

			opaque.scan.LibXmp_Scan_Sequences();

			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < (3 + 0x1f + 3 * 2); i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);
				Assert.AreEqual(5720, info.Total_Time, "Total time error");
				Assert.AreEqual(vals_F[i * 2], info.Bpm, "Tempo setting error");
				Assert.AreEqual(vals_F[i * 2 + 1], info.Speed, "Speed setting error");
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
