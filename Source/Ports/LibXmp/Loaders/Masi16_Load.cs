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
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Loader;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// 
	/// </summary>
	internal class Masi16_Load : IFormatLoader
	{
		// ReSharper disable InconsistentNaming
		private static readonly uint32 Magic_PSM_ = Common.Magic4('P', 'S', 'M', (char)0xfe);
		// ReSharper restore InconsistentNaming

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Masi16 = new Format_Loader
		{
			Id = Guid.Parse("C3D4C58B-E9C5-4F99-9DFB-FB1588FCE2C8"),
			Name = "Epic MegaGames MASI 16",
			Description = "MASI also known as PSM (ProTracker Studio Module) is a proprietary file format used by Epic in all their new games. This is an earlier version of the format.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Masi16_Load(LibXmp libXmp)
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
			return new Masi16_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			if (f.Hio_Read32B() != Magic_PSM_)
				return -1;

			lib.common.LibXmp_Read_Title(f, out t, 60, encoder);

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
			CPointer<uint8> buf = new CPointer<uint8>(1024);
			uint32[] p_Smp = new uint32[256];
			uint8[] sample_Map = new uint8[256];

			f.Hio_Read32B();

			f.Hio_Read(buf, 1, 60);
			mod.Name = encoder.GetString(buf.Buffer, buf.Offset, 59);

			c_int type = f.Hio_Read8();		// Song type
			c_int ver = f.Hio_Read8();		// Song version
			f.Hio_Read8();					// Pattern version

			if ((type & 0x01) != 0)			// Song mode not supported
				return -1;

			lib.common.LibXmp_Set_Type(m, string.Format("Epic MegaGames MASI 16 PSM {0}.{1:d02}", Ports.LibXmp.Common.Msn(ver), Ports.LibXmp.Common.Lsn(ver)));

			mod.Spd = f.Hio_Read8();
			mod.Bpm = f.Hio_Read8();
			f.Hio_Read8();					// Master volume
			f.Hio_Read16L();				// Song length
			mod.Len = f.Hio_Read16L();
			mod.Pat = f.Hio_Read16L();
			c_int stored_Ins = f.Hio_Read16L();
			f.Hio_Read16L();				// Ignore channels to play
			mod.Chn = f.Hio_Read16L();

			// Sanity check
			if ((mod.Len > 256) || (mod.Pat > 256) || (stored_Ins > 255) || (mod.Chn > Constants.Xmp_Max_Channels))
				return -1;

			mod.Trk = mod.Pat * mod.Chn;

			uint32 p_Ord = f.Hio_Read32L();
			uint32 p_Chn = f.Hio_Read32L();
			uint32 p_Pat = f.Hio_Read32L();
			uint32 p_Ins = f.Hio_Read32L();

			m.C4Rate = Constants.C4_Ntsc_Rate;

			f.Hio_Seek((c_long)(start + p_Ord), SeekOrigin.Begin);
			f.Hio_Read(mod.Xxo, 1, (size_t)mod.Len);

			CMemory.memset<uint8>(buf, 0, (size_t)mod.Chn);
			f.Hio_Seek((c_long)(start + p_Chn), SeekOrigin.Begin);
			f.Hio_Read(buf, 1, 16);

			for (c_int i = 0; i < mod.Chn; i++)
			{
				if (buf[i] < 16)
					mod.Xxc[i].Pan = buf[i] | (buf[i] << 4);
			}

			// Get the actual instruments count...
			mod.Ins = 0;

			for (c_int i = 0; i < stored_Ins; i++)
			{
				f.Hio_Seek((c_long)(start + p_Ins + 64 * i + 45), SeekOrigin.Begin);
				sample_Map[i] = (uint8)(f.Hio_Read16L() - 1);
				mod.Ins = Math.Max(mod.Ins, sample_Map[i] + 1);
			}

			if ((mod.Ins > 255) || (f.Hio_Error() != 0))
				return -1;

			mod.Smp = mod.Ins;

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			CMemory.memset<uint32>(p_Smp, 0, (size_t)p_Smp.Length);

			f.Hio_Seek((c_long)(start + p_Ins), SeekOrigin.Begin);

			for (c_int i = 0; i < stored_Ins; i++)
			{
				c_int num = sample_Map[i];

				if (f.Hio_Read(buf, 1, 64) < 64)
					return -1;

				Xmp_Instrument xxi = mod.Xxi[num];
				Xmp_Sample xxs = mod.Xxs[num];

				// Don't load duplicate instruments
				if (xxi.Sub != null)
					continue;

				if (lib.common.LibXmp_Alloc_SubInstrument(mod, num, 1) < 0)
					return -1;

				Xmp_SubInstrument sub = xxi.Sub[0];

				xxi.Name = encoder.GetString(buf.Buffer, buf.Offset + 13, 24);
				p_Smp[i] = DataIo.ReadMem32L(buf + 37);

				uint16 flags = buf[47];		// Sample type
				xxs.Len = (c_int)DataIo.ReadMem32L(buf + 48);
				xxs.Lps = (c_int)DataIo.ReadMem32L(buf + 52);
				xxs.Lpe = (c_int)DataIo.ReadMem32L(buf + 56);

				c_int fineTune = buf[60];
				sub.Vol = buf[61];
				uint16 c2Spd = DataIo.ReadMem16L(buf + 62);
				sub.Pan = 0x80;
				sub.Sid = num;

				xxs.Flg = (flags & 0x80) != 0 ? Xmp_Sample_Flag.Loop : Xmp_Sample_Flag.None;
				xxs.Flg |= (flags & 0x20) != 0 ? Xmp_Sample_Flag.Loop_BiDir : Xmp_Sample_Flag.None;

				lib.period.LibXmp_C2Spd_To_Note(c2Spd, out sub.Xpo, out sub.Fin);
				sub.Fin += (int8)((fineTune & 0x0f) << 4);
				sub.Xpo += (fineTune >> 4) - 7;

				// The documentation claims samples shouldn't exceed 64k. The
				// PS16 modules from Silverball and Epic Pinball confirm this.
				// Later Protracker Studio Modules (MASI) allow up to 1MB
				if ((uint32)xxs.Len > 64 * 1024)
					return -1;

				if (xxs.Len > 0)
					xxi.Nsm = 1;
			}

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				return -1;

			f.Hio_Seek((c_long)(start + p_Pat), SeekOrigin.Begin);

			for (c_int i = 0; i < mod.Pat; i++)
			{
				c_int len = f.Hio_Read16L() - 4;

				uint8 rows = f.Hio_Read8();
				if (rows > 64)
					return -1;

				uint8 chan = f.Hio_Read8();
				if (chan > 32)
					return -1;

				if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, rows) < 0)
					return -1;

				for (c_int r = 0; r < rows; r++)
				{
					while (len > 0)
					{
						uint8 b = f.Hio_Read8();
						len--;

						if (b == 0)
							break;

						c_int c = b & 0x0f;
						if (c >= mod.Chn)
							return -1;

						Xmp_Event @event = Ports.LibXmp.Common.Event(m, i, c, r);

						if ((b & 0x80) != 0)
						{
							@event.Note = (byte)(f.Hio_Read8() + 36);
							@event.Ins = f.Hio_Read8();
							len -= 2;
						}

						if ((b & 0x40) != 0)
						{
							@event.Vol = (byte)(f.Hio_Read8() + 1);
							len--;
						}

						if ((b & 0x20) != 0)
						{
							uint8 effect = f.Hio_Read8();
							uint8 param = f.Hio_Read8();
							uint8 param2 = 0;
							uint8 param3 = 0;

							if (effect == 40)	// Sample offset
							{
								param2 = f.Hio_Read8();
								param3 = f.Hio_Read8();
							}

							Masi16_Translate_Effect(@event, effect, param, param2, param3);
							len -= 2;
						}
					}
				}

				if (len > 0)
					f.Hio_Seek(len, SeekOrigin.Current);
			}

			// Read samples
			for (c_int i = 0; i < stored_Ins; i++)
			{
				Xmp_Sample xxs = mod.Xxs[sample_Map[i]];

				// Don't load duplicate sample data
				if (xxs.Data.IsNotNull)
					continue;

				f.Hio_Seek((c_long)(start + p_Smp[i]), SeekOrigin.Begin);

				if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.Diff, xxs, null, i) < 0)
					return -1;
			}

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Masi16_Translate_Effect(Xmp_Event @event, uint8 effect, uint8 param, uint8 param2, uint8 param3)
		{
			switch (effect)
			{
				// Fine volume slide up
				case 1:
				{
					@event.FxT = Effects.Fx_F_VSlide_Up;
					@event.FxP = param;
					break;
				}

				// Volume slide up
				case 2:
				{
					@event.FxT = Effects.Fx_VolSlide_Up;
					@event.FxP = param;
					break;
				}

				// Fine volume slide down
				case 3:
				{
					@event.FxT = Effects.Fx_F_VSlide_Dn;
					@event.FxP = param;
					break;
				}

				// Volume slide down
				case 4:
				{
					@event.FxT = Effects.Fx_VolSlide_Dn;
					@event.FxP = param;
					break;
				}

				// Fine portamento up
				case 10:
				{
					@event.FxT = Effects.Fx_F_Porta_Up;
					@event.FxP = param;
					break;
				}

				// Portamento up
				case 11:
				{
					@event.FxT = Effects.Fx_Porta_Up;
					@event.FxP = param;
					break;
				}

				// Fine portamento down
				case 12:
				{
					@event.FxT = Effects.Fx_F_Porta_Dn;
					@event.FxP = param;
					break;
				}

				// Portamento down
				case 13:
				{
					@event.FxT = Effects.Fx_Porta_Dn;
					@event.FxP = param;
					break;
				}

				// Tone portamento
				case 14:
				{
					@event.FxT = Effects.Fx_TonePorta;
					@event.FxP = param;
					break;
				}

				// Glissando control
				case 15:
				{
					@event.FxT = Effects.Fx_Extended;
					@event.FxP = (uint8)((Effects.Ex_Gliss << 4) | (param & 0x0f));
					break;
				}

				// Tone portamento + volume slide up
				case 16:
				{
					@event.FxT = Effects.Fx_TonePorta;
					@event.FxP = 0;
					@event.F2T = Effects.Fx_VolSlide_Up;
					@event.F2P = param;
					break;
				}

				// Tone portamento + volume slide down
				case 17:
				{
					@event.FxT = Effects.Fx_TonePorta;
					@event.FxP = 0;
					@event.F2T = Effects.Fx_VolSlide_Dn;
					@event.F2P = param;
					break;
				}

				// Vibrato
				case 20:
				{
					@event.FxT = Effects.Fx_Vibrato;
					@event.FxP = param;
					break;
				}

				// Vibrato waveform
				case 21:
				{
					@event.FxT = Effects.Fx_Extended;
					@event.FxP = (uint8)((Effects.Ex_Vibrato_Wf << 4) | (param & 0x0f));
					break;
				}

				// Vibrato + volume slide up
				case 22:
				{
					@event.FxT = Effects.Fx_Vibrato;
					@event.FxP = 0;
					@event.F2T = Effects.Fx_VolSlide_Up;
					@event.F2P = param;
					break;
				}

				// Vibrato + volume slide down
				case 23:
				{
					@event.FxT = Effects.Fx_Vibrato;
					@event.FxP = 0;
					@event.F2T = Effects.Fx_VolSlide_Dn;
					@event.F2P = param;
					break;
				}

				// Tremolo
				case 30:
				{
					@event.FxT = Effects.Fx_Tremolo;
					@event.FxP = param;
					break;
				}

				// Tremolo waveform
				case 31:
				{
					@event.FxT = Effects.Fx_Extended;
					@event.FxP = (uint8)((Effects.Ex_Tremolo_Wf << 4) | (param & 0x0f));
					break;
				}

				// Sample offset
				case 40:
				{
					// TODO: param and param3 are the fine and high offsets
					@event.FxT = Effects.Fx_Offset;
					@event.FxP = param2;
					break;
				}

				// Retrigger note
				case 41:
				{
					@event.FxT = Effects.Fx_Extended;
					@event.FxP = (uint8)((Effects.Ex_Retrig << 4) | (param & 0x0f));
					break;
				}

				// Note cut
				case 42:
				{
					@event.FxT = Effects.Fx_Extended;
					@event.FxP = (uint8)((Effects.Ex_Cut << 4) | (param & 0x0f));
					break;
				}

				// Note delay
				case 43:
				{
					@event.FxT = Effects.Fx_Extended;
					@event.FxP = (uint8)((Effects.Ex_Delay << 4) | (param & 0x0f));
					break;
				}

				// Position jump
				case 50:
				{
					@event.FxT = Effects.Fx_Jump;
					@event.FxP = param;
					break;
				}

				// Pattern break
				case 51:
				{
					@event.FxT = Effects.Fx_Break;
					@event.FxP = param;
					break;
				}

				// Jump loop
				case 52:
				{
					@event.FxT = Effects.Fx_Extended;
					@event.FxP = (uint8)((Effects.Ex_Pattern_Loop << 4) | (param & 0x0f));
					break;
				}

				// Pattern delay
				case 53:
				{
					@event.FxT = Effects.Fx_Patt_Delay;
					@event.FxP = param;
					break;
				}

				// Set speed
				case 60:
				{
					@event.FxT = Effects.Fx_S3M_Speed;
					@event.FxP = param;
					break;
				}

				// Set BPM
				case 61:
				{
					@event.FxT = Effects.Fx_S3M_Bpm;
					@event.FxP = param;
					break;
				}

				// Arpeggio
				case 70:
				{
					@event.FxT = Effects.Fx_Arpeggio;
					@event.FxP = param;
					break;
				}

				// Set finetune
				case 71:
				{
					@event.FxT = Effects.Fx_FineTune;
					@event.FxP = param;
					break;
				}

				// Set balance
				case 72:
				{
					@event.FxT = Effects.Fx_SetPan;
					@event.FxP = (uint8)((param & 0x0f) | ((param & 0x0f) << 4));
					break;
				}

				default:
				{
					@event.FxT = @event.FxP = 0;
					break;
				}
			}
		}
		#endregion
	}
}
