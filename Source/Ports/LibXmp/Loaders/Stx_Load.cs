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
	/// 
	/// </summary>
	internal class Stx_Load : IFormatLoader
	{
		#region Internal structures

		#region Stx_File_Header
		private class Stx_File_Header
		{
			public uint8[] Name = new uint8[20];		// Song name
			public uint8[] Magic = new uint8[8];		// !Scream!
			public uint16 PSize;						// Pattern 0 size?
			public uint16 Unknown1;						// ?!
			public uint16 PP_Pat;						// Pointer to pattern table
			public uint16 PP_Ins;						// Pattern to instrument table
			public uint16 PP_Chn;						// Pointer to channel table (?)
			public uint16 Unknown2;
			public uint16 Unknown3;
			public uint8 GVol;							// Global volume
			public uint8 Tempo;							// Playback tempo
			public uint16 Unknown4;
			public uint16 Unknown5;
			public uint16 PatNum;						// Number of patterns
			public uint16 InsNum;						// Number of instruments
			public uint16 OrdNum;						// Number of orders
			public uint16 Unknown6;						// Flags?
			public uint16 Unknown7;						// Version?
			public uint16 Unknown8;						// Ffi?
			public uint8[] Magic2 = new uint8[4];		// 'SCRM'
		}
		#endregion

		#region Stx_Instrument_Header
		private class Stx_Instrument_Header
		{
			public uint8 Type;							// Instrument type
			public uint8[] DosName = new uint8[13];		// DOS file name
			public uint16 MemSeg;						// Pointer to sample data
			public uint32 Length;						// Length
			public uint32 LoopBeg;						// Loop begin
			public uint32 LoopEnd;						// Loop end
			public uint8 Vol;							// Volume
			public uint8 Rsvd1;							// Reserved
			public uint8 Pack;							// Packing type (not used)
			public uint8 Flags;							// Loop/stereo/16bit samples flags
			public uint16 C2Spd;						// C 4 speed
			public uint16 Rsvd2;						// Reserved
			public uint8[] Rsvd3 = new uint8[4];		// Reserved
			public uint16 Int_Gp;						// Internal - GUS pointer
			public uint16 Int_512;						// Internal - SB pointer
			public uint32 Int_Last;						// Internal - SB index
			public uint8[] Name = new uint8[28];		// Instrument name
			public uint8[] Magic = new uint8[4];		// Reserved (for 'SCRS')
		}
		#endregion

		#endregion

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Stx = new Format_Loader
		{
			Id = Guid.Parse("BFFB2A7C-52D5-4492-9241-3AC2FD705D80"),
			Name = "Scream Tracker Music Interface Kit",
			Description = "This loader recognizes “STMIK 0.2” modules. “STMIK” (the Scream Tracker Music Interface Kit) was a module playing library distributed by Future Crew to play Scream Tracker module in games and demos. It uses an intermediate format between STM and S3M and comes with a tool converting STM modules to STX.\n\n“STMIK” was written by PSI of Future Crew, a.k.a. Sami Tammilehto.",
			Create = Create
		};

		private const uint8 Fx_None = 0xff;

		private static readonly uint8[] fx =
		[
			Fx_None,
			Effects.Fx_Speed,
			Effects.Fx_Jump,
			Effects.Fx_Break,
			Effects.Fx_VolSlide,
			Effects.Fx_Porta_Dn,
			Effects.Fx_Porta_Up,
			Effects.Fx_TonePorta,
			Effects.Fx_Vibrato,
			Effects.Fx_Tremor,
			Effects.Fx_Arpeggio
		];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Stx_Load(LibXmp libXmp)
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
			return new Stx_Load(libXmp);
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

			if ((CMemory.MemCmp(buf, "!Scream!", 8) != 0) && (CMemory.MemCmp(buf, "BMOD2STM", 8) != 0))
				return -1;

			f.Hio_Seek(start + 60, SeekOrigin.Begin);
			if (f.Hio_Read(buf, 1, 4) < 4)
				return -1;

			if (CMemory.MemCmp(buf, "SCRM", 4) != 0)
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
			bool broken = false;
			Xmp_Event dummy = new Xmp_Event();
			Stx_File_Header sfh = new Stx_File_Header();
			Stx_Instrument_Header sih = new Stx_Instrument_Header();
			bool bmod2stm = false;

			f.Hio_Read(sfh.Name, 20, 1);
			f.Hio_Read(sfh.Magic, 8, 1);
			sfh.PSize = f.Hio_Read16L();
			sfh.Unknown1 = f.Hio_Read16L();
			sfh.PP_Pat = f.Hio_Read16L();
			sfh.PP_Ins = f.Hio_Read16L();
			sfh.PP_Chn = f.Hio_Read16L();
			sfh.Unknown2 = f.Hio_Read16L();
			sfh.Unknown3 = f.Hio_Read16L();
			sfh.GVol = f.Hio_Read8();
			sfh.Tempo = f.Hio_Read8();
			sfh.Unknown4 = f.Hio_Read16L();
			sfh.Unknown5 = f.Hio_Read16L();
			sfh.PatNum = f.Hio_Read16L();
			sfh.InsNum = f.Hio_Read16L();
			sfh.OrdNum = f.Hio_Read16L();
			sfh.Unknown6 = f.Hio_Read16L();
			sfh.Unknown7 = f.Hio_Read16L();
			sfh.Unknown8 = f.Hio_Read16L();
			f.Hio_Read(sfh.Magic2, 4, 1);

			// Sanity check
			if ((sfh.PatNum > 254) || (sfh.InsNum > Constants.Max_Instruments) || (sfh.OrdNum > 256))
				return -1;

			// BMOD2STM does not convert pitch
			if (CMemory.StrNCmp(sfh.Magic, "BMOD2STM", 8) == 0)
				bmod2stm = true;

			mod.Ins = sfh.InsNum;
			mod.Pat = sfh.PatNum;
			mod.Trk = mod.Pat * mod.Chn;
			mod.Len = sfh.OrdNum;
			mod.Spd = Ports.LibXmp.Common.Msn(sfh.Tempo);
			mod.Smp = mod.Ins;
			m.C4Rate = Constants.C4_Ntsc_Rate;

			// STM2STX 1.0 released with STMIK 0.2 converts STMs with the pattern
			// length encoded in the first two bytes of the pattern (like S3M)
			f.Hio_Seek(start + (sfh.PP_Pat << 4), SeekOrigin.Begin);
			uint16 x16 = f.Hio_Read16L();
			f.Hio_Seek(start + (x16 << 4), SeekOrigin.Begin);
			x16 = f.Hio_Read16L();
			if (x16 == sfh.PSize)
				broken = true;

			lib.common.LibXmp_Copy_Adjust(out mod.Name, sfh.Name, 20, encoder);

			if (bmod2stm)
				lib.common.LibXmp_Set_Type(m, "BMOD2STM STX");
			else
				lib.common.LibXmp_Set_Type(m, string.Format("STM2STX 1.{0}", broken ? 0 : 1));

			CPointer<uint16> pp_Pat = CMemory.CAlloc<uint16>(mod.Pat);
			if (pp_Pat.IsNull)
				goto Err;

			CPointer<uint16> pp_Ins = CMemory.CAlloc<uint16>(mod.Ins);
			if (pp_Ins.IsNull)
				goto Err2;

			// Read pattern pointers
			f.Hio_Seek(start + (sfh.PP_Pat << 4), SeekOrigin.Begin);

			for (c_int i = 0; i < mod.Pat; i++)
				pp_Pat[i] = f.Hio_Read16L();

			// Read instrument pointers
			f.Hio_Seek(start + (sfh.PP_Ins << 4), SeekOrigin.Begin);

			for (c_int i = 0; i < mod.Ins; i++)
				pp_Ins[i] = f.Hio_Read16L();

			// Skip channel tabel (?)
			f.Hio_Seek(start + (sfh.PP_Chn << 4) + 32, SeekOrigin.Begin);

			// Read orders
			for (c_int i = 0; i < mod.Len; i++)
			{
				mod.Xxo[i] = f.Hio_Read8();
				f.Hio_Seek(4, SeekOrigin.Current);
			}

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				goto Err3;

			// Read and convert instruments and samples
			for (c_int i = 0; i < mod.Ins; i++)
			{
				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					goto Err3;

				f.Hio_Seek(start + (pp_Ins[i] << 4), SeekOrigin.Begin);

				sih.Type = f.Hio_Read8();
				f.Hio_Read(sih.DosName, 13, 1);
				sih.MemSeg = f.Hio_Read16L();
				sih.Length = f.Hio_Read32L();
				sih.LoopBeg = f.Hio_Read32L();
				sih.LoopEnd = f.Hio_Read32L();
				sih.Vol = f.Hio_Read8();
				sih.Rsvd1 = f.Hio_Read8();
				sih.Pack = f.Hio_Read8();
				sih.Flags = f.Hio_Read8();
				sih.C2Spd = f.Hio_Read16L();
				sih.Rsvd2 = f.Hio_Read16L();
				f.Hio_Read(sih.Rsvd3, 4, 1);
				sih.Int_Gp = f.Hio_Read16L();
				sih.Int_512 = f.Hio_Read16L();
				sih.Int_Last = f.Hio_Read32L();
				f.Hio_Read(sih.Name, 28, 1);
				f.Hio_Read(sih.Magic, 4, 1);

				if (f.Hio_Error() != 0)
					goto Err3;

				mod.Xxs[i].Len = (c_int)sih.Length;
				mod.Xxs[i].Lps = (c_int)sih.LoopBeg;
				mod.Xxs[i].Lpe = (c_int)sih.LoopEnd;

				if (mod.Xxs[i].Lpe == 0xffff)
					mod.Xxs[i].Lpe = 0;

				mod.Xxs[i].Flg = mod.Xxs[i].Lpe > 0 ? Xmp_Sample_Flag.Loop : Xmp_Sample_Flag.None;
				mod.Xxi[i].Sub[0].Vol = sih.Vol;
				mod.Xxi[i].Sub[0].Pan = 0x80;
				mod.Xxi[i].Sub[0].Sid = i;
				mod.Xxi[i].Nsm = 1;

				lib.common.LibXmp_Instrument_Name(mod, i, sih.Name, 12, encoder);

				lib.period.LibXmp_C2Spd_To_Note(sih.C2Spd, out mod.Xxi[i].Sub[0].Xpo, out mod.Xxi[i].Sub[0].Fin);
			}

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				goto Err3;

			// Read and convert pattern
			for (c_int i = 0; i < mod.Pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, 64) < 0)
					goto Err3;

				if (pp_Pat[i] == 0)
					continue;

				f.Hio_Seek(start + (pp_Pat[i] << 4), SeekOrigin.Begin);
				if (broken)
					f.Hio_Seek(2, SeekOrigin.Current);

				for (c_int r = 0; r < 64;)
				{
					uint8 b = f.Hio_Read8();
					if (f.Hio_Error() != 0)
						goto Err3;

					if (b == S3M_Load.S3M_Eor)
					{
						r++;
						continue;
					}

					c_int c = b & S3M_Load.S3M_Ch_Mask;
					Xmp_Event @event = c >= mod.Chn ? dummy : Ports.LibXmp.Common.Event(m, i, c, r);

					if ((b & S3M_Load.S3M_Ni_Follow) != 0)
					{
						uint8 n = f.Hio_Read8();

						switch (n)
						{
							case 255:
							{
								n = 0;
								break;		// Empty note
							}

							case 254:
							{
								n = Constants.Xmp_Key_Off;
								break;		// Key off
							}

							default:
							{
								n = (uint8)(37 + 12 * Ports.LibXmp.Common.Msn(n) + Ports.LibXmp.Common.Lsn(n));
								break;
							}
						}

						@event.Note = n;
						@event.Ins = f.Hio_Read8();
					}

					if ((b & S3M_Load.S3M_Vol_Follows) != 0)
						@event.Vol = (byte)(f.Hio_Read8() + 1);

					if ((b & S3M_Load.S3M_Fx_Follows) != 0)
					{
						c_int t = f.Hio_Read8();
						c_int p = f.Hio_Read8();

						if (t < fx.Length)
						{
							@event.FxT = fx[t];
							@event.FxP = (byte)p;

							switch (@event.FxT)
							{
								case Effects.Fx_Speed:
								{
									@event.FxP = Ports.LibXmp.Common.Msn(@event.FxP);
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
			}

			CMemory.Free(pp_Ins);
			CMemory.Free(pp_Pat);

			// Read samples
			for (c_int i = 0; i < mod.Ins; i++)
			{
				if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, mod.Xxs[i], null, i) < 0)
					goto Err;
			}

			m.Quirk |= Quirk_Flag.VsAll | Quirk_Flag.St3;
			m.Read_Event_Type = Read_Event.St3;

			return 0;

			Err3:
			CMemory.Free(pp_Ins);
			Err2:
			CMemory.Free(pp_Pat);
			Err:
			return -1;
		}
	}
}
