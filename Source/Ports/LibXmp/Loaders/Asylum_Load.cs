/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Kit;
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
	internal class Asylum_Load : IFormatLoader
	{
		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Asylum = new Format_Loader
		{
			Id = Guid.Parse("29FCE360-BD89-4A32-A068-BFF0D84F1C57"),
			Name = "Asylum Music Format",
			Description = "This loader recognize the “ASYLUM Music Format”, which was used in Crusader series of games by Origin. This format uses the .amf extension, but is very similar to a 8 Channel Mod file.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Asylum_Load(LibXmp libXmp)
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
			return new Asylum_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			byte[] buf = new byte[32];

			if (f.Hio_Read(buf, 1, 32) < 32)
				return -1;

			if (Encoding.ASCII.GetString(buf, 0, 32) != "ASYLUM Music Format V1.0\0\0\0\0\0\0\0\0")
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
			uint8[] buf = new uint8[2048];

			f.Hio_Seek(32, SeekOrigin.Current);		// Skip magic
			mod.Spd = f.Hio_Read8();							// Initial speed
			mod.Bpm = f.Hio_Read8();							// Initial BPM
			mod.Ins = f.Hio_Read8();							// Number of instruments
			mod.Pat = f.Hio_Read8();							// Number of patterns
			mod.Len = f.Hio_Read8();							// Module length
			mod.Rst = f.Hio_Read8();							// Restart byte

			// Sanity check - this format only stores 64 sample structures
			if (mod.Ins > 64)
				return -1;

			f.Hio_Read(mod.Xxo, 1, (size_t)mod.Len);	// Read orders
			f.Hio_Seek(start + 294, SeekOrigin.Begin);

			mod.Chn = 8;
			mod.Smp = mod.Ins;
			mod.Trk = mod.Pat * mod.Chn;

			lib.common.LibXmp_Set_Type(m, "Asylum Music Format");

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			// Read and convert instruments and samples
			for (c_int i = 0; i < mod.Ins; i++)
			{
				uint8[] insBuf = new uint8[37];

				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					return -1;

				if (f.Hio_Read(insBuf, 1, 37) != 37)
					return -1;

				lib.common.LibXmp_Instrument_Name(mod, i, insBuf, 22, encoder);
				mod.Xxi[i].Sub[0].Fin = (int8)(insBuf[22] << 4);
				mod.Xxi[i].Sub[0].Vol = insBuf[23];
				mod.Xxi[i].Sub[0].Xpo = (int8)insBuf[24];
				mod.Xxi[i].Sub[0].Pan = 0x80;
				mod.Xxi[i].Sub[0].Sid = i;

				mod.Xxs[i].Len = (c_int)DataIo.ReadMem32L(insBuf, 25);
				mod.Xxs[i].Lps = (c_int)DataIo.ReadMem32L(insBuf, 29);
				mod.Xxs[i].Lpe = mod.Xxs[i].Lps + (c_int)DataIo.ReadMem32L(insBuf, 33);

				// Sanity check - ASYLUM modules are converted from MODs
				if ((uint32)mod.Xxs[i].Len >= 0x20000)
					return -1;

				mod.Xxs[i].Flg = mod.Xxs[i].Lpe > 2 ? Xmp_Sample_Flag.Loop : Xmp_Sample_Flag.None;
			}

			f.Hio_Seek(37 * (64 - mod.Ins), SeekOrigin.Current);

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				return -1;

			// Read and convert patterns
			for (c_int i = 0; i < mod.Pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, 64) < 0)
					return -1;

				if (f.Hio_Read(buf, 1, 2048) < 2048)
					return -1;

				c_int pos = 0;

				for (c_int j = 0; j < 64 * 8; j++)
				{
					Xmp_Event @event = Ports.LibXmp.Common.Event(m, i, j % 8, j / 8);

					uint8 note = buf[pos++];
					if (note != 0)
						@event.Note = (byte)(note + 13);

					@event.Ins = buf[pos++];
					@event.FxT = buf[pos++];
					@event.FxP = buf[pos++];

					// TODO: m07.amf and m12.amf from Crusader: No Remorse
					// use 0x1b for what looks *plausibly* like retrigger.
					// No other ASYLUM modules use effects over 16
					if ((@event.FxT >= 0x10) && (@event.FxT != Effects.Fx_Multi_Retrig))
						@event.FxT = @event.FxP = 0;
				}
			}

			// Read samples
			for (c_int i = 0; i < mod.Ins; i++)
			{
				if (mod.Xxs[i].Len > 1)
				{
					if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, mod.Xxs[i], null, i) < 0)
						return -1;

					mod.Xxi[i].Nsm = 1;
				}
			}

			return 0;
		}
	}
}
