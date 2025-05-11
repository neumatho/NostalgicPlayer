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

			opaque.loadHelpers.LibXmp_Prepare_Scan();
			opaque.scan.LibXmp_Scan_Sequences();

			// This function relies on values initialized by xmp_start_player
			c_int ret = opaque.Xmp_Set_Tempo_Factor(1.0);
			Assert.AreEqual(-(c_int)Xmp_Error.State, ret, "Should fail if not already playing");

			opaque.Xmp_Start_Player(44100, Xmp_Format.Default);
			Assert.AreEqual(0, p.Ord, "Didn't start at pattern 0");

			ret = opaque.Xmp_Set_Tempo_Factor(1.0);
			Assert.AreEqual(0, ret, "Should set to 1.0");

			// Test xmp_set_tempo_factor's interactions with the current
			// playback time. Play a few frames so the time is non-zero
			for (c_int i = 0; i < 3; i++)
				opaque.Xmp_Play_Frame();

			opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info prev);
			ret = opaque.Xmp_Set_Tempo_Factor(2.0);
			Assert.AreEqual(0, ret, "Should set to 2.0");
			opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);
			Assert.AreEqual(prev.Time, info.Time, "Time should be the same prior to rescan");
			Assert.AreEqual(prev.Total_Time, info.Total_Time, "Total time should be the same prior to rescan");
			Assert.AreEqual(prev.Frame_Time, info.Frame_Time, "Frame time should be the same prior to rescan");
			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(prev.Total_Time, info.Total_Time, "Total time should be the same prior to rescan (2)");
			Assert.AreEqual(prev.Frame_Time, info.Frame_Time, "Frame time should be the same prior to rescan (2)");
			prev = info;
			opaque.Xmp_Scan_Module();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(prev.Time, info.Time / 2, "Time should be double after rescan");
			Assert.AreEqual(prev.Total_Time, info.Total_Time / 2, "Total time should be double after rescan");
			Assert.AreEqual(prev.Frame_Time, info.Frame_Time / 2, "Frame time should be double after rescan");
			ret = opaque.Xmp_Set_Tempo_Factor(0.5);
			Assert.AreEqual(0, ret, "Should set to 0.5");
			opaque.Xmp_Scan_Module();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(prev.Time / 2, info.Time, "Time should be half after rescan");
			Assert.AreEqual(prev.Total_Time / 2, info.Total_Time, "Total time should be half after rescan");
			Assert.AreEqual(prev.Frame_Time / 2, info.Frame_Time, "Frame time should be half after rescan");

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

			// Set oscillating BPMs to guarantee correct function at extremes
			opaque.loadHelpers.LibXmp_Free_Scan();

			New_Event(opaque, 0, 0, 0, 0, 0, 0, Effects.Fx_S3M_Bpm, 0x21, Effects.Fx_Speed, 1);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_S3M_Bpm, 0xff, Effects.Fx_Break, 0);
			m.Mod.Bpm = 255;    // Initial BPM

			opaque.loadHelpers.LibXmp_Prepare_Scan();
			opaque.scan.LibXmp_Scan_Sequences();
			opaque.Xmp_Restart_Module();

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
