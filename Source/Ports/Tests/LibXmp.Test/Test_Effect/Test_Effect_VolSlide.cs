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
		private static readonly c_int[] vals_VS =
		{
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
			9, 9, 9, 9,			// Fine slide up 1
			10, 10, 10, 10,		// Memory
			32, 32, 32, 32,		// Set 32
			32, 30, 28, 26,		// Down 2
			26, 29, 32, 35,		// Up 3
			35, 38, 41, 44		// Memory
		};

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_VolSlide()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			Create_Simple_Module(opaque, 2, 2);

			// Slide down & up with memory
			New_Event(opaque, 0, 0, 0, 49, 1, 0, Effects.Fx_VolSlide_Dn, 0x02, Effects.Fx_Speed, 4);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_VolSlide_Dn, 0x00, 0, 0);
			New_Event(opaque, 0, 2, 0, 0, 0, 0, Effects.Fx_VolSlide_Up, 0x01, 0, 0);
			New_Event(opaque, 0, 3, 0, 0, 0, 0, Effects.Fx_VolSlide_Up, 0x00, 0, 0);

			// Limits
			New_Event(opaque, 0, 4, 0, 0, 0, 0, Effects.Fx_VolSet, 0x3f, 0, 0);
			New_Event(opaque, 0, 5, 0, 0, 0, 0, Effects.Fx_VolSlide_Up, 0x01, 0, 0);
			New_Event(opaque, 0, 6, 0, 0, 0, 0, Effects.Fx_VolSet, 0x01, 0, 0);
			New_Event(opaque, 0, 7, 0, 0, 0, 0, Effects.Fx_VolSlide_Dn, 0x01, 0, 0);

			// Fine effects
			New_Event(opaque, 0, 8, 0, 0, 0, 0, Effects.Fx_VolSet, 0x0a, 0, 0);
			New_Event(opaque, 0, 9, 0, 0, 0, 0, Effects.Fx_F_VSlide, 0x02, 0, 0);
			New_Event(opaque, 0, 10, 0, 0, 0, 0, Effects.Fx_F_VSlide, 0x10, 0, 0);
			New_Event(opaque, 0, 11, 0, 0, 0, 0, Effects.Fx_F_VSlide, 0x00, 0, 0);

			// Secondary volslide
			New_Event(opaque, 0, 12, 0, 0, 0, 0, Effects.Fx_VolSet, 0x20, 0, 0);
			New_Event(opaque, 0, 13, 0, 0, 0, 0, Effects.Fx_VolSlide_2, 0x02, 0, 0);
			New_Event(opaque, 0, 14, 0, 0, 0, 0, Effects.Fx_VolSlide_2, 0x30, 0, 0);
			New_Event(opaque, 0, 15, 0, 0, 0, 0, Effects.Fx_VolSlide_2, 0x00, 0, 0);

			// Play it
			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 16 * 4; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);
				Assert.AreEqual(vals_VS[i], info.channel_Info[0].Volume, "Volume slide error");
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
