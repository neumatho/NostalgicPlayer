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
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Format;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Iff;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Loader;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// Note: envelope switching (effect 9) and sample status change (effect 8)
	/// not supported
	/// </summary>
	internal class Mdl_Load : IFormatLoader
	{
		#region Internal structures

		#region Mdl_Envelope
		private class Mdl_Envelope
		{
			public uint8 Num;
			public readonly uint8[] Data = new uint8[30];
			public uint8 Sus;
			public uint8 Loop;
		}
		#endregion

		#region Local_Data
		private class Local_Data
		{
			public CPointer<c_int> I_Index;
			public CPointer<c_int> S_Index;
			public CPointer<c_int> V_Index;		// Volume envelope
			public CPointer<c_int> P_Index;		// Pan envelope
			public CPointer<c_int> F_Index;		// Pitch envelope
			public CPointer<c_int> PackInfo;
			public bool Has_In;
			public bool Has_Pa;
			public bool Has_Tr;
			public bool Has_Ii;
			public bool Has_Is;
			public bool Has_Sa;
			public c_int V_EnvNum;
			public c_int P_EnvNum;
			public c_int F_EnvNum;
			public Mdl_Envelope[] V_Env;
			public Mdl_Envelope[] P_Env;
			public Mdl_Envelope[] F_Env;
		}
		#endregion

		#region Bits
		private class Bits
		{
			public uint32 B;
			public uint32 N;
		}
		#endregion

		#endregion

		// ReSharper disable InconsistentNaming
		private static readonly uint32 Magic_DMDL = Common.Magic4('D', 'M', 'D', 'L');

		private const c_int Mdl_Note_Follows = 0x04;
		private const c_int Mdl_Instrument_Follows = 0x08;
		private const c_int Mdl_Volume_Follows = 0x10;
		private const c_int Mdl_Effect_Follows = 0x20;
		private const c_int Mdl_Parameter1_Follows = 0x40;
		private const c_int Mdl_Parameter2_Follows = 0x80;
		// ReSharper restore InconsistentNaming

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Mdl = new Format_Loader
		{
			Id = Guid.Parse("8225281F-F5ED-4DEF-B301-C5C6ED8B8E3B"),
			Name = "Digitrakker",
			Description = "Digitrakker is inspired by the Amiga Protracker by let the music consists of pattern and instruments. One pattern contains up to 32 channels and up to 256 note lines. The songlist lays down, which pattern will be played at which position. The instruments, which can be used in the patterns, consist of samples, volume-, panning- and frequency-envelopes and some more parameters.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Mdl_Load(LibXmp libXmp)
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
			return new Mdl_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			if (f.Hio_Read32B() != Magic_DMDL)
				return -1;

			f.Hio_Read8();		// Version

			uint16 id = f.Hio_Read16B();
			if (id == 0x494e)	// IN
			{
				f.Hio_Read32B();
				lib.common.LibXmp_Read_Title(f, out t, 32, encoder);
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
			byte[] buf = new byte[8];
			Local_Data data = new Local_Data();
			c_int retVal = 0;

			// Check magic and get version
			f.Hio_Read32B();

			if (f.Hio_Read(buf, 1, 1) < 1)
				return -1;

			Iff handle = Iff.LibXmp_Iff_New();
			if (handle == null)
				return -1;

			// IFFoid chunk IDs
			handle.LibXmp_Iff_Register("IN".ToPointer(), Get_Chunk_In);		// Module info
			handle.LibXmp_Iff_Register("TR".ToPointer(), Get_Chunk_Tr);		// Tracks
			handle.LibXmp_Iff_Register("SA".ToPointer(), Get_Chunk_Sa);		// Sampled data
			handle.LibXmp_Iff_Register("VE".ToPointer(), Get_Chunk_Ve);		// Volume envelopes
			handle.LibXmp_Iff_Register("PE".ToPointer(), Get_Chunk_Pe);		// Pan envelopes
			handle.LibXmp_Iff_Register("FE".ToPointer(), Get_Chunk_Fe);		// Pitch envelopes

			if (Ports.LibXmp.Common.Msn(buf[0]) != 0)
			{
				handle.LibXmp_Iff_Register("II".ToPointer(), Get_Chunk_Ii);	// Instruments
				handle.LibXmp_Iff_Register("PA".ToPointer(), Get_Chunk_Pa);	// Patterns
				handle.LibXmp_Iff_Register("IS".ToPointer(), Get_Chunk_Is);	// Sample info
			}
			else
			{
				handle.LibXmp_Iff_Register("PA".ToPointer(), Get_Chunk_P0);	// Old 0.0 patterns
				handle.LibXmp_Iff_Register("IS".ToPointer(), Get_Chunk_I0);	// Old 0.0 sample info
			}

			// MDL uses an IFF-style file format with 16-bit IDs and little endian
			// 32-bit chunk size. There's only one chunk per data type (i.e. one
			// big chunk for all samples)
			handle.LibXmp_Iff_Id_Size(2);
			handle.LibXmp_Iff_Set_Quirk(Iff_Quirk_Flag.Little_Endian);

			lib.common.LibXmp_Set_Type(m, string.Format("Digitrakker MDL {0}.{1}", Ports.LibXmp.Common.Msn(buf[0]), Ports.LibXmp.Common.Lsn(buf[0])));

			m.VolBase = 0xff;
			m.C4Rate = Constants.C4_Ntsc_Rate;

			data.V_EnvNum = data.P_EnvNum = data.F_EnvNum = 0;
			data.S_Index = CMemory.calloc<c_int>(256);
			data.I_Index = CMemory.calloc<c_int>(256);
			data.V_Index = CMemory.malloc<c_int>(256);
			data.P_Index = CMemory.malloc<c_int>(256);
			data.F_Index = CMemory.malloc<c_int>(256);

			if (data.S_Index.IsNull || data.I_Index.IsNull || data.V_Index.IsNull || data.P_Index.IsNull || data.F_Index.IsNull)
			{
				retVal = -1;
				goto Err;
			}

			for (c_int i = 0; i < 256; i++)
				data.V_Index[i] = data.P_Index[i] = data.F_Index[i] = -1;

			// Load IFFoid chunks
			if (handle.LibXmp_Iff_Load(m, f, data) < 0)
			{
				handle.LibXmp_Iff_Release();
				retVal = -1;
				goto Err;
			}

			handle.LibXmp_Iff_Release();

			// Reindex instruments
			for (c_int i = 0; i < mod.Trk; i++)
			{
				for (c_int j = 0; j < mod.Xxt[i].Rows; j++)
				{
					Xmp_Event e = mod.Xxt[i].Event[j];

					for (c_int l = 0; l < mod.Ins; l++)
					{
						if ((e.Ins != 0) && (e.Ins == data.I_Index[l]))
						{
							e.Ins = (byte)(l + 1);
							break;
						}
					}
				}
			}

			// Reindex envelopes, etc
			for (c_int i = 0; i < mod.Ins; i++)
			{
				Fix_Env(i, mod.Xxi[i].Aei, data.V_Env, data.V_Index, data.V_EnvNum);
				Fix_Env(i, mod.Xxi[i].Pei, data.P_Env, data.P_Index, data.P_EnvNum);
				Fix_Env(i, mod.Xxi[i].Fei, data.F_Env, data.F_Index, data.F_EnvNum);

				for (c_int j = 0; j < mod.Xxi[i].Nsm; j++)
				{
					for (c_int k = 0; k < mod.Smp; k++)
					{
						if (mod.Xxi[i].Sub[j].Sid == data.S_Index[k])
						{
							mod.Xxi[i].Sub[j].Sid = k;
							break;
						}
					}
				}
			}

			Err:
			CMemory.free(data.F_Index);
			CMemory.free(data.P_Index);
			CMemory.free(data.V_Index);
			CMemory.free(data.I_Index);
			CMemory.free(data.S_Index);

			CMemory.free(data.PackInfo);

			m.Quirk |= Quirk_Flag.Ft2 | Quirk_Flag.KeyOff;
			m.Read_Event_Type = Read_Event.Ft2;
			m.Module_Flags = Xmp_Module_Flags.Uses_Tracks;

			return retVal;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Fix_Env(c_int i, Xmp_Envelope ei, Mdl_Envelope[] env, CPointer<c_int> idx, c_int envNum)
		{
			if (idx[i] >= 0)
			{
				ei.Flg = Xmp_Envelope_Flag.On;
				ei.Npt = 15;

				for (c_int j = 0; j < envNum; j++)
				{
					if (idx[i] == env[j].Num)
					{
						ei.Flg |= (env[j].Sus & 0x10) != 0 ? Xmp_Envelope_Flag.Sus : Xmp_Envelope_Flag.None;
						ei.Flg |= (env[j].Sus & 0x20) != 0 ? Xmp_Envelope_Flag.Loop : Xmp_Envelope_Flag.None;
						ei.Sus = env[j].Sus & 0x0f;
						ei.Lps = env[j].Loop & 0x0f;
						ei.Lpe = env[j].Loop & 0xf0;

						c_int lastX = -1;
						c_int k;

						for (k = 0; k < ei.Npt; k++)
						{
							c_int x = env[j].Data[k * 2];

							if (x == 0)
								break;

							ei.Data[k * 2] = (c_short)(lastX + x);
							ei.Data[k * 2 + 1] = env[j].Data[k * 2 + 1];

							lastX = ei.Data[k * 2];
						}

						ei.Npt = k;
						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Effects 1-6 (note effects) can only be entered in the first
		/// effect column, G-L (volume effects) only in the second column
		/// </summary>
		/********************************************************************/
		private void Xlat_Fx_Common(ref uint8 t, ref uint8 p)
		{
			switch (t)
			{
				// 7 - Set BPM
				case 0x07:
				{
					t = Effects.Fx_S3M_Bpm;
					break;
				}

				// 8 - Set pan
				// 9 - Set envelope -- not supported
				// A - Not used
				case 0x08:
				case 0x09:
				case 0x0a:
				{
					t = p = 0x00;
					break;
				}

				// B - Position jump
				// C - Set volume
				// D - Pattern break
				case 0x0b:
				case 0x0c:
				case 0x0d:
				{
					// Like Protracker
					break;
				}

				// E - Extended
				case 0x0e:
				{
					switch (Ports.LibXmp.Common.Msn(p))
					{
						// E0 - Not used
						// E3 - Not used
						// E8 - Set sample status -- unsupported
						case 0x0:
						case 0x3:
						case 0x8:
						{
							t = p = 0x00;
							break;
						}

						// E1 - Pan slide left
						case 0x1:
						{
							t = Effects.Fx_PanSlide;
							p <<= 4;
							break;
						}

						// E2 - Pan slide right
						case 0x2:
						{
							t = Effects.Fx_PanSlide;
							p &= 0x0f;
							break;
						}
					}

					break;
				}

				case 0x0f:
				{
					t = Effects.Fx_S3M_Speed;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Xlat_Fx1(ref uint8 t, ref uint8 p)
		{
			switch (t)
			{
				// - - No effect
				case 0x00:
				{
					p = 0;
					break;
				}

				// 5 - Arpeggio
				case 0x05:
				{
					t = Effects.Fx_Arpeggio;
					break;
				}

				// 6 - Not used
				case 0x06:
				{
					t = p = 0x00;
					break;
				}
			}

			Xlat_Fx_Common(ref t, ref p);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Xlat_Fx2(ref uint8 t, ref uint8 p)
		{
			switch (t)
			{
				// - - No effect
				case 0x00:
				{
					p = 0;
					break;
				}

				// G - Volume slide up
				case 0x01:
				{
					t = Effects.Fx_VolSlide_Up;
					break;
				}

				// H - Volume slide down
				case 0x02:
				{
					t = Effects.Fx_VolSlide_Dn;
					break;
				}

				// I - Multi retrig
				case 0x03:
				{
					t = Effects.Fx_Multi_Retrig;
					break;
				}

				// J - Tremolo
				case 0x04:
				{
					t = Effects.Fx_Tremolo;
					break;
				}

				// K - Tremor
				case 0x05:
				{
					t = Effects.Fx_Tremor;
					break;
				}

				// L - Not used
				case 0x06:
				{
					t = p = 0x00;
					break;
				}
			}

			Xlat_Fx_Common(ref t, ref p);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint Get_Bits(byte i, ref CPointer<uint8> buf, ref c_int len, Bits bits)
		{
			if (i == 0)
			{
				bits.B = DataIo.ReadMem32L(buf);
				buf += 4;
				len -= 4;

				bits.N = 32;
			}

			uint x = (uint)(bits.B & ((1 << i) - 1));	// Get i bits
			bits.B >>= i;
			bits.N -= i;

			if (bits.N <= 24)
			{
				if (len <= 0)		// FIXME: last few bits can't be consumed
					return x;

				bits.B |= (uint)(DataIo.ReadMem32L(buf) << (int)bits.N);
				buf++;
				bits.N += 8;
				len--;
			}

			return x;
		}



		/********************************************************************/
		/// <summary>
		/// From the Digitrakker docs:
		///
		/// The description of the sample-packmethode (1) [8bit packing]:...
		/// ----------------------------------------------------------------
		///
		/// The method is based on the Huffman algorithm. It's easy and very
		/// fast and effective on samples. The packed sample is a bit stream:
		///
		///      Byte 0    Byte 1    Byte 2    Byte 3
		/// Bit 76543210  fedcba98  nmlkjihg  ....rqpo
		///
		/// A packed byte is stored in the following form:
		///
		/// xxxx10..0s => byte = [xxxx] + (number of [0] bits between
		///     s and 1) * 16 - 8;
		/// if s==1 then byte = byte xor 255
		///
		/// If there are no [0] bits between the first bit (sign) and the
		/// [1] bit, you have the following form:
		///
		/// xxx1s => byte = [xxx]; if s=1 then byte = byte xor 255
		/// </summary>
		/********************************************************************/
		private c_int Unpack_Sample8(CPointer<uint8> t, CPointer<uint8> f, c_int len, c_int l)
		{
			c_int i;
			uint8 b, d;
			Bits bits = new Bits();

			Get_Bits(0, ref f, ref len, bits);

			for (i = b = d = 0; i < l; i++)
			{
				// Sanity check
				if (len < 0)
					return -1;

				c_int s = (c_int)Get_Bits(1, ref f, ref len, bits);

				if (Get_Bits(1, ref f, ref len, bits) != 0)
					b = (uint8)Get_Bits(3, ref f, ref len, bits);
				else
				{
					b = 8;

					while ((len >= 0) && (Get_Bits(1, ref f, ref len, bits) == 0))
					{
						// Sanity check
						if (b >= 240)
							return -1;

						b += 16;
					}

					b += (uint8)Get_Bits(4, ref f, ref len, bits);
				}

				if (s != 0)
					b ^= 0xff;

				d += b;
				t[0, 1] = d;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// The description of the sample-packmethode (2) [16bit packing]:...
		/// ----------------------------------------------------------------
		///
		/// It works as methode (1) but it only crunches every 2nd byte (the
		/// high-bytes of 16 bit samples). So when you depack 16 bit samples,
		/// you have to read 8 bits from the data-stream first. They present
		/// the lowbyte of the sample-word. Then depack the highbyte in the
		/// descripted way (methode [1]).
		/// Only the highbytes are delta-values. So take the lowbytes as they
		/// are. Go on this way for the whole sample
		/// </summary>
		/********************************************************************/
		private c_int Unpack_Sample16(CPointer<uint8> t, CPointer<uint8> f, c_int len, c_int l)
		{
			c_int i, lo;
			uint8 b, d;
			Bits bits = new Bits();

			Get_Bits(0, ref f, ref len, bits);

			for (i = lo = b = d = 0; i < l; i++)
			{
				// Sanity check
				if (len < 0)
					return -1;

				lo = (c_int)Get_Bits(8, ref f, ref len, bits);
				c_int s = (c_int)Get_Bits(1, ref f, ref len, bits);

				if (Get_Bits(1, ref f, ref len, bits) != 0)
					b = (uint8)Get_Bits(3, ref f, ref len, bits);
				else
				{
					b = 8;

					while ((len >= 0) && (Get_Bits(1, ref f, ref len, bits) == 0))
					{
						// Sanity check
						if (b >= 240)
							return -1;

						b += 16;
					}

					b += (uint8)Get_Bits(4, ref f, ref len, bits);
				}

				if (s != 0)
					b ^= 0xff;

				d += b;
				t[0, 1] = (uint8)lo;
				t[0, 1] = d;
			}

			return 0;
		}

		#region IFF chunk handlers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Chunk_In(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;
			c_int i;

			// Sanity check
			if (data.Has_In)
				return -1;

			data.Has_In = true;

			byte[] buf = new byte[32];
			f.Hio_Read(buf, 1, 32);
			mod.Name = encoder.GetString(buf).TrimEnd();

			f.Hio_Seek(20, SeekOrigin.Current);

			mod.Len = f.Hio_Read16L();
			mod.Rst = f.Hio_Read16L();
			f.Hio_Read8();					// GVol
			mod.Spd = f.Hio_Read8();
			mod.Bpm = f.Hio_Read8();

			// Sanity check
			if ((mod.Len > 256) || (mod.Rst > 255))
				return -1;

			for (i = 0; i < 32; i++)
			{
				uint8 chInfo = f.Hio_Read8();

				if ((chInfo & 0x80) != 0)
					break;

				mod.Xxc[i].Pan = chInfo << 1;
			}

			mod.Chn = i;
			f.Hio_Seek(32 - i - 1, SeekOrigin.Current);

			if (f.Hio_Read(mod.Xxo, 1, (size_t)mod.Len) != (size_t)mod.Len)
				return -1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Chunk_Pa(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;

			// Sanity check
			if (data.Has_Pa || !data.Has_In)
				return -1;

			data.Has_Pa = true;

			mod.Pat = f.Hio_Read8();

			mod.Xxp = new Xmp_Pattern[mod.Pat];
			if (mod.Xxp == null)
				return -1;

			for (c_int i = 0; i < mod.Pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern(mod, i) < 0)
					return -1;

				c_int chn = f.Hio_Read8();
				mod.Xxp[i].Rows = f.Hio_Read8() + 1;

				f.Hio_Seek(16, SeekOrigin.Current);		// Skip pattern name

				for (c_int j = 0; j < chn; j++)
				{
					c_int x = f.Hio_Read16L();

					if (j < mod.Chn)
						mod.Xxp[i].Index[j] = x;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Chunk_P0(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;

			// Sanity check
			if (data.Has_Pa || !data.Has_In)
				return -1;

			data.Has_Pa = true;

			mod.Pat = f.Hio_Read8();

			mod.Xxp = new Xmp_Pattern[mod.Pat];
			if (mod.Xxp == null)
				return -1;

			for (c_int i = 0; i < mod.Pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern(mod, i) < 0)
					return -1;

				mod.Xxp[i].Rows = 64;

				for (c_int j = 0; j < 32; j++)
				{
					uint16 x = f.Hio_Read16L();

					if (j < mod.Chn)
						mod.Xxp[i].Index[j] = x;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Chunk_Tr(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;
			c_int k, row;

			// Sanity check
			if (data.Has_Tr || !data.Has_Pa)
				return -1;

			data.Has_Tr = true;

			mod.Trk = f.Hio_Read16L() + 1;

			// Sanity check
			c_int max_Trk = 0;

			for (c_int i = 0; i < mod.Pat; i++)
			{
				for (c_int j = 0; j < mod.Chn; j++)
				{
					if (max_Trk < mod.Xxp[i].Index[j])
						max_Trk = mod.Xxp[i].Index[j];
				}
			}

			if (max_Trk >= mod.Trk)
				return -1;

			mod.Xxt = new Xmp_Track[mod.Trk];
			if (mod.Xxt == null)
				return -1;

			Xmp_Track track = new Xmp_Track
			{
				Event = ArrayHelper.InitializeArray<Xmp_Event>(256)
			};
			if (track == null)
				goto Err;

			// Empty track 0 is not stored in the file
			if (lib.common.LibXmp_Alloc_Track(mod, 0, 256) < 0)
				goto Err2;

			for (c_int i = 1; i < mod.Trk; i++)
			{
				// Length of the track in bytes
				c_int len = f.Hio_Read16L();

				track.Clear();

				for (row = 0; len != 0; )
				{
					// Sanity check
					if (row > 255)
						goto Err2;

					CPointer<Xmp_Event> ev = new CPointer<Xmp_Event>(track.Event, row);

					c_int j = f.Hio_Read8();
					len--;

					switch (j & 0x03)
					{
						case 0:
						{
							row += j >> 2;
							break;
						}

						case 1:
						{
							// Sanity check
							if ((row < 1) || ((row + (j >> 2)) > 255))
								goto Err2;

							for (k = 0; k <= (j >> 2); k++)
								ev[k].CopyFrom(ev[-1]);

							row += k - 1;
							break;
						}

						case 2:
						{
							// Sanity check
							if ((j >> 2) == row)
								goto Err2;

							ev[0].CopyFrom(track.Event[j >> 2]);
							break;
						}

						case 3:
						{
							if ((j & Mdl_Note_Follows) != 0)
							{
								uint8 b = f.Hio_Read8();
								len--;

								ev[0].Note = (byte)(b == 0xff ? Constants.Xmp_Key_Off : b + 12);
							}

							if ((j & Mdl_Instrument_Follows) != 0)
							{
								ev[0].Ins = f.Hio_Read8();
								len--;
							}

							if ((j & Mdl_Volume_Follows) != 0)
							{
								ev[0].Vol = f.Hio_Read8();
								len--;
							}

							if ((j & Mdl_Effect_Follows) != 0)
							{
								k = f.Hio_Read8();
								len--;

								ev[0].FxT = (byte)Ports.LibXmp.Common.Lsn(k);
								ev[0].F2T = (byte)Ports.LibXmp.Common.Msn(k);
							}

							if ((j & Mdl_Parameter1_Follows) != 0)
							{
								ev[0].FxP = f.Hio_Read8();
								len--;
							}

							if ((j & Mdl_Parameter2_Follows) != 0)
							{
								ev[0].F2P = f.Hio_Read8();
								len--;
							}

							break;
						}
					}

					row++;
				}

				if (row <= 64)
					row = 64;
				else if (row <= 128)
					row = 128;
				else
					row = 256;

				if (lib.common.LibXmp_Alloc_Track(mod, i, row) < 0)
					goto Err2;

				for (c_int j = 0; j < row; j++)
					mod.Xxt[i].Event[j].CopyFrom(track.Event[j]);

				// Translate effects
				for (c_int j = 0; j < row; j++)
				{
					Xmp_Event ev = mod.Xxt[i].Event[j];
					Xlat_Fx1(ref ev.FxT, ref ev.FxP);
					Xlat_Fx2(ref ev.F2T, ref ev.F2P);
				}
			}

			return 0;

			Err2:
			Err:
			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Chunk_Ii(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;
			c_int j;
			c_int map, last_Map;
			uint8[] buf = new uint8[40];

			// Sanity check
			if (data.Has_Ii)
				return -1;

			data.Has_Ii = true;

			mod.Ins = f.Hio_Read8();

			mod.Xxi = ArrayHelper.InitializeArray<Xmp_Instrument>(mod.Ins);
			if (mod.Xxi == null)
				return -1;

			for (c_int i = 0; i < mod.Ins; i++)
			{
				Xmp_Instrument xxi = mod.Xxi[i];

				data.I_Index[i] = f.Hio_Read8();
				xxi.Nsm = f.Hio_Read8();

				if (f.Hio_Read(buf, 1, 32) < 32)
					return -1;

				buf[32] = 0;
				lib.common.LibXmp_Instrument_Name(mod, i, buf, 32, encoder);

				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, xxi.Nsm) < 0)
					return -1;

				for (j = 0; j < Constants.Xmp_Max_Keys; j++)
					xxi.Map[j].Ins = 0xff;

				for (last_Map = j = 0; j < mod.Xxi[i].Nsm; j++)
				{
					Xmp_SubInstrument sub = xxi.Sub[j];

					sub.Sid = f.Hio_Read8();
					map = f.Hio_Read8() + 12;
					sub.Vol = f.Hio_Read8();

					for (c_int k = last_Map; k <= map; k++)
					{
						if (k < Constants.Xmp_Max_Keys)
							xxi.Map[k].Ins = (byte)j;
					}

					last_Map = map + 1;

					c_int x = f.Hio_Read8();		// Volume envelope

					if (j == 0)
						data.V_Index[i] = (x & 0x80) != 0 ? x & 0x3f : -1;

					if ((~x & 0x40) != 0)
						sub.Vol = 0xff;

					mod.Xxi[i].Sub[j].Pan = f.Hio_Read8() << 1;

					x = f.Hio_Read8();				// Pan envelope

					if (j == 0)
						data.P_Index[i] = (x & 0x80) != 0 ? x & 0x3f : -1;

					if ((~x & 0x40) != 0)
						sub.Pan = 0x80;

					x = f.Hio_Read16L();

					if (j == 0)
						xxi.Rls = x;

					sub.Vra = f.Hio_Read8();		// Vibrato rate
					sub.Vde = f.Hio_Read8() << 1;	// Vibrato depth
					sub.Vsw = f.Hio_Read8();        // Vibrato sweep
					sub.Vwf = f.Hio_Read8();		// Vibrato waveform
					f.Hio_Read8();					// Reserved

					x = f.Hio_Read8();				// Pitch envelope

					if (j == 0)
						data.F_Index[i] = (x & 0x80) != 0 ? x & 0x3f : -1;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Chunk_Is(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;
			uint8[] buf = new uint8[64];

			// Sanity check
			if (data.Has_Is)
				return -1;

			data.Has_Is = true;

			mod.Smp = f.Hio_Read8();
			mod.Xxs = ArrayHelper.InitializeArray<Xmp_Sample>(mod.Smp);
			if (mod.Xxs == null)
				return -1;

			m.Xtra = ArrayHelper.InitializeArray<Extra_Sample_Data>(mod.Smp);
			if (m.Xtra == null)
				return -1;

			data.PackInfo = CMemory.calloc<c_int>((size_t)mod.Smp);
			if (data.PackInfo.IsNull)
				return -1;

			for (c_int i = 0; i < mod.Smp; i++)
			{
				Xmp_Sample xxs = mod.Xxs[i];

				data.S_Index[i] = f.Hio_Read8();	// Sample number

				if (f.Hio_Read(buf, 1, 32) < 32)
					return -1;

				buf[32] = 0;
				lib.common.LibXmp_Copy_Adjust(out xxs.Name, buf, 31, encoder);

				f.Hio_Seek(8, SeekOrigin.Current);		// Sample filename

				c_int c5Spd = (c_int)f.Hio_Read32L();

				xxs.Len = (c_int)f.Hio_Read32L();
				xxs.Lps = (c_int)f.Hio_Read32L();
				xxs.Lpe = (c_int)f.Hio_Read32L();

				// Sanity check
				if ((xxs.Len < 0) || (xxs.Lps < 0) || (xxs.Lps > xxs.Len) || (xxs.Lpe > (xxs.Len - xxs.Lps)))
					return -1;

				xxs.Flg = xxs.Lpe > 0 ? Xmp_Sample_Flag.Loop : Xmp_Sample_Flag.None;
				xxs.Lpe = xxs.Lps + xxs.Lpe;

				m.Xtra[i].C5Spd = c5Spd;

				f.Hio_Read8();		// Volume in DMDL 0.0

				uint8 x = f.Hio_Read8();
				if ((x & 0x01) != 0)
				{
					xxs.Flg |= Xmp_Sample_Flag._16Bit;
					xxs.Len >>= 1;
					xxs.Lps >>= 1;
					xxs.Lpe >>= 1;
				}

				xxs.Flg |= (x & 0x02) != 0 ? Xmp_Sample_Flag.Loop_BiDir : Xmp_Sample_Flag.None;
				data.PackInfo[i] = (x & 0x0c) >> 2;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Chunk_I0(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;
			uint8[] buf = new uint8[64];

			// Sanity check
			if (data.Has_Ii || data.Has_Is)
				return -1;

			data.Has_Ii = true;
			data.Has_Is = true;

			mod.Ins = mod.Smp = f.Hio_Read8();

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			data.PackInfo = CMemory.calloc<c_int>((size_t)mod.Smp);
			if (data.PackInfo.IsNull)
				return -1;

			for (c_int i = 0; i < mod.Ins; i++)
			{
				Xmp_Instrument xxi = mod.Xxi[i];
				Xmp_Sample xxs = mod.Xxs[i];

				xxi.Nsm = 1;

				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					return -1;

				Xmp_SubInstrument sub = xxi.Sub[0];
				sub.Sid = data.I_Index[i] = data.S_Index[i] = f.Hio_Read8();

				if (f.Hio_Read(buf, 1, 32) < 32)
					return -1;

				buf[32] = 0;
				lib.common.LibXmp_Instrument_Name(mod, i, buf, 32, encoder);

				f.Hio_Seek(8, SeekOrigin.Current);		// Sample filename

				c_int c5Spd = f.Hio_Read16L();

				xxs.Len = (c_int)f.Hio_Read32L();
				xxs.Lps = (c_int)f.Hio_Read32L();
				xxs.Lpe = (c_int)f.Hio_Read32L();

				// Sanity check
				if ((xxs.Len < 0) || (xxs.Lps < 0) || (xxs.Lps > xxs.Len) || (xxs.Lpe > (xxs.Len - xxs.Lps)))
					return -1;

				xxs.Flg = xxs.Lpe > 0 ? Xmp_Sample_Flag.Loop : Xmp_Sample_Flag.None;
				xxs.Lpe = xxs.Lps + xxs.Lpe;

				sub.Vol = f.Hio_Read8();		// Volume
				sub.Pan = 0x80;

				m.Xtra[i].C5Spd = c5Spd;

				uint8 x = f.Hio_Read8();
				if ((x & 0x01) != 0)
				{
					xxs.Flg |= Xmp_Sample_Flag._16Bit;
					xxs.Len >>= 1;
					xxs.Lps >>= 1;
					xxs.Lpe >>= 1;
				}

				xxs.Flg |= (x & 0x02) != 0 ? Xmp_Sample_Flag.Loop_BiDir : Xmp_Sample_Flag.None;
				data.PackInfo[i] = (x & 0x0c) >> 2;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Chunk_Sa(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;
			c_int size_Bound;
			CPointer<uint8> smpBuf = null, buf;
			c_int smpBuf_Alloc = -1;

			c_int left = (c_int)(f.Hio_Size() - f.Hio_Tell());

			// Sanity check
			if (data.Has_Sa || !data.Has_Is || data.PackInfo.IsNull)
				return -1;

			data.Has_Sa = true;

			if (size < left)
				left = size;

			for (c_int i = 0; i < mod.Smp; i++)
			{
				Xmp_Sample xxs = mod.Xxs[i];

				c_int len = xxs.Len;
				if ((xxs.Flg & Xmp_Sample_Flag._16Bit) != 0)
					len <<= 1;

				// Bound the packed sample data size before trying to allocate RAM for it...
				switch (data.PackInfo[i])
				{
					case 0:
					{
						size_Bound = len;
						break;
					}

					case 1:
					{
						// See Unpack_Sample8: each byte packs to 5 bits minimum
						size_Bound = (len >> 3) * 5;
						break;
					}

					case 2:
					{
						// See Unpack_Sample16: each upper byte packs to 5 bits minimum, lower bytes are not packed
						size_Bound = (len >> 4) * 13;
						break;
					}

					default:
					{
						// Sanity check
						goto Err2;
					}
				}

				// Sanity check
				if (left < size_Bound)
					goto Err2;

				if (len > smpBuf_Alloc)
				{
					CPointer<uint8> tmp = CMemory.realloc(smpBuf, (size_t)len);
					if (tmp.IsNull)
						goto Err2;

					smpBuf = tmp;
					smpBuf_Alloc = len;
				}

				Hio s = f.GetSampleHio(i, xxs.Len);

				try
				{
					switch (data.PackInfo[i])
					{
						case 0:
						{
							if (s.Hio_Read(smpBuf, 1, (size_t)len) < (size_t)len)
								goto Err2;

							left -= len;
							break;
						}

						case 1:
						{
							len = (c_int)s.Hio_Read32L();

							// Sanity check
							if ((xxs.Flg & Xmp_Sample_Flag._16Bit) != 0)
								goto Err2;

							if ((len <= 0) || (len > Constants.Max_Sample_Size))		// Max compressed sample size
								goto Err2;

							buf = CMemory.malloc<uint8>((size_t)len + 4);
							if (buf.IsNull)
								goto Err2;

							if (s.Hio_Read(buf, 1, (size_t)len) != (size_t)len)
								goto Err3;

							// The unpack function may read slightly beyond the end
							buf[len] = buf[len + 1] = buf[len + 2] = buf[len + 3] = 0;

							if (Unpack_Sample8(smpBuf, buf, len, xxs.Len) < 0)
								goto Err3;

							CMemory.free(buf);
							left -= len + 4;
							break;
						}

						case 2:
						{
							len = (c_int)s.Hio_Read32L();

							// Sanity check
							if ((~xxs.Flg & Xmp_Sample_Flag._16Bit) != 0)
								goto Err2;

							if ((len <= 0) || (len > Constants.Max_Sample_Size))
								goto Err2;

							buf = CMemory.malloc<uint8>((size_t)len + 4);
							if (buf.IsNull)
								goto Err2;

							if (s.Hio_Read(buf, 1, (size_t)len) != (size_t)len)
								goto Err3;

							// The unpack function may read slightly beyond the end
							buf[len] = buf[len + 1] = buf[len + 2] = buf[len + 3] = 0;

							if (Unpack_Sample16(smpBuf, buf, len, xxs.Len) < 0)
								goto Err3;

							CMemory.free(buf);
							left -= len + 4;
							break;
						}
					}
				}
				finally
				{
					s.Hio_Close();
				}

				if (Sample.LibXmp_Load_Sample(m, null, Sample_Flag.NoLoad, xxs, smpBuf) < 0)
					goto Err2;
			}

			CMemory.free(smpBuf);

			return 0;

			Err3:
			CMemory.free(buf);

			Err2:
			CMemory.free(smpBuf);

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Chunk_Ve(Module_Data m, c_int size, Hio f, object parm)
		{
			Local_Data data = (Local_Data)parm;

			// Sanity check
			if (data.V_Env != null)
				return -1;

			data.V_EnvNum = f.Hio_Read8();
			if (data.V_EnvNum == 0)
				return 0;

			data.V_Env = ArrayHelper.InitializeArray<Mdl_Envelope>(data.V_EnvNum);
			if (data.V_Env == null)
				return -1;

			for (c_int i = 0; i < data.V_EnvNum; i++)
			{
				data.V_Env[i].Num = f.Hio_Read8();

				f.Hio_Read(data.V_Env[i].Data, 1, 30);

				data.V_Env[i].Sus = f.Hio_Read8();
				data.V_Env[i].Loop = f.Hio_Read8();
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Chunk_Pe(Module_Data m, c_int size, Hio f, object parm)
		{
			Local_Data data = (Local_Data)parm;

			// Sanity check
			if (data.P_Env != null)
				return -1;

			data.P_EnvNum = f.Hio_Read8();
			if (data.P_EnvNum == 0)
				return 0;

			data.P_Env = ArrayHelper.InitializeArray<Mdl_Envelope>(data.P_EnvNum);
			if (data.P_Env == null)
				return -1;

			for (c_int i = 0; i < data.P_EnvNum; i++)
			{
				data.P_Env[i].Num = f.Hio_Read8();

				f.Hio_Read(data.P_Env[i].Data, 1, 30);

				data.P_Env[i].Sus = f.Hio_Read8();
				data.P_Env[i].Loop = f.Hio_Read8();
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Chunk_Fe(Module_Data m, c_int size, Hio f, object parm)
		{
			Local_Data data = (Local_Data)parm;

			// Sanity check
			if (data.F_Env != null)
				return -1;

			data.F_EnvNum = f.Hio_Read8();
			if (data.F_EnvNum == 0)
				return 0;

			data.F_Env = ArrayHelper.InitializeArray<Mdl_Envelope>(data.F_EnvNum);
			if (data.F_Env == null)
				return -1;

			for (c_int i = 0; i < data.F_EnvNum; i++)
			{
				data.F_Env[i].Num = f.Hio_Read8();

				f.Hio_Read(data.F_Env[i].Data, 1, 30);

				data.F_Env[i].Sus = f.Hio_Read8();
				data.F_Env[i].Loop = f.Hio_Read8();
			}

			return 0;
		}
		#endregion

		#endregion
	}
}
