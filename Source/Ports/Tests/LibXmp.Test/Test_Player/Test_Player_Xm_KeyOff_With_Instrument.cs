/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Player
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Player
	{
		/********************************************************************/
		/// <summary>
		/// Don't reset the channel volume when a keyoff event is used with
		/// an instrument.
		///
		/// xyce-dans_la_rue.xm pattern 0E/0F:
		///
		/// 00 D#5 0B .. ...
		/// 01 ... .. .. ...
		/// 02 ... .. .. ...
		/// 03 ... .. .. ...
		/// 04 ... .. .. ...
		/// 05 ... .. .. ...
		/// 06 ... .. .. ...
		/// 07 ... .. -2 ...
		/// 08 ... .. -2 ...
		/// 0A ... .. -2 ...
		/// 0B ... .. -2 ...
		/// 0C === 0B .. ...
		/// 0D ... .. .. ...
		///
		/// ...
		///
		/// 00 === 0B .. ...
		/// 01 ... .. .. ...
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Xm_KeyOff_With_Instrument()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Frame_Info fi;

			Create_Simple_Module(opaque, 2, 2);

			Set_Instrument_Volume(opaque, 0, 0, 64);
			Set_Instrument_FadeOut(opaque, 0, 0x400);

			New_Event(opaque, 0, 0, 0, 60, 1, 0, Effects.Fx_Speed, 2, 0, 0);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_VolSlide, 8, 0, 0);
			New_Event(opaque, 0, 2, 0, 0, 0, 0, Effects.Fx_VolSlide, 8, 0, 0);
			New_Event(opaque, 0, 3, 0, Constants.Xmp_Key_Fade, 1, 0, 0, 0, 0, 0);
			New_Event(opaque, 0, 40, 0, Constants.Xmp_Key_Fade, 2, 0, 0, 0, 0, 0);
			Set_Quirk(opaque, Quirk_Flag.Ft2, Read_Event.Ft2);

			opaque.Xmp_Start_Player(44100, 0);

			// Row 0
			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out fi);

			Assert.AreEqual(59, fi.channel_Info[0].Note, "Set note");
			Assert.AreEqual(64, fi.channel_Info[0].Volume, "Volume");

			opaque.Xmp_Play_Frame();

			// Row 1
			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out fi);

			Assert.AreEqual(64, fi.channel_Info[0].Volume, "Volume");

			opaque.Xmp_Play_Frame();

			// Row 2
			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out fi);

			Assert.AreEqual(56, fi.channel_Info[0].Volume, "Volume");

			opaque.Xmp_Play_Frame();

			// Row 3: Keyoff with instrument
			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out fi);

			Assert.AreEqual(59, fi.channel_Info[0].Note, "Set note");
			Assert.AreEqual(63, fi.channel_Info[0].Volume, "Volume");

			opaque.Xmp_Play_Frame();

			// Row 4 - 63: Fadeout continues
			for (c_int i = 5; i < 64; i++)
			{
				c_int vol = 63 - (i - 5) * 2;

				opaque.Xmp_Play_Frame();
				Assert.AreEqual(vol > 0 ? vol : 0, fi.channel_Info[0].Volume, "Volume");

				// Instrument should not change in row 40
				Assert.AreEqual(0, fi.channel_Info[0].Instrument, "Instrument");

				opaque.Xmp_Get_Frame_Info(out fi);
				opaque.Xmp_Play_Frame();
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
