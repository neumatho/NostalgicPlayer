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
		public void Test_Effect_E9_Retrig()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Create_Simple_Module(opaque, 2, 2);

			New_Event(opaque, 0, 0, 0, 60, 2, 40, 0x0e, 0x92, 0x00, 0x00);
			New_Event(opaque, 0, 1, 0, 61, 2, 40, 0x0e, 0x90, 0x0f, 0x03);
			New_Event(opaque, 0, 2, 0, 62, 2, 40, 0x0e, 0x90, 0x00, 0x00);

			opaque.Xmp_Start_Player(44100, 0);

			// Row 0

			// Frame 0
			opaque.Xmp_Play_Frame();

			c_int voc = Util.Map_Channel(p, 0);
			Assert.IsTrue(voc >= 0, "Virtual map");
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			Assert.AreEqual(59, vi.Note, "Row 0 frame 0");
			Assert.IsTrue(vi.Pos0 == 0, "Sample position frame 0");

			// Frame 1
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(59, vi.Note, "Row 0 frame 1");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position frame 1");

			// Frame 2
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(59, vi.Note, "Row 0 frame 2");
			Assert.IsTrue(vi.Pos0 == 0, "Retrig frame 2");

			// Frame 3
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(59, vi.Note, "Row 0 frame 3");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position frame 3");

			// Frame 4
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(59, vi.Note, "Row 0 frame 4");
			Assert.IsTrue(vi.Pos0 == 0, "Sample position frame 4");

			// Frame 5
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(59, vi.Note, "Row 0 frame 5");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position frame 5");

			// Row 1 - without S3M quirk

			// Frame 0
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(60, vi.Note, "Row 0 frame 0");
			Assert.IsTrue(vi.Pos0 == 0, "Sample position frame 0");

			// Frame 1
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(60, vi.Note, "Row 0 frame 1");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position frame 1");

			// Frame 2
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(60, vi.Note, "Row 0 frame 2");
			Assert.IsTrue(vi.Pos0 != 0, "Sample position frame 2");

			// Row 2 - with S3M quirk
			Set_Quirk(opaque, Quirk_Flag.S3MRtg, Read_Event.Mod);

			// Frame 0
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(61, vi.Note, "Row 0 frame 0");
			Assert.IsTrue(vi.Pos0 == 0, "Sample position frame 0");

			// Frame 1
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(61, vi.Note, "Row 0 frame 1");
			Assert.IsTrue(vi.Pos0 == 0, "Sample position frame 1");

			// Frame 2
			opaque.Xmp_Play_Frame();
			Assert.AreEqual(61, vi.Note, "Row 0 frame 2");
			Assert.IsTrue(vi.Pos0 == 0, "Sample position frame 2");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
