/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Prev_Order
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Prev_Order
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Prev_Order_Skip()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Create_Simple_Module(opaque, 2, 2);
			Set_Quirk(opaque, Quirk_Flag.Marker, Read_Event.Mod);

			opaque.loadHelpers.LibXmp_Free_Scan();

			Set_Order(opaque, 0, 0);
			Set_Order(opaque, 1, 0xfe);
			Set_Order(opaque, 2, 0);
			Set_Order(opaque, 3, 0xff);
			Set_Order(opaque, 4, 1);

			opaque.loadHelpers.LibXmp_Prepare_Scan();
			opaque.scan.LibXmp_Scan_Sequences();

			opaque.Xmp_Start_Player(44100, 0);
			opaque.Xmp_Set_Position(2);
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(2, p.Ord, "Didn't start at pattern 2");

			for (c_int i = 0; i < 30; i++)
				opaque.Xmp_Play_Frame();

			opaque.Xmp_Prev_Position();
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(0, p.Ord, "Incorrect pattern");
			Assert.AreEqual(0, p.Row, "Incorrect row");
			Assert.AreEqual(0, p.Frame, "Incorrect frame");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
