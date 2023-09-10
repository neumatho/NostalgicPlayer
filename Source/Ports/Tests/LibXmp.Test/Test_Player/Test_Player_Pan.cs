/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Player
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Player
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Pan()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);

			Create_Simple_Module(opaque, 2, 2);

			// Set channel pan
			for (c_int i = 0; i < 4; i++)
				ctx.M.Mod.Xxc[i].Pan = 0x80;

			// Set event pan
			New_Event(opaque, 0, 0, 0, 61, 1, 0, Effects.Fx_SetPan, 0x00, Effects.Fx_Speed, 3);
			New_Event(opaque, 0, 0, 1, 61, 1, 0, Effects.Fx_SetPan, 0x40, 0, 0);
			New_Event(opaque, 0, 0, 2, 61, 1, 0, Effects.Fx_SetPan, 0x80, 0, 0);
			New_Event(opaque, 0, 0, 3, 61, 1, 0, Effects.Fx_SetPan, 0xc0, 0, 0);

			for (c_int i = 1; i < 64; i++)
			{
				New_Event(opaque, 0, i, 0, 61, 1, 0, Effects.Fx_SetPan, 0x00 + i, 0, 0);
				New_Event(opaque, 0, i, 1, 61, 1, 0, Effects.Fx_SetPan, 0x40 + i, 0, 0);
				New_Event(opaque, 0, i, 2, 61, 1, 0, Effects.Fx_SetPan, 0x80 + i, 0, 0);
				New_Event(opaque, 0, i, 3, 61, 1, 0, Effects.Fx_SetPan, 0xc0 + i, 0, 0);
			}

			opaque.Xmp_Start_Player(44100, 0);

			// Set mix to 100% pan separation
			opaque.Xmp_Set_Player(Xmp_Player.Mix, 100);

			for (c_int i = 0; i < 64; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

				c_int pan0 = info.Channel_Info[0].Pan;
				c_int pan1 = info.Channel_Info[1].Pan;
				c_int pan2 = info.Channel_Info[2].Pan;
				c_int pan3 = info.Channel_Info[3].Pan;

				Assert.AreEqual(0x00 + i, pan0, "Pan error in channel 0");
				Assert.AreEqual(0x40 + i, pan1, "Pan error in channel 1");
				Assert.AreEqual(0x80 + i, pan2, "Pan error in channel 2");
				Assert.AreEqual(0xc0 + i, pan3, "Pan error in channel 3");

				opaque.Xmp_Play_Frame();
				opaque.Xmp_Play_Frame();
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
