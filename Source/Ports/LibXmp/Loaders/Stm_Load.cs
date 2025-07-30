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
	internal class Stm_Load : IFormatLoader
	{
		#region Internal structures

		#region Stm_Instrument_Header
		private class Stm_Instrument_Header
		{
			public uint8[] Name { get; } = new uint8[12];		// ASCIIZ instrument name
			public uint8 Id { get; set; }						// Id=0
			public uint8 IDisk { get; set; }					// Instrument disk
			public uint16 Rsvd1 { get; set; }					// Reserved
			public uint16 Length { get; set; }					// Sample length
			public uint16 LoopBeg { get; set; }					// Loop begin
			public uint16 LoopEnd { get; set; }					// Loop end
			public uint8 Volume { get; set; }					// Playback volume
			public uint8 Rsvd2 { get; set; }					// Reserved
			public uint16 C2Spd { get; set; }					// C4 speed
			public uint32 Rsvd3 { get; set; }					// Reserved
			public uint16 ParaLen { get; set; }					// Length in paragraphs
		}
		#endregion

		#region Stm_File_SubHeader_V1
		/// <summary>
		/// V1 format header based on disassembled ST2
		/// </summary>
		private class Stm_File_SubHeader_V1
		{
			public uint16 InsNum { get; set; }					// Number of instruments
			public uint16 OrdNum { get; set; }					// Number of orders
			public uint16 PatNum { get; set; }					// Number of patterns
			public uint16 SRate { get; set; }					// Sample rate?
			public uint8 Tempo { get; set; }					// Playback tempo
			public uint8 Channels { get; set; }					// Number of channels
			public uint16 PSize { get; set; }					// Pattern size
			public uint16 Rsvd2 { get; set; }					// Reserved
			public uint16 Skip { get; set; }					// Bytes to skip
		}
		#endregion

		#region Stm_File_SubHeader_V2
		private class Stm_File_SubHeader_V2
		{
			public uint8 Tempo { get; set; }					// Playback tempo
			public uint8 Patterns { get; set; }					// Number of patterns
			public uint8 GVol { get; set; }						// Global volume
			public uint8[] Rsvd2 { get; } = new uint8[13];		// Reserved
		}
		#endregion

		#region Stm_File_Header
		private class Stm_File_Header
		{
			public uint8[] Name { get; } = new uint8[20];		// ASCIIZ song name
			public uint8[] Magic { get; } = new uint8[8];		// '!Scream!'
			public uint8 Rsvd1 { get; set; }					// '\x1a'
			public uint8 Type { get; set; }						// 1=song, 2=module
			public uint8 VerMaj { get; set; }					// Major version number
			public uint8 VerMin { get; set; }					// Minor version number
			// ReSharper disable once InconsistentNaming
			public (
				Stm_File_SubHeader_V1 V1,
				Stm_File_SubHeader_V2 V2
			) Sub;
			public Stm_Instrument_Header[] Ins { get; } = ArrayHelper.InitializeArray<Stm_Instrument_Header>(32);
		}
		#endregion

		#endregion

		private const c_int Stm_Type_Song = 0x01;
		private const c_int Stm_Type_Module = 0x02;

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Stm = new Format_Loader
		{
			Id = Guid.Parse("557C8681-93CF-4BB8-A3DD-632C55DD840A"),
			Name = "Scream Tracker 2",
			Description = "This loader recognizes “Scream Tracker” modules. “Scream Tracker” was the first PC tracker, as well as the first PC module format. Loosely inspired by the “SoundTracker” format, it does not have as many effects as Protracker, although it supports 31 instruments and 4 channels.\n\n“Scream Tracker” was written by PSI of Future Crew, a.k.a. Sami Tammilehto.",
			Create = Create
		};

		private const uint8 Fx_None = 0xff;

		// Skaven's note from http://www.futurecrew.com/skaven/oldies_music.html
		//
		// FYI for the tech-heads: In the old Scream Tracker 2 the Arpeggio command
		// (Jxx), if used in a single row with a 0x value, caused the note to skip
		// the specified amount of halftones upwards halfway through the row. I used
		// this in some songs to give the lead some character. However, when played
		// in ModPlug Tracker, this effect doesn't work the way it did back then
		private static readonly uint8[] fx =
		[
			Fx_None,
			Effects.Fx_Speed,			// A - Set tempo to [INFO]. 60 normal
			Effects.Fx_Jump,			// B - Break pattern and jmp to order [INFO]
			Effects.Fx_Break,			// C - Break pattern
			Effects.Fx_VolSlide,		// D - Slide volume; Hi-nibble=up, Lo-nibble=down
			Effects.Fx_Porta_Dn,		// E - Slide down at speed [INFO]
			Effects.Fx_Porta_Up,		// F - Slide up at speed [INFO]
			Effects.Fx_TonePorta,		// G - Slide to the note specified at speed [INFO]
			Effects.Fx_Vibrato,			// H - Vibrato; Hi-nibble, speed. Lo-nibble, size
			Effects.Fx_Tremor,			// I - Tremor; Hi-nibble, ontime. Lo-nibble, offtime
			Effects.Fx_Arpeggio,		// J - Arpeggio
			Fx_None,
			Fx_None,
			Fx_None,
			Fx_None,
			Fx_None
		];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Stm_Load(LibXmp libXmp)
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
			return new Stm_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			CPointer<uint8> buf = new CPointer<uint8>(8);

			f.Hio_Seek(start + 20, SeekOrigin.Begin);
			if (f.Hio_Read(buf, 1, 8) < 8)
				return -1;

			if (lib.common.LibXmp_Test_Name(buf, 8, Test_Name.None) != 0)		// Tracker name should be ASCII
				return -1;

			// EOF should be 0x1a. Putup10.stm and putup11.stm have 2 instead
			buf[0] = f.Hio_Read8();
			if ((buf[0] != 0x1a) && (buf[0] != 0x02))
				return -1;

			if (f.Hio_Read8() > Stm_Type_Module)
				return -1;

			buf[0] = f.Hio_Read8();
			buf[1] = f.Hio_Read8();
			uint16 version = (uint16)((100 * buf[0]) + buf[1]);

			if ((version != 110) && (version != 200) && (version != 210) && (version != 220) && (version != 221))
				return -1;

			f.Hio_Seek(start + 60, SeekOrigin.Begin);
			if (f.Hio_Read(buf, 1, 4) < 4)
				return -1;

			if (CMemory.MemCmp(buf, "SCRM", 4) == 0)	// We don't want STX files
				return -1;

			f.Hio_Seek(start + 0, SeekOrigin.Begin);
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
			Stm_File_Header sfh = new Stm_File_Header();
			bool blank_Pattern = false;
			c_int i;

			f.Hio_Read(sfh.Name, 20, 1);		// ASCIIZ song name
			f.Hio_Read(sfh.Magic, 8, 1);		// '!Scream!'
			sfh.Rsvd1 = f.Hio_Read8();					// '\x1a'
			sfh.Type = f.Hio_Read8();					// 1=song, 2=module
			sfh.VerMaj = f.Hio_Read8();					// Major version number
			sfh.VerMin = f.Hio_Read8();					// Minor version number

			uint16 version = (uint16)((100 * sfh.VerMaj) + sfh.VerMin);
			if ((version != 110) && (version != 200) && (version != 210) && (version != 220) && (version != 221))
				return -1;

			// TODO: Improve robustness of the loader against bad parameters

			if (version >= 200)
			{
				sfh.Sub.V2 = new Stm_File_SubHeader_V2();

				sfh.Sub.V2.Tempo = f.Hio_Read8();		// Playback tempo
				sfh.Sub.V2.Patterns = f.Hio_Read8();	// Number of patterns
				sfh.Sub.V2.GVol = f.Hio_Read8();		// Global volume
				f.Hio_Read(sfh.Sub.V2.Rsvd2, 13, 1);	// Reserved

				mod.Chn = 4;
				mod.Pat = sfh.Sub.V2.Patterns;
				mod.Spd = (version < 221) ? Ports.LibXmp.Common.Lsn(sfh.Sub.V2.Tempo / 10) : Ports.LibXmp.Common.Msn(sfh.Sub.V2.Tempo);
				mod.Ins = 31;
				mod.Len = (version == 200) ? 64 : 128;
			}
			else
			{
				sfh.Sub.V1 = new Stm_File_SubHeader_V1();

				sfh.Sub.V1.InsNum = f.Hio_Read16L();	// Number of instruments
				if (sfh.Sub.V1.InsNum > 32)
					return -1;

				sfh.Sub.V1.OrdNum = f.Hio_Read16L();	// Number of orders
				if (sfh.Sub.V1.OrdNum > Constants.Xmp_Max_Mod_Length)
					return -1;

				sfh.Sub.V1.PatNum = f.Hio_Read16L();	// Number of patterns
				if (sfh.Sub.V1.PatNum > Constants.Xmp_Max_Mod_Length)
					return -1;

				sfh.Sub.V1.SRate = f.Hio_Read16L();		// Sample rate?
				sfh.Sub.V1.Tempo = f.Hio_Read8();		// Playback tempo

				sfh.Sub.V1.Channels = f.Hio_Read8();	// Number of channels
				if (sfh.Sub.V1.Channels != 4)
					return -1;

				sfh.Sub.V1.PSize = f.Hio_Read16L();		// Pattern size
				if (sfh.Sub.V1.PSize != 64)
					return -1;

				sfh.Sub.V1.Rsvd2 = f.Hio_Read16L();		// Reserved
				sfh.Sub.V1.Skip = f.Hio_Read16L();		// Bytes to skip
				f.Hio_Seek(sfh.Sub.V1.Skip, SeekOrigin.Current);	// Skip bytes

				mod.Chn = sfh.Sub.V1.Channels;
				mod.Pat = sfh.Sub.V1.PatNum;
				mod.Spd = (version != 100) ? Ports.LibXmp.Common.Lsn(sfh.Sub.V1.Tempo / 10) : Ports.LibXmp.Common.Lsn(sfh.Sub.V1.Tempo);
				mod.Ins = sfh.Sub.V1.InsNum;
				mod.Len = sfh.Sub.V1.OrdNum;
			}

			for (i = 0; i < mod.Ins; i++)
			{
				f.Hio_Read(sfh.Ins[i].Name, 12, 1);	// Instrument name
				sfh.Ins[i].Id = f.Hio_Read8();					// Id=0
				sfh.Ins[i].IDisk = f.Hio_Read8();				// Instrument disk
				sfh.Ins[i].Rsvd1 = f.Hio_Read16L();				// Reserved
				sfh.Ins[i].Length = f.Hio_Read16L();			// Sample length
				sfh.Ins[i].LoopBeg = f.Hio_Read16L();			// Loop begin
				sfh.Ins[i].LoopEnd = f.Hio_Read16L();			// Loop end
				sfh.Ins[i].Volume = f.Hio_Read8();				// Playback volume
				sfh.Ins[i].Rsvd2 = f.Hio_Read8();				// Reserved
				sfh.Ins[i].C2Spd = f.Hio_Read16L();				// C4 speed
				sfh.Ins[i].Rsvd3 = f.Hio_Read32L();				// Reserved
				sfh.Ins[i].ParaLen = f.Hio_Read16L();			// Length in paragraphs
			}

			if (f.Hio_Error() != 0)
				return -1;

			mod.Smp = mod.Ins;
			m.C4Rate = Constants.C4_Ntsc_Rate;

			lib.common.LibXmp_Copy_Adjust(out mod.Name, sfh.Name, 20, encoder);

			if ((sfh.Magic[0] == 0) || (CMemory.StrNCmp(sfh.Magic, "PCSTV", 5) == 0) || (CMemory.StrNCmp(sfh.Magic, "!Scream!", 8) == 0))
				lib.common.LibXmp_Set_Type(m, string.Format("Scream Tracker {0}.{1:D2}", sfh.VerMaj, sfh.VerMin));
			else if (CMemory.StrNCmp(sfh.Magic, "SWavePro", 8) == 0)
				lib.common.LibXmp_Set_Type(m, string.Format("SoundWave Pro {0}.{1:D2}", sfh.VerMaj, sfh.VerMin));
			else
				lib.common.LibXmp_Copy_Adjust(out mod.Type, sfh.Magic, 8, encoder);

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			// Read and convert instruments and samples
			for (i = 0; i < mod.Ins; i++)
			{
				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					return -1;

				mod.Xxs[i].Len = sfh.Ins[i].Length;
				mod.Xxs[i].Lps = sfh.Ins[i].LoopBeg;
				mod.Xxs[i].Lpe = sfh.Ins[i].LoopEnd;

				if (mod.Xxs[i].Lpe == 0xffff)
					mod.Xxs[i].Lpe = 0;

				mod.Xxs[i].Flg = mod.Xxs[i].Lpe > 0 ? Xmp_Sample_Flag.Loop : Xmp_Sample_Flag.None;
				mod.Xxi[i].Sub[0].Vol = sfh.Ins[i].Volume;
				mod.Xxi[i].Sub[0].Pan = 0x80;
				mod.Xxi[i].Sub[0].Sid = i;

				if (mod.Xxs[i].Len > 0)
					mod.Xxi[i].Nsm = 1;

				lib.common.LibXmp_Instrument_Name(mod, i, sfh.Ins[i].Name, 12, encoder);

				lib.period.LibXmp_C2Spd_To_Note(sfh.Ins[i].C2Spd, out mod.Xxi[i].Sub[0].Xpo, out mod.Xxi[i].Sub[0].Fin);
			}

			if (f.Hio_Read(mod.Xxo, 1, (size_t)mod.Len) < (size_t)mod.Len)
				return -1;

			for (i = 0; i < mod.Len; i++)
			{
				if (mod.Xxo[i] >= 99)
					break;

				// Patterns >= the pattern count are valid blank patterns.
				// Examples: jimmy.stm, Rauno/dogs.stm, Skaven/hevijanis istu maas.stm.
				// Patterns >= 64 have undefined behavior in Screamtracker 2.
				if (mod.Xxo[i] >= mod.Pat)
				{
					mod.Xxo[i] = (byte)mod.Pat;
					blank_Pattern = true;
				}
			}

			c_int stored_Patterns = mod.Pat;
			if (blank_Pattern)
				mod.Pat++;

			mod.Trk = mod.Pat * mod.Chn;
			mod.Len = i;

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				return -1;

			// Read and convert patterns
			if (blank_Pattern)
			{
				if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, stored_Patterns, 64) < 0)
					return -1;
			}

			for (i = 0; i < stored_Patterns; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, 64) < 0)
					return -1;

				if (f.Hio_Error() != 0)
					return -1;

				for (c_int j = 0; j < 64; j++)
				{
					for (c_int k = 0; k < mod.Chn; k++)
					{
						Xmp_Event @event = Ports.LibXmp.Common.Event(m, i, k, j);

						uint8 b = f.Hio_Read8();
						if ((b == 251) || (b == 252))
							continue;		// Empty note

						if (b == 253)
						{
							@event.Note = Constants.Xmp_Key_Off;
							continue;		// Key off
						}

						if (b == 254)
							@event.Note = Constants.Xmp_Key_Off;
						else if (b == 255)
							@event.Note = 0;
						else
							@event.Note = (byte)(1 + Ports.LibXmp.Common.Lsn(b) + 12 * (3 + Ports.LibXmp.Common.Msn(b)));

						b = f.Hio_Read8();
						@event.Vol = (byte)(b & 0x07);
						@event.Ins = (byte)((b & 0xf8) >> 3);

						b = f.Hio_Read8();
						@event.Vol += (byte)((b & 0xf0) >> 1);

						if (version >= 200)
							@event.Vol = (byte)((@event.Vol > 0x40) ? 0 : @event.Vol + 1);
						else
						{
							if (@event.Vol > 0)
								@event.Vol = (byte)((@event.Vol > 0x40) ? 1 : @event.Vol + 1);
						}

						@event.FxT = fx[Ports.LibXmp.Common.Lsn(b)];
						@event.FxP = f.Hio_Read8();

						switch (@event.FxT)
						{
							case Effects.Fx_Speed:
							{
								@event.FxP = (byte)((version < 221) ? Ports.LibXmp.Common.Lsn(@event.FxP / 10) : Ports.LibXmp.Common.Msn(@event.FxP));
								break;
							}

							case Fx_None:
							{
								@event.FxP = @event.FxT = 0;
								break;
							}
						}
					}
				}
			}

			// Read samples
			for (i = 0; i < mod.Ins; i++)
			{
				if ((sfh.Ins[i].Volume == 0) || (sfh.Ins[i].Length == 0))
				{
					mod.Xxi[i].Nsm = 0;
					continue;
				}

				if (sfh.Type == Stm_Type_Song)
				{
					string instName = mod.Xxi[i].Name;

					if (string.IsNullOrEmpty(instName) || string.IsNullOrEmpty(m.DirName))
						continue;

					throw new NotImplementedException("Song format not supported");
				}
				else
				{
					f.Hio_Seek(start + (sfh.Ins[i].Rsvd1 << 4), SeekOrigin.Begin);

					if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, mod.Xxs[i], null, i) < 0)
						return -1;
				}
			}

			m.Quirk |= Quirk_Flag.VsAll | Quirk_Flag.St3;
			m.Read_Event_Type = Read_Event.St3;

			return 0;
		}
	}
}
