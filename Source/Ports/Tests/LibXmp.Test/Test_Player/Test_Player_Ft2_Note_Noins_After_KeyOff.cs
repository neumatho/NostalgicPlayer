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
		/// When a note is issued with no instrument after a keyoff event, it
		/// should be played [if instrument has no envelope]
		///
		/// Last Battle.xm:
		///
		/// 00 D#4 02 .. ...
		/// 01 ... .. .. ...
		/// 02 C-4 .. .. ...
		/// 03 ... .. .. ...
		/// 04 === .. .. ...
		/// 05 ... .. .. ...
		/// 06 D#4 .. 31 ... *===
		/// 07 ... .. .. ...
		/// 08 C-4 .. .. ...
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Note_Noins_After_KeyOff()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Frame_Info fi;

			Create_Simple_Module(opaque, 2, 2);

			Set_Instrument_Volume(opaque, 0, 0, 64);

			New_Event(opaque, 0, 0, 0, 60, 1, 0, 0x0f, 2, 0, 0);
			New_Event(opaque, 0, 1, 0, Constants.Xmp_Key_Off, 0, 0, 0, 0, 0, 0);
			New_Event(opaque, 0, 2, 0, 50, 0, 20, 0, 0, 0, 0);
			Set_Quirk(opaque, Quirk_Flag.Ft2, Read_Event.Ft2);

			opaque.Xmp_Start_Player(44100, 0);

			// Row 0
			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out fi);

			Assert.AreEqual(59, fi.channel_Info[0].Note, "Set note");
			Assert.AreEqual(64, fi.channel_Info[0].Volume, "Set volume");

			opaque.Xmp_Play_Frame();

			// Row 1: Key off
			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out fi);

			Assert.AreEqual(59, fi.channel_Info[0].Note, "Set note");
			Assert.AreEqual(0, fi.channel_Info[0].Volume, "Set volume");

			opaque.Xmp_Play_Frame();

			// Row 2: New note
			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out fi);

			Assert.AreEqual(49, fi.channel_Info[0].Note, "Set note");
			Assert.AreEqual(19, fi.channel_Info[0].Volume, "Set volume");

			opaque.Xmp_Play_Frame();

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
