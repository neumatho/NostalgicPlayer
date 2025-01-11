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
	/// This is not an official libxmp loader. I (Thomas Neumann) created it based on the MikMod loader,
	/// so I could get rid of MikMod from NostalgicPlayer
	/// </summary>
	internal class Dsm_Load : IFormatLoader
	{
		#region Internal structures

		#region Dsm_Song
		private class Dsm_Song
		{
			public uint8[] SongName = new uint8[28];
			public uint16 Version;
			public uint16 Flags;
			public uint32 Reserved2;
			public uint16 NumOrd;
			public uint16 NumSmp;
			public uint16 NumPat;
			public uint16 NumTrk;
			public uint8 GlobalVol;
			public uint8 MasterVol;
			public uint8 Speed;
			public uint8 Bpm;
			public uint8[] PanPos = new uint8[MaxChan];
			public uint8[] Orders = new uint8[MaxOrders];
		}
		#endregion

		#region Dsm_Inst
		private class Dsm_Inst
		{
			public uint8[] FileName = new uint8[13];
			public uint16 Flags;
			public uint8 Volume;
			public uint32 Length;
			public uint32 LoopStart;
			public uint32 LoopEnd;
			public uint32 Reserved1;
			public uint16 C2Spd;
			public uint16 Period;
			public uint8[] SampleName = new uint8[28];
		}
		#endregion

		#region Dsm_Note
		private class Dsm_Note
		{
			public uint8 Note;
			public uint8 Ins;
			public uint8 Vol;
			public uint8 Cmd;
			public uint8 Inf;
		}
		#endregion

		#endregion

		private const int MaxChan = 16;
		private const int MaxOrders = 128;
		private const int Surround = 0xa4;

		private const uint32 SongId = 0x534f4e47;		// SONG
		private const uint32 InstId = 0x494e5354;		// INST
		private const uint32 PattId = 0x50415454;		// PATT

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Dsm = new Format_Loader
		{
			Id = Guid.Parse("ED6F6B4C-3218-4B80-86C5-A2C62B9E3070"),
			Name = "Digital Sound Interface Kit RIFF",
			Description = "This loader recognizes the DSIK format, which is the internal module format of the “Digital Sound Interface Kit” (DSIK) library, the ancester of the SEAL library. This format has the same limitations as the S3M format.\n\nThe DSIK library was written by Carlos Hasan and released in 1994.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Dsm_Load(LibXmp libXmp)
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
			return new Dsm_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			CPointer<uint8> buf = new CPointer<uint8>(12);

			if (f.Hio_Read(buf, 1, 12) < 12)
				return -1;

			if ((CMemory.MemCmp(buf, "RIFF", 4) != 0) || (CMemory.MemCmp(buf + 8, "DSMF", 4) != 0))
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

			Dsm_Song mh = new Dsm_Song();
			CPointer<Dsm_Note> dsmBuf = CMemory.MAllocObj<Dsm_Note>(MaxChan * 64);
			if (dsmBuf.IsNull)
				return -1;

			uint32 blockId = 0;
			uint32 blockLp = 0;
			uint32 blockLn = 12;

			if (!GetBlockHeader(f, ref blockId, ref blockLp, ref blockLn))
				goto Err;

			if (blockId != SongId)
				goto Err;

			f.Hio_Read(mh.SongName, 1, 28);

			mh.Version = f.Hio_Read16L();
			mh.Flags = f.Hio_Read16L();
			mh.Reserved2 = f.Hio_Read32L();
			mh.NumOrd = f.Hio_Read16L();
			mh.NumSmp = f.Hio_Read16L();
			mh.NumPat = f.Hio_Read16L();
			mh.NumTrk = f.Hio_Read16L();
			mh.GlobalVol = f.Hio_Read8();
			mh.MasterVol = f.Hio_Read8();
			mh.Speed = f.Hio_Read8();
			mh.Bpm = f.Hio_Read8();

			f.Hio_Read(mh.PanPos, 1, MaxChan);
			f.Hio_Read(mh.Orders, 1, MaxOrders);

			if (f.Hio_Error() != 0)
				goto Err;

			// Set module variables
			lib.common.LibXmp_Copy_Adjust(out mod.Name, mh.SongName, 28, encoder);

			mod.Spd = mh.Speed;
			mod.Bpm = mh.Bpm;
			mod.Chn = mh.NumTrk;
			mod.Pat = mh.NumPat;
			mod.Trk = mod.Chn * mod.Pat;
			mod.Len = mh.NumOrd;
			mod.Rst = 0;

			CMemory.MemCpy<uint8>(mod.Xxo, mh.Orders, mod.Len);

			m.C4Rate = Constants.C4_Ntsc_Rate;
			m.Quirk |= Quirk_Flag.St3 | Quirk_Flag.ArpMem;
			m.Read_Event_Type = Read_Event.St3;

			for (int t = 0; t < MaxChan; t++)
			{
				if (mh.PanPos[t] == Surround)
				{
					mod.Xxc[t].Pan = 128;
					mod.Xxc[t].Flg |= Xmp_Channel_Flag.Surround;
				}
				else
					mod.Xxc[t].Pan = mh.PanPos[t] < 128 ? mh.PanPos[t] << 1 : 255;
			}

			mod.Ins = mh.NumSmp;
			mod.Smp = mod.Ins;

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				goto Err;

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				goto Err;

			int curSmp = 0;
			int curPat = 0;

			while ((curSmp < mod.Ins) || (curPat < mod.Pat))
			{
				if (!GetBlockHeader(f, ref blockId, ref blockLp, ref blockLn))
					goto Err;

				if ((blockId == InstId) && (curSmp < mod.Ins))
				{
					Dsm_Inst s = new Dsm_Inst();

					f.Hio_Read(s.FileName, 1, 13);

					s.Flags = f.Hio_Read16L();
					s.Volume = f.Hio_Read8();
					s.Length = f.Hio_Read32L();
					s.LoopStart = f.Hio_Read32L();
					s.LoopEnd = f.Hio_Read32L();
					s.Reserved1 = f.Hio_Read32L();
					s.C2Spd = f.Hio_Read16L();
					s.Period = f.Hio_Read16L();

					f.Hio_Read(s.SampleName, 1, 28);

					if (f.Hio_Error() != 0)
						goto Err;

					lib.common.LibXmp_Instrument_Name(mod, curSmp, s.SampleName, 28, encoder);

					mod.Xxs[curSmp].Len = (c_int)s.Length;
					mod.Xxs[curSmp].Lps = (c_int)s.LoopStart;
					mod.Xxs[curSmp].Lpe = (c_int)s.LoopEnd;

					if ((s.Flags & 1) != 0)
						mod.Xxs[curSmp].Flg |= Xmp_Sample_Flag.Loop;

					if (lib.common.LibXmp_Alloc_SubInstrument(mod, curSmp, 1) < 0)
						goto Err;

					mod.Xxi[curSmp].Nsm = 1;
					mod.Xxi[curSmp].Sub[0].Sid = curSmp;
					mod.Xxi[curSmp].Sub[0].Pan = 128;
					mod.Xxi[curSmp].Sub[0].Vol = s.Volume;

					lib.period.LibXmp_C2Spd_To_Note(s.C2Spd, out mod.Xxi[curSmp].Sub[0].Xpo, out mod.Xxi[curSmp].Sub[0].Fin);

					if (Sample.LibXmp_Load_Sample(m, f, (s.Flags & 2) == 0 ? Sample_Flag.Uns : Sample_Flag.None, mod.Xxs[curSmp], null, curSmp) < 0)
						goto Err;

					curSmp++;
				}
				else
				{
					if ((blockId == PattId) && (curPat < mod.Pat))
					{
						if (!ReadPattern(f, dsmBuf))
							goto Err;

						if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, curPat, 64) < 0)
							goto Err;

						for (int t = 0; t < mod.Chn; t++)
						{
							Xmp_Event[] events = m.Mod.Xxt[Ports.LibXmp.Common.Track_Num(m, curPat, t)].Event;
							ConvertTrack(dsmBuf, t * 64, events);
						}

						curPat++;
					}
				}
			}

			return 0;

			Err:
			CMemory.Free(dsmBuf);
			return -1;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will read the next block header
		/// </summary>
		/********************************************************************/
		private bool GetBlockHeader(Hio f, ref uint32 blockId, ref uint32 blockLp, ref uint32 blockLn)
		{
			// Make sure we're at the right position for reading the
			// next riff block, no matter how many bytes read
			f.Hio_Seek((c_long)(blockLp + blockLn), SeekOrigin.Begin);

			while (true)
			{
				blockId = f.Hio_Read32B();
				blockLn = f.Hio_Read32L();

				if (f.Hio_Error() != 0)
					return false;

				if ((blockId != SongId) && (blockId != InstId) && (blockId != PattId))
				{
					// Skip unknown block type
					f.Hio_Seek((c_long)blockLn, SeekOrigin.Current);
				}
				else
					break;
			}

			blockLp = (uint32)f.Hio_Tell();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Will read pattern data
		/// </summary>
		/********************************************************************/
		private bool ReadPattern(Hio f, CPointer<Dsm_Note> dsmBuf)
		{
			int row = 0;

			for (int i = 0; i < MaxChan * 64; i++)
			{
				Dsm_Note n = dsmBuf[i];
				n.Note = n.Ins = n.Vol = n.Cmd = n.Inf = 255;
			}

			short length = (short)f.Hio_Read16L();

			while (row < 64)
			{
				int flag = f.Hio_Read8();

				if ((f.Hio_Error() != 0) || (--length < 0))
					return false;

				if (flag != 0)
				{
					Dsm_Note n = dsmBuf[((flag & 0x0f) * 64) + row];

					if ((flag & 0x80) != 0)
						n.Note = f.Hio_Read8();

					if ((flag & 0x40) != 0)
						n.Ins = f.Hio_Read8();

					if ((flag & 0x20) != 0)
						n.Vol = f.Hio_Read8();

					if ((flag & 0x10) != 0)
					{
						n.Cmd = f.Hio_Read8();
						n.Inf = f.Hio_Read8();
					}
				}
				else
					row++;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Convert a track
		/// </summary>
		/********************************************************************/
		private void ConvertTrack(CPointer<Dsm_Note> tr, int offset, Xmp_Event[] events)
		{
			for (int t = 0; t < 64; t++)
			{
				Xmp_Event e = events[t];

				uint8 note = tr[offset + t].Note;
				uint8 ins = tr[offset + t].Ins;
				uint8 vol = tr[offset + t].Vol;
				uint8 cmd = tr[offset + t].Cmd;
				uint8 inf = tr[offset + t].Inf;

				if (ins != 255)
					e.Ins = ins;

				if ((note != 0) && (note != 255))
					e.Note = (byte)(12 + note);

				if (vol < 65)
					e.Vol = vol;

				if (cmd != 255)
				{
					if (cmd == 0x8)
					{
						if (inf == Surround)
						{
							e.FxT = Effects.Fx_Surround;
							e.FxP = 1;
						}
						else
						{
							if (inf <= 0x80)
							{
								inf = (uint8)((inf < 0x80) ? inf << 1 : 255);
								e.FxT = cmd;
								e.FxP = inf;
							}
						}
					}
					else if (cmd == 0x0b)
					{
						if (inf <= 0x7f)
						{
							e.FxT = cmd;
							e.FxP = inf;
						}
					}
					else
					{
						e.FxT = cmd;
						e.FxP = inf;
					}
				}
			}
		}
		#endregion
	}
}
