/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal Xmp_Context GetContext(Ports.LibXmp.LibXmp opaque)
		{
			return (Xmp_Context)new PrivateObject(opaque).GetField("ctx");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Create_Simple_Module(Ports.LibXmp.LibXmp opaque, c_int ins, c_int pat)
		{
			Xmp_Context ctx = GetContext(opaque);
			Module_Data m = ctx.M;

			opaque.loadHelpers.LibXmp_Load_Prologue();
			Xmp_Module mod = m.Mod;

			// Create module

			mod.Len = 2;
			mod.Pat = pat;
			mod.Ins = ins;
			mod.Chn = 4;
			mod.Trk = mod.Pat * mod.Chn;
			mod.Smp = mod.Ins;
			mod.Xxo[0] = 0;
			mod.Xxo[1] = 1;

			opaque.common.LibXmp_Init_Pattern(mod);

			for (c_int i = 0; i < mod.Pat; i++)
				opaque.common.LibXmp_Alloc_Pattern_Tracks(mod, i, 64);

			opaque.common.LibXmp_Init_Instrument(m);

			for (c_int i = 0; i < mod.Ins; i++)
			{
				mod.Xxi[i].Nsm = 1;
				opaque.common.LibXmp_Alloc_SubInstrument(mod, i, 1);

				mod.Xxi[i].Sub[0].Pan = 0x80;
				mod.Xxi[i].Sub[0].Vol = 0x40;
				mod.Xxi[i].Sub[0].Sid = i;

				mod.Xxs[i].Len = 10000;
				mod.Xxs[i].Lps = 0;
				mod.Xxs[i].Lpe = 10000;
				mod.Xxs[i].Flg = Xmp_Sample_Flag.Loop;
				mod.Xxs[i].Data = new byte[11000];
				mod.Xxs[i].DataOffset = 4;
			}

			// End of module creation

			opaque.loadHelpers.LibXmp_Load_Epilogue();
			opaque.loadHelpers.LibXmp_Prepare_Scan();
			opaque.scan.LibXmp_Scan_Sequences();

			ctx.State = Xmp_State.Loaded;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void New_Event(Ports.LibXmp.LibXmp opaque, c_int pat, c_int row, c_int chn, c_int note, c_int ins, c_int vol, c_int fxT, c_int fxP, c_int f2T, c_int f2P)
		{
			Xmp_Context ctx = GetContext(opaque);

			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			c_int track = mod.Xxp[pat].Index[chn];
			Xmp_Event e = mod.Xxt[track].Event[row];

			e.Note = (byte)note;
			e.Ins = (byte)ins;
			e.Vol = (byte)vol;
			e.FxT = (byte)fxT;
			e.FxP = (byte)fxP;
			e.F2T = (byte)f2T;
			e.F2P = (byte)f2P;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Set_Order(Ports.LibXmp.LibXmp opaque, c_int pos, c_int pat)
		{
			Xmp_Context ctx = GetContext(opaque);
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			mod.Xxo[pos] = (byte)pat;
			mod.Len = pos + 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Set_Instrument_Volume(Ports.LibXmp.LibXmp opaque, c_int ins, c_int sub, c_int vol)
		{
			Xmp_Context ctx = GetContext(opaque);
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			mod.Xxi[ins].Sub[sub].Vol = vol;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Set_Instrument_Nna(Ports.LibXmp.LibXmp opaque, c_int ins, c_int sub, Xmp_Inst_Nna nna, Xmp_Inst_Dct dct, Xmp_Inst_Dca dca)
		{
			Xmp_Context ctx = GetContext(opaque);
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			mod.Xxi[ins].Sub[sub].Nna = nna;
			mod.Xxi[ins].Sub[sub].Dct = dct;
			mod.Xxi[ins].Sub[sub].Dca = dca;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Set_Instrument_Envelope(Ports.LibXmp.LibXmp opaque, c_int ins, c_int node, c_int x, c_int y)
		{
			Xmp_Context ctx = GetContext(opaque);
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			mod.Xxi[ins].Aei.Data[node * 2] = (c_short)x;
			mod.Xxi[ins].Aei.Data[node * 2 + 1] = (c_short)y;

			mod.Xxi[ins].Aei.Npt = node + 1;
			mod.Xxi[ins].Aei.Flg |= Xmp_Envelope_Flag.On;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Set_Instrument_Envelope_Loop(Ports.LibXmp.LibXmp opaque, c_int ins, c_int lps, c_int lpe)
		{
			Xmp_Context ctx = GetContext(opaque);
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			mod.Xxi[ins].Aei.Lps = lps;
			mod.Xxi[ins].Aei.Lpe = lpe;
			mod.Xxi[ins].Aei.Flg |= Xmp_Envelope_Flag.Loop;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Set_Instrument_Envelope_Sus(Ports.LibXmp.LibXmp opaque, c_int ins, c_int sus)
		{
			Xmp_Context ctx = GetContext(opaque);
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			mod.Xxi[ins].Aei.Sus = sus;
			mod.Xxi[ins].Aei.Sue = sus;
			mod.Xxi[ins].Aei.Flg |= Xmp_Envelope_Flag.Sus | Xmp_Envelope_Flag.SLoop;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Set_Instrument_FadeOut(Ports.LibXmp.LibXmp opaque, c_int ins, c_int fade)
		{
			Xmp_Context ctx = GetContext(opaque);
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			mod.Xxi[ins].Rls = fade;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal void Set_Period_Type(Ports.LibXmp.LibXmp opaque, Period type)
		{
			Xmp_Context ctx = GetContext(opaque);
			Module_Data m = ctx.M;

			m.Period_Type = type;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal void Set_Quirk(Ports.LibXmp.LibXmp opaque, Quirk_Flag quirk, Read_Event read_Mode)
		{
			Xmp_Context ctx = GetContext(opaque);
			Module_Data m = ctx.M;

			m.Quirk |= quirk;
			m.Read_Event_Type = read_Mode;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal void Reset_Quirk(Ports.LibXmp.LibXmp opaque, Quirk_Flag quirk)
		{
			Xmp_Context ctx = GetContext(opaque);
			Module_Data m = ctx.M;

			m.Quirk &= ~quirk;
		}
	}
}
