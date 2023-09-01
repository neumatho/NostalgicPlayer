/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Effect
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Effect
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_2_Slide_Down()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Frame_Info info;

			Create_Simple_Module(opaque, 2, 2);

			// Standard pitch bend
			New_Event(opaque, 0, 0, 0, 84, 1, 0, 2, 2, 0, 0);
			for (c_int i = 1; i < 60; i++)
				New_Event(opaque, 0, i, 0, 0, 0, 0, 2, 0, 0, 0);

			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 60; i++)
			{
				c_int k = 113 + i * 10;

				for (c_int j = 0; j < 6; j++)
				{
					opaque.Xmp_Play_Frame();
					opaque.Xmp_Get_Frame_Info(out info);
					Assert.AreEqual(k + j * 2, Period(info), "Slide error");
				}
			}

			// Fine pitch bend
			opaque.Xmp_Restart_Module();

			New_Event(opaque, 0, 0, 0, 84, 1, 0, 2, 0xf2, 0, 0);
			for (c_int i = 1; i < 60; i++)
				New_Event(opaque, 0, i, 0, 0, 0, 0, 2, 0, 0, 0);

			// Check without fine slide quirk
			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(113, Period(info), "Slide error (no fine slide)");

			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(355, Period(info), "Slide error (no fine slide)");

			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(597, Period(info), "Slide error (no fine slide)");

			opaque.Xmp_Restart_Module();

			// Set fine slide quirk
			Set_Quirk(opaque, Quirk_Flag.FineFx, Read_Event.Mod);

			for (c_int i = 0; i < 60; i++)
			{
				c_int k = 113 + i * 2;

				for (c_int j = 0; j < 6; j++)
				{
					opaque.Xmp_Play_Frame();
					opaque.Xmp_Get_Frame_Info(out info);
					Assert.AreEqual(k + 2, Period(info), "Fine slide error");
				}
			}

			// Extra fine bend
			opaque.Xmp_Restart_Module();

			New_Event(opaque, 0, 0, 0, 84, 1, 0, 2, 0xe4, 0, 0);
			for (c_int i = 1; i < 60; i++)
				New_Event(opaque, 0, i, 0, 0, 0, 0, 2, 0, 0, 0);

			for (c_int i = 0; i < 60; i++)
			{
				c_int k = 113 + i * 1;

				for (c_int j = 0; j < 6; j++)
				{
					opaque.Xmp_Play_Frame();
					opaque.Xmp_Get_Frame_Info(out info);
					Assert.AreEqual(k + 1, Period(info), "Extra fine slide error");
				}
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
