/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Path = System.IO.Path;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Api
{
	/// <summary>
	/// Test xmp_seek_time and xmp_seek_time_frame
	/// </summary>
	public partial class Test_Api
	{
		private record Pos_Row_Frame(c_int Pos, c_int Row, c_int Frame);

		private static readonly c_int[] pos_Ode2Ptk =
		[
			0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 
			2, 3, 3, 3, 3, 3, 3, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 
			5, 6, 6, 6, 6, 7, 7, 7, 7, 8, 8, 8, 8, 9, 9, 9, 9, 
			10, 10, 10, 11, 11, 11, 11, 12, 12, 12, 12, 13, 13, 
			13, 13, 14, 14, 14, 14, 15, 15, 15, 15, 16, 16, 16, 
			17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 
			17, 17, 17, 17, 17, 17, 17, 17, 17, 17
		];

		private static readonly c_int[] pos_Dlr =
		[
			0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 4, 4, 4, 4,
			5, 5, 5, 5, 6, 6, 6, 7, 7, 7, 7, 8, 8, 8, 8, 9, 9,
			9, 9, 10, 10, 10, 11, 11, 11, 11, 12, 12, 12, 12, 13,
			13, 13, 14, 14, 14, 14, 15, 15, 15, 15, 16, 16, 16,
			17, 17, 18, 18, 18, 18, 18, 18, 19, 19, 19, 20, 20,
			20, 20, 21, 21, 21, 21, 22, 22, 22, 22, 23, 23, 23,
			24, 24, 24, 24, 25, 25, 25, 25, 26, 26, 26, 27, 27
		];

		private static readonly c_int[] seek_Frame_Times =
		[
			0, -10, -10000, c_int.MinValue, 10000, 32767, c_int.MaxValue, 12345
		];

		private static readonly Pos_Row_Frame[] frame_Ode2Ptk =
		[
			new Pos_Row_Frame(0, 0, 0),
			new Pos_Row_Frame(0, 0, 0),
			new Pos_Row_Frame(0, 0, 0),
			new Pos_Row_Frame(0, 0, 0),
			new Pos_Row_Frame(2, 1, 2),
			new Pos_Row_Frame(5, 63, 1),
			new Pos_Row_Frame(17, 39, 7),
			new Pos_Row_Frame(2, 20, 5)
		];

		private static readonly Pos_Row_Frame[] frame_Dlr =
		[
			new Pos_Row_Frame(0, 0, 0),
			new Pos_Row_Frame(0, 0, 0),
			new Pos_Row_Frame(0, 0, 0),
			new Pos_Row_Frame(0, 0, 0),
			new Pos_Row_Frame(3, 13, 0),
			new Pos_Row_Frame(9, 23, 2),
			new Pos_Row_Frame(44, 62, 2),
			new Pos_Row_Frame(3, 53, 2)
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Api_Seek_Time()
		{
			// Seek ode2ptk
			Ports.LibXmp.LibXmp ctx = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			LoadModule(dataDirectory, "Ode2Ptk.mod", ctx);
			ctx.Xmp_Start_Player(8000, 0);

			Test_Seek_Orders(ctx, pos_Ode2Ptk, "ode2ptk");
			Test_Order_Times(ctx, "ode2ptk");
			Test_Seek_Frames(ctx, frame_Ode2Ptk, "ode2ptk");

			ctx.Xmp_Release_Module();
			ctx.Xmp_Free_Context();

			// Seek dans le rue
			ctx = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			LoadModule(Path.Combine(dataDirectory, "M"), "Xyce-Dans_La_Rue.xm", ctx);
			ctx.Xmp_Start_Player(8000, 0);

			Test_Seek_Orders(ctx, pos_Dlr, "dans_la_rue");
			Test_Order_Times(ctx, "dans_la_rue");
			Test_Seek_Frames(ctx, frame_Dlr, "dans_la_rue");

			ctx.Xmp_Release_Module();
			ctx.Xmp_Free_Context();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Seek_Orders(Ports.LibXmp.LibXmp opaque, c_int[] table, string test)
		{
			for (c_int i = 0; i < 100; i++)
			{
				c_int ret = opaque.Xmp_Seek_Time(i * 1000);
				Assert.AreEqual(table[i], ret, $"xmp_seek_time to {i}s failed in '{test}': expected ord {table[i]}, got {ret}");

				ret = opaque.Xmp_Seek_Time_Frame(i * 1000);
				Assert.AreEqual(table[i], ret, $"xmp_seek_time_frame to {i}s failed in '{test}': expected ord {table[i]}, got {ret}");
			}
		}



		/********************************************************************/
		/// <summary>
		/// Because all API seeks leave stale data in the row/frame vars,
		/// one frame needs to be played after every seek to get accurate
		/// info
		/// </summary>
		/********************************************************************/
		private void Test_Seek_Frames(Ports.LibXmp.LibXmp opaque, Pos_Row_Frame[] table, string test)
		{
			Xmp_Context ctx = GetContext(opaque);
			Module_Data m = ctx.M;
			Player_Data p = ctx.P;

			for (c_int i = 0; i < seek_Frame_Times.Length; i++)
			{
				c_int ret = opaque.Xmp_Seek_Time(seek_Frame_Times[i]);
				opaque.Xmp_Play_Frame();
				string buf = $"xmp_seek_time to {seek_Frame_Times[i]}s failed in '{test}': expected {table[i].Pos} {m.Xxo_Info[ret].Start_Row} 0, got {ret} {p.Row} {p.Frame}";
				Assert.AreEqual(table[i].Pos, ret, buf);
				Assert.AreEqual(m.Xxo_Info[ret].Start_Row, p.Row, buf);
				Assert.AreEqual(0, p.Frame, buf);

				ret = opaque.Xmp_Seek_Time_Frame(seek_Frame_Times[i]);
				opaque.Xmp_Play_Frame();
				buf = $"xmp_seek_time_frame to {seek_Frame_Times[i]}s failed in '{test}': expected {table[i].Pos} {table[i].Row} {table[i].Frame}, got {ret} {p.Row} {p.Frame}";
				Assert.AreEqual(table[i].Pos, ret, buf);
				Assert.AreEqual(table[i].Row, p.Row, buf);
				Assert.AreEqual(table[i].Frame, p.Frame, buf);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Order_Times(Ports.LibXmp.LibXmp opaque, string test)
		{
			Xmp_Context ctx = GetContext(opaque);
			Module_Data m = ctx.M;

			// Should be able to seek to the exact order time without issue.
			// For extremely long modules (longest.med), this won't seek to
			// the correct spot currently due to API limitations
			for (c_int i = 0; i < m.Mod.Len; i++)
			{
				// API expects rounded values due to having been
				// designed to handle int (rather than double)
				c_double time = m.Xxo_Info[i].Time;
				Common.Clamp(ref time, 0.0, c_int.MaxValue);

				c_int ret = opaque.Xmp_Seek_Time((c_int)time);

				// Seek (but don't check) overflowed times and markers
				// so they get sanitized
				if ((time >= c_int.MaxValue) || (m.Mod.Xxo[i] == Constants.Xmp_Mark_Skip) || (m.Mod.Xxo[i] == Constants.Xmp_Mark_End))
					continue;

				Assert.AreEqual(i, ret, $"time seek to {time} failed in '{test}' @ {i} (got {ret})");
			}
		}
		#endregion
	}
}
