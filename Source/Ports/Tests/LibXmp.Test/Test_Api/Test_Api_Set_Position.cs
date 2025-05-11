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
		public void Test_Api_Set_Position()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Xmp_State state = (Xmp_State)opaque.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Unloaded, state, "State error");

			c_int ret = opaque.Xmp_Set_Position(0);
			Assert.AreEqual(-(c_int)Xmp_Error.State, ret, "State check error");

			Create_Simple_Module(opaque, 2, 2);
			opaque.loadHelpers.LibXmp_Free_Scan();
			Set_Order(opaque, 0, 0);
			Set_Order(opaque, 1, 1);
			Set_Order(opaque, 2, 0);
			Set_Order(opaque, 3, Constants.Xmp_Mark_End);	// End marker
			Set_Quirk(opaque, Quirk_Flag.Marker, Read_Event.It);

			opaque.loadHelpers.LibXmp_Prepare_Scan();
			opaque.scan.LibXmp_Scan_Sequences();

			opaque.Xmp_Start_Player(44100, Xmp_Format.Default);
			Assert.AreEqual(0, p.Ord, "Didn't start at pattern 0");

			ret = opaque.Xmp_Set_Position(2);
			Assert.AreEqual(2, ret, "Return value error");
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(2, p.Ord, "Didn't start at pattern 2");

			ret = opaque.Xmp_Set_Position(3);
			Assert.AreEqual(3, ret, "Return value error (marker position)");
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, p.Ord, "Didn't wrap to position 0");

			// xmp_set_position restarts a stopped module
			opaque.Xmp_Stop_Module();
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(-(c_int)Xmp_Error.End, ret, "Didn't stop module");
			ret = opaque.Xmp_Set_Position(0);
			Assert.AreEqual(-1, ret, "Didn't set position to -1");
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, p.Ord, "Not in position 0");
			opaque.Xmp_Stop_Module();
			ret = opaque.Xmp_Play_Frame();
			Assert.AreEqual(-(c_int)Xmp_Error.End, ret, "Didn't stop module");
			ret = opaque.Xmp_Set_Position(2);
			Assert.AreEqual(2, ret, "Didn't set position to 2");
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(2, p.Ord, "Not in position 2");

			// xmp_set_position works with a restarting module
			opaque.Xmp_Restart_Module();
			ret = opaque.Xmp_Set_Position(1);
			Assert.AreEqual(1, ret, "Didn't set position to 1");
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(1, p.Ord, "Not in position 1");

			ret = opaque.Xmp_Set_Position(4);
			Assert.AreEqual(-(c_int)Xmp_Error.Invalid, ret, "Return value error");
			ret = opaque.Xmp_Set_Position(256);
			Assert.AreEqual(-(c_int)Xmp_Error.Invalid, ret, "Return value error");
			ret = opaque.Xmp_Set_Position(c_int.MaxValue);
			Assert.AreEqual(-(c_int)Xmp_Error.Invalid, ret, "Return value error");
			ret = opaque.Xmp_Set_Position(-1);
			Assert.AreEqual(-(c_int)Xmp_Error.Invalid, ret, "Return value error");
			ret = opaque.Xmp_Set_Position(-256);
			Assert.AreEqual(-(c_int)Xmp_Error.Invalid, ret, "Return value error");
			ret = opaque.Xmp_Set_Position(c_int.MinValue);
			Assert.AreEqual(-(c_int)Xmp_Error.Invalid, ret, "Return value error");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
