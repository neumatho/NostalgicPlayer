/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Api
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Api
	{
		private enum Set_Position_MidFx_Fns
		{
			Set_Position,
			Next_Position,
			Prev_Position
		}

		private class Set_Position_MidFx_Data
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Set_Position_MidFx_Data(c_int row, c_int frame, c_int note, c_int volume)
			{
				Row = row;
				Frame = frame;
				Note = note;
				Volume = volume;
			}

			public c_int Row;
			public c_int Frame;
			public c_int Note;
			public c_int Volume;
		}

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Api_Set_Position_MidFx()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			Do_Set_Position_MidFx(opaque, "Set_Position_Mid_Jump.it", Set_Position_MidFx_Fns.Set_Position, 0, 3, 1);
			Do_Set_Position_MidFx(opaque, "Set_Position_Mid_Jump.it", Set_Position_MidFx_Fns.Next_Position, 0, 3, 1);
			Do_Set_Position_MidFx(opaque, "Set_Position_Mid_Jump.it", Set_Position_MidFx_Fns.Prev_Position, 4, 3, 3);

			Do_Set_Position_MidFx(opaque, "Set_Position_Mid_Break.it", Set_Position_MidFx_Fns.Set_Position, 0, 3, 1);
			Do_Set_Position_MidFx(opaque, "Set_Position_Mid_Break.it", Set_Position_MidFx_Fns.Next_Position, 0, 3, 1);
			Do_Set_Position_MidFx(opaque, "Set_Position_Mid_Break.it", Set_Position_MidFx_Fns.Prev_Position, 2, 3, 1);

			Do_Set_Position_MidFx(opaque, "Set_Position_Mid_Loop.it", Set_Position_MidFx_Fns.Set_Position, 0, 3, 1);
			Do_Set_Position_MidFx(opaque, "Set_Position_Mid_Loop.it", Set_Position_MidFx_Fns.Next_Position, 0, 3, 1);
			Do_Set_Position_MidFx(opaque, "Set_Position_Mid_Loop.it", Set_Position_MidFx_Fns.Prev_Position, 2, 3, 1);

			// IT pattern delay uses f.RowDelay
			Do_Set_Position_MidFx(opaque, "Set_Position_Mid_PattDelay.it", Set_Position_MidFx_Fns.Set_Position, 0, 3, 1);
			Do_Set_Position_MidFx(opaque, "Set_Position_Mid_PattDelay.it", Set_Position_MidFx_Fns.Next_Position, 0, 3, 1);
			Do_Set_Position_MidFx(opaque, "Set_Position_Mid_PattDelay.it", Set_Position_MidFx_Fns.Prev_Position, 2, 3, 1);

			// Non-IT pattern delay uses f.Delay
			Do_Set_Position_MidFx(opaque, "Set_Position_Mid_PattDelay.xm", Set_Position_MidFx_Fns.Set_Position, 0, 3, 1);
			Do_Set_Position_MidFx(opaque, "Set_Position_Mid_PattDelay.xm", Set_Position_MidFx_Fns.Next_Position, 0, 3, 1);
			Do_Set_Position_MidFx(opaque, "Set_Position_Mid_PattDelay.xm", Set_Position_MidFx_Fns.Prev_Position, 2, 3, 1);

			opaque.Xmp_Free_Context();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Do_Set_Position_MidFx(Ports.LibXmp.LibXmp opaque, string fileName, Set_Position_MidFx_Fns fn_To_Use, c_int start_Order, c_int frame_To_Set_Position, c_int target_Order)
		{
			// The target order for each module should be the same:
			//
			// - 2 ticks of C-5.
			// - 2 ticks of C-6.
			// - 4 ticks of F-5.
			// - note cut.
			//
			// If it doesn't play exactly like that then some of the player flow
			// state persisted after the position change
			Set_Position_MidFx_Data[] data =
			{
				new Set_Position_MidFx_Data(0, 0, 60, 64),
				new Set_Position_MidFx_Data(0, 1, 60, 64),
				new Set_Position_MidFx_Data(1, 0, 72, 64),
				new Set_Position_MidFx_Data(1, 1, 72, 64),
				new Set_Position_MidFx_Data(2, 0, 65, 64),
				new Set_Position_MidFx_Data(2, 1, 65, 64),
				new Set_Position_MidFx_Data(3, 0, 65, 64),
				new Set_Position_MidFx_Data(3, 1, 65, 64),
				new Set_Position_MidFx_Data(4, 0, 65, 0),
				new Set_Position_MidFx_Data(0, 0, 0, 0)
			};

			c_int ret = LoadModule(dataDirectory, fileName, opaque);
			Assert.AreEqual(0, ret, "Load module");

			opaque.Xmp_Start_Player(44100, Xmp_Format.Default);

			ret = opaque.Xmp_Set_Position(start_Order);
			ret = ret > 0 ? ret : 0;	// Normalize -1 for start order to 0 for check
			Assert.AreEqual(start_Order, ret, "Initial set position");

			for (c_int i = 0; i < frame_To_Set_Position; i++)
				opaque.Xmp_Play_Frame();

			switch (fn_To_Use)
			{
				case Set_Position_MidFx_Fns.Set_Position:
				{
					ret = opaque.Xmp_Set_Position(target_Order);
					Assert.AreEqual(target_Order, ret, "xmp_set_position");
					break;
				}

				case Set_Position_MidFx_Fns.Next_Position:
				{
					ret = opaque.Xmp_Next_Position();
					Assert.AreEqual(target_Order, ret, "xmp_next_position");
					break;
				}

				case Set_Position_MidFx_Fns.Prev_Position:
				{
					ret = opaque.Xmp_Prev_Position();
					Assert.AreEqual(target_Order, ret, "xmp_prev_position");
					break;
				}

				default:
				{
					Assert.Fail("Invalid set position function");
					return;
				}
			}

			for (c_int i = 0; data[i].Note != 0; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

				Assert.AreEqual(target_Order, info.Pos, "Pos mismatch");
				Assert.AreEqual(data[i].Row, info.Row, "Row mismatch");
				Assert.AreEqual(data[i].Frame, info.Frame, "Frame mismatch");
				Assert.AreEqual(data[i].Note, info.Channel_Info[0].Note, "Note mismatch");
				Assert.AreEqual(data[i].Volume, info.Channel_Info[0].Volume, "Volume mismatch");
			}

			opaque.Xmp_Release_Module();
		}
	}
}
