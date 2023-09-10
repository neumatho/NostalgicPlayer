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
		private static readonly c_int[] vals_Ft2_8 =
		{
			128, 128, 128,		// C-5 1 880   Play instrument w/ center pan
			255, 255, 255,		// --- - 8FF   Set pan to right
			255, 255, 255,		// F-5 - ---   New note keeps previous pan
			255, 255, 255,		// --- - 8FF   Set new pan
			0, 0, 0				// --- 1 ---   New inst resets to left pan
		};

		private static readonly c_int[] vals_St3_8 =
		{
			128, 128, 128,		// C-5 1 880   Play instrument w/ center pan
			255, 255, 255,		// --- - 8FF   Set pan to right
			255, 255, 255,		// F-5 - ---   New note keeps previous pan
			255, 255, 255,		// --- - 8FF   Set new pan
			0, 0, 0				// --- 1 ---   New inst resets to left pan
		};

		private static readonly c_int[] vals_It_8 =
		{
			128, 128, 128,		// C-5 1 880   Play instrument w/ center pan
			255, 255, 255,		// --- - 8FF   Set pan to right
			0, 0, 0,			// F-5 - ---   New note uses instrument pan
			255, 255, 255,		// --- - 8FF   Set new pan
			0, 0, 0				// --- 1 ---   New inst resets to left pan
		};

		private static readonly c_int[] vals_Dp_8 =
		{
			128, 128, 128,		// C-5 1 880   Play instrument w/ center pan
			255, 255, 255,		// --- - 8FF   Set pan to right
			255, 255, 255,		// F-5 - ---   Instrument pan is disabled
			255, 255, 255,		// --- - 8FF   Set new pan
			255, 255, 255		// --- 1 ---   New inst resets to left pan
		};

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_8_SetPan()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Xmp_Frame_Info info;

			Create_Simple_Module(opaque, 2, 2);

			// Set instrument 1 pan to left
			ctx.M.Mod.Xxi[0].Sub[0].Pan = 0;

			// Slide down & up with memory
			New_Event(opaque, 0, 0, 0, 60, 1, 0, Effects.Ex_SetPan, 0x80, Effects.Fx_Speed, 0x03);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Ex_SetPan, 0xff, 0, 0);
			New_Event(opaque, 0, 2, 0, 65, 0, 0, 0x00, 0x00, 0, 0);
			New_Event(opaque, 0, 3, 0, 0, 0, 0, Effects.Ex_SetPan, 0xff, 0, 0);
			New_Event(opaque, 0, 4, 0, 0, 1, 0, 0x00, 0x00, 0, 0);

			// Play it
			opaque.Xmp_Start_Player(44100, 0);

			// Set mix to 100% pan separation
			opaque.Xmp_Set_Player(Xmp_Player.Mix, 100);

			// Check FT2 event reader
			Set_Quirk(opaque, Quirk_Flag.Ft2, Read_Event.Ft2);

			for (c_int i = 0; i < 4 * 3; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				Assert.AreEqual(vals_Ft2_8[i], info.Channel_Info[0].Pan, "Pan error");
			}

			opaque.Xmp_Restart_Module();

			// Check ST3 event reader
			Set_Quirk(opaque, Quirk_Flag.St3, Read_Event.St3);

			for (c_int i = 0; i < 4 * 3; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				Assert.AreEqual(vals_St3_8[i], info.Channel_Info[0].Pan, "Pan error");
			}

			opaque.Xmp_Restart_Module();

			// Check IT event reader
			Set_Quirk(opaque, Quirk_Flag.It, Read_Event.It);

			for (c_int i = 0; i < 4 * 3; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				Assert.AreEqual(vals_It_8[i], info.Channel_Info[0].Pan, "Pan error");
			}

			opaque.Xmp_Restart_Module();

			// Check instrument pan as disabled
			ctx.M.Mod.Xxi[0].Sub[0].Pan = -1;

			for (c_int i = 0; i < 4 * 3; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				Assert.AreEqual(vals_Dp_8[i], info.Channel_Info[0].Pan, "Pan error");
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
