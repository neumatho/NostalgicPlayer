/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
	internal class It_Load : IFormatLoader
	{
		#region Internal structures
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

		#region IT flags
		[Flags]
		private enum It_Flag : uint16
		{
			Stereo = 0x01,
			Vol_Opt = 0x02,		// Not recognized
			Use_Inst = 0x04,
			Linear_Freq = 0x08,
			Old_Fx = 0x10,
			Link_Gxx = 0x20,
			Midi_Wheel = 0x40,
			Midi_Config = 0x80
		}
		#endregion

		#region IT special
		[Flags]
		private enum It_Special : uint16
		{
			None = 0,
			Has_Msg = 0x01,
			Edit_History = 0x02,
			Highlights = 0x04,
			Spec_MidiCfg = 0x08
		}
		#endregion

		#region IT instrument flags
		[Flags]
		private enum It_Inst_Flag : uint8
		{
			Sample = 0x01,
			_16Bit = 0x02,
			Stereo = 0x04,
			Loop = 0x10,
			SLoop = 0x20,
			BLoop = 0x40,
			BSLoop = 0x80
		}
		#endregion

		#region IT sample flags
		[Flags]
		private enum It_Smp_Flag : uint8
		{
			Sample = 0x01,
			_16Bit = 0x02,
			Stereo = 0x04,
			Comp = 0x08,
			Loop = 0x10,
			SLoop = 0x20,
			BLoop = 0x40,
			BSLoop = 0x80
		}
		#endregion

		#region IT sample conversion flags
		[Flags]
		private enum It_Cvt_Flag : uint8
		{
			Signed = 0x01,
			BigEnd = 0x02,		// 'safe to ignore' according to ittech.txt
			Diff = 0x04,		// Compressed sample flag
			ByteDiff = 0x08,	// 'safe to ignore' according to ittech.txt
			_12Bit = 0x10,		// 'safe to ignore' according to ittech.txt
			Adpcm = 0xff		// Special: always indicates Modplug ADPCM4
		}
		#endregion

		#region IT envelope flags
		[Flags]
		private enum It_Env_Flag : uint8
		{
			On = 0x01,
			Loop = 0x02,
			SLoop = 0x04,
			Carry = 0x08,
			Filter = 0x80
		}
		#endregion

		#region It_File_Header
		private class It_File_Header
		{
			/// <summary>
			/// 'IMPM'
			/// </summary>
			public uint32 Magic;

			/// <summary>
			/// ASCIIZ song name
			/// </summary>
			public uint8[] Name = new uint8[26];

			/// <summary>
			/// Pattern editor highlight
			/// </summary>
			public uint8 Hilite_Min;

			/// <summary>
			/// Pattern editor highlight
			/// </summary>
			public uint8 Hilite_Maj;

			/// <summary>
			/// Number of orders (must be even)
			/// </summary>
			public uint16 OrdNum;

			/// <summary>
			/// Number of instruments
			/// </summary>
			public uint16 InsNum;

			/// <summary>
			/// Number of samples
			/// </summary>
			public uint16 SmpNum;

			/// <summary>
			/// Number of patterns
			/// </summary>
			public uint16 PatNum;

			/// <summary>
			/// Tracker ID and version
			/// </summary>
			public uint16 Cwt;

			/// <summary>
			/// Format version
			/// </summary>
			public uint16 Cmwt;

			/// <summary>
			/// Flags
			/// </summary>
			public It_Flag Flags;

			/// <summary>
			/// More flags
			/// </summary>
			public It_Special Special;

			/// <summary>
			/// Global volume
			/// </summary>
			public uint8 Gv;

			/// <summary>
			/// Master volume
			/// </summary>
			public uint8 Mv;

			/// <summary>
			/// Initial speed
			/// </summary>
			public uint8 Is;

			/// <summary>
			/// Initial tempo
			/// </summary>
			public uint8 It;

			/// <summary>
			/// Panning separation
			/// </summary>
			public uint8 Sep;

			/// <summary>
			/// Pitch wheel depth
			/// </summary>
			public uint8 Pwd;

			/// <summary>
			/// Message length
			/// </summary>
			public uint16 MsgLen;

			/// <summary>
			/// Message offset
			/// </summary>
			public uint32 MsgOfs;

			/// <summary>
			/// Reserved
			/// </summary>
			public uint32 Rsvd;

			/// <summary>
			/// Channel pan settings
			/// </summary>
			public uint8[] ChPan = new uint8[64];

			/// <summary>
			/// Channel volume settings
			/// </summary>
			public uint8[] ChVol = new uint8[64];
		}
		#endregion

		#region It_Instrument1_Header
		private class It_Instrument1_Header
		{
			/// <summary>
			/// 'IMPI'
			/// </summary>
			public uint32 Magic;

			/// <summary>
			/// DOS file name
			/// </summary>
			public uint8[] DosName = new uint8[12];

			/// <summary>
			/// Always zero
			/// </summary>
			public uint8 Zero;

			/// <summary>
			/// Instrument flags
			/// </summary>
			public It_Env_Flag Flags;

			/// <summary>
			/// Volume loop start
			/// </summary>
			public uint8 Vls;

			/// <summary>
			/// Volume loop end
			/// </summary>
			public uint8 Vle;

			/// <summary>
			/// Sustain loop start
			/// </summary>
			public uint8 Sls;

			/// <summary>
			/// Sustain loop end
			/// </summary>
			public uint8 Sle;

			/// <summary>
			/// Reserved
			/// </summary>
			public uint16 Rsvd1;

			/// <summary>
			/// Fadeout (release)
			/// </summary>
			public uint16 FadeOut;

			/// <summary>
			/// New note action
			/// </summary>
			public uint8 Nna;

			/// <summary>
			/// Duplicate note check
			/// </summary>
			public uint8 Dnc;

			/// <summary>
			/// Tracker version
			/// </summary>
			public uint16 TrkVers;

			/// <summary>
			/// Number of samples
			/// </summary>
			public uint8 Nos;

			/// <summary>
			/// Reserved
			/// </summary>
			public uint8 Rsvd2;

			/// <summary>
			/// ASCIIZ instrument name
			/// </summary>
			public uint8[] Name = new uint8[26];

			/// <summary>
			/// Reserved
			/// </summary>
			public uint8[] Rsvd3 = new uint8[6];

			public uint8[] Keys = new uint8[240];
			public uint8[] EPoint = new uint8[200];
			public uint8[] ENode = new uint8[50];
		}
		#endregion

		#region It_Instrument2_Header
		private class It_Instrument2_Header
		{
			/// <summary>
			/// 'IMPI'
			/// </summary>
			public uint32 Magic;

			/// <summary>
			/// DOS file name
			/// </summary>
			public uint8[] DosName = new uint8[12];

			/// <summary>
			/// Always zero
			/// </summary>
			public uint8 Zero;

			/// <summary>
			/// New note action
			/// </summary>
			public uint8 Nna;

			/// <summary>
			/// Duplicate check type
			/// </summary>
			public uint8 Dct;

			/// <summary>
			/// Duplicate check action
			/// </summary>
			public uint8 Dca;

			/// <summary>
			/// 
			/// </summary>
			public uint16 FadeOut;

			/// <summary>
			/// Pitch-pan separation
			/// </summary>
			public uint8 Pps;

			/// <summary>
			/// Pitch-pan center
			/// </summary>
			public uint8 Ppc;

			/// <summary>
			/// Global volume
			/// </summary>
			public uint8 Gbv;

			/// <summary>
			/// Default pan
			/// </summary>
			public uint8 Dfp;

			/// <summary>
			/// Random volume variation
			/// </summary>
			public uint8 Rv;

			/// <summary>
			/// Random pan variation
			/// </summary>
			public uint8 Rp;

			/// <summary>
			/// Not used: Tracked version
			/// </summary>
			public uint16 TrkVers;

			/// <summary>
			/// Not used: Number of samples
			/// </summary>
			public uint8 Nos;

			/// <summary>
			/// Reserved
			/// </summary>
			public uint8 Rsvd1;

			/// <summary>
			/// ASCIIZ instrument name
			/// </summary>
			public uint8[] Name = new uint8[26];

			/// <summary>
			/// Initial filter cutoff
			/// </summary>
			public uint8 Ifc;

			/// <summary>
			/// Initial filter resonance
			/// </summary>
			public uint8 Ifr;

			/// <summary>
			/// MIDI channel
			/// </summary>
			public uint8 Mch;

			/// <summary>
			/// MIDI program
			/// </summary>
			public uint8 Mpr;

			/// <summary>
			/// MIDI bank
			/// </summary>
			public uint16 Mbnk;

			/// <summary>
			/// 
			/// </summary>
			public uint8[] Keys = new uint8[240];
		}
		#endregion

		#region It_Envelope_Node
		private class It_Envelope_Node
		{
			public int8 Y;
			public uint16 X;
		}
		#endregion

		#region It_Envelope
		private class It_Envelope
		{
			/// <summary>
			/// Flags
			/// </summary>
			public It_Env_Flag Flg;

			/// <summary>
			/// Number of node points
			/// </summary>
			public uint8 Num;

			/// <summary>
			/// Loop beginning
			/// </summary>
			public uint8 Lpb;

			/// <summary>
			/// Loop end
			/// </summary>
			public uint8 Lpe;

			/// <summary>
			/// Sustain loop beginning
			/// </summary>
			public uint8 Slb;

			/// <summary>
			/// Sustain loop end
			/// </summary>
			public uint8 Sle;

			public It_Envelope_Node[] Node = ArrayHelper.InitializeArray<It_Envelope_Node>(25);
			public uint8 Unused;
		}
		#endregion

		#region It_Sample_Header
		private class It_Sample_Header
		{
			/// <summary>
			/// 'IMPS'
			/// </summary>
			public uint32 Magic;

			/// <summary>
			/// DOS file name
			/// </summary>
			public uint8[] DosName = new uint8[12];

			/// <summary>
			/// Always zero
			/// </summary>
			public uint8 Zero;

			/// <summary>
			/// Global volume for instrument
			/// </summary>
			public uint8 Gvl;

			/// <summary>
			/// Sample flags
			/// </summary>
			public It_Smp_Flag Flags;

			/// <summary>
			/// Volume
			/// </summary>
			public uint8 Vol;

			/// <summary>
			/// ASCIIZ instrument name
			/// </summary>
			public uint8[] Name = new uint8[26];

			/// <summary>
			/// Sample flags
			/// </summary>
			public It_Cvt_Flag Convert;

			/// <summary>
			/// Default pan
			/// </summary>
			public uint8 Dfp;

			/// <summary>
			/// Length
			/// </summary>
			public uint32 Length;

			/// <summary>
			/// Loop begin
			/// </summary>
			public uint32 LoopBeg;

			/// <summary>
			/// Loop end
			/// </summary>
			public uint32 LoopEnd;

			/// <summary>
			/// C 5 speed
			/// </summary>
			public uint32 C5Spd;

			/// <summary>
			/// SusLoop begin
			/// </summary>
			public uint32 SLoopBeg;

			/// <summary>
			/// SusLoop end
			/// </summary>
			public uint32 SLoopEnd;

			/// <summary>
			/// Sample pointer
			/// </summary>
			public uint32 Sample_Ptr;

			/// <summary>
			/// Vibrato speed
			/// </summary>
			public uint8 Vis;

			/// <summary>
			/// Vibrato depth
			/// </summary>
			public uint8 Vid;

			/// <summary>
			/// Vibrato rate
			/// </summary>
			public uint8 Vir;

			/// <summary>
			/// Vibrato waveform
			/// </summary>
			public uint8 Vit;
		}
		#endregion

#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
		#endregion

		private static readonly uint32 Magic_IMPM = Common.Magic4('I', 'M', 'P', 'M');
		private static readonly uint32 Magic_IMPI = Common.Magic4('I', 'M', 'P', 'I');
		private static readonly uint32 Magic_IMPS = Common.Magic4('I', 'M', 'P', 'S');

		private const c_int Temp_Buffer_Len = 65536;

		private const uint8 Fx_None = 0xff;
		private const uint8 Fx_Xtnd = 0xfe;
		private const c_int L_Channels = 64;

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_It = new Format_Loader
		{
			Id = Guid.Parse("6D40CCCF-45AE-4F5C-BF6B-2ABCD31B80AC"),
			Name = "Impulse Tracker",
			Description = "This loader recognizes “Impulse Tracker” modules, currently the most powerful format. These modules support up to 64 real channels, and up to 256 virtual channels with the “New Note Action” feature. Besides, it has the widest range of effects, and supports 16 bit samples as well as surround sound.\n\n“Impulse Tracker” was written by Jeffrey Lim and released in 1996.",
			Create = Create
		};

		private static readonly uint8[] fx =
		[
			Fx_None,
			Effects.Fx_S3M_Speed,		// A
			Effects.Fx_Jump,			// B
			Effects.Fx_It_Break,		// C
			Effects.Fx_VolSlide,		// D
			Effects.Fx_Porta_Dn,		// E
			Effects.Fx_Porta_Up,		// F
			Effects.Fx_TonePorta,		// G
			Effects.Fx_Vibrato,			// H
			Effects.Fx_Tremor,			// I
			Effects.Fx_S3M_Arpeggio,	// J
			Effects.Fx_Vibra_VSlide,	// K
			Effects.Fx_Tone_VSlide,		// L
			Effects.Fx_Trk_Vol,			// M
			Effects.Fx_Trk_VSlide,		// N
			Effects.Fx_Offset,			// O
			Effects.Fx_It_PanSlide,		// P
			Effects.Fx_Multi_Retrig,	// Q
			Effects.Fx_Tremolo,			// R
			Fx_Xtnd,					// S
			Effects.Fx_It_Bpm,			// T
			Effects.Fx_Fine_Vibrato,	// U
			Effects.Fx_GlobalVol,		// V
			Effects.Fx_GVol_Slide,		// W
			Effects.Fx_SetPan,			// X
			Effects.Fx_Panbrello,		// Y
			Effects.Fx_Macro,			// Z
			Fx_None,					// ?
			Effects.Fx_MacroSmooth,		// /
			Fx_None,					// ?
			Fx_None,					// ?
			Fx_None						// ?
		];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private It_Load(LibXmp libXmp)
		{
			lib = libXmp;
			encoder = EncoderCollection.Dos;
		}




		/********************************************************************/
		/// <summary>
		/// Create a new instance of the loader
		/// </summary>
		/********************************************************************/
		public static IFormatLoader Create(LibXmp libXmp, Xmp_Context ctx)
		{
			return new It_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			if (f.Hio_Read32B() != Magic_IMPM)
				return -1;

			lib.common.LibXmp_Read_Title(f, out t, 26, encoder);

			// OpenMPT modules has a lot of extra extensions, which is not supported.
			// So if we can detect it is such a module, we won't play it
			if (IsOpenMpt(f))
				return -1;

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
			It_File_Header ifh = new It_File_Header();
			CPointer<uint32> pp_Ins;		// Pointers to instruments
			CPointer<uint32> pp_Smp;		// Pointers to samples
			CPointer<uint32> pp_Pat;		// Pointers to patterns
			CPointer<uint8> patBuf = null;
			bool pat_Before_Smp = false;
			bool is_Mpt_116 = false;

			// Load and convert header
			ifh.Magic = f.Hio_Read32B();
			if (ifh.Magic != Magic_IMPM)
				return -1;

			f.Hio_Read(ifh.Name, 26, 1);
			ifh.Hilite_Min = f.Hio_Read8();
			ifh.Hilite_Maj = f.Hio_Read8();

			ifh.OrdNum = f.Hio_Read16L();
			ifh.InsNum = f.Hio_Read16L();
			ifh.SmpNum = f.Hio_Read16L();
			ifh.PatNum = f.Hio_Read16L();

			ifh.Cwt = f.Hio_Read16L();
			ifh.Cmwt = f.Hio_Read16L();
			ifh.Flags = (It_Flag)f.Hio_Read16L();
			ifh.Special = (It_Special)f.Hio_Read16L();

			ifh.Gv = f.Hio_Read8();
			ifh.Mv = f.Hio_Read8();
			ifh.Is = f.Hio_Read8();
			ifh.It = f.Hio_Read8();
			ifh.Sep = f.Hio_Read8();
			ifh.Pwd = f.Hio_Read8();

			// Sanity check
			if (ifh.Gv > 0x80)
				goto Err;

			ifh.MsgLen = f.Hio_Read16L();
			ifh.MsgOfs = f.Hio_Read32L();
			ifh.Rsvd = f.Hio_Read32L();

			f.Hio_Read(ifh.ChPan, 64, 1);
			f.Hio_Read(ifh.ChVol, 64, 1);

			if (f.Hio_Error() != 0)
				goto Err;

			mod.Name = encoder.GetString(ifh.Name);
			mod.Len = ifh.OrdNum;
			mod.Ins = ifh.InsNum;
			mod.Smp = ifh.SmpNum;
			mod.Pat = ifh.PatNum;

			// Sanity check
			if ((mod.Ins > 255) || (mod.Smp > 255) || (mod.Pat > 255))
				goto Err;

			if (mod.Ins != 0)
			{
				pp_Ins = CMemory.CAlloc<uint32>(mod.Ins);
				if (pp_Ins.IsNull)
					goto Err;
			}
			else
				pp_Ins = null;

			pp_Smp = CMemory.CAlloc<uint32>(mod.Smp);
			if (pp_Smp.IsNull)
				goto Err2;

			pp_Pat = CMemory.CAlloc<uint32>(mod.Pat);
			if (pp_Pat.IsNull)
				goto Err3;

			mod.Spd = ifh.Is;
			mod.Bpm = ifh.It;

			bool sample_Mode = (~ifh.Flags & It_Flag.Use_Inst) != 0;

			if ((ifh.Flags & It_Flag.Linear_Freq) != 0)
				m.Period_Type = Containers.Common.Period.Linear;

			for (c_int i = 0; i < 64; i++)
			{
				Xmp_Channel xxc = mod.Xxc[i];

				if (ifh.ChPan[i] == 100)		// Surround -> center
					xxc.Flg |= Xmp_Channel_Flag.Surround;

				if ((ifh.ChPan[i] & 0x80) != 0)	// Channel mute
					xxc.Flg |= Xmp_Channel_Flag.Mute;

				if ((ifh.Flags & It_Flag.Stereo) != 0)
				{
					xxc.Pan = ifh.ChPan[i] * 0x80 >> 5;
					if (xxc.Pan > 0xff)
						xxc.Pan = 0xff;
				}
				else
					xxc.Pan = 0x80;

				xxc.Vol = ifh.ChVol[i];
			}

			if (mod.Len <= Constants.Xmp_Max_Mod_Length)
				f.Hio_Read(mod.Xxo, 1, (size_t)mod.Len);
			else
			{
				f.Hio_Read(mod.Xxo, 1, Constants.Xmp_Max_Mod_Length);
				f.Hio_Seek(mod.Len - Constants.Xmp_Max_Mod_Length, SeekOrigin.Current);
				mod.Len = Constants.Xmp_Max_Mod_Length;
			}

			bool new_Fx = (ifh.Flags & It_Flag.Old_Fx) == 0;

			for (c_int i = 0; i < mod.Ins; i++)
				pp_Ins[i] = f.Hio_Read32L();

			for (c_int i = 0; i < mod.Smp; i++)
				pp_Smp[i] = f.Hio_Read32L();

			for (c_int i = 0; i < mod.Pat; i++)
				pp_Pat[i] = f.Hio_Read32L();

			// Skip edit history if it exists
			if ((ifh.Special & It_Special.Edit_History) != 0)
			{
				c_int skip = f.Hio_Read16L() * 8;
				if ((f.Hio_Error() != 0) || ((skip != 0) && (f.Hio_Seek(skip, SeekOrigin.Current) < 0)))
					goto Err4;
			}

			if (((ifh.Flags & It_Flag.Midi_Config) != 0) || ((ifh.Special & It_Special.Spec_MidiCfg) != 0))
			{
				if (Load_It_Midi_Config(m, f) < 0)
					goto Err4;
			}

			m.DspEffects = CheckForDspEffects(pp_Ins, pp_Smp, pp_Pat, ifh, f);

			if ((mod.Smp != 0) && (mod.Pat != 0) && (pp_Pat[0] != 0) && (pp_Pat[0] < pp_Smp[0]))
				pat_Before_Smp = true;

			m.C4Rate = Constants.C4_Ntsc_Rate;

			Identify_Tracker(m, ifh, pat_Before_Smp, ref is_Mpt_116);

			if (sample_Mode)
				mod.Ins = mod.Smp;

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				goto Err4;

			for (c_int i = 0; i < mod.Ins; i++)
			{
				// IT files can have three different instrument types: 'New'
				// instruments, 'old' instruments or just samples. We need a
				// different loader for each of them
				Xmp_Instrument xxi = mod.Xxi[i];

				if (!sample_Mode && (ifh.Cmwt >= 0x200))
				{
					// New instrument format
					if (f.Hio_Seek((c_long)(start + pp_Ins[i]), SeekOrigin.Begin) < 0)
						goto Err4;

					if (Load_New_It_Instrument(xxi, f) < 0)
						goto Err4;
				}
				else if (!sample_Mode)
				{
					// Old instrument format
					if (f.Hio_Seek((c_long)(start + pp_Ins[i]), SeekOrigin.Begin) < 0)
						goto Err4;

					if (Load_Old_It_Instrument(xxi, f) < 0)
						goto Err4;
				}
			}

			// This buffer should be able to hold any pattern or sample block.
			// Round up to a multiple of 4--the sample decompressor relies on
			// this to simplify its code
			patBuf = CMemory.MAlloc<uint8>(Temp_Buffer_Len);
			if (patBuf.IsNull)
				goto Err4;

			for (c_int i = 0; i < mod.Smp; i++)
			{
				if (f.Hio_Seek((c_long)(start + pp_Smp[i]), SeekOrigin.Begin) < 0)
					goto Err4;

				if (Load_It_Sample(m, i, start, sample_Mode, patBuf, f) < 0)
					goto Err4;
			}

			// Reset any error status set by truncated samples
			f.Hio_Error();

			// Effects in muted channels are processed, so scan patterns first to
			// see the real number of channels
			c_int max_Ch = 0;

			for (c_int i = 0; i < mod.Pat; i++)
			{
				uint8[] mask = new uint8[L_Channels];

				// If the offset to a pattern is 0, the pattern is empty
				if (pp_Pat[i] == 0)
					continue;

				f.Hio_Seek((c_long)(start + pp_Pat[i]), SeekOrigin.Begin);
				c_int pat_Len = f.Hio_Read16L();
				c_int num_Rows = f.Hio_Read16L();
				f.Hio_Read16L();
				f.Hio_Read16L();

				// Sanity check:
				// - Impulse Tracker and Schism Tracker allow up to 200 rows
				// - ModPlug Tracker 1.16 allows 256 rows
				// - OpenMPT allows 1024 rows
				if (num_Rows > 1024)
				{
					pp_Pat[i] = 0;
					continue;
				}

				if (f.Hio_Read(patBuf, 1, (size_t)pat_Len) < (size_t)pat_Len)
					goto Err4;

				CPointer<uint8> pos = patBuf;

				c_int row = 0;

				while ((row < num_Rows) && (--pat_Len >= 0))
				{
					c_int b = pos[0, 1];
					if (f.Hio_Error() != 0)
						goto Err4;

					if (b == 0)
					{
						row++;
						continue;
					}

					c_int c = (b - 1) & 63;

					if (c > max_Ch)
						max_Ch = c;

					if ((b & 0x80) != 0)
					{
						if (pat_Len < 1)
							break;

						mask[c] = pos[0, 1];
						pat_Len--;
					}

					if ((mask[c] & 0x01) != 0)
					{
						pos++;
						pat_Len--;
					}

					if ((mask[c] & 0x02) != 0)
					{
						pos++;
						pat_Len--;
					}

					if ((mask[c] & 0x04) != 0)
					{
						pos++;
						pat_Len--;
					}

					if ((mask[c] & 0x08) != 0)
					{
						pos += 2;
						pat_Len -= 2;
					}
				}
			}

			// Set the number of channels actually used
			mod.Chn = max_Ch + 1;
			mod.Trk = mod.Pat * mod.Chn;

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				goto Err4;

			// Read patterns
			for (c_int i = 0; i < mod.Pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern(mod, i) < 0)
					goto Err4;

				// If the offset to a pattern is 0, the pattern is empty
				if (pp_Pat[i] == 0)
				{
					mod.Xxp[i].Rows = 64;

					for (c_int j = 0; j < mod.Chn; j++)
					{
						c_int tNum = i * mod.Chn + j;

						if (lib.common.LibXmp_Alloc_Track(mod, tNum, 64) < 0)
							goto Err4;

						mod.Xxp[i].Index[j] = tNum;
					}
					continue;
				}

				if (f.Hio_Seek((c_long)(start + pp_Pat[i]), SeekOrigin.Begin) < 0)
					goto Err4;

				if (Load_It_Pattern(m, i, new_Fx, patBuf, f) < 0)
					goto Err4;
			}

			CMemory.Free(patBuf);
			CMemory.Free(pp_Pat);
			CMemory.Free(pp_Smp);
			CMemory.Free(pp_Ins);

			// Song message
			if (((ifh.Special & It_Special.Has_Msg) != 0) && (ifh.MsgLen > 0))
			{
				uint8[] comment = new uint8[ifh.MsgLen];
				if (comment != null)
				{
					f.Hio_Seek((c_long)(start + ifh.MsgOfs), SeekOrigin.Begin);

					ifh.MsgLen = (uint16)f.Hio_Read(comment, 1, ifh.MsgLen);
					f.Hio_Error();	// Clear error if any

					m.Comment = encoder.GetString(comment);

					// Translate linefeeds
					m.Comment = m.Comment.Replace('\u266a', '\n');
				}
			}

			// Format quirks
			m.Quirk |= Quirk_Flag.It | Quirk_Flag.ArpMem | Quirk_Flag.InsVol;

			if ((ifh.Flags & It_Flag.Link_Gxx) != 0)
				m.Quirk |= Quirk_Flag.PrEnv;
			else
				m.Quirk |= Quirk_Flag.UniSld;

			if (new_Fx)
				m.Quirk |= Quirk_Flag.VibHalf | Quirk_Flag.VibInv;
			else
			{
				m.Quirk &= ~Quirk_Flag.VibAll;
				m.Quirk |= Quirk_Flag.ItOldFx;
			}

			if (sample_Mode)
				m.Quirk &= ~(Quirk_Flag.Virtual | Quirk_Flag.RstChn);

			m.GVolBase = 0x80;
			m.GVol = ifh.Gv;
			m.MVolBase = 48;
			m.MVol = ifh.Mv;
			m.Read_Event_Type = Read_Event.It;

			if (is_Mpt_116)
				lib.common.LibXmp_Apply_Mpt_PreAmp(m);

			return 0;

			Err4:
			CMemory.Free(patBuf);
			CMemory.Free(pp_Pat);

			Err3:
			CMemory.Free(pp_Smp);

			Err2:
			CMemory.Free(pp_Ins);

			Err:
			return -1;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Xlat_Fx(c_int c, Xmp_Event e, uint8[] last_Fxp, bool new_fx)
		{
			uint8 h = Ports.LibXmp.Common.Msn(e.FxP), l = Ports.LibXmp.Common.Lsn(e.FxP);

			switch (e.FxT = fx[e.FxT])
			{
				// Extended effect
				case Fx_Xtnd:
				{
					e.FxT = Effects.Fx_Extended;

					if ((h == 0) && (e.FxP == 0))
					{
						e.FxP = last_Fxp[c];
						h = Ports.LibXmp.Common.Msn(e.FxP);
						l = Ports.LibXmp.Common.Lsn(e.FxP);
					}
					else
						last_Fxp[c] = e.FxP;

					switch (h)
					{
						// Glissando
						case 0x1:
						{
							e.FxP = (byte)(0x30 | l);
							break;
						}

						// Finetune -- not supported
						case 0x2:
						{
							e.FxT = e.FxP = 0;
							break;
						}

						// Vibrato wave
						case 0x3:
						{
							e.FxP = (byte)(0x40 | l);
							break;
						}

						// Tremolo wave
						case 0x4:
						{
							e.FxP = (byte)(0x70 | l);
							break;
						}

						// Panbrello wave
						case 0x5:
						{
							if (l <= 3)
							{
								e.FxT = Effects.Fx_Panbrello_Wf;
								e.FxP = l;
							}
							else
								e.FxT = e.FxP = 0;

							break;
						}

						// Pattern delay
						case 0x6:
						{
							e.FxP = (byte)(0xe0 | l);
							break;
						}

						// Instrument functions
						case 0x7:
						{
							e.FxT = Effects.Fx_It_InstFunc;
							e.FxP &= 0x0f;
							break;
						}

						// Set pan position
						case 0x8:
						{
							e.FxT = Effects.Fx_SetPan;
							e.FxP = (byte)(l << 4);
							break;
						}

						case 0x9:
						{
							if ((l == 0) || (l == 1))
							{
								// 0x91 = Set surround
								e.FxT = Effects.Fx_Surround;
								e.FxP = l;
							}
							else if ((l == 0xe) || (l == 0xf))
							{
								// 0x9f = Play reverse (MPT)
								e.FxT = Effects.Fx_Reverse;
								e.FxP = (byte)(l - 0xe);
							}
							break;
						}

						// High offset
						case 0xa:
						{
							e.FxT = Effects.Fx_HiOffset;
							e.FxP = l;
							break;
						}

						// Pattern loop
						case 0xb:
						{
							e.FxP = (byte)(0x60 | l);
							break;
						}

						// Note cut
						// Note delay
						case 0xc:
						case 0xd:
						{
							if ((e.FxP = l) == 0)
								e.FxP++;	// SD0 and SC0 become SD1 and SC1

							e.FxP |= (byte)(h << 4);
							break;
						}

						// Pattern row delay
						case 0xe:
						{
							e.FxT = Effects.Fx_It_RowDelay;
							e.FxP = l;
							break;
						}

						// Set parametered macro
						case 0xf:
						{
							e.FxT = Effects.Fx_Macro_Set;
							e.FxP = l;
							break;
						}

						default:
						{
							e.FxT = e.FxP = 0;
							break;
						}
					}
					break;
				}

				case Effects.Fx_Tremor:
				{
					if (!new_fx && (e.FxP != 0))
						e.FxP = (byte)(((Ports.LibXmp.Common.Msn(e.FxP) + 1) << 4) | (Ports.LibXmp.Common.Lsn(e.FxP) + 1));

					break;
				}

				case Effects.Fx_GlobalVol:
				{
					if (e.FxP > 0x80)	// See storlek test 16
						e.FxT = e.FxP = 0;

					break;
				}

				// No effect
				case Fx_None:
				{
					e.FxT = e.FxP = 0;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Xlat_VolFx(Xmp_Event @event)
		{
			c_int b = @event.Vol;
			@event.Vol = 0;

			if (b <= 0x40)
				@event.Vol = (byte)(b + 1);
			else if ((b >= 65) && (b <= 74))	// A
			{
				@event.F2T = Effects.Fx_F_VSlide_Up_2;
				@event.F2P = (byte)(b - 65);
			}
			else if ((b >= 75) && (b <= 84))	// B
			{
				@event.F2T = Effects.Fx_F_VSlide_Dn_2;
				@event.F2P = (byte)(b - 75);
			}
			else if ((b >= 85) && (b <= 94))	// C
			{
				@event.F2T = Effects.Fx_VSlide_Up_2;
				@event.F2P = (byte)(b - 85);
			}
			else if ((b >= 95) && (b <= 104))	// D
			{
				@event.F2T = Effects.Fx_VSlide_Dn_2;
				@event.F2P = (byte)(b - 95);
			}
			else if ((b >= 105) && (b <= 114))	// E
			{
				@event.F2T = Effects.Fx_Porta_Dn;
				@event.F2P = (byte)((b - 105) << 2);
			}
			else if ((b >= 115) && (b <= 124))	// F
			{
				@event.F2T = Effects.Fx_Porta_Up;
				@event.F2P = (byte)((b - 115) << 2);
			}
			else if ((b >= 128) && (b <= 192))	// pan
			{
				if (b == 192)
					@event.F2P = 0xff;
				else
					@event.F2P = (byte)((b - 128) << 2);

				@event.F2T = Effects.Fx_SetPan;
			}
			else if ((b >= 193) && (b <= 202))	// G
			{
				uint8[] val =
				{
					0x00, 0x01, 0x04, 0x08, 0x10,
					0x20, 0x40, 0x60, 0x80, 0xff
				};

				@event.F2T = Effects.Fx_TonePorta;
				@event.F2P = val[b - 193];
			}
			else if ((b >= 203) && (b <= 212))	// H
			{
				@event.F2T = Effects.Fx_Vibrato;
				@event.F2P = (byte)(b - 203);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Fix_Name(CPointer<uint8> s, c_int l)
		{
			c_int i;

			// IT names can have 0 at start of data, replace with space
			for (l--, i = 0; i < l; i++)
			{
				if (s[i] == 0)
					s[i] = (uint8)' ';
			}

			for (i--; (i >= 0) && (s[i] == ' '); i--)
			{
				if (s[i] == ' ')
					s[i] = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Load_It_Midi_Config(Module_Data m, Hio f)
		{
			m.Midi = new Midi_Macro_Data();
			if (m.Midi == null)
				return -1;

			// Skip global MIDI macros
			if (f.Hio_Seek(9 * 32, SeekOrigin.Current) < 0)
				return -1;

			// SFx macros
			for (c_int i = 0; i < 16; i++)
			{
				if (f.Hio_Read(m.Midi.Param[i].Data, 1, 32) < 32)
					return -1;

				m.Midi.Param[i].Data[31] = 0x00;
			}

			// Zxx macros
			for (c_int i = 0; i < 128; i++)
			{
				if (f.Hio_Read(m.Midi.Fixed[i].Data, 1, 32) < 32)
					return -1;

				m.Midi.Fixed[i].Data[31] = 0x00;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Read_Envelope(Xmp_Envelope ei, It_Envelope env, Hio f)
		{
			CPointer<uint8> buf = new CPointer<uint8>(82);

			if (f.Hio_Read(buf, 1, 82) != 82)
				return -1;

			env.Flg = (It_Env_Flag)buf[0];
			env.Num = Math.Min(buf[1], (uint8)25);		// Clamp to IT max

			env.Lpb = buf[2];
			env.Lpe = buf[3];
			env.Slb = buf[4];
			env.Sle = buf[5];

			for (c_int i = 0; i < 25; i++)
			{
				env.Node[i].Y = (int8)buf[6 + i * 3];
				env.Node[i].X = DataIo.ReadMem16L(buf + 7 + i * 3);
			}

			ei.Flg = (env.Flg & It_Env_Flag.On) != 0 ? Xmp_Envelope_Flag.On : Xmp_Envelope_Flag.None;

			if ((env.Flg & It_Env_Flag.Loop) != 0)
				ei.Flg |= Xmp_Envelope_Flag.Loop;

			if ((env.Flg & It_Env_Flag.SLoop) != 0)
				ei.Flg |= Xmp_Envelope_Flag.Sus | Xmp_Envelope_Flag.SLoop;

			if ((env.Flg & It_Env_Flag.Carry) != 0)
				ei.Flg |= Xmp_Envelope_Flag.Carry;

			ei.Npt = env.Num;
			ei.Sus = env.Slb;
			ei.Sue = env.Sle;
			ei.Lps = env.Lpb;
			ei.Lpe = env.Lpe;

			if ((ei.Npt > 0) && (ei.Npt <= 25))
			{
				for (c_int i = 0; i < ei.Npt; i++)
				{
					ei.Data[i * 2] = (c_short)env.Node[i].X;
					ei.Data[i * 2 + 1] = env.Node[i].Y;
				}
			}
			else
				ei.Flg &= ~Xmp_Envelope_Flag.On;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Identify_Tracker(Module_Data m, It_File_Header ifh, bool pat_Before_Smp, ref bool is_Mpt_116)
		{
			string tracker_Name;
			bool sample_Mode = (~ifh.Flags & It_Flag.Use_Inst) != 0;

			m.Flow_Mode = FlowMode_Flag.Mode_IT_210;

			switch (ifh.Cwt >> 8)
			{
				case 0x00:
				{
					tracker_Name = "unmo3";
					break;
				}

				case 0x01:
				case 0x02:	// Test for Schism Tracker sources
				{
					if ((ifh.Cmwt == 0x200) && (ifh.Cwt == 0x0214) && (ifh.Flags == (It_Flag.Linear_Freq | It_Flag.Stereo)) && (ifh.Special == It_Special.None) && (ifh.Hilite_Maj == 0) && (ifh.Hilite_Min == 0) &&
					    (ifh.InsNum == 0) && (ifh.PatNum + 1 == ifh.OrdNum) && (ifh.Gv == 128) && (ifh.Mv == 100) && (ifh.Is == 1) && (ifh.Sep == 128) && (ifh.Pwd == 0) &&
					    (ifh.MsgLen == 0) && (ifh.MsgOfs == 0) && (ifh.Rsvd == 0))
					{
						tracker_Name = "OpenSPC conversion";
					}
					else if ((ifh.Cmwt == 0x200) && (ifh.Cwt == 0x0217))
					{
						tracker_Name = "ModPlug Tracker 1.16";

						// ModPlug Tracker files aren't really IMPM 2.00
						ifh.Cmwt = (uint16)(sample_Mode ? 0x100 : 0x214);
						m.Flow_Mode = FlowMode_Flag.Mode_MPT_116;
						is_Mpt_116 = true;
					}
					else if ((ifh.Cmwt == 0x200) && (ifh.Cwt == 0x0202) && pat_Before_Smp)
					{
						// ModPlug Tracker ITs from pre-alpha 4 use tracker
						// 0x0202 and format 0x0200. Unfortunately, ITs from
						// Impulse Tracker may *also* use this. These MPT ITs
						// can be detected because they write patterns before
						// samples/instruments
						tracker_Name = "ModPlug Tracker 1.0 pre-alpha";
						ifh.Cmwt = (uint16)(sample_Mode ? 0x100 : 0x200);

						// TODO: Pre-alpha 4 has its own Pattern Loop behavior;
						// the <= 1.16 behavior is present in pre-alpha 6
						m.Flow_Mode = FlowMode_Flag.Mode_MPT_116;
						is_Mpt_116 = true;
					}
					else if (ifh.Cwt == 0x0216)
						tracker_Name = "Impulse Tracker 2.14v3";
					else if (ifh.Cwt == 0x0217)
						tracker_Name = "Impulse Tracker 2.14v5";
					else if ((ifh.Cwt == 0x0214) && (ifh.Rsvd == Common.Magic4('C', 'H', 'B', 'I')))
						tracker_Name = "Chibi Tracker";
					else
					{
						tracker_Name = string.Format("Impulse Tracker {0}.{1:x2}", (ifh.Cwt & 0x0f00) >> 8, ifh.Cwt & 0xff);

						if (ifh.Cwt < 0x104)
							m.Flow_Mode = FlowMode_Flag.Mode_IT_100;
						else if (ifh.Cwt < 0x210)
							m.Flow_Mode = FlowMode_Flag.Mode_IT_104;
					}
					break;
				}

				case 0x08:
				case 0x7f:
				{
					if (ifh.Cwt == 0x0888)
					{
						tracker_Name = "OpenMPT 1.17";

						// TODO: 1.17.02.49 onward implement IT 2.10+
						// Pattern Loop when the IT compatibility flag is set
						// (by default, it is not set)
						m.Flow_Mode = FlowMode_Flag.Mode_MPT_116;
						is_Mpt_116 = true;
					}
					else if (ifh.Cwt == 0x7fff)
						tracker_Name = "munch.py";
					else
						tracker_Name = string.Format("Unknown ({0:x4})", ifh.Cwt);

					break;
				}

				default:
				{
					switch (ifh.Cwt >> 12)
					{
						case 0x1:
						{
							lib.common.LibXmp_Schism_Tracker_String(out tracker_Name, 40, (ifh.Cwt & 0x0fff), (c_int)ifh.Rsvd);
							break;
						}

						case 0x5:
						{
							tracker_Name = string.Format("OpenMPT {0}.{1:x2}", (ifh.Cwt & 0x0f00) >> 8, ifh.Cwt & 0xff);
							if (ifh.Rsvd == Common.Magic4('C', 'H', 'B', 'I'))
								tracker_Name += " (compat.)";

							break;
						}

						case 0x6:
						{
							tracker_Name = string.Format("BeRoTracker {0}.{1:x2}", (ifh.Cwt & 0x0f00) >> 8, ifh.Cwt & 0xff);
							break;
						}

						default:
						{
							tracker_Name = string.Format("Unknown ({0:x4})", ifh.Cwt);
							break;
						}
					}
					break;
				}
			}

			lib.common.LibXmp_Set_Type(m, string.Format("{0} IT {1}.{2:x2}", tracker_Name, ifh.Cmwt >> 8, ifh.Cmwt & 0xff));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Load_Old_It_Instrument(Xmp_Instrument xxi, Hio f)
		{
			c_int[] inst_Map = new c_int[120];
			c_int[] inst_RMap = new c_int[Constants.Xmp_Max_Keys];
			It_Instrument1_Header i1h = new It_Instrument1_Header();
			c_int j, k;
			CPointer<uint8> buf = new CPointer<uint8>(64);

			if (f.Hio_Read(buf, 1, 64) != 64)
				return -1;

			i1h.Magic = DataIo.ReadMem32B(buf);
			if (i1h.Magic != Magic_IMPI)
				return -1;

			CMemory.MemCpy(i1h.DosName, buf + 4, 12);
			i1h.Zero = buf[16];
			i1h.Flags = (It_Env_Flag)buf[17];
			i1h.Vls = buf[18];
			i1h.Vle = buf[19];
			i1h.Sls = buf[20];
			i1h.Sle = buf[21];
			i1h.FadeOut = DataIo.ReadMem16L(buf + 24);
			i1h.Nna = buf[26];
			i1h.Dnc = buf[27];
			i1h.TrkVers = DataIo.ReadMem16L(buf + 28);
			i1h.Nos = buf[30];

			CMemory.MemCpy(i1h.Name, buf + 32, 26);
			Fix_Name(i1h.Name, 26);

			if (f.Hio_Read(i1h.Keys, 1, 240) != 240)
				return -1;

			if (f.Hio_Read(i1h.EPoint, 1, 200) != 200)
				return -1;

			if (f.Hio_Read(i1h.ENode, 1, 50) != 50)
				return -1;

			lib.common.LibXmp_Copy_Adjust(out xxi.Name, i1h.Name, 25, encoder);

			xxi.Rls = i1h.FadeOut << 7;

			xxi.Aei.Flg = Xmp_Envelope_Flag.None;

			if ((i1h.Flags & It_Env_Flag.On) != 0)
				xxi.Aei.Flg |= Xmp_Envelope_Flag.On;

			if ((i1h.Flags & It_Env_Flag.Loop) != 0)
				xxi.Aei.Flg |= Xmp_Envelope_Flag.Loop;

			if ((i1h.Flags & It_Env_Flag.SLoop) != 0)
				xxi.Aei.Flg |= Xmp_Envelope_Flag.Sus | Xmp_Envelope_Flag.SLoop;

			if ((i1h.Flags & It_Env_Flag.Carry) != 0)
				xxi.Aei.Flg |= Xmp_Envelope_Flag.Sus | Xmp_Envelope_Flag.Carry;

			xxi.Aei.Lps = i1h.Vls;
			xxi.Aei.Lpe = i1h.Vle;
			xxi.Aei.Sus = i1h.Sls;
			xxi.Aei.Sue = i1h.Sle;

			for (k = 0; (k < 25) && (i1h.ENode[k * 2] != 0xff); k++)
			{
			}

			// Sanity check
			if ((k >= 25) || (i1h.ENode[k * 2] != 0xff))
				return -1;

			for (xxi.Aei.Npt = k; k-- != 0; )
			{
				xxi.Aei.Data[k * 2] = i1h.ENode[k * 2];
				xxi.Aei.Data[k * 2 + 1] = i1h.ENode[k * 2 + 1];
			}

			// See how many different instruments we have
			for (j = 0; j < 120; j++)
				inst_Map[j] = -1;

			for (k = j = 0; j < Constants.Xmp_Max_Keys; j++)
			{
				c_int c = j < 120 ? i1h.Keys[j * 2 + 1] - 1 : -1;

				if ((c < 0) || (c >= 120))
				{
					xxi.Map[j].Ins = 0;
					xxi.Map[j].Xpo = 0;
					continue;
				}

				if (inst_Map[c] == -1)
				{
					inst_Map[c] = k;
					inst_RMap[k] = c;
					k++;
				}

				xxi.Map[j].Ins = (byte)inst_Map[c];
				xxi.Map[j].Xpo = (sbyte)(i1h.Keys[j * 2] - j);
			}

			xxi.Nsm = k;
			xxi.Vol = 0x40;

			if (k != 0)
			{
				xxi.Sub = ArrayHelper.InitializeArray<Xmp_SubInstrument>(k);
				if (xxi.Sub == null)
					return -1;

				for (j = 0; j < k; j++)
				{
					Xmp_SubInstrument sub = xxi.Sub[j];

					sub.Sid = inst_RMap[j];
					sub.Nna = (Xmp_Inst_Nna)i1h.Nna;
					sub.Dct = i1h.Dnc != 0 ? Xmp_Inst_Dct.Note : Xmp_Inst_Dct.Off;
					sub.Dca = Xmp_Inst_Dca.Cut;
					sub.Pan = -1;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Load_New_It_Instrument(Xmp_Instrument xxi, Hio f)
		{
			c_int[] inst_Map = new c_int[120];
			c_int[] inst_RMap = new c_int[Constants.Xmp_Max_Keys];
			It_Instrument2_Header i2h = new It_Instrument2_Header();
			It_Envelope env = new It_Envelope();
			Xmp_Inst_Dca[] dca2Nna = { Xmp_Inst_Dca.Cut, Xmp_Inst_Dca.Off, Xmp_Inst_Dca.Fade, Xmp_Inst_Dca.Fade /* Northern Sky (cj-north.it) has this... */ };
			c_int j, k;
			CPointer<uint8> buf = new CPointer<uint8>(64);

			if (f.Hio_Read(buf, 1, 64) != 64)
				return -1;

			i2h.Magic = DataIo.ReadMem32B(buf);
			if (i2h.Magic != Magic_IMPI)
				return -1;

			CMemory.MemCpy(i2h.DosName, buf + 4, 12);
			i2h.Zero = buf[16];
			i2h.Nna = buf[17];
			i2h.Dct = buf[18];
			i2h.Dca = buf[19];

			// Sanity check
			if (i2h.Dca > 3)
			{
				// Northern Sky has an instrument with DCA 3
				i2h.Dca = 0;
			}

			i2h.FadeOut = DataIo.ReadMem16L(buf + 20);
			i2h.Pps = buf[22];
			i2h.Ppc = buf[23];
			i2h.Gbv = buf[24];
			i2h.Dfp = buf[25];
			i2h.Rv = buf[26];
			i2h.Rp = buf[27];
			i2h.TrkVers = DataIo.ReadMem16L(buf + 28);
			i2h.Nos = buf[30];

			CMemory.MemCpy(i2h.Name, buf + 32, 26);
			Fix_Name(i2h.Name, 26);

			i2h.Ifc = buf[58];
			i2h.Ifr = buf[59];
			i2h.Mch = buf[60];
			i2h.Mpr = buf[61];
			i2h.Mbnk = DataIo.ReadMem16L(buf + 62);

			if (f.Hio_Read(i2h.Keys, 1, 240) != 240)
				return -1;

			lib.common.LibXmp_Copy_Adjust(out xxi.Name, i2h.Name, 25, encoder);
			xxi.Rls = i2h.FadeOut << 6;

			// Envelopes

			if (Read_Envelope(xxi.Aei, env, f) < 0)
				return -1;

			if (Read_Envelope(xxi.Pei, env, f) < 0)
				return -1;

			if (Read_Envelope(xxi.Fei, env, f) < 0)
				return -1;

			if ((xxi.Pei.Flg & Xmp_Envelope_Flag.On) != 0)
			{
				for (j = 0; j < xxi.Pei.Npt; j++)
					xxi.Pei.Data[j * 2 + 1] += 32;
			}

			if (((xxi.Aei.Flg & Xmp_Envelope_Flag.On) != 0) && (xxi.Aei.Npt == 0))
				xxi.Aei.Npt = 1;

			if (((xxi.Pei.Flg & Xmp_Envelope_Flag.On) != 0) && (xxi.Pei.Npt == 0))
				xxi.Pei.Npt = 1;

			if (((xxi.Fei.Flg & Xmp_Envelope_Flag.On) != 0) && (xxi.Fei.Npt == 0))
				xxi.Fei.Npt = 1;

			if ((env.Flg & It_Env_Flag.Filter) != 0)
			{
				xxi.Fei.Flg |= Xmp_Envelope_Flag.Flt;

				for (j = 0; j < env.Num; j++)
				{
					xxi.Fei.Data[j * 2 + 1] += 32;
					xxi.Fei.Data[j * 2 + 1] *= 4;
				}
			}
			else
			{
				// Pitch envelope is *50 to get fine interpolation
				for (j = 0; j < env.Num; j++)
					xxi.Fei.Data[j * 2 + 1] *= 50;
			}

			// See how many different instruments we have
			for (j = 0; j < 120; j++)
				inst_Map[j] = -1;

			for (k = j = 0; j < 120; j++)
			{
				c_int c = i2h.Keys[j * 2 + 1] - 1;

				if ((c < 0) || (c >= 120))
				{
					xxi.Map[j].Ins = 0xff;	// No sample
					xxi.Map[j].Xpo = 0;
					continue;
				}

				if (inst_Map[c] == -1)
				{
					inst_Map[c] = k;
					inst_RMap[k] = c;
					k++;
				}

				xxi.Map[j].Ins = (byte)inst_Map[c];
				xxi.Map[j].Xpo = (sbyte)(i2h.Keys[j * 2] - j);
			}

			xxi.Nsm = k;
			xxi.Vol = Math.Min(i2h.Gbv, (byte)128) >> 1;

			if (k != 0)
			{
				xxi.Sub = ArrayHelper.InitializeArray<Xmp_SubInstrument>(k);
				if (xxi.Sub == null)
					return -1;

				for (j = 0; j < k; j++)
				{
					Xmp_SubInstrument sub = xxi.Sub[j];

					sub.Sid = inst_RMap[j];
					sub.Nna = (Xmp_Inst_Nna)i2h.Nna;
					sub.Dct = (Xmp_Inst_Dct)i2h.Dct;
					sub.Dca = dca2Nna[i2h.Dca];
					sub.Pan = (i2h.Dfp & 0x80) != 0 ? -1 : i2h.Dfp * 4;
					sub.Ifc = i2h.Ifc;
					sub.Ifr = i2h.Ifr;
					sub.Rvv = (i2h.Rp << 8) | i2h.Rv;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Force_Sample_Length(Xmp_Sample xxs, Extra_Sample_Data xtra, c_int len)
		{
			xxs.Len = len;

			if (xxs.Lpe > xxs.Len)
				xxs.Lpe = xxs.Len;

			if (xxs.Lps >= xxs.Len)
				xxs.Flg &= ~Xmp_Sample_Flag.Loop;

			if (xtra != null)
			{
				if (xtra.Sue > xxs.Len)
					xtra.Sue = xxs.Len;

				if (xtra.Sus >= xxs.Len)
					xxs.Flg &= ~(Xmp_Sample_Flag.SLoop | Xmp_Sample_Flag.SLoop_BiDir);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private CPointer<byte> Unpack_It_Sample(Xmp_Sample xxs, It_Sample_Header ish, CPointer<byte> tmpBuf, Hio f)
		{
			c_int bytes = xxs.Len;
			c_int channels = 1;

			if ((ish.Flags & It_Smp_Flag._16Bit) != 0)
				bytes <<= 1;

			if ((ish.Flags & It_Smp_Flag.Stereo) != 0)
			{
				bytes <<= 1;
				channels = 2;
			}

			CPointer<uint8> decBuf = CMemory.CAlloc<byte>(bytes);
			if (decBuf == null)
				return null;

			if ((ish.Flags & It_Smp_Flag._16Bit) != 0)
			{
				Span<int16> pos = MemoryMarshal.Cast<uint8, int16>(decBuf.AsSpan());

				for (c_int i = 0; i < channels; i++)
				{
					Sample.ItSex_Decompress16(f, pos, xxs.Len, tmpBuf, Temp_Buffer_Len, (ish.Convert & It_Cvt_Flag.Diff) != 0);
					pos = pos.Slice(xxs.Len);
				}
			}
			else
			{
				CPointer<uint8> pos = decBuf;

				for (c_int i = 0; i < channels; i++)
				{
					Sample.ItSex_Decompress8(f, pos, xxs.Len, tmpBuf, Temp_Buffer_Len, (ish.Convert & It_Cvt_Flag.Diff) != 0);
					pos += xxs.Len;
				}
			}

			return decBuf;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Load_It_Sample(Module_Data m, c_int i, c_int start, bool sample_Mode, CPointer<uint8> tmpBuf, Hio f)
		{
			It_Sample_Header ish = new It_Sample_Header();
			Xmp_Module mod = m.Mod;
			CPointer<uint8> buf = new CPointer<uint8>(80);

			if (sample_Mode)
			{
				mod.Xxi[i].Sub = ArrayHelper.InitializeArray<Xmp_SubInstrument>(1);
				if (mod.Xxi[i].Sub == null)
					return -1;
			}

			if (f.Hio_Read(buf, 1, 80) != 80)
				return -1;

			ish.Magic = DataIo.ReadMem32B(buf);

			// Changed to continue to allow use-brdg.it and use-funk.it to
			// load correctly (both IT 2.04)
			if (ish.Magic != Magic_IMPS)
				return 0;

			Xmp_Sample xxs = mod.Xxs[i];
			Extra_Sample_Data xtra = m.Xtra[i];

			CMemory.MemCpy(ish.DosName, buf + 4, 12);
			ish.Zero = buf[16];
			ish.Gvl = buf[17];
			ish.Flags = (It_Smp_Flag)buf[18];
			ish.Vol = buf[19];

			CMemory.MemCpy(ish.Name, buf + 20, 26);
			Fix_Name(ish.Name, 26);

			ish.Convert = (It_Cvt_Flag)buf[46];
			ish.Dfp = buf[47];
			ish.Length = DataIo.ReadMem32L(buf + 48);
			ish.LoopBeg = DataIo.ReadMem32L(buf + 52);
			ish.LoopEnd = DataIo.ReadMem32L(buf + 56);
			ish.C5Spd = DataIo.ReadMem32L(buf + 60);
			ish.SLoopBeg = DataIo.ReadMem32L(buf + 64);
			ish.SLoopEnd = DataIo.ReadMem32L(buf + 68);
			ish.Sample_Ptr = DataIo.ReadMem32L(buf + 72);
			ish.Vis = buf[76];
			ish.Vid = buf[77];
			ish.Vir = buf[78];
			ish.Vit = buf[79];

			if ((ish.Flags & It_Smp_Flag._16Bit) != 0)
				xxs.Flg = Xmp_Sample_Flag._16Bit;

			if ((ish.Flags & It_Smp_Flag.Stereo) != 0)
				xxs.Flg |= Xmp_Sample_Flag.Stereo;

			xxs.Len = (c_int)ish.Length;

			xxs.Lps = (c_int)ish.LoopBeg;
			xxs.Lpe = (c_int)ish.LoopEnd;
			xxs.Flg |= (ish.Flags & It_Smp_Flag.Loop) != 0 ? Xmp_Sample_Flag.Loop : Xmp_Sample_Flag.None;
			xxs.Flg |= (ish.Flags & It_Smp_Flag.BLoop) != 0 ? Xmp_Sample_Flag.Loop_BiDir : Xmp_Sample_Flag.None;
			xxs.Flg |= (ish.Flags & It_Smp_Flag.SLoop) != 0 ? Xmp_Sample_Flag.SLoop : Xmp_Sample_Flag.None;
			xxs.Flg |= (ish.Flags & It_Smp_Flag.BSLoop) != 0 ? Xmp_Sample_Flag.SLoop_BiDir : Xmp_Sample_Flag.None;

			if ((ish.Flags & It_Smp_Flag.SLoop) != 0)
			{
				xtra.Sus = (c_int)ish.SLoopBeg;
				xtra.Sue = (c_int)ish.SLoopEnd;
			}

			if (sample_Mode)
			{
				// Create an instrument for each sample
				mod.Xxi[i].Vol = 64;
				mod.Xxi[i].Sub[0].Vol = ish.Vol;
				mod.Xxi[i].Sub[0].Pan = 0x80;
				mod.Xxi[i].Sub[0].Sid = i;
				mod.Xxi[i].Nsm = xxs.Len != 0 ? 1 : 0;

				lib.common.LibXmp_Instrument_Name(mod, i, ish.Name, 25, encoder);
			}
			else
				lib.common.LibXmp_Copy_Adjust(out xxs.Name, ish.Name, 25, encoder);

			// Convert C5SPD to relnote/finetune
			//
			// In IT we can have a sample associated with two or more
			// instruments, but c5spd is a sample attribute -- so we must
			// scan all xmp instruments to set the correct transposition
			for (c_int j = 0; j < mod.Ins; j++)
			{
				for (c_int k = 0; k < mod.Xxi[j].Nsm; k++)
				{
					Xmp_SubInstrument sub = mod.Xxi[j].Sub[k];

					if (sub.Sid == i)
					{
						sub.Vol = ish.Vol;
						sub.Gvl = Math.Min(ish.Gvl, (byte)64);
						sub.Vra = ish.Vis;	// Sample to sub-instrument vibrato
						sub.Vde = ish.Vid << 1;
						sub.Vwf = ish.Vit;
						sub.Vsw = (0xff - ish.Vir) >> 1;

						lib.period.LibXmp_C2Spd_To_Note((c_int)ish.C5Spd, out mod.Xxi[j].Sub[k].Xpo, out mod.Xxi[j].Sub[k].Fin);

						// Set sample pan (overrides subinstrument)
						if ((ish.Dfp & 0x80) != 0)
							sub.Pan = (ish.Dfp & 0x7f) * 4;
						else if (sample_Mode)
							sub.Pan = -1;
					}
				}
			}

			if (((ish.Flags & It_Smp_Flag.Sample) != 0) && (xxs.Len > 1))
			{
				Sample_Flag cvt = Sample_Flag.None;

				// Sanity check - some modules may have invalid sizes on
				// unused samples so only check this if the sample flag is set
				if (xxs.Len > Constants.Max_Sample_Size)
					return -1;

				if (f.Hio_Seek((c_long)(start + ish.Sample_Ptr), SeekOrigin.Begin) != 0)
					return -1;

				if ((xxs.Lpe > xxs.Len) || (xxs.Lps >= xxs.Lpe))
					xxs.Flg &= ~Xmp_Sample_Flag.Loop;

				if (ish.Convert == It_Cvt_Flag.Adpcm)
					cvt |= Sample_Flag.Adpcm;

				if ((~ish.Convert & It_Cvt_Flag.Signed) != 0)
					cvt |= Sample_Flag.Uns;

				// Compressed samples
				if ((ish.Flags & It_Smp_Flag.Comp) != 0)
				{
					c_int samples = xxs.Len;

					if ((ish.Flags & It_Smp_Flag.Stereo) != 0)
						samples <<= 1;

					// Sanity check - the lower bound on IT compressed
					// sample size (in bytes) is a little over 1/8th of the
					// number of SAMPLES in the sample
					c_long file_Len = f.Hio_Size();
					c_long min_Size = samples >> 3;
					c_long left = file_Len - (c_long)ish.Sample_Ptr;

					// No data to read at all? Just skip it...
					if (left <= 0)
						return 0;

					if ((file_Len > 0) && (left < min_Size))
						Force_Sample_Length(xxs, xtra, left << 3);

					Hio s = f.GetSampleHio(i, samples);

					CPointer<uint8> decBuf = Unpack_It_Sample(xxs, ish, tmpBuf, s);
					if (decBuf.IsNull)
					{
						s.Hio_Close();
						return -1;
					}

					s.Hio_Close();

					if ((ish.Flags & It_Smp_Flag._16Bit) != 0)
					{
						if (!BitConverter.IsLittleEndian)
							cvt |= Sample_Flag.BigEnd;
					}

					c_int ret = Sample.LibXmp_Load_Sample(m, null, Sample_Flag.NoLoad | cvt, mod.Xxs[i], decBuf);
					if (ret < 0)
						return -1;
				}
				else
				{
					if (Sample.LibXmp_Load_Sample(m, f, cvt, mod.Xxs[i], null, i) < 0)
						return -1;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Load_It_Pattern(Module_Data m, c_int i, bool new_Fx, CPointer<uint8> patBuf, Hio f)
		{
			Xmp_Module mod = m.Mod;
			Xmp_Event dummy = new Xmp_Event();
			Xmp_Event[] lastEvent = ArrayHelper.InitializeArray<Xmp_Event>(L_Channels);
			uint8[] mask = new uint8[L_Channels];
			uint8[] last_Fxp = new uint8[64];

			c_int r = 0;
			c_int num_Rows;

			c_int pat_Len = f.Hio_Read16L();
			mod.Xxp[i].Rows = num_Rows = f.Hio_Read16L();

			if (lib.common.LibXmp_Alloc_Tracks_In_Pattern(mod, i) < 0)
				return -1;

			f.Hio_Read16L();
			f.Hio_Read16L();

			if (f.Hio_Read(patBuf, 1, (size_t)pat_Len) < (size_t)pat_Len)
				return -1;

			CPointer<uint8> pos = patBuf;

			while ((r < num_Rows) && (--pat_Len >= 0))
			{
				uint8 b = pos[0, 1];

				if (b == 0)
				{
					r++;
					continue;
				}

				c_int c = (b - 1) & 63;

				if ((b & 0x80) != 0)
				{
					if (pat_Len < 1)
						break;

					mask[c] = pos[0, 1];
					pat_Len--;
				}

				// WARNING: We IGNORE events in disabled channels. Disabled
				// channels should be muted only, but we don't know the
				// real number of channels before loading the patterns and
				// we don't want to set it to 64 channels
				Xmp_Event @event;

				if (c >= mod.Chn)
					@event = dummy;
				else
					@event = Ports.LibXmp.Common.Event(m, i, c, r);

				if ((mask[c] & 0x01) != 0)
				{
					if (pat_Len < 1)
						break;

					b = pos[0, 1];

					// From ittech.txt:
					// Note ranges from 0->119 (C-0 -> B-9)
					// 255 = note off, 254 = note cut
					// Others = note fade (already programmed into IT's player
					//                     but not available in the editor)
					switch (b)
					{
						// Key off
						case 0xff:
						{
							b = Constants.Xmp_Key_Off;
							break;
						}

						// Cut
						case 0xfe:
						{
							b = Constants.Xmp_Key_Cut;
							break;
						}

						default:
						{
							if (b > 119)	// Fade
								b = Constants.Xmp_Key_Fade;
							else
								b++;

							break;
						}
					}

					lastEvent[c].Note = @event.Note = b;
					pat_Len--;
				}

				if ((mask[c] & 0x02) != 0)
				{
					if (pat_Len < 1)
						break;

					b = pos[0, 1];
					lastEvent[c].Ins = @event.Ins = b;
					pat_Len--;
				}

				if ((mask[c] & 0x04) != 0)
				{
					if (pat_Len < 1)
						break;

					b = pos[0, 1];
					lastEvent[c].Vol = @event.Vol = b;
					Xlat_VolFx(@event);
					pat_Len--;
				}

				if ((mask[c] & 0x08) != 0)
				{
					if (pat_Len < 2)
						break;

					b = pos[0, 1];
					if (b >= fx.Length)
						pos++;
					else
					{
						@event.FxT = b;
						@event.FxP = pos[0, 1];

						Xlat_Fx(c, @event, last_Fxp, new_Fx);

						lastEvent[c].FxT = @event.FxT;
						lastEvent[c].FxP = @event.FxP;
					}

					pat_Len -= 2;
				}

				if ((mask[c] & 0x10) != 0)
					@event.Note = lastEvent[c].Note;

				if ((mask[c] & 0x20) != 0)
					@event.Ins = lastEvent[c].Ins;

				if ((mask[c] & 0x40) != 0)
				{
					@event.Vol = lastEvent[c].Vol;
					Xlat_VolFx(@event);
				}

				if ((mask[c] & 0x80) != 0)
				{
					@event.FxT = lastEvent[c].FxT;
					@event.FxP = lastEvent[c].FxP;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool IsOpenMpt(Hio f)
		{
			// Start to check the version
			if (f.Hio_Seek(40, SeekOrigin.Begin) < 0)
				return false;

			uint16 cwt = f.Hio_Read16L();
			if ((cwt < 0x0889) || (cwt > 0x0fff))
				return false;

			// Check for the "228" mark
			if (f.Hio_Seek(-4, SeekOrigin.End) < 0)
				return false;

			uint32 offset = f.Hio_Read32L();
			if (offset >= f.Hio_Size())
				return false;

			if (f.Hio_Seek((c_long)offset, SeekOrigin.Begin) < 0)
				return false;

			if ((f.Hio_Read32B() & 0xffffff00) == 0x32323800)
				return true;

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Check if the module contains any DSP effects/VST plugins
		/// </summary>
		/********************************************************************/
		private string[] CheckForDspEffects(CPointer<uint32> pp_Ins, CPointer<uint32> pp_Smp, CPointer<uint32> pp_Pat, It_File_Header ifh, Hio f)
		{
			List<string> effects = new List<string>();

			uint8[] buf = new uint8[64];
			uint32 minPointer = uint32.MaxValue;

			minPointer = Math.Min(minPointer, MinValue(pp_Ins));
			minPointer = Math.Min(minPointer, MinValue(pp_Smp));
			minPointer = Math.Min(minPointer, MinValue(pp_Pat));

			if (ifh.Special.HasFlag(It_Special.Has_Msg))
				minPointer = Math.Min(minPointer, ifh.MsgOfs);

			while (f.Hio_Tell() < minPointer)
			{
				uint32 code = f.Hio_Read32B();
				uint32 length = f.Hio_Read32L();

				if (code == 0x5854504d)		// XTPM
					break;

				if ((code >= 0x46583030) && (code <= 0x46583939))	// FX00-FX99
				{
					// VST plugin. Find the name
					f.Hio_Seek(64, SeekOrigin.Current);

					f.Hio_Read(buf, 64, 1);
					effects.Add(encoder.GetString(buf));

					length -= 128;
				}

				f.Hio_Seek((c_long)length, SeekOrigin.Current);
			}

			return effects.Count == 0 ? null : effects.Distinct().OrderBy(x => x).ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint32 MinValue(CPointer<uint32> pp)
		{
			uint32 minValue = uint32.MaxValue;

			if (pp.IsNotNull)
			{
				for (int i = pp.Length - 1; i >= 0; i--)
				{
					if (pp[i] < minValue)
						minValue = pp[i];
				}
			}

			return minValue;
		}
		#endregion
	}
}
