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
using NVorbis;
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
			/// <summary>
			/// ID text: "Extended module: "
			/// </summary>
			public uint8[] Id = new uint8[17];

			/// <summary>
			/// Module name, padded with zeros
			/// </summary>
			public uint8[] Name = new uint8[20];

			/// <summary>
			/// 0x1a
			/// </summary>
			public uint8 DosEof;

			/// <summary>
			/// Tracker name
			/// </summary>
			public uint8[] Tracker = new uint8[20];

			/// <summary>
			/// Version number, minor-major
			/// </summary>
			public uint16 Version;

			/// <summary>
			/// Header size
			/// </summary>
			public uint32 HeaderSz;

			/// <summary>
			/// Song length (in pattern order table)
			/// </summary>
			public uint16 SongLen;

			/// <summary>
			/// Restart position
			/// </summary>
			public uint16 Restart;

			/// <summary>
			/// Number of channels (2,4,6,8,10,...,32)
			/// </summary>
			public uint16 Channels;

			/// <summary>
			/// Number of patterns (max 256)
			/// </summary>
			public uint16 Patterns;

			/// <summary>
			/// Number of instruments (max 128)
			/// </summary>
			public uint16 Instruments;

			/// <summary>
			/// Bit 0: 0=Amiga freq table, 1=Linear
			/// </summary>
			public Xm_Flags Flags;

			/// <summary>
			/// Default tempo
			/// </summary>
			public uint16 Tempo;

			/// <summary>
			/// Default BPM
			/// </summary>
			public uint16 Bpm;

			/// <summary>
			/// Pattern order table
			/// </summary>
			public uint8[] Order = new uint8[256];
		}
		#endregion

		#region Xm_Pattern_Header
		private class Xm_Pattern_Header
		{
			/// <summary>
			/// Pattern header length
			/// </summary>
			public uint32 Length;

			/// <summary>
			/// Packing type (always 0)
			/// </summary>
			public uint8 Packing;

			/// <summary>
			/// Number of rows in pattern (1..256)
			/// </summary>
			public uint16 Rows;

			/// <summary>
			/// Packed patterndata size
			/// </summary>
			public uint16 DataSize;
		}
		#endregion

		#region Xm_Instrument_Header
		private class Xm_Instrument_Header
		{
			/// <summary>
			/// Instrument size
			/// </summary>
			public uint32 Size;

			/// <summary>
			/// Instrument name
			/// </summary>
			public uint8[] Name = new uint8[22];

			/// <summary>
			/// Instrument type (always 0)
			/// </summary>
			public uint8 Type;

			/// <summary>
			/// Number of samples in instrument
			/// </summary>
			public uint16 Samples;

			/// <summary>
			/// Sample header size
			/// </summary>
			public uint32 Sh_Size;
		}
		#endregion

		#region Xm_Instrument
		private class Xm_Instrument
		{
			/// <summary>
			/// Sample number for all notes
			/// </summary>
			public uint8[] Sample = new uint8[96];

			/// <summary>
			/// Points for volume envelope
			/// </summary>
			public uint16[] V_Env = new uint16[24];

			/// <summary>
			/// Points for panning envelope
			/// </summary>
			public uint16[] P_Env = new uint16[24];

			/// <summary>
			/// Number of volume points
			/// </summary>
			public uint8 V_Pts;

			/// <summary>
			/// Number of panning points
			/// </summary>
			public uint8 P_Pts;

			/// <summary>
			/// Volume sustain point
			/// </summary>
			public uint8 V_Sus;

			/// <summary>
			/// Volume loop start point
			/// </summary>
			public uint8 V_Start;

			/// <summary>
			/// Volume loop end point
			/// </summary>
			public uint8 V_End;

			/// <summary>
			/// Panning sustain point
			/// </summary>
			public uint8 P_Sus;

			/// <summary>
			/// Panning loop start point
			/// </summary>
			public uint8 P_Start;

			/// <summary>
			/// Panning loop end point
			/// </summary>
			public uint8 P_End;

			/// <summary>
			/// Bit 0: On; 1: Sustain; 2: Loop
			/// </summary>
			public Xm_Envelope_Flag V_Type;

			/// <summary>
			/// Bit 0: On; 1: Sustain; 2: Loop
			/// </summary>
			public Xm_Envelope_Flag P_Type;

			/// <summary>
			/// Vibrato waveform
			/// </summary>
			public uint8 Y_Wave;

			/// <summary>
			/// Vibrato sweep
			/// </summary>
			public uint8 Y_Sweep;

			/// <summary>
			/// Vibrato depth
			/// </summary>
			public uint8 Y_Depth;

			/// <summary>
			/// Vibrato rate
			/// </summary>
			public uint8 Y_Rate;

			/// <summary>
			/// Volume fadeout
			/// </summary>
			public uint16 V_Fade;
		}
		#endregion

		#region Xm_Sample_Header
		private class Xm_Sample_Header
		{
			/// <summary>
			/// Sample length
			/// </summary>
			public uint32 Length;

			/// <summary>
			/// Sample loop start
			/// </summary>
			public uint32 Loop_Start;

			/// <summary>
			/// Sample loop length
			/// </summary>
			public uint32 Loop_Length;

			/// <summary>
			/// Volume
			/// </summary>
			public uint8 Volume;

			/// <summary>
			/// Finetune (signed byte -128..+127)
			/// </summary>
			public int8 FineTune;

			/// <summary>
			/// Flags
			/// </summary>
			public Xm_Sample_Flag Type;

			/// <summary>
			/// Panning (0-255)
			/// </summary>
			public uint8 Pan;

			/// <summary>
			/// Relative note number (signed byte)
			/// </summary>
			public int8 RelNote;

			/// <summary>
			/// Reserved
			/// </summary>
			public uint8 Reserved;

			/// <summary>
			/// Sample name
			/// </summary>
			public uint8[] Name = new uint8[22];
		}
		#endregion

