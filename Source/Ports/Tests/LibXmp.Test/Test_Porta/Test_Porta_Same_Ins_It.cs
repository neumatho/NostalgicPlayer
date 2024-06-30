/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Porta
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Porta
	{
		// Case 3: Tone portamento
		//
		//   Instrument -> None    Same    Valid   Inval
		// PT1.1           Cont            NewVol?
		// PT1.3           Cont    NewVol  NewVol* Cut
		// PT2.3           Cont    NewVol  NewVol* Cut
		// PT3.15          Cont    NewVol  NewVol  Cut     <= "Standard"
		// PT3.61          Cont    NewVol  NewVol  Cut     <=
		// PT4b2           Cont    NewVol  NewVol  Cut     <=
		// MED             Cont    NewVol  NewVol  Cut     <=
		// FT2             Cont    OldVol  OldVol  OldVol
		// PT3.15             Cont    NewVol  NewVol  Cont
		// IT(s)           Cont    NewVol  NewVol  Cont
		// IT(i) @         Cont    NewVol  NewVol  Cont
		// DT32            Cont    NewVol  NewVol  Cut

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Porta_Same_Ins_It()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Create_Simple_Module(opaque, 2, 2);
			Set_Instrument_Volume(opaque, 0, 0, 22);
			Set_Instrument_Volume(opaque, 1, 0, 33);
			New_Event(opaque, 0, 0, 0, 60, 1, 44, 0x0f, 2, 0, 0);
			New_Event(opaque, 0, 1, 0, 50, 1, 0, 0x03, 4, 0, 0);
			Set_Quirk(opaque, Quirk_Flag.It, Read_Event.It);

			opaque.Xmp_Start_Player(44100, 0);

			// Row 0
			opaque.Xmp_Play_Frame();

			c_int voc = Map_Channel(p, 0);
			Assert.IsTrue(voc >= 0, "Virtual map");
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			Assert.AreEqual(59, vi.Note, "Set note");
			Assert.AreEqual(0, vi.Ins, "Set instrument");
			Assert.AreEqual(43 * 16, vi.Vol, "Set volume");
			Assert.IsTrue(vi.Pos0 == 0, "Sample position");

			opaque.Xmp_Play_Frame();

			// Row 1: Same instrument with tone portamento (IT)
			//
			// When the same instrument as the current instrument is played
			// with tone portamento, IT keeps playing the current sample but
			// sets the volume to the instrument's default volume
			opaque.Xmp_Play_Frame();

			Assert.AreEqual(0, vi.Ins, "Not same instrument");
			Assert.AreEqual(59, vi.Note, "Not same note");
			Assert.AreEqual(22 * 16, vi.Vol, "Not new volume");
			Assert.IsTrue(vi.Pos0 != 0, "Sample reset");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
