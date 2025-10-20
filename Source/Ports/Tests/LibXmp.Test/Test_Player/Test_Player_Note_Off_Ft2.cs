/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer;

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
		public void Test_Player_Note_Off_Ft2()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Create_Simple_Module(opaque, 2, 2);

			New_Event(opaque, 0, 0, 0, 60, 1, 44, 0x0f, 2, 0, 0);
			New_Event(opaque, 0, 1, 0, Constants.Xmp_Key_Off, 0, 0, 0x00, 0, 0, 0);
			Set_Quirk(opaque, Quirk_Flag.Ft2, Read_Event.Ft2);
			Set_Instrument_FadeOut(opaque, 0, 16000);

			// Test: Without envelope in FT2 mode
			opaque.Xmp_Start_Player(44100, 0);
			opaque.Xmp_Play_Frame();

			c_int voc = Map_Channel(p, 0);
			Assert.IsGreaterThanOrEqualTo(0, voc, "Virtual map");
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			Assert.AreEqual(59, vi.Note, "Set note");
			Assert.AreEqual(0, vi.Ins, "Set instrument");
			Assert.AreEqual(43, vi.Vol / 16, "Set volume");
			Assert.AreEqual(0, vi.Pos0, "Sample position");

			opaque.Xmp_Play_Frame();

			// Row 1: Test keyoff
			opaque.Xmp_Play_Frame();

			Assert.AreEqual(59, vi.Note, "Not same note");
			Assert.AreEqual(0, vi.Ins, "Not same instrument");
			Assert.AreEqual(0, vi.Vol / 16, "Didn't cut note");

			// Test: With envelope in FT2 mode
			Set_Instrument_Envelope(opaque, 0, 0, 0, 32);
			Set_Instrument_Envelope(opaque, 0, 1, 1, 32);
			Set_Instrument_Envelope(opaque, 0, 2, 2, 64);
			Set_Instrument_Envelope(opaque, 0, 3, 4, 0);
			Set_Instrument_Envelope_Sus(opaque, 0, 1);

			opaque.Xmp_Restart_Module();
			opaque.Xmp_Play_Frame();

			Assert.AreEqual(59, vi.Note, "Set note");
			Assert.AreEqual(0, vi.Ins, "Set instrument");
			Assert.AreEqual(21, vi.Vol / 16, "Envelope volume");
			Assert.AreEqual(0, vi.Pos0, "Sample position");

			opaque.Xmp_Play_Frame();

			// Row 1: Test keyoff
			opaque.Xmp_Play_Frame();

			Assert.AreEqual(59, vi.Note, "Not same note");
			Assert.AreEqual(0, vi.Ins, "Not same instrument");
			Assert.AreEqual(21, vi.Vol / 16, "Didn't follow envelope + fadeout");

			opaque.Xmp_Play_Frame();
			Assert.AreEqual(32, vi.Vol / 16, "Didn't follow envelope");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
