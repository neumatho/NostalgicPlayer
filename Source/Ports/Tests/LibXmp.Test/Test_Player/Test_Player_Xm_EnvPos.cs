/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
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
		/// If the envelope position is set outside the loop via Lxx effect,
		/// Fasttracker 2 will ignore the loop
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Xm_EnvPos()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Frame_Info fi;

			Create_Simple_Module(opaque, 2, 2);

			// Envelope from Ebony Owl Netsuke.xm, first instrument
			Set_Instrument_Envelope(opaque, 0, 0, 0, 59);
			Set_Instrument_Envelope(opaque, 0, 1, 1, 30);
			Set_Instrument_Envelope(opaque, 0, 2, 2, 1);
			Set_Instrument_Envelope(opaque, 0, 3, 9, 0);
			Set_Instrument_Envelope(opaque, 0, 4, 34, 62);
			Set_Instrument_Envelope(opaque, 0, 5, 35, 15);
			Set_Instrument_Envelope(opaque, 0, 6, 36, 0);
			Set_Instrument_Envelope(opaque, 0, 7, 93, 0);
			Set_Instrument_Envelope(opaque, 0, 8, 94, 59);
			Set_Instrument_Envelope(opaque, 0, 9, 99, 31);
			Set_Instrument_Envelope(opaque, 0, 10, 102, 19);
			Set_Instrument_Envelope(opaque, 0, 11, 106, 11);
			Set_Instrument_Envelope_Loop(opaque, 0, 0, 0);
			Set_Instrument_Envelope_Sus(opaque, 0, 3);

			New_Event(opaque, 0, 0, 0, 60, 1, 0, Effects.Fx_EnvPos, 34, 0, 0);
			Set_Quirk(opaque, Quirk_Flag.Ft2 | Quirk_Flag.Ft2Env, Read_Event.Ft2);

			opaque.Xmp_Start_Player(44100, 0);

			// Frame 0
			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out fi);
			Assert.AreEqual(15, fi.Channel_Info[0].Volume, "Wrong volume at frame 0");

			// Frame 1
			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out fi);
			Assert.AreEqual(0, fi.Channel_Info[0].Volume, "Wrong volume at frame 1");

			// Frame 2
			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out fi);
			Assert.AreEqual(0, fi.Channel_Info[0].Volume, "Wrong volume at frame 2");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
