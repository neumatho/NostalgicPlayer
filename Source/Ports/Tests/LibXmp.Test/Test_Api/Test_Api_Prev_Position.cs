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
		public void Test_Api_Prev_Position()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Xmp_State state = (Xmp_State)opaque.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Unloaded, state, "State error");

			c_int ret = opaque.Xmp_Prev_Position();
			Assert.AreEqual(-(c_int)Xmp_Error.State, ret, "State check error");

			Create_Simple_Module(opaque, 2, 2);
			opaque.loadHelpers.LibXmp_Free_Scan();
			Set_Order(opaque, 0, 0);
			Set_Order(opaque, 1, 1);
			Set_Order(opaque, 2, 0);

			opaque.loadHelpers.LibXmp_Prepare_Scan();
			opaque.scan.LibXmp_Scan_Sequences();

			opaque.Xmp_Start_Player(44100, Xmp_Format.Default);
			Assert.AreEqual(0, p.Ord, "Didn't start at pattern 0");

			ret = opaque.Xmp_Prev_Position();
			Assert.AreEqual(0, ret, "Prev position error");
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, p.Ord, "Not in position 0");

			opaque.Xmp_Set_Position(2);
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(2, p.Ord, "Didn't set position 2");

			ret = opaque.Xmp_Prev_Position();
			Assert.AreEqual(1, ret, "Prev position error");
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(1, p.Ord, "Didn't change to prev position");

			// xmp_prev_position should not restart a stopped module
			opaque.Xmp_Stop_Module();
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(-(c_int)Xmp_Error.End, ret, "Didn't stop module");
			ret = opaque.Xmp_Prev_Position();
			Assert.AreEqual(0, ret, "Not in position 0");
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(-(c_int)Xmp_Error.End, ret, "Module should still be stopped");

			// xmp_prev_position should not be able to seek backwards from restart
			opaque.Xmp_Restart_Module();
			ret = opaque.Xmp_Prev_Position();
			Assert.AreEqual(0, ret, "Didn't change to position 0");
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, p.Ord, "Not in position 0");

			// xmp_prev_position should not be confused by a skip marker
			// at position 0
			opaque.Xmp_End_Player();

			opaque.loadHelpers.LibXmp_Free_Scan();
			Set_Order(opaque, 0, Constants.Xmp_Mark_Skip);
			Set_Order(opaque, 1, 0);
			Set_Quirk(opaque, Quirk_Flag.Marker, Read_Event.It);

			opaque.loadHelpers.LibXmp_Prepare_Scan();
			opaque.scan.LibXmp_Scan_Sequences();

			opaque.Xmp_Start_Player(44100, Xmp_Format.Default);		// Skip marker position
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(1, p.Ord, "Didn't start at pattern 1");
			ret = opaque.Xmp_Prev_Position();
			Assert.AreEqual(0, ret, "Prev position error");
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(1, p.Ord, "Not in position 1");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
