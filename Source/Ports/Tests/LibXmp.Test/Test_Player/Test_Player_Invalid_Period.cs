/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player;
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
		public void Test_Player_Invalid_Period()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;
			Xmp_Frame_Info info;

			Create_Simple_Module(opaque, 2, 2);

			New_Event(opaque, 0, 0, 0, 49, 1, 0, 0, 0, 0, 0);

			opaque.Xmp_Start_Player(44100, 0);
			Channel_Data xc = p.Xc_Data[0];

			// Frame 0
			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);

			Assert.AreEqual(3506176U, info.channel_Info[0].Period, "Period error");
			Assert.AreEqual(64, info.channel_Info[0].Volume, "Volume error");

			// Frame 1
			xc.Period = 1;
			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);

			Assert.AreEqual(4096U, info.channel_Info[0].Period, "Period error");
			Assert.AreEqual(64, info.channel_Info[0].Volume, "Volume error");

			// Frame 2
			xc.Period = 0;
			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);

			Assert.AreEqual(4096U, info.channel_Info[0].Period, "Period error");
			Assert.AreEqual(64, info.channel_Info[0].Volume, "Volume error");

			// Frame 3 -- Periods are updated in update_frequency() so it
			// will appear one frame later
			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);

			Assert.AreEqual(4096U, info.channel_Info[0].Period, "Period error");
			Assert.AreEqual(0, info.channel_Info[0].Volume, "Volume error");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
