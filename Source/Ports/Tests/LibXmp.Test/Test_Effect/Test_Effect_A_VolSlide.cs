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
		private static readonly c_int[] vals_A =
		[
			64, 62, 60, 58,		// Down 2
			58, 56, 54, 52,		// Memory
			52, 53, 54, 55,		// Up 1
			55, 56, 57, 58,		// Memory
			63, 63, 63, 63,		// Set 63
			63, 64, 64, 64,		// Up 1
			1, 1, 1, 1,			// Set 1
			1, 0, 0, 0,			// Down 1
			10, 10, 10, 10,		// Set 10
			10, 25, 40, 55,		// Slide 0xf2
			55, 64, 64, 64,		// Slide 0x00
			64, 64, 64, 64,		// Slide 0x1f
			64, 64, 64, 64		// Slide 0x00
		];

		private static readonly c_int[] vals_Fine_A =
		[
			64, 62, 60, 58,		// Down 2
			58, 56, 54, 52,		// Memory
			52, 53, 54, 55,		// Up 1
			55, 56, 57, 58,		// Memory
			63, 63, 63, 63,		// Set 63
			63, 64, 64, 64,		// Up 1
			1, 1, 1, 1,			// Set 1
			1, 0, 0, 0,			// Down 1
			10, 10, 10, 10,		// Set 10
			8, 8, 8, 8,			// Fine slide down 2
			6, 6, 6, 6,			// Continue
			7, 7, 7, 7,			// Fine slide up 1
			8, 8, 8, 8			// Continue
		];

		private static readonly c_int[] vals_Pdn_A =
		[
			64, 62, 60, 58,		// Down 2
			58, 56, 54, 52,		// Memory
			52, 53, 54, 55,		// Up 1
			55, 56, 57, 58,		// Memory
			63, 63, 63, 63,		// Set 63
			63, 64, 64, 64,		// Up 1
			1, 1, 1, 1,			// Set 1
			1, 0, 0, 0,			// Down 1
			10, 10, 10, 10,		// Set 10
			10, 8, 6, 4,		// Slide 0xf2
			4, 2, 0, 0,			// Continue
			0, 0, 0, 0,			// Slide 0x1f
			0, 0, 0, 0			// Continue
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_A_VolSlide()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Frame_Info info;

			Create_Simple_Module(opaque, 2, 2);

			// Slide down & up with memory
			New_Event(opaque, 0, 0, 0, 49, 1, 0, Effects.Fx_VolSlide, 0x02, Effects.Fx_Speed, 4);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_VolSlide, 0x00, 0, 0);
			New_Event(opaque, 0, 2, 0, 0, 0, 0, Effects.Fx_VolSlide, 0x10, 0, 0);
			New_Event(opaque, 0, 3, 0, 0, 0, 0, Effects.Fx_VolSlide, 0x00, 0, 0);

			// Limits
			New_Event(opaque, 0, 4, 0, 0, 0, 0, Effects.Fx_VolSet, 0x3f, 0, 0);
			New_Event(opaque, 0, 5, 0, 0, 0, 0, Effects.Fx_VolSlide, 0x10, 0, 0);
			New_Event(opaque, 0, 6, 0, 0, 0, 0, Effects.Fx_VolSet, 0x01, 0, 0);
			New_Event(opaque, 0, 7, 0, 0, 0, 0, Effects.Fx_VolSlide, 0x01, 0, 0);

			// Fine effects
			New_Event(opaque, 0, 8, 0, 0, 0, 0, Effects.Fx_VolSet, 0x0a, 0, 0);
			New_Event(opaque, 0, 9, 0, 0, 0, 0, Effects.Fx_VolSlide, 0xf2, 0, 0);
			New_Event(opaque, 0, 10, 0, 0, 0, 0, Effects.Fx_VolSlide, 0x00, 0, 0);
			New_Event(opaque, 0, 11, 0, 0, 0, 0, Effects.Fx_VolSlide, 0x1f, 0, 0);
			New_Event(opaque, 0, 12, 0, 0, 0, 0, Effects.Fx_VolSlide, 0x00, 0, 0);

			// Play it
			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 13 * 4; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				Assert.AreEqual(vals_A[i], info.Channel_Info[0].Volume, "Volume slide error");
			}

			// Again with fine effects
			Set_Quirk(opaque, Quirk_Flag.FineFx, Read_Event.Mod);
			opaque.Xmp_Restart_Module();

			for (c_int i = 0; i < 13 * 4; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				Assert.AreEqual(vals_Fine_A[i], info.Channel_Info[0].Volume, "Volume slide error");
			}

			// Again with volume priority down
			Reset_Quirk(opaque, Quirk_Flag.FineFx);
			Set_Quirk(opaque, Quirk_Flag.VolPdn, Read_Event.Mod);
			opaque.Xmp_Restart_Module();

			for (c_int i = 0; i < 13 * 4; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				Assert.AreEqual(vals_Pdn_A[i], info.Channel_Info[0].Volume, "Volume slide error");
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
