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
		private static readonly c_int[] vals_Pan =
		[
			128, 173, 173, 173, 173, 173,
			173, 212, 212, 212, 212, 212,
			212, 238, 238, 238, 238, 238,
			238, 247, 247, 247, 247, 247,

			159, 150, 150, 150, 150, 150,
			150, 128, 128, 128, 128, 128,
			128, 128, 128, 128, 128, 128,
			128, 106, 106, 106, 106, 106
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Panbrello()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);

			Create_Simple_Module(opaque, 2, 2);

			// Set channel pan
			for (c_int i = 0; i < 4; i++)
				ctx.M.Mod.Xxc[i].Pan = 0x80;

			// Set event pan
			New_Event(opaque, 0, 0, 0, 61, 1, 0, Effects.Fx_Panbrello, 0x4f, 0, 0);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_Panbrello, 0x00, 0, 0);
			New_Event(opaque, 0, 2, 0, 0, 0, 0, Effects.Fx_Panbrello, 0x00, 0, 0);
			New_Event(opaque, 0, 3, 0, 0, 0, 0, Effects.Fx_Panbrello, 0x00, 0, 0);
			New_Event(opaque, 0, 4, 0, 0, 0, 0, Effects.Fx_Panbrello, 0x84, 0, 0);
			New_Event(opaque, 0, 5, 0, 0, 0, 0, Effects.Fx_Panbrello, 0x00, 0, 0);
			New_Event(opaque, 0, 6, 0, 0, 0, 0, 0, 0x00, 0, 0);
			New_Event(opaque, 0, 7, 0, 0, 0, 0, Effects.Fx_Panbrello, 0x00, 0, 0);

			opaque.Xmp_Start_Player(44100, 0);

			// Set mix to 100% pan separation
			opaque.Xmp_Set_Player(Xmp_Player.Mix, 100);

			for (c_int i = 0; i < 48; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

				Assert.AreEqual(vals_Pan[i], info.Channel_Info[0].Pan, "Pan error");
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
