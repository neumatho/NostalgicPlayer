﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		/// Fasttracker 2 increments the envelope position before checking
		/// for the end of the envelope loop, and only loops if the envelope
		/// position is exactly at the envelope end. This causes it to
		/// entirely skip loops where the start and end points are the same.
		///
		/// However, if the sustain point is on the same position and has NOT
		/// been released, it will be held at this position (this was
		/// previously broken by an old incorrect bugfix). See
		/// fade_2_grey_visage.xm instrument 3, orders 3-4, channels 8-9
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Xm_Envelope_Zero_Loop()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Frame_Info fi;

			Create_Simple_Module(opaque, 2, 2);

			Set_Instrument_Envelope(opaque, 0, 0, 0, 64);
			Set_Instrument_Envelope(opaque, 0, 1, 16, 0);
			Set_Instrument_Envelope_Loop(opaque, 0, 0, 0);

			Set_Instrument_Envelope(opaque, 1, 0, 0, 64);
			Set_Instrument_Envelope(opaque, 1, 1, 16, 0);
			Set_Instrument_Envelope_Loop(opaque, 1, 0, 0);
			Set_Instrument_Envelope_Sus(opaque, 1, 0);

			New_Event(opaque, 0, 0, 0, 60, 1, 0, 0, 0, 0, 0);
			New_Event(opaque, 0, 0, 1, 60, 2, 0, 0, 0, 0, 0);
			Set_Quirk(opaque, Quirk_Flag.Ft2 | Quirk_Flag.Ft2Env, Read_Event.Ft2);

			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 16; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out fi);

				Assert.AreEqual(64 - (i * 4), fi.Channel_Info[0].Volume, "Wrong volume");
				Assert.AreEqual(64, fi.Channel_Info[1].Volume, "Wrong volume (sus)");
			}

			for (c_int i = 0; i < 16; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out fi);

				Assert.AreEqual(0, fi.Channel_Info[0].Volume, "Volume not zero");
				Assert.AreEqual(64, fi.Channel_Info[1].Volume, "Volume not 64 (sus)");
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