#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
		#endregion

		private enum Format
		{
			Xm,
			OggMod
		}

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

			byte[] buf = new byte[20];

			if (f.Hio_Read(buf, 1, 17) < 17)		// ID text
				return -1;

			if (Encoding.ASCII.GetString(buf, 0, 17) != "Extended Module: ")
				return -1;

			lib.common.LibXmp_Read_Title(f, out t, 20, encoder);

			return FindFormat(f) == format ? 0 : -1;
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
			bool is_Mpt_116 = false;
			byte[] buf = new byte[80];

			if (f.Hio_Read(buf, 80, 1) != 1)
				return -1;

			Array.Copy(buf, xfh.Id, 17);	// ID text
			Array.Copy(buf, 17, xfh.Name, 0, 20);	// Module name

			// Skip 0x1a

			Array.Copy(buf, 38, xfh.Tracker, 0, 20);	// Tracker name
			xfh.Version = DataIo.ReadMem16L(buf, 58);		// Version number, minor-major
			xfh.HeaderSz = DataIo.ReadMem32L(buf, 60);	// Header size
			xfh.SongLen = DataIo.ReadMem16L(buf, 64);		// Song length
			xfh.Restart = DataIo.ReadMem16L(buf, 66);		// Restart position
			xfh.Channels = DataIo.ReadMem16L(buf, 68);	// Number of channels
			xfh.Patterns = DataIo.ReadMem16L(buf, 70);	// Number of patterns
			xfh.Instruments = DataIo.ReadMem16L(buf, 72);	// Number of instruments
			xfh.Flags = (Xm_Flags)DataIo.ReadMem16L(buf, 74);// 0=Amiga freq table, 1=Linear
			xfh.Tempo = DataIo.ReadMem16L(buf, 76);		// Default tempo
			xfh.Bpm = DataIo.ReadMem16L(buf, 78);			// Default BPM

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
				if (Encoding.ASCII.GetString(xfh.Tracker, 0, 6) != "MED2XM")
					return -1;
			}

			// Honor header size -- needed by BoobieSqueezer XMs
			c_int len = (c_int)xfh.HeaderSz - 0x14;

			if ((len < 0) || (len > 256))
				return -1;

			Array.Clear(xfh.Order);

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

			Array.Copy(xfh.Order, mod.Xxo, mod.Len);

			string tracker_Name = Encoding.ASCII.GetString(xfh.Tracker).TrimEnd(' ', '\0');

			// OpenMPT accurately emulates weird FT2 bugs
			if (tracker_Name.StartsWith("FastTracker v2.00"))
			{
				m.Quirk |= Quirk_Flag.Ft2Bugs;
				claims_Ft2 = true;
			}
			else if (tracker_Name.StartsWith("OpenMPT "))
				m.Quirk |= Quirk_Flag.Ft2Bugs;

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
					mod.Bpm = 125 * (0x35 - mod.Bpm * 2) / 33;

				m.Quirk &= ~Quirk_Flag.Ft2Bugs;
			}

			if (tracker_Name.StartsWith("FastTracker v 2.00"))
			{
				tracker_Name = "old ModPlug Tracker";
				m.Quirk &= ~Quirk_Flag.Ft2Bugs;
				is_Mpt_116 = true;
			}

			lib.common.LibXmp_Set_Type(m, string.Format("{0} XM {1}.{2:D2}", tracker_Name, xfh.Version >> 8, xfh.Version & 0xff));

			// Honor header size
			if (f.Hio_Seek((c_long)(start + xfh.HeaderSz + 60), SeekOrigin.Begin) < 0)
				return -1;

			// XM 1.02/1.03 has a different patterns and instruments order
			if (xfh.Version <= 0x0103)
			{
				if (Load_Instruments(m, xfh.Version, f) < 0)
					return -1;

				if (Load_Patterns(m, xfh.Version, f) < 0)
					return -1;
			}
			else
			{
				if (Load_Patterns(m, xfh.Version, f) < 0)
					return -1;
				
				if (Load_Instruments(m, xfh.Version, f) < 0)
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

						if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.Diff, mod.Xxs[sid], null, sid) < 0)
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
						byte[] c = new byte[sz + 1];
						sz = (uint32)f.Hio_Read(c, 1, sz);

						m.Comment = encoder.GetString(c, 0, (int)sz);

						// Translate linefeeds
						m.Comment = m.Comment.Replace('\u266a', '\n');
					}
				}
				else if ((ext == magicMidi) || (ext == magicPnam) || (ext == magicCnam) || (ext == magicChFx) || (ext == magicXtpm))
				{
					known = true;

					if (sz != 0)
						f.Hio_Seek((c_long)sz, SeekOrigin.Current);
				}
				else
				{
					if ((ext & magicFx) == magicFx)
						known = true;

					if (sz != 0)
						f.Hio_Seek((c_long)sz, SeekOrigin.Current);
				}

				if (known && claims_Ft2)
					is_Mpt_116 = true;

				if (ext == magicXtpm)
					break;
			}

			if (is_Mpt_116)
			{
				lib.common.LibXmp_Set_Type(m, string.Format("ModPlug Tracker 1.16 XM {0}.{1:D2}", xfh.Version >> 8, xfh.Version & 0xff));

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
		private c_int Load_Xm_Pattern(Module_Data m, c_int num, c_int version, uint8[] patBuf, Hio f)
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
			int pat = 0;

			c_int size_Read = (c_int)f.Hio_Read(patBuf, 1, (size_t)size);
			if (size_Read < size)
				Array.Clear(patBuf, size_Read, size - size_Read);

			for (c_int j = 0; j < r; j++)
			{
				for (c_int k = 0; k < mod.Chn; k++)
				{
					Xmp_Event @event = Ports.LibXmp.Common.Event(m, num, k, j);

					if (--size < 0)
						goto Err;

					uint8 b = patBuf[pat++];
					if ((b & Xm_Event_Packing) != 0)
					{
						if ((b & Xm_Event_Note_Follows) != 0)
						{
							if (--size < 0)
								goto Err;

							@event.Note = patBuf[pat++];
						}

						if ((b & Xm_Event_Instrument_Follows) != 0)
						{
							if (--size < 0)
								goto Err;

							@event.Ins = patBuf[pat++];
						}

						if ((b & Xm_Event_Volume_Follows) != 0)
						{
							if (--size < 0)
								goto Err;

							@event.Vol = patBuf[pat++];
						}

						if ((b & Xm_Event_FxType_Follows) != 0)
						{
							if (--size < 0)
								goto Err;

							@event.FxT = patBuf[pat++];
						}

						if ((b & Xm_Event_FxParm_Follows) != 0)
						{
							if (--size < 0)
								goto Err;

							@event.FxP = patBuf[pat++];
						}
					}
					else
					{
						size -= 4;
						if (size < 0)
							goto Err;

						@event.Note = b;
						@event.Ins = patBuf[pat++];
						@event.Vol = patBuf[pat++];
						@event.FxT = patBuf[pat++];
						@event.FxP = patBuf[pat++];
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
					{
						// See OpenMPT keyoff+instr.xm test case
						if ((@event.FxT == 0x0e) && (Ports.LibXmp.Common.Msn(@event.FxP) == 0x0d))
							@event.Note = Constants.Xmp_Key_Off;
						else
							@event.Note = @event.Ins != 0 ? Constants.Xmp_Key_Fade : Constants.Xmp_Key_Off;
					}
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

							// From OpenMPT TonePortamentoMemory.xm:
							// "Another nice bug (...) is the combination of both
							// portamento commands (Mx and 3xx) in the same cell:
							// The 3xx parameter is ignored completely, and the Mx
							// parameter is doubled. (M2 3FF is the same as M4 000)
							if ((@event.FxT ==Effects.Fx_TonePorta) || (@event.FxT == Effects.Fx_Tone_VSlide))
							{
								if (@event.FxT == Effects.Fx_TonePorta)
									@event.FxT = 0;
								else
									@event.FxT = Effects.Fx_VolSlide;

								@event.FxP = 0;

								if (@event.F2P < 0x80)
									@event.F2P <<= 1;
								else
									@event.F2P = 0xff;
							}

							// From OpenMPT porta-offset.xm:
							// "If there is a portamento command next to an offset
							// command, the offset command is ignored completely. In
							// particular, the offset parameter is not memorized."
							if ((@event.FxT == Effects.Fx_Offset) && (@event.F2T == Effects.Fx_TonePorta))
								@event.FxT = @event.FxP = 0;

							break;
						}
					}

					@event.Vol = 0;
				}
			}

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

			uint8[] patBuf = new uint8[65536];
			if (patBuf == null)
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

			return 0;

			Err:
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
			uint8[] data = new uint8[len];
			if (data == null)
				return -1;

			f.Hio_Read32B();

			if ((f.Hio_Error() != 0) || (f.Hio_Read(data, 1, (size_t)len - 4) != (size_t)(len - 4)))
				return -1;

			c_int n;
			float[] decodedBuffer;
			uint8[] pcm;

			try
			{
				using (VorbisReader vorbis = new VorbisReader(new MemoryStream(data)))
				{
					decodedBuffer = new float[vorbis.TotalSamples];

					n = vorbis.ReadSamples(decodedBuffer);
					if ((n != vorbis.TotalSamples) || (vorbis.Channels != 1))
						return -1;
				}
			}
			catch (Exception)
			{
				return -1;
			}

			if ((xxs.Flg & Xmp_Sample_Flag._16Bit) == 0)
			{
				pcm = new uint8[n];
				Span<int8> pcm8 = MemoryMarshal.Cast<uint8, int8>(pcm);

				for (c_int i = 0; i < n; i++)
					pcm8[i] = (int8)((int16)(decodedBuffer[i] * 32767.0f) >> 8);
			}
			else
			{
				pcm = new uint8[n * 2];
				Span<int16> pcm16 = MemoryMarshal.Cast<uint8, int16>(pcm);

				for (c_int i = 0; i < n; i++)
					pcm16[i] = (int16)(decodedBuffer[i] * 32767.0f);
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

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Load_Instruments(Module_Data m, c_int version, Hio f)
		{
			Xmp_Module mod = m.Mod;
			Xm_Instrument_Header xih = new Xm_Instrument_Header();
			Xm_Instrument xi = new Xm_Instrument();
			Xm_Sample_Header[] xsh = ArrayHelper.InitializeArray<Xm_Sample_Header>(Xm_Max_Samples_Per_Inst);
			c_int sample_Num = 0;
			uint8[] buf = new uint8[208];

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

				xih.Size = DataIo.ReadMem32L(buf, 0);		// Instrument size
				Array.Copy(buf, 4, xih.Name, 0, 22);		// Instrument name
				xih.Type = buf[26];								// Instrument type (always 0)
				xih.Samples = DataIo.ReadMem16L(buf, 27);	// Number of samples
				xih.Sh_Size = DataIo.ReadMem32L(buf, 29);	// Sample header size

				// Sanity check
				if ((c_int)xih.Size < Xm_Inst_Header_Size)
					return -1;

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
					int b = 0;

					if (f.Hio_Read(buf, 208, 1) != 1)
						return -1;

					Array.Copy(buf, b, xi.Sample, 0, 96);
					b += 96;

					for (c_int j = 0; j < 24; j++)
					{
						xi.V_Env[j] = DataIo.ReadMem16L(buf, b);	// Points for volume envelope
						b += 2;
					}

					for (c_int j = 0; j < 24; j++)
					{
						xi.P_Env[j] = DataIo.ReadMem16L(buf, b);	// Points for panning envelope
						b += 2;
					}

					xi.V_Pts = buf[b++];					// Number of volume points
					xi.P_Pts = buf[b++];					// Number of pan points
					xi.V_Sus = buf[b++];					// Volume sustain point
					xi.V_Start = buf[b++];					// Volume loop start point
					xi.V_End = buf[b++];					// Volume loop end point
					xi.P_Sus = buf[b++];					// Pan sustain point
					xi.P_Start = buf[b++];					// Pan loop start point
					xi.P_End = buf[b++];					// Pan loop end point
					xi.V_Type = (Xm_Envelope_Flag)buf[b++];
					xi.P_Type = (Xm_Envelope_Flag)buf[b++];
					xi.Y_Wave = buf[b++];					// Vibrato waveform
					xi.Y_Sweep = buf[b++];					// Vibrato sweep
					xi.Y_Depth = buf[b++];					// Vibrato depth
					xi.Y_Rate = buf[b++];					// Vibrato rate
					xi.V_Fade = DataIo.ReadMem16L(buf, b);	// Volume fadeout

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
					int b = 0;

					if (sample_Num >= mod.Smp)
						return -1;

					Xmp_Sample xxs = mod.Xxs[sample_Num];

					if (f.Hio_Read(buf, 40, 1) != 1)
						return -1;

					xsh[j].Length = DataIo.ReadMem32L(buf, b);		// Sample length
					b += 4;

					// Sanity check
					if (xsh[j].Length > Constants.Max_Sample_Size)
						return -1;

					xsh[j].Loop_Start = DataIo.ReadMem32L(buf, b);	// Sample loop start
					b += 4;
					xsh[j].Loop_Length = DataIo.ReadMem32L(buf, b);	// Sample loop length
					b += 4;

					xsh[j].Volume = buf[b++];						// Volume
					xsh[j].FineTune = (int8)buf[b++];				// Finetune (-128..+127)
					xsh[j].Type = (Xm_Sample_Flag)buf[b++];			// Flags
					xsh[j].Pan = buf[b++];							// Panning (0-255)
					xsh[j].RelNote = (int8)buf[b++];				// Relative note number
					xsh[j].Reserved = buf[b++];
					Array.Copy(buf, b, xsh[j].Name, 0, 22);

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

							total_Sample_Size += (c_long)xsh[j].Length;
							continue;
						}

						if (Sample.LibXmp_Load_Sample(m, f, flags, xxs, null, sub.Sid) < 0)
							return -1;

						if ((flags & Sample_Flag.Adpcm) != 0)
							total_Sample_Size += (c_long)(16 + ((xsh[j].Length + 1) >> 1));
						else
							total_Sample_Size += (c_long)xsh[j].Length;
					}
				}

				// Reposition correctly in case of 16-bit sample having odd in-file length.
				// See "Lead Lined for '99", reported by Dennis Mulleneers
				if (f.Hio_Seek((c_long)(instr_Pos + xih.Size + 40 * xih.Samples + total_Sample_Size), SeekOrigin.Begin) < 0)
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
		#endregion
	}
}
