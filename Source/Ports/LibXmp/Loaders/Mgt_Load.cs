/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Text;
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
	internal class Mgt_Load : IFormatLoader
	{
		private static readonly uint32 Magic_MGT = Common.Magic4('\0', 'M', 'G', 'T');
		private static readonly uint32 Magic_MCS = Common.Magic4((char)0xbd, 'M', 'C', 'S');

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Mgt = new Format_Loader
		{
			Id = Guid.Parse("372E56E7-1D62-4558-B337-FE33939945DC"),
			Name = "Megatracker",
			Description = "One of the first Falcon-only trackers to be released. It supports up to 32 voices with 8-bit samples (interpolated to 16-bits). The tracker has a somewhat messy GUI, but still original with screen scrolling (downwards) to see all 32 tracks at once. Tracker was written by Simplet/Abstract and Axel Follet/MCS.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Mgt_Load(LibXmp libXmp)
		{
			lib = libXmp;
			encoder = EncoderCollection.Atari;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new instance of the loader
		/// </summary>
		/********************************************************************/
		private static IFormatLoader Create(LibXmp libXmp, Xmp_Context ctx)
		{
			return new Mgt_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			if (f.Hio_Read24B() != Magic_MGT)
				return -1;

			f.Hio_Read8();

			if (f.Hio_Read32B() != Magic_MCS)
				return -1;

			f.Hio_Seek(18, SeekOrigin.Current);
			c_int sng_Ptr = (c_int)f.Hio_Read32B();
			f.Hio_Seek(start + sng_Ptr, SeekOrigin.Begin);

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
			c_int[] sdata = new c_int[64];
			byte[] buf = new byte[32];

			f.Hio_Read24B();			// MGT
			c_int ver = f.Hio_Read8();
			f.Hio_Read32B();			// MCS

			lib.common.LibXmp_Set_Type(m, string.Format("Megatracker MGT v{0}.{1}", Ports.LibXmp.Common.Msn(ver), Ports.LibXmp.Common.Lsn(ver)));

			mod.Chn = f.Hio_Read16B();
			f.Hio_Read16B();			// Number of songs
			mod.Len = f.Hio_Read16B();
			mod.Pat = f.Hio_Read16B();
			mod.Trk = f.Hio_Read16B();
			mod.Ins = mod.Smp = f.Hio_Read16B();
			f.Hio_Read16B();			// Reserved
			f.Hio_Read32B();			// Reserved

			// Sanity check
			if ((mod.Chn > Constants.Xmp_Max_Channels) || (mod.Pat > Constants.Max_Patterns) || (mod.Ins > 64))
				return -1;

			c_int sng_Ptr = (c_int)f.Hio_Read32B();
			c_int seq_Ptr = (c_int)f.Hio_Read32B();
			c_int ins_Ptr = (c_int)f.Hio_Read32B();
			c_int pat_Ptr = (c_int)f.Hio_Read32B();
			c_int trk_Ptr = (c_int)f.Hio_Read32B();
			f.Hio_Read32B();			// Sample offset
			f.Hio_Read32B();			// Total smp len
			f.Hio_Read32B();			// Unpacked trk size

			f.Hio_Seek(start + sng_Ptr, SeekOrigin.Begin);

			f.Hio_Read(buf, 1, 32);
			mod.Name = encoder.GetString(buf);

			seq_Ptr = (c_int)f.Hio_Read32B();
			mod.Len = f.Hio_Read16B();
			mod.Rst = f.Hio_Read16B();
			mod.Bpm = f.Hio_Read8();
			mod.Spd = f.Hio_Read8();
			f.Hio_Read16B();			// Global volume
			f.Hio_Read8();				// Master L
			f.Hio_Read8();				// Master R

			// Sanity check
			if ((mod.Len > Constants.Xmp_Max_Mod_Length) || (mod.Rst > 255))
				return -1;

			for (c_int i = 0; i < mod.Chn; i++)
				f.Hio_Read16B();		// Pan

			m.C4Rate = Constants.C4_Ntsc_Rate;

			// Sequence
			f.Hio_Seek(start + seq_Ptr, SeekOrigin.Begin);

			for (c_int i = 0; i < mod.Len; i++)
			{
				c_int pos = f.Hio_Read16B();

				// Sanity check
				if (pos >= mod.Pat)
					return -1;

				mod.Xxo[i] = (byte)pos;
			}

			// Instruments
			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			f.Hio_Seek(start + ins_Ptr, SeekOrigin.Begin);

			for (c_int i = 0; i < mod.Ins; i++)
			{
				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					return -1;

				f.Hio_Read(buf, 1, 32);
				mod.Xxi[i].Name = encoder.GetString(buf);

				sdata[i] = (c_int)f.Hio_Read32B();
				mod.Xxs[i].Len = (c_int)f.Hio_Read32B();

				// Sanity check
				if (mod.Xxs[i].Len > Constants.Max_Sample_Size)
					return -1;

				mod.Xxs[i].Lps = (c_int)f.Hio_Read32B();
				mod.Xxs[i].Lpe = mod.Xxs[i].Lps + (c_int)f.Hio_Read32B();
				f.Hio_Read32B();
				f.Hio_Read32B();

				c_int c2Spd = (c_int)f.Hio_Read32B();
				lib.period.LibXmp_C2Spd_To_Note(c2Spd, out mod.Xxi[i].Sub[0].Xpo, out mod.Xxi[i].Sub[0].Fin);

				mod.Xxi[i].Sub[0].Vol = f.Hio_Read16B() >> 4;
				f.Hio_Read8();			// Vol L
				f.Hio_Read8();          // Vol R
				mod.Xxi[i].Sub[0].Pan = 0x80;

				c_int flags = f.Hio_Read8();
				mod.Xxs[i].Flg = (flags & 0x03) != 0 ? Xmp_Sample_Flag.Loop : Xmp_Sample_Flag.None;
				mod.Xxs[i].Flg |= (flags & 0x02) != 0 ? Xmp_Sample_Flag.Loop_BiDir : Xmp_Sample_Flag.None;

				mod.Xxi[i].Sub[0].Fin += 0 * f.Hio_Read8();		// FIXME

				f.Hio_Read8();			// Unused
				f.Hio_Read8();
				f.Hio_Read8();
				f.Hio_Read8();
				f.Hio_Read16B();
				f.Hio_Read32B();
				f.Hio_Read32B();

				mod.Xxi[i].Nsm = mod.Xxs[i].Len != 0 ? 1 : 0;
				mod.Xxi[i].Sub[0].Sid = i;
			}

			// Pattern init - alloc extra track
			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				return -1;

			// Tracks

			// Sanity check
			if (trk_Ptr >= f.Hio_Size())
				return -1;

			for (c_int i = 1; i < mod.Trk; i++)
			{
				f.Hio_Seek(start + trk_Ptr + i * 4, SeekOrigin.Begin);
				c_int offset = (c_int)f.Hio_Read32B();
				f.Hio_Seek(start + offset, SeekOrigin.Begin);

				c_int rows = f.Hio_Read16B();

				// Sanity check
				if (rows > 255)
					return -1;

				if (lib.common.LibXmp_Alloc_Track(mod, i, rows) < 0)
					return -1;

				for (c_int j = 0; j < rows; j++)
				{
					// TODO libxmp can't really support the wide effect
					// params Megatracker uses right now, but less bad
					// conversions of certain effects could be attempted
					uint8 b = f.Hio_Read8();
					j += b & 0x03;

					// Sanity check
					if (j >= rows)
						return -1;

					uint8 note = 0;
					Xmp_Event @event = mod.Xxt[i].Event[j];

					if ((b & 0x04) != 0)
						note = f.Hio_Read8();

					if ((b & 0x08) != 0)
						@event.Ins = f.Hio_Read8();

					if ((b & 0x10) != 0)
						@event.Vol = f.Hio_Read8();

					if ((b & 0x20) != 0)
						@event.FxT = f.Hio_Read8();

					if ((b & 0x40) != 0)
						@event.FxP = f.Hio_Read8();

					if ((b & 0x80) != 0)
						f.Hio_Read8();

					if (note == 1)
						@event.Note = Constants.Xmp_Key_Off;
					else if (note > 11)		// Adjusted to play codeine.mgt
						@event.Note = (byte)(note + 1);

					// Effects
					if (@event.FxT < 0x10)
					{
						// Like Amiga
					}
					else
					{
						switch (@event.FxT)
						{
							case 0x13:
							case 0x14:
							case 0x15:
							case 0x17:
							case 0x1c:
							case 0x1d:
							case 0x1e:
							{
								@event.FxT = Effects.Fx_Extended;
								@event.FxP = (byte)(((@event.FxT & 0x0f) << 4) | (@event.FxP & 0x0f));
								break;
							}

							default:
							{
								@event.FxT = @event.FxP = 0;
								break;
							}
						}
					}

					// Volume and volume column effects
					if ((@event.Vol >= 0x10) && (@event.Vol <= 0x50))
					{
						@event.Vol -= 0x0f;
						continue;
					}

					switch (@event.Vol >> 4)
					{
						// Volume slide down
						case 0x06:
						{
							@event.F2T = Effects.Fx_VolSlide_2;
							@event.F2P = (byte)(@event.Vol - 0x60);
							break;
						}

						// Volume slide up
						case 0x07:
						{
							@event.F2T = Effects.Fx_VolSlide_2;
							@event.F2P = (byte)((@event.Vol - 0x70) << 4);
							break;
						}

						// Fine volume slide down
						case 0x08:
						{
							@event.F2T = Effects.Fx_Extended;
							@event.F2P = (byte)((Effects.Ex_F_VSlide_Dn << 4) | (@event.Vol - 0x80));
							break;
						}

						// Fine volume slide up
						case 0x09:
						{
							@event.F2T = Effects.Fx_Extended;
							@event.F2P = (byte)((Effects.Ex_F_VSlide_Up << 4) | (@event.Vol - 0x90));
							break;
						}

						// Set vibrato speed
						case 0x0a:
						{
							@event.F2T = Effects.Fx_Vibrato;
							@event.F2P = (byte)((@event.Vol - 0xa0) << 4);
							break;
						}

						// Vibrato
						case 0x0b:
						{
							@event.F2T = Effects.Fx_Vibrato;
							@event.F2P = (byte)(@event.Vol - 0xb0);
							break;
						}

						// Set panning
						case 0x0c:
						{
							@event.F2T = Effects.Fx_SetPan;
							@event.F2P = (byte)(((@event.Vol - 0xc0) << 4) + 8);
							break;
						}

						// Pan slide left
						case 0x0d:
						{
							@event.F2T = Effects.Fx_PanSlide;
							@event.F2P = (byte)((@event.Vol - 0xd0) << 4);
							break;
						}

						// Pan slide right
						case 0x0e:
						{
							@event.F2T = Effects.Fx_PanSlide;
							@event.F2P = (byte)(@event.Vol - 0xe0);
							break;
						}

						// Tone portamento
						case 0x0f:
						{
							@event.F2T = Effects.Fx_TonePorta;
							@event.F2P = (byte)((@event.Vol - 0xf0) << 4);
							break;
						}
					}

					@event.Vol = 0;
				}
			}

			// Extra track
			if (mod.Trk > 0)
			{
				mod.Xxt[0] = new Xmp_Track();
				mod.Xxt[0].Rows = 64;
				mod.Xxt[0].Event = ArrayHelper.InitializeArray<Xmp_Event>(64);
			}

			// Read and convert patterns
			f.Hio_Seek(start + pat_Ptr, SeekOrigin.Begin);

			for (c_int i = 0; i < mod.Pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern(mod, i) < 0)
					return -1;

				c_int rows = f.Hio_Read16B();

				// Sanity check
				if (rows > 256)
					return -1;

				mod.Xxp[i].Rows = rows;

				for (c_int j = 0; j < mod.Chn; j++)
				{
					c_int track = f.Hio_Read16B() - 1;

					// Sanity check
					if (track >= mod.Trk)
						return -1;

					mod.Xxp[i].Index[j] = track;
				}
			}

			// Read samples
			for (c_int i = 0; i < mod.Ins; i++)
			{
				if (mod.Xxi[i].Nsm == 0)
					continue;

				f.Hio_Seek(start + sdata[i], SeekOrigin.Begin);

				if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, mod.Xxs[i], null, i) < 0)
					return -1;
			}

			m.Module_Flags = Xmp_Module_Flags.Uses_Tracks;

			return 0;
		}
	}
}
