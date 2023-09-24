/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
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
		/// Fasttracker 2 process the first point before it checks for
		/// sustain/looping. This test will check if the loop on the first
		/// point is ignored
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Xm_Envelope()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Frame_Info fi;

			Create_Simple_Module(opaque, 2, 2);

			Set_Instrument_Envelope(opaque, 0, 0, 0, 64);
			Set_Instrument_Envelope(opaque, 0, 1, 16, 0);
			Set_Instrument_Envelope_Loop(opaque, 0, 0, 0);

			New_Event(opaque, 0, 0, 0, 60, 1, 0, 0, 0, 0, 0);
			Set_Quirk(opaque, Quirk_Flag.Ft2 | Quirk_Flag.Ft2Env, Read_Event.Ft2);

			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 16; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out fi);

				Assert.AreEqual(64 - (i * 4), fi.Channel_Info[0].Volume, "Wrong volume");
			}

			for (c_int i = 0; i < 16; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out fi);

				Assert.AreEqual(0, fi.Channel_Info[0].Volume, "Volume not zero");
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
