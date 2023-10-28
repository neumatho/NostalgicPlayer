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
	internal class _669_Load : IFormatLoader
	{
		#region Internal structures

		#region C669_File_Header
		private class C669_File_Header
		{
			/// <summary>
			/// 'if' = standard (Composer), 'JN'=extended (Unis)
			/// </summary>
			public uint8[] Marker = new uint8[2];

			/// <summary>
			/// Song message
			/// </summary>
			public uint8[] Message = new uint8[108];

			/// <summary>
			/// Number of samples (0-64)
			/// </summary>
			public uint8 Nos;

			/// <summary>
			/// Number of patterns (0-128)
			/// </summary>
			public uint8 Nop;

			/// <summary>
			/// Loop order number
			/// </summary>
			public uint8 Loop;

			/// <summary>
			/// Order list
			/// </summary>
			public uint8[] Order = new uint8[128];

			/// <summary>
			/// Tempo list for patterns
			/// </summary>
			public uint8[] Speed = new uint8[128];

			/// <summary>
			/// Break list for patterns
			/// </summary>
			public uint8[] PBrk = new uint8[128];
		}
		#endregion

		#region C669_Instrument_Header
		private class C669_Instrument_Header
		{
			/// <summary>
			/// ASCIIZ instrument name
			/// </summary>
			public uint8[] Name = new uint8[13];

			/// <summary>
			/// Instrument length
			/// </summary>
			public uint32 Length;

			/// <summary>
			/// Instrument loop start
			/// </summary>
			public uint32 Loop_Start;

			/// <summary>
			/// Instrument loop end
			/// </summary>
			public uint32 LoopEnd;
		}
		#endregion

		#endregion

		private enum Format
		{
			Composer,
			Unis
		}

		private readonly Format format;
		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Composer669 = new Format_Loader
		{
			Id = Guid.Parse("DA884858-983E-45DB-B4FF-470DBB927239"),
			Name = "Composer 669",
			Description = "This loader recognizes “Composer 669” modules. The 669 format were among the first PC module formats. They do not have a wide range of effects and support 8 channels.\n\n“Composer 669” was written by Tran of Renaissance, a.k.a. Tomasz Pytel and released in 1992.",
			Create = Create_Composer
		};

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Unis669 = new Format_Loader
		{
			Id = Guid.Parse("216FCDE9-3E12-474A-A27D-28914C8C894D"),
			Name = "Unis 669",
			Description = "This loader recognizes “Unis 669” modules. This format is the successor of the “Composer 669” and introduces some new effects like the super fast tempo and stereo balance. Support 8 channels.\n\n“Unis 669 Composer” was written by Jason Nunn and released in 1994.",
			Create = Create_Unis
		};

		// Effects bug fixed by Miod Vallat <miodrag@multimania.com>
		private static readonly uint8[] fx =
		{
			Effects.Fx_669_Porta_Up,
			Effects.Fx_669_Porta_Dn,
			Effects.Fx_669_TPorta,
			Effects.Fx_669_FineTune,
			Effects.Fx_669_Vibrato,
			Effects.Fx_Speed_Cp
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private _669_Load(LibXmp libXmp, Format format)
		{
			this.format = format;
			lib = libXmp;
			encoder = EncoderCollection.Dos;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new instance of the loader
		/// </summary>
		/********************************************************************/
		private static IFormatLoader Create_Composer(LibXmp libXmp, Xmp_Context ctx)
		{
			return new _669_Load(libXmp, Format.Composer);
		}



		/********************************************************************/
		/// <summary>
		/// Create a new instance of the loader
		/// </summary>
		/********************************************************************/
		private static IFormatLoader Create_Unis(LibXmp libXmp, Xmp_Context ctx)
		{
			return new _669_Load(libXmp, Format.Unis);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			uint16 id = f.Hio_Read16B();

			if ((id != 0x6966) && (id != 0x4a4e))
				return -1;

			if ((id == 0x6966) && (format != Format.Composer))
				return -1;

			if ((id == 0x4a4e) && (format != Format.Unis))
				return -1;

			f.Hio_Seek(110, SeekOrigin.Begin);
			if (f.Hio_Read8() > 64)
				return -1;

			if (f.Hio_Read8() > 128)
				return -1;

			f.Hio_Seek(240, SeekOrigin.Begin);
			if (f.Hio_Read8() != 0xff)
				return -1;

			f.Hio_Seek(start + 2, SeekOrigin.Begin);
			lib.common.LibXmp_Read_Title(f, out t, 36, encoder);

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
			C669_File_Header sfh = new C669_File_Header();
			C669_Instrument_Header sih = new C669_Instrument_Header();
			uint8[] ev = new uint8[3];

			f.Hio_Read(sfh.Marker, 2, 1);		// 'if' = standard (Composer), 'JN'=extended (Unis)
			f.Hio_Read(sfh.Message, 108, 1);		// Song message
			sfh.Nos = f.Hio_Read8();						// Number of samples (0-64)
			sfh.Nop = f.Hio_Read8();						// Number of patterns (0-128)

			// Sanity check
			if ((sfh.Nos > 64) || (sfh.Nop > 128))
				return -1;

			sfh.Loop = f.Hio_Read8();						// Loop order number

			if (f.Hio_Read(sfh.Order, 1, 128) != 128)	// Order list
				return -1;

			if (f.Hio_Read(sfh.Speed, 1, 128) != 128)	// Tempo list for patterns
				return -1;

			if (f.Hio_Read(sfh.PBrk, 1, 128) != 128)		// Break list for patterns
				return -1;

			mod.Chn = 8;
			mod.Ins = sfh.Nos;
			mod.Pat = sfh.Nop;
			mod.Trk = mod.Chn * mod.Pat;

			for (i = 0; i < 128; i++)
			{
				if (sfh.Order[i] > sfh.Nop)
					break;
			}

			mod.Len = i;
			Array.Copy(sfh.Order, mod.Xxo, mod.Len);

			mod.Spd = 6;
			mod.Bpm = 78;
			mod.Smp = mod.Ins;

			m.Period_Type = Containers.Common.Period.CSpd;
			m.C4Rate = Constants.C4_Ntsc_Rate;

			lib.common.LibXmp_Copy_Adjust(out mod.Name, sfh.Message, 36, encoder);
			lib.common.LibXmp_Set_Type(m, format == Format.Unis ? "UNIS 669" : "Composer 669");

			// Split the comment into 3 lines
			m.Comment = encoder.GetString(sfh.Message, 0, 36).TrimEnd() + "\n" +
			            encoder.GetString(sfh.Message, 36, 36).TrimEnd() + "\n" +
			            encoder.GetString(sfh.Message, 72, 36).TrimEnd();

			// Read and convert instruments and samples
			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			for (i = 0; i < mod.Ins; i++)
			{
				Xmp_Instrument xxi = mod.Xxi[i];
				Xmp_Sample xxs = mod.Xxs[i];

				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					return -1;

				Xmp_SubInstrument sub = xxi.Sub[0];

				f.Hio_Read(sih.Name, 13, 1);		// ASCIIZ instrument name
				sih.Length = f.Hio_Read32L();				// Instrument size
				sih.Loop_Start = f.Hio_Read32L();			// Instrument loop start
				sih.LoopEnd = f.Hio_Read32L();				// Instrument loop end

				// Sanity check
				if (sih.Length > Constants.Max_Sample_Size)
					return -1;

				xxs.Len = (c_int)sih.Length;
				xxs.Lps = (c_int)sih.Loop_Start;
				xxs.Lpe = sih.LoopEnd >= 0xfffff ? 0 : (c_int)sih.LoopEnd;
				xxs.Flg = xxs.Lpe != 0 ? Xmp_Sample_Flag.Loop : 0;	// 1 == Forward loop

				sub.Vol = 0x40;
				sub.Pan = 0x80;
				sub.Sid = i;

				if (xxs.Len > 0)
					xxi.Nsm = 1;

				lib.common.LibXmp_Instrument_Name(mod, i, sih.Name, 13, encoder);
			}

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				return -1;

			// Read and convert patterns
			for (i = 0; i < mod.Pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, 64) < 0)
					return -1;

				Xmp_Event @event = Ports.LibXmp.Common.Event(m, i, 0, 0);
				@event.F2T = Effects.Fx_Speed_Cp;
				@event.F2P = sfh.Speed[i];

				c_int pBrk = sfh.PBrk[i];
				if (pBrk >= 64)
					return -1;

				@event = Ports.LibXmp.Common.Event(m, i, 1, pBrk);
				@event.F2T = Effects.Fx_Break;
				@event.F2P = 0;

				for (c_int j = 0; j < 64 * 8; j++)
				{
					@event = Ports.LibXmp.Common.Event(m, i, j % 8, j / 8);

					if (f.Hio_Read(ev, 1, 3) < 3)
						return -1;

					if ((ev[0] & 0xfe) != 0xfe)
					{
						@event.Note = (byte)(1 + 36 + (ev[0] >> 2));
						@event.Ins = (byte)(1 + Ports.LibXmp.Common.Msn(ev[1]) + ((ev[0] & 0x03) << 4));
					}

					if (ev[0] != 0xff)
						@event.Vol = (byte)((Ports.LibXmp.Common.Lsn(ev[1]) << 2) + 1);

					if (ev[2] != 0xff)
					{
						if (Ports.LibXmp.Common.Msn(ev[2]) >= fx.Length)
							continue;

						@event.FxT = fx[Ports.LibXmp.Common.Msn(ev[2])];
						@event.FxP = Ports.LibXmp.Common.Lsn(ev[2]);

						if (@event.FxT == Effects.Fx_Speed_Cp)
							@event.F2T = Effects.Fx_Per_Cancel;
					}
				}
			}

			// Read samples
			for (i = 0; i < mod.Ins; i++)
			{
				if (mod.Xxs[i].Len <= 2)
					continue;

				if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.Uns, mod.Xxs[i], null, i) < 0)
					return -1;
			}

			for (i = 0; i < mod.Chn; i++)
				mod.Xxc[i].Pan = Common.DefPan(m, (i % 2) * 0xff);

			m.Quirk |= Quirk_Flag.PbAll | Quirk_Flag.PerPat;

			return 0;
		}
	}
}
