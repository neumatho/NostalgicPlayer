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
	/// Galaxy Music System 4.0 module file loader
	///
	/// Based on modules converted using mod2j2b.exe
	/// </summary>
	internal class Gal4_Load : IFormatLoader
	{
		#region Internal structures

		#region Local_Data
		private class Local_Data
		{
			public c_int SNum;
		}
		#endregion

		#endregion

		private const int Gal4_Samp_16Bit = 1 << 2;
		private const int Gal4_Samp_Loop = 1 << 3;
		private const int Gal4_Samp_Loop_BiDir = 1 << 4;
		private const int Gal4_Samp_Set_Panning = 1 << 5;

		private readonly LibXmp lib;
		private readonly Encoding encoder;
		private readonly bool testMode;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Gal4 = new Format_Loader
		{
			Id = Guid.Parse("21DB3451-6506-43D7-8FBF-215D4FB67F31"),
			Name = "Galaxy Music System 4.0",
			Description = "This format is a conversion and packer format used in the game Jazz Jackrabbit 2.\n\nEarlier versions of the game did not have a specific music format, but used Scream Tracker 3 or Impulse Tracker formats directly. Later on, this format was invented and used.\n\nThis loader can load version 4 of the format.",
			Create = Create
		};

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_TestOnly = new Format_Loader
		{
			Id = Guid.Parse("A086A486-7843-48A6-B99C-0B9245E750FC"),
			Name = "Test only",
			OnlyAvailableInTest = true,
			Create = Create_TestOnly
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Gal4_Load(LibXmp libXmp, bool test)
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
			return new Gal4_Load(libXmp, false);
		}



		/********************************************************************/
		/// <summary>
		/// Create a new instance of the loader
		/// </summary>
		/********************************************************************/
		private static IFormatLoader Create_TestOnly(LibXmp libXmp, Xmp_Context ctx)
		{
			return new Gal4_Load(libXmp, true);
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

				if (f.Hio_Read32B() != Common.Magic4('A', 'M', 'F', 'F'))
					return -1;

				if (f.Hio_Read32B() != Common.Magic4('M', 'A', 'I', 'N'))
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

				c_int offset = (c_int)f.Hio_Tell();

				mod.Smp = mod.Ins = 0;

				Iff handle = Iff.LibXmp_Iff_New();
				if (handle == null)
					return -1;

				m.C4Rate = Constants.C4_Ntsc_Rate;

				// IFF chunk IDs
				c_int ret = handle.LibXmp_Iff_Register("MAIN".ToPointer(), Get_Main);
				ret |= handle.LibXmp_Iff_Register("ORDR".ToPointer(), Get_Ordr);
				ret |= handle.LibXmp_Iff_Register("PATT".ToPointer(), Get_Patt_Cnt);
				ret |= handle.LibXmp_Iff_Register("INST".ToPointer(), Get_Inst_Cnt);

				if (ret != 0)
					return -1;

				handle.LibXmp_Iff_Set_Quirk(Iff_Quirk_Flag.Little_Endian);
				handle.LibXmp_Iff_Set_Quirk(Iff_Quirk_Flag.Chunk_Trunc4);

				// Load IFF chunks
				if (handle.LibXmp_Iff_Load(m, f, data) < 0)
				{
					handle.LibXmp_Iff_Release();
					return -1;
				}

				handle.LibXmp_Iff_Release();

				mod.Trk = mod.Pat * mod.Chn;

				if (lib.common.LibXmp_Init_Instrument(m) < 0)
					return -1;

				if (lib.common.LibXmp_Init_Pattern(mod) < 0)
					return -1;

				f.Hio_Seek(start + offset, SeekOrigin.Begin);
				data.SNum = 0;

				handle = Iff.LibXmp_Iff_New();
				if (handle == null)
					return -1;

				// IFF chunk IDs
				ret = handle.LibXmp_Iff_Register("PATT".ToPointer(), Get_Patt);
				ret |= handle.LibXmp_Iff_Register("INST".ToPointer(), Get_Inst);

				if (ret != 0)
					return -1;

				handle.LibXmp_Iff_Set_Quirk(Iff_Quirk_Flag.Little_Endian);
				handle.LibXmp_Iff_Set_Quirk(Iff_Quirk_Flag.Chunk_Trunc4);

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
					mod.Xxc[i].Pan = 0x80;

				// TODO: JJ2 1.20 shareware does not support fine slides from
				// converted XMs. Should be tested with converted S3Ms, though
				m.Quirk |= Quirk_Flag.FineFx;
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
		private c_int Get_Main(Module_Data m, uint32 size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			byte[] buf = new byte[64];

			if (f.Hio_Read(buf, 1, 64) < 64)
				return -1;

			mod.Name = encoder.GetString(buf);
			lib.common.LibXmp_Set_Type(m, "Galaxy Music System 4.0");

			c_int flags = f.Hio_Read8();

			if ((~flags & 0x01) != 0)
				m.Period_Type = Containers.Common.Period.Linear;

			mod.Chn = f.Hio_Read8();
			mod.Spd = f.Hio_Read8();
			mod.Bpm = f.Hio_Read8();

			f.Hio_Read16L();		// Unknown - 0x01c5
			f.Hio_Read16L();		// Unknown - 0xff00
			f.Hio_Read8();			// Unknown - 0x80

			// Sanity check
			if (mod.Chn > 32)
				return -1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Ordr(Module_Data m, uint32 size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;

			mod.Len = f.Hio_Read8() + 1;
			if (f.Hio_Error() != 0)
				return -1;

			for (c_int i = 0; i < mod.Len; i++)
				mod.Xxo[i] = f.Hio_Read8();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Patt_Cnt(Module_Data m, uint32 size, Hio f, object parm)
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
		private c_int Get_Inst_Cnt(Module_Data m, uint32 size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;

			f.Hio_Read8();			// 00

			c_int i = f.Hio_Read8() + 1;	// Instrument number

			// Sanity check
			if (i > Constants.Max_Instruments)
				return -1;

			if (i > mod.Ins)
				mod.Ins = i;

			f.Hio_Seek(28, SeekOrigin.Current);		// Skip name

			mod.Smp += f.Hio_Read8();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Patt(Module_Data m, uint32 size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Xmp_Event dummy = new Xmp_Event();

			c_int i = f.Hio_Read8();	// Pattern number
			c_int len = (c_int)f.Hio_Read32L();

			// Sanity check
			if ((i >= mod.Pat) || (len <= 0) || (mod.Xxp[i] != null))
				return -1;

			c_int rows = f.Hio_Read8() + 1;

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
		private c_int Get_Inst(Module_Data m, uint32 size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;
			CPointer<byte> buf = new CPointer<byte>(30);

			f.Hio_Read8();			// 00
			c_int i = f.Hio_Read8();    // Instrument number

			// Sanity check
			if ((i >= mod.Ins) || (mod.Xxi[i].Nsm != 0))
				return -1;

			Xmp_Instrument xxi = mod.Xxi[i];
			Xmp_Envelope aei = xxi.Aei;
			Xmp_Envelope pei = xxi.Pei;

			f.Hio_Read(buf, 1, 28);
			xxi.Name = encoder.GetString(buf.AsSpan());

			xxi.Nsm = f.Hio_Read8();

			for (c_int j = 0; j < 108; j++)
				xxi.Map[j].Ins = f.Hio_Read8();

			f.Hio_Seek(11, SeekOrigin.Current);		// Unknown

			c_int vwf = f.Hio_Read8();			// Vibrato waveform
			c_int vsw = f.Hio_Read8();			// Vibrato sweep
			f.Hio_Read8();						// Unknown
			f.Hio_Read8();						// Unknown
			c_int vde = f.Hio_Read8();			// Vibrato depth
			c_int vra = f.Hio_Read16L() / 16;	// Vibrato waveform
			f.Hio_Read8();						// Unknown

			c_int val = f.Hio_Read8();			// PV envelopes flags

			if ((Ports.LibXmp.Common.Lsn(val) & 0x01) != 0)
				aei.Flg |= Xmp_Envelope_Flag.On;

			if ((Ports.LibXmp.Common.Lsn(val) & 0x02) != 0)
				aei.Flg |= Xmp_Envelope_Flag.Sus;

			if ((Ports.LibXmp.Common.Lsn(val) & 0x04) != 0)
				aei.Flg |= Xmp_Envelope_Flag.Loop;

			if ((Ports.LibXmp.Common.Msn(val) & 0x01) != 0)
				pei.Flg |= Xmp_Envelope_Flag.On;

			if ((Ports.LibXmp.Common.Msn(val) & 0x02) != 0)
				pei.Flg |= Xmp_Envelope_Flag.Sus;

			if ((Ports.LibXmp.Common.Msn(val) & 0x04) != 0)
				pei.Flg |= Xmp_Envelope_Flag.Loop;

			val = f.Hio_Read8();				// PV envelopes points
			aei.Npt = Ports.LibXmp.Common.Lsn(val) + 1;
			pei.Npt = Ports.LibXmp.Common.Msn(val) + 1;

			val = f.Hio_Read8();				// PV envelopes sustain point
			aei.Sus = Ports.LibXmp.Common.Lsn(val);
			pei.Sus = Ports.LibXmp.Common.Msn(val);

			val = f.Hio_Read8();				// PV envelopes loop start
			aei.Lps = Ports.LibXmp.Common.Lsn(val);
			pei.Lps = Ports.LibXmp.Common.Msn(val);

			val = f.Hio_Read8();				// PV envelopes loop end
			aei.Lpe = Ports.LibXmp.Common.Lsn(val);
			pei.Lpe = Ports.LibXmp.Common.Msn(val);

			if ((aei.Npt <= 0) || (aei.Npt > Math.Min(10, Constants.Xmp_Max_Env_Points)))
				aei.Flg &= ~Xmp_Envelope_Flag.On;

			if ((pei.Npt <= 0) || (pei.Npt > Math.Min(10, Constants.Xmp_Max_Env_Points)))
				pei.Flg &= ~Xmp_Envelope_Flag.On;

			if (f.Hio_Read(buf, 1, 30) < 30)		// Volume envelope points
				return -1;

			for (c_int j = 0; j < aei.Npt; j++)
			{
				if (j >= 10)
					break;

				aei.Data[j * 2] = (c_short)(DataIo.ReadMem16L(buf + (j * 3)) / 16);
				aei.Data[(j * 2) + 1] = buf[(j * 3) + 2];
			}

			if (f.Hio_Read(buf, 1, 30) < 30)		// Pan envelope points
				return -1;

			for (c_int j = 0; j < pei.Npt; j++)
			{
				if (j >= 10)
					break;

				pei.Data[j * 2] = (c_short)(DataIo.ReadMem16L(buf + (j * 3)) / 16);
				pei.Data[(j * 2) + 1] = buf[(j * 3) + 2];
			}

			f.Hio_Read8();		// Fadeout - 0x80->0x02 0x310->0x0c
			f.Hio_Read8();		// Unknown

			if (xxi.Nsm == 0)
				return 0;

			if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, xxi.Nsm) < 0)
				return -1;

			for (c_int j = 0; j < xxi.Nsm; j++, data.SNum++)
			{
				Xmp_SubInstrument sub = xxi.Sub[j];
				Xmp_Sample xxs = mod.Xxs[data.SNum];

				f.Hio_Read32B();	// SAMP
				f.Hio_Read32B();	// Size

				f.Hio_Read(buf, 1, 28);
				xxs.Name = encoder.GetString(buf.AsSpan());

				c_int pan = f.Hio_Read8();
				sub.Vol = f.Hio_Read8();

				c_int flags = f.Hio_Read8();
				f.Hio_Read8();      // Unknown - 0x80

				sub.Pan = (flags & Gal4_Samp_Set_Panning) != 0 ? Math.Min(pan * 4, 255) : Constants.Xmp_Inst_No_Default_Pan;
				sub.Vwf = vwf;
				sub.Vde = vde;
				sub.Vra = vra;
				sub.Vsw = vsw;
				sub.Sid = data.SNum;

				xxs.Len = (c_int)f.Hio_Read32L();
				xxs.Lps = (c_int)f.Hio_Read32L();
				xxs.Lpe = (c_int)f.Hio_Read32L();

				xxs.Flg = Xmp_Sample_Flag.None;

				if ((flags & Gal4_Samp_16Bit) != 0)
					xxs.Flg |= Xmp_Sample_Flag._16Bit;

				if ((flags & Gal4_Samp_Loop) != 0)
					xxs.Flg |= Xmp_Sample_Flag.Loop;

				if ((flags & Gal4_Samp_Loop_BiDir) != 0)
					xxs.Flg |= Xmp_Sample_Flag.Loop_BiDir;

				c_int sRate = (c_int)f.Hio_Read32L();
				c_int finetune = 0;
				lib.period.LibXmp_C2Spd_To_Note(sRate, out sub.Xpo, out sub.Fin);
				sub.Fin += finetune;

				f.Hio_Read32L();	// 0x00000000
				f.Hio_Read32L();	// Unknown

				if (xxs.Len > 1)
				{
					if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, xxs, null, data.SNum) < 0)
						return -1;
				}
			}

			return 0;
		}
		#endregion

		#endregion
	}
}
