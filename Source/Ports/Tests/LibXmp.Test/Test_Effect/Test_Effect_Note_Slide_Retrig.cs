/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Effect
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Effect
	{
		private static readonly c_int[] vals_NS_RT =
		[
			856, 856, 856, 808, 808, 808, 763,
			763, 763, 720, 720, 720, 679, 679,
			679, 679, 679, 641, 641, 641, 605,
			605, 605, 605, 539, 539, 539, 480,
			480, 480, 428, 428, 381, 381, 340,
			340, 286, 240, 202, 170, 143, 120,

			120, 120, 120, 127, 127, 127, 135,
			135, 135, 143, 143, 143, 151, 151,
			151, 151, 151, 160, 160, 160, 170,
			170, 170, 170, 191, 191, 191, 214,
			214, 214, 240, 240, 270, 270, 303,
			303, 360, 428, 509, 605, 720, 856
		];

		private static readonly bool[] vals2_NS_RT =
		[
			true, false, false, true, false, false, true,
			false, false, true, false, false, true, false,
			false, false, false, true, false, false, true,
			false, false, false, true, false, false, true,
			false, false, true, false, true, false, true,
			false, true, true, true, true, true, true,

			false, false, false, true, false, false, true,
			false, false, true, false, false, true, false,
			false, false, false, true, false, false, true,
			false, false, false, true, false, false, true,
			false, false, true, false, true, false, true,
			false, true, true, true, true, true, true
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Note_Slide_Retrig()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Create_Simple_Module(opaque, 2, 2);

			New_Event(opaque, 0, 0, 0, 49, 1, 0, Effects.Fx_NSlide_R_Up, 0x31, Effects.Fx_Speed, 7);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_NSlide_R_Up, 0x00, 0, 0);
			New_Event(opaque, 0, 2, 0, 0, 0, 0, Effects.Fx_NSlide_R_Up, 0x31, 0, 0);
			New_Event(opaque, 0, 3, 0, 0, 0, 0, Effects.Fx_NSlide_R_Up, 0x32, 0, 0);
			New_Event(opaque, 0, 4, 0, 0, 0, 0, Effects.Fx_NSlide_R_Up, 0x22, 0, 0);
			New_Event(opaque, 0, 5, 0, 0, 0, 0, Effects.Fx_NSlide_R_Up, 0x13, 0, 0);

			New_Event(opaque, 0, 6, 0, 0, 0, 0, Effects.Fx_NSlide_R_Dn, 0x31, 0, 0);
			New_Event(opaque, 0, 7, 0, 0, 0, 0, Effects.Fx_NSlide_R_Dn, 0x00, 0, 0);
			New_Event(opaque, 0, 8, 0, 0, 0, 0, Effects.Fx_NSlide_R_Dn, 0x31, 0, 0);
			New_Event(opaque, 0, 9, 0, 0, 0, 0, Effects.Fx_NSlide_R_Dn, 0x32, 0, 0);
			New_Event(opaque, 0, 10, 0, 0, 0, 0, Effects.Fx_NSlide_R_Dn, 0x22, 0, 0);
			New_Event(opaque, 0, 11, 0, 0, 0, 0, Effects.Fx_NSlide_R_Dn, 0x13, 0, 0);

			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 12; i++)
			{
				for (c_int j = 0; j < 7; j++)
				{
					opaque.Xmp_Play_Frame();
					opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

					c_int voc = Map_Channel(p, 0);
					Assert.IsGreaterThanOrEqualTo(0, voc, "Virtual map");
					Mixer_Voice vi = p.Virt.Voice_Array[voc];

					Assert.AreEqual(vals_NS_RT[i * 7 + j], Period(info), "Note slide error");

					if (vals2_NS_RT[i * 7 + j])
						Assert.AreEqual(0, vi.Pos0, "Sample position");
					else
						Assert.AreNotEqual(0, vi.Pos0, "Sample position");
				}
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
