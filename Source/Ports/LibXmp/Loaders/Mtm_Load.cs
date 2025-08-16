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
	/// 
	/// </summary>
	internal class Mtm_Load : IFormatLoader
	{
		#region Internal structures

		#region Mtm_File_Header
		private class Mtm_File_Header
		{
			public uint8[] Magic { get; } = new uint8[3];		// "MTM"
			public uint8 Version { get; set; }					// MSN=major, LSN=minor
			public uint8[] Name { get; } = new uint8[20];		// ASCIIZ module name
			public uint16 Tracks { get; set; }					// Number of tracks saved
			public uint8 Patterns { get; set; }					// Number of patterns saved
			public uint8 ModLen { get; set; }					// Module length
			public uint16 ExtraLen { get; set; }				// Length of the comment field
			public uint8 Samples { get; set; }					// Number of samples
			public uint8 Attr { get; set; }						// Always zero
			public uint8 Rows { get; set; }						// Number rows per track
			public uint8 Channels { get; set; }					// Number of tracks per pattern
			public uint8[] Pan { get; } = new uint8[32];		// Pan positions for each channel
		}
		#endregion

		#region Mtm_Instrument_Header
		private class Mtm_Instrument_Header
		{
			public uint8[] Name { get; } = new uint8[22];		// Instrument name
			public uint32 Length { get; set; }					// Instrument length in bytes
			public uint32 Loop_Start { get; set; }				// Sample loop start
			public uint32 LoopEnd { get; set; }					// Sample loop end
			public uint8 FineTune { get; set; }					// Finetune
			public uint8 Volume { get; set; }					// Playback volume
			public uint8 Attr { get; set; }						// &0x01: 16 bit sample
		}
		#endregion

		#endregion

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Mtm = new Format_Loader
		{
			Id = Guid.Parse("2C7EEE56-803D-49E5-936D-6AD8FC14B013"),
			Name = "MultiTracker",
			Description = "This loader recognizes the “MultiTracker” modules. This tracker from the PC, was the first one to have 32 channels and supports GUS soundcards. It introduced the mtm file format, which I will say is the successor to the mod format. In this format, a pattern contains individual tracks, which can be combined as will. Tracker was written by Daniel Goldstein.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Mtm_Load(LibXmp libXmp)
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
			return new Mtm_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			uint8[] buf = new uint8[4];

			if (f.Hio_Read(buf, 1, 4) < 4)
				return -1;

			if (CMemory.MemCmp(buf, "MTM", 3) != 0)
				return -1;

			if (buf[3] != 0x10)
				return -1;

			lib.common.LibXmp_Read_Title(f, out t, 20, encoder);

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
			Mtm_File_Header mfh = new Mtm_File_Header();
			Mtm_Instrument_Header mih = new Mtm_Instrument_Header();
			CPointer<uint8> mt = new CPointer<uint8>(192);
			bool[] fxx = new bool[2];

			f.Hio_Read(mfh.Magic, 3, 1);		// "MTM"
			mfh.Version = f.Hio_Read8();				// MSN=major, LSN=minor
			f.Hio_Read(mfh.Name, 20, 1);		// ASCIIZ module name
			mfh.Tracks = f.Hio_Read16L();				// Number of tracks saved
			mfh.Patterns = f.Hio_Read8();				// Number of patterns saved
			mfh.ModLen = f.Hio_Read8();					// Module length
			mfh.ExtraLen = f.Hio_Read16L();				// Length of the comment field

			mfh.Samples = f.Hio_Read8();				// Number of samples
			if (mfh.Samples > 63)
				return -1;

			mfh.Attr = f.Hio_Read8();					// Always zero

			mfh.Rows = f.Hio_Read8();					// Number rows per track
			if (mfh.Rows != 64)
				return -1;

			mfh.Channels = f.Hio_Read8();				// Number of tracks per pattern
			if (mfh.Channels > Math.Min(32, Constants.Xmp_Max_Channels))
				return -1;

			f.Hio_Read(mfh.Pan, 32, 1);		// Pan positions for each channel

			if (f.Hio_Error() != 0)
				return -1;

			mod.Trk = mfh.Tracks + 1;
			mod.Pat = mfh.Patterns + 1;
			mod.Len = mfh.ModLen + 1;
			mod.Ins = mfh.Samples;
			mod.Smp = mod.Ins;
			mod.Chn = mfh.Channels;
			mod.Spd = 6;
			mod.Bpm = 125;
			m.Module_Flags |= Xmp_Module_Flags.Uses_Tracks;

			mod.Name = encoder.GetString(mfh.Name, 0, 20);
			lib.common.LibXmp_Set_Type(m, string.Format("MultiTracker {0}.{1:D2} MTM", Ports.LibXmp.Common.Msn(mfh.Version), Ports.LibXmp.Common.Lsn(mfh.Version)));

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			// Read and convert instruments
			for (c_int i = 0; i < mod.Ins; i++)
			{
				Xmp_Instrument xxi = mod.Xxi[i];
				Xmp_Sample xxs = mod.Xxs[i];

				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					return -1;

				Xmp_SubInstrument sub = xxi.Sub[0];

				f.Hio_Read(mih.Name, 22, 1);	// Instrument name
				mih.Length = f.Hio_Read32L();			// Instrument length in bytes

				if (mih.Length > Constants.Max_Sample_Size)
					return -1;

				mih.Loop_Start = f.Hio_Read32L();		// Sample loop start
				mih.LoopEnd = f.Hio_Read32L();			// Sample loop end
				mih.FineTune = f.Hio_Read8();			// Finetune
				mih.Volume = f.Hio_Read8();				// Playback volume
				mih.Attr = f.Hio_Read8();				// &0x01: 16 bit sample

				xxs.Len = (c_int)mih.Length;
				xxs.Lps = (c_int)mih.Loop_Start;
				xxs.Lpe = (c_int)mih.LoopEnd;
				xxs.Flg = (xxs.Lpe > 2) ? Xmp_Sample_Flag.Loop : Xmp_Sample_Flag.None;

				if ((mih.Attr & 1) != 0)
				{
					xxs.Flg |= Xmp_Sample_Flag._16Bit;
					xxs.Len >>= 1;
					xxs.Lps >>= 1;
					xxs.Lpe >>= 1;
				}

				sub.Vol = mih.Volume;
				sub.Fin = (int8)(mih.FineTune << 4);
				sub.Pan = 0x80;
				sub.Sid = i;

				lib.common.LibXmp_Instrument_Name(mod, i, mih.Name, 22, encoder);

				if (xxs.Len > 0)
					mod.Xxi[i].Nsm = 1;
			}

			f.Hio_Read(mod.Xxo, 1, 128);

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				return -1;

			fxx[0] = fxx[1] = false;

			for (c_int i = 0; i < mod.Trk; i++)
			{
				if (lib.common.LibXmp_Alloc_Track(mod, i, mfh.Rows) < 0)
					return -1;

				if (i == 0)
					continue;

				if (f.Hio_Read(mt, 3, 64) != 64)
					return -1;

				for (c_int j = 0; j < 64; j++)
				{
					Xmp_Event e = mod.Xxt[i].Event[j];
					CPointer<uint8> d = mt + j * 3;

					e.Note = (byte)(d[0] >> 2);
					if (e.Note != 0)
						e.Note += 37;

					e.Ins = (byte)(((d[0] & 0x3) << 4) + Ports.LibXmp.Common.Msn(d[1]));
					e.FxT = Ports.LibXmp.Common.Lsn(d[1]);
					e.FxP = d[2];

					if (e.FxT > Effects.Fx_Speed)
						e.FxT = e.FxP = 0;

					// See tempo mode detection below
					if (e.FxT == Effects.Fx_Speed)
						fxx[e.FxP >= 0x20 ? 1 : 0] = true;

					// Set pan effect translation
					if ((e.FxT == Effects.Fx_Extended) && (Ports.LibXmp.Common.Msn(e.FxP) == 0x8))
					{
						e.FxT = Effects.Fx_SetPan;
						e.FxP <<= 4;
					}
				}
			}

			// Read patterns
			for (c_int i = 0; i < mod.Pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern(mod, i) < 0)
					return -1;

				mod.Xxp[i].Rows = 64;

				for (c_int j = 0; j < 32; j++)
				{
					c_int track = f.Hio_Read16L();

					if (track >= mod.Trk)
						track = 0;

					if (j < mod.Chn)
						mod.Xxp[i].Index[j] = track;
				}
			}

			// Tempo mode detection.
			//
			// The MTM tempo effect has an unusual property: when speed is set, the
			// tempo resets to 125, and when tempo is set, the speed resets to 6.
			// Modules that use both speed and tempo effects need to emulate this.
			// See: Absolve the Ambience by Sybaris, Soma by Ranger Rick.
			// 
			// Dual Module Player and other DOS players did not know about this and
			// did not implement support for it, and instead used Protracker Fxx.
			// Many MTM authors created modules that rely on this which are various
			// degrees of broken in the tracker they were made with! Several MTMs
			// by Phoenix and Silent Mode expect this. The majority of them can be
			// detected by checking for high Fxx and low Fxx on the same row
			if (fxx[0] && fxx[1])
			{
				// Both used, check patterns
				for (c_int i = 0; i < mod.Pat; i++)
				{
					for (c_int j = 0; j < mfh.Rows; j++)
					{
						fxx[0] = fxx[1] = false;

						for (c_int k = 0; k < mod.Chn; k++)
						{
							Xmp_Event e = Ports.LibXmp.Common.Event(m, i, k, j);

							if (e.FxT == Effects.Fx_Speed)
								fxx[e.FxP >= 0x20 ? 1 : 0] = true;
						}

						if (fxx[0] && fxx[1])
						{
							// Same row, no change required
							goto Probably_Dmp;
						}
					}
				}

				for (c_int i = 0; i < mod.Pat; i++)
				{
					for (c_int j = 0; j < mfh.Rows; j++)
					{
						for (c_int k = 0; k < mod.Chn; k++)
						{
							Xmp_Event e = Ports.LibXmp.Common.Event(m, i, k, j);

							if (e.FxT == Effects.Fx_Speed)
							{
								e.F2T = Effects.Fx_Speed;
								e.F2P = (byte)((e.FxP < 0x20) ? 125 : 6);
							}
						}
					}
				}
			}
			Probably_Dmp:

			// Comments
			if (mfh.ExtraLen != 0)
			{
				uint8[] comment = new uint8[mfh.ExtraLen + 1];
				if (comment != null)
				{
					// Comments are stored in 40 byte ASCIIZ lines
					c_int len = (c_int)f.Hio_Read(comment, 1, mfh.ExtraLen);
					c_int last_Line = 0;

					for (c_int i = 0; i + 40 <= len; i += 40)
					{
						if (comment[i] != 0x00)
							last_Line = i + 40;
					}

					c_int j, line;

					for (j = 0, line = 0; line < last_Line; line += 40)
					{
						c_int pos = line;

						for (c_int i = 0; i < 39; i++)
						{
							if (comment[pos + i] == 0x00)
								break;

							comment[j++] = comment[pos + i];
						}

						comment[j++] = 10;
					}

					comment[j] = 0x00;

					m.Comment = encoder.GetString(comment).Replace('\u25d9', '\n');
				}
				else
					f.Hio_Seek(mfh.ExtraLen, SeekOrigin.Current);
			}

			// Read samples
			for (c_int i = 0; i < mod.Ins; i++)
			{
				if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.Uns, mod.Xxs[i], null, i) < 0)
					return -1;
			}

			for (c_int i = 0; i < mod.Chn; i++)
				mod.Xxc[i].Pan = mfh.Pan[i] << 4;

			return 0;
		}
	}
}
