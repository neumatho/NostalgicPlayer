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
	/// Galaxy Music System 5.0 module file loader
	///
	/// Based on the format description by Dr.Eggman
	/// (http://www.jazz2online.com/J2Ov2/articles/view.php?articleID=288)
	/// and Jazz Jackrabbit modules by Alexander Brandon from Lori Central
	/// (http://www.loricentral.com/jj2music.html)
	/// </summary>
	internal class Gal5_Load : IFormatLoader
	{
		#region Internal structures

		#region Local_Data
		private class Local_Data
		{
			public uint8[] Chn_Pan { get; } = new uint8[64];
		}
		#endregion

		#endregion

		private readonly LibXmp lib;
		private readonly Encoding encoder;
		private readonly bool testMode;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Gal5 = new Format_Loader
		{
			Id = Guid.Parse("525AFC28-8D6D-42D2-8F00-295164664868"),
			Name = "Galaxy Music System 5.0",
			Description = "This format is a conversion and packer format used in the game Jazz Jackrabbit 2.\n\nEarlier versions of the game did not have a specific music format, but used Scream Tracker 3 or Impulse Tracker formats directly. Later on, this format was invented and used.\n\nThis loader can load the newest version of the format.",
			Create = Create
		};

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_TestOnly = new Format_Loader
		{
			Id = Guid.Parse("6040692D-5BAE-4C4A-BF79-276C9A1EEBAC"),
			Name = "Test only",
			OnlyAvailableInTest = true,
			Create = Create_TestOnly
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Gal5_Load(LibXmp libXmp, bool test)
		{
			lib = libXmp;
			encoder = EncoderCollection.Dos;
			testMode = test;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new instance of the loader
		/// </summary>
		/********************************************************************/
		private static IFormatLoader Create(LibXmp libXmp, Xmp_Context ctx)
		{
			return new Gal5_Load(libXmp, false);
		}



		/********************************************************************/
		/// <summary>
		/// Create a new instance of the loader
		/// </summary>
		/********************************************************************/
		private static IFormatLoader Create_TestOnly(LibXmp libXmp, Xmp_Context ctx)
		{
			return new Gal5_Load(libXmp, true);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			Muse muse = new Muse();
			bool closeFile = true;

			if (muse.IsMuseFile(f) < 0)
			{
				if (!testMode)
					return -1;

				closeFile = false;
				f.Hio_Seek(0, SeekOrigin.Begin);
			}
			else
			{
				f = muse.GetHioWithDecompressedData(f);
				if (f == null)
					return -1;
			}

			try
			{
				if (f.Hio_Read32B() != Common.Magic4('R', 'I', 'F', 'F'))
					return -1;

				f.Hio_Read32B();

				if (f.Hio_Read32B() != Common.Magic4('A', 'M', ' ', ' '))
					return -1;

				if (f.Hio_Read32B() != Common.Magic4('I', 'N', 'I', 'T'))
					return -1;

				f.Hio_Read32B();		// Skip size

				lib.common.LibXmp_Read_Title(f, out t, 64, encoder);
			}
			finally
			{
				if (closeFile)
					f.Hio_Close();
			}

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

			if (!testMode)
			{
				Muse muse = new Muse();

				f = muse.GetHioWithDecompressedData(f);
				if (f == null)
					return -1;
			}

			try
			{
				f.Hio_Read32B();		// Skip RIFF
				f.Hio_Read32B();		// Skip size
				f.Hio_Read32B();		// Skip AM

				c_int offset = f.Hio_Tell();

				mod.Smp = mod.Ins = 0;

				Iff handle = Iff.LibXmp_Iff_New();
				if (handle == null)
					return -1;

				m.C4Rate = Constants.C4_Ntsc_Rate;

				// IFF chunk IDs
				c_int ret = handle.LibXmp_Iff_Register("INIT".ToPointer(), Get_Init);
				ret |= handle.LibXmp_Iff_Register("ORDR".ToPointer(), Get_Ordr);
				ret |= handle.LibXmp_Iff_Register("PATT".ToPointer(), Get_Patt_Cnt);
				ret |= handle.LibXmp_Iff_Register("INST".ToPointer(), Get_Inst_Cnt);

				if (ret != 0)
					return -1;

				handle.LibXmp_Iff_Set_Quirk(Iff_Quirk_Flag.Little_Endian);
				handle.LibXmp_Iff_Set_Quirk(Iff_Quirk_Flag.Skip_Embedded);
				handle.LibXmp_Iff_Set_Quirk(Iff_Quirk_Flag.Chunk_Align2);

				// Load IFF chunks
				if (handle.LibXmp_Iff_Load(m, f, data) < 0)
				{
					handle.LibXmp_Iff_Release();
					return -1;
				}

				handle.LibXmp_Iff_Release();

				mod.Trk = mod.Pat * mod.Chn;
				mod.Smp = mod.Ins;

				if (lib.common.LibXmp_Init_Instrument(m) < 0)
					return -1;

				if (lib.common.LibXmp_Init_Pattern(mod) < 0)
					return -1;

				f.Hio_Seek(start + offset, SeekOrigin.Begin);

				handle = Iff.LibXmp_Iff_New();
				if (handle == null)
					return -1;

				// IFF chunk IDs
				ret = handle.LibXmp_Iff_Register("PATT".ToPointer(), Get_Patt);
				ret |= handle.LibXmp_Iff_Register("INST".ToPointer(), Get_Inst);

				if (ret != 0)
					return -1;

				handle.LibXmp_Iff_Set_Quirk(Iff_Quirk_Flag.Little_Endian);
				handle.LibXmp_Iff_Set_Quirk(Iff_Quirk_Flag.Skip_Embedded);
				handle.LibXmp_Iff_Set_Quirk(Iff_Quirk_Flag.Chunk_Align2);

				// Load IFF chunks
				if (handle.LibXmp_Iff_Load(m, f, data) < 0)
				{
					handle.LibXmp_Iff_Release();
					return -1;
				}

				handle.LibXmp_Iff_Release();

				// Alloc missing patterns
				for (c_int i = 0; i < mod.Pat; i++)
				{
					if (mod.Xxp[i] == null)
					{
						if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, 64) < 0)
							return -1;
					}
				}

				for (c_int i = 0; i < mod.Chn; i++)
					mod.Xxc[i].Pan = data.Chn_Pan[i] * 2;

				m.Quirk |= Quirk_Flag.Ft2;
				m.Read_Event_Type = Read_Event.Ft2;
			}
			finally
			{
				if (!testMode)
					f.Hio_Close();
			}

			return 0;
		}

		#region Private methods

		#region IFF chunk handlers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Init(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;
			byte[] buf = new byte[64];

			if (f.Hio_Read(buf, 1, 64) < 64)
				return -1;

			mod.Name = encoder.GetString(buf);
			lib.common.LibXmp_Set_Type(m, "Galaxy Music System 5.0");

			c_int flags = f.Hio_Read8();	// Bit 0: Amiga period

			if ((~flags & 0x01) != 0)
				m.Period_Type = Containers.Common.Period.Linear;

			mod.Chn = f.Hio_Read8();
			mod.Spd = f.Hio_Read8();
			mod.Bpm = f.Hio_Read8();

			f.Hio_Read16L();		// Unknown - 0x01c5
			f.Hio_Read16L();		// Unknown - 0xff00
			f.Hio_Read8();			// Unknown - 0x80

			if (f.Hio_Read(data.Chn_Pan, 1, 64) != 64)
				return -1;

			// Sanity check
			if (mod.Chn > Constants.Xmp_Max_Channels)
				return -1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Ordr(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;

			mod.Len = f.Hio_Read8() + 1;
			// Don't follow Dr. Eggman's specs here

			for (c_int i = 0; i < mod.Len; i++)
				mod.Xxo[i] = f.Hio_Read8();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Patt_Cnt(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;

			c_int i = f.Hio_Read8() + 1;	// Pattern number

			if (i > mod.Pat)
				mod.Pat = i;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Inst_Cnt(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;

			f.Hio_Read32B();		// 42 01 00 00
			f.Hio_Read8();			// 00

			c_int i = f.Hio_Read8() + 1;	// Instrument number

			// Sanity check
			if (i > Constants.Max_Instruments)
				return -1;

			if (i > mod.Ins)
				mod.Ins = i;

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
			Xmp_Event dummy = new Xmp_Event();

			c_int i = f.Hio_Read8();	// Pattern number
			c_int len = (c_int)f.Hio_Read32L();

			c_int rows = f.Hio_Read8() + 1;

			// Sanity check - don't allow duplicate patterns
			if ((len < 0) || (mod.Xxp[i] != null))
				return -1;

			if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, rows) < 0)
				return -1;

			for (c_int r = 0; r < rows;)
			{
				uint8 flag = f.Hio_Read8();

				if (flag == 0)
				{
					r++;
					continue;
				}

				if (f.Hio_Error() != 0)
					return -1;

				c_int chan = flag & 0x1f;

				Xmp_Event @event = chan < mod.Chn ? Ports.LibXmp.Common.Event(m, i, chan, r) : dummy;

				if ((flag & 0x80) != 0)
				{
					uint8 fxp = f.Hio_Read8();
					uint8 fxt = f.Hio_Read8();

					switch (fxt)
					{
						// Speed
						case 0x14:
						{
							fxt = Effects.Fx_S3M_Speed;
							break;
						}

						default:
						{
							if (fxt > 0x0f)
								fxt = fxp = 0;

							break;
						}
					}

					@event.FxT = fxt;
					@event.FxP = fxp;
				}

				if ((flag & 0x40) != 0)
				{
					@event.Ins = f.Hio_Read8();
					@event.Note = f.Hio_Read8();

					if (@event.Note == 128)
						@event.Note = Constants.Xmp_Key_Off;
				}

				if ((flag & 0x20) != 0)
					@event.Vol = (byte)(1 + f.Hio_Read8() / 2);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Inst(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			byte[] buf = new byte[29];

			f.Hio_Read32B();		// 42 01 00 00
			f.Hio_Read8();			// 00
			c_int i = f.Hio_Read8();    // Instrument number

			// Sanity check - don't allow duplicate instruments
			if (mod.Xxi[i].Nsm != 0)
				return -1;

			f.Hio_Read(buf, 1, 28);
			mod.Xxi[i].Name = encoder.GetString(buf);

			f.Hio_Seek(290, SeekOrigin.Current);	// Sample/note map, envelopes
			mod.Xxi[i].Nsm = f.Hio_Read16L();

			if (mod.Xxi[i].Nsm == 0)
				return 0;

			if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, mod.Xxi[i].Nsm) < 0)
				return -1;

			// FIXME: Currently reading only the first sample

			f.Hio_Read32B();	// RIFF
			f.Hio_Read32B();	// Size
			f.Hio_Read32B();	// AS
			f.Hio_Read32B();	// SAMP
			f.Hio_Read32B();	// Size
			f.Hio_Read32B();	// Unknown - usually 0x40000000

			f.Hio_Read(buf, 1, 28);
			mod.Xxs[i].Name = encoder.GetString(buf);

			f.Hio_Read32B();	// Unknown - 0x0000
			f.Hio_Read8();      // Unknown - 0x00

			mod.Xxi[i].Sub[0].Sid = i;
			mod.Xxi[i].Vol = f.Hio_Read8();
			mod.Xxi[i].Sub[0].Pan = 0x80;
			mod.Xxi[i].Sub[0].Vol = (f.Hio_Read16L() + 1) / 512;

			c_int flags = f.Hio_Read16L();
			f.Hio_Read16L();	// Unknown - 0x0080

			mod.Xxs[i].Len = (c_int)f.Hio_Read32L();
			mod.Xxs[i].Lps = (c_int)f.Hio_Read32L();
			mod.Xxs[i].Lpe = (c_int)f.Hio_Read32L();

			mod.Xxs[i].Flg = Xmp_Sample_Flag.None;
			bool has_Unsigned_Sample = false;

			if ((flags & 0x04) != 0)
				mod.Xxs[i].Flg |= Xmp_Sample_Flag._16Bit;

			if ((flags & 0x08) != 0)
				mod.Xxs[i].Flg |= Xmp_Sample_Flag.Loop;

			if ((flags & 0x10) != 0)
				mod.Xxs[i].Flg |= Xmp_Sample_Flag.Loop | Xmp_Sample_Flag.Loop_BiDir;

			if ((~flags & 0x80) != 0)
				has_Unsigned_Sample = true;

			c_int sRate = (c_int)f.Hio_Read32L();
			c_int finetune = 0;
			lib.period.LibXmp_C2Spd_To_Note(sRate, out mod.Xxi[i].Sub[0].Xpo, out mod.Xxi[i].Sub[0].Fin);
			mod.Xxi[i].Sub[0].Fin += finetune;

			f.Hio_Read32L();	// 0x00000000
			f.Hio_Read32L();	// Unknown

			if (mod.Xxs[i].Len > 1)
			{
				if (Sample.LibXmp_Load_Sample(m, f, has_Unsigned_Sample ? Sample_Flag.Uns : Sample_Flag.None, mod.Xxs[i], null, i) < 0)
					return -1;
			}

			return 0;
		}
		#endregion

		#endregion
	}
}
