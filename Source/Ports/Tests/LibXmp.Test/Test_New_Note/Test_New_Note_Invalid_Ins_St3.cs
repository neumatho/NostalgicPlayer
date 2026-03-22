/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player;

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
		public void Test_New_Note_Invalid_Ins_St3()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Create_Read_Event_Test_Module(opaque, 2);
			New_Event(opaque, 0, 0, 0, Key_C5, Ins_0, 0, 0x00, 0, 0, 0);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_VolSet, Set_Vol, Effects.Fx_SetPan, Set_Pan);
			New_Event(opaque, 0, 2, 0, Key_D4, Ins_Inval, 0, 0x00, 0, 0, 0);
			New_Event(opaque, 0, 3, 0, Key_C4, 0, 0, 0x00, 0, 0, 0);
			New_Event(opaque, 0, 4, 0, Key_C5, Ins_0, 0, 0x00, 0, 0, 0);
			New_Event(opaque, 0, 5, 0, Key_B5, Ins_1, 0, 0x00, 0, 0, 0);
			Set_Quirk(opaque, Quirk_Flag.St3, Read_Event.St3);

			opaque.Xmp_Start_Player(Constants.Xmp_Min_SRate, 0);

			// Row 0
			opaque.Xmp_Play_Frame();

			Channel_Data xc = p.Xc_Data[0];
			c_int voc = Map_Channel(p, 0);
			Assert.IsGreaterThanOrEqualTo(0, voc, "Virtual map");
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			Check_New(xc, vi, Key_C5, Ins_0, Ins_0_Sub_0_Vol, Ins_0_Sub_0_Pan, Ins_0_Fade, "row 0");

			opaque.Xmp_Play_Frame();

			// Row 1: Set non-default volume and pan
			opaque.Xmp_Play_Frame();

			Check_On(xc, vi, Key_C5, Ins_0, Set_Vol, Set_Pan, Ins_0_Fade, "row 1");

			opaque.Xmp_Play_Frame();

			// Row 2: Invalid instrument with new note (ST3)
			//
			// When a new invalid instrument and a new note is set, ST3 plays
			// the old sample with current volume and new note
			opaque.Xmp_Play_Frame();

			Check_New(xc, vi, Key_D4, Ins_0, Set_Vol, Set_Pan, Ins_0_Fade, "row 2");

			opaque.Xmp_Play_Frame();

			// Row 3: FIXME: restarts the old note, seems wrong? (ST3)
			opaque.Xmp_Play_Frame();

			/*Check_Off(xc, vi, "row 3");*/

			opaque.Xmp_Play_Frame();

			// Row 4
			opaque.Xmp_Play_Frame();

			Check_New(xc, vi, Key_C5, Ins_0, Ins_0_Sub_0_Vol, Ins_0_Sub_0_Pan, Ins_0_Fade, "row 4");

			opaque.Xmp_Play_Frame();

			// Row 5: invalid subinstrument with new note (ST3)
			// 
			// An invalid subinstrument of a valid instrument also cuts.
			// ST3 doesn't support this feature; it is provided for other formats
			opaque.Xmp_Play_Frame();

			// FIXME: what is consistency? check_off(xc, vi, "row 5");

			opaque.Xmp_Play_Frame();

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
