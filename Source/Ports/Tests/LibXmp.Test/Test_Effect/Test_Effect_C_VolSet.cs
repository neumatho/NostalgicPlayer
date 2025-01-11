/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Effect
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Effect
	{
		private static readonly c_int[] vals_C =
		[
			48, 48, 48,		// Set 0x30
			64, 64, 64,		// Instrument default
			0, 0, 0,		// Set 0x00
			64, 64, 64,		// Set 0x40
			64, 64, 64,		// Set 0x41
			64, 64, 64,		// Set 0x50
			64, 64, 64,		// Set 0x51
			64, 64, 64		// Set 0xa0
		];

		private static readonly c_int[] vals2_C =
		[
			38, 38, 38,		// Set 0x30
			51, 51, 51,		// Instrument default
			0, 0, 0,		// Set 0x00
			51, 51, 51,		// Set 0x40
			52, 52, 52,		// Set 0x41
			64, 64, 64,		// Set 0x50
			64, 64, 64,		// Set 0x51
			64, 64, 64		// Set 0xa0
		];

		private static readonly c_int[] vals3_C =
		[
			16, 16, 16,		// Set 0x30
			0, 0, 0,		// Instrument default
			64, 64, 64,		// Set 0x00
			0, 0, 0,		// Set 0x40
			0, 0, 0,		// Set 0x41
			0, 0, 0,		// Set 0x50
			0, 0, 0,		// Set 0x51
			0, 0, 0			// Set 0xa0
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_C_VolSet()
		{
			c_int[] volMap = new c_int[65];

			for (c_int i = 0; i < 64; i++)
				volMap[i] = 64 - i;

			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Module_Data m = ctx.M;
			Xmp_Frame_Info info;

			Create_Simple_Module(opaque, 2, 2);

			// Slide down & up with memory
			New_Event(opaque, 0, 0, 0, 60, 1, 0x20, Effects.Fx_VolSet, 0x30, Effects.Fx_Speed, 3);
			New_Event(opaque, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0);
			New_Event(opaque, 0, 2, 0, 0, 0, 0x20, Effects.Fx_VolSet, 0x00, 0, 0);
			New_Event(opaque, 0, 3, 0, 0, 0, 0, Effects.Fx_VolSet, 0x40, 0, 0);
			New_Event(opaque, 0, 4, 0, 0, 0, 0, Effects.Fx_VolSet, 0x41, 0, 0);
			New_Event(opaque, 0, 5, 0, 0, 0, 0, Effects.Fx_VolSet, 0x50, 0, 0);
			New_Event(opaque, 0, 6, 0, 0, 0, 0, Effects.Fx_VolSet, 0x51, 0, 0);
			New_Event(opaque, 0, 7, 0, 0, 0, 0, Effects.Fx_VolSet, 0xa0, 0, 0);

			// Play it
			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 8 * 3; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				Assert.AreEqual(vals_C[i], info.Channel_Info[0].Volume, "Volume set error");
			}

			// Again different volbase
			m.VolBase = 0x50;
			opaque.Xmp_Restart_Module();

			for (c_int i = 0; i < 8 * 3; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				Assert.AreEqual(vals2_C[i], info.Channel_Info[0].Volume, "Volume set error");
			}

			// Again with volume map
			m.VolBase = 0x40;
			m.Vol_Table = volMap;
			opaque.Xmp_Restart_Module();

			for (c_int i = 0; i < 8 * 3; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				Assert.AreEqual(vals3_C[i], info.Channel_Info[0].Volume, "Volume set error");
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
