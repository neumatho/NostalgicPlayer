/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test
{
	/// <summary>
	/// Common functions for the read event test_new_note_*,
	/// test_no_note_*, and test_porta_* regression tests
	/// </summary>
	public partial class Test
	{
		/// <summary></summary>
		protected const c_int Key_C4 = 49;
		/// <summary></summary>
		protected const c_int Key_D4 = 51;
		/// <summary></summary>
		protected const c_int Key_C5 = 61;
		/// <summary></summary>
		protected const c_int Key_B5 = 72;

		/// <summary></summary>
		protected const c_int Ins_0 = 1;
		/// <summary></summary>
		protected const c_int Ins_1 = 2;
		/// <summary></summary>
		protected const c_int Ins_Inval = 3;

		/// <summary></summary>
		protected const c_int Ins_0_Sub_0_Vol = 22;
		/// <summary></summary>
		protected const c_int Ins_0_Sub_1_Vol = 11;
		/// <summary></summary>
		protected const c_int Ins_1_Sub_0_Vol = 33;

		/// <summary></summary>
		protected const c_int Ins_0_Sub_0_Pan = 0x40;
		/// <summary></summary>
		protected const c_int Ins_0_Sub_1_Pan = 0xc0;
		/// <summary></summary>
		protected const c_int Ins_1_Sub_0_Pan = 0xfe;

		/// <summary></summary>
		protected const c_int Ins_0_Fade = 0x400;
		/// <summary></summary>
		protected const c_int Ins_1_Fade = 0x1ffe;

		/// <summary></summary>
		protected const c_int Set_Vol = 43;
		/// <summary></summary>
		protected const c_int Set_Pan = 0x80;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Create_Read_Event_Test_Module(Ports.LibXmp.LibXmp opaque, c_int speed)
		{
			// Need to do this first, else the M and Mod below will not be set
			Create_Simple_Module(opaque, 2, 2);

			Xmp_Context ctx = GetContext(opaque);
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			mod.Spd = speed;
			opaque.scan.LibXmp_Scan_Sequences();	// Rescan for new speed

			// Instrument 1: Activate second subinstrument
			Xmp_Instrument xxi = mod.Xxi[0];
			xxi.Map[Key_B5 - 1].Ins = 1;

			Set_Instrument_Volume(opaque, 0, 0, Ins_0_Sub_0_Vol);
			Set_Instrument_Volume(opaque, 0, 1, Ins_0_Sub_1_Vol);
			Set_Instrument_Panning(opaque, 0, 0, Ins_0_Sub_0_Pan);
			Set_Instrument_Panning(opaque, 0, 1, Ins_0_Sub_1_Pan);
			Set_Instrument_FadeOut(opaque, 0, Ins_0_Fade);

			// Instrument 2: Map invalid subinstrument to B-5
			xxi = mod.Xxi[1];
			xxi.Map[Key_B5 - 1].Ins = 2;

			Set_Instrument_Volume(opaque, 1, 0, Ins_1_Sub_0_Vol);
			Set_Instrument_Panning(opaque, 1, 0, Ins_1_Sub_0_Pan);
			Set_Instrument_FadeOut(opaque, 1, Ins_1_Fade);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal void Check_Active(Channel_Data xc, Mixer_Voice vi, c_int note, c_int ins, c_int vol, c_int pan, c_int ins_Fade, string desc)
		{
			Assert.IsGreaterThanOrEqualTo(0, vi.Chn, $"{desc}: mixer voice not active");

			string msg = $"{desc}: note (xc:{xc.Note} vi:{vi.Note}) != expected {note - 1}";
			Assert.AreEqual(note - 1, xc.Note, msg);
			Assert.AreEqual(note - 1, vi.Note, msg);

			if (ins >= 1)
			{
				msg = $"{desc}: ins (xc:{xc.Ins} vi:{vi.Ins}) != expected {ins - 1}";
				Assert.AreEqual(ins - 1, xc.Ins, msg);
				Assert.AreEqual(ins - 1, vi.Ins, msg);
			}

			if (vol >= 0)
			{
				msg = $"{desc}: volume (xc:{xc.Volume} vi:{vi.Vol}) != expected {vol}";
				Assert.AreEqual(vol, xc.Volume, msg);
				Assert.AreEqual(vol << 4, vi.Vol, msg);
			}

			if (pan >= 0)
			{
				msg = $"{desc}: pan (xc:{xc.Pan.Val} vi:{(sbyte)(vi.Pan + 0x80)}) != expected {pan}";
				Assert.AreEqual(pan, xc.Pan.Val, msg);
				Assert.AreEqual((sbyte)(pan + 0x80), vi.Pan, msg);
			}

			if (ins_Fade >= 0)
				Assert.AreEqual(ins_Fade, xc.Ins_Fade, $"{desc}: fade rate {xc.Ins_Fade} != expected {ins_Fade}");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal void Check_New(Channel_Data xc, Mixer_Voice vi, c_int note, c_int ins, c_int vol, c_int pan, c_int ins_Fade, string desc)
		{
			Check_Active(xc, vi, note, ins, vol, pan, ins_Fade, desc);

			// New notes have an initial mixer sample position of 0
			Assert.AreEqual(0, vi.Pos0, $"{desc}: mixer sample pos {vi.Pos0} != expected 0");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal void Check_On(Channel_Data xc, Mixer_Voice vi, c_int note, c_int ins, c_int vol, c_int pan, c_int ins_Fade, string desc)
		{
			Check_Active(xc, vi, note, ins, vol, pan, ins_Fade, desc);

			// Note should not be new, but rather continued from another line
			Assert.AreNotEqual(0, vi.Pos0, $"{desc}: mixer sample pos {vi.Pos0} != expected !0");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal void Check_Off(Channel_Data xc, Mixer_Voice vi, string desc)
		{
			// Fully off mixer channels should be unmapped and have volume 0
			Assert.IsLessThan(0, vi.Chn, $"{desc}: mixer voice is active");

			Assert.AreEqual(0, vi.Vol, $"{desc}: mixer voice volume {vi.Vol} != 0");
		}
	}
}
