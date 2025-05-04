/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Text;
using Polycode.NostalgicPlayer.CKit;
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
	internal class Fnk_Load : IFormatLoader
	{
		#region Internal structures

		#region Fnk_Instrument
		private class Fnk_Instrument
		{
			public readonly uint8[] Name = new uint8[19];		// ASCIIZ instrument name
			public uint32 Loop_Start;							// Instrument loop start
			public uint32 Length;								// Instrument length
			public uint8 Volume;								// Volume (0-255)
			public uint8 Pan;									// Pan (0-255)
			public uint8 Shifter;								// Portamento and offset shift
			public uint8 Waveform;								// Vibrato and tremolo waveforms
			public uint8 Retrig;								// Retrig and arpeggio speed
		}
		#endregion

		#region Fnk_Header
		private class Fnk_Header
		{
			public readonly uint8[] Marker = new uint8[4];		// 'Funk'
			public readonly uint8[] Info = new uint8[4];
			public uint32 FileSize;								// File size
			public readonly uint8[] Fmt = new uint8[4];			// F2xx, Fkxx or Fvxx
			public uint8 Loop;									// Loop order number
			public readonly uint8[] Order = new uint8[256];		// Order list
			public readonly uint8[] PBrk = new uint8[128];		// Break list for patterns
			public readonly Fnk_Instrument[] Fih = ArrayHelper.InitializeArray<Fnk_Instrument>(64);	// Instruments
		}
		#endregion

		#endregion

		private static readonly uint32 Magic_Funk = Common.Magic4('F', 'u', 'n', 'k');

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Funk = new Format_Loader
		{
			Id = Guid.Parse("946F7C0E-164E-4BBE-9A66-7306082130B5"),
			Name = "Funktracker",
			Description = "This is a text based UNIX tracker created by Jason Nunn.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Fnk_Load(LibXmp libXmp)
		{
			lib = libXmp;
			encoder = EncoderCollection.Iso8859_1;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new instance of the loader
		/// </summary>
		/********************************************************************/
		private static IFormatLoader Create(LibXmp libXmp, Xmp_Context ctx)
		{
			return new Fnk_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			if (f.Hio_Read32B() != Magic_Funk)
				return -1;

			f.Hio_Read8();
			uint8 a = f.Hio_Read8();
			uint8 b = f.Hio_Read8();
			f.Hio_Read8();

			// Creation year (-1980)
			if ((a >> 1) < 10)
				return -1;

			// CPU and card
			if ((Ports.LibXmp.Common.Msn(b) > 7) || (Ports.LibXmp.Common.Lsn(b) > 9))
				return -1;

			c_int size = (c_int)f.Hio_Read32L();
			if (size < 1024)
				return -1;

			if (f.Hio_Size() != size)
				return -1;

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
			c_int i;
			Fnk_Header ffh = new Fnk_Header();
			uint8[] ev = new uint8[3];

			f.Hio_Read(ffh.Marker, 4, 1);
			f.Hio_Read(ffh.Info, 4, 1);
			ffh.FileSize = f.Hio_Read32L();
			f.Hio_Read(ffh.Fmt, 4, 1);
			ffh.Loop = f.Hio_Read8();
			f.Hio_Read(ffh.Order, 256, 1);
			f.Hio_Read(ffh.PBrk, 128, 1);

			for (i = 0; i < 128; i++)
			{
				if (ffh.PBrk[i] >= 64)
					return -1;
			}

			for (i = 0; i < 64; i++)
			{
				f.Hio_Read(ffh.Fih[i].Name, 19, 1);
				ffh.Fih[i].Loop_Start = f.Hio_Read32L();
				ffh.Fih[i].Length = f.Hio_Read32L();
				ffh.Fih[i].Volume = f.Hio_Read8();
				ffh.Fih[i].Pan = f.Hio_Read8();
				ffh.Fih[i].Shifter = f.Hio_Read8();
				ffh.Fih[i].Waveform = f.Hio_Read8();
				ffh.Fih[i].Retrig = f.Hio_Read8();

				// Sanity check
				if (ffh.Fih[i].Length >= ffh.FileSize)
					return -1;
			}

			mod.Smp = mod.Ins = 64;

			for (i = 0; (i < 256) && (ffh.Order[i] != 0xff); i++)
			{
				if (ffh.Order[i] > mod.Pat)
					mod.Pat = ffh.Order[i];
			}

			mod.Pat++;

			// Sanity check
			if (mod.Pat > 128)
				return -1;

			mod.Len = i;
			CMemory.MemCpy<uint8>(mod.Xxo, ffh.Order, mod.Len);

			mod.Spd = 4;
			mod.Bpm = 125;
			mod.Chn = 0;

			// If an R1 fmt (funktype = Fk** or Fv**), then ignore byte 3. It's
			// unreliable. It used to store the (GUS) sample memory requirement
			if ((ffh.Fmt[0] == 'F') && (ffh.Fmt[1] == '2'))
			{
				if ((((int8)ffh.Info[3] >> 1) & 0x40) != 0)
					mod.Bpm -= (ffh.Info[3] >> 1) & 0x3f;
				else
					mod.Bpm += (ffh.Info[3] >> 1) & 0x3f;

				lib.common.LibXmp_Set_Type(m, "FunktrackerGOLD");
			}
			else if ((ffh.Fmt[0] == 'F') && ((ffh.Fmt[1] == 'v') || (ffh.Fmt[1] == 'k')))
				lib.common.LibXmp_Set_Type(m, "Funktracker");
			else
			{
				mod.Chn = 8;
				lib.common.LibXmp_Set_Type(m, "Funktracker DOS32");
			}

			if (mod.Chn == 0)
			{
				mod.Chn = (ffh.Fmt[2] < '0') || (ffh.Fmt[2] > '9') || (ffh.Fmt[3] < '0') || (ffh.Fmt[3] > '9') ? 8 : (ffh.Fmt[2] - '0') * 10 + (ffh.Fmt[3] - '0');

				// Sanity check
				if ((mod.Chn <= 0) || (mod.Chn > Constants.Xmp_Max_Channels))
					return -1;
			}

			mod.Bpm = 4 * mod.Bpm / 5;
			mod.Trk = mod.Chn * mod.Pat;

			// FNK allows mode per instrument but we don't, so use linear for all
			m.Period_Type = Containers.Common.Period.Linear;

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			// Convert instruments
			for (i = 0; i < mod.Ins; i++)
			{
				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					return -1;

				mod.Xxs[i].Len = (c_int)ffh.Fih[i].Length;
				mod.Xxs[i].Lps = (c_int)ffh.Fih[i].Loop_Start;

				if (mod.Xxs[i].Lps == -1)
					mod.Xxs[i].Lps = 0;

				mod.Xxs[i].Lpe = (c_int)ffh.Fih[i].Length;
				mod.Xxs[i].Flg = (c_int)ffh.Fih[i].Loop_Start != -1 ? Xmp_Sample_Flag.Loop : Xmp_Sample_Flag.None;

				mod.Xxi[i].Sub[0].Vol = ffh.Fih[i].Volume;
				mod.Xxi[i].Sub[0].Pan = ffh.Fih[i].Pan;
				mod.Xxi[i].Sub[0].Sid = i;

				if (mod.Xxs[i].Len > 0)
					mod.Xxi[i].Nsm = 1;

				lib.common.LibXmp_Instrument_Name(mod, i, ffh.Fih[i].Name, 19, encoder);
			}

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				return -1;

			// Read and convert patterns
			for (i = 0; i < mod.Pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, 64) < 0)
					return -1;

				Ports.LibXmp.Common.Event(m, i, 0, ffh.PBrk[i]).F2T = Effects.Fx_Break;

				for (c_int j = 0; j < 64; j++)
				{
					for (c_int k = 0; k < mod.Chn; k++)
					{
						Xmp_Event @event = Ports.LibXmp.Common.Event(m, i, k, j);

						if (f.Hio_Read(ev, 1, 3) < 3)
							return -1;

						Fnk_Translate_Event(@event, ev, ffh);
					}
				}
			}

			// Read samples
			for (i = 0; i < mod.Ins; i++)
			{
				if (mod.Xxs[i].Len <= 2)
					continue;

				if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, mod.Xxs[i], null, i) < 0)
					return -1;
			}

			for (i = 0; i < mod.Chn; i++)
				mod.Xxc[i].Pan = 0x80;

			m.VolBase = 0xff;
			m.Quirk = Quirk_Flag.VsAll;

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Fnk_Translate_Event(Xmp_Event @event, uint8[] ev, Fnk_Header ffh)
		{
			switch (ev[0] >> 2)
			{
				case 0x3f:
				case 0x3e:
				case 0x3d:
					break;

				default:
				{
					@event.Note = (byte)(37 + (ev[0] >> 2));
					@event.Ins = (byte)(1 + Ports.LibXmp.Common.Msn(ev[1]) + ((ev[0] & 0x03) << 4));
					@event.Vol = ffh.Fih[@event.Ins - 1].Volume;
					break;
				}
			}

			switch (Ports.LibXmp.Common.Lsn(ev[1]))
			{
				case 0x00:
				{
					@event.FxT = Effects.Fx_Per_Porta_Up;
					@event.FxP = ev[2];
					break;
				}

				case 0x01:
				{
					@event.FxT = Effects.Fx_Per_Porta_Dn;
					@event.FxP = ev[2];
					break;
				}

				case 0x02:
				{
					@event.FxT = Effects.Fx_Per_TPorta;
					@event.FxP = ev[2];
					break;
				}

				case 0x03:
				{
					@event.FxT = Effects.Fx_Per_Vibrato;
					@event.FxP = ev[2];
					break;
				}

				case 0x06:
				{
					@event.FxT = Effects.Fx_Per_VSld_Up;
					@event.FxP = (byte)(ev[2] << 1);
					break;
				}

				case 0x07:
				{
					@event.FxT = Effects.Fx_Per_VSld_Dn;
					@event.FxP = (byte)(ev[2] << 1);
					break;
				}

				case 0x0b:
				{
					@event.FxT = Effects.Fx_Arpeggio;
					@event.FxP = ev[2];
					break;
				}

				case 0x0d:
				{
					@event.FxT = Effects.Fx_VolSet;
					@event.FxP = ev[2];
					break;
				}

				case 0x0e:
				{
					if ((ev[2] == 0x0a) || (ev[2] == 0x0b) || (ev[2] == 0x0c))
					{
						@event.FxT = Effects.Fx_Per_Cancel;
						break;
					}

					switch (Ports.LibXmp.Common.Msn(ev[2]))
					{
						case 0x1:
						{
							@event.FxT = Effects.Fx_Extended;
							@event.FxP = (byte)((Effects.Ex_Cut << 4) | Ports.LibXmp.Common.Lsn(ev[2]));
							break;
						}

						case 0x2:
						{
							@event.FxT = Effects.Fx_Extended;
							@event.FxP = (byte)((Effects.Ex_Delay << 4) | Ports.LibXmp.Common.Lsn(ev[2]));
							break;
						}

						case 0xd:
						{
							@event.FxT = Effects.Fx_Extended;
							@event.FxP = (byte)((Effects.Ex_Retrig << 4) | Ports.LibXmp.Common.Lsn(ev[2]));
							break;
						}

						case 0xe:
						{
							@event.FxT = Effects.Fx_SetPan;
							@event.FxP = (byte)(8 + (Ports.LibXmp.Common.Lsn(ev[2]) << 4));
							break;
						}

						case 0xf:
						{
							@event.FxT = Effects.Fx_Speed;
							@event.FxP = Ports.LibXmp.Common.Lsn(ev[2]);
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
