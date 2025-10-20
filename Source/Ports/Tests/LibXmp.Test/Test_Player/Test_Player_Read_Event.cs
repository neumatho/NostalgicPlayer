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
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Read_Event()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Create_Simple_Module(opaque, 2, 2);

			New_Event(opaque, 0, 0, 0, 60, 2, 40, 0x0f, 3, 0, 0);
			New_Event(opaque, 0, 1, 0, 61, 1, 0, 0x00, 0, 0, 0);

			opaque.Xmp_Start_Player(44100, 0);
			opaque.Xmp_Play_Frame();

			c_int voc = Map_Channel(p, 0);
			Assert.IsGreaterThanOrEqualTo(0, voc, "Virtual map");
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			Assert.AreEqual(59, vi.Note, "Set note");
			Assert.AreEqual(1, vi.Ins, "Set instrument");
			Assert.AreEqual(39 * 16, vi.Vol, "Set volume");
			Assert.AreEqual(3, p.Speed, "Set effect");
			Assert.AreEqual(0, vi.Pos0, "Sample position");

			opaque.Xmp_Play_Frame();

			Assert.AreEqual(59, vi.Note, "Set note");
			Assert.AreEqual(1, vi.Ins, "Set instrument");
			Assert.AreEqual(39 * 16, vi.Vol, "Set volume");
			Assert.AreNotEqual(0, vi.Pos0, "Sample position");

			opaque.Xmp_Play_Frame();

			Assert.AreEqual(59, vi.Note, "Set note");
			Assert.AreEqual(1, vi.Ins, "Set instrument");
			Assert.AreEqual(39 * 16, vi.Vol, "Set volume");
			Assert.AreNotEqual(0, vi.Pos0, "Sample position");

			opaque.Xmp_Play_Frame();

			Assert.AreEqual(60, vi.Note, "Set note");
			Assert.AreEqual(0, vi.Ins, "Set instrument");
			Assert.AreEqual(64 * 16, vi.Vol, "Set volume");
			Assert.AreEqual(0, vi.Pos0, "Sample position");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
