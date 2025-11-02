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
	internal class Imf_Load : IFormatLoader
	{
		#region Internal structures

		#region Imf_Channel
		private class Imf_Channel
		{
			public uint8[] Name { get; } = new uint8[12];		// Channel name (ASCIIZ-String, max 11 chars)
			public uint8 Status { get; set; }					// Channel status
			public uint8 Pan { get; set; }						// Pan positions
			public uint8 Chorus { get; set; }					// Default chorus
			public uint8 Reverb { get; set; }					// Default reverb
		}
		#endregion

		#region Imf_Header
		private class Imf_Header
		{
			public uint8[] Name { get; } = new uint8[32];		// Song name (ASCIIZ-String, max. 31 chars)
			public uint16 Len { get; set; }						// Number of orders saved
			public uint16 Pat { get; set; }						// Number of patterns saved
			public uint16 Ins { get; set; }						// Number of instruments saved
			public uint16 Flg { get; set; }						// Module flags
			public uint8[] Unused1 { get; } = new uint8[8];
			public uint8 Tpo { get; set; }						// Default tempo (1..255)
			public uint8 Bpm { get; set; }						// Default beats per minute (BPM) (32..255)
			public uint8 Vol { get; set; }						// Default master volume (0..64)
			public uint8 Amp { get; set; }						// Amplification factor (4..127)
			public uint8[] Unused2 { get; } = new uint8[8];
			public uint32 Magic { get; set; }					// 'IM10'
			public Imf_Channel[] Chn { get; } = ArrayHelper.InitializeArray<Imf_Channel>(32);	// Channel settings
			public uint8[] Pos { get; } = new uint8[256];		// Order list
		}
		#endregion

		#region Imf_Env
		private class Imf_Env
		{
			public uint8 Npt { get; set; }						// Number of envelope points
			public uint8 Sus { get; set; }						// Envelope sustain point
			public uint8 Lps { get; set; }						// Envelope loop start point
			public uint8 Lpe { get; set; }						// Envelope loop end point
			public uint8 Flg { get; set; }						// Envelope flags
			public uint8[] Unused { get; } = new uint8[3];
		}
		#endregion

		#region Imf_Instrument
		private class Imf_Instrument
		{
			public uint8[] Name { get; } = new uint8[32];		// Inst. name (ASCIIZ-String, max. 31 chars)
			public uint8[] Map { get; } = new uint8[120];		// Multisample settings
			public uint8[] Unused { get; } = new uint8[8];
			public uint16[] Vol_Env { get; } = new uint16[32];	// Volume envelope settings
			public uint16[] Pan_Env { get; } = new uint16[32];	// Pan envelope settings
			public uint16[] Pitch_Env { get; } = new uint16[32];// Pitch envelope settings
			public Imf_Env[] Env { get; } = ArrayHelper.InitializeArray<Imf_Env>(3);
			public uint16 Fadeout { get; set; }					// Fadeout rate (0...0FFFH)
			public uint16 Nsm { get; set; }						// Number of samples in instrument
			public uint32 Magic { get; set; }					// 'II10'
		}
		#endregion

		#region Imf_Sample_Flag
		[Flags]
		private enum Imf_Sample_Flag : uint8
		{
			Loop = 0x01,
			Bidi = 0x02,
			_16Bit = 0x04,
			DefPan = 0x08
		}
		#endregion

		#region Imf_Sample
		private class Imf_Sample
		{
			public uint8[] Name { get; } = new uint8[13];		// Sample filename (12345678.ABC)
			public uint8[] Unused1 { get; } = new uint8[3];
			public uint32 Len { get; set; }						// Length
			public uint32 Lps { get; set; }						// Loop start
			public uint32 Lpe { get; set; }						// Loop end
			public uint32 Rate { get; set; }					// Sample rate
			public uint8 Vol { get; set; }						// Default volume (0..64)
			public uint8 Pan { get; set; }						// Default pan (00h = Left / 80h = Middle)
			public uint8[] Unused2 { get; } = new uint8[14];
			public Imf_Sample_Flag Flg { get; set; }			// Sample flags
			public uint8[] Unused3 { get; } = new uint8[5];
			public uint16 Ems { get; set; }						// Reserved for internal usage
			public uint32 DRam { get; set; }					// Reserved for internal usage
			public uint32 Magic { get; set; }					// 'IS10'
		}
		#endregion

		#endregion

		private const c_int Imf_Eor = 0x00;
		private const c_int Imf_Ch_Mask = 0x1f;
		private const c_int Imf_Ni_Follow = 0x20;
		private const c_int Imf_Fx_Follows = 0x80;
		private const c_int Imf_F2_Follows = 0x40;

		// ReSharper disable InconsistentNaming
		private static readonly uint32 Magic_IM10 = Common.Magic4('I', 'M', '1', '0');
		private static readonly uint32 Magic_II10 = Common.Magic4('I', 'I', '1', '0');
		private static readonly uint32 Magic_IS10 = Common.Magic4('I', 'S', '1', '0');
		private static readonly uint32 Magic_IW10 = Common.Magic4('I', 'W', '1', '0');		// Leaving all behind.imf
		// ReSharper restore InconsistentNaming

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Imf = new Format_Loader
		{
			Id = Guid.Parse("F0906D97-B9B3-451E-870D-A97529D38480"),
			Name = "Imago Orpheus",
			Description = "This loader recognizes “Imago Orpheus” modules. This format is roughly equivalent to the XM format, but with two effects columns instead of a volume column and an effect column.\n\nImago Orpheus was written by Lutz Roeder and released in 1994.",
			Create = Create
		};

		private const uint8 None = 0xff;
		private const uint8 Fx_Imf_FPorta_Up = 0xfe;
		private const uint8 Fx_Imf_FPorta_Dn = 0xfd;

		// Effect conversion table
		private static readonly uint8[] fx =
		[
			None,
			Effects.Fx_S3M_Speed,
			Effects.Fx_S3M_Bpm,
			Effects.Fx_TonePorta,
			Effects.Fx_Tone_VSlide,
			Effects.Fx_Vibrato,
			Effects.Fx_Vibra_VSlide,
			Effects.Fx_Fine_Vibrato,
			Effects.Fx_Tremolo,
			Effects.Fx_S3M_Arpeggio,
			Effects.Fx_SetPan,
			Effects.Fx_PanSlide,
			Effects.Fx_VolSet,
			Effects.Fx_VolSlide,
			Effects.Fx_F_VSlide,
			Effects.Fx_FineTune,
			Effects.Fx_NSlide_Up,
			Effects.Fx_NSlide_Dn,
			Effects.Fx_Porta_Up,
			Effects.Fx_Porta_Dn,
			Fx_Imf_FPorta_Up,
			Fx_Imf_FPorta_Dn,
			Effects.Fx_Flt_CutOff,
			Effects.Fx_Flt_Resn,
			Effects.Fx_Offset,
			None,					// Fine offset
			Effects.Fx_Keyoff,
			Effects.Fx_Multi_Retrig,
			Effects.Fx_Tremor,
			Effects.Fx_Jump,
			Effects.Fx_Break,
			Effects.Fx_GlobalVol,
			Effects.Fx_GVol_Slide,
			Effects.Fx_Extended,
			Effects.Fx_Chorus,
			Effects.Fx_Reverb
		];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Imf_Load(LibXmp libXmp)
		{
			lib = libXmp;
			encoder = EncoderCollection.Dos;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new instance of the loader
		/// </summary>
		/********************************************************************/
		private static IFormatLoader Create(LibXmp libXmp, Xmp_Context ctx)
		{
			return new Imf_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			f.Hio_Seek(start + 60, SeekOrigin.Begin);

			if (f.Hio_Read32B() != Magic_IM10)
				return -1;

			f.Hio_Seek(start, SeekOrigin.Begin);
			lib.common.LibXmp_Read_Title(f, out t, 32, encoder);

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
			Imf_Header ih = new Imf_Header();
			Imf_Instrument ii = new Imf_Instrument();
			Imf_Sample @is = new Imf_Sample();
			c_int smp_Num;

			// Load and convert header
			f.Hio_Read(ih.Name, 32, 1);
			ih.Len = f.Hio_Read16L();
			ih.Pat = f.Hio_Read16L();
			ih.Ins = f.Hio_Read16L();
			ih.Flg = f.Hio_Read16L();
			f.Hio_Read(ih.Unused1, 8, 1);
			ih.Tpo = f.Hio_Read8();
			ih.Bpm = f.Hio_Read8();
			ih.Vol = f.Hio_Read8();
			ih.Amp = f.Hio_Read8();
			f.Hio_Read(ih.Unused2, 8, 1);
			ih.Magic = f.Hio_Read32B();

			// Sanity check
			if ((ih.Len > 256) || (ih.Pat > 256) || (ih.Ins > 255))
				return -1;

			for (i = 0; i < 32; i++)
			{
				f.Hio_Read(ih.Chn[i].Name, 12, 1);
				ih.Chn[i].Chorus = f.Hio_Read8();
				ih.Chn[i].Reverb = f.Hio_Read8();
				ih.Chn[i].Pan = f.Hio_Read8();
				ih.Chn[i].Status = f.Hio_Read8();
			}

			if (f.Hio_Read(ih.Pos, 256, 1) < 1)
				return -1;

			if (ih.Magic != Magic_IM10)
				return -1;

			lib.common.LibXmp_Copy_Adjust(out mod.Name, ih.Name, 32, encoder);

			mod.Len = ih.Len;
			mod.Ins = ih.Ins;
			mod.Smp = 1024;
			mod.Pat = ih.Pat;

			if ((ih.Flg & 0x01) != 0)
				m.Period_Type = Containers.Common.Period.Linear;

			mod.Spd = ih.Tpo;
			mod.Bpm = ih.Bpm;

			lib.common.LibXmp_Set_Type(m, "Imago Orpheus 1.0 IMF");

			mod.Chn = 0;

			for (i = 0; i < 32; i++)
			{
				// 0=enabled; 1=muted, but still processed; 2=disabled
				if (ih.Chn[i].Status >= 2)
					continue;

				mod.Chn = i + 1;
				mod.Xxc[i].Pan = ih.Chn[i].Pan;
			}

			mod.Trk = mod.Pat * mod.Chn;

			CMemory.memcpy<uint8>(mod.Xxo, ih.Pos, (size_t)mod.Len);

			for (i = 0; i < mod.Len; i++)
			{
				if (mod.Xxo[i] == 0xff)
					mod.Xxo[i]--;
			}

			m.C4Rate = Constants.C4_Ntsc_Rate;

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				return -1;

			// Read patterns
			for (i = 0; i < mod.Pat; i++)
			{
				c_int pat_Len = f.Hio_Read16L() - 4;

				c_int rows = f.Hio_Read16L();

				// Sanity check
				if (rows > 256)
					return -1;

				if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, rows) < 0)
					return -1;

				c_int r = 0;

				while (--pat_Len >= 0)
				{
					uint8 b = f.Hio_Read8();

					if (b == Imf_Eor)
					{
						r++;
						continue;
					}

					// Sanity check
					if (r >= rows)
						return -1;

					c_int c = b & Imf_Ch_Mask;
					Xmp_Event @event = (c >= mod.Chn) ? dummy : Ports.LibXmp.Common.Event(m, i, c, r);

					if ((b & Imf_Ni_Follow) != 0)
					{
						uint8 n = f.Hio_Read8();

						switch (n)
						{
							case 255:
							case 160:		// ?!
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

					if ((b & Imf_Fx_Follows) != 0)
					{
						@event.FxT = f.Hio_Read8();
						@event.FxP = f.Hio_Read8();

						Xlat_Fx(c, ref @event.FxT, ref @event.FxP);
						pat_Len -= 2;
					}

					if ((b & Imf_F2_Follows) != 0)
					{
						@event.F2T = f.Hio_Read8();
						@event.F2P = f.Hio_Read8();

						Xlat_Fx(c, ref @event.F2T, ref @event.F2P);
						pat_Len -= 2;
					}
				}
			}

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			// Read and convert instruments and samples
			for (smp_Num = i = 0; i < mod.Ins; i++)
			{
				Xmp_Instrument xxi = mod.Xxi[i];

				f.Hio_Read(ii.Name, 32, 1);
				ii.Name[31] = 0;

				f.Hio_Read(ii.Map, 120, 1);
				f.Hio_Read(ii.Unused, 8, 1);

				for (c_int j = 0; j < 32; j++)
					ii.Vol_Env[j] = f.Hio_Read16L();

				for (c_int j = 0; j < 32; j++)
					ii.Pan_Env[j] = f.Hio_Read16L();

				for (c_int j = 0; j < 32; j++)
					ii.Pitch_Env[j] = f.Hio_Read16L();

				for (c_int j = 0; j < 3; j++)
				{
					ii.Env[j].Npt = f.Hio_Read8();
					ii.Env[j].Sus = f.Hio_Read8();
					ii.Env[j].Lps = f.Hio_Read8();
					ii.Env[j].Lpe = f.Hio_Read8();
					ii.Env[j].Flg = f.Hio_Read8();

					f.Hio_Read(ii.Env[j].Unused, 3, 1);
				}

				ii.Fadeout = f.Hio_Read16L();
				ii.Nsm = f.Hio_Read16L();
				ii.Magic = f.Hio_Read32B();

				// Sanity check
				if (ii.Nsm > 255)
					return -1;

				// Imago Orpheus may emit blank instruments with a signature
				// of four nuls. Found in "Leaving all behind.imf" by Karsten Koch
				if ((ii.Magic != Magic_II10) && (ii.Magic != 0))
					return -2;

				xxi.Nsm = ii.Nsm;

				if (xxi.Nsm > 0)
				{
					if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, xxi.Nsm) < 0)
						return -1;
				}

				xxi.Name = encoder.GetString(ii.Name, 0, 31);

				for (c_int j = 0; j < 108; j++)
					xxi.Map[j + 12].Ins = ii.Map[j];

				xxi.Aei.Npt = ii.Env[0].Npt;
				xxi.Aei.Sus = ii.Env[0].Sus;
				xxi.Aei.Lps = ii.Env[0].Lps;
				xxi.Aei.Lpe = ii.Env[0].Lpe;
				xxi.Aei.Flg = (ii.Env[0].Flg & 0x01) != 0 ? Xmp_Envelope_Flag.On : 0;
				xxi.Aei.Flg |= (ii.Env[0].Flg & 0x02) != 0 ? Xmp_Envelope_Flag.Sus : 0;
				xxi.Aei.Flg |= (ii.Env[0].Flg & 0x04) != 0 ? Xmp_Envelope_Flag.Loop : 0;

				// Sanity check
				if (xxi.Aei.Npt > 16)
					return -1;

				for (c_int j = 0; j < xxi.Aei.Npt; j++)
				{
					xxi.Aei.Data[j * 2] = (c_short)ii.Vol_Env[j * 2];
					xxi.Aei.Data[j * 2 + 1] = (c_short)ii.Vol_Env[j * 2 + 1];
				}

				for (c_int j = 0; j < ii.Nsm; j++, smp_Num++)
				{
					Xmp_SubInstrument sub = xxi.Sub[j];
					Xmp_Sample xxs = mod.Xxs[smp_Num];

					f.Hio_Read(@is.Name, 13, 1);
					f.Hio_Read(@is.Unused1, 3, 1);
					@is.Len = f.Hio_Read32L();
					@is.Lps = f.Hio_Read32L();
					@is.Lpe = f.Hio_Read32L();
					@is.Rate = f.Hio_Read32L();
					@is.Vol = f.Hio_Read8();
					@is.Pan = f.Hio_Read8();
					f.Hio_Read(@is.Unused2, 14, 1);
					@is.Flg = (Imf_Sample_Flag)f.Hio_Read8();
					f.Hio_Read(@is.Unused3, 5, 1);
					@is.Ems = f.Hio_Read16L();
					@is.DRam = f.Hio_Read32L();
					@is.Magic = f.Hio_Read32B();

					if ((@is.Magic != Magic_IS10) && (@is.Magic != Magic_IW10))
						return -1;

					// Sanity check
					if ((@is.Len > 0x100000) || (@is.Lps > 0x100000) || (@is.Lpe > 0x100000))
						return -1;

					sub.Sid = smp_Num;
					sub.Vol = @is.Vol;
					sub.Pan = (@is.Flg & Imf_Sample_Flag.DefPan) != 0 ? @is.Pan : -1;
					xxs.Len = (c_int)@is.Len;
					xxs.Lps = (c_int)@is.Lps;
					xxs.Lpe = (c_int)@is.Lpe;
					xxs.Flg = Xmp_Sample_Flag.None;

					if ((@is.Flg & Imf_Sample_Flag.Loop) != 0)
						xxs.Flg |= Xmp_Sample_Flag.Loop;

					if ((@is.Flg & Imf_Sample_Flag.Bidi) != 0)
						xxs.Flg |= Xmp_Sample_Flag.Loop_BiDir;

					if ((@is.Flg & Imf_Sample_Flag._16Bit) != 0)
					{
						xxs.Flg |= Xmp_Sample_Flag._16Bit;
						xxs.Len >>= 1;
						xxs.Lps >>= 1;
						xxs.Lpe >>= 1;
					}

					lib.period.LibXmp_C2Spd_To_Note((c_int)@is.Rate, out sub.Xpo, out sub.Fin);

					if (xxs.Len <= 0)
						continue;

					c_int sid = sub.Sid;

					if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, mod.Xxs[sid], null, smp_Num) < 0)
						return -1;
				}
			}

			if (lib.common.LibXmp_Realloc_Samples(m, smp_Num) < 0)
				return -1;

			m.C4Rate = Constants.C4_Ntsc_Rate;
			m.Quirk |= Quirk_Flag.Filter | Quirk_Flag.St3 | Quirk_Flag.ArpMem;
			m.Flow_Mode = FlowMode_Flag.Mode_Orpheus;
			m.Read_Event_Type = Read_Event.St3;

			m.GVol = ih.Vol;
			m.MVol = ih.Amp;
			m.MVolBase = 48;
			Ports.LibXmp.Common.Clamp(ref m.MVol, 4, 127);

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Effect translation
		/// </summary>
		/********************************************************************/
		private void Xlat_Fx(c_int c, ref uint8 fxT, ref uint8 fxP)
		{
			uint8 h = Ports.LibXmp.Common.Msn(fxP), l = Ports.LibXmp.Common.Lsn(fxP);

			if (fxT >= fx.Length)
			{
				fxT = fxP = 0;
				return;
			}

			switch (fxT = fx[fxT])
			{
				case Fx_Imf_FPorta_Up:
				{
					fxT = Effects.Fx_Porta_Up;

					if (fxP < 0x30)
						fxP = (uint8)(Ports.LibXmp.Common.Lsn(fxP >> 2) | 0xe0);
					else
						fxP = (uint8)(Ports.LibXmp.Common.Lsn(fxP >> 4) | 0xf0);

					break;
				}

				case Fx_Imf_FPorta_Dn:
				{
					fxT = Effects.Fx_Porta_Dn;

					if (fxP < 0x30)
						fxP = (uint8)(Ports.LibXmp.Common.Lsn(fxP >> 2) | 0xe0);
					else
						fxP = (uint8)(Ports.LibXmp.Common.Lsn(fxP >> 4) | 0xf0);

					break;
				}

				// Extended effects
				case Effects.Fx_Extended:
				{
					switch (h)
					{
						case 0x1:	// Set filter
						case 0x2:	// Undefined
						case 0x4:	// Undefined
						case 0x6:	// Undefined
						case 0x7:	// Undefined
						case 0x9:	// Undefined
						case 0xe:	// Ignore envelope
						case 0xf:	// Invert loop
						{
							fxP = fxT = 0;
							break;
						}

						// Glissando
						case 0x3:
						{
							fxP = (uint8)(l | (Effects.Ex_Gliss << 4));
							break;
						}

						// Vibrato waveform
						case 0x5:
						{
							fxP = (uint8)(l | (Effects.Ex_Vibrato_Wf << 4));
							break;
						}

						// Tremolo waveform
						case 0x8:
						{
							fxP = (uint8)(l | (Effects.Ex_Tremolo_Wf << 4));
							break;
						}

						// Pattern loop
						case 0xa:
						{
							fxP = (uint8)(l | (Effects.Ex_Pattern_Loop << 4));
							break;
						}

						// Pattern delay
						case 0xb:
						{
							fxP = (uint8)(l | (Effects.Ex_Patt_Delay << 4));
							break;
						}

						case 0xc:
						{
							if (l == 0)
								fxT = fxP = 0;

							break;
						}
					}

					break;
				}

				// No effect
				case None:
				{
					fxT = fxP = 0;
					break;
				}
			}
		}
		#endregion
	}
}
