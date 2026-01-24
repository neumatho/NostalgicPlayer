/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Format;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Iff;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Loader;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// 
	/// </summary>
	internal class Arch_Load : IFormatLoader
	{
		#region Internal structures

		#region Local_Data
		private class Local_Data
		{
			public c_int Year;
			public c_int Month;
			public c_int Day;
			public bool PFlag;
			public bool SFlag;
			public c_int Max_Ins;
			public c_int Max_Pat;
			public bool Has_MVox;
			public bool Has_PNum;
			public readonly uint8[] Ster = new uint8[8];
			public readonly uint8[] Rows = new uint8[64];
		}
		#endregion

		#endregion

		#region lib_Table
		/// <summary>
		/// Linear (0 to 0x40) to logarithmic volume conversion.
		/// This is only used for the Protracker-compatible "linear volume" effect in
		/// Andy Southgate's StasisMod. In this implementation linear and logarithmic
		/// volumes can be freely intermixed
		/// </summary>
		private static readonly uint8[] lin_Table =
		[
			0x00, 0x48, 0x64, 0x74, 0x82, 0x8a, 0x92, 0x9a,
			0xa2, 0xa6, 0xaa, 0xae, 0xb2, 0xb6, 0xea, 0xbe,
			0xc2, 0xc4, 0xc6, 0xc8, 0xca, 0xcc, 0xce, 0xd0,
			0xd2, 0xd4, 0xd6, 0xd8, 0xda, 0xdc, 0xde, 0xe0,
			0xe2, 0xe2, 0xe4, 0xe4, 0xe6, 0xe6, 0xe8, 0xe8,
			0xea, 0xea, 0xec, 0xec, 0xee, 0xee, 0xf0, 0xf0,
			0xf2, 0xf2, 0xf4, 0xf4, 0xf6, 0xf6, 0xf8, 0xf8,
			0xfa, 0xfa, 0xfc, 0xfc, 0xfe, 0xfe, 0xfe, 0xfe,
			0xfe
		];
		#endregion

		private static readonly uint32 Magic_MUSX = Common.Magic4('M', 'U', 'S', 'X');
		private static readonly uint32 Magic_MNAM = Common.Magic4('M', 'N', 'A', 'M');
		private static readonly uint32 Magic_SNAM = Common.Magic4('S', 'N', 'A', 'M');
		private static readonly uint32 Magic_SVOL = Common.Magic4('S', 'V', 'O', 'L');
		private static readonly uint32 Magic_SLEN = Common.Magic4('S', 'L', 'E', 'N');
		private static readonly uint32 Magic_ROFS = Common.Magic4('R', 'O', 'F', 'S');
		private static readonly uint32 Magic_RLEN = Common.Magic4('R', 'L', 'E', 'N');
		private static readonly uint32 Magic_SDAT = Common.Magic4('S', 'D', 'A', 'T');

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Arch = new Format_Loader
		{
			Id = Guid.Parse("0861053D-5A57-437F-9116-049BD1CCBB97"),
			Name = "Archimedes Tracker",
			Description = "Protracker like tracker for the Ahorn Archemedes computer.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Arch_Load(LibXmp libXmp)
		{
			lib = libXmp;
			encoder = EncoderCollection.Acorn;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new instance of the loader
		/// </summary>
		/********************************************************************/
		private static IFormatLoader Create(LibXmp libXmp, Xmp_Context ctx)
		{
			return new Arch_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			if (f.Hio_Read32B() != Magic_MUSX)
				return -1;

			f.Hio_Read32L();

			while (!f.Hio_Eof())
			{
				uint32 id = f.Hio_Read32B();
				uint32 len = f.Hio_Read32L();

				// Sanity check
				if (len > 0x100000)
					return -1;

				if (id == Magic_MNAM)
				{
					lib.common.LibXmp_Read_Title(f, out t, 32, encoder);
					return 0;
				}

				f.Hio_Seek((c_long)len, SeekOrigin.Current);
			}

			lib.common.LibXmp_Read_Title(f, out t, 0, encoder);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Loader(Module_Data m, Hio f, c_int start)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = new Local_Data();

			f.Hio_Read32B();    // MUSX
			f.Hio_Read32B();

			Iff handle = Iff.LibXmp_Iff_New();
			if (handle == null)
				return -1;

			// IFF chunk IDs
			handle.LibXmp_Iff_Register("TINF".ToPointer(), Get_Tinf);
			handle.LibXmp_Iff_Register("MVOX".ToPointer(), Get_Mvox);
			handle.LibXmp_Iff_Register("STER".ToPointer(), Get_Ster);
			handle.LibXmp_Iff_Register("MNAM".ToPointer(), Get_Mnam);
			handle.LibXmp_Iff_Register("ANAM".ToPointer(), Get_Anam);
			handle.LibXmp_Iff_Register("MLEN".ToPointer(), Get_Mlen);
			handle.LibXmp_Iff_Register("PNUM".ToPointer(), Get_Pnum);
			handle.LibXmp_Iff_Register("PLEN".ToPointer(), Get_Plen);
			handle.LibXmp_Iff_Register("SEQU".ToPointer(), Get_Sequ);
			handle.LibXmp_Iff_Register("PATT".ToPointer(), Get_Patt);
			handle.LibXmp_Iff_Register("SAMP".ToPointer(), Get_Samp);

			handle.LibXmp_Iff_Set_Quirk(Iff_Quirk_Flag.Little_Endian);

			// Load IFF chunks
			if (handle.LibXmp_Iff_Load(m, f, data) < 0)
			{
				handle.LibXmp_Iff_Release();
				return -1;
			}

			handle.LibXmp_Iff_Release();

			for (c_int i = 0; i < mod.Chn; i++)
				mod.Xxc[i].Pan = Common.DefPan(m, (((i + 3) / 2) % 2) * 0xff);

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Fix_Effect(Xmp_Event e)
		{
			switch (e.FxT)
			{
				// 00 xy Normal play or Arpeggio
				case 0x00:
				{
					e.FxT = Effects.Fx_Arpeggio;
					// x: first halfnote to add
					// y: second halftone to subtract
					break;
				}

				// 01 xx Slide up
				case 0x01:
				{
					e.FxT = Effects.Fx_Porta_Up;
					break;
				}

				// 02 xx Slide down
				case 0x02:
				{
					e.FxT = Effects.Fx_Porta_Dn;
					break;
				}

				// 03 xx Tone Portamento
				case 0x03:
				{
					e.FxT = Effects.Fx_TonePorta;
					break;
				}

				// 0B xx Break Pattern
				case 0x0b:
				{
					e.FxT = Effects.Fx_Break;
					break;
				}

				case 0x0c:
				{
					// Set linear volume
					if (e.FxP <= 64)
					{
						e.FxT = Effects.Fx_VolSet;
						e.FxP = lin_Table[e.FxP];
					}
					else
						e.FxP = e.FxT = 0;

					break;
				}

				// 0E xy Set stereo
				case 0x0e:

				// StasisMod's non-standard set panning effect
				// y: stereo position (1-7, ignored). 1 = left, 4 = center, 7 = right
				case 0x19:
				{
					if ((e.FxP > 0) && (e.FxP < 8))
					{
						e.FxT = Effects.Fx_SetPan;
						e.FxP = (byte)(42 * e.FxP - 40);
					}
					else
						e.FxT = e.FxP = 0;

					break;
				}

				// 10 xx Volume Slide Up
				case 0x10:
				{
					e.FxT = Effects.Fx_VolSlide_Up;
					break;
				}

				// 11 xx Volume Slide Down
				case 0x11:
				{
					e.FxT = Effects.Fx_VolSlide_Dn;
					break;
				}

				// 13 xx Position Jump
				case 0x13:
				{
					e.FxT = Effects.Fx_Jump;
					break;
				}

				// 15 xy Line Jump. (not in manual)
				case 0x15:
				{
					// Jump to line 10*x+y in same pattern. (10*x+y>63 ignored)
					if ((Ports.LibXmp.Common.Msn(e.FxP) * 10 + Ports.LibXmp.Common.Lsn(e.FxP)) < 64)
					{
						e.FxT = Effects.Fx_Line_Jump;
						e.FxP = (byte)(Ports.LibXmp.Common.Msn(e.FxP) * 10 + Ports.LibXmp.Common.Lsn(e.FxP));
					}
					else
						e.FxT = e.FxP = 0;

					break;
				}

				// 1C xy Set Speed
				case 0x1c:
				{
					e.FxT = Effects.Fx_Speed;
					break;
				}

				// 1F xx Set Volume
				case 0x1f:
				{
					e.FxT = Effects.Fx_VolSet;

					// All volumes are logarithmic
					break;
				}

				default:
				{
					e.FxT = e.FxP = 0;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Tinf(Module_Data m, c_int size, Hio f, object parm)
		{
			Local_Data data = (Local_Data)parm;

			c_int x = f.Hio_Read8();
			data.Year = (((x & 0xf0) >> 4) * 10) + (x & 0x0f);
			x = f.Hio_Read8();
			data.Year += (((x & 0xf0) >> 4) * 1000) + ((x & 0x0f) * 100);

			x = f.Hio_Read8();
			data.Month = (((x & 0xf0) >> 4) * 10) + (x & 0x0f);

			x = f.Hio_Read8();
			data.Day = (((x & 0xf0) >> 4) * 10) + (x & 0x0f);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Mvox(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;

			uint32 chn = f.Hio_Read32L();

			// Sanity check
			if ((chn < 1) || (chn > 8) || data.Has_MVox)
				return -1;

			mod.Chn = (c_int)chn;
			data.Has_MVox = true;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Ster(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;

			if (f.Hio_Read(data.Ster, 1, 8) != 8)
				return -1;

			for (c_int i = 0; i < mod.Chn; i++)
			{
				if ((data.Ster[i] > 0) && (data.Ster[i] < 8))
					mod.Xxc[i].Pan = 42 * data.Ster[i] - 40;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Mnam(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			byte[] buf = new byte[32];

			if (f.Hio_Read(buf, 1, 32) != 32)
				return -1;

			for (c_int i = 0; (i < 32) && (buf[i] != 0x00); i++)
			{
				if (buf[i] == 0x01)
				{
					buf[i] = 0x00;
					break;
				}
			}

			mod.Name = encoder.GetString(buf, 0, 32);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Anam(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			byte[] buf = new byte[32];

			if (f.Hio_Read(buf, 1, 32) != 32)
				return -1;

			for (c_int i = 0; (i < 32) && (buf[i] != 0x00); i++)
			{
				if (buf[i] == 0x01)
				{
					buf[i] = 0x00;
					break;
				}
			}

			mod.Author = encoder.GetString(buf, 0, 32);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Mlen(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;

			uint32 len = f.Hio_Read32L();

			// Sanity check
			if (len > 0xff)
				return -1;

			mod.Len = (c_int)len;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Pnum(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;

			uint32 pat = f.Hio_Read32L();

			// Sanity check
			if ((pat < 1) || (pat > 64) || data.Has_PNum)
				return -1;

			mod.Pat = (c_int)pat;
			data.Has_PNum = true;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Plen(Module_Data m, c_int size, Hio f, object parm)
		{
			Local_Data data = (Local_Data)parm;

			if (f.Hio_Read(data.Rows, 1, 64) != 64)
				return -1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Sequ(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;

			f.Hio_Read(mod.Xxo, 1, 128);
			lib.common.LibXmp_Set_Type(m, "Archimedes Tracker");

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Patt(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;

			// Sanity check
			if (!data.Has_MVox || !data.Has_PNum)
				return -1;

			if (!data.PFlag)
			{
				data.PFlag = true;
				data.Max_Pat = 0;

				mod.Trk = mod.Pat * mod.Chn;

				if (lib.common.LibXmp_Init_Pattern(mod) < 0)
					return -1;
			}

			// Sanity check
			if ((data.Max_Pat >= mod.Pat) || (data.Max_Pat >= 64))
				return -1;

			c_int i = data.Max_Pat;

			if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, data.Rows[i]) < 0)
				return -1;

			for (c_int j = 0; j < data.Rows[i]; j++)
			{
				for (c_int k = 0; k < mod.Chn; k++)
				{
					Xmp_Event @event = Ports.LibXmp.Common.Event(m, i, k, j);

					@event.FxP = f.Hio_Read8();
					@event.FxT = f.Hio_Read8();
					@event.Ins = f.Hio_Read8();
					@event.Note = f.Hio_Read8();

					if (@event.Note != 0)
						@event.Note += 48;

					Fix_Effect(@event);
				}
			}

			data.Max_Pat++;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Samp(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;

			if (!data.SFlag)
			{
				mod.Smp = mod.Ins = 36;

				if (lib.common.LibXmp_Init_Instrument(m) < 0)
					return -1;

				data.SFlag = true;
				data.Max_Ins = 0;
			}

			// FIXME: More than 36 sample slots used. Unfortunately we
			// have no way to handle this without two passes, and it's
			// officially supposed to be 36, so ignore the rest
			if (data.Max_Ins >= 36)
				return 0;

			c_int i = data.Max_Ins;

			mod.Xxi[i].Nsm = 1;

			if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
				return -1;

			if (f.Hio_Read32B() != Magic_SNAM)		// SNAM
				return -1;

			{
				// Should usually be 0x14 but zero is not unknown
				c_int name_Len = (c_int)f.Hio_Read32L();

				// Sanity check
				if ((name_Len < 0) || (name_Len > 32))
					return -1;

				uint8[] name = new uint8[name_Len + 1];
				f.Hio_Read(name, 1, (size_t)name_Len);

				mod.Xxi[i].Name = encoder.GetString(name, 0, name_Len);
			}

			if (f.Hio_Read32B() != Magic_SVOL)      // SVOL
				return -1;

			f.Hio_Read32L();

			mod.Xxi[i].Sub[0].Vol = (c_int)(f.Hio_Read32L() & 0xff);

			if (f.Hio_Read32B() != Magic_SLEN)      // SLEN
				return -1;

			f.Hio_Read32L();

			mod.Xxs[i].Len = (c_int)f.Hio_Read32L();

			if (f.Hio_Read32B() != Magic_ROFS)      // ROFS
				return -1;

			f.Hio_Read32L();

			mod.Xxs[i].Lps = (c_int)f.Hio_Read32L();

			if (f.Hio_Read32B() != Magic_RLEN)      // RLEN
				return -1;

			f.Hio_Read32L();

			mod.Xxs[i].Lpe = (c_int)f.Hio_Read32L();

			if (f.Hio_Read32B() != Magic_SDAT)      // SDAT
				return -1;

			f.Hio_Read32L();
			f.Hio_Read32L();    // 0x00000000

			mod.Xxi[i].Sub[0].Sid = i;
			mod.Xxi[i].Sub[0].Pan = 0x80;

			m.Vol_Table = VolTable.LibXmp_Arch_Vol_Table;
			m.VolBase = 0xff;

			// Clean bad loops
			if ((mod.Xxs[i].Lps < 0) || (mod.Xxs[i].Lps >= mod.Xxs[i].Len))
				mod.Xxs[i].Lps = mod.Xxs[i].Lpe = 0;

			if (mod.Xxs[i].Lpe > 2)
			{
				if (mod.Xxs[i].Lpe > (mod.Xxs[i].Len - mod.Xxs[i].Lps))
					mod.Xxs[i].Lpe = mod.Xxs[i].Len - mod.Xxs[i].Lps;

				mod.Xxs[i].Flg = Xmp_Sample_Flag.Loop;
				mod.Xxs[i].Lpe = mod.Xxs[i].Lps + mod.Xxs[i].Lpe;
			}
			else if ((mod.Xxs[i].Lpe == 2) && (mod.Xxs[i].Lps > 0))
			{
				// Non-zero repeat offset and repeat length of 2
				// means loop to end of sample
				mod.Xxs[i].Flg = Xmp_Sample_Flag.Loop;
				mod.Xxs[i].Lpe = mod.Xxs[i].Len;
			}

			if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.Vidc, mod.Xxs[i], null, i) < 0)
				return -1;

			data.Max_Ins++;

			return 0;
		}
		#endregion
	}
}
