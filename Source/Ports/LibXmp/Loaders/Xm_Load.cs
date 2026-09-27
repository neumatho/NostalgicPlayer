/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibVorbis;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Containers;
using Polycode.NostalgicPlayer.Ports.LibVorbisFile;
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
	internal class Xm_Load : IFormatLoader
	{
		#region Internal structures
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

		#region XM flags
		[Flags]
		private enum Xm_Flags : uint16
		{
			Linear_Period_Mode = 0x01
		}
		#endregion

		#region XM envelope flags
		[Flags]
		private enum Xm_Envelope_Flag : uint8
		{
			On = 0x01,
			Sustain = 0x02,
			Loop = 0x04
		}
		#endregion

		#region XM sample flags
		[Flags]
		private enum Xm_Sample_Flag : uint8
		{
			None = 0,
			Loop_Forward = 0x01,
			Loop_PingPong = 0x02,
			Loop_Mask = Loop_Forward | Loop_PingPong,
			_16Bit = 0x10,
			Stereo = 0x20
		}
		#endregion

		#region Xm_File_Header
		private class Xm_File_Header
		{
			public readonly uint8[] Id = new uint8[17];			// ID text: "Extended module: "
			public readonly uint8[] Name = new uint8[20];		// Module name, padded with zeros
			public uint8 DosEof;								// 0x1a
			public readonly uint8[] Tracker = new uint8[20];	// Tracker name
			public uint16 Version;								// Version number, minor-major
			public uint32 HeaderSz;								// Header size
			public uint16 SongLen;								// Song length (in pattern order table)
			public uint16 Restart;								// Restart position
			public uint16 Channels;								// Number of channels (2,4,6,8,10,...,32)
			public uint16 Patterns;								// Number of patterns (max 256)
			public uint16 Instruments;							// Number of instruments (max 128)
			public Xm_Flags Flags;								// Bit 0: 0=Amiga freq table, 1=Linear
			public uint16 Tempo;								// Default tempo
			public uint16 Bpm;									// Default BPM
			public readonly uint8[] Order = new uint8[256];		// Pattern order table
		}
		#endregion

		#region Xm_Pattern_Header
		private class Xm_Pattern_Header
		{
			public uint32 Length;								// Pattern header length
			public uint8 Packing;								// Packing type (always 0)
			public uint16 Rows;									// Number of rows in pattern (1..256)
			public uint16 DataSize;								// Packed patterndata size
		}
		#endregion

		#region Xm_Instrument_Header
		private class Xm_Instrument_Header
		{
			public uint32 Size;									// Instrument size
			public readonly uint8[] Name = new uint8[22];		// Instrument name
			public uint8 Type;									// Instrument type (always 0)
			public uint16 Samples;								// Number of samples in instrument
			public uint32 Sh_Size;								// Sample header size
		}
		#endregion

		#region Xm_Instrument
		private class Xm_Instrument
		{
			public readonly uint8[] Sample = new uint8[96];		// Sample number for all notes
			public readonly uint16[] V_Env = new uint16[24];	// Points for volume envelope
			public readonly uint16[] P_Env = new uint16[24];	// Points for panning envelope
			public uint8 V_Pts;									// Number of volume points
			public uint8 P_Pts;									// Number of panning points
			public uint8 V_Sus;									// Volume sustain point
			public uint8 V_Start;								// Volume loop start point
			public uint8 V_End;									// Volume loop end point
			public uint8 P_Sus;									// Panning sustain point
			public uint8 P_Start;								// Panning loop start point
			public uint8 P_End;									// Panning loop end point
			public Xm_Envelope_Flag V_Type;						// Bit 0: On; 1: Sustain; 2: Loop
			public Xm_Envelope_Flag P_Type;						// Bit 0: On; 1: Sustain; 2: Loop
			public uint8 Y_Wave;								// Vibrato waveform
			public uint8 Y_Sweep;								// Vibrato sweep
			public uint8 Y_Depth;								// Vibrato depth
			public uint8 Y_Rate;								// Vibrato rate
			public uint16 V_Fade;								// Volume fadeout
		}
		#endregion

		#region XM tracker versions
		/// <summary>
		/// TNE: Trackers which could have written an XM file. Ported from
		/// the OpenMPT loader and only used to tell ModPlug Tracker and
		/// OpenMPT apart from everything else
		/// </summary>
		[Flags]
		private enum Xm_Tracker_Version
		{
			/// <summary>
			/// Probably not made with MPT
			/// </summary>
			Unknown = 0x00,

			/// <summary>
			/// Made with MPT alpha / beta
			/// </summary>
			OldModPlug = 0x01,

			/// <summary>
			/// Made with MPT (not alpha / beta)
			/// </summary>
			NewModPlug = 0x02,

			/// <summary>
			/// Made with OpenMPT
			/// </summary>
			OpenMpt = 0x08,

			/// <summary>
			/// We are very sure that we found the correct tracker version
			/// </summary>
			Confirmed = 0x10,

			/// <summary>
			/// "FastTracker v2.00", but FastTracker has not been ruled out
			/// </summary>
			Ft2Generic = 0x20,

			/// <summary>
			/// Not FastTracker 2: The instrument type changed between two
			/// instruments, or a null character was found in the song title
			/// </summary>
			Ft2Clone = 0x80,

			/// <summary>
			/// Could be PlayerPRO
			/// </summary>
			PlayerPro = 0x100,

			/// <summary>
			/// Probably DigiTrakker
			/// </summary>
			DigiTrakker = 0x200
		}
		#endregion

		#region Xm_Sample_Header
		private class Xm_Sample_Header
		{
			public uint32 Length;								// Sample length
			public uint32 Loop_Start;							// Sample loop start
			public uint32 Loop_Length;							// Sample loop length
			public uint8 Volume;								// Volume
			public int8 FineTune;								// Finetune (signed byte -128..+127)
			public Xm_Sample_Flag Type;							// Flags
			public uint8 Pan;									// Panning (0-255)
			public int8 RelNote;								// Relative note number (signed byte)
			public uint8 Reserved;								// Reserved
			public readonly uint8[] Name = new uint8[22];		// Sample name
		}
		#endregion

#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
		#endregion

		private enum Format
		{
			Xm,
			OggMod
		}

		private const uint8 Xm_Event_Packing = 0x80;
		private const uint8 Xm_Event_Pack_Mask = 0x7f;
		private const uint8 Xm_Event_Note_Follows = 0x01;
		private const uint8 Xm_Event_Instrument_Follows = 0x02;
		private const uint8 Xm_Event_Volume_Follows = 0x04;
		private const uint8 Xm_Event_FxType_Follows = 0x08;
		private const uint8 Xm_Event_FxParm_Follows = 0x10;

		// Packed structures size
		private const c_int Xm_Inst_Header_Size = 29;
		private const c_int Xm_Inst_Size = 212;

		/// <summary>
		/// grass.near.the.house.xm defines 23 samples in instrument 1. FT2 docs
		/// specify at most 16. See https://github.com/libxmp/libxmp/issues/168
		/// for more details
		/// </summary>
		private const int Xm_Max_Samples_Per_Inst = 32;

		// Ogg
		private const uint Magic_Oggs = 0x4f676753;

		// TNE: Chunks written by ModPlug Tracker / OpenMPT
		private const uint Magic_Text = 0x74657874;
		private const uint Magic_Midi = 0x4d494449;
		private const uint Magic_Pnam = 0x504e414d;
		private const uint Magic_Cnam = 0x434e414d;
		private const uint Magic_Chfx = 0x43484658;
		private const uint Magic_Xtpm = 0x5854504d;
		private const uint Magic_Stpm = 0x5354504d;
		private const uint Magic_Impi = 0x494d5049;
		private const uint Magic_Imps = 0x494d5053;

		// TNE: Sizes and flags used by the ModPlug Tracker / OpenMPT test
		private const uint32 Xm_Instrument_Header_Size = 263;
		private const uint32 Xm_Sample_Header_Size = 40;
		private const uint8 Xm_Envelope_Loop = 0x04;
		private const uint8 Xm_Sample_Adpcm = 0xad;
		private const uint16 Xm_Extended_Filter_Range = 0x1000;

		private readonly Format format;
		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Xm = new Format_Loader
		{
			Id = Guid.Parse("1574A876-5F9D-4BAE-81AF-7DB01370ADDD"),
			Name = "FastTracker II",
			Description = "This loader recognizes “FastTracker 2” modules. This format was designed from scratch, instead of creating yet another ProTracker variation. It was the first format using instruments as well as samples, and envelopes for finer effects.\nFastTracker 2 was written by Fredrik Huss and Magnus Hogdahl, and released in 1994.",
			Create = Create_Xm
		};

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_OggMod = new Format_Loader
		{
			Id = Guid.Parse("F1878ED9-37B8-4D5F-9AFE-46B6A9C195DF"),
			Name = "OggMod",
			Description = "This format is the same as FastTracker 2, except that the samples are packed with Ogg-Vorbis. This make the modules smaller. The tool was created by Neil Graham.",
			Create = Create_OggMod
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Xm_Load(LibXmp libXmp, Format format)
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
		private static IFormatLoader Create_Xm(LibXmp libXmp, Xmp_Context ctx)
		{
			return new Xm_Load(libXmp, Format.Xm);
		}



		/********************************************************************/
		/// <summary>
		/// Create a new instance of the loader
		/// </summary>
		/********************************************************************/
		private static IFormatLoader Create_OggMod(LibXmp libXmp, Xmp_Context ctx)
		{
			return new Xm_Load(libXmp, Format.OggMod);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			CPointer<byte> buf = new CPointer<byte>(20);

			if (f.Hio_Read(buf, 1, 17) < 17)		// ID text
				return -1;

			if (CMemory.memcmp(buf, "Extended Module: ", 17) != 0)
				return -1;

			lib.common.LibXmp_Read_Title(f, out t, 20, encoder);

			if (FindFormat(f) != format)
				return -1;

			// TNE: OggMod modules are never taken by the OpenMPT player,
			// so those are always kept here
			if (format == Format.OggMod)
				return 0;

			return Test_Extended(f, start);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Loader(Module_Data m, Hio f, c_int start)
		{
			Xmp_Module mod = m.Mod;
			Xm_File_Header xfh = new Xm_File_Header();
			bool claims_Ft2 = false;
			bool is_Mpt_Old = false;
			bool is_Mpt_116 = false;
			bool mpt_Ins_Headers = false;
			CPointer<byte> buf = new CPointer<byte>(80);

			if (f.Hio_Read(buf, 80, 1) != 1)
				return -1;

			CMemory.memcpy(xfh.Id, buf, 17);		// ID text
			CMemory.memcpy(xfh.Name, buf + 17, 20);	// Module name

			// Skip 0x1a

			CMemory.memcpy(xfh.Tracker, buf + 38, 20);// Tracker name
			xfh.Version = DataIo.ReadMem16L(buf + 58);		// Version number, minor-major
			xfh.HeaderSz = DataIo.ReadMem32L(buf + 60);		// Header size
			xfh.SongLen = DataIo.ReadMem16L(buf + 64);		// Song length
			xfh.Restart = DataIo.ReadMem16L(buf + 66);		// Restart position
			xfh.Channels = DataIo.ReadMem16L(buf + 68);		// Number of channels
			xfh.Patterns = DataIo.ReadMem16L(buf + 70);		// Number of patterns
			xfh.Instruments = DataIo.ReadMem16L(buf + 72);	// Number of instruments
			xfh.Flags = (Xm_Flags)DataIo.ReadMem16L(buf + 74);// 0=Amiga freq table, 1=Linear
			xfh.Tempo = DataIo.ReadMem16L(buf + 76);			// Default tempo
			xfh.Bpm = DataIo.ReadMem16L(buf + 78);			// Default BPM

			// Sanity checks
			if (xfh.SongLen > 256)
				return -1;

			if (xfh.Patterns > 256)
				return -1;

			if (xfh.Instruments > 255)
				return -1;

			if (xfh.Channels > Constants.Xmp_Max_Channels)
				return -1;

			// FT2 and MPT allow up to 255 BPM. OpenMPT allows up to 1000 BPM
			if ((xfh.Tempo >= 32) || (xfh.Bpm < 32) || (xfh.Bpm > 1000))
			{
				if (CMemory.memcmp(xfh.Tracker, "MED2XM", 6) != 0)
					return -1;
			}

			// Honor header size -- needed by BoobieSqueezer XMs
			c_int len = (c_int)xfh.HeaderSz - 0x14;

			if ((len < 0) || (len > 256))
				return -1;

			CMemory.memset<uint8>(xfh.Order, 0, (size_t)xfh.Order.Length);

			if (f.Hio_Read(xfh.Order, (size_t)len, 1) != 1)	// Pattern order table
				return -1;

			mod.Name = encoder.GetString(xfh.Name, 0, 20);

			mod.Len = xfh.SongLen;
			mod.Chn = xfh.Channels;
			mod.Pat = xfh.Patterns;
			mod.Ins = xfh.Instruments;
			mod.Rst = xfh.Restart >= xfh.SongLen ? 0 : xfh.Restart;
			mod.Spd = xfh.Tempo;
			mod.Bpm = xfh.Bpm;
			mod.Trk = mod.Chn * mod.Pat + 1;

			m.C4Rate = Constants.C4_Ntsc_Rate;
			m.Period_Type = (xfh.Flags & Xm_Flags.Linear_Period_Mode) != 0 ? Containers.Common.Period.Linear : Containers.Common.Period.Amiga;

			CMemory.memcpy<uint8>(mod.Xxo, xfh.Order, (size_t)mod.Len);

			string tracker_Name = Encoding.Latin1.GetString(xfh.Tracker).TrimEnd(' ', '\0');

			if (tracker_Name.StartsWith("FastTracker v2.00"))
			{
				m.Quirk |= Quirk_Flag.Ft2Bugs;
				claims_Ft2 = true;
			}
			else if (tracker_Name.StartsWith("Fasttracker II clone"))
				m.Quirk |= Quirk_Flag.Ft2Bugs;
			else if (tracker_Name.StartsWith("OpenMPT "))
			{
				// OpenMPT accurately emulates weird FT2 bugs
				m.Quirk |= Quirk_Flag.Ft2Bugs;
			}

			if (xfh.HeaderSz == 0x0113)
			{
				tracker_Name = "unknown tracker";
				m.Quirk &= ~Quirk_Flag.Ft2Bugs;
			}
			else if (tracker_Name.Length == 0)
			{
				// Best guess
				tracker_Name = "Digitrakker";
				m.Quirk &= ~Quirk_Flag.Ft2Bugs;
			}

			// See MMD1 loader for explanation
			if (tracker_Name.StartsWith("MED2XM by J.Pynnone"))
			{
				if (mod.Bpm <= 10)
					mod.Bpm = 125 * (0x35 - (mod.Bpm * 2)) / 33;

				m.Quirk &= ~Quirk_Flag.Ft2Bugs;
			}

			if (tracker_Name.StartsWith("FastTracker v 2.00"))
			{
				tracker_Name = "old ModPlug Tracker";
				m.Quirk &= ~Quirk_Flag.Ft2Bugs;
				is_Mpt_Old = true;
			}

			if (tracker_Name.StartsWith("Skale Tracker") || tracker_Name.StartsWith("Sk@le Tracker"))
			{
				// Skale Tracker allows Dxx Byy to jump to row X
				m.Flow_Mode |= FlowMode_Flag.Jump_No_Row_Set;
			}

			lib.common.LibXmp_Set_Type(m, string.Format("{0} XM {1}.{2:D2}", tracker_Name, xfh.Version >> 8, xfh.Version & 0xff));

			// Honor header size
			if (f.Hio_Seek((c_long)(start + xfh.HeaderSz + 60), SeekOrigin.Begin) < 0)
				return -1;

			// XM 1.02/1.03 has a different patterns and instruments order
			if (xfh.Version <= 0x0103)
			{
				if (Load_Instruments(m, xfh.Version, out mpt_Ins_Headers, f) < 0)
					return -1;

				if (Load_Patterns(m, xfh.Version, f) < 0)
					return -1;
			}
			else
			{
				if (Load_Patterns(m, xfh.Version, f) < 0)
					return -1;
				
				if (Load_Instruments(m, xfh.Version, out mpt_Ins_Headers, f) < 0)
					return -1;
			}

			// XM 1.02 stores all the samples after the patterns
			if (xfh.Version <= 0x0103)
			{
				for (c_int i = 0; i < mod.Ins; i++)
				{
					for (c_int j = 0; j < mod.Xxi[i].Nsm; j++)
					{
						c_int sid = mod.Xxi[i].Sub[j].Sid;

						if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.Diff, mod.Xxs[sid], null) < 0)
							return -1;
					}
				}
			}

			// Load MPT properties from the end of the file
			uint32 magicText = Common.Magic4('t', 'e', 'x', 't');
			uint32 magicMidi = Common.Magic4('M', 'I', 'D', 'I');
			uint32 magicPnam = Common.Magic4('P', 'N', 'A', 'M');
			uint32 magicCnam = Common.Magic4('C', 'N', 'A', 'M');
			uint32 magicChFx = Common.Magic4('C', 'H', 'F', 'X');
			uint32 magicXtpm = Common.Magic4('X', 'T', 'P', 'M');
			uint32 magicFx = Common.Magic4('F', 'X', '\0', '\0');

			while (true)
			{
				uint32 ext = f.Hio_Read32B();
				uint32 sz = f.Hio_Read32L();
				bool known = false;

				if ((f.Hio_Error() != 0) || (sz > 0x7fffffff))
					break;

				if (ext == magicText)
				{
					known = true;

					if (m.Comment == null)
					{
						if (sz <= f.Hio_Size())
						{
							byte[] c = new byte[sz + 1];
							sz = (uint32)f.Hio_Read(c, 1, sz);

							m.Comment = encoder.GetString(c, 0, (int)sz);

							// Translate linefeeds
							m.Comment = m.Comment.Replace('\u266a', '\n');

							sz = 0;
						}
					}
				}
				else if ((ext == magicMidi) || (ext == magicPnam) || (ext == magicCnam) || (ext == magicChFx) || (ext == magicXtpm))
				{
					known = true;
				}
				else
				{
					if ((ext & magicFx) == magicFx)
						known = true;
				}

				if (known && claims_Ft2)
					is_Mpt_116 = true;

				if ((sz != 0) && (f.Hio_Seek(sz, SeekOrigin.Current) < 0))
					break;

				if (ext == magicXtpm)
					break;
			}

			if (claims_Ft2 && mpt_Ins_Headers)
				is_Mpt_116 = true;

			if (is_Mpt_116)
				lib.common.LibXmp_Set_Type(m, string.Format("ModPlug Tracker 1.16 XM {0}.{1:D2}", xfh.Version >> 8, xfh.Version & 0xff));

			if (is_Mpt_116 || is_Mpt_Old)
			{
				m.Quirk &= ~Quirk_Flag.Ft2Bugs;
				m.Flow_Mode = FlowMode_Flag.Mode_MPT_116;
				m.MVolBase = 48;
				m.MVol = 48;

				lib.common.LibXmp_Apply_Mpt_PreAmp(m);
			}

			for (c_int i = 0; i < mod.Chn; i++)
				mod.Xxc[i].Pan = 0x80;

			m.Quirk |= Quirk_Flag.Ft2 | Quirk_Flag.Ft2Env;
			m.Read_Event_Type = Read_Event.Ft2;

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Load_Xm_Pattern(Module_Data m, c_int num, c_int version, CPointer<uint8> patBuf, Hio f)
		{
			c_int headSize = version > 0x0102 ? 9 : 8;
			Xmp_Module mod = m.Mod;
			Xm_Pattern_Header xph = new Xm_Pattern_Header();

			xph.Length = f.Hio_Read32L();
			xph.Packing = f.Hio_Read8();
			xph.Rows = version > 0x0102 ? f.Hio_Read16L() : (uint16)(f.Hio_Read8() + 1);

			// Sanity check
			if (xph.Rows > 256)
				goto Err;

			xph.DataSize = f.Hio_Read16L();
			f.Hio_Seek((c_long)(xph.Length - headSize), SeekOrigin.Current);

			if (f.Hio_Error() != 0)
				goto Err;

			c_int r = xph.Rows;
			if (r == 0)
				r = 0x100;

			if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, num, r) < 0)
				goto Err;

			if (xph.DataSize == 0)
				return 0;

			c_int size = xph.DataSize;
			CPointer<uint8> pat = patBuf;

			c_int size_Read = (c_int)f.Hio_Read(patBuf, 1, (size_t)size);
			if (size_Read < size)
				CMemory.memset<uint8>(patBuf + size_Read, 0, (size_t)(size - size_Read));

			for (c_int j = 0; j < r; j++)
			{
				for (c_int k = 0; k < mod.Chn; k++)
				{
					// Some XMs have cleanly truncated patterns. See:
					// Balrog/f0rtify.xm; Decayer-9/purification.xm;
					// Falcon (PL)/eaten vinyl.xm; Headcrasher/microcosm.xm;
					// Jazztiz/ta-da-da-da.xm; Jisemdu/smile.xm;
					// Markus Plomgren/cool jazzy jeff!!!.xm;
					// Orange/optical.xm; Skyraver/spirit of life.xm;
					// Sonic (UK)'s atomic_subculture.xm, luvdup.xm,
					// phuture.xm; Teemu/speed.xm; Warhawk/anaconda.xm
					if ((pat - patBuf) == xph.DataSize)
						goto Early_Pattern_End;

					Xmp_Event @event = Ports.LibXmp.Common.Event(m, num, k, j);

					if (--size < 0)
						goto Err;

					uint8 b = pat[0, 1];
					if ((b & Xm_Event_Packing) != 0)
					{
						if ((b & Xm_Event_Note_Follows) != 0)
						{
							if (--size < 0)
								goto Err;

							@event.Note = pat[0, 1];
						}

						if ((b & Xm_Event_Instrument_Follows) != 0)
						{
							if (--size < 0)
								goto Err;

							@event.Ins = pat[0, 1];
						}

						if ((b & Xm_Event_Volume_Follows) != 0)
						{
							if (--size < 0)
								goto Err;

							@event.Vol = pat[0, 1];
						}

						if ((b & Xm_Event_FxType_Follows) != 0)
						{
							if (--size < 0)
								goto Err;

							@event.FxT = pat[0, 1];
						}

						if ((b & Xm_Event_FxParm_Follows) != 0)
						{
							if (--size < 0)
								goto Err;

							@event.FxP = pat[0, 1];
						}
					}
					else
					{
						size -= 4;
						if (size < 0)
							goto Err;

						@event.Note = b;
						@event.Ins = pat[0, 1];
						@event.Vol = pat[0, 1];
						@event.FxT = pat[0, 1];
						@event.FxP = pat[0, 1];
					}

					// Sanity check
					switch (@event.FxT)
					{
						case 18:
						case 19:
						case 22:
						case 23:
						case 24:
						case 26:
						case 28:
						case 30:
						case 31:
						case 32:
						{
							@event.FxT = 0;
							break;
						}
					}

					if (@event.FxT > 34)
						@event.FxT = 0;

					if (@event.Note == 0x61)
						@event.Note = Constants.Xmp_Key_Off;
					else if (@event.Note > 0)
						@event.Note += 12;

					if (@event.FxT == 0x0e)
					{
						if (Ports.LibXmp.Common.Msn(@event.FxP) == Effects.Ex_FineTune)
						{
							byte val = (byte)((Ports.LibXmp.Common.Lsn(@event.FxP) - 8) & 0xf);
							@event.FxP = (byte)((Effects.Ex_FineTune << 4) | val);
						}

						switch (@event.FxP)
						{
							case 0x43:
							case 0x73:
							{
								@event.FxP--;
								break;
							}
						}
					}

					if ((@event.FxT == Effects.Fx_Xf_Porta) && (Ports.LibXmp.Common.Msn(@event.FxP) == 0x09))
					{
						// Translate MPT hacks
						switch (Ports.LibXmp.Common.Lsn(@event.FxP))
						{
							// Surround off
							// Surround on
							case 0x0:
							case 0x1:
							{
								@event.FxT = Effects.Fx_Surround;
								@event.FxP = Ports.LibXmp.Common.Lsn(@event.FxP);
								break;
							}

							// Play forward
							// Play reverse
							case 0xe:
							case 0xf:
							{
								@event.FxT = Effects.Fx_Reverse;
								@event.FxP = (byte)(Ports.LibXmp.Common.Lsn(@event.FxP) - 0xe);
								break;
							}
						}
					}

					if (@event.Vol == 0)
						continue;

					// Volume set
					if ((@event.Vol >= 0x10) && (@event.Vol <= 0x50))
					{
						@event.Vol -= 0x0f;
						continue;
					}

					// Volume column effects
					switch (@event.Vol >> 4)
					{
						// Volume slide down
						case 0x06:
						{
							@event.F2T = Effects.Fx_VolSlide_2;
							@event.F2P = (byte)(@event.Vol - 0x60);
							break;
						}

						// Volume slide up
						case 0x07:
						{
							@event.F2T = Effects.Fx_VolSlide_2;
							@event.F2P = (byte)((@event.Vol - 0x70) << 4);
							break;
						}

						// Fine volume slide down
						case 0x08:
						{
							@event.F2T = Effects.Fx_Extended;
							@event.F2P = (byte)((Effects.Ex_F_VSlide_Dn << 4) | (@event.Vol - 0x80));
							break;
						}

						// Fine volume slide up
						case 0x09:
						{
							@event.F2T = Effects.Fx_Extended;
							@event.F2P = (byte)((Effects.Ex_F_VSlide_Up << 4) | (@event.Vol - 0x90));
							break;
						}

						// Set vibrato speed
						case 0x0a:
						{
							@event.F2T = Effects.Fx_Vibrato;
							@event.F2P = (byte)((@event.Vol - 0xa0) << 4);
							break;
						}

						// Vibrato
						case 0x0b:
						{
							@event.F2T = Effects.Fx_Vibrato;
							@event.F2P = (byte)(@event.Vol - 0xb0);
							break;
						}

						// Set panning
						case 0x0c:
						{
							@event.F2T = Effects.Fx_SetPan;
							@event.F2P = (byte)((@event.Vol - 0xc0) << 4);
							break;
						}

						// Pan slide left
						case 0x0d:
						{
							@event.F2T = Effects.Fx_PanSl_NoMem;
							@event.F2P = (byte)((@event.Vol - 0xd0) << 4);
							break;
						}

						// Pan slide right
						case 0x0e:
						{
							@event.F2T = Effects.Fx_PanSl_NoMem;
							@event.F2P = (byte)(@event.Vol - 0xe0);
							break;
						}

						// Tone portamento
						case 0x0f:
						{
							@event.F2T = Effects.Fx_TonePorta;
							@event.F2P = (byte)((@event.Vol - 0xf0) << 4);
							break;
						}
					}

					@event.Vol = 0;
				}
			}

			Early_Pattern_End:
			return 0;

			Err:
			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Load_Patterns(Module_Data m, c_int version, Hio f)
		{
			Xmp_Module mod = m.Mod;
			c_int i;

			mod.Pat++;

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				return -1;

			CPointer<uint8> patBuf = CMemory.calloc<uint8>(65536);
			if (patBuf.IsNull)
				return -1;

			for (i = 0; i < mod.Pat - 1; i++)
			{
				if (Load_Xm_Pattern(m, i, version, patBuf, f) < 0)
					goto Err;
			}

			// Alloc one extra pattern
			{
				c_int t = i * mod.Chn;

				if (lib.common.LibXmp_Alloc_Pattern(mod, i) < 0)
					goto Err;

				mod.Xxp[i].Rows = 64;

				if (lib.common.LibXmp_Alloc_Track(mod, t, 64) < 0)
					goto Err;

				for (c_int j = 0; j < mod.Chn; j++)
					mod.Xxp[i].Index[j] = t;
			}

			CMemory.free(patBuf);
			return 0;

			Err:
			CMemory.free(patBuf);
			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Is_Ogg_Sample(Hio f, Xmp_Sample xxs)
		{
			// Sample must be at least 4 bytes long to be an OGG sample.
			// Bonnie's Bookstore music.oxm contains zero length samples
			// followed immediately by OGG samples
			if (xxs.Len < 4)
				return false;

			f.Hio_Read32L();	// Size
			uint32 id = f.Hio_Read32B();
			if ((f.Hio_Error() != 0) || (f.Hio_Seek(-8, SeekOrigin.Current) < 0))
				return false;

			if (id != Magic_Oggs)	// Copy input data if not Ogg file
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int OggDec(Module_Data m, Hio f, Xmp_Sample xxs, c_int len)
		{
			CPointer<uint8> data = CMemory.calloc<uint8>((size_t)len);
			if (data.IsNull)
				return -1;

			f.Hio_Read32B();

			if ((f.Hio_Error() != 0) || (f.Hio_Read(data, 1, (size_t)len - 4) != (size_t)(len - 4)))
				return -1;

			CPointer<uint8> pcm;

			c_int n = Vorbis_Decode_Memory(data, len, out c_int ch, out c_int _, out CPointer<int16> pcm16);
			CMemory.free(data);

			if ((n < 0) || (ch != 1))
			{
				CMemory.free(pcm16);
				return -1;
			}

			if (((xxs.Flg & Xmp_Sample_Flag._16Bit) == 0) && (n > 0))
			{
				pcm = new CPointer<uint8>(n);

				for (c_int i = 0; i < n; i++)
					pcm[i] = (uint8)(pcm16[i] >> 8);
			}
			else
			{
				pcm = new CPointer<uint8>(n * 2);
				pcm16.AsSpan().CopyTo(MemoryMarshal.Cast<uint8, int16>(pcm.AsSpan()));
			}

			if ((xxs.Flg & Xmp_Sample_Flag.Stereo) != 0)
			{
				// OXM stereo is a single channel non-interleaved stream
				n >>= 1;
			}

			xxs.Len = n;

			Sample_Flag flags = Sample_Flag.NoLoad;

			if (!BitConverter.IsLittleEndian)
				flags |= Sample_Flag.BigEnd;

			c_int ret = Sample.LibXmp_Load_Sample(m, null, flags, xxs, pcm);
			CMemory.free(pcm16);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Load_Instruments(Module_Data m, c_int version, out bool mpt_Ins_Headers, Hio f)
		{
			Xmp_Module mod = m.Mod;
			Xm_Instrument_Header xih = new Xm_Instrument_Header();
			Xm_Instrument xi = new Xm_Instrument();
			Xm_Sample_Header[] xsh = ArrayHelper.InitializeArray<Xm_Sample_Header>(Xm_Max_Samples_Per_Inst);
			c_int sample_Num = 0;
			CPointer<uint8> buf = new CPointer<uint8>(208);

			mpt_Ins_Headers = false;

			// ESTIMATED value! We don't know the actual value at this point
			mod.Smp = Constants.Max_Samples;

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			for (c_int i = 0; i < mod.Ins; i++)
			{
				c_long instr_Pos = f.Hio_Tell();
				Xmp_Instrument xxi = mod.Xxi[i];

				// Modules converted with MOD2XM 1.0 always say we have 31
				// instruments, but file may end abruptly before that. Also covers
				// XMLiTE stripped modules and truncated files. This test will not
				// work if file has trailing garbage.
				//
				// Note: loading 4 bytes past the instrument header to get the
				// sample header size (if it exists). This is NOT considered to
				// be part of the instrument header
				if (f.Hio_Read(buf, Xm_Inst_Header_Size + 4, 1) != 1)
					break;

				xih.Size = DataIo.ReadMem32L(buf);				// Instrument size
				CMemory.memcpy(xih.Name, buf + 4, 22);// Instrument name
				xih.Type = buf[26];								// Instrument type (always 0)
				xih.Samples = DataIo.ReadMem16L(buf + 27);	// Number of samples
				xih.Sh_Size = DataIo.ReadMem32L(buf + 29);	// Sample header size

				// Sanity check
				if ((c_int)xih.Size < Xm_Inst_Header_Size)
					return -1;

				if ((xih.Samples > Xm_Max_Samples_Per_Inst) || ((xih.Samples > 0) && (xih.Sh_Size > 0x100)))
					return -1;

				// Yet another Modplug Tracker tell: it saves huge zero-filled
				// instrument headers for unused instruments
				if ((xih.Size == 0x107) && (xih.Samples == 0) && (xih.Sh_Size == 0))
					mpt_Ins_Headers = true;

				lib.common.LibXmp_Instrument_Name(mod, i, xih.Name, 22, encoder);

				xxi.Nsm = xih.Samples;

				if (xxi.Nsm == 0)
				{
					// Sample size should be in struct xm_instrument according to
					// the official format description, but FT2 actually puts it in
					// struct xm_instrument header. There's a tracker or converter
					// that follow the specs, so we must handle both cases (see
					// "Braintomb" by Jazztiz/ART).

					// Umm, Cyke O'Path <cyker@heatwave.co.uk> sent me a couple of
					// mods ("Breath of the Wind" and "Broken Dimension") that
					// reserve the instrument data space after the instrument header
					// even if the number of instruments is set to 0. In these modules
					// the instrument header size is marked as 263. The following
					// generalization should take care of both cases
					if (f.Hio_Seek((c_int)xih.Size - (Xm_Inst_Header_Size + 4), SeekOrigin.Current) < 0)
						return -1;

					continue;
				}

				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, xxi.Nsm) < 0)
					return -1;

				// For BoobieSqueezer (see http://boobie.rotfl.at/)
				// It works pretty much the same way as Impulse Tracker's sample
				// only mode, where it will strip off the instrument data
				if (xih.Size < (Xm_Inst_Header_Size + Xm_Inst_Size))
				{
					xi = new Xm_Instrument();
					f.Hio_Seek((c_int)(xih.Size - (Xm_Inst_Header_Size + 4)), SeekOrigin.Current);
				}
				else
				{
					CPointer<uint8> b = buf;

					if (f.Hio_Read(buf, 208, 1) != 1)
						return -1;

					CMemory.memcpy(xi.Sample, b, 96);
					b += 96;

					for (c_int j = 0; j < 24; j++)
					{
						xi.V_Env[j] = DataIo.ReadMem16L(b);	// Points for volume envelope
						b += 2;
					}

					for (c_int j = 0; j < 24; j++)
					{
						xi.P_Env[j] = DataIo.ReadMem16L(b);	// Points for panning envelope
						b += 2;
					}

					xi.V_Pts = b[0, 1];					// Number of volume points
					xi.P_Pts = b[0, 1];					// Number of pan points
					xi.V_Sus = b[0, 1];					// Volume sustain point
					xi.V_Start = b[0, 1];				// Volume loop start point
					xi.V_End = b[0, 1];					// Volume loop end point
					xi.P_Sus = b[0, 1];					// Pan sustain point
					xi.P_Start = b[0, 1];				// Pan loop start point
					xi.P_End = b[0, 1];					// Pan loop end point
					xi.V_Type = (Xm_Envelope_Flag)b[0, 1];
					xi.P_Type = (Xm_Envelope_Flag)b[0, 1];
					xi.Y_Wave = b[0, 1];					// Vibrato waveform
					xi.Y_Sweep = b[0, 1];				// Vibrato sweep
					xi.Y_Depth = b[0, 1];				// Vibrato depth
					xi.Y_Rate = b[0, 1];					// Vibrato rate
					xi.V_Fade = DataIo.ReadMem16L(b);	// Volume fadeout

					// Skip reserved space
					if (f.Hio_Seek((c_int)xih.Size - (Xm_Inst_Header_Size + Xm_Inst_Size), SeekOrigin.Current) < 0)
						return -1;

					// Envelope
					xxi.Rls = xi.V_Fade << 1;
					xxi.Aei.Npt = xi.V_Pts;
					xxi.Aei.Sus = xi.V_Sus;
					xxi.Aei.Lps = xi.V_Start;
					xxi.Aei.Lpe = xi.V_End;
					xxi.Aei.Flg = ConvertEnvelopeFlag(xi.V_Type);
					xxi.Pei.Npt = xi.P_Pts;
					xxi.Pei.Sus = xi.P_Sus;
					xxi.Pei.Lps = xi.P_Start;
					xxi.Pei.Lpe = xi.P_End;
					xxi.Pei.Flg = ConvertEnvelopeFlag(xi.P_Type);

					if ((xxi.Aei.Npt <= 0) || (xxi.Aei.Npt > 12))
						xxi.Aei.Flg &= ~Xmp_Envelope_Flag.On;
					else
						Array.Copy(xi.V_Env, xxi.Aei.Data, xxi.Aei.Npt * 2);

					if ((xxi.Pei.Npt <= 0) || (xxi.Pei.Npt > 12))
						xxi.Pei.Flg &= ~Xmp_Envelope_Flag.On;
					else
						Array.Copy(xi.P_Env, xxi.Pei.Data, xxi.Pei.Npt * 2);

					for (c_int j = 12; j < 108; j++)
					{
						xxi.Map[j].Ins = xi.Sample[j - 12];

						if (xxi.Map[j].Ins >= xxi.Nsm)
							xxi.Map[j].Ins = 0xff;
					}
				}

				// Read subinstrument and sample parameters

				for (c_int j = 0; j < xxi.Nsm; j++, sample_Num++)
				{
					Xmp_SubInstrument sub = xxi.Sub[j];
					CPointer<uint8> b = buf;

					if (sample_Num >= mod.Smp)
					{
						if (lib.common.LibXmp_Realloc_Samples(m, mod.Smp * 3 / 2) < 0)
							return -1;
					}

					Xmp_Sample xxs = mod.Xxs[sample_Num];

					if (f.Hio_Read(buf, 40, 1) != 1)
						return -1;

					xsh[j].Length = DataIo.ReadMem32L(b);		// Sample length
					b += 4;

					// Sanity check
					if (xsh[j].Length > Constants.Max_Sample_Size)
						return -1;

					xsh[j].Loop_Start = DataIo.ReadMem32L(b);	// Sample loop start
					b += 4;
					xsh[j].Loop_Length = DataIo.ReadMem32L(b);	// Sample loop length
					b += 4;

					xsh[j].Volume = b[0, 1];					// Volume
					xsh[j].FineTune = (int8)b[0, 1];			// Finetune (-128..+127)
					xsh[j].Type = (Xm_Sample_Flag)b[0, 1];		// Flags
					xsh[j].Pan = b[0, 1];						// Panning (0-255)
					xsh[j].RelNote = (int8)b[0, 1];				// Relative note number
					xsh[j].Reserved = b[0, 1];
					CMemory.memcpy(xsh[j].Name, b, 22);

					sub.Vol = xsh[j].Volume;
					sub.Pan = xsh[j].Pan;
					sub.Xpo = xsh[j].RelNote;
					sub.Fin = xsh[j].FineTune;
					sub.Vwf = xi.Y_Wave;
					sub.Vde = xi.Y_Depth << 2;
					sub.Vra = xi.Y_Rate;
					sub.Vsw = xi.Y_Sweep;
					sub.Sid = sample_Num;

					lib.common.LibXmp_Copy_Adjust(out xxs.Name, xsh[j].Name, 22, encoder);

					xxs.Len = (c_int)xsh[j].Length;
					xxs.Lps = (c_int)xsh[j].Loop_Start;
					xxs.Lpe = (c_int)(xsh[j].Loop_Start + xsh[j].Loop_Length);

					xxs.Flg = Xmp_Sample_Flag.None;

					if ((xsh[j].Type & Xm_Sample_Flag._16Bit) != 0)
					{
						xxs.Flg |= Xmp_Sample_Flag._16Bit;
						xxs.Len >>= 1;
						xxs.Lps >>= 1;
						xxs.Lpe >>= 1;
					}

					if ((xsh[j].Type & Xm_Sample_Flag.Stereo) != 0)
					{
						xxs.Flg |= Xmp_Sample_Flag.Stereo;
						xxs.Len >>= 1;
						xxs.Lps >>= 1;
						xxs.Lpe >>= 1;
					}

					xxs.Flg |= (xsh[j].Type & Xm_Sample_Flag.Loop_Forward) != 0 ? Xmp_Sample_Flag.Loop : Xmp_Sample_Flag.None;
					xxs.Flg |= (xsh[j].Type & Xm_Sample_Flag.Loop_PingPong) != 0 ? (Xmp_Sample_Flag.Loop | Xmp_Sample_Flag.Loop_BiDir) : Xmp_Sample_Flag.None;
				}

				// Read actual sample data
				c_long total_Sample_Size = 0;

				for (c_int j = 0; j < xxi.Nsm; j++)
				{
					Xmp_SubInstrument sub = xxi.Sub[j];
					Xmp_Sample xxs = mod.Xxs[sub.Sid];

					Sample_Flag flags = Sample_Flag.Diff;

					if (xsh[j].Reserved == 0xad)
						flags = Sample_Flag.Adpcm;

					if (version > 0x0103)
					{
						if (Is_Ogg_Sample(f, xxs))
						{
							if (OggDec(m, f, xxs, (c_int)xsh[j].Length) < 0)
								return -1;

							total_Sample_Size += xsh[j].Length;
							continue;
						}

						if (Sample.LibXmp_Load_Sample(m, f, flags, xxs, null) < 0)
							return -1;

						if ((flags & Sample_Flag.Adpcm) != 0)
							total_Sample_Size += 16 + ((xsh[j].Length + 1) >> 1);
						else
							total_Sample_Size += xsh[j].Length;
					}
				}

				// Reposition correctly in case of 16-bit sample having odd in-file length.
				// See "Lead Lined for '99", reported by Dennis Mulleneers
				if (f.Hio_Seek(instr_Pos + xih.Size + (40 * xih.Samples) + total_Sample_Size, SeekOrigin.Begin) < 0)
					return -1;
			}

			// Final sample number adjustment
			if (lib.common.LibXmp_Realloc_Samples(m, sample_Num) < 0)
				return -1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Convert envelope flag from one type to another
		/// </summary>
		/********************************************************************/
		private Xmp_Envelope_Flag ConvertEnvelopeFlag(Xm_Envelope_Flag flag)
		{
			Xmp_Envelope_Flag newFlag = Xmp_Envelope_Flag.None;

			if ((flag & Xm_Envelope_Flag.On) != 0)
				newFlag |= Xmp_Envelope_Flag.On;

			if ((flag & Xm_Envelope_Flag.Sustain) != 0)
				newFlag |= Xmp_Envelope_Flag.Sus;

			if ((flag & Xm_Envelope_Flag.Loop) != 0)
				newFlag |= Xmp_Envelope_Flag.Loop;

			Debug.Assert(((int)flag & ~7) == 0);

			return newFlag;
		}



		/********************************************************************/
		/// <summary>
		/// Try to figure out, which format the xm module is
		/// </summary>
		/********************************************************************/
		private Format FindFormat(Hio f)
		{
			if (f.Hio_Seek(21, SeekOrigin.Current) < 0)
				return Format.Xm;

			uint16 version = f.Hio_Read16L();
			if (version <= 0x0103)
				return Format.Xm;

			uint32 headerSize = f.Hio_Read32L();

			if (f.Hio_Seek(6, SeekOrigin.Current) < 0)
				return Format.Xm;

			uint16 patternCount = f.Hio_Read16L();
			uint16 instrumentCount = f.Hio_Read16L();

			if (f.Hio_Seek((c_long)headerSize - 14, SeekOrigin.Current) < 0)
				return Format.Xm;

			// Skip patterns
			for (c_int i = 0; i < patternCount; i++)
			{
				headerSize = f.Hio_Read32L();

				if (f.Hio_Seek(3, SeekOrigin.Current) < 0)
					return Format.Xm;

				uint16 patternSize = f.Hio_Read16L();

				if (f.Hio_Seek((c_long)headerSize - 9 + patternSize, SeekOrigin.Current) < 0)
					return Format.Xm;
			}

			// Find sample data and check it
			for (c_int i = 0; i < instrumentCount; i++)
			{
				if ((f.Hio_Size() - f.Hio_Tell()) < Xm_Inst_Header_Size)
					break;

				headerSize = f.Hio_Read32L();

				if (f.Hio_Seek(23, SeekOrigin.Current) < 0)
					return Format.Xm;

				uint16 sampleCount = f.Hio_Read16L();

				if (f.Hio_Seek((c_long)headerSize - 29, SeekOrigin.Current) < 0)
					return Format.Xm;

				if (sampleCount > 0)
				{
					uint32[] sampleLengths = new uint32[sampleCount];

					for (c_int j = 0; j < sampleCount; j++)
					{
						sampleLengths[j] = f.Hio_Read32L();

						if (f.Hio_Seek(13, SeekOrigin.Current) < 0)
							return Format.Xm;

						if (f.Hio_Read8() == 0xad)
							sampleLengths[j] = 16 + ((sampleLengths[j] + 1) >> 1);

						if (f.Hio_Seek(22, SeekOrigin.Current) < 0)
							return Format.Xm;
					}

					for (c_int j = 0; j < sampleCount; j++)
					{
						if (sampleLengths[j] != 0)
						{
							if (f.Hio_Seek(4, SeekOrigin.Current) < 0)
								return Format.Xm;

							uint32 id = f.Hio_Read32B();

							if (f.Hio_Error() != 0)
								return Format.Xm;

							if (id == Magic_Oggs)
								return Format.OggMod;

							if (f.Hio_Seek((c_long)sampleLengths[j] - 8, SeekOrigin.Current) < 0)
								return Format.Xm;
						}
					}
				}
			}

			return Format.Xm;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Vorbis_Decode_Memory(CPointer<uint8> data, c_int len, out c_int ch, out c_int rate, out CPointer<int16> decodedBuffer)
		{
			VorbisError result = VorbisFile.Ov_Open(new ReadOnlyMemoryStream(data.AsMemory(len)), false, out VorbisFile vorbisFile, null, 0);
			if (result != VorbisError.Ok)
			{
				ch = 0;
				rate = 0;
				decodedBuffer = null;

				return -1;
			}

			long todo = vorbisFile.Ov_Pcm_Total(-1);

			VorbisInfo info = vorbisFile.Ov_Info(-1);
			ch = info.channels;
			rate = (c_int)info.rate;

			if (ch != 1)
			{
				decodedBuffer = null;

				return -1;
			}

			decodedBuffer = CMemory.calloc<int16>((size_t)todo);
			if (decodedBuffer.IsNull)
				return -1;

			int offset = 0;
			int total = 0;

			while (todo > 0)
			{
				int done = (c_int)vorbisFile.Ov_Read_Float(out CPointer<c_float>[] buffer, (c_int)todo, out _);
				if (done == (int)VorbisError.Hole)
					continue;

				if (done <= 0)
					break;

				// Copy the samples into one buffer
				for (int i = 0; i < done; i++)
					decodedBuffer[offset++] = (short)Math.Clamp(buffer[0][i] * 32767, -32768, 32767);

				todo -= done;
				total += done;
			}

			vorbisFile.Ov_Clear();

			return total;
		}



		/********************************************************************/
		/// <summary>
		/// TNE: Tell if the module was made with ModPlug Tracker or OpenMPT.
		/// Those are played by the OpenMPT player instead, so -1 is returned
		/// for them. This is the mirror image of ExtendedProbeXm() in the
		/// LibOpenMpt port and the two have to agree, or a module ends up
		/// being rejected by both players
		/// </summary>
		/********************************************************************/
		private c_int Test_Extended(Hio f, c_int start)
		{
			if (LibXmp.UnitTestMode)
				return 0;

			CPointer<uint8> songName = new CPointer<uint8>(20);
			CPointer<uint8> trackerName = new CPointer<uint8>(20);

			if (f.Hio_Seek(start + 17, SeekOrigin.Begin) < 0)
				return 0;

			if (f.Hio_Read(songName, 1, 20) < 20)
				return 0;

			if (f.Hio_Seek(start + 38, SeekOrigin.Begin) < 0)
				return 0;

			if (f.Hio_Read(trackerName, 1, 20) < 20)
				return 0;

			uint16 version = f.Hio_Read16L();
			uint32 headerSize = f.Hio_Read32L();
			uint16 orders = f.Hio_Read16L();
			uint16 restartPos = f.Hio_Read16L();
			uint16 channels = f.Hio_Read16L();
			uint16 patterns = f.Hio_Read16L();
			uint16 instruments = f.Hio_Read16L();
			uint16 flags = f.Hio_Read16L();

			if (f.Hio_Error() != 0)
				return 0;

			// The OpenMPT player runs these checks before anything else and
			// leaves the module alone when they do not hold
			if ((channels == 0) || (channels > 192))
				return 0;

			if (f.Hio_Size() < (start + 80 + orders + (4 * (patterns + instruments))))
				return 0;

			bool isOpenMptName = CMemory.memcmp(trackerName, "OpenMPT ", 8) == 0;

			// ModPlugin is the web browser plugin which later became
			// ModPlug Tracker, and it is the only thing ever writing this
			// name. Only some of the modules hold ADPCM packed samples, so
			// the name has to be taken as a signature on its own
			bool isModPlugName = isOpenMptName || (CMemory.memcmp(trackerName, "MOD Plugin packed", 17) == 0);

			// FastTracker 2 pads the song title with spaces, while some
			// other trackers terminate it with a null character
			c_int firstNull = FindNullCharacter(songName, 20);

			Xm_Tracker_Version madeWith;

			if ((CMemory.memcmp(trackerName, "FastTracker v2.00   ", 20) == 0) && (headerSize == 276))
			{
				if (version < 0x0104)
					madeWith = Xm_Tracker_Version.Ft2Generic | Xm_Tracker_Version.Confirmed;
				else if (firstNull >= 0)
				{
					// PlayerPRO fills the rest of the buffer after the null
					// terminator with spaces and does not support a song
					// restart position
					if (restartPos != 0)
						madeWith = Xm_Tracker_Version.Ft2Clone | Xm_Tracker_Version.NewModPlug;
					else if (firstNull == 19)
						madeWith = Xm_Tracker_Version.Ft2Clone | Xm_Tracker_Version.NewModPlug | Xm_Tracker_Version.PlayerPro;
					else if (IsOnlySpaces(songName + (firstNull + 1), 20 - (firstNull + 1)))
						madeWith = Xm_Tracker_Version.PlayerPro | Xm_Tracker_Version.Confirmed;
					else
						madeWith = Xm_Tracker_Version.Ft2Clone | Xm_Tracker_Version.NewModPlug;
				}
				else
				{
					if (restartPos != 0)
						madeWith = Xm_Tracker_Version.Ft2Generic | Xm_Tracker_Version.NewModPlug;
					else
						madeWith = Xm_Tracker_Version.Ft2Generic | Xm_Tracker_Version.NewModPlug | Xm_Tracker_Version.PlayerPro;
				}
			}
			else if (CMemory.memcmp(trackerName, "FastTracker v 2.00  ", 20) == 0)
			{
				// ModPlug Tracker 1.0, the exact version is found below
				madeWith = Xm_Tracker_Version.OldModPlug;
			}
			else
			{
				// Something else. Only the trackers telling something about
				// ModPlug Tracker / OpenMPT are checked here
				madeWith = Xm_Tracker_Version.Unknown | Xm_Tracker_Version.Confirmed;

				if (isOpenMptName)
					madeWith = Xm_Tracker_Version.OpenMpt | Xm_Tracker_Version.Confirmed;
				else if (CMemory.memcmp(trackerName, "Fasttracker II clone", 20) == 0)
				{
					// 8bitbubsy's FastTracker 2 clone
					madeWith = Xm_Tracker_Version.Ft2Generic | Xm_Tracker_Version.Confirmed;
				}
				else if ((CMemory.memcmp(trackerName, "*Converted ", 11) == 0) && (CMemory.memcmp(trackerName + 14, "-File*", 6) == 0))
					madeWith = Xm_Tracker_Version.DigiTrakker | Xm_Tracker_Version.Confirmed;
			}

			// The extended filter range is only ever written by ModPlug
			// Tracker / OpenMPT, and this loader does not support it at all
			if ((flags & Xm_Extended_Filter_Range) != 0)
				return -1;

			// Skip the order list and jump to the pattern data. A file
			// which is cut short is not left alone right away, because the
			// tracker name may already have told enough
			bool truncated = f.Hio_Seek(start + headerSize + 60, SeekOrigin.Begin) < 0;

			if (!truncated && (version >= 0x0104))
				SkipPatterns(f, version, patterns);

			bool anyAdpcm = false;
			bool oggResolved = false;
			uint8 sampleReserved = 0;
			c_int lastInstrType = -1;
			c_int lastSampleReserved = -1;
			int64 lastSampleHeaderSize = -1;
			bool instrumentWithSamplesEncountered = false;
			c_long totalSampleBytes = 0;
			c_long fileSize = f.Hio_Size();

			c_int numInstruments = Math.Min((c_int)instruments, 255);

			for (c_int instr = 1; !truncated && (instr <= numInstruments); instr++)
			{
				c_long curPos = f.Hio_Tell();

				if ((fileSize - curPos) < 4)
					break;

				// The stored size tells how much of the instrument header
				// is really present in the file. Everything behind it is
				// taken as zero, just like the partial structure read done
				// by the OpenMPT loader
				uint32 storedSize = f.Hio_Read32L();
				uint32 instrHeaderSize = storedSize == 0 ? Xm_Instrument_Header_Size : storedSize;
				uint32 presentSize = Math.Min(instrHeaderSize, Xm_Instrument_Header_Size);

				uint8 instrType = 0;
				uint16 numSamples = 0;
				uint32 sampleHeaderSize = 0;

				if ((presentSize >= 33) && (f.Hio_Seek(curPos + 26, SeekOrigin.Begin) >= 0))
				{
					instrType = f.Hio_Read8();
					numSamples = f.Hio_Read16L();
					sampleHeaderSize = f.Hio_Read32L();
				}

				uint8 volLoopStart = 0, volLoopEnd = 0, volFlags = 0;
				uint8 panLoopStart = 0, panLoopEnd = 0, panFlags = 0;
				uint8 midiEnabled = 0, midiChannel = 0, muteComputer = 0;
				uint16 midiProgram = 0;

				if ((presentSize >= 248) && (f.Hio_Seek(curPos + 228, SeekOrigin.Begin) >= 0))
				{
					volLoopStart = f.Hio_Read8();
					volLoopEnd = f.Hio_Read8();
					f.Hio_Seek(1, SeekOrigin.Current);		// Panning sustain point
					panLoopStart = f.Hio_Read8();
					panLoopEnd = f.Hio_Read8();
					volFlags = f.Hio_Read8();
					panFlags = f.Hio_Read8();
					f.Hio_Seek(6, SeekOrigin.Current);		// Auto vibrato and fade out
					midiEnabled = f.Hio_Read8();
					midiChannel = f.Hio_Read8();
					midiProgram = f.Hio_Read16L();
					f.Hio_Seek(2, SeekOrigin.Current);		// Pitch wheel range
					muteComputer = f.Hio_Read8();
				}

				if ((curPos + instrHeaderSize) > fileSize)
				{
					truncated = true;
					break;
				}

				f.Hio_Seek(curPos + instrHeaderSize, SeekOrigin.Begin);

				// Time for some version detection stuff
				if (madeWith == Xm_Tracker_Version.OldModPlug)
				{
					// ModPlug Tracker 1.0 alpha stores 245 and beta 263
					if ((storedSize == 245) || (storedSize == 263))
						madeWith |= Xm_Tracker_Version.Confirmed;
					else
						madeWith = Xm_Tracker_Version.Unknown | Xm_Tracker_Version.Confirmed;
				}
				else if (numSamples == 0)
				{
					// Empty instruments make tracker identification pretty easy
					if ((storedSize == 263) && (sampleHeaderSize == 0) && ((madeWith & Xm_Tracker_Version.NewModPlug) != 0))
						madeWith |= Xm_Tracker_Version.Confirmed;
					else if ((storedSize != 29) && ((madeWith & Xm_Tracker_Version.DigiTrakker) != 0))
						madeWith &= ~Xm_Tracker_Version.DigiTrakker;
					else if (((madeWith & (Xm_Tracker_Version.Ft2Clone | Xm_Tracker_Version.Ft2Generic)) != 0) && (storedSize != 33))
					{
						// Sure is not FastTracker 2
						madeWith = Xm_Tracker_Version.Unknown;
					}

					if (storedSize != 33)
						madeWith &= ~Xm_Tracker_Version.PlayerPro;
					else if ((sampleHeaderSize > Xm_Sample_Header_Size) && ((madeWith & Xm_Tracker_Version.PlayerPro) != 0))
					{
						// Older PlayerPRO versions write garbage into the
						// sample header size field, and it is different
						// for each sample
						if (instrumentWithSamplesEncountered || ((lastSampleHeaderSize != -1) && (sampleHeaderSize != lastSampleHeaderSize)))
							madeWith = Xm_Tracker_Version.PlayerPro | Xm_Tracker_Version.Confirmed;

						lastSampleHeaderSize = sampleHeaderSize;
					}
				}

				if (lastInstrType == -1)
					lastInstrType = instrType;
				else if ((lastInstrType != instrType) && ((madeWith & Xm_Tracker_Version.Ft2Generic) != 0))
				{
					// FastTracker 2 writes some random junk into the
					// instrument type field, but it is always the same
					// junk for every instrument saved
					madeWith &= ~Xm_Tracker_Version.Ft2Generic;
					madeWith |= Xm_Tracker_Version.Ft2Clone;
				}

				if (numSamples > 0)
				{
					instrumentWithSamplesEncountered = true;

					// If MIDI settings are present, this is definitely not
					// an old ModPlug Tracker or PlayerPRO
					if ((midiEnabled | midiChannel | midiProgram | muteComputer) != 0)
						madeWith &= ~(Xm_Tracker_Version.OldModPlug | Xm_Tracker_Version.NewModPlug | Xm_Tracker_Version.PlayerPro);

					if ((storedSize != 263) || (instrType != 0))
						madeWith &= ~Xm_Tracker_Version.PlayerPro;

					if (((madeWith & Xm_Tracker_Version.Confirmed) == 0) && ((madeWith & Xm_Tracker_Version.PlayerPro) != 0))
					{
						// Earlier PlayerPRO versions do not seem to set the loop points to 0xff
						if ((((volFlags & Xm_Envelope_Loop) == 0) && (volLoopStart == 0xff) && (volLoopEnd == 0xff)) ||
						    (((panFlags & Xm_Envelope_Loop) == 0) && (panLoopStart == 0xff) && (panLoopEnd == 0xff)))
						{
							madeWith |= Xm_Tracker_Version.Confirmed;
							madeWith &= ~Xm_Tracker_Version.NewModPlug;
						}
					}

					// Read the sample headers
					uint32[] sampleSizes = new uint32[numSamples];
					bool[] adpcmSamples = new bool[numSamples];

					CPointer<uint8> sampleName = new CPointer<uint8>(22);

					for (c_int smp = 0; smp < numSamples; smp++)
					{
						if ((fileSize - f.Hio_Tell()) < Xm_Sample_Header_Size)
						{
							truncated = true;
							break;
						}

						uint32 length = f.Hio_Read32L();
						f.Hio_Seek(9, SeekOrigin.Current);		// Loop points and volume
						int8 fineTune = (int8)f.Hio_Read8();
						uint8 sampleFlags = f.Hio_Read8();
						uint8 pan = f.Hio_Read8();
						f.Hio_Seek(1, SeekOrigin.Current);		// Relative tone
						uint8 reserved = f.Hio_Read8();
						f.Hio_Read(sampleName, 1, 22);

						sampleReserved |= reserved;

						if ((reserved != 0) && (reserved != Xm_Sample_Adpcm))
							madeWith &= ~(Xm_Tracker_Version.OldModPlug | Xm_Tracker_Version.NewModPlug | Xm_Tracker_Version.OpenMpt);

						if (lastSampleReserved == -1)
							lastSampleReserved = reserved;
						else if (lastSampleReserved != reserved)
							madeWith &= ~Xm_Tracker_Version.PlayerPro;

						if (pan != 128)
							madeWith &= ~Xm_Tracker_Version.PlayerPro;

						if (((fineTune & 0x0f) != 0) && (fineTune != 127))
							madeWith &= ~Xm_Tracker_Version.PlayerPro;

						// FastTracker 2 stores the sample name length here.
						// It just copies the whole Pascal string, and that
						// string might have ended with spaces even before
						// being space padded in the file, so an exact
						// length comparison cannot be made
						if (((madeWith & (Xm_Tracker_Version.Ft2Generic | Xm_Tracker_Version.Ft2Clone)) != 0) &&
						    ((madeWith & (Xm_Tracker_Version.NewModPlug | Xm_Tracker_Version.PlayerPro)) != 0) &&
						    ((madeWith & Xm_Tracker_Version.Confirmed) == 0) &&
						    ((reserved > 22) || !IsOnlySpaces(sampleName + reserved, 22 - reserved)))
						{
							madeWith &= ~Xm_Tracker_Version.Ft2Generic;
							madeWith |= Xm_Tracker_Version.Ft2Clone | Xm_Tracker_Version.Confirmed;
						}

						bool isAdpcm = (reserved == Xm_Sample_Adpcm) && ((sampleFlags & 0x30) == 0);
						if (isAdpcm)
							anyAdpcm = true;

						sampleSizes[smp] = length;
						adpcmSamples[smp] = isAdpcm;
					}

					if (truncated)
						break;

					// Read the sample data
					for (c_int smp = 0; smp < numSamples; smp++)
					{
						c_long chunkSize = adpcmSamples[smp] ? 16 + ((sampleSizes[smp] + 1) / 2) : sampleSizes[smp];

						if (version < 0x0104)
						{
							// Version 1.02 and 1.03 store the sample data after the patterns instead
							totalSampleBytes += chunkSize;
							continue;
						}

						c_long chunkPos = f.Hio_Tell();

						// Only the first sample holding any data is checked
						// for Ogg Vorbis. OggMod encodes every single
						// sample, so one look is enough to tell an .oxm
						// apart. OggMod stores the length of the decoded
						// sample in front of the stream, so the magic sits
						// 4 bytes in
						if (!oggResolved && (chunkSize >= 8))
						{
							oggResolved = true;

							f.Hio_Seek(4, SeekOrigin.Current);

							if (f.Hio_Read32B() == Magic_Oggs)
								return 0;
						}

						if ((chunkPos + chunkSize) > fileSize)
						{
							truncated = true;
							break;
						}

						f.Hio_Seek(chunkPos + chunkSize, SeekOrigin.Begin);
					}

					if (truncated)
						break;
				}

				// Only when it is known that OggMod has not been at work,
				// the module may be left to the OpenMPT player
				if (oggResolved && IsMadeWithModPlug(isModPlugName, madeWith, anyAdpcm))
					return -1;
			}

			if (!truncated && (version < 0x0104))
			{
				// Patterns and sample data are stored after the
				// instruments in version 1.02 and 1.03
				SkipPatterns(f, version, patterns);

				f.Hio_Seek(totalSampleBytes, SeekOrigin.Current);
			}

			// A null terminated song name is quite possibly ModPlug
			// Tracker. It could really be a ModPlug made file which has
			// been resaved in FastTracker 2, though
			if ((sampleReserved == 0) && ((madeWith & Xm_Tracker_Version.NewModPlug) != 0) && (firstNull >= 0))
				madeWith |= Xm_Tracker_Version.Confirmed;

			// All the sample data has been passed by now, so it is known
			// whether OggMod has been at work or not
			if (IsMadeWithModPlug(isModPlugName, madeWith, anyAdpcm))
				return -1;

			// The song extensions holding the song comments, the MIDI
			// configuration, the pattern names and the channel names are
			// only ever written by ModPlug Tracker / OpenMPT, so a single
			// one of them settles it no matter what the tracker detection
			// above has come up with
			if (ReadMagic(f, Magic_Text) ||		// Song comments
			    ReadMagic(f, Magic_Midi) ||		// MIDI configuration
			    ReadMagic(f, Magic_Pnam) ||		// Pattern names
			    ReadMagic(f, Magic_Cnam))		// Channel names
			{
				return -1;
			}

			// Mix plugins. This loader has no support for those at all, so
			// it does not matter which tracker wrote them
			if (((fileSize - f.Hio_Tell()) >= 8) && SkipMixPlugins(f, fileSize))
				return -1;

			// Extended instrument and song properties are only written by
			// OpenMPT 1.17 and later, so finding either of them is enough
			if ((numInstruments > 0) && ReadMagic(f, Magic_Xtpm))
				return -1;

			if (ReadMagic(f, Magic_Stpm))
				return -1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Tell if the collected flags point at a module made with ModPlug
		/// Tracker or OpenMPT
		/// </summary>
		/********************************************************************/
		private bool IsMadeWithModPlug(bool isModPlugName, Xm_Tracker_Version madeWith, bool anyAdpcm)
		{
			// OpenMPT 1.17 and later and ModPlugin both write their own
			// name into the header, and only ModPlug ever packed samples
			// with its own ADPCM compression
			if (isModPlugName || anyAdpcm)
				return true;

			if ((madeWith & Xm_Tracker_Version.Confirmed) == 0)
				return false;

			// ModPlug Tracker 1.0 alpha / beta
			if ((madeWith & Xm_Tracker_Version.OldModPlug) != 0)
				return true;

			// ModPlug Tracker 1.0 - 1.16. PlayerPRO writes files which
			// look almost the same, so those are left alone
			return ((madeWith & Xm_Tracker_Version.NewModPlug) != 0) && ((madeWith & Xm_Tracker_Version.PlayerPro) == 0);
		}



		/********************************************************************/
		/// <summary>
		/// Skip over all the patterns without unpacking them
		/// </summary>
		/********************************************************************/
		private void SkipPatterns(Hio f, uint16 version, uint16 patterns)
		{
			for (c_int pat = 0; pat < patterns; pat++)
			{
				c_long curPos = f.Hio_Tell();

				uint32 patHeaderSize = f.Hio_Read32L();
				if ((patHeaderSize < 8) || ((curPos + patHeaderSize) > f.Hio_Size()))
					break;

				f.Hio_Seek(1, SeekOrigin.Current);		// Pack method (= 0)

				// Number of rows
				f.Hio_Seek(version == 0x0102 ? 1 : 2, SeekOrigin.Current);

				// A packed size of 0 indicates a completely empty pattern
				uint16 packedSize = f.Hio_Read16L();

				if (f.Hio_Seek(curPos + (c_long)patHeaderSize + packedSize, SeekOrigin.Begin) < 0)
					break;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Skip over all the mix plugin chunks without parsing them. Tells
		/// whether any real mix plugin chunk was found
		/// </summary>
		/********************************************************************/
		private bool SkipMixPlugins(Hio f, c_long fileSize)
		{
			bool hasPluginChunks = false;

			while ((fileSize - f.Hio_Tell()) >= 9)
			{
				c_long curPos = f.Hio_Tell();

				uint32 code = f.Hio_Read32B();
				uint32 chunkSize = f.Hio_Read32L();

				if ((code == Magic_Impi) ||		// IT instrument, we definitely read too far
				    (code == Magic_Imps) ||		// IT sample, ditto
				    (code == Magic_Xtpm) ||		// Instrument extensions, ditto
				    (code == Magic_Stpm) ||		// Song extensions, ditto
				    ((f.Hio_Tell() + chunkSize) > fileSize))
				{
					f.Hio_Seek(curPos, SeekOrigin.Begin);
					break;
				}

				if ((code == Magic_Chfx) || IsPluginChunk(code))
					hasPluginChunks = true;

				f.Hio_Seek(chunkSize, SeekOrigin.Current);
			}

			return hasPluginChunks;
		}



		/********************************************************************/
		/// <summary>
		/// Tell if the given chunk identifier is one of the FXnn chunks
		/// holding the settings of a single mix plugin
		/// </summary>
		/********************************************************************/
		private bool IsPluginChunk(uint32 code)
		{
			uint8 c2 = (uint8)(code >> 8);
			uint8 c3 = (uint8)code;

			return ((code >> 16) == 0x4658) && (c2 >= 0x30) && (c2 <= 0x39) && (c3 >= 0x30) && (c3 <= 0x39);
		}



		/********************************************************************/
		/// <summary>
		/// Read the given magic. The file position is only moved when the
		/// magic matches
		/// </summary>
		/********************************************************************/
		private bool ReadMagic(Hio f, uint32 magic)
		{
			c_long curPos = f.Hio_Tell();

			uint32 id = f.Hio_Read32B();

			if ((f.Hio_Error() != 0) || (id != magic))
			{
				f.Hio_Seek(curPos, SeekOrigin.Begin);
				return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return the index of the first null character or -1 if the string
		/// does not hold any
		/// </summary>
		/********************************************************************/
		private c_int FindNullCharacter(CPointer<uint8> str, c_int length)
		{
			for (c_int i = 0; i < length; i++)
			{
				if (str[i] == 0x00)
					return i;
			}

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// Tell if the given string only holds space characters
		/// </summary>
		/********************************************************************/
		private bool IsOnlySpaces(CPointer<uint8> str, c_int length)
		{
			for (c_int i = 0; i < length; i++)
			{
				if (str[i] != 0x20)
					return false;
			}

			return true;
		}
		#endregion
	}
}
