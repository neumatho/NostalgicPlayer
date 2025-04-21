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
using Polycode.NostalgicPlayer.Ports.LibXmp.FormatExtras;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// 
	/// </summary>
	internal class Far_Load : IFormatLoader
	{
		#region Internal structures

		#region Far_Header
		private class Far_Header
		{
			public uint32 Magic;								// File magic: 'FAR\xfe'
			public readonly uint8[] Name = new uint8[40];		// Song name
			public readonly uint8[] CrLf = new uint8[3];		// 0x0d 0x0a 0x1a
			public uint16 HeaderSize;							// Remaining header size in bytes
			public uint8 Version;								// Version MSN=major, LSN=minor
			public readonly uint8[] Ch_On = new uint8[16];		// Channel on/off switches
			public readonly uint8[] Rsvd1 = new uint8[9];		// Current editing values
			public uint8 Tempo;									// Default tempo
			public readonly uint8[] Pan = new uint8[16];		// Channel pan definitions
			public readonly uint8[] Rsvd2 = new uint8[4];		// Grid, mode (for editor)
			public uint16 TextLen;								// Length of embedded text
		}
		#endregion

		#region Far_Header2
		private class Far_Header2
		{
			public readonly uint8[] Order = new uint8[256];		// Orders
			public uint8 Patterns;								// Number of stored patterns (?)
			public uint8 SongLen;								// Song length in patterns
			public uint8 Restart;								// Restart pos
			public readonly uint16[] PatSize = new uint16[256];	// Size of each pattern in bytes
		}
		#endregion

		#region Far_Instrument
		private class Far_Instrument
		{
			public readonly uint8[] Name = new uint8[32];		// Instrument name
			public uint32 Length;								// Length of sample (up to 64Kb)
			public uint8 FineTune;								// Finetune (unsupported)
			public uint8 Volume;								// Volume (unsupported?)
			public uint32 Loop_Start;							// Loop start
			public uint32 LoopEnd;								// Loop end
			public uint8 SampleType;							// 1=16 bit sample
			public uint8 LoopMode;
		}
		#endregion

		#endregion

		private static readonly uint32 Magic_FAR = Common.Magic4('F', 'A', 'R', '\xfe');

		private const c_int Comment_MaxLines = 44;

		private readonly LibXmp lib;
		private readonly Xmp_Context ctx;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Far = new Format_Loader
		{
			Id = Guid.Parse("E2C8619A-6957-43C9-A518-71002E37408B"),
			Name = "Farandole Composer",
			Description = "This loader recognizes “Farandole” modules. These modules can be up to 16 channels and have Protracker comparable effects.\n\nThe Farandole composer was written by Daniel Potter and released in 1994.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Far_Load(LibXmp libXmp, Xmp_Context ctx)
		{
			lib = libXmp;
			this.ctx = ctx;
			encoder = EncoderCollection.Dos;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new instance of the loader
		/// </summary>
		/********************************************************************/
		private static IFormatLoader Create(LibXmp libXmp, Xmp_Context ctx)
		{
			return new Far_Load(libXmp, ctx);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			if (f.Hio_Read32B() != Magic_FAR)
				return -1;

			lib.common.LibXmp_Read_Title(f, out t, 40, encoder);

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
			Far_Header ffh = new Far_Header();
			Far_Header2 ffh2 = new Far_Header2();
			Far_Instrument fih = new Far_Instrument();
			uint8[] sample_Map = new uint8[8];

			ffh.Magic = f.Hio_Read32B();				// File magic: 'FAR\xfe'
			f.Hio_Read(ffh.Name, 40, 1);		// Song name
			f.Hio_Read(ffh.CrLf, 3, 1);		// 0x0d 0x0a 0x1a
			ffh.HeaderSize = f.Hio_Read16L();			// Remaining header size in bytes
			ffh.Version = f.Hio_Read8();				// Version MSN=major, LSN=minor
			f.Hio_Read(ffh.Ch_On, 16, 1);	// Channel on/off switches
			f.Hio_Seek(9, SeekOrigin.Current);	// Current editing values
			ffh.Tempo = f.Hio_Read8();					// Default tempo
			f.Hio_Read(ffh.Pan, 16, 1);		// Channel pan definitions
			f.Hio_Read32L();							// Grid, mode (for editor)
			ffh.TextLen = f.Hio_Read16L();				// Length of embedded text

			// Sanity check
			if (ffh.Tempo >= 16)
				return -1;

			CPointer<uint8> comment = CMemory.MAlloc<uint8>(ffh.TextLen + Comment_MaxLines + 1);
			if (comment.IsNotNull)
			{
				Far_Read_Text(comment, ffh.TextLen, f);
				m.Comment = encoder.GetString(comment.AsSpan()).Replace('\u25d9', '\n');
			}
			else
				f.Hio_Seek(ffh.TextLen, SeekOrigin.Current);	// Skip song text

			f.Hio_Read(ffh2.Order, 256, 1);	// Orders
			ffh2.Patterns = f.Hio_Read8();				// Number of stored patterns (?)
			ffh2.SongLen = f.Hio_Read8();				// Song length in patterns
			ffh2.Restart = f.Hio_Read8();				// Restart pos

			for (i = 0; i < 256; i++)
				ffh2.PatSize[i] = f.Hio_Read16L();		// Size of each pattern in bytes

			if (f.Hio_Error() != 0)
				return -1;

			// Skip unsupported header extension if it exists. The documentation claims
			// this field is the "remaining" header size, but it's the total size
			if (ffh.HeaderSize > (869 + ffh.TextLen))
			{
				if (f.Hio_Seek(ffh.HeaderSize, SeekOrigin.Begin) != 0)
					return -1;
			}

			mod.Chn = 16;
			mod.Len = ffh2.SongLen;
			mod.Rst = ffh2.Restart;
			CMemory.MemCpy<byte>(mod.Xxo, ffh2.Order, mod.Len);

			for (mod.Pat = i = 0; i < 256; i++)
			{
				if (ffh2.PatSize[i] != 0)
					mod.Pat = i + 1;
			}

			// Make sure referenced zero-sized patterns are also counted
			for (i = 0; i < mod.Len; i++)
			{
				if (mod.Pat <= mod.Xxo[i])
					mod.Pat = mod.Xxo[i] + 1;
			}

			mod.Trk = mod.Chn * mod.Pat;

			if (Far_Module_Extra.LibXmp_Far_New_Module_Extras(lib, ctx, m) != 0)
				return -1;

			Far_Module_Extra.Far_Module_Extra_Info me = (Far_Module_Extra.Far_Module_Extra_Info)m.Extra.Module_Extras;
			me.Coarse_Tempo = ffh.Tempo;
			me.Fine_Tempo = 0;
			me.Tempo_Mode = 1;
			m.Time_Factor = Constants.Far_Time_Factor;

			Far_Module_Extra extras = (Far_Module_Extra)m.Extra;
			extras.LibXmp_Far_Translate_Tempo(1, 0, me.Coarse_Tempo, ref me.Fine_Tempo, ref mod.Spd, ref mod.Bpm);

			m.Period_Type = Containers.Common.Period.CSpd;
			m.C4Rate = Constants.C4_Ntsc_Rate;

			m.Quirk |= Quirk_Flag.VsAll | Quirk_Flag.PbAll | Quirk_Flag.VibAll;

			mod.Name = encoder.GetString(ffh.Name, 0, 40);
			lib.common.LibXmp_Set_Type(m, string.Format("Farandole Composer {0}.{1}", Ports.LibXmp.Common.Msn(ffh.Version), Ports.LibXmp.Common.Lsn(ffh.Version)));

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				return -1;

			// Read and convert patterns
			CPointer<uint8> patBuf = CMemory.MAlloc<uint8>(256 * 16 * 4);
			if (patBuf.IsNull)
				return -1;

			for (i = 0; i < mod.Pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern(mod, i) < 0)
					goto Err;

				if (ffh2.PatSize[i] == 0)
					continue;

				c_int rows = (ffh2.PatSize[i] - 2) / 64;

				// Sanity check
				if ((rows <= 0) || (rows > 256))
					goto Err;

				mod.Xxp[i].Rows = rows;

				if (lib.common.LibXmp_Alloc_Tracks_In_Pattern(mod, i) < 0)
					goto Err;

				uint8 brk = (uint8)(f.Hio_Read8() + 1);
				f.Hio_Read8();

				if (f.Hio_Read(patBuf, (size_t)rows * 64, 1) < 1)
					goto Err;

				CPointer<uint8> pos = patBuf;

				for (c_int j = 0; j < mod.Xxp[i].Rows; j++)
				{
					for (c_int k = 0; k < mod.Chn; k++)
					{
						Xmp_Event @event = Ports.LibXmp.Common.Event(m, i, k, j);

						if ((k == 0) && (j == brk))
							@event.F2T = Effects.Fx_Break;

						uint8 note = pos[0, 1];
						uint8 ins = pos[0, 1];
						uint8 vol = pos[0, 1];
						uint8 fxb = pos[0, 1];

						if (note != 0)
							@event.Note = (byte)(note + 48);

						if ((@event.Note != 0) || (ins != 0))
							@event.Ins = (byte)(ins + 1);

						if ((vol >= 0x01) && (vol <= 0x10))
							@event.Vol = (byte)((vol - 1) * 16 + 1);

						Far_Translate_Effect(@event, Ports.LibXmp.Common.Msn(fxb), Ports.LibXmp.Common.Lsn(fxb), vol);
					}
				}
			}

			CMemory.Free(patBuf);

			// Allocate tracks for any patterns referenced with a size of 0. These
			// use the configured pattern break position, which is 64 by default
			for (i = 0; i < mod.Len; i++)
			{
				c_int pat = mod.Xxo[i];

				if (mod.Xxp[pat].Rows == 0)
				{
					mod.Xxp[pat].Rows = 64;

					if (lib.common.LibXmp_Alloc_Tracks_In_Pattern(mod, pat) < 0)
						return -1;
				}
			}

			mod.Ins = -1;

			if (f.Hio_Read(sample_Map, 1, 8) < 8)
				return -1;

			for (i = 0; i < 64; i++)
			{
				if ((sample_Map[i / 8] & (1 << (i % 8))) != 0)
					mod.Ins = i;
			}

			mod.Ins++;

			mod.Smp = mod.Ins;

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			// Read and convert instruments and samples
			for (i = 0; i < mod.Ins; i++)
			{
				if ((sample_Map[i / 8] & (1 << (i % 8))) == 0)
					continue;

				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					return -1;

				f.Hio_Read(fih.Name, 32, 1);		// Instrument name
				fih.Length = f.Hio_Read32L();				// Length of sample (up to 64Kb)
				fih.FineTune = f.Hio_Read8();				// Finetune (unsupported)
				fih.Volume = f.Hio_Read8();					// Volume (unsupported?)
				fih.Loop_Start = f.Hio_Read32L();			// Loop start
				fih.LoopEnd = f.Hio_Read32L();				// Loop end
				fih.SampleType = f.Hio_Read8();				// 1=16 bit sample
				fih.LoopMode = f.Hio_Read8();

				// Sanity check
				if ((fih.Length > 0x10000) || (fih.Loop_Start > 0x10000) || (fih.LoopEnd > 0x10000))
					return -1;

				mod.Xxs[i].Len = (c_int)fih.Length;
				mod.Xxs[i].Lps = (c_int)fih.Loop_Start;
				mod.Xxs[i].Lpe = (c_int)fih.LoopEnd;
				mod.Xxs[i].Flg = Xmp_Sample_Flag.None;

				if (mod.Xxs[i].Len > 0)
					mod.Xxi[i].Nsm = 1;

				if (fih.SampleType != 0)
				{
					mod.Xxs[i].Flg |= Xmp_Sample_Flag._16Bit;
					mod.Xxs[i].Len >>= 1;
					mod.Xxs[i].Lps >>= 1;
					mod.Xxs[i].Lpe >>= 1;
				}

				mod.Xxs[i].Flg |= fih.LoopMode != 0 ? Xmp_Sample_Flag.Loop : 0;
				mod.Xxi[i].Sub[0].Vol = 0xff;
				mod.Xxi[i].Sub[0].Sid = i;

				lib.common.LibXmp_Instrument_Name(mod, i, fih.Name, 32, encoder);

				if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, mod.Xxs[i], null, i) < 0)
					return -1;
			}

			// Panning map
			for (i = 0; i < 16; i++)
			{
				if (ffh.Ch_On[i] == 0)
					mod.Xxc[i].Flg |= Xmp_Channel_Flag.Mute;

				if (ffh.Pan[i] < 0x10)
					mod.Xxc[i].Pan = (ffh.Pan[i] << 4) | ffh.Pan[i];
			}

			m.VolBase = 0xf0;

			return 0;

			Err:
			CMemory.Free(patBuf);
			return -1;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Far_Translate_Effect(Xmp_Event @event, c_int fx, c_int param, c_int vol)
		{
			switch (fx)
			{
				// 0x0? Global funct
				case 0x0:
				{
					switch (param)
					{
						// 0x01 Ramp delay on
						// 0x02 Ramp delay off
						case 0x1:
						case 0x2:
						{
							// These control volume ramping and can be ignored
							break;
						}

						// 0x03 Fulfill loop
						case 0x3:
						{
							// This is intended to be sustain release, but the
							// effect is buggy and just cuts most of the time
							@event.FxT = Effects.Fx_Keyoff;
							break;
						}

						// 0x04 Old FAR tempo
						case 0x4:
						{
							@event.FxT = Effects.Fx_Far_Tempo;
							@event.FxP = 0x10;
							break;
						}

						// 0x05 New FAR tempo
						case 0x5:
						{
							@event.FxT = Effects.Fx_Far_Tempo;
							@event.FxP = 0x20;
							break;
						}
					}
					break;
				}

				// 0x1? Pitch offset up
				case 0x1:
				{
					@event.FxT = Effects.Fx_Far_Porta_Up;
					@event.FxP = (byte)param;
					break;
				}

				// 0x2? Pitch offset down
				case 0x2:
				{
					@event.FxT = Effects.Fx_Far_Porta_Dn;
					@event.FxP = (byte)param;
					break;
				}

				// 0x3? Note-port
				case 0x3:
				{
					@event.FxT = Effects.Fx_Far_TPorta;
					@event.FxP = (byte)param;
					break;
				}

				// 0x4? Retrigger
				case 0x4:
				{
					@event.FxT = Effects.Fx_Far_Retrig;
					@event.FxP = (byte)param;
					break;
				}

				// 0x5? Set vibrato depth
				case 0x5:
				{
					@event.FxT = Effects.Fx_Far_VibDepth;
					@event.FxP = (byte)param;
					break;
				}

				// 0x6? Vibrato note
				case 0x6:
				{
					@event.FxT = Effects.Fx_Far_Vibrato;
					@event.FxP = (byte)param;
					break;
				}

				// 0x7? Vol sld up
				case 0x7:
				{
					@event.FxT = Effects.Fx_F_VSlide_Up;
					@event.FxP = (byte)(param << 4);
					break;
				}

				// 0x8? Vol sld down
				case 0x8:
				{
					@event.FxT = Effects.Fx_F_VSlide_Dn;
					@event.FxP = (byte)(param << 4);
					break;
				}

				// 0x9? Sustained vibrato
				case 0x9:
				{
					@event.FxT = Effects.Fx_Far_Vibrato;
					@event.FxP = (byte)(0x10 /* Vibrato sustain flag */ | param);
					break;
				}

				// 0xa? Slide-to-vol
				case 0xa:
				{
					if ((vol >= 0x01) && (vol <= 0x10))
					{
						@event.FxT = Effects.Fx_Far_SlideVol;
						@event.FxP = (byte)(((vol - 1) << 4) | param);
						@event.Vol = 0;
					}
					break;
				}

				// 0xb? Balance
				case 0xb:
				{
					@event.FxT = Effects.Fx_SetPan;
					@event.FxP = (byte)((param << 4) | param);
					break;
				}

				// 0xc? Note offset
				case 0xc:
				{
					@event.FxT = Effects.Fx_Far_Delay;
					@event.FxP = (byte)param;
					break;
				}

				// 0xd? Fine tempo down
				case 0xd:
				{
					@event.FxT = Effects.Fx_Far_F_Tempo;
					@event.FxP = (byte)param;
					break;
				}

				// 0xe? Fine tempo up
				case 0xe:
				{
					@event.FxT = Effects.Fx_Far_F_Tempo;
					@event.FxP = (byte)(param << 4);
					break;
				}

				// 0xf? Set tempo
				case 0xf:
				{
					@event.FxT = Effects.Fx_Far_Tempo;
					@event.FxP = (byte)param;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Far_Read_Text(CPointer<byte> dest, size_t textLen, Hio f)
		{
			// FAR module text uses 132-char lines with no line breaks...
			if (textLen > Comment_MaxLines * 132)
				textLen = Comment_MaxLines * 132;

			while (textLen != 0)
			{
				size_t end = Math.Min(textLen, 132);
				textLen -= end;
				end = f.Hio_Read(dest, 1, end);

				size_t lastChar = 0;

				for (size_t i = 0; i < end; i++)
				{
					// Nulls in the text area are equivalent to spaces
					if (dest[i] == 0x00)
						dest[i] = 0x20;
					else if (dest[i] != ' ')
						lastChar = i;
				}

				dest += lastChar + 1;
				dest[0, 1] = 0x0a;
			}

			dest[0] = 0x00;
		}
		#endregion
	}
}
