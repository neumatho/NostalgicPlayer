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
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Lzw;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// 
	/// </summary>
	internal class Sym_Load : IFormatLoader
	{
		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Sym = new Format_Loader
		{
			Id = Guid.Parse("D09B8566-D2FD-4314-82B9-55EF7A7CD82E"),
			Name = "Digital Symphony",
			Description = "Tracker created by Oregan Developments for the Acorn computer.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Sym_Load(LibXmp libXmp)
		{
			lib = libXmp;
			encoder = EncoderCollection.Acorn;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new instance of the loader
		/// </summary>
		/********************************************************************/
		private static IFormatLoader Create(LibXmp libXmp, Xmp_Context ctx)
		{
			return new Sym_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			uint32 a = f.Hio_Read32B();
			uint32 b = f.Hio_Read32B();

			if ((a != 0x02011313) || (b != 0x1412010b))     // BASSTRAK
				return -1;

			c_int ver = f.Hio_Read8();

			// v1 files are the same as v0 but may contain strange compression
			// formats. Deal with that problem later if it arises
			if (ver > 1)
				return -1;

			f.Hio_Read8();		// chn
			f.Hio_Read16L();    // pat
			f.Hio_Read16L();    // trk
			f.Hio_Read24L();    // infolen

			for (c_int i = 0; i < 63; i++)
			{
				if ((~f.Hio_Read8() & 0x80) != 0)
					f.Hio_Read24L();
			}

			lib.common.LibXmp_Read_Title(f, out t, f.Hio_Read8(), encoder);

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
			c_int[] sn = new c_int[64];
			c_int size, ret;
			c_int max_Sample_Size = 1;
			uint8[] allowed_Effects = new uint8[8];
			uint8[] tmpBuf = new uint8[33];

			f.Hio_Seek(8, SeekOrigin.Begin);	// BASSTRAK

			f.Hio_Read8();		// Version
			lib.common.LibXmp_Set_Type(m, "Digital Symphony");

			mod.Chn = f.Hio_Read8();
			mod.Len = mod.Pat = f.Hio_Read16L();

			// Sanity check
			if ((mod.Chn < 1) || (mod.Chn > 8) || (mod.Pat > Constants.Xmp_Max_Mod_Length))
				return -1;

			mod.Trk = f.Hio_Read16L();
			c_int infoLen = (c_int)f.Hio_Read24L();

			// Sanity check - track 0x1000 is used to indicate the empty track
			if (mod.Trk > 0x1000)
				return -1;

			mod.Ins = mod.Smp = 63;

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			for (i = 0; i < mod.Ins; i++)
			{
				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					return -1;

				sn[i] = f.Hio_Read8();	// Sample name length

				if ((~sn[i] & 0x80) != 0)
				{
					mod.Xxs[i].Len = (c_int)(f.Hio_Read24L() << 1);
					mod.Xxi[i].Nsm = 1;

					// Sanity check
					if (mod.Xxs[i].Len > 0x80000)
						return -1;

					if (max_Sample_Size < mod.Xxs[i].Len)
						max_Sample_Size = mod.Xxs[i].Len;
				}
			}

			uint32 a = f.Hio_Read8();	// Track name length

			if (a > 32)
			{
				f.Hio_Read(tmpBuf, 1, 32);
				f.Hio_Seek((c_int)(a - 32), SeekOrigin.Begin);
			}
			else
				f.Hio_Read(tmpBuf, 1, a);

			mod.Name = encoder.GetString(tmpBuf);

			f.Hio_Read(allowed_Effects, 1, 8);

			mod.Trk++;		// Alloc extra empty track

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				return -1;

			// Determine the required size of temporary buffer and allocate it now.
			// Uncompressed sequence size
			c_int sequence_Size = mod.Len * mod.Chn * 2;
			if (sequence_Size > max_Sample_Size)
				max_Sample_Size = sequence_Size;

			c_int tracks_Size = 64 * (mod.Trk - 1) * 4;	// Uncompressed tracks size
			if (tracks_Size > max_Sample_Size)
				max_Sample_Size = tracks_Size;

			CPointer<uint8> buf = CMemory.MAlloc<uint8>(max_Sample_Size);
			if (buf.IsNull)
				return -1;

			// Sequence
			a = f.Hio_Read8();		// Packing

			if ((a != 0) && (a != 1))
				goto Err;

			if (a != 0)
			{
				if (Lzw.LibXmp_Read_Lzw(buf, (size_t)sequence_Size, (size_t)sequence_Size, Lzw_Flag.Sym, f) < 0)
					goto Err;
			}
			else
			{
				if (f.Hio_Read(buf, 1, (size_t)sequence_Size) != (size_t)sequence_Size)
					goto Err;
			}

			for (i = 0; i < mod.Len; i++)		// len == pat
			{
				if (lib.common.LibXmp_Alloc_Pattern(mod, i) < 0)
					goto Err;

				mod.Xxp[i].Rows = 64;

				for (c_int j = 0; j < mod.Chn; j++)
				{
					c_int idx = 2 * (i * mod.Chn + j);
					c_int t = (c_int)ReadPtr16L(buf + idx);

					if (t == 0x1000)
					{
						// Empty track
						t = mod.Trk - 1;
					}
					else if (t >= (mod.Trk - 1))
					{
						// Sanity check
						goto Err;
					}

					mod.Xxp[i].Index[j] = t;
				}

				mod.Xxo[i] = (byte)i;
			}

			// Read and convert patterns
			//
			// Patterns are stored in blocks of up to 2000 patterns. If there are
			// more than 2000 patterns, they need to be read in multiple passes
			//
			// See 4096_patterns.dsym
			for (i = 0; i < tracks_Size; i += 4 * 64 * 2000)
			{
				c_int blk_Size = Math.Min(tracks_Size - i, 4 * 64 * 2000);

				a = f.Hio_Read8();

				if ((a != 0) && (a != 1))
					goto Err;

				if (a != 0)
				{
					if (Lzw.LibXmp_Read_Lzw(buf + i, (size_t)blk_Size, (size_t)blk_Size, Lzw_Flag.Sym, f) < 0)
						goto Err;
				}
				else
				{
					if (f.Hio_Read(buf + i, 1, (size_t)blk_Size) != (size_t)blk_Size)
						goto Err;
				}
			}

			for (i = 0; i < (mod.Trk - 1); i++)
			{
				if (lib.common.LibXmp_Alloc_Track(mod, i, 64) < 0)
					goto Err;

				for (c_int j = 0; j < mod.Xxt[i].Rows; j++)
				{
					Xmp_Event @event = mod.Xxt[i].Event[j];

					uint32 b = ReadPtr32L(buf + 4 * (i * 64 + j));

					@event.Note = (byte)(b & 0x0000003f);
					if (@event.Note != 0)
						@event.Note += 48;

					@event.Ins = (byte)((b & 0x00001fc0) >> 6);
					@event.FxT = (byte)((b & 0x000fc000) >> 14);
					c_int parm = (c_int)((b & 0xfff00000) >> 20);

					if ((allowed_Effects[@event.FxT >> 3] & (1 << (@event.FxT & 7))) != 0)
						Fix_Effect(@event, parm);
					else
						@event.FxT = 0;
				}
			}

			// Empty track
			if (lib.common.LibXmp_Alloc_Track(mod, i, 64) < 0)
				goto Err;

			// Load and convert instruments
			for (i = 0; i < mod.Ins; i++)
			{
				Xmp_Sample xxs = mod.Xxs[i];
				Xmp_Instrument xxi = mod.Xxi[i];
				Extra_Sample_Data xtra = m.Xtra[i];
				uint8[] nameBuf = new uint8[128];

				f.Hio_Read(nameBuf, 1, (size_t)sn[i] & 0x7f);
				lib.common.LibXmp_Instrument_Name(mod, i, nameBuf, 32, encoder);

				if ((~sn[i] & 0x80) != 0)
				{
					xtra.Sus = (c_int)(f.Hio_Read24L() << 1);
					c_int loopLen = (c_int)(f.Hio_Read24L() << 1);
					xtra.Sue = xtra.Sus + loopLen;

					if ((xtra.Sus < xxs.Len) && (xtra.Sus < xtra.Sue) && (xtra.Sue <= xxs.Len) && (loopLen > 2))
						xxs.Flg |= Xmp_Sample_Flag.SLoop;

					xxi.Sub[0].Vol = f.Hio_Read8();
					xxi.Sub[0].Pan = 0x80;

					// Finetune adjusted comparing DSym and S3M versions
					// of "inside out"
					xxi.Sub[0].Fin = (int8)(f.Hio_Read8() << 4);
					xxi.Sub[0].Sid = i;
				}

				if (((sn[i] & 0x80) != 0) || (xxs.Len == 0))
					continue;

				a = f.Hio_Read8();

				Hio s = f.GetSampleHio(i, xxs.Len);

				switch (a)
				{
					// Signed 8-bit, logarithmic
					case 0:
					{
						ret = Sample.LibXmp_Load_Sample(m, s, Sample_Flag.Vidc, xxs, null);
						break;
					}

					// LZW compressed signed 8-bit delta, linear
					case 1:
					{
						size = xxs.Len;

						if (Lzw.LibXmp_Read_Lzw(buf, (size_t)size, (size_t)size, Lzw_Flag.Sym, s) < 0)
							goto Err;

						ret = Sample.LibXmp_Load_Sample(m, null, Sample_Flag.NoLoad | Sample_Flag.Diff, xxs, buf);
						break;
					}

					// Signed 8-bit, linear
					case 2:
					{
						ret = Sample.LibXmp_Load_Sample(m, s, Sample_Flag.None, xxs, null);
						break;
					}

					// Signed 16-bit, linear
					case 3:
					{
						xxs.Flg |= Xmp_Sample_Flag._16Bit;
						ret = Sample.LibXmp_Load_Sample(m, s, Sample_Flag.None, xxs, null);
						break;
					}

					// Sigma-delta compressed unsigned 8-bit, linear
					case 4:
					{
						size = xxs.Len;

						if (Lzw.LibXmp_Read_Sigma_Delta(buf, (size_t)size, (size_t)size, s) < 0)
							goto Err;

						ret = Sample.LibXmp_Load_Sample(m, null, Sample_Flag.NoLoad | Sample_Flag.Uns, xxs, buf);
						break;
					}

					// Sigma-delta compressed signed 8-bit, logarithmic
					case 5:
					{
						size = xxs.Len;

						if (Lzw.LibXmp_Read_Sigma_Delta(buf, (size_t)size, (size_t)size, s) < 0)
							goto Err;

						// This uses a bit packing that isn't either mu-law or
						// normal Archimedes VIDC. Convert to the latter
						for (c_int j = 0; j < size; j++)
						{
							uint8 t = (uint8)((buf[j] < 128) ? ~buf[j] : buf[j]);
							buf[j] = (uint8)((buf[j] >> 7) | (t << 1));
						}

						ret = Sample.LibXmp_Load_Sample(m, null, Sample_Flag.NoLoad | Sample_Flag.Vidc, xxs, buf);
						break;
					}

					default:
					{
						s.Hio_Close();
						goto Err;
					}
				}

				s.Hio_Close();

				if (ret < 0)
					goto Err;
			}

			// Information text
			if (infoLen > 0)
			{
				a = f.Hio_Read8();      // Packing

				CPointer<uint8> comment = CMemory.MAlloc<uint8>(infoLen + 1);
				if (comment.IsNotNull)
				{
					comment[infoLen] = 0;

					if (a != 0)
						ret = Lzw.LibXmp_Read_Lzw(comment, (size_t)infoLen, (size_t)infoLen, Lzw_Flag.Sym, f);
					else
					{
						size = (c_int)f.Hio_Read(comment, 1, (size_t)infoLen);
						ret = (size < infoLen) ? -1 : 0;
					}

					if (ret < 0)
					{
						CMemory.Free(comment);
						comment.SetToNull();
					}
					else
						m.Comment = encoder.GetString(comment.Buffer, comment.Offset, comment.Length);
				}
			}

			for (i = 0; i < mod.Chn; i++)
				mod.Xxc[i].Pan = Common.DefPan(m, (((i + 3) / 2) % 2) * 0xff);

			m.Quirk = Quirk_Flag.VibAll | Quirk_Flag.KeyOff | Quirk_Flag.InvLoop;
			m.Module_Flags = Xmp_Module_Flags.Uses_Tracks;

			CMemory.Free(buf);

			return 0;

			Err:
			CMemory.Free(buf);

			return -1;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Fix_Effect(Xmp_Event e, c_int parm)
		{
			switch (e.FxT)
			{
				// 00 xyz Normal play or arpeggio + Volume Slide Up
				// 01 xyy Slide Up + Volume Slide Up
				// 02 xyy Slide Down + Volume Slide Up
				case 0x00:
				case 0x01:
				case 0x02:
				{
					if ((parm & 0xff) != 0)
						e.FxP = (byte)(parm & 0xff);
					else
						e.FxT = 0;

					if ((parm >> 8) != 0)
					{
						e.F2T = Effects.Fx_VolSlide_Up;
						e.F2P = (byte)(parm >> 8);
					}
					break;
				}

				// 03 xyy Tone Portamento
				// 04 xyz Vibrato
				// 07 xyz Tremolo
				case 0x03:
				case 0x04:
				case 0x07:
				{
					e.FxP = (byte)parm;
					break;
				}

				// 05 xyz Tone Portamento + Volume Slide
				// 06 xyz Vibrato + Volume Slide
				case 0x05:
				case 0x06:
				{
					e.FxP = (byte)parm;

					if (parm == 0)
						e.FxT -= 2;

					break;
				}

				// 09 xxx Set Sample Offset
				case 0x09:
				{
					e.FxP = (byte)(parm >> 1);
					e.F2T = Effects.Fx_HiOffset;
					e.F2P = (byte)(parm >> 9);
					break;
				}

				// 0A xyz Volume Slide + Fine Slide Up
				case 0x0a:
				{
					if ((parm & 0xff) != 0)
						e.FxP = (byte)(parm & 0xff);
					else
						e.FxT = 0;

					e.F2T = Effects.Fx_Extended;
					e.F2P = (byte)((Effects.Ex_F_Porta_Up << 4) | ((parm & 0xf00) >> 8));
					break;
				}

				// 0B xxx Position jump
				// 0C xyy Set Volume
				case 0x0b:
				case 0x0c:
				{
					e.FxP = (byte)parm;
					break;
				}

				// 0D xyy Pattern break
				case 0x0d:
				{
					e.FxT = Effects.Fx_It_Break;
					e.FxP = (byte)((parm & 0xff) < 0x40 ? parm : 0);
					break;
				}

				// 0F xxx Set Speed
				case 0x0f:
				{
					if (parm != 0)
					{
						e.FxT = Effects.Fx_S3M_Speed;
						e.FxP = (byte)Math.Min(parm, 255);
					}
					else
						e.FxT = 0;

					break;
				}

				// 13 xxy Glissando Control
				case 0x13:
				{
					e.FxT = Effects.Fx_Extended;
					e.FxP = (byte)((Effects.Ex_Gliss << 4) | (parm & 0x0f));
					break;
				}

				// 14 xxy Set Vibrato Waveform
				case 0x14:
				{
					e.FxT = Effects.Fx_Extended;
					e.FxP = (byte)((Effects.Ex_Vibrato_Wf << 4) | (parm & 0x0f));
					break;
				}

				// 15 xxy Set Fine Tune
				case 0x15:
				{
					e.FxT = Effects.Fx_Extended;
					e.FxP = (byte)((Effects.Ex_FineTune << 4) | (parm & 0x0f));
					break;
				}

				// 16 xxx Jump to Loop
				case 0x16:
				{
					// TODO: 16, 19 should be able to support larger params
					e.FxT = Effects.Fx_Extended;
					e.FxP = (byte)((Effects.Ex_Pattern_Loop << 4) | (Math.Min(parm, 0x0f)));
					break;
				}

				// 17 xxy Set Tremolo Waveform
				case 0x17:
				{
					e.FxT = Effects.Fx_Extended;
					e.FxP = (byte)((Effects.Ex_Tremolo_Wf << 4) | (parm & 0x0f));
					break;
				}

				// 19 xxx Retrig Note
				case 0x19:
				{
					if (parm < 0x100)
					{
						e.FxT = Effects.Fx_Retrig;
						e.FxP = (byte)(parm & 0xff);
					}
					else
					{
						// Ignore
						e.FxT = 0;
					}
					break;
				}

				// 11 xyy Fine Slide Up + Fine Volume Slide Up
				// 12 xyy Fine Slide Down + Fine Volume Slide Up
				// 1A xyy Fine Slide Up + Fine Volume Slide Down
				// 1B xyy Fine Slide Down + Fine Volume Slide Down
				case 0x11:
				case 0x12:
				case 0x1a:
				case 0x1b:
				{
					uint8 pitch_Effect = (((e.FxT == 0x11) || (e.FxT == 0x1a)) ? Effects.Fx_F_Porta_Up : Effects.Fx_F_Porta_Dn);
					uint8 vol_Effect = (((e.FxT == 0x11) || (e.FxT == 0x12)) ? Effects.Fx_F_VSlide_Up : Effects.Fx_F_VSlide_Dn);

					if ((parm & 0xff) != 0)
					{
						e.FxT = pitch_Effect;
						e.FxP = (byte)(parm & 0xff);
					}
					else
						e.FxT = 0;

					if ((parm >> 8) != 0)
					{
						e.F2T = vol_Effect;
						e.F2P = (byte)(parm >> 8);
					}
					break;
				}

				// 1C xxx Note Cut
				case 0x1c:
				{
					// TODO: 1c, 1d, 1e should be able to support larger params
					e.FxT = Effects.Fx_Extended;
					e.FxP = (byte)((Effects.Ex_Cut << 4) | (Math.Min(parm, 0x0f)));
					break;
				}

				// 1D xxx Note Delay
				case 0x1d:
				{
					e.FxT = Effects.Fx_Extended;
					e.FxP = (byte)((Effects.Ex_Delay << 4) | (Math.Min(parm, 0x0f)));
					break;
				}

				// 1E xxx Pattern Delay
				case 0x1e:
				{
					e.FxT = Effects.Fx_Extended;
					e.FxP = (byte)((Effects.Ex_Patt_Delay << 4) | (Math.Min(parm, 0x0f)));
					break;
				}

				// 1F xxy Invert Loop
				case 0x1f:
				{
					e.FxT = Effects.Fx_Extended;
					e.FxP = (byte)((Effects.Ex_InvLoop << 4) | (parm & 0x0f));
					break;
				}

				// 20 xyz Normal play on Arpeggio + Volume Slide Down
				case 0x20:
				{
					e.FxT = Effects.Fx_Arpeggio;
					e.FxP = (byte)(parm & 0xff);

					if ((parm >> 8) != 0)
					{
						e.F2T = Effects.Fx_VolSlide_Dn;
						e.F2P = (byte)(parm >> 8);
					}
					break;
				}

				// 21 xyy Slide Up + Volume Slide Down
				case 0x21:
				{
					if ((parm & 0xff) != 0)
					{
						e.FxT = Effects.Fx_Porta_Up;
						e.FxP = (byte)(parm & 0xff);
					}
					else
						e.FxT = 0;

					if ((parm >> 8) != 0)
					{
						e.F2T = Effects.Fx_VolSlide_Dn;
						e.F2P = (byte)(parm >> 8);
					}
					break;
				}

				// 22 xyy Slide Down + Volume Slide Down
				case 0x22:
				{
					if ((parm & 0xff) != 0)
					{
						e.FxT = Effects.Fx_Porta_Dn;
						e.FxP = (byte)(parm & 0xff);
					}
					else
						e.FxT = 0;

					if ((parm >> 8) != 0)
					{
						e.F2T = Effects.Fx_VolSlide_Dn;
						e.F2P = (byte)(parm >> 8);
					}
					break;
				}

				// 2A xyz Volume Slide + Fine Slide Down
				case 0x2a:
				{
					if ((parm & 0xff) != 0)
					{
						e.FxT = Effects.Fx_VolSlide;
						e.FxP = (byte)(parm & 0xff);
					}
					else
						e.FxT = 0;

					e.F2T = Effects.Fx_Extended;
					e.F2P = (byte)((Effects.Ex_F_Porta_Dn << 4) | (parm >> 8));
					break;
				}

				// 2B xyy Line Jump
				case 0x2b:
				{
					e.FxT = Effects.Fx_Line_Jump;
					e.FxP = (byte)((parm < 0x40) ? parm : 0);
					break;
				}

				// 2F xxx Set Tempo
				case 0x2f:
				{
					if (parm != 0)
					{
						parm = (parm + 4) >> 3;		// Round to nearest
						Ports.LibXmp.Common.Clamp(ref parm, Constants.Xmp_Min_Bpm, 255);

						e.FxT = Effects.Fx_S3M_Bpm;
						e.FxP = (byte)parm;
					}
					else
						e.FxT = 0;

					break;
				}

				// 30 xxy Set Stereo
				case 0x30:
				{
					e.FxT = Effects.Fx_SetPan;

					if ((parm & 7) != 0)
					{
						// !Tracker-style panning: 1=left, 4=center, 7=right
						if (!((parm & 8) != 0))
							e.FxP = (byte)(42 * ((parm & 7) - 1) + 2);
						else
							e.FxT = 0;
					}
					else
					{
						parm >>= 4;

						if (parm < 128)
							e.FxP = (byte)(parm + 128);
						else if (parm > 128)
							e.FxP = (byte)(255 - parm);
						else
							e.FxT = 0;
					}
					break;
				}

				// 31 xxx Song Upcall
				case 0x31:
				{
					e.FxT = 0;
					break;
				}

				// 32 xxx Unset Sample Repeat
				case 0x32:
				{
					e.FxT = Effects.Fx_Keyoff;
					e.FxP = 0;
					break;
				}

				default:
				{
					e.FxT = 0;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint32 ReadPtr32L(CPointer<uint8> p)
		{
			uint32 a = p[0];
			uint32 b = p[1];
			uint32 c = p[2];
			uint32 d = p[3];

			return (d << 24) | (c << 16) | (b << 8) | a;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint32 ReadPtr16L(CPointer<uint8> p)
		{
			uint32 a = p[0];
			uint32 b = p[1];

			return (b << 8) | a;
		}
		#endregion
	}
}
