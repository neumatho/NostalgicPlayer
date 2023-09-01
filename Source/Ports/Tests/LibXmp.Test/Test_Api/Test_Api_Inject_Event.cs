/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer;
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
		public void Test_Api_Inject_Event()
		{
			Xmp_Event @event = new Xmp_Event
			{
				Note = 60,
				Ins = 2,
				Vol = 40,
				FxT = 0xf,
				FxP = 3,
				F2T = 0,
				F2P = 0,
				_Flag = 0
			};

			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Create_Simple_Module(opaque, 2, 2);

			opaque.Xmp_Start_Player(44100, 0);
			opaque.Xmp_Play_Frame();

			opaque.Xmp_Inject_Event(3, @event);
			opaque.Xmp_Play_Frame();

			c_int voc = Util.Map_Channel(p, 3);
			Assert.IsTrue(voc >= 0, "Virtual map");
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			Assert.AreEqual(59, vi.Note, "Set note");
			Assert.AreEqual(1, vi.Ins, "Set instrument");
			Assert.AreEqual(39 * 16, vi.Vol, "Set volume");
			Assert.AreEqual(3, p.Speed, "Set effect");
			Assert.IsTrue(vi.Pos0 == 0, "Sample position");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
