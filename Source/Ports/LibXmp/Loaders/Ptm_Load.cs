/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.C;
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
	internal class Ptm_Load : IFormatLoader
	{
		#region Internal structures

		#region Ptm_File_Header
		private class Ptm_File_Header
		{
			public uint8[] Name { get; } = new uint8[28];		// Song name
			public uint8 DosEof { get; set; }					// 0x1a
			public uint8 VerMin { get; set; }					// Minor version
			public uint8 VerMaj { get; set; }					// Major version
			public uint8 Rsvd1 { get; set; }					// Reserved
			public uint16 OrdNum { get; set; }					// Number of orders (must be even)
			public uint16 InsNum { get; set; }					// Number of instruments
			public uint16 PatNum { get; set; }					// Number of patterns
			public uint16 ChnNum { get; set; }					// Number of channels
			public uint16 Flags { get; set; }					// Flags (set to 0)
			public uint16 Rsvd2 { get; set; }					// Reserved
			public uint32 Magic { get; set; }					// 'PTMF'
			public uint8[] Rsvd3 { get; } = new uint8[16];		// Reserved
			public uint8[] ChSet { get; } = new uint8[32];		// Channel settings
			public uint8[] Order { get; } = new uint8[256];		// Orders
			public uint16[] PatSeg { get; } = new uint16[128];
		}
		#endregion

		#region Ptm_Instrument_Header
		private class Ptm_Instrument_Header
		{
			public uint8 Type { get; set; }						// Sample type
			public uint8[] DosName { get; } = new uint8[12];	// DOS file name
			public uint8 Vol { get; set; }						// Volume
			public uint16 C4Spd { get; set; }					// C4 speed
			public uint16 SmpSeg { get; set; }					// Sample segment (not used)
			public uint32 SmpOfs { get; set; }					// Sample offset
			public uint32 Length { get; set; }					// Length
			public uint32 LoopBeg { get; set; }					// Loop begin
			public uint32 LoopEnd { get; set; }					// Loop end
			public uint32 GusBeg { get; set; }					// GUS begin address
			public uint32 GusLps { get; set; }					// GUS loop start address
			public uint32 GusLpe { get; set; }					// GUS loop end address
			public uint8 GusFlg { get; set; }					// GUS loop flag
			public uint8 Rsvd1 { get; set; }					// Reserved
			public uint8[] Name { get; } = new uint8[28];		// Instrument name
			public uint32 Magic { get; set; }					// 'PTMS'
		}
		#endregion

		#endregion

		#region Tables
		private static readonly c_int[] Ptm_Vol =
		[
			0, 5, 8, 10, 12, 14, 15, 17, 18, 20, 21, 22, 23, 25, 26,
			27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 37, 38, 39, 40,
			41, 42, 42, 43, 44, 45, 46, 46, 47, 48, 49, 49, 50, 51, 51,
			52, 53, 54, 54, 55, 56, 56, 57, 58, 58, 59, 59, 60, 61, 61,
			62, 63, 63, 64, 64
		];
		#endregion

		// ReSharper disable InconsistentNaming
		private static readonly uint32 Magic_PTMF = Common.Magic4('P', 'T', 'M', 'F');

		private const uint8 Ptm_Ch_Mask = 0x1f;
		private const uint8 Ptm_Ni_Follow = 0x20;
		private const uint8 Ptm_Vol_Follows = 0x80;
		private const uint8 Ptm_Fx_Follows = 0x40;

		private const uint8 Ptm_Ins_None = 0;
		private const uint8 Ptm_Ins_Sample = 1;
		private const uint8 Ptm_Ins_Opl = 2;	// Unused
		private const uint8 Ptm_Ins_Midi = 3;	// Unused
		private const uint8 Ptm_Ins_Loop = 1 << 2;
		private const uint8 Ptm_Ins_Loop_BiDir = 1 << 3;
		private const uint8 Ptm_Ins_Loop_16Bit = 1 << 4;
		// ReSharper restore InconsistentNaming

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Ptm = new Format_Loader
		{
			Id = Guid.Parse("270E60B6-9504-4DE2-9703-5A1D68CEC800"),
			Name = "Poly Tracker",
			Description = "Poly Tracker was written by Lone Ranger of AcmE. It is based on the Scream Tracker 3 format with some changes where needed to fit Poly Tracker needs.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Ptm_Load(LibXmp libXmp)
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
			return new Ptm_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			f.Hio_Seek(start + 44, SeekOrigin.Begin);
			if (f.Hio_Read32B() != Magic_PTMF)
			{
				t = null;
				return -1;
			}

			f.Hio_Seek(start + 0, SeekOrigin.Begin);
			lib.common.LibXmp_Read_Title(f, out t, 28, encoder);

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
			c_int[] smp_Ofs = new c_int[256];
			Ptm_File_Header pfh = new Ptm_File_Header();
			Ptm_Instrument_Header pih = new Ptm_Instrument_Header();

			// Load and convert header
			f.Hio_Read(pfh.Name, 28, 1);			// Song name
			pfh.DosEof = f.Hio_Read8();						// 0x1a
			pfh.VerMin = f.Hio_Read8();						// Minor version
			pfh.VerMaj = f.Hio_Read8();						// Major version
			pfh.Rsvd1 = f.Hio_Read8();						// Reserved
			pfh.OrdNum = f.Hio_Read16L();					// Number of orders (must be even)
			pfh.InsNum = f.Hio_Read16L();					// Number of instruments
			pfh.PatNum = f.Hio_Read16L();					// Number of patterns
			pfh.ChnNum = f.Hio_Read16L();					// Number of channels
			pfh.Flags = f.Hio_Read16L();					// Flags (set to 0)
			pfh.Rsvd2 = f.Hio_Read16L();					// Reserved
			pfh.Magic = f.Hio_Read32B();					// 'PTMF'

			if (pfh.Magic != Magic_PTMF)
				return -1;

			// Sanity check
			if ((pfh.OrdNum > 256) || (pfh.InsNum > 255) || (pfh.PatNum > 128) || (pfh.ChnNum > 32))
				return -1;

			f.Hio_Read(pfh.Rsvd3, 16, 1);		// Reserved
			f.Hio_Read(pfh.ChSet, 32, 1);		// Channel settings
			f.Hio_Read(pfh.Order, 256, 1);		// Orders

			for (c_int i = 0; i < 128; i++)
				pfh.PatSeg[i] = f.Hio_Read16L();

			if (f.Hio_Error() != 0)
				return -1;

			mod.Len = pfh.OrdNum;
			mod.Ins = pfh.InsNum;
			mod.Pat = pfh.PatNum;
			mod.Chn = pfh.ChnNum;
			mod.Trk = mod.Pat * mod.Chn;
			mod.Smp = mod.Ins;
			mod.Spd = 6;
			mod.Bpm = 125;
			CMemory.memcpy<uint8>(mod.Xxo, pfh.Order, 256);

			m.C4Rate = Constants.C4_Ntsc_Rate;

			lib.common.LibXmp_Copy_Adjust(out mod.Name, pfh.Name, 28, encoder);
			lib.common.LibXmp_Set_Type(m, $"Poly Tracker PTM {pfh.VerMaj}.{pfh.VerMin:x2}");

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			// Read and convert instrument and samples
			for (c_int i = 0; i < mod.Ins; i++)
			{
				Xmp_Instrument xxi = mod.Xxi[i];
				Xmp_Sample xxs = mod.Xxs[i];

				pih.Type = f.Hio_Read8();					// Sample type
				f.Hio_Read(pih.DosName, 12, 1);	// DOS file name
				pih.Vol = f.Hio_Read8();					// Volume
				pih.C4Spd = f.Hio_Read16L();				// C4 speed
				pih.SmpSeg = f.Hio_Read16L();				// Sample segment (not used)
				pih.SmpOfs = f.Hio_Read32L();				// Sample offset
				pih.Length = f.Hio_Read32L();				// Length
				pih.LoopBeg = f.Hio_Read32L();				// Loop begin
				pih.LoopEnd = f.Hio_Read32L();				// Loop end
				pih.GusBeg = f.Hio_Read32L();				// GUS begin address
				pih.GusLps = f.Hio_Read32L();				// GUS loop start address
				pih.GusLpe = f.Hio_Read32L();				// GUS loop end address
				pih.GusFlg = f.Hio_Read8();					// GUS loop flag
				pih.Rsvd1 = f.Hio_Read8();					// Reserved
				f.Hio_Read(pih.Name, 28, 1);		// Instrument name
				pih.Magic = f.Hio_Read32B();				// 'PTMS'

				if (f.Hio_Error() != 0)
					return -1;

				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					return -1;

				Xmp_SubInstrument sub = xxi.Sub[0];

				smp_Ofs[i] = (c_int)pih.SmpOfs;
				xxs.Len = (c_int)pih.Length;
				xxs.Lps = (c_int)pih.LoopBeg;
				xxs.Lpe = (c_int)pih.LoopEnd;

				if ((mod.Xxs[i].Len > 0) && (Ptm_Ins_Type(pih.Type) == Ptm_Ins_Sample))
					mod.Xxi[i].Nsm = 1;

				xxs.Flg = Xmp_Sample_Flag.None;

				if ((pih.Type & Ptm_Ins_Loop) != 0)
					xxs.Flg |= Xmp_Sample_Flag.Loop;

				if ((pih.Type & Ptm_Ins_Loop_BiDir) != 0)
					xxs.Flg |= Xmp_Sample_Flag.Loop | Xmp_Sample_Flag.Loop_BiDir;

				if ((pih.Type & Ptm_Ins_Loop_16Bit) != 0)
				{
					xxs.Flg |= Xmp_Sample_Flag._16Bit;
					xxs.Len >>= 1;
					xxs.Lps >>= 1;
					xxs.Lpe >>= 1;
				}

				sub.Vol = pih.Vol;
				sub.Pan = 0x80;
				sub.Sid = i;
				pih.Magic = 0;

				lib.common.LibXmp_Instrument_Name(mod, i, pih.Name, 28, encoder);

				// Convert C4SPD to relnote/finetune
				lib.period.LibXmp_C2Spd_To_Note(pih.C4Spd, out sub.Xpo, out sub.Fin);
			}

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				return -1;

			// Read patterns
			for (c_int i = 0; i < mod.Pat; i++)
			{
				// Channel control to prevent infinite loop in pattern reading
				// addresses fuzz bug reported by Lionel Debroux in 20161223
				bool[] chn_Ctrl = new bool[32];

				if (pfh.PatSeg[i] == 0)
					continue;

				if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, 64) < 0)
					return -1;

				f.Hio_Seek(start + 16 * pfh.PatSeg[i], SeekOrigin.Begin);
				c_int r = 0;

				CMemory.memset(chn_Ctrl, false, (size_t)chn_Ctrl.Length);

				while (r < 64)
				{
					uint8 b = f.Hio_Read8();
					if (b == 0)
					{
						r++;
						CMemory.memset(chn_Ctrl, false, (size_t)chn_Ctrl.Length);
						continue;
					}

					c_int c = b & Ptm_Ch_Mask;
					if (chn_Ctrl[c])
					{
						// Uh-oh, something wrong happend
						return -1;
					}

					// Mark this channel as read
					chn_Ctrl[c] = true;

					if (c >= mod.Chn)
						continue;

					Xmp_Event @event = Ports.LibXmp.Common.Event(m, i, c, r);

					if ((b & Ptm_Ni_Follow) != 0)
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
								n += 12;
								break;
							}
						}

						@event.Note = n;
						@event.Ins = f.Hio_Read8();
					}

					if ((b & Ptm_Fx_Follows) != 0)
					{
						@event.FxT = f.Hio_Read8();
						@event.FxP = f.Hio_Read8();

						if (@event.FxT > 0x17)
							@event.FxT = @event.FxP = 0;

						switch (@event.FxT)
						{
							// Break (hex parameter)
							case 0x0d:
							{
								@event.FxP = Effects.Fx_It_Break;
								break;
							}

							// Extended effect
							case 0x0e:
							{
								if (Ports.LibXmp.Common.Msn(@event.FxP) == 0x8)		// Pan set
								{
									@event.FxT = Effects.Fx_SetPan;
									@event.FxP = (uint8)(Ports.LibXmp.Common.Lsn(@event.FxP) << 4);
								}

								break;
							}

							// Set global volume
							case 0x10:
							{
								@event.FxT = Effects.Fx_GlobalVol;
								break;
							}

							// Multi retrig
							case 0x11:
							{
								@event.FxT = Effects.Fx_Multi_Retrig;
								break;
							}

							// Fine vibrato
							case 0x12:
							{
								@event.FxT = Effects.Fx_Fine_Vibrato;
								break;
							}

							// Note slide down
							case 0x13:
							{
								@event.FxT = Effects.Fx_NSlide_Dn;
								break;
							}

							// Note slide up
							case 0x14:
							{
								@event.FxT = Effects.Fx_NSlide_Up;
								break;
							}

							// Note slide down + retrig
							case 0x15:
							{
								@event.FxT = Effects.Fx_NSlide_R_Dn;
								break;
							}

							// Note slide up + retrig
							case 0x16:
							{
								@event.FxT = Effects.Fx_NSlide_R_Up;
								break;
							}

							// Reverse sample
							case 0x17:
							{
								@event.FxT = @event.FxP = 0;
								break;
							}
						}
					}

					if ((b & Ptm_Vol_Follows) != 0)
						@event.Vol = (uint8)(f.Hio_Read8() + 1);
				}
			}

			for (c_int i = 0; i < mod.Smp; i++)
			{
				if (mod.Xxi[i].Nsm == 0)
					continue;

				if (mod.Xxs[i].Len == 0)
					continue;

				f.Hio_Seek(start + smp_Ofs[i], SeekOrigin.Begin);
				if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag._8BDiff, mod.Xxs[i], null, i) < 0)
					return -1;
			}

			m.Vol_Table = Ptm_Vol;

			for (c_int i = 0; i < mod.Chn; i++)
				mod.Xxc[i].Pan = pfh.ChSet[i] << 4;

			m.Quirk |= Quirk_Flag.St3;

			// Has none of ST3's loop quirks; loop jumps unset prior break.
			// TODO: There is an obscure bug where loop jumps take precedence over
			// position jumps *ONLY WHEN THE PLAYER IS AT SPEED 1*.
			// TODO: Jumps are always to row 0
			m.Flow_Mode = FlowMode_Flag.Loop_Global | FlowMode_Flag.Loop_Unset_Break;
			m.Read_Event_Type = Read_Event.St3;

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int Ptm_Ins_Type(int x)
		{
			return x & 3;
		}
		#endregion
	}
}
