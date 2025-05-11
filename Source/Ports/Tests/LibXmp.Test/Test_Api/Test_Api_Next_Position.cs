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
		public void Test_Api_Next_Position()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Xmp_State state = (Xmp_State)opaque.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Unloaded, state, "State error");

			c_int ret = opaque.Xmp_Next_Position();
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

			ret = opaque.Xmp_Next_Position();
			Assert.AreEqual(1, ret, "Next position error");
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(1, p.Ord, "Didn't change to next position");

			opaque.Xmp_Set_Position(2);
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(2, p.Ord, "Didn't set position 2");

			ret = opaque.Xmp_Next_Position();
			Assert.AreEqual(2, ret, "Not in position 2");

			// xmp_next_position should restart a stopped module
			opaque.Xmp_Stop_Module();
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(-(c_int)Xmp_Error.End, ret, "Didn't stop module");
			ret = opaque.Xmp_Next_Position();
			Assert.AreEqual(-1, ret, "Didn't change to position -1");
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, p.Ord, "Not in position 0");

			// xmp_next_position should not advance a restarting module
			opaque.Xmp_Restart_Module();
			ret = opaque.Xmp_Next_Position();
			Assert.AreEqual(-1, ret, "Not at position -1");
			ret = opaque.Xmp_Next_Position();
			Assert.AreEqual(-1, ret, "Not at position -1");
			ret = opaque.Xmp_Next_Position();
			Assert.AreEqual(-1, ret, "Not at position -1");
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, p.Ord, "Not in position 0");

			// xmp_next_position should not be confused by a module
			// with 256 positions
			opaque.Xmp_End_Player();

			opaque.loadHelpers.LibXmp_Free_Scan();
			Set_Order(opaque, 0, 0);

			for (c_int i = 1; i < 256; i++)
				Set_Order(opaque, i, Constants.Xmp_Mark_Skip);

			Set_Quirk(opaque, Quirk_Flag.Marker, Read_Event.It);

			opaque.loadHelpers.LibXmp_Prepare_Scan();
			opaque.scan.LibXmp_Scan_Sequences();

			opaque.Xmp_Start_Player(44100, Xmp_Format.Default);		// Skip marker position
			Assert.AreEqual(0, p.Ord, "Didn't start at pattern 0");
			ret = opaque.Xmp_Next_Position();
			Assert.AreEqual(255, ret, "Next position error");
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, p.Ord, "Not in position 0");
			ret = opaque.Xmp_Next_Position();
			Assert.AreEqual(255, ret, "Next position error");
			ret = opaque.Xmp_Next_Position();
			Assert.AreEqual(255, ret, "Not in position 255");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
