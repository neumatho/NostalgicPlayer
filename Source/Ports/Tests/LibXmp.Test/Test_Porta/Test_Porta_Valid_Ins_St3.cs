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
		// ST3             Cont    NewVol  NewVol  Cont
		// IT(s)           Cont    NewVol  NewVol  Cont
		// IT(i) @         Cont    NewVol  NewVol  Cont
		// DT32            Cont    NewVol  NewVol  Cut

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Porta_Valid_Ins_St3()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Create_Read_Event_Test_Module(opaque, 2);
			New_Event(opaque, 0, 0, 0, Key_C5, Ins_0, 0, 0x00, 0, 0, 0);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_VolSet, Set_Vol, Effects.Fx_SetPan, Set_Pan);
			New_Event(opaque, 0, 2, 0, Key_D4, Ins_1, 0, Effects.Fx_TonePorta, 4, 0, 0);
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

			// Row 2: Valid instrument with tone portamento (ST3)
			//
			// When a new valid instrument, different from the current instrument
			// is played with tone portamento, ST3 keeps playing the current
			// sample but sets the volume to the new instrument's default volume
			opaque.Xmp_Play_Frame();

			Check_On(xc, vi, Key_C5, Ins_0, Ins_1_Sub_0_Vol, Ins_1_Sub_0_Pan, -1/*FIXME: Ins_1_Fade*/, "row 2");

			opaque.Xmp_Play_Frame();

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
