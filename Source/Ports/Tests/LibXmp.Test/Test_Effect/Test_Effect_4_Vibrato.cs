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
		private static readonly c_int[] vals_4 =
		{
			143, 143, 144, 146, 147, 148,
			149, 149, 150, 150, 150, 149,
			254, 254, 257, 259, 261, 261,
			265, 265, 262, 258, 254, 250,
			453, 453, 459, 464, 467, 468,
			467, 467, 464, 459, 453, 447,
			808, 808, 819, 823, 819, 808,
			792, 792, 785, 792, 808, 824,
			1440, 1440, 1462, 1456, 1431, 1417,
			1429, 1429, 1466, 1456, 1417, 1419
		};

		private static readonly c_int[] vals2_4 =
		{
			143, 144, 146, 147, 148, 149,
			150, 150, 150, 150, 148, 146,
			254, 257, 259, 261, 261, 261,
			262, 258, 254, 250, 246, 243,
			453, 459, 464, 467, 468, 467,
			464, 459, 453, 447, 442, 439,
			808, 819, 823, 819, 808, 797,
			785, 792, 808, 824, 831, 824,
			1440, 1462, 1456, 1431, 1417, 1431,
			1461, 1463, 1424, 1414, 1451, 1468
		};

		private static readonly c_int[] vals3_4 =
		{
			143, 143, 144, 145, 145, 146,
			146, 146, 146, 146, 145, 144,
			254, 255, 256, 257, 257, 257,
			258, 256, 254, 252, 250, 249,
			453, 456, 458, 460, 460, 460,
			458, 456, 453, 450, 448, 446,
			808, 813, 815, 813, 808, 803,
			797, 800, 808, 816, 819, 816,
			1440, 1451, 1448, 1436, 1429, 1436,
			1450, 1451, 1432, 1427, 1445, 1454
		};

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_4_Vibrato()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Frame_Info info;

			Create_Simple_Module(opaque, 2, 2);

			New_Event(opaque, 0, 0, 0, 80, 1, 0, Effects.Fx_Vibrato, 0x24, 0, 0);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_Vibrato, 0x34, 0, 0);
			New_Event(opaque, 0, 2, 0, 70, 0, 0, Effects.Fx_Vibrato, 0x44, 0, 0);
			New_Event(opaque, 0, 3, 0, 0, 0, 0, Effects.Fx_Vibrato, 0x46, 0, 0);
			New_Event(opaque, 0, 4, 0, 60, 0, 0, Effects.Fx_Vibrato, 0x48, 0, 0);
			New_Event(opaque, 0, 5, 0, 0, 0, 0, Effects.Fx_Vibrato, 0x00, 0, 0);
			New_Event(opaque, 0, 6, 0, 50, 0, 0, Effects.Fx_Vibrato, 0x88, 0, 0);
			New_Event(opaque, 0, 7, 0, 0, 0, 0, Effects.Fx_Vibrato, 0x8c, 0, 0);
			New_Event(opaque, 0, 8, 0, 40, 0, 0, Effects.Fx_Vibrato, 0xcc, 0, 0);
			New_Event(opaque, 0, 9, 0, 0, 0, 0, Effects.Fx_Vibrato, 0xff, 0, 0);

			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 10 * 6; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				Assert.AreEqual(vals_4[i], Period(info), "Vibrato error");
			}

			// Check vibrato in all frames flag
			opaque.Xmp_Restart_Module();
			Set_Quirk(opaque, Quirk_Flag.VibAll, Read_Event.Mod);

			for (c_int i = 0; i < 10 * 6; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				Assert.AreEqual(vals2_4[i], Period(info), "Vibrato error");
			}

			// Check deep vibrato flag
			opaque.Xmp_Restart_Module();
			Set_Quirk(opaque, Quirk_Flag.VibHalf, Read_Event.Mod);

			for (c_int i = 0; i < 10 * 6; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				Assert.AreEqual(vals3_4[i], Period(info), "Half vibrato error");
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
