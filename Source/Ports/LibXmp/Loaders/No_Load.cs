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
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Loader;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// Nir Oren's Liquid Tracker old "NO" format
	/// </summary>
	internal class No_Load : IFormatLoader
	{
		#region Tables
		// Note that 0.80b or slightly earlier started translating these in the
		// editor to the new Liquid Module effects. It's not clear if the saving
		// format was modified at this point, but 0.80b and 0.82b are not aware
		// of the Liquid Module format.
		// 
		// TODO: the changelog alleges an Fxx (05h) global volume effect was added
		// in 0.67b, which would be present in releases 0.68b and 0.69b only
		private static readonly uint8[] fx =
		[
			Effects.Fx_Speed,
			Effects.Fx_Vibrato,
			Effects.Fx_Break,
			Effects.Fx_Porta_Dn,
			Effects.Fx_Porta_Up,
			0,
			Effects.Fx_Arpeggio,
			Effects.Fx_SetPan,
			0,	// Special
			Effects.Fx_Jump,
			Effects.Fx_Tremolo,
			Effects.Fx_VolSlide,
			0,	// Special
			Effects.Fx_TonePorta,
			Effects.Fx_Offset
		];

		// These are all FX_EXTENDED but in a custom order
		private static readonly uint8[] fx_Misc2 =
		[
			Effects.Ex_F_Porta_Up,
			Effects.Ex_F_Porta_Dn,
			Effects.Ex_F_VSlide_Up,
			Effects.Ex_F_VSlide_Dn,
			Effects.Ex_Vibrato_Wf,
			Effects.Ex_Tremolo_Wf,
			Effects.Ex_Retrig,
			Effects.Ex_Cut,
			Effects.Ex_Delay,
			0,
			0,
			Effects.Ex_Pattern_Loop,
			Effects.Ex_Patt_Delay,
			0,
			0,
			0
		];
		#endregion

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_No = new Format_Loader
		{
			Id = Guid.Parse("4EB23902-4597-443B-B621-9D660BBDA074"),
			Name = "Liquid Tracker NO",
			Description = "DOS tracker created by Nir Oren. This loader can load the old format.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private No_Load(LibXmp libXmp)
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
			return new No_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			byte[] buf = new byte[33];

			f.Hio_Seek(start, SeekOrigin.Begin);

			if (f.Hio_Read32B() != 0x4e4f0000)		// NO 0x00 0x00
				return -1;

			if (f.Hio_Read(buf, 1, 33) < 33)
				return -1;

			c_int nSize = buf[0];
			if (nSize > 29)
				return -1;

			// Test title
			for (c_int i = 0; i < nSize; i++)
			{
				if (buf[i + 1] == '\0')
					return -1;
			}

			// Test number of patterns
			c_int pat = buf[30];
			if (pat == 0)
				return -1;

			// Test number of channels
			c_int chn = buf[32];
			if ((chn <= 0) || (chn > 16))
				return -1;

			f.Hio_Seek(start + 5, SeekOrigin.Begin);

			lib.common.LibXmp_Read_Title(f, out t, nSize, encoder);

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
			CPointer<uint8> buf = new  CPointer<uint8>(46);
			c_int i;

			f.Hio_Read32B();		// NO 0x00 0x00

			lib.common.LibXmp_Set_Type(m, "Liquid Tracker");

			c_int nSize = f.Hio_Read8();
			if (f.Hio_Read(buf, 1, 29) < 29)
				return -1;

			buf[nSize] = 0x00;
			mod.Name = encoder.GetString(buf.Buffer, buf.Offset, buf.Length).TrimEnd();

			mod.Pat = f.Hio_Read8();
			f.Hio_Read8();
			mod.Chn = f.Hio_Read8();
			mod.Trk = mod.Pat * mod.Chn;
			f.Hio_Read8();
			f.Hio_Read16L();
			f.Hio_Read16L();
			f.Hio_Read8();
			mod.Ins = mod.Smp = 63;

			for (i = 0; i < 256; i++)
			{
				uint8 x = f.Hio_Read8();
				if (x == 0xff)
					break;

				mod.Xxo[i] = x;
			}

			f.Hio_Seek(255 - i, SeekOrigin.Current);
			mod.Len = i;

			m.C4Rate = Constants.C4_Ntsc_Rate;

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			// Read instrument names
			for (i = 0; i < mod.Ins; i++)
			{
				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					return -1;

				if (f.Hio_Read(buf, 1, 46) < 46)
					return -1;

				nSize = Math.Min((c_int)buf[0], 30);
				lib.common.LibXmp_Instrument_Name(mod, i, buf + 1, nSize, encoder);

				mod.Xxi[i].Sub[0].Vol = buf[31];

				c_int c2Spd = DataIo.ReadMem16L(buf + 32);

				mod.Xxs[i].Len = (c_int)DataIo.ReadMem32L(buf + 34);
				mod.Xxs[i].Lps = (c_int)DataIo.ReadMem32L(buf + 38);
				mod.Xxs[i].Lpe = (c_int)DataIo.ReadMem32L(buf + 42);

				if (mod.Xxs[i].Len > 0)
					mod.Xxi[i].Nsm = 1;

				mod.Xxs[i].Flg = mod.Xxs[i].Lpe > 0 ? Xmp_Sample_Flag.Loop : Xmp_Sample_Flag.None;
				mod.Xxi[i].Sub[0].Fin = 0;
				mod.Xxi[i].Sub[0].Pan = 0x80;
				mod.Xxi[i].Sub[0].Sid = i;

				lib.period.LibXmp_C2Spd_To_Note(c2Spd, out mod.Xxi[i].Sub[0].Xpo, out mod.Xxi[i].Sub[0].Fin);
			}

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				return -1;

			// Read and convert patterns
			for (i = 0; i < mod.Pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, 64) < 0)
					return -1;

				for (c_int j = 0; j < mod.Xxp[i].Rows; j++)
				{
					for (c_int k = 0; k < mod.Chn; k++)
					{
						Xmp_Event @event = Ports.LibXmp.Common.Event(m, i, k, j);

						uint32 x = f.Hio_Read32L();
						uint32 note = x & 0x0000003f;
						uint32 ins = (x & 0x00001fc0) >> 6;
						uint32 vol = (x & 0x000fe000) >> 13;
						uint32 fxt = (x & 0x00f00000) >> 20;
						uint32 fxp = (x & 0xff000000) >> 24;

						if (note != 0x3f)
							@event.Note = (byte)(37 + note);

						if (ins != 0x7f)
							@event.Ins = (byte)(1 + ins);

						if (vol != 0x7f)
							@event.Vol = (byte)(1 + vol);

						if (fxt != 0x0f)
							No_Translate_Effect(@event, (c_int)fxt, (c_int)fxp);
					}
				}
			}

			// Read samples
			for (i = 0; i < mod.Ins; i++)
			{
				if (mod.Xxs[i].Len == 0)
					continue;

				if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.Uns, mod.Xxs[i], null, i) < 0)
					return -1;
			}

			m.Quirk |= Quirk_Flag.FineFx | Quirk_Flag.RtOnce;
			m.Flow_Mode = FlowMode_Flag.Mode_Liquid;
			m.Read_Event_Type = Read_Event.St3;

			for (i = 0; i < mod.Chn; i++)
				mod.Xxc[i].Pan = Common.DefPan(m, (i & 1) != 0 ? 0xff : 0x00);

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void No_Translate_Effect(Xmp_Event @event, c_int fxt, c_int fxp)
		{
			c_int value;

			switch (fxt)
			{
				// Axx Set Speed/BPM
				// Bxy Vibrato
				// Cxx Cut (break)
				// Dxx Porta Down
				// Exx Porta Up
				// Gxx Arpeggio
				// Jxx Jump Position
				// Kxy Tremolo
				// Lxy Volslide (fine 0.80b+ only)
				// Nxx Note Portamento
				// Oxx Sample Offset
				case 0x0:
				case 0x1:
				case 0x2:
				case 0x3:
				case 0x4:
				case 0x6:
				case 0x9:
				case 0xa:
				case 0xb:
				case 0xd:
				case 0xe:
				{
					@event.FxT = fx[fxt];
					@event.FxP = (byte)fxp;
					break;
				}

				// Hxx Pan Control
				case 0x7:
				{
					// Value is decimal, effective values >64 are ignored
					value = Ports.LibXmp.Common.Msn(fxp) * 10 + Ports.LibXmp.Common.Lsn(fxp);

					if (value == 70)
					{
						// TODO: Reset panning H70 (H6A also works)
						// This resets ALL channels to default pan
					}
					else if (value <= 64)
					{
						@event.FxT = Effects.Fx_SetPan;
						@event.FxP = (byte)(value * 0xff / 64);
					}
					break;
				}

				// Ixy Misc 1
				case 0x8:
				{
					switch (Ports.LibXmp.Common.Msn(fxp))
					{
						// I0y Vibrato + volslide up
						case 0x0:
						{
							@event.FxT = Effects.Fx_Vibra_VSlide;
							@event.FxP = (byte)(Ports.LibXmp.Common.Lsn(fxp) << 4);
							break;
						}

						// I1y Vibrato + volslide down
						case 0x1:
						{
							@event.FxT = Effects.Fx_Vibra_VSlide;
							@event.FxP = (byte)Ports.LibXmp.Common.Lsn(fxp);
							break;
						}

						// I2y Noteporta + volslide up
						case 0x2:
						{
							@event.FxT = Effects.Fx_Tone_VSlide;
							@event.FxP = (byte)(Ports.LibXmp.Common.Lsn(fxp) << 4);
							break;
						}

						// I3y Noteporta + volslide down
						case 0x3:
						{
							@event.FxT = Effects.Fx_Tone_VSlide;
							@event.FxP = (byte)Ports.LibXmp.Common.Lsn(fxp);
							break;
						}

						// TODO: If these were ever implemented they were after 0.64b
						// and before 0.80b only, i.e. versions not available to test

						// I4y Tremolo + volslide up
						case 0x4:
						{
							@event.FxT = Effects.Fx_Tremolo;
							@event.FxP = 0;
							@event.F2T = Effects.Fx_VolSlide;
							@event.F2P = (byte)(Ports.LibXmp.Common.Lsn(fxp) << 4);
							break;
						}

						// I5y Tremolo + volslide down
						case 0x5:
						{
							@event.FxT = Effects.Fx_Tremolo;
							@event.FxP = 0;
							@event.F2T = Effects.Fx_VolSlide;
							@event.F2P = (byte)Ports.LibXmp.Common.Lsn(fxp);
							break;
						}
					}
					break;
				}

				// Mxy Misc 2
				case 0xc:
				{
					value = Ports.LibXmp.Common.Msn(fxp);
					fxp = Ports.LibXmp.Common.Lsn(fxp);

					switch (value)
					{
						// M4x Vibrato Waveform
						// M5x Tremolo Waveform
						case 0x4:
						case 0x5:
						{
							if ((fxp & 3) == 3)
								fxp--;

							// Fall-through
							goto case 0x0;
						}

						// M0y Fine Porta Up
						// M1y Fine Porta Down
						// M2y Fine Volslide Up
						// M3y Fine Volslide Down
						// M6y Note Retrigger
						// M7y Note Cut
						// M8y Note Delay
						// MBy Pattern Loop
						// MCy Pattern Delay
						case 0x0:
						case 0x1:
						case 0x2:
						case 0x3:
						case 0x6:
						case 0x7:
						case 0x8:
						case 0xb:
						case 0xc:
						{
							@event.FxT = Effects.Fx_Extended;
							@event.FxP = (byte)((fx_Misc2[value] << 4) | fxp);
							break;
						}
					}
					break;
				}
			}
		}
		#endregion
	}
 }
