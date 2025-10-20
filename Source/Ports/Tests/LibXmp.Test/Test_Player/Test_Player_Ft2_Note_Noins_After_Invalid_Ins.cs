/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		/// C-5 39	-> valid instrument
		/// --- 30	-> valid instrument
		/// C-5 --	-> this note plays
		///
		/// C-5 39	-> valid instrument
		/// --- 28	-> invalid instrument
		/// C-5 --	-> this note shouldn't play
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Note_Noins_After_Invalid_Ins()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Create_Simple_Module(opaque, 2, 2);

			Set_Instrument_Volume(opaque, 0, 0, 22);
			Set_Instrument_Volume(opaque, 1, 0, 33);

			New_Event(opaque, 0, 0, 0, 60, 1, 44, 0x0f, 2, 0, 0);
			New_Event(opaque, 0, 1, 0, 0, 2, 0, 0x00, 0, 0, 0);
			New_Event(opaque, 0, 2, 0, 50, 0, 0, 0x00, 0, 0, 0);
			New_Event(opaque, 0, 3, 0, 60, 1, 44, 0x00, 0, 0, 0);
			New_Event(opaque, 0, 4, 0, 0, 3, 0, 0x00, 0, 0, 0);
			New_Event(opaque, 0, 5, 0, 50, 0, 0, 0x00, 0, 0, 0);
			Set_Quirk(opaque, Quirk_Flag.Ft2, Read_Event.Ft2);

			opaque.Xmp_Start_Player(44100, 0);

			// Row 0 - Valid instrument
			opaque.Xmp_Play_Frame();

			c_int voc = Map_Channel(p, 0);
			Assert.IsGreaterThanOrEqualTo(0, voc, "Virtual map");
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			Assert.AreEqual(59, vi.Note, "Set note");
			Assert.AreEqual(0, vi.Ins, "Set instrument");
			Assert.AreEqual(43 * 16, vi.Vol, "Set volume");

			opaque.Xmp_Play_Frame();

			// Row 1: Valid instrument
			opaque.Xmp_Play_Frame();

			Assert.AreEqual(59, vi.Note, "Set note");
			Assert.AreEqual(0, vi.Ins, "Not same instrument");
			Assert.AreNotEqual(0, vi.Vol, "Cut sample");

			opaque.Xmp_Play_Frame();

			// Row 2: No instrument (should play)
			opaque.Xmp_Play_Frame();

			Assert.AreEqual(49, vi.Note, "Set note");
			Assert.AreEqual(1, vi.Ins, "Set instrument");
			Assert.AreNotEqual(0, vi.Vol, "Cut sample");

			opaque.Xmp_Play_Frame();

			// Row 3: Valid instrument
			opaque.Xmp_Play_Frame();

			Assert.AreEqual(59, vi.Note, "Set note");
			Assert.AreEqual(0, vi.Ins, "Set instrument");
			Assert.AreNotEqual(0, vi.Vol, "Cut sample");

			opaque.Xmp_Play_Frame();

			// Row 4: Invalid instrument
			opaque.Xmp_Play_Frame();

			Assert.AreNotEqual(0, vi.Vol, "Cut sample");

			opaque.Xmp_Play_Frame();

			// Row 5: No instrument (shouldn't play)
			opaque.Xmp_Play_Frame();

			Assert.AreEqual(0, vi.Vol, "Didn't cut sample");

			opaque.Xmp_Play_Frame();

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
