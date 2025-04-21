/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
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
	/// 
	/// </summary>
	internal class Ult_Load : IFormatLoader
	{
		#region Internal structures

		#region Ult_Header
		private class Ult_Header
		{
			public readonly uint8[] Magic = new uint8[15];		// 'MAS_UTrack_V00x'
			public readonly uint8[] Name = new uint8[32];		// Song name
			public uint8 MsgSize;								// ver < 1.4: zero
		}
		#endregion

		#region Ult_Header2
		private class Ult_Header2
		{
			public readonly uint8[] Order = new uint8[256];		// Orders
			public uint8 Channels;								// Number of channels - 1
			public uint8 Patterns;								// Number of patterns - 1
		}
		#endregion

		#region Ult_Instrument
		private class Ult_Instrument
		{
			public readonly uint8[] Name = new uint8[32];		// Instrument name
			public readonly uint8[] DosName = new uint8[12];	// DOS file name
			public uint32 Loop_Start;							// Loop start
			public uint32 LoopEnd;								// Loop end
			public uint32 SizeStart;							// Sample size is SizeEnd - SizeStart
			public uint32 SizeEnd;
			public uint8 Volume;								// Volume (log; ver >= 1.4 linear)
			public uint8 BidiLoop;								// Sample loop flags
			public int16 FineTune;								// Finetune
			public uint16 C2Spd;								// C2 frequency
		}
		#endregion

		#region Ult_Event
		private class Ult_Event
		{
			public uint8 Ins;
			public uint8 FxT;									// MSN = fxt, LSN = f2t
			public uint8 F2P;									// Secondary comes first -- little endian!
			public uint8 FxP;
		}
		#endregion

		#endregion

		private const c_int Comment_MaxLines = 256;

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Ult = new Format_Loader
		{
			Id = Guid.Parse("368009A5-E68F-4C33-8ACA-ED6B8448A2A6"),
			Name = "UltraTracker",
			Description = "This loader recognizes “UltraTracker” modules. They are mostly similar to Protracker modules, but support two effects per channel.\n\n“UltraTracker” was written by MAS of Prophecy, a.k.a. Marc Andre Schallehn, and released in 1993.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Ult_Load(LibXmp libXmp)
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
			return new Ult_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			CPointer<uint8> buf = new CPointer<uint8>(15);

			if (f.Hio_Read(buf, 1, 15) < 15)
				return -1;

			if (CMemory.MemCmp(buf, "MAS_UTrack_V00", 14) != 0)
				return -1;

			if ((buf[14] < '1') || (buf[14] > '4'))
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
			c_int i;
			Ult_Header ufh = new Ult_Header();
			Ult_Header2 ufh2 = new Ult_Header2();
			Ult_Instrument uih = new Ult_Instrument();
			Ult_Event ue = new Ult_Event();
			string[] verStr = [ "< 1.4", "1.4", "1.5", "1.6" ];

			f.Hio_Read(ufh.Magic, 15, 1);
			f.Hio_Read(ufh.Name, 32, 1);
			ufh.MsgSize = f.Hio_Read8();

			c_int ver = ufh.Magic[14] - '0';

			lib.common.LibXmp_Copy_Adjust(out mod.Name, ufh.Name, 32, encoder);
			lib.common.LibXmp_Set_Type(m, string.Format("UltraTracker {0} ULT V{1:d3}", verStr[ver - 1], ver));

			m.C4Rate = Constants.C4_Ntsc_Rate;

			if (ufh.MsgSize > 0)
			{
				uint8[] comment = new uint8[ufh.MsgSize * 33];
				Ult_Read_Text(comment, ufh.MsgSize * 32U, f);
				m.Comment = encoder.GetString(comment).Replace('\u25d9', '\n');
			}

			mod.Ins = mod.Smp = f.Hio_Read8();

			// Read and convert instruments
			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			for (i = 0; i < mod.Ins; i++)
			{
				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					return -1;

				f.Hio_Read(uih.Name, 32, 1);
				f.Hio_Read(uih.DosName, 12, 1);
				uih.Loop_Start = f.Hio_Read32L();
				uih.LoopEnd = f.Hio_Read32L();
				uih.SizeStart = f.Hio_Read32L();
				uih.SizeEnd = f.Hio_Read32L();
				uih.Volume = f.Hio_Read8();
				uih.BidiLoop = f.Hio_Read8();
				uih.C2Spd = (uint16)((ver >= 4) ? f.Hio_Read16L() : 0);	// Incorrect in ult_form.txt
				uih.FineTune = (int16)f.Hio_Read16L();

				if (f.Hio_Error() != 0)
					return -1;

				// Sanity check:
				// "[SizeStart] seems to tell UT how to load the sample into the GUS's
				// onboard memory." The maximum supported GUS RAM is 16 MB (PnP).
				// Samples also can't cross 256k boundaries. In practice it seems like
				// nothing ever goes over 1 MB, the maximum on most GUS cards
				if ((uih.SizeStart > uih.SizeEnd) || (uih.SizeEnd > (16 << 20)) || (uih.SizeEnd - uih.SizeStart > (256 << 10)))
					return -1;

				mod.Xxs[i].Len = (c_int)(uih.SizeEnd - uih.SizeStart);
				mod.Xxs[i].Lps = (c_int)uih.Loop_Start;
				mod.Xxs[i].Lpe = (c_int)uih.LoopEnd;

				if (mod.Xxs[i].Len > 0)
					mod.Xxi[i].Nsm = 1;

				// BiDi Loop : (Bidirectional Loop)
				//
				// UT takes advantage of the Gus's ability to loop a sample in
				// several different ways. By setting the Bidi Loop, the sample can
				// be played forward or backwards, looped or not looped. The Bidi
				// variable also tracks the sample resolution (8 or 16 bit).
				//
				// The following table shows the possible values of the Bidi Loop.
				// Bidi = 0  : No looping, forward playback,  8bit sample
				// Bidi = 4  : No Looping, forward playback, 16bit sample
				// Bidi = 8  : Loop Sample, forward playback, 8bit sample
				// Bidi = 12 : Loop Sample, forward playback, 16bit sample
				// Bidi = 24 : Loop Sample, reverse playback 8bit sample
				// Bidi = 28 : Loop Sample, reverse playback, 16bit sample

				switch (uih.BidiLoop)
				{
					case 20:		// Type 20 is in seasons.ult
					case 4:
					{
						mod.Xxs[i].Flg = Xmp_Sample_Flag._16Bit;
						break;
					}

					case 8:
					{
						mod.Xxs[i].Flg = Xmp_Sample_Flag.Loop;
						break;
					}

					case 12:
					{
						mod.Xxs[i].Flg = Xmp_Sample_Flag._16Bit | Xmp_Sample_Flag.Loop;
						break;
					}

					case 24:
					{
						mod.Xxs[i].Flg = Xmp_Sample_Flag.Loop | Xmp_Sample_Flag.Loop_Reverse;
						break;
					}

					case 28:
					{
						mod.Xxs[i].Flg = Xmp_Sample_Flag._16Bit | Xmp_Sample_Flag.Loop | Xmp_Sample_Flag.Loop_Reverse;
						break;
					}
				}

				// TODO: Add logarithmic volume support
				mod.Xxi[i].Sub[0].Vol = uih.Volume;
				mod.Xxi[i].Sub[0].Pan = 0x80;
				mod.Xxi[i].Sub[0].Sid = i;

				lib.common.LibXmp_Instrument_Name(mod, i, uih.Name, 24, encoder);

				if (ver > 3)
					lib.period.LibXmp_C2Spd_To_Note(uih.C2Spd, out mod.Xxi[i].Sub[0].Xpo, out mod.Xxi[i].Sub[0].Fin);
			}

			f.Hio_Read(ufh2.Order, 256, 1);
			ufh2.Channels = f.Hio_Read8();
			ufh2.Patterns = f.Hio_Read8();

			if (f.Hio_Error() != 0)
				return -1;

			for (i = 0; i < 256; i++)
			{
				if (ufh2.Order[i] == 0xff)
					break;

				mod.Xxo[i] = ufh2.Order[i];
			}

			mod.Len = i;
			mod.Chn = ufh2.Channels + 1;
			mod.Pat = ufh2.Patterns + 1;
			mod.Spd = 6;
			mod.Bpm = 125;
			mod.Trk = mod.Chn * mod.Pat;

			// Sanity check
			if (mod.Chn > Constants.Xmp_Max_Channels)
				return -1;

			for (i = 0; i < mod.Chn; i++)
			{
				if (ver >= 3)
				{
					uint8 x8 = f.Hio_Read8();
					mod.Xxc[i].Pan = 255 * x8 / 15;
				}
				else
					mod.Xxc[i].Pan = Common.DefPan(m, (((i + 1) / 2) % 2) * 0xff);	// ???
			}

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				return -1;

			// Read and convert patterns

			// Events are stored by channel
			for (i = 0; i < mod.Pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, 64) < 0)
					return -1;
			}

			for (i = 0; i < mod.Chn; i++)
			{
				for (c_int j = 0; j < 64 * mod.Pat;)
				{
					c_int cnt = 1;
					uint8 x8 = f.Hio_Read8();		// Read note or repeat code (0xfc)

					if (x8 == 0xfc)
					{
						cnt = f.Hio_Read8();		// Read repeat count
						x8 = f.Hio_Read8();			// Read note
					}

					ue.Ins = f.Hio_Read8();
					ue.FxT = f.Hio_Read8();
					ue.F2P = f.Hio_Read8();
					ue.FxP = f.Hio_Read8();

					if (f.Hio_Error() != 0)
						return -1;

					if (cnt == 0)
						cnt++;

					if ((j + cnt) > (64 * mod.Pat))
						return -1;

					for (c_int k = 0; k < cnt; k++, j++)
					{
						Xmp_Event @event = Ports.LibXmp.Common.Event(m, j >> 6, i, j & 0x3f);
						@event.Clear();

						if (x8 != 0)
							@event.Note = (byte)(x8 + 36);

						@event.Ins = ue.Ins;
						@event.FxT = Ports.LibXmp.Common.Msn(ue.FxT);
						@event.F2T = Ports.LibXmp.Common.Lsn(ue.FxT);
						@event.FxP = ue.FxP;
						@event.F2P = ue.F2P;

						Ult_Translate_Effect(ref @event.FxT, ref @event.FxP);
						Ult_Translate_Effect(ref @event.F2T, ref @event.F2P);
					}
				}
			}

			for (i = 0; i < mod.Ins; i++)
			{
				if (mod.Xxs[i].Len == 0)
					continue;

				if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, mod.Xxs[i], null, i) < 0)
					return -1;
			}

			m.VolBase = 0x100;

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Ult_Translate_Effect(ref uint8 fxT, ref uint8 fxP)
		{
			switch (fxT)
			{
				// Tone portamento
				case 0x03:
				{
					fxT = Effects.Fx_Ult_TPorta;
					break;
				}

				// 'Special' effect
				// Reserved
				case 0x05:
				case 0x06:
				{
					fxT = fxP = 0;
					break;
				}

				// Pan
				case 0x0b:
				{
					fxT = Effects.Fx_SetPan;
					fxP <<= 4;
					break;
				}

				// Sample offset
				case 0x09:
				{
					// TODO: fine sample offset (requires new effect or 2 more effect lanes)
					fxP <<= 2;
					break;
				}

				// Speed/BPM
				// 00:    Default speed (6)/BPM (125)
				// 01-2f: Set speed
				// 30-ff: Set BPM (CIA compatible)
				case 0x0f:
				{
					fxT = Effects.Fx_Ult_Tempo;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Ult_Read_Text(CPointer<byte> dest, size_t textLen, Hio f)
		{
			// ULT module text uses 32-char lines with no line breaks...
			if (textLen > Comment_MaxLines * 32)
				textLen = Comment_MaxLines * 32;

			while (textLen != 0)
			{
				size_t end = Math.Min(textLen, 32);
				textLen -= end;
				end = f.Hio_Read(dest, 1, end);

				size_t lastChar = 0;

				for (size_t i = 0; i < end; i++)
				{
					// Nulls in the text area are equivalent to spaces
					if (dest[i] == 0x00)
						dest[i] = 0x20;
					else if (dest[i] != 0x20)
						lastChar = i;
				}

				dest += lastChar + 1;
				dest[0, 1] = 0x0a;
			}

			if (dest.Length > 0)
				dest[0] = 0x00;
		}
		#endregion
	}
}
