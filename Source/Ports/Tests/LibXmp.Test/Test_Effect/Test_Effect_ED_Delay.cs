/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Effect
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Effect
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_ED_Delay()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Create_Simple_Module(opaque, 2, 2);

			New_Event(opaque, 0, 0, 0, 60, 2, 40, 0x0f, 0x02, 0x00, 0x00);
			New_Event(opaque, 0, 1, 0, 61, 1, 0, 0, 0, 0x0e, 0xd0);
			New_Event(opaque, 0, 2, 0, 62, 2, 0, 0x0e, 0xd1, 0, 0);
			New_Event(opaque, 0, 3, 0, 63, 2, 0, 0, 0, 0x0e, 0xd3);
			New_Event(opaque, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0);
			New_Event(opaque, 0, 5, 0, 0, 0, 0, 0, 0, 0x0e, 0xd1);
			New_Event(opaque, 0, 6, 0, 0, 0, 0, 0, 0, 0x0e, 0xd1);
			New_Event(opaque, 0, 7, 0, 0, 0, 0, 0, 0, 0x0e, 0xd0);
			New_Event(opaque, 0, 8, 0, 0, 3, 0, 0, 0, 0, 0);
			New_Event(opaque, 0, 9, 0, 0, 0, 0, 0, 0, 0x0e, 0xd1);

			opaque.Xmp_Start_Player(44100, 0);

			// Row 0
			opaque.Xmp_Play_Frame();

			c_int voc = Map_Channel(p, 0);
			Assert.IsTrue(voc >= 0, "Virtual map");
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			Assert.AreEqual(59, vi.Note, "Row 0 frame 0");
			Assert.IsTrue(vi.Pos0 == 0, "Sample position");

			opaque.Xmp_Play_Frame();
			Assert.AreEqual(59, vi.Note, "Row 0 frame 1");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position");

			// Row 1
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(60, vi.Note, "Row 1 frame 0");
			Assert.IsTrue(vi.Pos0 == 0, "Sample position");

			opaque.Xmp_Play_Frame();
			Assert.AreEqual(60, vi.Note, "Row 1 frame 1");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position");

			// Row 2: Delay this frame
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(60, vi.Note, "Row 2 frame 0");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position");

			// Note changes in frame 1
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(61, vi.Note, "Row 2 frame 1");
			Assert.IsTrue(vi.Pos0 == 0, "Sample position");

			// Row 3: Delay larger than speed
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(61, vi.Note, "Row 3 frame 0");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position");

			opaque.Xmp_Play_Frame();
			Assert.AreEqual(61, vi.Note, "Row 3 frame 1");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position");

			// Row 4: Nothing should happen
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(61, vi.Note, "Row 4 frame 0");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position");

			opaque.Xmp_Play_Frame();
			Assert.AreEqual(61, vi.Note, "Row 4 frame 1");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position");

			// Now test retrigs on delay effect

			// Row 5: In standard mode
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(61, vi.Note, "Row 5 frame 0");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position");

			opaque.Xmp_Play_Frame();
			Assert.AreEqual(61, vi.Note, "Row 5 frame 1");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position");

			Set_Quirk(opaque, Quirk_Flag.RtDelay, Read_Event.Ft2);

			// Row 6: In retrig mode
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(61, vi.Note, "Row 6 frame 0");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position");

			opaque.Xmp_Play_Frame();
			Assert.AreEqual(61, vi.Note, "Row 6 frame 1");
			Assert.IsTrue(vi.Pos0 == 0, "Sample position");

			// Row 7: Don't retrig
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(61, vi.Note, "Row 7 frame 0");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position");

			opaque.Xmp_Play_Frame();
			Assert.AreEqual(61, vi.Note, "Row 7 frame 1");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position");

			// Row 8: Invalid instrument is ignored
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(61, vi.Note, "Row 8 frame 0");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position");

			opaque.Xmp_Play_Frame();
			Assert.AreEqual(61, vi.Note, "Row 8 frame 1");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position");

			// Row 9: Don't retrig
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(61, vi.Note, "Row 9 frame 0");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position");
			Assert.IsTrue(vi.Vol != 0, "Voice volume");

			opaque.Xmp_Play_Frame();
			// We cut the virtual voice on invalid instrument
			Assert.AreEqual(0, vi.Note, "Row 9 frame 1");
			Assert.IsTrue(vi.Pos0 == 0, "Sample position");
			Assert.IsTrue(vi.Vol == 0, "Voice volume");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
