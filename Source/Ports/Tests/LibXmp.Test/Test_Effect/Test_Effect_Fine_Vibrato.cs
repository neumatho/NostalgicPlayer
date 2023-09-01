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
		private static readonly c_int[] vals_FVib =
		{
			143, 143, 143, 143, 144, 144,
			144, 144, 144, 144, 144, 144,
			254, 254, 254, 255, 255, 255,
			256, 256, 256, 255, 254, 253,
			453, 453, 454, 455, 456, 456,
			456, 456, 455, 454, 453, 452,
			808, 808, 810, 811, 810, 808,
			804, 804, 803, 804, 808, 812,
			1440, 1440, 1445, 1444, 1438, 1435,
			1438, 1438, 1446, 1444, 1435, 1435
		};

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Fine_Vibrato()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			Create_Simple_Module(opaque, 2, 2);

			New_Event(opaque, 0, 0, 0, 80, 1, 0, Effects.Fx_Fine_Vibrato, 0x24, 0, 0);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_Fine_Vibrato, 0x34, 0, 0);
			New_Event(opaque, 0, 2, 0, 70, 0, 0, Effects.Fx_Fine_Vibrato, 0x44, 0, 0);
			New_Event(opaque, 0, 3, 0, 0, 0, 0, Effects.Fx_Fine_Vibrato, 0x46, 0, 0);
			New_Event(opaque, 0, 4, 0, 60, 0, 0, Effects.Fx_Fine_Vibrato, 0x48, 0, 0);
			New_Event(opaque, 0, 5, 0, 0, 0, 0, Effects.Fx_Fine_Vibrato, 0x00, 0, 0);
			New_Event(opaque, 0, 6, 0, 50, 0, 0, Effects.Fx_Fine_Vibrato, 0x88, 0, 0);
			New_Event(opaque, 0, 7, 0, 0, 0, 0, Effects.Fx_Fine_Vibrato, 0x8c, 0, 0);
			New_Event(opaque, 0, 8, 0, 40, 0, 0, Effects.Fx_Fine_Vibrato, 0xcc, 0, 0);
			New_Event(opaque, 0, 9, 0, 0, 0, 0, Effects.Fx_Fine_Vibrato, 0xff, 0, 0);

			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 10 * 6; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);
				Assert.AreEqual(vals_FVib[i], Period(info), "Fine vibrato error");
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
