/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Effect
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Effect
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_PastNote_Off()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			Create_Simple_Module(opaque, 2, 2);
			Set_Instrument_Volume(opaque, 0, 0, 22);
			Set_Instrument_Volume(opaque, 1, 0, 33);
			Set_Instrument_Nna(opaque, 0, 0, Xmp_Inst_Nna.Cont, Xmp_Inst_Dct.Off, Xmp_Inst_Dca.Cut);
			Set_Instrument_Envelope(opaque, 0, 0, 0, 32);
			Set_Instrument_Envelope(opaque, 0, 1, 1, 32);
			Set_Instrument_Envelope(opaque, 0, 2, 2, 64);
			Set_Instrument_Envelope(opaque, 0, 3, 4, 0);
			Set_Instrument_Envelope_Sus(opaque, 0, 1);

			New_Event(opaque, 0, 0, 0, 60, 1, 44, 0x0f, 2, 0, 0);
			New_Event(opaque, 0, 1, 0, 50, 2, 0, Effects.Fx_It_InstFunc, 0x01, 0, 0);
			Set_Quirk(opaque, Quirk_Flag.It, Read_Event.It);

			opaque.Xmp_Start_Player(44100, 0);

			// Row 0
			opaque.Xmp_Play_Frame();	// Frame 0

			c_int voc = Map_Channel(p, 0);
			Assert.IsGreaterThanOrEqualTo(0, voc, "Virtual map");
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			Assert.AreEqual(59, vi.Note, "Set note");
			Assert.AreEqual(0, vi.Ins, "Set instrument");
			Assert.AreEqual(21, vi.Vol / 16, "Set volume");	// With envelope
			Assert.AreEqual(0, vi.Pos0, "Sample position");

			opaque.Xmp_Play_Frame();	// Frame 1

			// Row 1: New event to test NNA
			opaque.Xmp_Play_Frame();	// Frame 2

			Assert.AreEqual(59, vi.Note, "Not same note");
			Assert.AreEqual(0, vi.Ins, "Not same instrument");
			Assert.AreEqual(43, vi.Vol / 16, "Not new volume");
			Assert.AreNotEqual(0, vi.Pos0, "Sample reset");

			// Find virtual voice for channel 0
			c_int i;
			for (i = 0; i < p.Virt.MaxVoc; i++)
			{
				if (p.Virt.Voice_Array[i].Chn == 0)
					break;
			}

			Assert.AreNotEqual(i, p.Virt.MaxVoc, "Didn't virtual voice");

			// New instrument in virtual channel. When NNA is set to OFF,
			// the new instrument plays in a new voice and the old instrument
			// escapes from the envelope sustain point (keyoff event), follows
			// the rest of the envelope and resets the virtual channel
			Mixer_Voice vi2 = p.Virt.Voice_Array[i];

			Assert.AreEqual(1, vi2.Ins, "Not new instrument");
			Assert.AreEqual(49, vi2.Note, "Not new note");
			Assert.AreEqual(33 * 16, vi2.Vol, "Not new instrument volume");
			Assert.AreEqual(0, vi2.Pos0, "Sample didn't reset");

			// Follow envelope
			// TODO: Check if envelope follows noteoff immediately or only
			//       in the next update!
			opaque.Xmp_Play_Frame();	// Frame 3

			Assert.AreEqual(4, vi.Chn, "Didn't copy channel");
			Assert.AreEqual(59, vi.Note, "Not same note");
			Assert.AreEqual(0, vi.Ins, "Not same instrument");
			Assert.AreEqual(21, vi.Vol / 16, "Not following envelope");
			Assert.AreNotEqual(0, vi2.Pos0, "Sample reset");

			opaque.Xmp_Play_Frame();	// Frame 4

			Assert.AreEqual(-1, vi.Chn, "Didn't end envelope");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
