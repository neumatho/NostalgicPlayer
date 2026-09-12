/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
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
		public void Test_Api_Scan_Module()
		{
			Xmp_Frame_Info info;

			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			// Try to scan before loading
			opaque.Xmp_Scan_Module();

			Create_Simple_Module(opaque, 2, 2);

			New_Event(opaque, 0, 0, 0, 0, 0, 0, Effects.Fx_Speed, 0x03, 0, 0);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_Speed, 0x1f, 0, 0);
			New_Event(opaque, 0, 2, 0, 0, 0, 0, Effects.Fx_Speed, 0x02, 0, 0);
			New_Event(opaque, 0, 3, 0, 0, 0, 0, Effects.Fx_Speed, 0x20, 0, 0);
			New_Event(opaque, 0, 4, 0, 0, 0, 0, Effects.Fx_Speed, 0x80, 0, 0);

			opaque.Xmp_Scan_Module();

			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < (3 + 0x1f + (3 * 2)); i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				Assert.AreEqual(5720, info.Total_Time, "Total time error");
			}

			opaque.Xmp_Release_Module();

			// Load something with an absurd number of sequences
			c_int ret = LoadModule(dataDirectory, "Scan_240_Seq.it", opaque);
			Assert.AreEqual(0, ret, "Load module");

			opaque.Xmp_Scan_Module();

			opaque.Xmp_Get_Module_Info(out Xmp_Module_Info mInfo);
			Assert.AreEqual(240, mInfo.Num_Sequences, "Should have 240 sequences");

			for (c_int i = 0; i < mInfo.Num_Sequences; i++)
				Assert.AreEqual(i, mInfo.Seq_Data[i].Entry_Point, "Entry point");

			opaque.Xmp_Release_Module();

			// Invalid patterns followed by valid pattern -> 1 sequence
			Create_Simple_Module(opaque, 2, 2);

			opaque.loadHelpers.LibXmp_Free_Scan();
			Set_Order(opaque, 0, 0x63);
			Set_Order(opaque, 1, Constants.Xmp_Mark_Skip);
			Set_Order(opaque, 2, 0);
			opaque.loadHelpers.LibXmp_Prepare_Scan();

			opaque.Xmp_Get_Module_Info(out mInfo);
			Assert.AreEqual(3, mInfo.Mod.Len, "Should have 3 positions");
			Assert.AreEqual(1, mInfo.Num_Sequences, "Should have 1 sequence");

			opaque.Xmp_Scan_Module();
			opaque.Xmp_Start_Player(Constants.Xmp_Min_SRate, Xmp_Format.Default);

			opaque.Xmp_Get_Module_Info(out mInfo);
			Assert.AreEqual(3, mInfo.Mod.Len, "Should have 3 positions");
			Assert.AreEqual(1, mInfo.Num_Sequences, "Should have 1 sequence");
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, ret, "Should play");

			opaque.Xmp_Restart_Module();
			opaque.Xmp_Set_Player(Xmp_Player.Mode, (c_int)Xmp_Mode.S3M);	// Rescan

			opaque.Xmp_Get_Module_Info(out mInfo);
			Assert.AreEqual(3, mInfo.Mod.Len, "Should have 3 positions");
			Assert.AreEqual(1, mInfo.Num_Sequences, "Should have 1 sequence");
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, ret, "Should play");

			opaque.Xmp_Release_Module();

			// Invalid patterns with no valid pattern -> 1 sequence
			Create_Simple_Module(opaque, 2, 2);

			opaque.loadHelpers.LibXmp_Free_Scan();
			Set_Order(opaque, 0, 0x12);
			Set_Order(opaque, 1, 0x34);
			Set_Order(opaque, 2, 0xde);
			opaque.loadHelpers.LibXmp_Prepare_Scan();

			opaque.Xmp_Get_Module_Info(out mInfo);
			// TODO: libxmp_prepare_scan is still clobbering length in this case
//			Assert.AreEqual(3, mInfo.Mod.Len, "Should have 3 positions");
			Assert.AreEqual(1, mInfo.Num_Sequences, "Should have 1 sequence");

			opaque.Xmp_Scan_Module();
			opaque.Xmp_Start_Player(Constants.Xmp_Min_SRate, Xmp_Format.Default);

			opaque.Xmp_Get_Module_Info(out mInfo);
//			Assert.AreEqual(3, mInfo.Mod.Len, "Should have 3 positions");
			Assert.AreEqual(1, mInfo.Num_Sequences, "Should have 1 sequence");
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(-(c_int)Xmp_Error.End, ret, "Nothing to play");

			opaque.Xmp_Restart_Module();
			opaque.Xmp_Set_Player(Xmp_Player.Mode, (c_int)Xmp_Mode.S3M);	// Rescan

			opaque.Xmp_Get_Module_Info(out mInfo);
