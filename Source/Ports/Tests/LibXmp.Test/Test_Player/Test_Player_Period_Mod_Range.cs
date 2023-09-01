/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
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
		public void Test_Player_Period_Mod_Range()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Frame_Info info;

			Create_Simple_Module(opaque, 2, 2);

			opaque.Xmp_Start_Player(44100, 0);

			New_Event(opaque, 0, 0, 0, 50, 1, 0, 0x0f, 1, 0, 0);
			New_Event(opaque, 0, 1, 0, 49, 1, 0, 0, 0, 0, 0);
			New_Event(opaque, 0, 2, 0, 48, 1, 0, 0, 0, 0, 0);
			New_Event(opaque, 0, 3, 0, 83, 1, 0, 0, 0, 0, 0);
			New_Event(opaque, 0, 4, 0, 84, 1, 0, 0, 0, 0, 0);
			New_Event(opaque, 0, 5, 0, 85, 1, 0, 0, 0, 0, 0);

			// Test note limits
			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(808, Period(info), "Bad period");

			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(856, Period(info), "Bad period");

			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(907, Period(info), "Bad period");

			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(120, Period(info), "Bad period");

			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(113, Period(info), "Bad period");

			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(107, Period(info), "Bad period");

			opaque.Xmp_Restart_Module();

			// Test again with mod range on
			Set_Quirk(opaque, 0, Read_Event.Mod);
			Set_Period_Type(opaque, Ports.LibXmp.Containers.Common.Period.ModRng);

			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(808, Period(info), "Bad period");

			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(856, Period(info), "Bad period");

			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(856, Period(info), "Bad period");

			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(120, Period(info), "Bad period");

			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(113, Period(info), "Bad period");

			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(113, Period(info), "Bad period");

			opaque.Xmp_Restart_Module();

			// Test lower limit
			New_Event(opaque, 0, 0, 0, 49, 1, 0, 0x02, 1, 0, 0);

			for (c_int i = 1; i < 20; i++)
				New_Event(opaque, 0, i, 0, 0, 0, 0, 0x02, 1, 0, 0);

			for (c_int i = 0; i < 20 * 6; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				Assert.IsTrue(Period(info) <= 907, "Bad lower limit");
			}

			opaque.Xmp_Restart_Module();

			// Test upper limit
			New_Event(opaque, 0, 0, 0, 84, 1, 0, 0x01, 1, 0, 0);

			for (c_int i = 1; i < 20; i++)
				New_Event(opaque, 0, i, 0, 0, 0, 0, 0x01, 1, 0, 0);

			for (c_int i = 0; i < 20 * 6; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				Assert.IsTrue(Period(info) >= 108, "Bad upper limit");
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
