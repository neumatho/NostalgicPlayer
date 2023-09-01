/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		public void Test_Api_Set_Row()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Xmp_State state = (Xmp_State)opaque.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Unloaded, state, "State error");

			c_int ret = opaque.Xmp_Set_Row(0);
			Assert.AreEqual(-(c_int)Xmp_Error.State, ret, "State check error");

			Create_Simple_Module(opaque, 1, 1);
			opaque.loadHelpers.LibXmp_Free_Scan();
			Set_Order(opaque, 0, 0);
			Set_Order(opaque, 1, 0xff);		// End marker
			Set_Quirk(opaque, Quirk_Flag.Marker, Read_Event.It);

			opaque.loadHelpers.LibXmp_Prepare_Scan();
			opaque.scan.LibXmp_Scan_Sequences();

			opaque.Xmp_Start_Player(44100, Xmp_Format.Default);
			Assert.AreEqual(0, p.Row, "Didn't start at row 0");

			ret = opaque.Xmp_Set_Row(1);
			Assert.AreEqual(1, ret, "Return value error");
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(1, p.Row, "Didn't set row 1");

			ret = opaque.Xmp_Set_Row(64);
			Assert.AreEqual(-(c_int)Xmp_Error.Invalid, ret, "Return value error");

			// Go to end marker
			ret = opaque.Xmp_Set_Position(1);
			Assert.AreEqual(1, ret, "set_position error");

			// Set row on a marker should just fail (meaningless)
			ret = opaque.Xmp_Set_Row(0);
			Assert.AreEqual(-(c_int)Xmp_Error.Invalid, ret, "Set row at marker");

			opaque.Xmp_Free_Context();
		}
	}
}
