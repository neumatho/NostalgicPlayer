﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Format;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Loader;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// 
	/// </summary>
	internal class S3M_Load : IFormatLoader
	{
		#region Internal structures
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

		#region S3M flags
		[Flags]
		private enum S3M_Flag : uint16
		{
			St2_Vib = 0x01,			// Not recognized
			St2_Tempo = 0x02,		// Not recognized
			Amiga_Slide = 0x04,		// Not recognized
			Vol_Opt = 0x08,			// Not recognized
			Amiga_Range = 0x10,
			Sb_Filter = 0x20,		// Not recognized
			St300_Vols = 0x40,
			Custom_Data = 0x80		// Not recognized
		}
		#endregion

		#region S3M_File_Header
		private class S3M_File_Header
		{
			/// <summary>
			/// Song name
			/// </summary>
			public uint8[] Name = new uint8[28];

			/// <summary>
			/// 0x1a
			/// </summary>
			public uint8 DosEof;

			/// <summary>
			/// File type
			/// </summary>
			public uint8 Type;

			/// <summary>
			/// Reserved
			/// </summary>
			public uint8[] Rsvd1 = new uint8[2];

			/// <summary>
			/// Number of orders (must be even)
			/// </summary>
			public uint16 OrdNum;

			/// <summary>
			/// Number of instruments
			/// </summary>
			public uint16 InsNum;

			/// <summary>
			/// Number of patterns
			/// </summary>
			public uint16 PatNum;

			/// <summary>
			/// Flags
			/// </summary>
			public S3M_Flag Flags;

			/// <summary>
			/// Tracker ID and version
			/// </summary>
			public uint16 Version;

			/// <summary>
			/// File format information
			/// </summary>
			public uint16 Ffi;

			/// <summary>
			/// 'SCRM'
			/// </summary>
			public uint32 Magic;

			/// <summary>
			/// Global volume
			/// </summary>
			public uint8 Gv;

			/// <summary>
			/// Initial speed
			/// </summary>
			public uint8 Is;

			/// <summary>
			/// Initial tempo
			/// </summary>
			public uint8 It;

			/// <summary>
			/// Master volume
			/// </summary>
			public uint8 Mv;

			/// <summary>
			/// Ultra click removal
			/// </summary>
			public uint8 Uc;

			/// <summary>
			/// Default pan positions if 0xfc
			/// </summary>
			public uint8 Dp;

			/// <summary>
			/// Reserved
			/// </summary>
			public uint8[] Rsvd2 = new uint8[8];

			/// <summary>
			/// Ptr to special custom data
			/// </summary>
			public uint16 Special;

			/// <summary>
			/// Channel settings
			/// </summary>
			public uint8[] ChSet = new uint8[32];
		}
		#endregion

		#region S3M_Instrument_Header
		private class S3M_Instrument_Header
		{
			/// <summary>
			/// DOS file name
			/// </summary>
			public uint8[] DosName = new uint8[12];

			/// <summary>
			/// High byte of sample pointer
			/// </summary>
			public uint8 MemSeg_Hi;

			/// <summary>
			/// Pointer to sample data
			/// </summary>
			public uint16 MemSeg;

			/// <summary>
			/// Length
			/// </summary>
			public uint32 Length;

			/// <summary>
			/// Loop begin
			/// </summary>
			public uint32 LoopBeg;

			/// <summary>
			/// Loop end
			/// </summary>
			public uint32 LoopEnd;

			/// <summary>
			/// Volume
			/// </summary>
			public uint8 Vol;

			/// <summary>
			/// Reserved
			/// </summary>
			public uint8 Rsvd1;

			/// <summary>
			/// Packing type (not used)
			/// </summary>
			public uint8 Pack;

			/// <summary>
			/// Loop/stereo/16 bit samples flags
			/// </summary>
			public uint8 Flags;

			/// <summary>
			/// C 4 speed
			/// </summary>
			public uint16 C2Spd;

			/// <summary>
			/// Reserved
			/// </summary>
			public uint16 Rsvd2;

			/// <summary>
			/// Reserved
			/// </summary>
			public uint8[] Rsvd3 = new uint8[4];

			/// <summary>
			/// Internal - GUS pointer
			/// </summary>
			public uint16 Int_Gp;

			/// <summary>
			/// Internal - SB pointer
			/// </summary>
			public uint16 Int_512;

			/// <summary>
			/// Internal - SB index
			/// </summary>
			public uint32 Int_Last;

			/// <summary>
			/// Instrument name
			/// </summary>
			public uint8[] Name = new uint8[28];

			/// <summary>
			/// 'SCRS'
			/// </summary>
			public uint32 Magic;
		}
		#endregion

		#region S3M_Adlib_Header
		private class S3M_Adlib_Header
		{
			/// <summary>
			/// DOS file name
			/// </summary>
			public uint8[] DosName = new uint8[12];

			/// <summary>
			/// 0x00 0x00 0x00
			/// </summary>
			public uint8[] Rsvd1 = new uint8[3];

			/// <summary>
			/// Adlib registers
			/// </summary>
			public uint8[] Reg = new uint8[12];

			/// <summary>
			/// 
			/// </summary>
			public uint8 Vol;

			/// <summary>
			/// 
			/// </summary>
			public uint8 Dsk;

			/// <summary>
			/// 
			/// </summary>
			public uint8[] Rsvd2 = new uint8[2];

			/// <summary>
			/// C 4 speed
			/// </summary>
			public uint16 C2Spd;

			/// <summary>
			/// Reserved
			/// </summary>
			public uint16 Rsvd3;

			/// <summary>
			/// Reserved
			/// </summary>
			public uint8[] Rsvd4 = new uint8[12];

			/// <summary>
			/// Instrument name
			/// </summary>
			public uint8[] Name = new uint8[28];

			/// <summary>
			/// 'SCRI'
			/// </summary>
			public uint32 Magic;
		}
		#endregion

#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
		#endregion

		private static readonly uint32 Magic_SCRM = Common.Magic4('S', 'C', 'R', 'M');
		private static readonly uint32 Magic_SCRI = Common.Magic4('S', 'C', 'R', 'I');
		private static readonly uint32 Magic_SCRS = Common.Magic4('S', 'C', 'R', 'S');

		// S3M packed pattern
		private const c_int S3M_Eor = 0;				// End of row
		private const c_int S3M_Ch_Mask = 0x1f;			// Channel
		private const c_int S3M_Ni_Follow = 0x20;		// Note and instrument follow
		private const c_int S3M_Vol_Follows = 0x40;		// Volume follows
		private const c_int S3M_Fx_Follows = 0x80;		// Effect and parameter follow

		// S3M mix volume
		private const c_int S3M_Mv_Volume = 0x7f;		// Module mix volume, typically 16 to 127
		private const c_int S3M_Mv_Stereo = 0x80;		// Module is stereo if set, otherwise mono

		// S3M channel pan
		private const c_int S3M_Pan_Set = 0x20;
		private const c_int S3M_Pan_Mask = 0x0f;

		// S3M channel info
		private const c_int S3M_Ch_On = 0x80;			// Psi says it's bit 8, I'll assume bit 7
		private const c_int S3M_Ch_Off = 0xff;
		private const c_int S3M_Ch_Number = 0x1f;
		private const c_int S3M_Ch_Right = 0x08;
		private const c_int S3M_Ch_Adlib = 0x10;

		private const uint8 None = 0xff;
		private const uint8 Fx_S3M_Extended = 0xfe;

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_S3M = new Format_Loader
		{
			Id = Guid.Parse("EB0B4765-CA32-43A3-AC3A-93ED4907498B"),
			Name = "Scream Tracker 3",
			Create = Create
		};

		//XX Skal lave et nyt format for hver del type, f.eks. OpenMPT og Schism Tracker

		private static readonly uint8[] fx = new uint8[27]
		{
			None,
			Effects.Fx_S3M_Speed,		// Axx: Set speed to xx (the default is 06)
			Effects.Fx_Jump,			// Bxx: Jump to order xx (hexadecimal)
			Effects.Fx_Break,			// Cxx: Break pattern to row xx (decimal)
			Effects.Fx_VolSlide,		// Dxy: Volume slide down by y/up by x
			Effects.Fx_Porta_Dn,		// Exx: Slide down by xx
			Effects.Fx_Porta_Up,		// Fxx: Slide up by xx
			Effects.Fx_TonePorta,		// Gxx: Tone portamento with speed xx
			Effects.Fx_Vibrato,			// Hxy: Vibrato with speed x and depth y
			Effects.Fx_Tremor,			// Ixy: Tremor with ontime x and offtime y
			Effects.Fx_S3M_Arpeggio,	// Jxy: Arpeggio with halftone additions
			Effects.Fx_Vibra_VSlide,	// Kxy: Dual command: H00 and Dxy
			Effects.Fx_Tone_VSlide,		// Lxy: Dual command: G00 and Dxy
			None,
			None,
			Effects.Fx_Offset,			// Oxy: Set sample offset
			None,
			Effects.Fx_Multi_Retrig,	// Qxy: Retrig (+volumeslide) note
			Effects.Fx_Tremolo,			// Rxy: Tremolo with speed x and depth y
			Fx_S3M_Extended,			// Sxx: (misc effects)
			Effects.Fx_S3M_Bpm,			// Txx: Tempo = xx (hex)
			Effects.Fx_Fine_Vibrato,	// Uxx: Fine vibrato
			Effects.Fx_GlobalVol,		// Vxx: Set global volume
			None,
			Effects.Fx_SetPan,			// Xxx: Set pan
			None,
			None
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private S3M_Load(LibXmp libXmp)
		{
			lib = libXmp;
			encoder = EncoderCollection.Dos;
		}




		/********************************************************************/
		/// <summary>
		/// Create a new instance of the loader
		/// </summary>
		/********************************************************************/
		public static IFormatLoader Create(LibXmp libXmp)
		{
			return new S3M_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			f.Hio_Seek(start + 44, SeekOrigin.Begin);
			if (f.Hio_Read32B() != Magic_SCRM)
				return -1;

			f.Hio_Seek(start + 29, SeekOrigin.Begin);
			if (f.Hio_Read8() != 0x10)
				return -1;

			f.Hio_Seek(start, SeekOrigin.Begin);
			lib.common.LibXmp_Read_Title(f, out t, 28, encoder);

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
			c_int i;
			Xmp_Event dummy = new Xmp_Event();
			S3M_File_Header sfh = new S3M_File_Header();
			S3M_Instrument_Header sih = new S3M_Instrument_Header();
			S3M_Adlib_Header sah = new S3M_Adlib_Header();
			uint16[] pp_Ins;				// Parapointers to instruments
			uint16[] pp_Pat;                // Parapointers to patterns
			bool stereo;
			uint8[] buf = new uint8[96];

			if (f.Hio_Read(buf, 1, 96) != 96)
				goto Err;

			Array.Copy(buf, sfh.Name, 28);	// Song name
			sfh.Type = buf[30];												// File type
			sfh.OrdNum = DataIo.ReadMem16L(buf, 32);					// Number of orders (must be even)
			sfh.InsNum = DataIo.ReadMem16L(buf, 34);					// Number of instruments
			sfh.PatNum = DataIo.ReadMem16L(buf, 36);					// Number of patterns
			sfh.Flags = (S3M_Flag)DataIo.ReadMem16L(buf, 38);			// Flags
			sfh.Version = DataIo.ReadMem16L(buf, 40);					// Tracker ID and version
			sfh.Ffi = DataIo.ReadMem16L(buf, 42);						// File format information

			// Sanity check
			if ((sfh.Ffi != 1) && (sfh.Ffi != 2))
				goto Err;

			if ((sfh.OrdNum > 255) || (sfh.InsNum > 255) || (sfh.PatNum > 255))
				goto Err;

			sfh.Magic = DataIo.ReadMem32B(buf, 44);					// 'SCRM'
			sfh.Gv = buf[48];												// Global volume
			sfh.Is = buf[49];												// Initial speed
			sfh.It = buf[50];												// Initial tempo
			sfh.Mv = buf[51];												// Master volume
			sfh.Uc = buf[52];												// Ultra click removal
			sfh.Dp = buf[53];												// Default pan positions if 0xfc
			Array.Copy(buf, 54, sfh.Rsvd2, 0, 8);	// Reserved
			sfh.Special = DataIo.ReadMem16L(buf, 62);					// Ptr to special custom data
			Array.Copy(buf, 64, sfh.ChSet, 0, 32);	// Channel settings

			if (sfh.Magic != Magic_SCRM)
				goto Err;

			lib.common.LibXmp_Copy_Adjust(out mod.Name, sfh.Name, 28, encoder);

			pp_Ins = new uint16[sfh.InsNum];
			if (pp_Ins == null)
				goto Err;

			pp_Pat = new uint16[sfh.PatNum];
			if (pp_Pat == null)
				goto Err2;

			if ((sfh.Flags & S3M_Flag.Amiga_Range) != 0)
				m.Period_Type = Containers.Common.Period.ModRng;

			if ((sfh.Flags & S3M_Flag.St300_Vols) != 0)
				m.Quirk |= Quirk_Flag.VsAll;

			mod.Spd = sfh.Is;
			mod.Bpm = sfh.It;
			mod.Chn = 0;

			// Mix volume and stereo flag conversion (reported by Saga Musix).
			// 1) Old format uses mix volume 0-7, and the stereo flag is 0x10.
			// 2) Newer ST3s unconditionally convert MV 0x02 and 0x12 to 0x20
			m.MVolBase = 48;

			if (sfh.Ffi == 1)
			{
				m.MVol = ((sfh.Mv & 0xf) + 1) * 0x10;
				stereo = (sfh.Mv & 0x10) != 0;
				Ports.LibXmp.Common.Clamp(ref m.MVol, 0x10, 0x7f);
			}
			else if ((sfh.Mv == 0x02) || (sfh.Mv == 0x12))
			{
				m.MVol = 0x20;
				stereo = (sfh.Mv & 0x10) != 0;
			}
			else
			{
				m.MVol = sfh.Mv & S3M_Mv_Volume;
				stereo = (sfh.Mv & S3M_Mv_Stereo) != 0;

				if (m.MVol == 0)
					m.MVol = 48;		// Default is 48
				else if (m.MVol < 16)
					m.MVol = 16;		// Minimum is 16
			}

			// "Note that in stereo, the mastermul is internally multiplied by
			// 11/8 inside the player since there is generally more room in the
			// output stream." Do the inverse to affect fewer modules
			if (!stereo)
				m.MVol = m.MVol * 8 / 11;

			for (i = 0; i < 32; i++)
			{
				if (sfh.ChSet[i] == S3M_Ch_Off)
					continue;

				mod.Chn = i + 1;

				c_int x = sfh.ChSet[i] & S3M_Ch_Number;
				if (stereo && (x < S3M_Ch_Adlib))
					mod.Xxc[i].Pan = x < S3M_Ch_Right ? 0x30 : 0xc0;
				else
					mod.Xxc[i].Pan = 0x80;
			}

			if (sfh.OrdNum <= Constants.Xmp_Max_Mod_Length)
			{
				mod.Len = sfh.OrdNum;

				if (f.Hio_Read(mod.Xxo, 1, (size_t)mod.Len) != (size_t)mod.Len)
					goto Err3;
			}
			else
			{
				mod.Len = Constants.Xmp_Max_Mod_Length;

				if (f.Hio_Read(mod.Xxo, 1, (size_t)mod.Len) != (size_t)mod.Len)
					goto Err3;

				if (f.Hio_Seek(sfh.OrdNum - Constants.Xmp_Max_Mod_Length, SeekOrigin.Current) < 0)
					goto Err3;
			}

			// Don't trust sfh.patnum
			mod.Pat = -1;

			for (i = 0; i < mod.Len; ++i)
			{
				if ((mod.Xxo[i] < 0xfe) && (mod.Xxo[i] > mod.Pat))
					mod.Pat = mod.Xxo[i];
			}

			mod.Pat++;

			if (mod.Pat > sfh.PatNum)
				mod.Pat = sfh.PatNum;

			if (mod.Pat == 0)
				goto Err3;

			mod.Trk = mod.Pat * mod.Chn;

			// Load and convert header
			mod.Ins = sfh.InsNum;
			mod.Smp = mod.Ins;

			for (i = 0; i < sfh.InsNum; i++)
				pp_Ins[i] = f.Hio_Read16L();

			for (i = 0; i < sfh.PatNum; i++)
				pp_Pat[i] = f.Hio_Read16L();

			// Default pan positions
			for (i = 0, sfh.Dp -= 0xfc; (sfh.Dp == 0) && (i < 32); i++)
			{
				uint8 x = f.Hio_Read8();
				if ((x & S3M_Pan_Set) != 0)
					mod.Xxc[i].Pan = (x << 4) & 0xff;
			}

			m.C4Rate = Constants.C4_Ntsc_Rate;

			if (sfh.Version == 0x1300)
				m.Quirk |= Quirk_Flag.VsAll;

			string tracker_Name;

			switch (sfh.Version >> 12)
			{
				case 1:
				{
					tracker_Name = string.Format("Scream Tracker {0}.{1:x2}", (sfh.Version & 0x0f00) >> 8, sfh.Version & 0xff);
					m.Quirk |= Quirk_Flag.St3Bugs;
					break;
				}

				case 2:
				{
					tracker_Name = string.Format("Imago Orpheus {0}.{1:x2}", (sfh.Version & 0x0f00) >> 8, sfh.Version & 0xff);
					break;
				}

				case 3:
				{
					if (sfh.Version == 0x3216)
						tracker_Name = "Impulse Tracker 2.14v3";
					else if (sfh.Version == 0x3217)
						tracker_Name = "Impulse Tracker 2.14v5";
					else
						tracker_Name = string.Format("Impulse Tracker {0}.{1:x2}", (sfh.Version & 0x0f00) >> 8, sfh.Version & 0xff);

					break;
				}

				case 5:
				{
					if (sfh.Version == 0x5447)
						tracker_Name = "Graoumf Tracker";
					else if ((sfh.Rsvd2[0] != 0) || (sfh.Rsvd2[1] != 0))
						tracker_Name = string.Format("OpenMPT {0}.{1:x2}.{2:x2}.{3:x2}", (sfh.Version & 0x0f00) >> 8, sfh.Version & 0xff, sfh.Rsvd2[1], sfh.Rsvd2[0]);
					else
						tracker_Name = string.Format("OpenMPT {0}.{1:x2}", (sfh.Version & 0x0f00) >> 8, sfh.Version & 0xff);

					m.Quirk |= Quirk_Flag.St3Bugs;
					break;
				}

				case 4:
				{
					if (sfh.Version != 0x4100)
					{
						lib.common.LibXmp_Schism_Tracker_String(out tracker_Name, 40, (sfh.Version & 0x0fff), sfh.Rsvd2[0] | (sfh.Rsvd2[1] << 8));
						break;
					}

					goto case 6;
				}

				case 6:
				{
					tracker_Name = string.Format("BeRoTracker {0}.{1:x2}", (sfh.Version & 0x0f00) >> 8, sfh.Version & 0xff);
					break;
				}

				default:
				{
					tracker_Name = string.Format("Unknown {0:x4", sfh.Version);
					break;
				}
			}

			lib.common.LibXmp_Set_Type(m, string.Format("{0} S3M", tracker_Name));

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				goto Err3;

			// Read patterns
			for (i = 0; i < mod.Pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, 64) < 0)
					goto Err3;

				if (pp_Pat[i] == 0)
					continue;

				f.Hio_Seek(start + pp_Pat[i] * 16, SeekOrigin.Begin);

				c_int r = 0;
				c_int pat_Len = f.Hio_Read16L() - 2;

				while ((pat_Len >= 0) && (r < mod.Xxp[i].Rows))
				{
					c_int b = f.Hio_Read8();

					if (f.Hio_Error() != 0)
						goto Err3;

					if (b == S3M_Eor)
					{
						r++;
						continue;
					}

					c_int c = b & S3M_Ch_Mask;
					Xmp_Event @event = c >= mod.Chn ? dummy : Ports.LibXmp.Common.Event(m, i, c, r);

					if ((b & S3M_Ni_Follow) != 0)
					{
						uint8 n = f.Hio_Read8();

						switch (n)
						{
							case 255:
							{
								n = 0;
								break;		// Empty note
							}

							case 254:
							{
								n = Constants.Xmp_Key_Off;
								break;		// Key off
							}

							default:
							{
								n = (uint8)(13 + 12 * Ports.LibXmp.Common.Msn(n) + Ports.LibXmp.Common.Lsn(n));
								break;
							}
						}

						@event.Note = n;
						@event.Ins = f.Hio_Read8();
						pat_Len -= 2;
					}

					if ((b & S3M_Vol_Follows) != 0)
					{
						@event.Vol = (byte)(f.Hio_Read8() + 1);
						pat_Len--;
					}

					if ((b & S3M_Fx_Follows) != 0)
					{
						@event.FxT = f.Hio_Read8();
						@event.FxP = f.Hio_Read8();
						Xlat_Fx(c, @event);

						pat_Len -= 2;
					}
				}
			}

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				goto Err3;

			// Read and convert instruments and samples
			for (i = 0; i < mod.Ins; i++)
			{
				Xmp_Instrument xxi = mod.Xxi[i];
				Xmp_Sample xxs = mod.Xxs[i];
				c_int ret;

				xxi.Sub = ArrayHelper.InitializeArray<Xmp_SubInstrument>(1);
				if (xxi.Sub == null)
					goto Err3;

				Xmp_SubInstrument sub = xxi.Sub[0];

				f.Hio_Seek(start + pp_Ins[i] * 16, SeekOrigin.Begin);

				sub.Pan = 0x80;
				sub.Sid = i;

				if (f.Hio_Read(buf, 1, 80) != 80)
					goto Err3;

				if (buf[0] >= 2)
				{
					// OPL2 FM instrument

					Array.Copy(buf, 1, sah.DosName, 0, 12);	// DOS file name
					Array.Copy(buf, 16, sah.Reg, 0, 12);		// Adlib registers
					sah.Vol = buf[28];
					sah.Dsk = buf[29];
					sah.C2Spd = DataIo.ReadMem16L(buf, 32);			// C4 speed
					Array.Copy(buf, 48, sah.Name, 0, 28);	// Instrument name
					sah.Magic = DataIo.ReadMem32B(buf, 76);			// 'SCRI'

					if (sah.Magic != Magic_SCRI)
						goto Err3;

					sah.Magic = 0;

					lib.common.LibXmp_Instrument_Name(mod, i, sah.Name, 28, encoder);

					xxi.Nsm = 1;
					sub.Vol = sah.Vol;

					lib.period.LibXmp_C2Spd_To_Note(sah.C2Spd, out sub.Xpo, out sub.Fin);
					sub.Xpo += 12;

					ret = Sample.LibXmp_Load_Sample(m, f, Sample_Flag.Adlib, xxs, sah.Reg, i);
					if (ret < 0)
						goto Err3;

					continue;
				}

				Array.Copy(buf, 1, sih.DosName, 0, 12);	// DOS file name
				sih.MemSeg_Hi = buf[13];									// High byte of sample pointer
				sih.MemSeg = DataIo.ReadMem16L(buf, 14);				// Pointer to sample data
				sih.Length = DataIo.ReadMem32L(buf, 16);				// Length

				if (sih.Length > Constants.Max_Sample_Size)
					goto Err3;

				sih.LoopBeg = DataIo.ReadMem32L(buf, 20);				// Loop begin
				sih.LoopEnd = DataIo.ReadMem32L(buf, 24);				// Loop end
				sih.Vol = buf[28];											// Volume
				sih.Pack = buf[30];											// Packing type
				sih.Flags = buf[31];										// Loop/stereo/16 bit flags
				sih.C2Spd = DataIo.ReadMem16L(buf, 32);				// C4 speed
				Array.Copy(buf, 48, sih.Name, 0, 28);	// Instrument name
				sih.Magic = DataIo.ReadMem32B(buf, 76);				// 'SCRS'

				if ((buf[0] == 1) && (sih.Magic != Magic_SCRS))
					goto Err3;

				xxs.Len = (c_int)sih.Length;
				xxi.Nsm = sih.Length > 0 ? 1 : 0;
				xxs.Lps = (c_int)sih.LoopBeg;
				xxs.Lpe = (c_int)sih.LoopEnd;

				xxs.Flg = (sih.Flags & 1) != 0 ? Xmp_Sample_Flag.Loop : Xmp_Sample_Flag.None;

				if ((sih.Flags & 4) != 0)
					xxs.Flg |= Xmp_Sample_Flag._16Bit;

				Sample_Flag load_Sample_Flag = (sfh.Ffi == 1) ? Sample_Flag.None : Sample_Flag.Uns;
				if (sih.Pack == 4)
					load_Sample_Flag = Sample_Flag.Adpcm;

				sub.Vol = sih.Vol;
				sih.Magic = 0;

				lib.common.LibXmp_Instrument_Name(mod, i, sih.Name, 28, encoder);

				lib.period.LibXmp_C2Spd_To_Note(sih.C2Spd, out sub.Xpo, out sub.Fin);

				uint32 sample_Segment = sih.MemSeg + ((uint32)sih.MemSeg_Hi << 16);

				if (f.Hio_Seek((c_long)(start + 16 * sample_Segment), SeekOrigin.Begin) < 0)
					goto Err3;

				ret = Sample.LibXmp_Load_Sample(m, f, load_Sample_Flag, xxs, null, i);
				if (ret < 0)
					goto Err3;
			}

			m.Quirk |= Quirk_Flag.St3 | Quirk_Flag.ArpMem;
			m.Read_Event_Type = Read_Event.St3;

			return 0;

			Err3:
			pp_Pat = null;
			Err2:
			pp_Ins = null;
			Err:
			return -1;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Xlat_Fx(c_int c, Xmp_Event e)
		{
			uint8 h = Ports.LibXmp.Common.Msn(e.FxP), l = Ports.LibXmp.Common.Lsn(e.FxP);

			if (e.FxT >= fx.Length)
			{
				e.FxT = e.FxP = 0;
				return;
			}

			switch (e.FxT = fx[e.FxT])
			{
				case Effects.Fx_S3M_Bpm:
				{
					if (e.FxP < 0x20)
						e.FxP = e.FxT = 0;

					break;
				}

				// Extended effects
				case Fx_S3M_Extended:
				{
					e.FxT = Effects.Fx_Extended;

					switch (h)
					{
						// Glissando
						case 0x1:
						{
							e.FxP = (byte)(Ports.LibXmp.Common.Lsn(e.FxP) | (Effects.Ex_Gliss << 4));
							break;
						}

						// Finetune
						case 0x2:
						{
							e.FxP = (byte)(((Ports.LibXmp.Common.Lsn(e.FxP) - 8) & 0x0f) | (Effects.Ex_FineTune << 4));
							break;
						}

						// Vibrato wave
						case 0x3:
						{
							e.FxP = (byte)(Ports.LibXmp.Common.Lsn(e.FxP) | (Effects.Ex_Vibrato_Wf << 4));
							break;
						}

						// Tremolo wave
						case 0x4:
						{
							e.FxP = (byte)(Ports.LibXmp.Common.Lsn(e.FxP) | (Effects.Ex_Tremolo_Wf << 4));
							break;
						}

						// Ignore
						case 0x5:
						case 0x6:
						case 0x7:
						case 0x9:
						case 0xa:
						{
							e.FxT = e.FxP = 0;
							break;
						}

						// Set pan
						case 0x8:
						{
							e.FxT = Effects.Fx_SetPan;
							e.FxP = (byte)(l << 4);
							break;
						}

						// Pattern loop
						case 0xb:
						{
							e.FxP = (byte)(Ports.LibXmp.Common.Lsn(e.FxP) | (Effects.Ex_Pattern_Loop << 4));
							break;
						}

						case 0xc:
						{
							if (l == 0)
								e.FxT = e.FxP = 0;

							break;
						}
					}
					break;
				}

				case Effects.Fx_SetPan:
				{
					// Saga Musix says: "The X effect in S3M files is not
					// exclusive to IT and clones. You will find tons of S3Ms made
					// with ST3 itself using this effect (and relying on an
					// external player being used). X in S3M also behaves
					// differently than in IT, which your code does not seem to
					// handle: X00 - X80 is left... right, XA4 is surround (like
					// S91 in IT), other values are not supposed to do anything
					if (e.FxP == 0xa4)
					{
						// Surround
						e.FxT = Effects.Fx_Surround;
						e.FxP = 1;
					}
					else
					{
						c_int pan = e.FxP << 1;
						if (pan > 0xff)
							pan = 0xff;

						e.FxP = (byte)pan;
					}
					break;
				}

				// No effect
				case None:
				{
					e.FxT = e.FxP = 0;
					break;
				}
			}
		}
		#endregion
	}
}
