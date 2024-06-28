/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Api
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Api
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Api_Set_Tempo_Factor()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;

			Xmp_State state = (Xmp_State)opaque.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Unloaded, state, "State error");

			Create_Simple_Module(opaque, 1, 1);
			opaque.loadHelpers.LibXmp_Free_Scan();
			Set_Order(opaque, 0, 0);

			New_Event(opaque, 0, 0, 0, 0, 0, 0, Effects.Fx_S3M_Bpm, 0x21, Effects.Fx_Speed, 1);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_S3M_Bpm, 0xff, Effects.Fx_Break, 0);
			m.Mod.Bpm = 255;	// Initial BPM

			opaque.loadHelpers.LibXmp_Prepare_Scan();
			opaque.scan.LibXmp_Scan_Sequences();

			// This function relies on values initialized by xmp_start_player
			c_int ret = opaque.Xmp_Set_Tempo_Factor(1.0);
			Assert.AreEqual(-(c_int)Xmp_Error.State, ret, "Should fail if not already playing");

			opaque.Xmp_Start_Player(44100, Xmp_Format.Default);
			Assert.AreEqual(0, p.Ord, "Didn't start at pattern 0");

			ret = opaque.Xmp_Set_Tempo_Factor(0.0);
			Assert.AreEqual(-1, ret, "Didn't fail to set tempo factor to 0.0");
			ret = opaque.Xmp_Set_Tempo_Factor(1000.0);
			Assert.AreEqual(-1, ret, "Didn't fail to set tempo factor to extreme value");
			ret = opaque.Xmp_Set_Tempo_Factor(-10.0);
			Assert.AreEqual(-1, ret, "Didn't fail to set tempo factor to negative value");

			ret = opaque.Xmp_Set_Tempo_Factor(c_double.PositiveInfinity);
			Assert.AreEqual(-1, ret, "Didn't fail to set tempo factor to infinity");
			ret = opaque.Xmp_Set_Tempo_Factor(c_double.NegativeInfinity);
			Assert.AreEqual(-1, ret, "Didn't fail to set tempo factor to negative infinity");

			ret = opaque.Xmp_Set_Tempo_Factor(c_double.NaN);
			Assert.AreEqual(-1, ret, "Didn't fail to set tempo factor to NaN");
			ret = opaque.Xmp_Set_Tempo_Factor(-c_double.NaN);
			Assert.AreEqual(-1, ret, "Didn't fail to set tempo factor to -NaN");

			// It should always work with reasonable tempo factors
			c_double factor;
			for (factor = 0.1; factor <= 2.0; factor += 0.1)
			{
				// bpm = 255
				ret = opaque.Xmp_Set_Tempo_Factor(factor);
				Assert.AreEqual(0, ret, "Failed to set tempo factor (0)");
				opaque.Xmp_Play_Frame();
				Assert.AreEqual(0, p.Row, "Didn't play frame (0)");

				// bpm = 33
				ret = opaque.Xmp_Set_Tempo_Factor(factor);
				Assert.AreEqual(0, ret, "Failed to set tempo factor (1)");
				opaque.Xmp_Play_Frame();
				Assert.AreEqual(1, p.Row, "Didn't play frame (1)");
			}

			// Anything is fine here, as long as the mixer doesn't crash
			for (; factor <= 128.0; factor *= 2.0)
			{
				// bpm = 255
				opaque.Xmp_Set_Tempo_Factor(factor);
				opaque.Xmp_Play_Frame();
				Assert.AreEqual(0, p.Row, "Didn't play frame (0)");

				// bpm = 33
				opaque.Xmp_Set_Tempo_Factor(factor);
				opaque.Xmp_Play_Frame();
				Assert.AreEqual(1, p.Row, "Didn't play frame (1)");
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
