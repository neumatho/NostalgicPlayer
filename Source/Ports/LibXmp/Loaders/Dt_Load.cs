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
	/// TODO: Digital Home Studio features (SV19 extensions, IENV, 2.06 format, etc).
	/// TODO: Digital Home Studio DTM 2.1 test; 2.03, 2.04, 1.9, 2.1 effects tests.
	/// TODO: libxmp needs a new pan law for modules from 2.04 until 1.9.
	/// TODO: libxmp does not support track or pattern names.
	/// TODO: libxmp does not support SV19 fractional BPM.
	/// TODO: libxmp does not support horrible DTM stereo hacks (see below).
	/// TODO: libxmp can't tell 2.04 and pre-VERS commercial modules apart.
	/// TODO: were external samples ever implemented?
	/// </summary>
	internal class Dt_Load : IFormatLoader
	{
		#region Internal structures

		#region Local_Data
		private class Local_Data
		{
			public bool Vers_Flag;
			public bool Patt_Flag;
			public bool SV19_Flag;
			public bool PFlag;
			public bool SFlag;
			public c_int Stereo;								// 0 = Old stereo, ff = Panoramic stereo (>=2.04)
			public c_int Depth;									// Global sample depth used by pre-2.04 modules
			public c_int C2Spd;									// Global sample rate used by pre-2.04 modules
			public c_int Version;
			public c_int Version_Derived;
			public c_int Format;
			public c_int RealPat;
			public c_int Last_Pat;
			public c_int InsNum;
			public c_int Chn;

			public CPointer<uint8> PatBuf;
			public size_t PatBuf_Alloc;
		}
		#endregion

		#endregion

		private static readonly uint32 Magic_D_T_ = Common.Magic4('D', '.', 'T', '.');

		// Values to compare against version_derived for compatibility.
		// These don't directly correspond to real values in the format,
		// they were just picked so the "commercial" formats with VERS would
		// have higher values than those determined by the pattern format.
		// Note that commercial versions 1.0 through 1.9 come after 2.04, and
		// versions prior to 2.015 did not support the DTM format. VERS and
		// SV19 are not emitted up through at least version DT 1.901, so most
		// of the commercial versions will have their modules IDed as 2.04.
		// Later versions (1.914 and 1.917 confirmed) are aware of VERS and SV19.
		// 
		// Note that there's a later Digital Tracker release series for BeOS
		// using a completely new format that this loader doesn't support

		private const int DTM_V2015 = 2015;						// Digital Tracker 2.015 and 2.02
		private const int DTM_V203 = 2030;						// Digital Tracker 2.03
		private const int DTM_V204 = 2040;						// Digital Tracker 2.04 thru 1.9
		private const int DTM_V19 = (19 << 8);					// Digital Tracker 1.9 (later vers)
		private const int DTM_V21 = (21 << 8);					// Digital Home Studio

		// Pattern format versions, which don't correspond directly to file
		// format versions. Versions 2.015 thru 2.03 have all zero bytes here and
		// store patterns using the Protracker format; 2.04 through 1.9 use ASCII
		// "2.04" and store note numbers instead of periods; Digital Home Studio
		// uses an ASCII "2.06" and stores completely unpacked pattern fields

		private const uint32 Format_Mod = 0;
		private static readonly uint32 Format_V204 = Common.Magic4('2', '.', '0', '4');
		private static readonly uint32 Format_V206 = Common.Magic4('2', '.', '0', '6');

		// "Mono" mode has always been panoramic stereo, but this wasn't clearly
		// communicated in the documentation or UI until Digital Home Studio
		private const int DTM_Old_Stereo = 0x00;
		private const int DTM_Panoramic_Stereo = 0xff;

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Dt = new Format_Loader
		{
			Id = Guid.Parse("D3DE30A5-4786-43C5-824F-8AA216174008"),
			Name = "Digital Tracker",
			Description = "This editor was written for the Atari by Softjee. It is the little brother to Digital Home Studio. This player can load Digital Trackers own format.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Dt_Load(LibXmp libXmp)
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
			return new Dt_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			if (f.Hio_Read32B() != Magic_D_T_)
				return -1;

			uint32 size = f.Hio_Read32B();		// Chunk size
			f.Hio_Read16B();					// Type
			f.Hio_Read16B();					// Stereo mode; global depth (pre-2.04)
			f.Hio_Read16B();					// Reserved
			f.Hio_Read16B();					// Tempo
			f.Hio_Read16B();					// BPM
			f.Hio_Read32B();					// Global sample rate (pre-2.04)

			Ports.LibXmp.Common.Clamp(ref size, 14U, Constants.Xmp_Name_Size + 14U);
			size -= 14;

			lib.common.LibXmp_Read_Title(f, out t, (c_int)size, encoder);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Loader(Module_Data m, Hio f, c_int start)
		{
			Local_Data data = new Local_Data();
			Xmp_Module mod = m.Mod;

			Iff handle = Iff.LibXmp_Iff_New();
			if (handle == null)
				return -1;

			m.C4Rate = Constants.C4_Ntsc_Rate;

			// IFF chunk IDs
			c_int ret = handle.LibXmp_Iff_Register("D.T.".ToPointer(), Get_D_T_);
			ret |= handle.LibXmp_Iff_Register("S.Q.".ToPointer(), Get_S_Q_);
			ret |= handle.LibXmp_Iff_Register("VERS".ToPointer(), Get_Vers);
			ret |= handle.LibXmp_Iff_Register("PATT".ToPointer(), Get_Patt);
			ret |= handle.LibXmp_Iff_Register("INST".ToPointer(), Get_Inst);
			ret |= handle.LibXmp_Iff_Register("SV19".ToPointer(), Get_Sv19);
			ret |= handle.LibXmp_Iff_Register("DAPT".ToPointer(), Get_Dapt);
			ret |= handle.LibXmp_Iff_Register("DAIT".ToPointer(), Get_Dait);
			ret |= handle.LibXmp_Iff_Register("TEXT".ToPointer(), Get_Text);

			if (ret != 0)
				return -1;

			// Load IFF chunks
			ret = handle.LibXmp_Iff_Load(m, f, data);

			handle.LibXmp_Iff_Release();
			CMemory.Free(data.PatBuf);

			if (ret < 0)
				return -1;

			// Alloc remaining patterns
			if (mod.Xxp != null)
			{
				for (c_int i = data.Last_Pat; i < mod.Pat; i++)
				{
					if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, 64) < 0)
						return -1;
				}
			}

			// Correct the module type now that the version fields are known
			if (data.Version >= 20)
				lib.common.LibXmp_Set_Type(m, string.Format("Digital Home Studio DTM {0}.{1}", data.Version / 10, data.Version % 10));
			else if (data.Version != 0)
				lib.common.LibXmp_Set_Type(m, string.Format("Digital Tracker {0}.{1} DTM", data.Version / 10, data.Version % 10));
			else if (data.Format == Format_V204)
				lib.common.LibXmp_Set_Type(m, "Digital Tracker 2.04 DTM");
			else if (data.Depth != 0)
				lib.common.LibXmp_Set_Type(m, "Digital Tracker 2.03 DTM");
			else
				lib.common.LibXmp_Set_Type(m, "Digital Tracker 2.015 DTM");

			if ((data.Version_Derived >= DTM_V204) && (data.Stereo == DTM_Panoramic_Stereo))
			{
				// Panoramic stereo mode: all channels default to center.
				// In 1.9xx, the SV19 chunk is used to specify initial values,
				// so this should be skipped if it was loaded
				if (!data.SV19_Flag)
				{
					for (c_int i = 0; i < mod.Chn; i++)
						mod.Xxc[i].Pan = 0x80;
				}
			}

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Dtm_Translate_Effect(Xmp_Event @event, Local_Data data)
		{
			switch (@event.FxT)
			{
				// Portamento up
				// Portamento down
				// Tone portamento
				// Vibrato
				// Tone portamento + vol slide
				// Vibrato + vol slide
				// Tremolo
				// Offset
				// Volume slide
				// Set volume
				case 0x1:
				case 0x2:
				case 0x3:
				case 0x4:
				case 0x5:
				case 0x6:
				case 0x7:
				case 0x9:
				case 0xa:
				case 0xc:
				{
					// Protracker compatible
					break;
				}

				// Arpeggio
				case 0x0:
				{
					// DT beta through 2.04: does nothing
					if (data.Version_Derived <= DTM_V203)
						@event.FxP = 0;

					break;
				}

				// Set panning
				case 0x8:
				{
					// DT 2.04+: only supported in panoramic stereo mode
					if ((data.Version_Derived >= DTM_V204) && (data.Stereo == DTM_Panoramic_Stereo))
					{
						// DT 1.9 and up have 800 as full right and 8FF as
						// full left. 2.04 through 1.1 use the high nibble to
						// control the channel's left mix value (0:low, F:high)
						// and the low nibble control the channel's right mix
						// level. 0 is not completely silent. The default
						// setting is 15 (full) for each channel.
						// 
						// For the older behavior, see:
						//   Bitmaps/no happy end !!!!!!.dtm
						//   Tyan/fruchtix 1997.dtm
						if (data.Version_Derived >= DTM_V19)
							@event.FxP ^= 0xff;
						else
						{
							// TODO: solve DT's old pan law into libxmp's.
							// would be nice to have pan law support instead.
							// 
							// L = 0x80 * (left + 1) = vol * (0x80 - pan)
							// R = 0x80 * (right + 1) = vol * (0x80 + pan)
							c_int left = Ports.LibXmp.Common.Msn(@event.FxP);
							c_int right = Ports.LibXmp.Common.Lsn(@event.FxP);
							c_int pan = 0x80 * (right - left) / (left + right + 2);
							c_int vol;

							if (right > left)
								vol = 0x80 * ((right + 1) << 2) / (0x80 + pan);
							else if (left > right)
								vol = 0x80 * ((left + 1) << 2) / (0x80 - pan);
							else
								vol = (left + 1) << 2;

							@event.FxT = Effects.Fx_SetPan;
							@event.FxP = (byte)(pan + 0x80);
							@event.F2T = Effects.Fx_Trk_Vol;
							@event.F2P = (byte)vol;
						}
					}
					else
						@event.FxP = @event.FxT = 0;

					break;
				}

				// Position jump
				case 0xb:
				{
					if ((data.Version_Derived <= DTM_V203) && (data.Chn > 4))
					{
						// DT 2.04 and under: in 6 and 8 channel modules,
						// the break position is somewhere in the middle of
						// a pattern unless followed by a Dxx. This can also
						// do things like jump to position "255"
						// FIXME: try to more closely approximate this?
						@event.F2T = Effects.Fx_Break;
						@event.F2P = 0x32;
					}
					else if (data.Version_Derived == DTM_V204)
					{
						// DT commercial 1.0 through 1.2: effect is off-by-one;
						// B01 will jump to 0, B02 will jump to 1, etc.
						// This works as expected from 1.901 onward.
						// TODO: does anything rely on the broken version?
						if (@event.FxP > 0)
							@event.FxP--;
					}
					break;
				}

				// Pattern break
				case 0xd:
				{
					// DT beta through commercial 1.2: parameter is ignored.
					// This works as expected from 1.901 onward.
					// TODO: does anything rely on the broken version?
					if (data.Version_Derived <= DTM_V203)
						@event.FxP = 0;

					break;
				}

				// Set speed/tempo
				case 0xf:
				{
					// DT beta: all parameters set speed, 0 acts like 1
					// DT 1.01 thru 2.04: 0 and 20h act like speed 1 (no clamp)
					// DT commercial 1.0: 0 and 20h act like speed 1 (clamped to 66)
					// DT commercial 1.1: 0 and 20h act like speed 1 (no clamp)
					// DT commercial 1.2: 0 is ignored, 20h is a BPM (clamped to 66)
					// DT commercial 1.901: 0 is ignored, 20h is a BPM (no clamp)
					// The 66 BPM clamp in some versions seems to be to avoid DSP
					// bugs with slow BPMs. Whatever issue was fixed by 1.901.
					// TODO: what does anything actually rely on, if at all?
					if (data.Version_Derived <= DTM_V203)
					{
						if ((@event.FxP == 0) || (@event.FxP == 0x20))
							@event.FxP = 1;
					}
					break;
				}

				case 0xe:
				{
					switch (Ports.LibXmp.Common.Msn(@event.FxP))
					{
						// Fine portamento up
						// Fine portamento down
						// Glissando control
						// Pattern loop
						// Fine volume slide up
						// Fine volume slide down
						// Note cut
						// Note delay
						// Pattern delay
						case 0x1:
						case 0x2:
						case 0x3:
						case 0x6:
						case 0xa:
						case 0xb:
						case 0xc:
						case 0xd:
						case 0xe:
						{
							// Protracker compatible
							break;
						}

						// Vibrato waveform
						case 0x4:
						{
							// DT 2.04 and up: sine rampup(period) square square
							if ((@event.FxP & 3) == 3)
								@event.FxP -= 1;

							// Continuous variants are missing from all versions
							@event.FxP &= 3;
							break;
						}

						// Set finetune
						case 0x5:
						{
							// DT 2.04: only works if a note is present
							if (@event.Note == 0)
								@event.FxT = @event.FxP = 0;

							break;
						}

						// Set panning
						case 0x8:
						{
							// Missing from all versions except commercial 1.1,
							// where it seems to be a global volume effect?
							// fall-through
							goto default;
						}

						default:
						{
							@event.FxT = @event.FxP = 0;
							break;
						}
					}
					break;
				}

				default:
				{
					@event.FxT = @event.FxP = 0;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Dtm_Event_Size(c_int format)
		{
			if ((format == Format_Mod) || (format == Format_V204))
				return 4;

			if (format == Format_V206)
				return 6;

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Dtm_Translate_Event(Xmp_Event @event, CPointer<uint8> @in, Local_Data data)
		{
			if (data.Format == Format_Mod)
				lib.common.LibXmp_Decode_ProTracker_Event(@event, @in);
			else if (data.Format == Format_V204)
			{
				if ((@in[0] > 0) && (@in[0] <= 0x7c))
					@event.Note = (byte)(12 * (@in[0] >> 4) + (@in[0] & 0x0f) + 12);

				@event.Vol = (byte)((@in[1] & 0xfc) >> 2);
				@event.Ins = (byte)(((@in[1] & 0x03) << 4) + (@in[2] >> 4));
				@event.FxT = (byte)(@in[2] & 0x0f);
				@event.FxP = @in[3];
			}
			else if (data.Format == Format_V206)
			{
				// FIXME Digital Home Studio
			}

			Dtm_Translate_Effect(@event, data);
		}

		#region IFF chunk handlers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_D_T_(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;
			c_int name_Len = size - 14;

			if ((size < 14) || (size > 142))
				return -1;

			f.Hio_Read16B();						// Type
			data.Stereo = f.Hio_Read8();			// 0: Old stereo, ff: Panoramic stereo
			data.Depth = f.Hio_Read8();				// Global sample depth (pre-2.04)
			f.Hio_Read16B();						// Reserved
			mod.Spd = f.Hio_Read16B();

			c_int b = f.Hio_Read16B();
			if (b > 0)		// RAMBO.DTM has bpm 0
				mod.Bpm = b;

			data.C2Spd = (c_int)f.Hio_Read32B();	// Global sample rate (pre-2.04)

			if ((data.Stereo != DTM_Old_Stereo) && (data.Stereo != DTM_Panoramic_Stereo))
				data.Stereo = DTM_Old_Stereo;

			// Global sample depth is applied to all samples pre-2.04.
			// Later Digital Tracker versions incorrectly ignore this field when
			// importing pre-2.04 modules.
			//
			// DT 2.015 and 2.02 will save a value of 0 in this field
			if ((data.Depth != 0) && (data.Depth != 8) && (data.Depth != 16))
				data.Depth = 8;

			// Only known used values for global sample rate are 8400 and 24585,
			// but 12292 and 49170 are also supported and referenced in the UI.
			// 2.04 will replace the sample rates with whatever was in this field.
			// The only values that do not crash here besides 8400 are Atari Falcon
			// hardware frequencies:
			// 
			// f = floor((25175040 >> 8) / div) = floor(98340 / div)
			switch (data.C2Spd)
			{
				case 49170:
				case 24585:
				case 12292:
				case 8400:
					break;

				default:
				{
					data.C2Spd = 8400;
					break;
				}
			}

			Ports.LibXmp.Common.Clamp(ref name_Len, 0, Constants.Xmp_Name_Size);
			byte[] name = new byte[name_Len + 1];
			f.Hio_Read(name, (size_t)name_Len, 1);
			mod.Name = encoder.GetString(name, 0, name_Len);

			lib.common.LibXmp_Set_Type(m, "Digital Tracker DTM");

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_S_Q_(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			c_int i, maxPat;

			// Sanity check
			if (mod.Pat != 0)
				return -1;

			mod.Len = f.Hio_Read16B();
			mod.Rst = f.Hio_Read16B();

			// Sanity check
			if ((mod.Len > 256) || (mod.Rst > 255))
				return -1;

			f.Hio_Read32B();		// Reserved

			for (maxPat = i = 0; i < 128; i++)
			{
				mod.Xxo[i] = f.Hio_Read8();

				if (mod.Xxo[i] > maxPat)
					maxPat = mod.Xxo[i];
			}

			mod.Pat = maxPat + 1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Vers(Module_Data m, c_int size, Hio f, object parm)
		{
			Local_Data data = (Local_Data)parm;

			if (data.Vers_Flag || (size < 4))
				return 0;

			data.Vers_Flag = true;

			data.Version = (c_int)f.Hio_Read32B();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Sv19(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;
			CPointer<uint8> buf = new CPointer<uint8>(32 * 2);

			if (!data.Vers_Flag || (data.Version < 19) || (size < 86))
			{
				// Ignore in the extreme off chance a module with this exists
				return 0;
			}

			data.SV19_Flag = true;

			f.Hio_Read16B();					// Ticks per beat, by default 24
			uint32 bpm_Frac = f.Hio_Read32B();	// Initial BPM (fractional portion)

			// Round up to nearest for now
			if (bpm_Frac >= 0x80000000U)
				mod.Bpm++;

			// Initial pan table. 0=left 90=center 180=right
			// Do not load in old stereo mode for now
			if (f.Hio_Read(buf, 1, 64) < 64)
				return -1;

			for (c_int i = 0; i < 32; i++)
			{
				c_int val = DataIo.ReadMem16B(buf + 2 * i);

				if ((val <= 180) && (data.Stereo == DTM_Panoramic_Stereo))
					mod.Xxc[i].Pan = val * 0x100 / 180;
			}

			// Instrument type table.
			// The file format claims 32 bytes but it's only 16.
			// It's not clear how to influence this table, if possible

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Patt(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;
			uint8[] ver = new uint8[4];

			// Sanity check
			if (data.Patt_Flag || data.PFlag || (size < 8))
				return -1;

			data.Patt_Flag = true;

			data.Chn = mod.Chn = f.Hio_Read16B();
			data.RealPat = f.Hio_Read16B();

			if (f.Hio_Read(ver, 1, 4) < 4)
				return -1;

			data.Format = (c_int)DataIo.ReadMem32B(ver);
			mod.Trk = mod.Chn * mod.Pat;

			if (data.Format != Format_Mod)
				data.Version_Derived = (data.Format == Format_V206) ? DTM_V21 : DTM_V204;
			else
			{
				// DTM 2.015/2.02 have depth=0 and 31 instruments instead of 63.
				// There are also modules with depth!=0 but 31 instruments,
				// and it's not clear what the origin of those is
				if (data.Depth != 0)
					data.Version_Derived = DTM_V203;
				else
					data.Version_Derived = DTM_V2015;
			}

			if (data.Vers_Flag && (data.Version != 0))
			{
				data.Version_Derived = data.Version << 8;
				m.Flow_Mode = FlowMode_Flag.Mode_DTM_19;
			}
			else
			{
				// 6/8 channel mode's position jump bugs also affect its
				// interactions with pattern loop. (Very weakly) attempt
				// to support the bugs
				m.Flow_Mode = (data.Version_Derived <= DTM_V204) && (mod.Chn > 4) ? FlowMode_Flag.Mode_DTM_2015_6CH : FlowMode_Flag.Mode_DTM_2015;
			}

			// Sanity check
			if (mod.Chn > Constants.Xmp_Max_Channels)
				return -1;

			if (Dtm_Event_Size(data.Format) < 0)
				return -1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Inst(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;
			CPointer<uint8> buf = new CPointer<uint8>(50);

			// Sanity check
			if (mod.Ins != 0)
				return -1;

			mod.Ins = mod.Smp = f.Hio_Read16B();

			// Sanity check
			if (mod.Ins > Constants.Max_Instruments)
				return -1;

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			for (c_int i = 0; i < mod.Ins; i++)
			{
				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					return -1;

				if (f.Hio_Read(buf, 1, 50) < 50)
					return -1;

				c_int len = (c_int)DataIo.ReadMem32B(buf + 4);
				if (len < 0)
					len = 0;

				mod.Xxs[i].Len = len;
				mod.Xxi[i].Nsm = (mod.Xxs[i].Len > 0) ? 1 : 0;

				c_int fine = (int8)(buf[8] << 4);	// Finetune

				mod.Xxi[i].Sub[0].Vol = buf[9];
				mod.Xxi[i].Sub[0].Pan = 0x80;

				c_int repStart = (c_int)DataIo.ReadMem32B(buf + 10);
				c_int repLen = (c_int)DataIo.ReadMem32B(buf + 14);

				if ((repStart >= len) || (repStart < 0) || (repLen < 0))
				{
					repStart = 0;
					repLen = 0;
				}

				if (repLen > (len - repStart))
					repLen = len - repStart;

				mod.Xxs[i].Lps = repStart;
				mod.Xxs[i].Lpe = repStart + repLen;

				lib.common.LibXmp_Instrument_Name(mod, i, buf + 18, 22, encoder);

				// DT pre-2.04: global sample depth is used for playback; the
				// sample depth field seems to be for the resampler only.
				// See Lot/5th-2.dtm, which relies on this
				if (data.Version_Derived <= DTM_V203)
					buf[41] = (uint8)(data.Depth != 0 ? data.Depth : 8);

				c_int stereo = buf[40];		// Stereo

				if (buf[41] > 8)			// Resolution (8, 16, or rarely 0)
				{
					mod.Xxs[i].Flg |= Xmp_Sample_Flag._16Bit;
					mod.Xxs[i].Len >>= 1;
					mod.Xxs[i].Lps >>= 1;
					mod.Xxs[i].Lpe >>= 1;
				}

				if (stereo != 0)
				{
					// TODO: in old stereo mode, stereo samples do something
					// unusual: the left channel is sent to the first
					// channel of the "pair" it is played in, and the
					// right channel is sent to the second channel of
					// the "pair" (a "pair" is channels 1&2, 3&4, etc.).
					// If the stereo sample is played in the first channel,
					// it overrides whatever is played by the second, but
					// if it is played in the second channel, the first
					// channel can override it (or more often is silent).
					// Note that for 1&2, 5&6, etc, the left sample will
					// pan right and the right sample will be pan left.
					// 
					// The worst part of this is that each channel of the
					// stereo sample is subject to the pitch and volume OF
					// THE CHANNEL IT IS PLAYING IN. This probably needs a
					// custom read_event and some mixer hacks
					mod.Xxs[i].Flg |= Xmp_Sample_Flag.Stereo;
					mod.Xxs[i].Len >>= 1;
					mod.Xxs[i].Lps >>= 1;
					mod.Xxs[i].Lpe >>= 1;
				}

				if ((mod.Xxs[i].Lpe - mod.Xxs[i].Lps) > 1)
					mod.Xxs[i].Flg |= Xmp_Sample_Flag.Loop;

				c_int midiNote = DataIo.ReadMem16B(buf + 42);			// MIDI note
				c_int c2Spd = (c_int)DataIo.ReadMem32B(buf + 46);		// Frequency

				// DT pre-2.04: the sample rate is used for resampling only(?)
				// and the global sample rate is used for playback instead.
				// This field was removed from 2.04
				if (data.Version_Derived <= DTM_V203)
					c2Spd = data.C2Spd;

				lib.period.LibXmp_C2Spd_To_Note(c2Spd, out mod.Xxi[i].Sub[0].Xpo, out mod.Xxi[i].Sub[0].Fin);

				// DT 2.03 through commercial 1.2: midi note resamples
				// the sample and changes the rate in some cases, but does
				// nothing during normal playback.
				// DT 1.901+: changing the midi note acts as a transpose
				// during playback and does not modify the sample at all.
				// Values under C-2 are clamped, over B-7 are ignored
				if ((data.Version_Derived >= DTM_V19) && (midiNote < 95))
					mod.Xxi[i].Sub[0].Xpo += 48 - Math.Max(24, midiNote);

				// It's strange that we have both c2spd and finetune
				mod.Xxi[i].Sub[0].Fin += fine;

				mod.Xxi[i].Sub[0].Sid = i;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Dapt(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;

			if (!data.Patt_Flag)
				return -1;

			if (!data.PFlag)
			{
				data.PFlag = true;
				data.Last_Pat = 0;

				if (lib.common.LibXmp_Init_Pattern(mod) < 0)
					return -1;
			}

			f.Hio_Read32B();		// 0xffffffff
			c_int pat = f.Hio_Read16B();
			c_int rows = f.Hio_Read16B();

			// Sanity check
			// DT 2.04, all known versions of 1.9x: maximum rows is 96.
			// These can load/resave larger patterns made with a hex editor,
			// but with a buggy UI. It isn't clear how stable these are.
			// DHS: maximum configurable by interface is 4*99 = 396.
			// TODO: it uses a different format for this entire chunk
			if ((pat < 0) || (pat >= mod.Pat) || (rows < 0) || ((data.Version_Derived <= DTM_V19) && (rows > 96)) || ((data.Version_Derived > DTM_V19) && (rows > 396)))
				return -1;

			if (pat < data.Last_Pat)
				return -1;

			for (c_int i = data.Last_Pat; i <= pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, rows) < 0)
					return -1;
			}

			data.Last_Pat = pat + 1;

			// (Re)allocate the pattern buffer
			c_int event_Size = Dtm_Event_Size(data.Format);
			size_t total_Size = (size_t)(event_Size * rows * mod.Chn);

			if (data.PatBuf_Alloc < total_Size)
			{
				CPointer<uint8> tmp = CMemory.Realloc(data.PatBuf, (c_int)total_Size);
				if (tmp.IsNull)
					return -1;

				data.PatBuf = tmp;
				data.PatBuf_Alloc = total_Size;
			}

			if (f.Hio_Read(data.PatBuf, 1, total_Size) < total_Size)
				return -1;

			CPointer<uint8> pos = data.PatBuf;

			for (c_int j = 0; j < rows; j++)
			{
				for (c_int k = 0; k < mod.Chn; k++)
				{
					Xmp_Event @event = Ports.LibXmp.Common.Event(m, pat, k, j);
					Dtm_Translate_Event(@event, pos, data);
					pos += event_Size;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Dait(Module_Data m, c_int size, Hio f, object parm)
		{
			Xmp_Module mod = m.Mod;
			Local_Data data = (Local_Data)parm;

			if (!data.SFlag)
			{
				data.SFlag = true;
				data.InsNum = 0;
			}

			if (size > 2)
			{
				// Sanity check
				if (data.InsNum >= mod.Ins)
					return -1;

				c_int ret = Sample.LibXmp_Load_Sample(m, f, Sample_Flag.BigEnd | Sample_Flag.Interleaved, mod.Xxs[mod.Xxi[data.InsNum].Sub[0].Sid], null, data.InsNum);

				if (ret < 0)
					return -1;
			}

			data.InsNum++;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Text(Module_Data m, c_int size, Hio f, object parm)
		{
			if ((size < 12) || ((size & 1) != 0) || !string.IsNullOrEmpty(m.Comment))
				return 0;

			f.Hio_Read16B();
			uint32 len = f.Hio_Read32B();
			f.Hio_Read16B();
			f.Hio_Read16B();

			// "=$FFFF <=> length is odd"
			c_int skip_Byte;

			if (f.Hio_Read16B() == 0xffff)
				skip_Byte = 1;
			else
				skip_Byte = 0;

			if (len != (uint32)(size - 12 - skip_Byte))
				return 0;

			if (len == 0)
				return 0;

			if ((c_long)len > (f.Hio_Size() - f.Hio_Tell()))
				return 0;

			CPointer<uint8> comment = CMemory.MAlloc<uint8>((c_int)len + 1);
			if (comment.IsNotNull)
			{
				if (skip_Byte != 0)
					f.Hio_Read8();

				len = (uint32)f.Hio_Read(comment, 1, len);
				comment[len] = 0;

				m.Comment = string.Empty;
				int startIndex = 0;

				for (int i = 0; i < len; i++)
				{
					if (comment[i] == 0x0a)
					{
						int clipLen = ((i > 0) && (comment[i - 1] == 0x0d) ? i - 1 : i) - startIndex;

						m.Comment += encoder.GetString(comment.Buffer, comment.Offset + startIndex, clipLen) + "\n";
						startIndex = i + 1;
					}
				}

				m.Comment += encoder.GetString(comment.Buffer, comment.Offset + startIndex, (int)(len - startIndex));
			}

			return 0;
		}
		#endregion

		#endregion
	}
}
