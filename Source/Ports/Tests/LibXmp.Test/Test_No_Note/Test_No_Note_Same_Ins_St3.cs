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
		//   * Protracker 1.3/2.3 queues sample changes immediately, but they don't take
		//     effect until the current playing sample completes its loop. This is
		//     supported by libxmp, as it shouldn't significantly hurt PT3 compatibility
		//
		//   # Don't reset envelope

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_No_Note_Same_Ins_St3()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Create_Read_Event_Test_Module(opaque, 2);
			New_Event(opaque, 0, 0, 0, Key_C5, Ins_0, 0, 0x00, 0, 0, 0);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_VolSet, Set_Vol, Effects.Fx_SetPan, Set_Pan);
			New_Event(opaque, 0, 2, 0, 0, Ins_0, 0, 0x00, 0, 0, 0);
			New_Event(opaque, 0, 3, 0, Key_B5, Ins_0, 0, 0x00, 0, 0, 0);
			New_Event(opaque, 0, 4, 0, 0, 0, 0, Effects.Fx_VolSet, Set_Vol, Effects.Fx_SetPan, Set_Pan);
			New_Event(opaque, 0, 5, 0, 0, Ins_0, 0, 0x00, 0, 0, 0);
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

			// Row 2: Valid instrument with no note (ST3)
			//
			// When a new valid instrument is the same as the current instrument
			// and no note is set, ST3 keeps playing the current sample but
			// sets the volume to the instrument's default volume
			opaque.Xmp_Play_Frame();

			Check_On(xc, vi, Key_C5, Ins_0, Ins_0_Sub_0_Vol, Ins_0_Sub_0_Pan, Ins_0_Fade, "row 2");

			opaque.Xmp_Play_Frame();

			// Row 3: Same, except subinstrument 1
			// This is not supported by ST3; provided for other formats
			opaque.Xmp_Play_Frame();

			Check_New(xc, vi, Key_B5, Ins_0, Ins_0_Sub_1_Vol, Ins_0_Sub_1_Pan, Ins_0_Fade, "row 3");

			opaque.Xmp_Play_Frame();

			// Row 4: Set non-default volume and pan
			opaque.Xmp_Play_Frame();

			Check_On(xc, vi, Key_B5, Ins_0, Set_Vol, Set_Pan, Ins_0_Fade, "row 4");

			opaque.Xmp_Play_Frame();

			// Row 5
			opaque.Xmp_Play_Frame();

			Check_On(xc, vi, Key_B5, Ins_0, Ins_0_Sub_1_Vol, Ins_0_Sub_1_Pan, Ins_0_Fade, "row 5");

			opaque.Xmp_Play_Frame();

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
