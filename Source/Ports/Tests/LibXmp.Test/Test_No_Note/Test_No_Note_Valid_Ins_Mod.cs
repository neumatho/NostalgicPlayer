/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_No_Note
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_No_Note
	{
		// Case 2: New instrument (no note)
		//
		//   Instrument -> None    Same    Valid   Inval
		// PT1.1           -       Play    Play    Cut
		// PT1.3           -       NewVol  NewVol* Cut
		// PT2.3           -       NewVol  NewVol* Cut
		// PT3.15          -       NewVol  NewVol  Cut     <= "Standard"
		// PT3.61          -       NewVol  NewVol  Cut     <=
		// PT4b2           -       NewVol  NewVol  Cut     <=
		// MED             -       Hold    Hold    Cut%
		// FT2             -       OldVol  OldVol  OldVol
		// ST3             -       NewVol  NewVol  Cont
		// IT(s)           -       NewVol  NewVol  Cont
		// IT(i)           -       NewVol# Play    Cont
		// DT32            -       NewVol# NewVol# Cut
		//
		// Play    = Play new note with new default volume
		// Switch  = Play new note with current volume
		// NewVol  = Don't play sample, set new default volume
		// OldVol  = Don't play sample, set old default volume
		// Cont    = Continue playing sample
		// Cut     = Stop playing sample
		//
		//   * Protracker 1.3/2.3 switches to new sample in the line after the new
		//     instrument event. The new instrument is not played from start (i.e. a
		//     short transient sample may not be played). This behaviour is NOT
		//     emulated by the current version of xmp.
		//
		//     00 C-2 03 A0F  <=  Play instrument 03 and slide volume down
		//     01 --- 02 000  <=  Set volume of instrument 02, playing instrument 03
		//     02 --- 00 000  <=  Switch to instrument 02 (weird!)
		//
		//     00 C-2 03 000  <=  Play instrument 03
		//     01 A-3 02 308  <=  Start portamento with instrument 03
		//     02 --- 00 xxx  <=  Switch to instrument 02 (weird!)
		//
		//   # Don't reset envelope

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_No_Note_Valid_Ins_Mod()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Create_Simple_Module(opaque, 2, 2);
			Set_Instrument_Volume(opaque, 0, 0, 22);
			Set_Instrument_Volume(opaque, 1, 0, 33);
			New_Event(opaque, 0, 0, 0, 60, 1, 44, 0x0f, 2, 0, 0);
			New_Event(opaque, 0, 1, 0, 0, 2, 0, 0x00, 0, 0, 0);

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

			// Row 1: Valid instrument with no note (PT 3.15)
			//
			// When a new valid instrument, different from the current instrument
			// and no note is set, PT3.15 keeps playing the current sample but
			// sets the volume to the new instrument's default volume
			opaque.Xmp_Play_Frame();

			Assert.AreEqual(0, vi.Ins, "Not original instrument");
			Assert.AreEqual(59, vi.Note, "Not same note");
			Assert.AreEqual(33 * 16, vi.Vol, "Not new volume");
			Assert.IsTrue(vi.Pos0 != 0, "Sample reset");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
