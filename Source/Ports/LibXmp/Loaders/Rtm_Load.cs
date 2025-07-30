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
	internal class Rtm_Load : IFormatLoader
	{
		#region Internal structures

		#region ObjectHeader
		private class ObjectHeader
		{
			public byte[] Id { get; } = new byte[4];			// "RTMM"
			public byte Rc { get; set; }						// 0x20
			public byte[] Name { get; } = new byte[32];			// Module name
			public byte Eof { get; set; }						// 0x1a
			public uint16 Version { get; set; }					// Version of the format (actual: 0x110)
			public uint16 HeaderSize { get; set; }				// Object header size
		}
		#endregion

		#region RtmmHeader
		/// <summary>
		/// Real Tracker Music Module
		/// </summary>
		private class RtmmHeader
		{
			public byte[] Software { get; } = new byte[20];		// Software used for saving the module
			public byte[] Composer { get; } = new byte[32];
			public uint16 Flags { get; set; }					// Song flags. Bit 0: Linear table, Bit 1: Track names present
			public uint8 NTrack { get; set; }					// Number of tracks
			public uint8 NInstr { get; set; }					// Number of instruments
			public uint16 NPosition { get; set; }				// Number of positions
			public uint16 NPattern { get; set; }				// Number of patterns
			public uint8 Speed { get; set; }					// Initial speed
			public uint8 Tempo { get; set; }					// Initial tempo
			public int8[] Panning { get; } = new int8[32];		// Initial panning (for S3M compatibility)
			public uint32 ExtraDataSize { get; set; }			// Length of data after the header

			// Version 1.12
			public byte[] OriginalName { get; } = new byte[32];
		}
		#endregion

		#region RtndHeader
		/// <summary>
		/// Real Tracker Note Data
		/// </summary>
		private class RtndHeader
		{
			public uint16 Flags { get; set; }					// Always 1
			public uint8 NTrack { get; set; }
			public uint16 NRows { get; set; }
			public uint32 DataSize { get; set; }				// Size of packed data
		}
		#endregion

		#region EnvelopePoint
		private class EnvelopePoint
		{
			public c_long X { get; set; }
			public c_long Y { get; set; }
		}
		#endregion

		#region Envelope
		private class Envelope
		{
			public uint8 NPoint { get; set; }
			public EnvelopePoint[] Point { get; } = ArrayHelper.InitializeArray<EnvelopePoint>(12);
			public uint8 Sustain { get; set; }
			public uint8 LoopStart { get; set; }
			public uint8 LoopEnd { get; set; }
			public uint16 Flags { get; set; }					// Bit 0: Enable envelope, Bit 1: Sustain, Bit 2: Loop
		}
		#endregion

		#region RtinHeader
		/// <summary>
		/// Real Tracker Instrument
		/// </summary>
		private class RtinHeader
		{
			public uint8 NSample { get; set; }
			public uint16 Flags { get; set; }					// Bit 0: Default panning enabled, Bit 1: Mute samples
			public uint8[] Table { get; } = new uint8[120];		// Sample number for each note
			public Envelope VolumeEnv { get; } = new Envelope();
			public Envelope PanningEnv { get; } = new Envelope();
			public int8 VibFlg { get; set; }					// Vibrato type
			public int8 VibSweep { get; set; }					// Vibrato sweep
			public int8 VibDepth { get; set; }					// Vibrato depth
			public int8 VibRate { get; set; }					// Vibrato rate
			public uint16 VolFade { get; set; }

			// Version 1.10
			public uint8 MidiPort { get; set; }
			public uint8 MidiChannel { get; set; }
			public uint8 MidiProgram { get; set; }
			public uint8 MidiEnabled { get; set; }

			// Version 1.12
			public int8 MidiTranspose { get; set; }
			public uint8 MidiBenderRange { get; set; }
			public uint8 MidiBaseVolume { get; set; }
			public int8 MidiUseVelocity { get; set; }
		}
		#endregion

		#region RtsmHeader
		/// <summary>
		/// Real Tracker Sample
		/// </summary>
		private class RtsmHeader
		{
			public uint16 Flags { get; set; }					// Bit 1: 16 bits, Bit 2: Delta encoded (always)
			public uint8 BaseVolume { get; set; }
			public uint8 DefaultVolume { get; set; }
			public uint32 Length { get; set; }
			public uint8 Loop { get; set; }						// =0: No loop, =1: Forward loop, =2: Bi-directional loop
			public uint8[] Reserved { get; } = new uint8[3];
			public uint32 LoopBegin { get; set; }
			public uint32 LoopEnd { get; set; }
			public uint32 BaseFreq { get; set; }
			public uint8 BaseNote { get; set; }
			public int8 Panning { get; set; }					// Panning from -64 to 64
		}
		#endregion

		#endregion

		private const int Max_Samp = 1024;

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Rtm = new Format_Loader
		{
			Id = Guid.Parse("A948CA75-BF68-48B3-99DF-1D95281D53B8"),
			Name = "Real Tracker",
			Description = "Tracker created by Arnaud Hasenfratz.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Rtm_Load(LibXmp libXmp)
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
			return new Rtm_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			byte[] buf = new byte[4];

			if (f.Hio_Read(buf, 1, 4) < 4)
				return -1;

			if (CMemory.MemCmp(buf, "RTMM", 4) != 0)
				return -1;

			if (f.Hio_Read8() != 0x20)
				return -1;

			lib.common.LibXmp_Read_Title(f, out t, 32, encoder);

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
			ObjectHeader oh = new ObjectHeader();
			RtmmHeader rh = new RtmmHeader();
			RtndHeader rp = new RtndHeader();
			RtinHeader ri = new RtinHeader();
			RtsmHeader rs = new RtsmHeader();
			byte[] tracker_Name = new byte[21];
			byte[] composer = new byte[33];

			if (Read_Object_Header(f, oh, "RTMM") < 0)
				return -1;

			c_int version = oh.Version;

			f.Hio_Read(tracker_Name, 1, 20);
			tracker_Name[20] = 0;

			f.Hio_Read(composer, 1, 32);
			composer[32] = 0;

			rh.Flags = f.Hio_Read16L();		// Bit 0: Linear table, Bit 1: Track names
			rh.NTrack = f.Hio_Read8();
			rh.NInstr = f.Hio_Read8();
			rh.NPosition = f.Hio_Read16L();
			rh.NPattern = f.Hio_Read16L();
			rh.Speed = f.Hio_Read8();
			rh.Tempo = f.Hio_Read8();
			f.Hio_Read(rh.Panning, 32, 1);
			rh.ExtraDataSize = f.Hio_Read32L();

			// Sanity check
			if ((f.Hio_Error() != 0) || (rh.NPosition > 255) || (rh.NTrack > 32) || (rh.NPattern > 255))
				return -1;

			if (version >= 0x0112)
				f.Hio_Seek(32, SeekOrigin.Current);		// Skip original name

			for (c_int i = 0; i < rh.NPosition; i++)
			{
				mod.Xxo[i] = (byte)f.Hio_Read16L();

				if (mod.Xxo[i] >= rh.NPattern)
					return -1;
			}

			mod.Name = encoder.GetString(oh.Name);
			mod.Author = encoder.GetString(composer);

			lib.common.LibXmp_Set_Type(m, string.Format("{0} RTM {1:x}.{2:x2}", encoder.GetString(tracker_Name), version >> 8, version & 0xff));

			mod.Len = rh.NPosition;
			mod.Pat = rh.NPattern;
			mod.Chn = rh.NTrack;
			mod.Trk = mod.Chn * mod.Pat;
			mod.Ins = rh.NInstr;
			mod.Spd = rh.Speed;
			mod.Bpm = rh.Tempo;

			m.C4Rate = Constants.C4_Ntsc_Rate;
			m.Period_Type = (rh.Flags & 0x01) != 0 ? Containers.Common.Period.Linear : Containers.Common.Period.Amiga;

			for (c_int i = 0; i < mod.Chn; i++)
				mod.Xxc[i].Pan = rh.Panning[i] & 0xff;

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				return -1;

			c_int offset = (c_int)(42 + oh.HeaderSize + rh.ExtraDataSize);

			for (c_int i = 0; i < mod.Pat; i++)
			{
				f.Hio_Seek(start + offset, SeekOrigin.Begin);

				if (Read_Object_Header(f, oh, "RTND") < 0)
					return -1;

				rp.Flags = f.Hio_Read16L();
				rp.NTrack = f.Hio_Read8();
				rp.NRows = f.Hio_Read16L();
				rp.DataSize = f.Hio_Read32L();

				// Sanity check
				if ((rp.NTrack > rh.NTrack) || (rp.NRows > 256))
					return -1;

				offset += (c_int)(42 + oh.HeaderSize + rp.DataSize);

				if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, rp.NRows) < 0)
					return -1;

				for (c_int r = 0; r < rp.NRows; r++)
				{
					for (c_int j = 0;; j++)
					{
						uint8 c = f.Hio_Read8();

						if (c == 0)		// Next row
							break;

						// Sanity check
						if (j >= rp.NTrack)
							return -1;

						Xmp_Event @event = Ports.LibXmp.Common.Event(m, i, j, r);

						if ((c & 0x01) != 0)	// Set track
						{
							j = f.Hio_Read8();

							// Sanity check
							if (j >= rp.NTrack)
								return -1;

							@event = Ports.LibXmp.Common.Event(m, i, j, r);
						}

						if ((c & 0x02) != 0)	// Read note
						{
							@event.Note = (byte)(f.Hio_Read8() + 1);

							if (@event.Note == 0xff)
								@event.Note = Constants.Xmp_Key_Off;
							else
								@event.Note += 12;
						}

						if ((c & 0x04) != 0)	// Read instrument
							@event.Ins = f.Hio_Read8();

						if ((c & 0x08) != 0)	// Read effect
							@event.FxT = f.Hio_Read8();

						if ((c & 0x10) != 0)	// Read parameter
							@event.FxP = f.Hio_Read8();

						if ((c & 0x20) != 0)	// Read effect 2
							@event.F2T = f.Hio_Read8();

						if ((c & 0x40) != 0)	// Read parameter 2
							@event.F2P = f.Hio_Read8();
					}
				}
			}

			// Load instruments
			f.Hio_Seek(start + offset, SeekOrigin.Begin);

			// ESTIMATED value! We don't know the actual value at this point
			mod.Smp = Max_Samp;

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			c_int smpNum = 0;

			for (c_int i = 0; i < mod.Ins; i++)
			{
				Xmp_Instrument xxi = mod.Xxi[i];

				if (Read_Object_Header(f, oh, "RTIN") < 0)
					return -1;

				lib.common.LibXmp_Instrument_Name(mod, i, oh.Name, 32, encoder);

				if (oh.HeaderSize == 0)
				{
					ri.NSample = 0;
					continue;
				}

				ri.NSample = f.Hio_Read8();
				ri.Flags = f.Hio_Read16L();		// Bit 0: Default panning enabled

				if (f.Hio_Read(ri.Table, 1, 120) != 120)
					return -1;

				ri.VolumeEnv.NPoint = f.Hio_Read8();

				// Sanity check
				if (ri.VolumeEnv.NPoint >= 12)
					return -1;

				for (c_int j = 0; j < 12; j++)
				{
					ri.VolumeEnv.Point[j].X = (c_int)f.Hio_Read32L();
					ri.VolumeEnv.Point[j].Y = (c_int)f.Hio_Read32L();
				}

				ri.VolumeEnv.Sustain = f.Hio_Read8();
				ri.VolumeEnv.LoopStart = f.Hio_Read8();
				ri.VolumeEnv.LoopEnd = f.Hio_Read8();
				ri.VolumeEnv.Flags = f.Hio_Read16L();	// Bit 0: Enable, 1: Sus, 2: Loop

				ri.PanningEnv.NPoint = f.Hio_Read8();

				// Sanity check
				if (ri.PanningEnv.NPoint >= 12)
					return -1;

				for (c_int j = 0; j < 12; j++)
				{
					ri.PanningEnv.Point[j].X = (c_int)f.Hio_Read32L();
					ri.PanningEnv.Point[j].Y = (c_int)f.Hio_Read32L();
				}

				ri.PanningEnv.Sustain = f.Hio_Read8();
				ri.PanningEnv.LoopStart = f.Hio_Read8();
				ri.PanningEnv.LoopEnd = f.Hio_Read8();
				ri.PanningEnv.Flags = f.Hio_Read16L();

				ri.VibFlg = (int8)f.Hio_Read8();
				ri.VibSweep = (int8)f.Hio_Read8();
				ri.VibDepth = (int8)f.Hio_Read8();
				ri.VibRate = (int8)f.Hio_Read8();
				ri.VolFade = f.Hio_Read16L();

				if (version >= 0x0110)
				{
					ri.MidiPort = f.Hio_Read8();
					ri.MidiChannel = f.Hio_Read8();
					ri.MidiProgram = f.Hio_Read8();
					ri.MidiEnabled = f.Hio_Read8();
				}

				if (version >= 0x0112)
				{
					ri.MidiTranspose = (int8)f.Hio_Read8();
					ri.MidiBenderRange = f.Hio_Read8();
					ri.MidiBaseVolume = f.Hio_Read8();
					ri.MidiUseVelocity = (int8)f.Hio_Read8();
				}

				xxi.Nsm = ri.NSample;

				if (xxi.Nsm > 16)
					xxi.Nsm = 16;

				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, xxi.Nsm) < 0)
					return -1;

				for (c_int j = 0; j < 120; j++)
					xxi.Map[j].Ins = ri.Table[j];

				// Envelope
				xxi.Rls = ri.VolFade;
				xxi.Aei.Npt = ri.VolumeEnv.NPoint;
				xxi.Aei.Sus = ri.VolumeEnv.Sustain;
				xxi.Aei.Lps = ri.VolumeEnv.LoopStart;
				xxi.Aei.Lpe = ri.VolumeEnv.LoopEnd;
				xxi.Aei.Flg = (Xmp_Envelope_Flag)ri.VolumeEnv.Flags;
				xxi.Pei.Npt = ri.PanningEnv.NPoint;
				xxi.Pei.Sus = ri.PanningEnv.Sustain;
				xxi.Pei.Lps = ri.PanningEnv.LoopStart;
				xxi.Pei.Lpe = ri.PanningEnv.LoopEnd;
				xxi.Pei.Flg = (Xmp_Envelope_Flag)ri.PanningEnv.Flags;

				for (c_int j = 0; j < xxi.Aei.Npt; j++)
				{
					xxi.Aei.Data[j * 2 + 0] = (c_short)ri.VolumeEnv.Point[j].X;
					xxi.Aei.Data[j * 2 + 1] = (c_short)(ri.VolumeEnv.Point[j].Y / 2);
				}

				for (c_int j = 0; j < xxi.Pei.Npt; j++)
				{
					xxi.Pei.Data[j * 2 + 0] = (c_short)ri.PanningEnv.Point[j].X;
					xxi.Pei.Data[j * 2 + 1] = (c_short)(32 + ri.PanningEnv.Point[j].Y / 2);
				}

				// For each sample
				for (c_int j = 0; j < xxi.Nsm; j++, smpNum++)
				{
					Xmp_SubInstrument sub = xxi.Sub[j];

					if (Read_Object_Header(f, oh, "RTSM") < 0)
						return -1;

					rs.Flags = f.Hio_Read16L();
					rs.BaseVolume = f.Hio_Read8();
					rs.DefaultVolume = f.Hio_Read8();
					rs.Length = f.Hio_Read32L();
					rs.Loop = (uint8)f.Hio_Read32L();
					rs.LoopBegin = f.Hio_Read32L();
					rs.LoopEnd = f.Hio_Read32L();
					rs.BaseFreq = f.Hio_Read32L();
					rs.BaseNote = f.Hio_Read8();
					rs.Panning = (int8)f.Hio_Read8();

					lib.period.LibXmp_C2Spd_To_Note((c_int)rs.BaseFreq, out sub.Xpo, out sub.Fin);
					sub.Xpo += 48 - rs.BaseNote;
					sub.Vol = rs.DefaultVolume * rs.BaseVolume / 0x40;
					sub.Pan = 0x80 + rs.Panning * 2;

					// Autovibrato oddities:
					// Wave:  TODO: 0 sine, 1 square, 2 ramp down, 3 ramp up
					//        All invalid values are also ramp up.
					// Depth: the UI limits it 0-15, but higher values
					//        work. Negatives are very broken.
					// Rate:  the UI limits 0-63; but higher and negative
					//        values actually work how you would expect!
					//        Rate is half as fast as libxmp currently.
					// Sweep: the UI limits 0-255 but loads as signed.
					//        During playback, it is treated as unsigned.
					sub.Vwf = Math.Min((uint8)ri.VibFlg, (uint8)3);
					sub.Vde = Math.Max(ri.VibDepth, (int8)0) << 2;
					sub.Vra = (ri.VibRate + ((ri.VibRate > 0) ? 1 : 0)) >> 1;
					sub.Vsw = (uint8)ri.VibSweep;
					sub.Sid = smpNum;

					if (smpNum >= mod.Smp)
					{
						if (lib.common.LibXmp_Realloc_Samples(m, mod.Smp * 3 / 2) < 0)
							return -1;
					}

					Xmp_Sample xxs = mod.Xxs[smpNum];

					lib.common.LibXmp_Copy_Adjust(out xxs.Name, oh.Name, 31, encoder);

					xxs.Len = (c_int)rs.Length;
					xxs.Lps = (c_int)rs.LoopBegin;
					xxs.Lpe = (c_int)rs.LoopEnd;

					xxs.Flg = Xmp_Sample_Flag.None;

					if ((rs.Flags & 0x02) != 0)
					{
						xxs.Flg |= Xmp_Sample_Flag._16Bit;
						xxs.Len >>= 1;
						xxs.Lps >>= 1;
						xxs.Lpe >>= 1;
					}

					xxs.Flg |= (rs.Loop & 0x03) != 0 ? Xmp_Sample_Flag.Loop : Xmp_Sample_Flag.None;
					xxs.Flg |= rs.Loop == 2 ? Xmp_Sample_Flag.Loop_BiDir : Xmp_Sample_Flag.None;

					if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.Diff, xxs, null, smpNum) < 0)
						return -1;
				}
			}

			// Final sample number adjust
			if (lib.common.LibXmp_Realloc_Samples(m, smpNum) < 0)
				return -1;

			m.Quirk |= Quirk_Flag.Ft2;
			m.Read_Event_Type = Read_Event.Ft2;

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Read_Object_Header(Hio f, ObjectHeader h, string id)
		{
			f.Hio_Read(h.Id, 4, 1);

			if (CMemory.MemCmp(id, h.Id, 4) != 0)
				return -1;

			h.Rc = f.Hio_Read8();
			if (h.Rc != 0x20)
				return -1;

			if (f.Hio_Read(h.Name, 1, 32) != 32)
				return -1;

			h.Eof = f.Hio_Read8();
			h.Version = f.Hio_Read16L();
			h.HeaderSize = f.Hio_Read16L();

			return 0;
		}
		#endregion
	}
}
