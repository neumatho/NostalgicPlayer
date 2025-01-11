/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal class Load_Helpers
	{
		private class Module_Quirk
		{
			public uint8[] Md5;
			public Xmp_Flags Flags;
			public Xmp_Mode Mode;
		}

		private static readonly Module_Quirk[] mq =
		[
			new Module_Quirk
			{
				Md5 = new byte[16],
				Flags = Xmp_Flags.None,
				Mode = 0
			}
		];

		private readonly LibXmp lib;
		private readonly Xmp_Context ctx;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Load_Helpers(LibXmp libXmp, Xmp_Context ctx)
		{
			lib = libXmp;
			this.ctx = ctx;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Adjust_String(ref string s)
		{
			char[] c = s.ToCharArray();
			int strLen = c.Length;

			if (strLen > 0)
			{
				for (c_int i = 0; i < strLen; i++)
				{
					if ((uint8)c[i] < 32)
						c[i] = ' ';
				}

				while ((strLen > 0) && (c[strLen - 1] == ' '))
					strLen--;

				s = new string(c, 0, strLen);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Load_Prologue()
		{
			Module_Data m = ctx.M;

			// Reset variables
			m.Mod = new Xmp_Module();
			m.RRate = Constants.Pal_Rate;
			m.C4Rate = Constants.C4_Pal_Rate;
			m.VolBase = 0x40;
			m.GVol = m.GVolBase = 0x40;
			m.Vol_Table = null;
			m.Quirk = 0;
			m.Read_Event_Type = Read_Event.Mod;
			m.Period_Type = Containers.Common.Period.Amiga;
			m.Compare_VBlank = false;
			m.Comment = null;
			m.Scan_Cnt = null;
			m.Midi = null;
			m.Module_Flags = Xmp_Module_Flags.None;

			// Set defaults
			m.Mod.Pat = 0;
			m.Mod.Trk = 0;
			m.Mod.Chn = 4;
			m.Mod.Ins = 0;
			m.Mod.Smp = 0;
			m.Mod.Spd = 6;
			m.Mod.Bpm = 125;
			m.Mod.Len = 0;
			m.Mod.Rst = 0;

			m.Extra = null;

			m.Time_Factor = Constants.Default_Time_Factor;

			for (c_int i = 0; i < 64; i++)
			{
				c_int pan = (((i + 1) / 2) % 2) * 0xff;

				m.Mod.Xxc[i].Pan = 0x80 + (pan - 0x80) * m.DefPan / 100;
				m.Mod.Xxc[i].Vol = 0x40;
				m.Mod.Xxc[i].Flg = Xmp_Channel_Flag.None;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Load_Epilogue()
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			mod.Gvl = m.GVol;

			// Sanity check for module parameters
			Common.Clamp(ref mod.Len, 0, Constants.Xmp_Max_Mod_Length);
			Common.Clamp(ref mod.Pat, 0, 257);	// Some formats have an extra pattern
			Common.Clamp(ref mod.Ins, 0, 255);
			Common.Clamp(ref mod.Smp, 0, Constants.Max_Samples);
			Common.Clamp(ref mod.Chn, 0, Constants.Xmp_Max_Channels);

			// Fix cases where the restart value is invalid e.g. kc_fall8.xm
			// from http://aminet.net/mods/mvp/mvp_0002.lha (reported by
			// Ralf Hoffmann <ralf@boomerangsworld.de>)
			if (mod.Rst >= mod.Len)
				mod.Rst = 0;

			// Sanity check for tempo and BPM
			if ((mod.Spd <= 0) || (mod.Spd > 255))
				mod.Spd = 6;

			Common.Clamp(ref mod.Bpm, Constants.Xmp_Min_Bpm, 1000);

			// Set appropriate values for instrument volumes and subinstrument
			// global volumes when QUIRK_INSVOL is not set, to keep volume values
			// consistent if the user inspects struct xmp_module. We can later
			// set volumes in the loaders and eliminate the quirk
			for (c_int i = 0; i < mod.Ins; i++)
			{
				if ((~m.Quirk & Quirk_Flag.InsVol) != 0)
					mod.Xxi[i].Vol = m.VolBase;

				for (c_int j = 0; j < mod.Xxi[i].Nsm; j++)
				{
					if ((~m.Quirk & Quirk_Flag.InsVol) != 0)
						mod.Xxi[i].Sub[j].Gvl = m.VolBase;
				}
			}

			// Sanity check for envelopes
			for (c_int i = 0; i < mod.Ins; i++)
			{
				Check_Envelope(mod.Xxi[i].Aei);
				Check_Envelope(mod.Xxi[i].Fei);
				Check_Envelope(mod.Xxi[i].Pei);
				Clamp_Volume_Envelope(m, mod.Xxi[i].Aei);
			}

			// TODO: There's no unintrusive and clean way to get this struct into
			// libxmp_load_sample currently, so bound these fields here for now
			for (c_int i = 0; i < mod.Smp; i++)
			{
				Xmp_Sample xxs = mod.Xxs[i];
				Extra_Sample_Data xtra = m.Xtra[i];

				if (xtra.Sus < 0)
					xtra.Sus = 0;

				if (xtra.Sue > xxs.Len)
					xtra.Sue = xxs.Len;

				if ((xtra.Sus >= xxs.Len) || (xtra.Sus >= xtra.Sue))
				{
					xtra.Sus = xtra.Sue = 0;
					xxs.Flg &= ~(Xmp_Sample_Flag.SLoop | Xmp_Sample_Flag.SLoop_BiDir);
				}
			}

			p.Filter = false;
			p.Mode = Xmp_Mode.Auto;
			p.Flags = p.Player_Flags;

			Module_Quirks();
			LibXmp_Set_Player_Mode();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Prepare_Scan()
		{
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			if ((mod.Xxp == null) || (mod.Xxt == null))
				return -(c_int)Xmp_Error.Load;

			c_int ord = 0;
			while ((ord < mod.Len) && (mod.Xxo[ord] >= mod.Pat))
				ord++;

			if (ord >= mod.Len)
			{
				mod.Len = 0;
				return 0;
			}

			m.Scan_Cnt = new uint8[mod.Len][];
			if (m.Scan_Cnt == null)
				return -(c_int)Xmp_Error.System;

			for (c_int i = 0; i < mod.Len; i++)
			{
				c_int pat_Idx = mod.Xxo[i];

				// Add pattern if referenced in orders
				if ((pat_Idx < mod.Pat) && (mod.Xxp[pat_Idx] == null))
				{
					if (lib.common.LibXmp_Alloc_Pattern(mod, pat_Idx) < 0)
						return -(c_int)Xmp_Error.System;
				}

				Xmp_Pattern pat = pat_Idx >= mod.Pat ? null : mod.Xxp[pat_Idx];
				m.Scan_Cnt[i] = new uint8[(pat != null) && (pat.Rows != 0) ? pat.Rows : 1];
				if (m.Scan_Cnt[i] == null)
					return -(c_int)Xmp_Error.System;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Free_Scan()
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			if (m.Scan_Cnt != null)
				m.Scan_Cnt = null;

			p.Scan = null;
		}



		/********************************************************************/
		/// <summary>
		/// Process player personality flags
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Set_Player_Mode()
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Quirk_Flag q;

			switch (p.Mode)
			{
				case Xmp_Mode.Auto:
					break;

				case Xmp_Mode.Mod:
				{
					m.C4Rate = Constants.C4_Pal_Rate;
					m.Quirk = Quirk_Flag.None;
					m.Read_Event_Type = Read_Event.Mod;
					m.Period_Type = Containers.Common.Period.Amiga;
					break;
				}

				case Xmp_Mode.NoiseTracker:
				{
					m.C4Rate = Constants.C4_Pal_Rate;
					m.Quirk = Quirk_Flag.NoBpm;
					m.Read_Event_Type = Read_Event.Mod;
					m.Period_Type = Containers.Common.Period.ModRng;
					break;
				}

				case Xmp_Mode.ProTracker:
				{
					m.C4Rate = Constants.C4_Pal_Rate;
					m.Quirk = Quirk_Flag.ProTrack;
					m.Read_Event_Type = Read_Event.Mod;
					m.Period_Type = Containers.Common.Period.ModRng;
					break;
				}

				case Xmp_Mode.S3M:
				{
					q = m.Quirk & (Quirk_Flag.VsAll | Quirk_Flag.ArpMem);
					m.C4Rate = Constants.C4_Ntsc_Rate;
					m.Quirk = Quirk_Flag.St3 | q;
					m.Read_Event_Type = Read_Event.St3;
					break;
				}

				case Xmp_Mode.St3:
				{
					q = m.Quirk & (Quirk_Flag.VsAll | Quirk_Flag.ArpMem);
					m.C4Rate = Constants.C4_Ntsc_Rate;
					m.Quirk = Quirk_Flag.St3 | Quirk_Flag.St3Bugs | q;
					m.Read_Event_Type = Read_Event.St3;
					break;
				}

				case Xmp_Mode.St3Gus:
				{
					q = m.Quirk & (Quirk_Flag.VsAll | Quirk_Flag.ArpMem);
					m.C4Rate = Constants.C4_Ntsc_Rate;
					m.Quirk = Quirk_Flag.St3 | Quirk_Flag.St3Bugs | q;
					m.Quirk &= ~Quirk_Flag.RstChn;
					m.Read_Event_Type = Read_Event.St3;
					break;
				}

				case Xmp_Mode.Xm:
				{
					m.C4Rate = Constants.C4_Ntsc_Rate;
					m.Quirk = Quirk_Flag.Ft2;
					m.Read_Event_Type = Read_Event.Ft2;
					break;
				}

				case Xmp_Mode.Ft2:
				{
					m.C4Rate = Constants.C4_Ntsc_Rate;
					m.Quirk = Quirk_Flag.Ft2 | Quirk_Flag.Ft2Bugs;
					m.Read_Event_Type = Read_Event.Ft2;
					break;
				}

				case Xmp_Mode.It:
				{
					m.C4Rate = Constants.C4_Ntsc_Rate;
					m.Quirk = Quirk_Flag.It | Quirk_Flag.VibHalf | Quirk_Flag.VibInv;
					m.Read_Event_Type = Read_Event.It;
					break;
				}

				case Xmp_Mode.ItSmp:
				{
					m.C4Rate = Constants.C4_Ntsc_Rate;
					m.Quirk = Quirk_Flag.It | Quirk_Flag.VibHalf | Quirk_Flag.VibInv;
					m.Quirk &= ~(Quirk_Flag.Virtual | Quirk_Flag.RstChn);
					m.Read_Event_Type = Read_Event.It;
					break;
				}

				default:
					return -1;
			}

			if (p.Mode != Xmp_Mode.Auto)
				m.Compare_VBlank = false;

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Module_Quirks()
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;

			for (c_int i = 0; (mq[i].Flags != Xmp_Flags.None) || (mq[i].Mode != 0); i++)
			{
				if (ArrayHelper.ArrayCompare(m.Md5, 0, mq[i].Md5, 0, 16))
				{
					p.Flags |= mq[i].Flags;
					p.Mode = mq[i].Mode;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Check_Envelope(Xmp_Envelope env)
		{
			// Disable envelope if invalid number of points
			if ((env.Npt <= 0) || (env.Npt > Constants.Xmp_Max_Env_Points))
				env.Flg &= ~Xmp_Envelope_Flag.On;

			// Disable envelope loop if invalid loop parameters
			if ((env.Lps >= env.Npt) || (env.Lpe >= env.Npt))
				env.Flg &= ~Xmp_Envelope_Flag.Loop;

			// Disable envelope sustain if invalid sustain
			if ((env.Sus >= env.Npt) || (env.Sue >= env.Npt))
				env.Flg &= ~Xmp_Envelope_Flag.Sus;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Clamp_Volume_Envelope(Module_Data m, Xmp_Envelope env)
		{
			// Clamp broken values in the volume envelope to the expected range
			if ((env.Flg & Xmp_Envelope_Flag.On) != 0)
			{
				for (c_int i = 0; i < env.Npt; i++)
					Common.Clamp(ref env.Data[i * 2 + 1], 0, (int16)m.VolBase);
			}
		}
		#endregion
	}
}
