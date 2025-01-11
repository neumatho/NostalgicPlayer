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
		private static readonly c_int[] vals_GV =
		[
			32, 32, 32,		// Set as 0x20
			0, 0, 0,		// Set as 0x00
			64, 64, 64,		// Set as 0x40
			64, 64, 64,		// Set as 0x41
			64, 64, 64		// Set as 0x90
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_GlobalVol()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			Create_Simple_Module(opaque, 2, 2);

			New_Event(opaque, 0, 0, 0, 0, 0, 0, Effects.Fx_GlobalVol, 0x20, Effects.Fx_Speed, 3);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_GlobalVol, 0x00, 0, 0);
			New_Event(opaque, 0, 2, 0, 0, 0, 0, Effects.Fx_GlobalVol, 0x40, 0, 0);
			New_Event(opaque, 0, 3, 0, 0, 0, 0, Effects.Fx_GlobalVol, 0x41, 0, 0);
			New_Event(opaque, 0, 4, 0, 0, 0, 0, Effects.Fx_GlobalVol, 0x90, 0, 0);

			// Set format as IT so silent channels will be reset. Global
			// volume slides shouldn't be ignored!
			Set_Quirk(opaque, Quirk_Flag.It, Read_Event.It);

			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 5 * 3; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);
				Assert.AreEqual(vals_GV[i], info.Volume, "Global volume error");
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
