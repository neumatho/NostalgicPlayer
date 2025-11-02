/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Format;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Iff;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Loader;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// Originally based on the PSM loader from Modplug by Olivier Lapicque and
	/// fixed comparing the One Must Fall! PSMs with Kenny Chou's MTM files.
	///
	/// From EPICTEST Readme.1st:
	///
	/// The Music And Sound Interface, MASI, is the basis behind all new Epic
	/// games. MASI uses its own proprietary file format, PSM, for storing
	/// its music.
	///
	/// kode54's comment on Sinaria PSMs in the foo_dumb hydrogenaudio forum:
	///
	/// "The Sinaria variant uses eight character pattern and instrument IDs,
	/// the sample headers are laid out slightly different, and the patterns
	/// use a different format for the note values, and also different effect
	/// scales for certain commands.
	///
	/// [Epic] PSM uses high nibble for octave and low nibble for note, for
	/// a valid range up to 0x7F, for a range of D-1 through D#9 compared to
	/// IT. (...) Sinaria PSM uses plain note values, from 1 - 83, for a
	/// range of C-3 through B-9.
	///
	/// [Epic] PSM also uses an effect scale for portamento, volume slides,
	/// and vibrato that is about four times as sensitive as the IT equivalents.
	/// Sinaria does not. This seems to coincide with the MOD/S3M to PSM
	/// converter that Joshua Jensen released in the EPICTEST.ZIP file which
	/// can still be found on a few FTP sites. It converted effects literally,
	/// even though the bundled players behaved as the libraries used with
	/// Epic's games did and made the effects sound too strong."
	///
	/// Claudio's note: Sinaria seems to have a finetune byte just before
	/// volume and some kind of (stereo?) interleaved sample, with 16-byte
	/// frames (see Sinaria songs 5 and 8). Sinaria song 10 still sounds
	/// ugly, maybe caused by finetune issues?
	/// </summary>
	internal class Masi_Load : IFormatLoader
	{
		#region Internal structures

		#region Local_Data
		private class Local_Data
		{
			public bool Sinaria { get; set; }
			public c_int Cur_Pat { get; set; }
			public c_int Cur_Ins { get; set; }
			public ref CPointer<uint8> PNam => ref _PNam;
			private CPointer<uint8> _PNam;
			public ref CPointer<uint8> POrd => ref _POrd;
			private CPointer<uint8> _POrd;
		}
		#endregion

		#endregion

		// ReSharper disable InconsistentNaming
		private static readonly uint32 Magic_PSM = Common.Magic4('P', 'S', 'M', ' ');
		private static readonly uint32 Magic_FILE = Common.Magic4('F', 'I', 'L', 'E');
		private static readonly uint32 Magic_TITL = Common.Magic4('T', 'I', 'T', 'L');
		private static readonly uint32 Magic_OPLH = Common.Magic4('O', 'P', 'L', 'H');
		private static readonly uint32 Magic_PPAN = Common.Magic4('P', 'P', 'A', 'N');
		// ReSharper restore InconsistentNaming

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Masi = new Format_Loader
		{
			Id = Guid.Parse("1DA5B401-5A25-43C3-A5CF-F1C1BDDFB8D3"),
			Name = "Epic MegaGames MASI",
			Description = "MASI also known as PSM (ProTracker Studio Module) is a proprietary file format used by Epic in all their new games.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Masi_Load(LibXmp libXmp)
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
			return new Masi_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			if (f.Hio_Read32B() != Magic_PSM)
				return -1;

			f.Hio_Read8();
			f.Hio_Read8();
			f.Hio_Read8();

			if (f.Hio_Read8() != 0)
				return -1;

			if (f.Hio_Read32B() != Magic_FILE)
				return -1;

			f.Hio_Read32B();

			c_int val = (c_int)f.Hio_Read32L();
			f.Hio_Seek(val, SeekOrigin.Current);

			if (f.Hio_Read32B() == Magic_TITL)
			{
				val = (c_int)f.Hio_Read32L();
				lib.common.LibXmp_Read_Title(f, out t, val, encoder);
			}
			else
				lib.common.LibXmp_Read_Title(f, out t, 0, encoder);

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
			Local_Data data = new Local_Data();

			f.Hio_Read32B();

			data.Sinaria = false;
			mod.Name = string.Empty;

			f.Hio_Seek(8, SeekOrigin.Current);		// Skip file size and FILE

			mod.Smp = mod.Ins = 0;
			data.Cur_Pat = 0;
			data.Cur_Ins = 0;

			c_int offset = (c_int)f.Hio_Tell();

			Iff handle = Iff.LibXmp_Iff_New();
			if (handle == null)
				goto Err;

			// IFF chunk IDs
			c_int ret = handle.LibXmp_Iff_Register("TITL".ToPointer(), Get_Titl);
			ret |= handle.LibXmp_Iff_Register("SDFT".ToPointer(), Get_Sdft);
			ret |= handle.LibXmp_Iff_Register("SONG".ToPointer(), Get_Song);
			ret |= handle.LibXmp_Iff_Register("DSMP".ToPointer(), Get_DSmp_Cnt);
			ret |= handle.LibXmp_Iff_Register("PBOD".ToPointer(), Get_PBod_Cnt);

			if (ret != 0)
				goto Err;

			handle.LibXmp_Iff_Set_Quirk(Iff_Quirk_Flag.Little_Endian);

			// Load IFF chunks
			if (handle.LibXmp_Iff_Load(m, f, data) < 0)
			{
				handle.LibXmp_Iff_Release();
				goto Err;
			}

			handle.LibXmp_Iff_Release();

			mod.Trk = mod.Pat * mod.Chn;

			data.PNam = CMemory.MAlloc<uint8>(mod.Pat * 8);     // Pattern names
			if (data.PNam.IsNull)
				goto Err;

			data.POrd = CMemory.MAlloc<uint8>(Constants.Xmp_Max_Mod_Length * 8);
			if (data.POrd.IsNull)
				goto Err2;

			lib.common.LibXmp_Set_Type(m, data.Sinaria ? "Sinaria PSM" : "Epic MegaGames MASI PSM");

			m.C4Rate = Constants.C4_Ntsc_Rate;

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				goto Err3;

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				goto Err3;

			f.Hio_Seek(start + offset, SeekOrigin.Begin);

			mod.Len = 0;

			handle = Iff.LibXmp_Iff_New();
			if (handle == null)
				goto Err3;

			// IFF chunk IDs
			ret = handle.LibXmp_Iff_Register("SONG".ToPointer(), Get_Song_2);
			ret |= handle.LibXmp_Iff_Register("DSMP".ToPointer(), Get_DSmp);
			ret |= handle.LibXmp_Iff_Register("PBOD".ToPointer(), Get_PBod);

			if (ret != 0)
				goto Err3;

			handle.LibXmp_Iff_Set_Quirk(Iff_Quirk_Flag.Little_Endian);

			// Load IFF chunks
			if (handle.LibXmp_Iff_Load(m, f, data) < 0)
			{
				handle.LibXmp_Iff_Release();
				goto Err3;
			}

			handle.LibXmp_Iff_Release();

			for (c_int i = 0; i < mod.Len; i++)
			{
				c_int j;

				for (j = 0; j < mod.Pat; j++)
				{
					if (CMemory.MemCmp(data.POrd + i * 8, data.PNam + j * 8, data.Sinaria ? 8 : 4) == 0)
					{
						mod.Xxo[i] = (byte)j;
						break;
					}
				}

				if (j == mod.Pat)
					break;
			}

			CMemory.Free(data.POrd);
			CMemory.Free(data.PNam);

			return 0;

			Err3:
			CMemory.Free(data.POrd);
			Err2:
			CMemory.Free(data.PNam);
			Err:
			return -1;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint8 Convert_Porta(uint8 param, bool sinaria)
		{
			if (sinaria)
				return param;

			if (param < 4)
				return (uint8)(param | 0xf0);
			else
				return (uint8)(param >> 2);
		}

		#region IFF chunk handlers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Sdft(Module_Data m, c_int size, Hio f, object parm)
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Titl(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			byte[] buf = new byte[Constants.Xmp_Name_Size];

			size = size > Constants.Xmp_Name_Size - 1 ? Constants.Xmp_Name_Size - 1 : size;
			size = (c_int)f.Hio_Read(buf, 1, (size_t)size);

			mod.Name = encoder.GetString(buf, 0, size);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_DSmp_Cnt(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;

			mod.Ins++;
			mod.Smp = mod.Ins;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_PBod_Cnt(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;
			uint8[] buf = new uint8[20];

			mod.Pat++;

			if (f.Hio_Read(buf, 1, 20) < 20)
				return -1;

			if ((buf[9] != 0) && (buf[13] == 0))
				data.Sinaria = true;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_DSmp(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;
			byte[] buf = new byte[32];

			c_int flags = f.Hio_Read8();									// Flags
			f.Hio_Seek(8, SeekOrigin.Current);					// Song name
			f.Hio_Seek(data.Sinaria ? 8 : 4, SeekOrigin.Current);		// smpid

			c_int i = data.Cur_Ins;

			if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
				return -1;

			Xmp_Instrument xxi = mod.Xxi[i];
			Xmp_SubInstrument sub = xxi.Sub[0];
			Xmp_Sample xxs = mod.Xxs[i];

			f.Hio_Read(buf, 1, 31);
			xxi.Name = encoder.GetString(buf, 0, 31);

			f.Hio_Seek(8, SeekOrigin.Current);
			f.Hio_Read8();		// insno
			f.Hio_Read8();

			xxs.Len = (c_int)f.Hio_Read32L();
			xxs.Lps = (c_int)f.Hio_Read32L();
			xxs.Lpe = (c_int)f.Hio_Read32L();
			xxs.Flg = (flags & 0x80) != 0 ? Xmp_Sample_Flag.Loop : Xmp_Sample_Flag.None;

			f.Hio_Read16L();

			if (xxs.Lpe < 0)
				xxs.Lpe = 0;

			if (xxs.Len > 0)
				xxi.Nsm = 1;

			c_int fineTune = 0;

			if (data.Sinaria)
				fineTune = (int8)(f.Hio_Read8S() << 4);

			sub.Vol = f.Hio_Read8() / 2 + 1;
			f.Hio_Read32L();
			sub.Pan = 0x80;
			sub.Sid = i;

			c_int sRate = f.Hio_Read16L();

			lib.period.LibXmp_C2Spd_To_Note(sRate, out sub.Xpo, out sub.Fin);
			sub.Fin += fineTune;

			f.Hio_Seek(16, SeekOrigin.Current);

			if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag._8BDiff, xxs, null, i) < 0)
				return -1;

			data.Cur_Ins++;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_PBod(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;
			Xmp_Event dummy = new Xmp_Event();

			c_int i = data.Cur_Pat;

			f.Hio_Read32L();
			f.Hio_Read(data.PNam + i * 8, 1, data.Sinaria ? 8U : 4U);

			c_int rows = f.Hio_Read16L();
			if (f.Hio_Error() != 0)
				return -1;

			if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, rows) < 0)
				return -1;

			c_int r = 0;

			do
			{
				c_int rowLen = f.Hio_Read16L() - 2;
				if (f.Hio_Error() != 0)
					return -1;

				while (rowLen > 0)
				{
					uint8 flag = f.Hio_Read8();

					if (rowLen == 1)
						break;

					uint8 chan = f.Hio_Read8();
					rowLen -= 2;

					Xmp_Event @event = chan < mod.Chn ? Ports.LibXmp.Common.Event(m, i, chan, r) : dummy;

					if ((flag & 0x80) != 0)
					{
						uint8 note = f.Hio_Read8();
						rowLen--;

						if (data.Sinaria)
							note += 36;
						else
							note = (uint8)((note >> 4) * 12 + (note & 0x0f) + 1 + 12);

						@event.Note = note;
					}

					if ((flag & 0x40) != 0)
					{
						@event.Ins = (byte)(f.Hio_Read8() + 1);
						rowLen--;
					}

					if ((flag & 0x20) != 0)
					{
						@event.Vol = (byte)(f.Hio_Read8() / 2 + 1);
						rowLen--;
					}

					if ((flag & 0x10) != 0)
					{
						uint8 fxt = f.Hio_Read8();
						uint8 fxp = f.Hio_Read8();
						rowLen -= 2;

						switch (fxt)
						{
							// Fine volume slide up
							case 0x01:
							{
								fxt = Effects.Fx_Extended;
								fxp = (uint8)((Effects.Ex_F_VSlide_Up << 4) | ((fxp / 2) & 0x0f));
								break;
							}

							// Volume slide up
							case 0x02:
							{
								fxt = Effects.Fx_VolSlide;
								fxp = (uint8)((fxp / 2) << 4);
								break;
							}

							// Fine volume slide down
							case 0x03:
							{
								fxt = Effects.Fx_Extended;
								fxp = (uint8)((Effects.Ex_F_VSlide_Dn << 4) | ((fxp / 2) & 0x0f));
								break;
							}

							// Volume slide down
							case 0x04:
							{
								fxt = Effects.Fx_VolSlide;
								fxp /= 2;
								break;
							}

							// Fine portamento up
							case 0x0b:
							{
								fxt = Effects.Fx_Porta_Up;
								fxp = (uint8)((Effects.Ex_F_Porta_Up << 4) | Convert_Porta(fxp, data.Sinaria));
								break;
							}

							// Portamento up
							case 0x0c:
							{
								fxt = Effects.Fx_Porta_Up;
								fxp = Convert_Porta(fxp, data.Sinaria);
								break;
							}

							// Fine portamento down
							case 0x0d:
							{
								fxt = Effects.Fx_Porta_Dn;
								fxp = (uint8)((Effects.Ex_F_Porta_Dn << 4) | Convert_Porta(fxp, data.Sinaria));
								break;
							}

							// Portamento down
							case 0x0e:
							{
								fxt = Effects.Fx_Porta_Dn;
								fxp = Convert_Porta(fxp, data.Sinaria);
								break;
							}

							// Tone portamento
							case 0x0f:
							{
								fxt = Effects.Fx_TonePorta;
								fxp >>= 2;
								break;
							}

							// Tone portamento + volume slide up
							case 0x10:
							{
								fxt = Effects.Fx_Tone_VSlide;
								fxp = (uint8)(fxt & 0xf0);
								break;
							}

							// Glissando
							case 0x11:
							{
								fxt = Effects.Fx_Extended;
								fxp = (uint8)((Effects.Ex_Gliss << 4) | (fxp & 0x0f));
								break;
							}

							// Tone portamento + volume slide down
							case 0x12:
							{
								fxt = Effects.Fx_Tone_VSlide;
								fxp >>= 4;
								break;
							}

							// 0x13: S3M S: crashes MASI

							// Vibrato
							case 0x15:
							{
								fxt = data.Sinaria ? Effects.Fx_Vibrato : Effects.Fx_Fine_Vibrato;
								// fxp remains the same
								break;
							}

							// Vibrato waveform
							case 0x16:
							{
								fxt = Effects.Fx_Extended;
								fxp = (uint8)((Effects.Ex_Vibrato_Wf << 4) | (fxp & 0x0f));
								break;
							}

							// Vibrato + volume slide up
							case 0x17:
							{
								fxt = Effects.Fx_Vibra_VSlide;
								fxp >>= 4;
								break;
							}

							// Vibrato + volume slide down
							case 0x18:
							{
								fxt = Effects.Fx_Vibra_VSlide;
								fxp = (uint8)(fxp & 0x0f);
								break;
							}

							// Tremolo
							case 0x1f:
							{
								fxt = Effects.Fx_Tremolo;
								// fxp remains the same
								break;
							}

							// Tremolo waveform
							case 0x20:
							{
								fxt = Effects.Fx_Extended;
								fxp = (uint8)((Effects.Ex_Tremolo_Wf << 4) | (fxp & 0x0f));
								break;
							}

							// 3-byte offset
							case 0x29:
							{
								fxt = Effects.Fx_Offset;

								// Use only the middle byte
								fxp = f.Hio_Read8();
								f.Hio_Read8();
								rowLen -= 2;
								break;
							}

							// Retrig note
							case 0x2a:
							{
								fxt = Effects.Fx_Extended;
								fxp = (uint8)((Effects.Ex_Retrig << 4) | (fxp & 0x0f));
								break;
							}

							// Note cut
							case 0x2b:
							{
								fxt = Effects.Fx_Extended;
								fxp = (uint8)((Effects.Ex_Cut << 4) | (fxp & 0x0f));
								break;
							}

							// Note delay
							case 0x2c:
							{
								fxt = Effects.Fx_Extended;
								fxp = (uint8)((Effects.Ex_Delay << 4) | (fxp & 0x0f));
								break;
							}

							// Position jump
							case 0x33:
							{
								// Not used in MASI
								fxt = Effects.Fx_Jump;
								fxp >>= 1;
								f.Hio_Read8();
								rowLen--;
								break;
							}

							// Pattern break
							case 0x34:
							{
								// Not used in MASI
								fxt = Effects.Fx_Break;
								break;
							}

							// Pattern loop
							case 0x35:
							{
								fxt = Effects.Fx_Extended;
								fxp = (uint8)((Effects.Ex_Pattern_Loop << 4) | (fxp & 0x0f));
								break;
							}

							// Pattern delay
							case 0x36:
							{
								fxt = Effects.Fx_Extended;
								fxp = (uint8)((Effects.Ex_Patt_Delay << 4) | (fxp & 0x0f));
								break;
							}

							// Speed
							case 0x3d:
							{
								fxt = Effects.Fx_Speed;
								break;
							}

							// Tempo
							case 0x3e:
							{
								fxt = Effects.Fx_Speed;
								break;
							}

							// Arpeggio
							case 0x47:
							{
								fxt = Effects.Fx_S3M_Arpeggio;
								break;
							}

							// Set finetune
							case 0x48:
							{
								fxt = Effects.Fx_Extended;
								fxp = (uint8)((Effects.Ex_FineTune << 4) | (fxp & 0x0f));
								break;
							}

							// Set pan
							case 0x49:
							{
								fxt = Effects.Fx_SetPan;
								fxp <<= 4;
								break;
							}

							default:
							{
								fxt = fxp = 0;
								break;
							}
						}

						@event.FxT = fxt;
						@event.FxP = fxp;
					}
				}

				r++;
			}
			while (r < rows);

			data.Cur_Pat++;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Song(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;

			f.Hio_Seek(10, SeekOrigin.Current);
			mod.Chn = f.Hio_Read8();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Subchunk loader based on OpenMPT LoadPSM.cpp
		/// </summary>
		/********************************************************************/
		private c_int Get_Song_2(Module_Data m, c_int size, Hio f, object parm)
		{
			byte[] buf = new byte[20];

			f.Hio_Read(buf, 1, 9);
			f.Hio_Read16L();
			size -= 11;

			// Iterate over subchunks. We want OPLH and PPAN
			while (size > 0)
			{
				uint32 magic = f.Hio_Read32B();
				c_int subchunk_Size = (c_int)f.Hio_Read32L();
				size -= 8;

				if ((subchunk_Size <= 0) || (f.Hio_Error() != 0))
					return -1;

				size -= subchunk_Size;

				if (magic == Magic_OPLH)
				{
					if (Subchunk_Oplh(m, subchunk_Size, f, parm) < 0)
						return -1;
				}
				else if (magic == Magic_PPAN)
				{
					if (Subchunk_Ppan(m, subchunk_Size, f, parm) < 0)
						return -1;
				}
				else
					f.Hio_Seek(subchunk_Size, SeekOrigin.Current);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Subchunk_Oplh(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;
			c_int first_Order_Chunk = c_int.MaxValue;

			// First two bytes = Number of chunks that follow
			c_int num_Chunks = f.Hio_Read16L();

			// Sub sub chunks
			for (c_int i = 0; (i < num_Chunks) && (size > 0); i++)
			{
				c_int opcode = f.Hio_Read8();
				size--;

				if (opcode == 0)	// Last sub sub chunk
					break;

				// Saga Musix's note in OpenMPT:
				// 
				// "This is more like a playlist than a collection of global
				//  values. In theory, a tempo item inbetween two order items
				//  should modify the tempo when switching patterns. No module
				//  uses this feature in practice though, so we can keep our
				//  loader simple. Unimplemented opcodes do nothing or freeze
				//  MASI."
				switch (opcode)
				{
					// Play order list item
					case 0x01:
					{
						if (mod.Len >= Constants.Xmp_Max_Mod_Length)
							return -1;

						f.Hio_Read(data.POrd + mod.Len * 8, 1, data.Sinaria ? 8U : 4U);
						size -= data.Sinaria ? 8 : 4;
						mod.Len++;

						if (first_Order_Chunk == c_int.MaxValue)
							first_Order_Chunk = i;

						break;
					}

					// 0x02: Play range
					// 0x03: Jump loop

					// Jump line (restart position)
					case 0x04:
					{
						c_int restart_Chunk = f.Hio_Read16L();
						size -= 2;

						// This jumps to the command line, but since we're converting
						// play order list items to our order list, only change the
						// restart position if it's after the first order chunk
						if (restart_Chunk >= first_Order_Chunk)
							mod.Rst = restart_Chunk - first_Order_Chunk;

						break;
					}

					// 0x05: Channel flip
					// 0x06: Transpose

					// Default speed
					case 0x07:
					{
						mod.Spd = f.Hio_Read8();
						size--;
						break;
					}

					// Default tempo
					case 0x08:
					{
						mod.Bpm = f.Hio_Read8();
						size--;
						break;
					}

					// Sample map table
					case 0x0c:
					{
						f.Hio_Read16L();
						f.Hio_Read16L();
						f.Hio_Read16L();
						size -= 6;
						break;
					}

					// Channel panning table
					case 0x0d:
					{
						c_int chn = f.Hio_Read8();
						c_int pan = f.Hio_Read8();
						c_int type = f.Hio_Read8();
						size -= 3;

						if (chn >= Constants.Xmp_Max_Channels)
							break;

						Xmp_Channel xxc = mod.Xxc[chn];

						switch (type)
						{
							// Use panning
							case 0:
							{
								xxc.Pan = pan ^ 0x80;
								break;
							}

							// Surround
							case 2:
							{
								xxc.Pan = 0x80;
								xxc.Flg |= Xmp_Channel_Flag.Surround;
								break;
							}

							// Center
							case 4:
							{
								xxc.Pan = 0x80;
								break;
							}
						}

						break;
					}

					// Channel volume table
					case 0x0e:
					{
						c_int chn = f.Hio_Read8();
						c_int vol = f.Hio_Read8();
						size -= 2;

						if (chn >= Constants.Xmp_Max_Channels)
							break;

						Xmp_Channel xxc = mod.Xxc[chn];

						xxc.Vol = (vol >> 2) + 1;
						break;
					}

					default:
						return -1;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Sinaria channel panning table
		/// </summary>
		/********************************************************************/
		private c_int Subchunk_Ppan(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;

			for (c_int i = 0; (i < Constants.Xmp_Max_Channels) && (size > 0); i++)
			{
				Xmp_Channel xxc = mod.Xxc[i];

				c_int type = f.Hio_Read8();
				c_int pan = f.Hio_Read8();
				size -= 2;

				switch (type)
				{
					// Use panning
					case 0:
					{
						xxc.Pan = pan ^ 0x80;
						break;
					}

					// Surround
					case 2:
					{
						xxc.Pan = 0x80;
						xxc.Flg |= Xmp_Channel_Flag.Surround;
						break;
					}

					// Center
					case 4:
					{
						xxc.Pan = 0x80;
						break;
					}
				}
			}

			return 0;
		}
		#endregion

		#endregion
	}
}