//			Assert.AreEqual(3, mInfo.Mod.Len, "Should have 3 positions");
			Assert.AreEqual(1, mInfo.Num_Sequences, "Should have 1 sequence");
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(-(c_int)Xmp_Error.End, ret, "Nothing to play");

			opaque.Xmp_Release_Module();

			// Valid, end markers, valid -> 1 sequence MOD, 2 sequences S3M
			Create_Simple_Module(opaque, 2, 2);

			opaque.loadHelpers.LibXmp_Free_Scan();
			Set_Order(opaque, 0, 0);
			Set_Order(opaque, 1, Constants.Xmp_Mark_End);
			Set_Order(opaque, 2, 1);
			opaque.loadHelpers.LibXmp_Prepare_Scan();

			opaque.Xmp_Get_Module_Info(out mInfo);
			Assert.AreEqual(3, mInfo.Mod.Len, "Should have 3 positions");
			Assert.AreEqual(1, mInfo.Num_Sequences, "Should have 1 sequence");

			opaque.Xmp_Scan_Module();
			opaque.Xmp_Start_Player(Constants.Xmp_Min_SRate, Xmp_Format.Default);

			opaque.Xmp_Get_Module_Info(out mInfo);
			Assert.AreEqual(3, mInfo.Mod.Len, "Should have 3 positions");
			Assert.AreEqual(1, mInfo.Num_Sequences, "Should have 1 sequence");
			opaque.Xmp_Set_Position(1);
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, ret, "Should play");

			opaque.Xmp_Restart_Module();
			opaque.Xmp_Set_Player(Xmp_Player.Mode, (c_int)Xmp_Mode.S3M);	// Rescan

			opaque.Xmp_Get_Module_Info(out mInfo);
			Assert.AreEqual(3, mInfo.Mod.Len, "Should have 3 positions");
			Assert.AreEqual(2, mInfo.Num_Sequences, "Should have 2 sequences");
			opaque.Xmp_Set_Position(1);
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, ret, "Should play");

			opaque.Xmp_Release_Module();

			// End marker, valid -> 1 sequence MOD, 2 sequences S3M
			Create_Simple_Module(opaque, 2, 2);

			opaque.loadHelpers.LibXmp_Free_Scan();
			Set_Order(opaque, 0, Constants.Xmp_Mark_End);
			Set_Order(opaque, 1, Constants.Xmp_Mark_End);
			Set_Order(opaque, 2, 0);
			opaque.loadHelpers.LibXmp_Prepare_Scan();

			opaque.Xmp_Get_Module_Info(out mInfo);
			Assert.AreEqual(3, mInfo.Mod.Len, "Should have 3 positions");
			Assert.AreEqual(1, mInfo.Num_Sequences, "Should have 1 sequence");

			opaque.Xmp_Scan_Module();
			opaque.Xmp_Start_Player(Constants.Xmp_Min_SRate, Xmp_Format.Default);

			opaque.Xmp_Get_Module_Info(out mInfo);
			Assert.AreEqual(3, mInfo.Mod.Len, "Should have 3 positions");
			Assert.AreEqual(1, mInfo.Num_Sequences, "Should have 1 sequence");
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, ret, "Should play");

			opaque.Xmp_Restart_Module();
			opaque.Xmp_Set_Player(Xmp_Player.Mode, (c_int)Xmp_Mode.S3M);	// Rescan

			opaque.Xmp_Get_Module_Info(out mInfo);
			Assert.AreEqual(3, mInfo.Mod.Len, "Should have 3 positions");
			Assert.AreEqual(2, mInfo.Num_Sequences, "Should have 2 sequences");
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(-(c_int)Xmp_Error.End, ret, "Nothing to play");
			opaque.Xmp_Set_Position(1);
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, ret, "Gets added to sequence 1 apparently...");
			opaque.Xmp_Set_Position(2);
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, ret, "Should play");

			opaque.Xmp_Release_Module();

			// Invalid, end marker, valid -> 1 sequence MOD, 2 sequences S3M
			Create_Simple_Module(opaque, 2, 2);

			opaque.loadHelpers.LibXmp_Free_Scan();
			Set_Order(opaque, 0, 0x63);
			Set_Order(opaque, 1, Constants.Xmp_Mark_End);
			Set_Order(opaque, 2, 0);
			opaque.loadHelpers.LibXmp_Prepare_Scan();

			opaque.Xmp_Get_Module_Info(out mInfo);
			Assert.AreEqual(3, mInfo.Mod.Len, "Should have 3 positions");
			Assert.AreEqual(1, mInfo.Num_Sequences, "Should have 1 sequence");

			opaque.Xmp_Scan_Module();
			opaque.Xmp_Start_Player(Constants.Xmp_Min_SRate, Xmp_Format.Default);

			opaque.Xmp_Get_Module_Info(out mInfo);
			Assert.AreEqual(3, mInfo.Mod.Len, "Should have 3 positions");
			Assert.AreEqual(1, mInfo.Num_Sequences, "Should have 1 sequence");
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, ret, "Should play");

			opaque.Xmp_Restart_Module();
			opaque.Xmp_Set_Player(Xmp_Player.Mode, (c_int)Xmp_Mode.S3M);	// Rescan

			opaque.Xmp_Get_Module_Info(out mInfo);
			Assert.AreEqual(3, mInfo.Mod.Len, "Should have 3 positions");
			Assert.AreEqual(2, mInfo.Num_Sequences, "Should have 2 sequences");
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(-(c_int)Xmp_Error.End, ret, "Nothing to play");
			opaque.Xmp_Set_Position(1);
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(-(c_int)Xmp_Error.End, ret, "Nothing to play");
			opaque.Xmp_Set_Position(2);
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, ret, "Should play");

			opaque.Xmp_Release_Module();

			// Length 0 -> 1 sequence, OK, -XMP_END on xmp_play_frame
			Create_Simple_Module(opaque, 2, 2);

			opaque.loadHelpers.LibXmp_Free_Scan();
			opaque.Xmp_Get_Module_Info(out mInfo);
			mInfo.Mod.Len = 0;
			opaque.loadHelpers.LibXmp_Prepare_Scan();

			opaque.Xmp_Get_Module_Info(out mInfo);
			Assert.AreEqual(0, mInfo.Mod.Len, "Should have 0 positions");
			Assert.AreEqual(1, mInfo.Num_Sequences, "Should have 1 sequence");

			opaque.Xmp_Scan_Module();
			opaque.Xmp_Start_Player(Constants.Xmp_Min_SRate, Xmp_Format.Default);

			opaque.Xmp_Get_Module_Info(out mInfo);
			Assert.AreEqual(0, mInfo.Mod.Len, "Should have 0 positions");
			Assert.AreEqual(1, mInfo.Num_Sequences, "Should have 1 sequence");
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(-(c_int)Xmp_Error.End, ret, "Nothing to play");
			opaque.Xmp_Set_Position(1);
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(-(c_int)Xmp_Error.End, ret, "Nothing to play");

			opaque.Xmp_Restart_Module();
			opaque.Xmp_Set_Player(Xmp_Player.Mode, (c_int)Xmp_Mode.S3M);	// Rescan

			opaque.Xmp_Get_Module_Info(out mInfo);
			Assert.AreEqual(0, mInfo.Mod.Len, "Should have 0 positions");
			Assert.AreEqual(1, mInfo.Num_Sequences, "Should have 2 sequence");
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(-(c_int)Xmp_Error.End, ret, "Nothing to play");
			opaque.Xmp_Set_Position(1);
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(-(c_int)Xmp_Error.End, ret, "Nothing to play");

			opaque.Xmp_Release_Module();

			// Rescan causing sequence changes mid-playback: MOD -> S3M
			Create_Simple_Module(opaque, 2, 2);

			opaque.loadHelpers.LibXmp_Free_Scan();
			New_Event(opaque, 0, 0, 0, 0, 0, 0, Effects.Fx_Speed, 0x01, Effects.Fx_Break, 0);
			Set_Order(opaque, 0, 0);
			Set_Order(opaque, 1, 0xff);
			Set_Order(opaque, 2, 0);
			Set_Order(opaque, 3, 0xff);
			Set_Order(opaque, 4, 0);
			Set_Order(opaque, 5, 0xff);
			opaque.loadHelpers.LibXmp_Prepare_Scan();

			opaque.Xmp_Scan_Module();
			opaque.Xmp_Start_Player(Constants.Xmp_Min_SRate, Xmp_Format.Default);

			opaque.Xmp_Get_Module_Info(out mInfo);
			Assert.AreEqual(6, mInfo.Mod.Len, "Should have 6 positions");
			Assert.AreEqual(1, mInfo.Num_Sequences, "Should have 1 sequence");
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, ret, "Should play");
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(0, info.Pos, "Should be position 0");
			Assert.AreEqual(0, info.Sequence, "Should be sequence 0");
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, ret, "Should play");
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(2, info.Pos, "Should be position 2");
			Assert.AreEqual(0, info.Sequence, "Should be sequence 0");

			opaque.Xmp_Set_Player(Xmp_Player.Mode, (c_int)Xmp_Mode.S3M);	// Rescan

			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, ret, "Should play");
			opaque.Xmp_Get_Frame_Info(out info);
			// Note: prior to 4.7.3, this remained in sequence 0
			Assert.AreEqual(2, info.Pos, "Should be position 2");
			Assert.AreEqual(1, info.Sequence, "Should be sequence 1");

			// Rescan causing sequence changes mid-playback: S3M -> MOD
			opaque.Xmp_Set_Position(4);
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, ret, "Should play");
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(4, info.Pos, "Should be position 4");
			Assert.AreEqual(2, info.Sequence, "Should be sequence 2");
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, ret, "Should play");
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(4, info.Pos, "Should be position 4");
			Assert.AreEqual(2, info.Sequence, "Should be sequence 2");

			opaque.Xmp_Set_Player(Xmp_Player.Mode, (c_int)Xmp_Mode.Mod);	// Rescan

			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, ret, "Should play");
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(0, info.Pos, "Should be position 0");
			Assert.AreEqual(0, info.Sequence, "Should be sequence 0");

			opaque.Xmp_Release_Module();

			opaque.Xmp_Free_Context();
		}
	}
}
