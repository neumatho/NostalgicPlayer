/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Format;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Loader;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// Liquid Tracker module loader based on the format description written
	/// by Nir Oren. Tested with Shell.liq sent by Adi Sapir.
	///
	/// TODO:
	/// - Vibrato, tremolo intensities wrong?
	/// - Gxx allows values >64 :(
	/// - MCx sets volume to 0
	/// - MDx doesn't delay volume?
	/// - MEx repeated tick 0s don't act like tick 0
	/// - Nxx has some bizarre behavior that prevents it from working between
	///   patterns sometimes and sometimes causes notes to drop out?
	/// - Pxx shares effect memory with Lxy, possibly other related awful things.
	/// - LIQ portamento is 4x more fine than S3M-compatibility portamento?
	///   (i.e. LIQ-mode UF1 seems equivalent to S3M-mode UE1). LT's S3M loader
	///   renames notes down by two octaves which is possibly implicated here.
	/// </summary>
	internal class Liq_Load : IFormatLoader
	{
		#region Internal structures

		#region Liq_Flag
		[Flags]
		private enum Liq_Flag
		{
			Cut_On_Limit = 0x01,
			Scream_Tracker_Compat = 0x02
		}
		#endregion

		#region Liq_Sample_Flag
		[Flags]
		private enum Liq_Sample_Flag
		{
			_16Bit = 0x01,
			Stereo = 0x02,
			Signed = 0x04
		}
		#endregion

		#region Liq_Header
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
		private class Liq_Header
		{
			public readonly uint8[] Magic = new uint8[14];		// "Liquid Module:"
			public readonly uint8[] Name = new uint8[30];		// ASCIIZ module name
			public readonly uint8[] Author = new uint8[20];		// Author name
			public uint8 _0x1a;									// 0x1a
			public readonly uint8[] Tracker = new uint8[20];	// Tracker name
			public uint16 Version;								// Format version
			public uint16 Speed;								// Initial speed
			public uint16 Bpm;									// Initial bpm
			public uint16 Low;									// Lowest note (Amiga Period*4)
			public uint16 High;									// Uppest note (Amiga Period*4)
			public uint16 Chn;									// Number of channels
			public Liq_Flag Flags;								// Module flags
			public uint16 Pat;									// Number of patterns saved
			public uint16 Ins;									// Number of instruments
			public uint16 Len;									// Module length
			public uint16 HdrSz;								// Header size
		}
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
		#endregion

		#region Liq_Instrument
		private class Liq_Instrument
		{
			public readonly uint8[] Magic = new uint8[4];		// 'L', 'D', 'S', 'S'
			public uint16 Version;								// LDSS header version
			public readonly uint8[] Name = new uint8[30];		// Instrument name
			public readonly uint8[] Editor = new uint8[20];		// Generator name
			public readonly uint8[] Author = new uint8[20];		// Author name
			public uint8 Hw_Id;									// Hardware used to record the sample
			public uint32 Length;								// Sample length
			public uint32 LoopStart;							// Sample loop start
			public uint32 LoopEnd;								// Sample loop end
			public uint32 C2Spd;								// C2SPD
			public uint8 Vol;									// Volume
			public Liq_Sample_Flag Flags;						// Flags
			public uint8 Pan;									// Pan
			public uint8 Midi_Ins;								// General MIDI instrument
			public uint8 Gvl;									// Global volume
			public uint8 Chord;									// Chord type
			public uint16 HdrSz;								// LDSS header size
			public uint16 Comp;									// Compression algorithm
			public uint32 Crc;									// CRC
			public uint8 Midi_Ch;								// MIDI channel
			public uint8 Loop_Type;								// -1 or 0: normal, 1: ping pong
			public readonly uint8[] Rsvd = new uint8[10];		// Reserved
			public readonly uint8[] FileName = new uint8[25];	// DOS file name
		}
		#endregion

		#region Liq_Pattern
		private class Liq_Pattern
		{
			public readonly uint8[] Magic = new uint8[4];		// 'L', 'P', 0, 0
			public readonly uint8[] Name = new uint8[30];		// ASCIIZ pattern name
			public uint16 Rows;									// Number of rows
			public uint32 Size;									// Size of packed pattern
			public uint32 Reserved;								// Reserved
		}
		#endregion

		#endregion

		#region Tables
		private static readonly uint8[] fx =
		[
			Effects.Fx_Arpeggio,
			Effects.Fx_S3M_Bpm,
			Effects.Fx_Break,
			Effects.Fx_Porta_Dn,
			None,
			Effects.Fx_Fine_Vibrato,
			Effects.Fx_GlobalVol,
			None,
			None,
			Effects.Fx_Jump,
			None,
			Effects.Fx_VolSlide,
			Effects.Fx_Extended,
			Effects.Fx_TonePorta,
			Effects.Fx_Offset,
			Effects.Fx_SetPan,
			None,
			Effects.Fx_Retrig,
			Effects.Fx_S3M_Speed,
			Effects.Fx_Tremolo,
			Effects.Fx_Porta_Up,
			Effects.Fx_Vibrato,
			None,
			Effects.Fx_Tone_VSlide,
			Effects.Fx_Vibra_VSlide
		];
		#endregion

		private const byte None = 0xff;

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Liq = new Format_Loader
		{
			Id = Guid.Parse("1FCC5321-BFBD-48B8-9FA4-693F28696E4B"),
			Name = "Liquid Tracker",
			Description = "DOS tracker created by Nir Oren.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Liq_Load(LibXmp libXmp)
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
			return new Liq_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			byte[] buf = new byte[15];

			if (f.Hio_Read(buf, 1, 14) < 14)
				return -1;

			if (CMemory.MemCmp(buf, "Liquid Module:", 14) != 0)
				return -1;

			lib.common.LibXmp_Read_Title(f, out t, 30, encoder);

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
			Liq_Header lh = new Liq_Header();
			Liq_Instrument li = new Liq_Instrument();
			Liq_Pattern lp = new Liq_Pattern();
			uint8[] tracker_Name = new uint8[21];
			c_int num_Channels_Stored;
			c_int num_Orders_Stored;

			f.Hio_Read(lh.Magic, 14, 1);
			f.Hio_Read(lh.Name, 30, 1);
			f.Hio_Read(lh.Author, 20, 1);
			f.Hio_Read8();
			f.Hio_Read(lh.Tracker, 20, 1);

			lh.Version = f.Hio_Read16L();
			lh.Speed = f.Hio_Read16L();
			lh.Bpm = f.Hio_Read16L();
			lh.Low = f.Hio_Read16L();
			lh.High = f.Hio_Read16L();
			lh.Chn = f.Hio_Read16L();
			lh.Flags = (Liq_Flag)f.Hio_Read32L();
			lh.Pat = f.Hio_Read16L();
			lh.Ins = f.Hio_Read16L();
			lh.Len = f.Hio_Read16L();
			lh.HdrSz = f.Hio_Read16L();

			// Sanity check
			if ((lh.Chn > Constants.Xmp_Max_Channels) || (lh.Pat > 256) || (lh.Ins > 256))
				return -1;

			if ((lh.Version >> 8) == 0)
			{
				lh.HdrSz = lh.Len;
				lh.Len = 0;

				// Skip 3 of 5 undocumented bytes (already read 2)
				f.Hio_Seek(3, SeekOrigin.Current);

				num_Channels_Stored = 64;
				num_Orders_Stored = 256;

				if (lh.Chn > 64)
					return -1;
			}
			else
			{
				num_Channels_Stored = lh.Chn;
				num_Orders_Stored = lh.Len;
			}

			if (lh.Len > 256)
				return -1;

			mod.Spd = lh.Speed;
			mod.Bpm = Math.Min((c_int)lh.Bpm, 255);
			mod.Chn = lh.Chn;
			mod.Pat = lh.Pat;
			mod.Ins = mod.Smp = lh.Ins;
			mod.Len = lh.Len;
			mod.Trk = mod.Chn * mod.Pat;

			m.Quirk |= Quirk_Flag.InsVol;

			mod.Name = encoder.GetString(lh.Name, 0, 30);
			CMemory.MemCpy<uint8>(tracker_Name, lh.Tracker, 20);
			tracker_Name[20] = 0;

			for (i = 20; i >= 0; i--)
			{
				if (tracker_Name[i] == 0x20)
					tracker_Name[i] = 0;

				if (tracker_Name[i] != 0)
					break;
			}

			lib.common.LibXmp_Set_Type(m, string.Format("{0} LIQ {1}.{2:d2}", encoder.GetString(tracker_Name), lh.Version >> 8, lh.Version & 0x00ff));

			for (i = 0; i < mod.Chn; i++)
			{
				uint8 pan = f.Hio_Read8();

				if (pan < 64)
					pan <<= 2;
				else if (pan == 64)
					pan = 0xff;
				else if (pan == 66)
				{
					pan = 0x80;
					mod.Xxc[i].Flg |= Xmp_Channel_Flag.Surround;
				}
				else
					return -1;

				mod.Xxc[i].Pan = pan;
			}

			if (i < num_Channels_Stored)
				f.Hio_Seek(num_Channels_Stored - i, SeekOrigin.Current);

			for (i = 0; i < mod.Chn; i++)
				mod.Xxc[i].Vol = f.Hio_Read8();

			if (i < num_Channels_Stored)
				f.Hio_Seek(num_Channels_Stored - i, SeekOrigin.Current);

			f.Hio_Read(mod.Xxo, 1, (size_t)num_Orders_Stored);

			// Skip 1.01 echo pools
			f.Hio_Seek(start + lh.HdrSz, SeekOrigin.Begin);

			// Version 0.00 doesn't store length
			if (lh.Version < 0x100)
			{
				for (i = 0; i < 256; i++)
				{
					if (mod.Xxo[i] == 0xff)
						break;
				}

				mod.Len = i;
			}

			m.C4Rate = Constants.C4_Ntsc_Rate;

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				return -1;

			// Read and convert patterns
			uint8 x1 = 0;
			uint8 x2 = 0;

			for (i = 0; i < mod.Pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern(mod, i) < 0)
					return -1;

				uint32 pMag = f.Hio_Read32B();

				// In spite of the documentation's claims, a magic of !!!! doesn't
				// do anything special here. Liquid Tracker expects a full pattern
				// specification regardless of the magic and no modules use !!!!
				if (pMag != Common.Magic4('L', 'P', '\0', '\0'))
					return -1;

				f.Hio_Read(lp.Name, 30, 1);
				lp.Rows = f.Hio_Read16L();
				lp.Size = f.Hio_Read32L();
				lp.Reserved = f.Hio_Read32L();

				// Sanity check
				if (lp.Rows > 256)
					return -1;

				mod.Xxp[i].Rows = lp.Rows;
				lib.common.LibXmp_Alloc_Tracks_In_Pattern(mod, i);

				c_int row = 0;
				c_int channel = 0;
				c_int count = f.Hio_Tell();

				// Packed pattern data is stored full Track after full Track from the left to
				// the right (all Intervals in Track and then going Track right). You should
				// expect 0C0h on any pattern end, and then your Unpacked Patterndata Pointer
				// should be equal to the value in offset [24h]; if it's not, you should exit
				// with an error
				Read_Event:

				// Sanity check
				if ((i >= mod.Pat) || (channel >= mod.Chn) || (row >= mod.Xxp[i].Rows))
					return -1;

				Xmp_Event @event = Ports.LibXmp.Common.Event(m, i, channel, row);

				if (x2 != 0)
				{
					if (Decode_Event(x1, @event, f) < 0)
						return -1;

					x2--;
					goto Next_Row;
				}

				x1 = f.Hio_Read8();

				Test_Event:
				// Sanity check
				if ((i >= mod.Pat) || (channel >= mod.Chn) || (row >= mod.Xxp[i].Rows))
					return -1;

				@event = Ports.LibXmp.Common.Event(m, i, channel, row);

				switch (x1)
				{
					// End of pattern
					case 0xc0:
					{
						if ((f.Hio_Tell() - count) != lp.Size)
							return -1;

						goto Next_Pattern;
					}

					// Skip channels
					case 0xe1:
					{
						x1 = f.Hio_Read8();
						channel += x1;

						// Fall thru
						goto case 0xa0;
					}

					// Next channel
					case 0xa0:
					{
						channel++;

						if (channel >= mod.Chn)
							channel--;

						row = -1;
						goto Next_Row;
					}

					// Skip rows
					case 0xe0:
					{
						x1 = f.Hio_Read8();
						row += x1;

						// Fall thru
						goto case 0x80;
					}

					// Next row
					case 0x80:
					{
						goto Next_Row;
					}
				}

				if ((x1 > 0xc0) && (x1 < 0xe0))		// Packed data
				{
					if (Decode_Event(x1, @event, f) < 0)
						return -1;

					goto Next_Row;
				}

				if ((x1 > 0xa0) && (x1 < 0xc0))		// Packed data repeat
				{
					x2 = f.Hio_Read8();

					if (Decode_Event(x1, @event, f) < 0)
						return -1;

					goto Next_Row;
				}

				if ((x1 > 0x80) && (x1 < 0xa0))		// Packed data repeat, keep note
				{
					x2 = f.Hio_Read8();

					if (Decode_Event(x1, @event, f) < 0)
						return -1;

					while (x2 != 0)
					{
						row++;

						// Sanity check
						if (row >= lp.Rows)
							return -1;

						Ports.LibXmp.Common.Event(m, i, channel, row).CopyFrom(@event);
						x2--;
					}

					goto Next_Row;
				}

				// Unpacked data
				if (Liq_Translate_Note(x1, @event) < 0)
					return -1;

				x1 = f.Hio_Read8();
				if (x1 > 100)
				{
					row++;
					goto Test_Event;
				}

				if (x1 != 0xff)
					@event.Ins = (byte)(x1 + 1);

				x1 = f.Hio_Read8();
				if (x1 != 0xff)
					@event.Vol = (byte)(x1 + 1);

				x1 = f.Hio_Read8();
				if (x1 != 0xff)
					@event.FxT = (byte)(x1 - 'A');

				x1 = f.Hio_Read8();
				@event.FxP = x1;

				// Sanity check
				if ((@event.Ins > 100) || (@event.Vol > 65))
					return -1;

				Liq_Translate_Effect(@event);

				Next_Row:
				row++;

				if (row >= mod.Xxp[i].Rows)
				{
					row = 0;
					x2 = 0;
					channel++;
				}

				// Sanity check
				if (channel >= mod.Chn)
					channel = 0;

				goto Read_Event;

				Next_Pattern:
				;
			}

			// Read and convert instruments
			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			for (i = 0; i < mod.Ins; i++)
			{
				Xmp_Instrument xxi = mod.Xxi[i];
				Xmp_Sample xxs = mod.Xxs[i];

				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					return -1;

				Xmp_SubInstrument sub = xxi.Sub[0];

				uint32 ldss_Magic = f.Hio_Read32B();

				if (ldss_Magic == Common.Magic4('?', '?', '?', '?'))
					continue;

				if (ldss_Magic != Common.Magic4('L', 'D', 'S', 'S'))
					return -1;

				li.Version = f.Hio_Read16L();
				f.Hio_Read(li.Name, 30, 1);
				f.Hio_Read(li.Editor, 20, 1);
				f.Hio_Read(li.Author, 20, 1);
				li.Hw_Id = f.Hio_Read8();

				li.Length = f.Hio_Read32L();
				li.LoopStart = f.Hio_Read32L();
				li.LoopEnd = f.Hio_Read32L();
				li.C2Spd = f.Hio_Read32L();

				li.Vol = f.Hio_Read8();
				li.Flags = (Liq_Sample_Flag)f.Hio_Read8();
				li.Pan = f.Hio_Read8();
				li.Midi_Ins = f.Hio_Read8();
				li.Gvl = f.Hio_Read8();
				li.Chord = f.Hio_Read8();

				li.HdrSz = f.Hio_Read16L();
				li.Comp = f.Hio_Read16L();
				li.Crc = f.Hio_Read32L();

				li.Midi_Ch = f.Hio_Read8();
				li.Loop_Type = f.Hio_Read8();
				f.Hio_Read(li.Rsvd, 1, 10);
				f.Hio_Read(li.FileName, 25, 1);

				// Sanity check
				if (f.Hio_Error() != 0)
					return -1;

				xxi.Nsm = li.Length != 0 ? 1 : 0;
				xxi.Vol = 0x40;

				xxs.Len = (c_int)li.Length;
				xxs.Lps = (c_int)li.LoopStart;
				xxs.Lpe = (c_int)li.LoopEnd;

				// Note: LIQ_SAMPLE_SIGNED is ignored by Liquid Tracker 1.50;
				// all samples are interpreted as signed
				if ((li.Flags & Liq_Sample_Flag._16Bit) != 0)
				{
					xxs.Flg |= Xmp_Sample_Flag._16Bit;
					xxs.Len >>= 1;
					xxs.Lps >>= 1;
					xxs.Lpe >>= 1;
				}

				// Storage is the same as S3M i.e. left then right. The shareware
				// version mixes stereo samples to mono, as stereo samples were
				// listed as a registered feature (yet to be verified)
				if ((li.Flags & Liq_Sample_Flag.Stereo) != 0)
				{
					xxs.Flg |= Xmp_Sample_Flag.Stereo;
					xxs.Len >>= 1;
					xxs.Lps >>= 1;
					xxs.Lpe >>= 1;
				}

				if (li.LoopEnd > 0)
				{
					xxs.Flg |= Xmp_Sample_Flag.Loop;

					if (li.Loop_Type == 1)
						xxs.Flg |= Xmp_Sample_Flag.Loop_BiDir;
				}

				// Global volume was added(?) in LDSS 1.01 and, like the channel
				// volume, has a range of 0-64 with 32=100%
				if (li.Version < 0x101)
					li.Gvl = 0x20;

				sub.Vol = li.Vol;
				sub.Gvl = li.Gvl;
				sub.Pan = li.Pan;
				sub.Sid = i;

				lib.common.LibXmp_Instrument_Name(mod, i, li.Name, 30, encoder);

				lib.period.LibXmp_C2Spd_To_Note((c_int)li.C2Spd, out sub.Xpo, out sub.Fin);
				f.Hio_Seek(li.HdrSz - 0x90, SeekOrigin.Current);

				if (xxs.Len == 0)
					continue;

				if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, xxs, null, i) < 0)
					return -1;
			}

			m.Quirk |= Quirk_Flag.FineFx | Quirk_Flag.RtOnce;
			m.Flow_Mode = (lh.Flags & Liq_Flag.Scream_Tracker_Compat) != 0 ? FlowMode_Flag.Mode_Liquid_Compat : FlowMode_Flag.Mode_Liquid;
			m.Read_Event_Type = Read_Event.St3;

			// Channel volume and instrument global volume are both "normally" 32 and
			// can be increased to 64, effectively allowing per-channel and
			// per-instrument gain of 2x each. Simulate this by keeping the original
			// volumes while increasing mix volume by 2x each.
			// 
			// The global volume effect also (unintentionally?) has gain functionality
			// in the sense that values >64 are not ignored. This isn't supported yet
			m.MVol = 48 * 2 * 2;
			m.MVolBase = 48;

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Liq_Translate_Effect(Xmp_Event e)
		{
			uint8 h = Ports.LibXmp.Common.Msn(e.FxP), l = Ports.LibXmp.Common.Lsn(e.FxP);

			if (e.FxT >= fx.Length)
			{
				e.FxT = e.FxP = 0;
				return;
			}

			e.FxT = fx[e.FxT];

			switch (e.FxT)
			{
				// Global volume (decimal)
				case Effects.Fx_GlobalVol:
				{
					l = (uint8)((e.FxP >> 4) * 10 + (e.FxP & 0x0f));
					e.FxP = l;
					break;
				}

				// Extended effects
				case Effects.Fx_Extended:
				{
					switch (h)
					{
						// Glissando
						case 0x3:
						{
							e.FxP = (byte)(l | (Effects.Ex_Gliss << 4));
							break;
						}

						// Vibrato wave
						case 0x4:
						{
							if ((l & 3) == 3)
								l--;

							e.FxP = (byte)(l | (Effects.Ex_Vibrato_Wf << 4));
							break;
						}

						// Finetune
						case 0x5:
						{
							e.FxP = (byte)(l | (Effects.Ex_FineTune << 4));
							break;
						}

						// Pattern loop
						case 0x6:
						{
							e.FxP = (byte)(l | (Effects.Ex_Pattern_Loop << 4));
							break;
						}

						// Tremolo wave
						case 0x7:
						{
							if ((l & 3) == 3)
								l--;

							e.FxP = (byte)(l | (Effects.Ex_Tremolo_Wf << 4));
							break;
						}

						// Cut
						case 0xc:
						{
							e.FxP = (byte)(l | (Effects.Ex_Cut << 4));
							break;
						}

						// Delay
						case 0xd:
						{
							e.FxP = (byte)(l | (Effects.Ex_Delay << 4));
							break;
						}

						// Pattern delay
						case 0xe:
						{
							e.FxP = (byte)(l | (Effects.Ex_Patt_Delay << 4));
							break;
						}

						// Ignore
						default:
						{
							e.FxT = e.FxP = 0;
							break;
						}
					}
					break;
				}

				// Pan control
				case Effects.Fx_SetPan:
				{
					l = (uint8)((e.FxP >> 4) * 10 + (e.FxP & 0x0f));

					if (l == 70)
					{
						// TODO: If the effective value is 70, reset ALL channels to
						// default pan positions
						e.FxT = e.FxP = 0;
					}
					else if (l == 66)
					{
						e.FxT = Effects.Fx_Surround;
						e.FxP = 1;
					}
					else if (l <= 64)
						e.FxP = (byte)(l * 0xff / 64);
					else
						e.FxT = e.FxP = 0;

					break;
				}

				// No effect
				case None:
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
		private c_int Liq_Translate_Note(uint8 note, Xmp_Event @event)
		{
			if (note == 0xfe)
				@event.Note = Constants.Xmp_Key_Off;
			else
			{
				// 1.00+ format documents claim a 9 octave range, but Liquid Tracker
				// <=1.50 only allows the use of the first 7. 0.00 should be within
				// the NO range of 5 octaves. Either way, they convert the same for
				// now, so just ignore any note that libxmp can't handle
				if (note > 107)
					return -1;

				if ((note + 36) >= Constants.Xmp_Max_Keys)
					return 0;

				@event.Note = (byte)(note + 1 + 36);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Decode_Event(uint8 x1, Xmp_Event @event, Hio f)
		{
			uint8 x2 = 0;

			@event.Clear();

			if ((x1 & 0x01) != 0)
			{
				x2 = f.Hio_Read8();

				if (Liq_Translate_Note(x2, @event) < 0)
					return -1;
			}

			if ((x1 & 0x02) != 0)
				@event.Ins = (byte)(f.Hio_Read8() + 1);

			if ((x1 & 0x04) != 0)
				@event.Vol = (byte)(f.Hio_Read8() + 1);

			if ((x1 & 0x08) != 0)
				@event.FxT = (byte)(f.Hio_Read8() - 'A');
			else
				@event.FxT = None;

			if ((x1 & 0x10) != 0)
				@event.FxP = f.Hio_Read8();
			else
				@event.FxP = 0xff;

			// Sanity check
			if ((@event.Ins > 100) || (@event.Vol > 65))
				return -1;

			Liq_Translate_Effect(@event);

			return 0;
		}
		#endregion
	}
}
