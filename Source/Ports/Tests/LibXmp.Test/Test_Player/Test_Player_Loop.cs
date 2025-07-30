/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Player
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Player
	{
		private const c_int It_Skip = 0xfe;
		private const c_int It_End = 0xff;

		private struct Test_Seq
		{
			public c_int Entry { get; set; }
			public c_int Ticks { get; set; }
		}

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Loop()
		{
			Test_Seq[] test_Seq = new Test_Seq[16];
			Xmp_Frame_Info info;

			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);

			Create_Simple_Module(opaque, 1, 16);
			opaque.loadHelpers.LibXmp_Free_Scan();
			Set_Quirk(opaque, Quirk_Flag.Marker, Read_Event.It);

			c_int pat = 0;
			c_int pos = 0;
			c_int seq = 0;

			// Main sequence
			New_Event(opaque, pat, 0, 0, 0, 0, 0, Effects.Fx_Break, 0, 0, 0);
			test_Seq[seq].Entry = pos;
			test_Seq[seq].Ticks = 6;
			Set_Order(opaque, pos++, pat++);
			c_int remote_End = pos;			// For use by later sequence
			Set_Order(opaque, pos++, It_End);
			seq++;

			// Sequence: pattern, end
			New_Event(opaque, pat, 0, 0, 0, 0, 0, Effects.Fx_Break, 0, 0, 0);
			test_Seq[seq].Entry = pos;
			test_Seq[seq].Ticks = 6;
			Set_Order(opaque, pos++, pat++);
			Set_Order(opaque, pos++, It_End);
			seq++;

			// Sequence: skip, pattern, end
			New_Event(opaque, pat, 0, 0, 0, 0, 0, Effects.Fx_Break, 0, 0, 0);
			test_Seq[seq].Entry = pos + 1;
			test_Seq[seq].Ticks = 6;
			Set_Order(opaque, pos++, It_Skip);
			Set_Order(opaque, pos++, pat++);
			Set_Order(opaque, pos++, It_End);
			seq++;

			// Sequence: pattern jump into end marker
			New_Event(opaque, pat, 0, 0, 0, 0, 0, Effects.Fx_Jump, remote_End, 0, 0);
			test_Seq[seq].Entry = pos;
			test_Seq[seq].Ticks = 6;
			Set_Order(opaque, pos++, pat++);
			Set_Order(opaque, pos++, It_End);
			seq++;

			// Sequence: jump into skip into end. This should be last
			c_int jump_Skip_End = pat;
			test_Seq[seq].Entry = pos;
			test_Seq[seq].Ticks = 6;
			Set_Order(opaque, pos++, pat++);
			seq++;

			// Sequence: do it again, because the reuse is what caused a bug
			test_Seq[seq].Entry = pos;
			test_Seq[seq].Ticks = 6;
			Set_Order(opaque, pos++, jump_Skip_End);
			seq++;

			// Target
			New_Event(opaque, jump_Skip_End, 0, 0, 0, 0, 0, Effects.Fx_Jump, pos, 0, 0);
			Set_Order(opaque, pos++, It_Skip);
			// Intentionally no terminating IT_END

			ctx.M.Mod.Len = pos;
			opaque.loadHelpers.LibXmp_Prepare_Scan();
			opaque.scan.LibXmp_Scan_Sequences();

			c_int ret = opaque.Xmp_Start_Player(Constants.Xmp_Min_SRate, 0);
			Assert.AreEqual(0, ret, "Failed to start player");

			for (c_int i = 0; i < seq; i++)
			{
				opaque.Xmp_Restart_Module();

				ret = opaque.Xmp_Set_Position(test_Seq[i].Entry);

				if (i > 0)
					Assert.AreEqual(test_Seq[i].Entry, ret, "Failed to set position");
				else
					Assert.AreEqual(-1, ret, "Failed to set position");

				opaque.Xmp_Get_Frame_Info(out info);
				Assert.AreEqual(i, info.Sequence, "Entered wrong sequence");

				ctx.P.Loop_Count = 0;

				for (c_int j = 0; j <= test_Seq[i].Ticks; j++)
				{
					Assert.AreEqual(0, info.Loop_Count, "Loop occurred too early");

					opaque.Xmp_Play_Frame();

					opaque.Xmp_Get_Frame_Info(out info);
					Assert.AreEqual(i, info.Sequence, "Wrong sequence");
				}

				Assert.AreEqual(1, info.Loop_Count, "Failed to detect loop");
			}

			opaque.Xmp_Free_Context();
		}
	}
}
