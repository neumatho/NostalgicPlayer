﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_New_Note
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_New_Note
	{
		// Case 1: New note
		//
		//   Instrument -> None    Same    Valid   Inval
		// PT1.1           Play    Play    Play    Cut
		// PT1.3           Play    Play    Play    Cut
		// PT2.3           Switch  Play    Play    Cut     <=
		// PT3.15          Switch  Play    Play    Cut     <= "Standard"
		// PT3.61          Switch  Play    Play    Cut     <=
		// PT4b2           Switch  Play    Play    Cut     <=
		// MED             Switch  Play    Play    Cut     <=
		// FT2             Switch  Play    Play    Cut     <=
		// ST3             Switch  Play    Play    Switch
		// IT(s)           Switch  Play    Play    ?
		// IT(i)           Switch  Play    Play    Cont
		// DT32            Play    Play    Play    Cut
		//
		// Play    = Play new note with new default volume
		// Switch  = Play new note with current volume
		// NewVol  = Don't play sample, set new default volume
		// OldVol  = Don't play sample, set old default volume
		// Cont    = Continue playing sample
		// Cut     = Stop playing sample

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_New_Note_Valid_Ins_Ft2()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Create_Simple_Module(opaque, 2, 2);
			Set_Instrument_Volume(opaque, 0, 0, 22);
			Set_Instrument_Volume(opaque, 1, 0, 33);
			New_Event(opaque, 0, 0, 0, 60, 1, 44, 0x0f, 2, 0, 0);
			New_Event(opaque, 0, 1, 0, 50, 2, 0, 0x00, 0, 0, 0);
			Set_Quirk(opaque, Quirk_Flag.Ft2, Read_Event.Ft2);

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

			// Row 1: Valid instrument with new note (FT2)
			//
			// When a new valid instrument and a new note is set, FT2 plays
			// the new sample with the new instrument's default volume
			opaque.Xmp_Play_Frame();

			Assert.AreEqual(1, vi.Ins, "Not new instrument");
			Assert.AreEqual(49, vi.Note, "Not new note");
			Assert.AreEqual(33 * 16, vi.Vol, "Not new instrument volume");
			Assert.IsTrue(vi.Pos0 == 0, "Sample didn't reset");

			opaque.Xmp_Play_Frame();

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
