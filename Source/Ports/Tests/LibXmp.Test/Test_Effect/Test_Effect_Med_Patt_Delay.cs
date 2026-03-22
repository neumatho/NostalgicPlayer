/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Effect
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Effect
	{
		private static readonly c_double[] times =
		[
			40.0,
			80.0,
			400.0,
			720.0,
			2320.0,
			3920.0,
			3940.0
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Med_Patt_Delay()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Context_Data ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Create_Simple_Module(opaque, 2, 2);

			New_Event(opaque, 0, 0, 0, 0, 0, 0, Effects.Fx_Patt_Delay, 0x01, 0, 0x00);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, 0, 0x00, Effects.Fx_Patt_Delay, 0x01);
			New_Event(opaque, 0, 2, 0, 0, 0, 0, Effects.Fx_Patt_Delay, 0x0f, 0, 0x00);
			New_Event(opaque, 0, 3, 0, 0, 0, 0, 0, 0x00, Effects.Fx_Patt_Delay, 0x0f);
			New_Event(opaque, 0, 4, 0, 0, 0, 0, Effects.Fx_Patt_Delay, 0x4f, 0, 0x00);
			New_Event(opaque, 0, 5, 0, 0, 0, 0, 0, 0x00, Effects.Fx_Patt_Delay, 0x4f);
			New_Event(opaque, 0, 6, 0, 0, 0, 0, 0, 0x00, Effects.Fx_Break, 0x00);

			opaque.loadHelpers.LibXmp_Free_Scan();

			ctx.M.Mod.Len = 1;
			ctx.M.Mod.Spd = 1;

			opaque.loadHelpers.LibXmp_Prepare_Scan();
			opaque.scan.LibXmp_Scan_Sequences();

			c_double time = times[times.Length - 1];
			Assert.AreEqual(time, p.Scan[0].Time, $"Scan duration mismatch: {p.Scan[0].Time} != {time}");

			opaque.Xmp_Start_Player(Constants.Xmp_Min_SRate, 0);

			for (c_int i = 0; i < times.Length; i++)
			{
				// Want the time from the end of the row--play
				// until the next row is reached, then use the
				// previous time
				c_int ret;

				do
				{
					time = p.Current_Time;
					ret = opaque.Xmp_Play_Frame();
				}
				while ((ret == 0) && (p.Row == i));

				Assert.AreEqual(times[i], time, $"Current time @ {i} mismatch: {time} != {times[i]}");
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
