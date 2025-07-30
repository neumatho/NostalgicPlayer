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
		private struct Pos_Row
		{
			public c_int Pos { get; set; }
			public c_int Row { get; set; }
		}

		private static readonly Pos_Row[] vals_LJ =
		[
			new Pos_Row { Pos = 0, Row = 0 },		// Jump to 16 (fwd)
			new Pos_Row { Pos = 0, Row = 16 },		// Jump to 12 (back)
			new Pos_Row { Pos = 0, Row = 12 },
			new Pos_Row { Pos = 0, Row = 13 },
			new Pos_Row { Pos = 0, Row = 14 },
			new Pos_Row { Pos = 0, Row = 15 },		// Break, then line jump: break with line jump target
			new Pos_Row { Pos = 1, Row = 0 },		// Jump to 63 (last valid)
			new Pos_Row { Pos = 1, Row = 63 },
			new Pos_Row { Pos = 2, Row = 0 },		// Pattern jump then line jump: pattern jump with line jump target
			new Pos_Row { Pos = 3, Row = 16 },		// Line jump then break: line jump to the break target
			new Pos_Row { Pos = 3, Row = 63 },
			new Pos_Row { Pos = 4, Row = 0 },		// Line jump then pattern jump: pattern jump to line 0
			new Pos_Row { Pos = 5, Row = 0 },
			new Pos_Row { Pos = 6, Row = 0 },		// Infinite loop -- libxmp should exit eventually
			new Pos_Row { Pos = 6, Row = 0 },
			new Pos_Row { Pos = 6, Row = 0 },
			new Pos_Row { Pos = 6, Row = 0 }
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Line_Jump()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			Create_Simple_Module(opaque, 1, 7);

			New_Event(opaque, 0, 0, 0, 0, 0, 0, Effects.Fx_Line_Jump, 0x10, Effects.Fx_Speed, 1);
			New_Event(opaque, 0, 15, 0, 0, 0, 0, Effects.Fx_It_Break, 0x20, 0, 0);
			New_Event(opaque, 0, 15, 1, 0, 0, 0, Effects.Fx_Line_Jump, 0x00, 0, 0);
			New_Event(opaque, 0, 16, 0, 0, 0, 0, Effects.Fx_Line_Jump, 0x0c, 0, 0);
			New_Event(opaque, 1, 0, 0, 0, 0, 0, Effects.Fx_Line_Jump, 0x3f, 0, 0);
			New_Event(opaque, 2, 0, 0, 0, 0, 0, Effects.Fx_Jump, 0x03, 0, 0);
			New_Event(opaque, 2, 0, 1, 0, 0, 0, Effects.Fx_Line_Jump, 0x10, 0, 0);
			New_Event(opaque, 3, 16, 0, 0, 0, 0, Effects.Fx_Line_Jump, 0x20, 0, 0);
			New_Event(opaque, 3, 16, 1, 0, 0, 0, Effects.Fx_It_Break, 0x3f, 0, 0);
			New_Event(opaque, 4, 0, 0, 0, 0, 0, Effects.Fx_Line_Jump, 0x3f, 0, 0);
			New_Event(opaque, 4, 0, 1, 0, 0, 0, Effects.Fx_Jump, 0x05, 0, 0);
			New_Event(opaque, 5, 0, 0, 0, 0, 0, Effects.Fx_It_Break, 0x00, 0, 0);
			New_Event(opaque, 6, 0, 0, 0, 0, 0, Effects.Fx_Line_Jump, 0x00, 0, 0);

			opaque.loadHelpers.LibXmp_Free_Scan();

			for (c_int i = 0; i < 7; i++)
				Set_Order(opaque, i, i);

			opaque.loadHelpers.LibXmp_Prepare_Scan();
			c_int ret = opaque.scan.LibXmp_Scan_Sequences();
			Assert.AreEqual(0, ret, "Scan error");

			opaque.Xmp_Start_Player(8000, 0);

			for (c_int i = 0; i < vals_LJ.Length; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);
				Assert.AreEqual(vals_LJ[i].Pos, info.Pos, "Line jump error");
				Assert.AreEqual(vals_LJ[i].Row, info.Row, "Line jump error");
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
