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
	internal class Amf_Load : IFormatLoader
	{
		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Amf = new Format_Loader
		{
			Id = Guid.Parse("D1499BF4-82D2-4B9C-9CDA-7D2ADE977E63"),
			Name = "Digital Sound and Music Interface",
			Description = "This loader recognizes the “Advanced Module Format”, which is the internal module format of the “Digital Sound and Music Interface” (DSMI) library.\n\nThis format has the same limitations as the S3M format. The most famous DSMI application was DMP, the Dual Module Player.\n\nDMP and the DSMI library were written by Otto Chrons. DSMI was first released in 1993.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Amf_Load(LibXmp libXmp)
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
			return new Amf_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			byte[] buf = new byte[4];

			if (f.Hio_Read(buf, 1, 3) < 3)
				return -1;

			if ((buf[0] != 'A') || (buf[1] != 'M') || (buf[2] != 'F'))
				return -1;

			c_int ver = f.Hio_Read8();
			if ((ver != 0x01) && (ver < 0x08) || (ver > 0x0e))
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
			uint8[] buf = new uint8[1024];
			bool no_LoopEnd = false;

			f.Hio_Read(buf, 1, 3);
			c_int ver = f.Hio_Read8();

			if (f.Hio_Read(buf, 1, 32) != 32)
				return -1;

			mod.Name = encoder.GetString(buf, 0, 32);
			lib.common.LibXmp_Set_Type(m, string.Format("DSMI {0}.{1} AMF", ver / 10, ver % 10));

			mod.Ins = f.Hio_Read8();
			mod.Len = f.Hio_Read8();
			mod.Trk = f.Hio_Read16L();
			mod.Chn = 4;

			if (ver >= 0x09)
				mod.Chn = f.Hio_Read8();

			// Sanity check
			if ((mod.Ins == 0) || (mod.Len == 0) || (mod.Trk == 0) || (mod.Chn == 0) || (mod.Chn > Constants.Xmp_Max_Channels))
				return -1;

			mod.Smp = mod.Ins;
			mod.Pat = mod.Len;

			if ((ver == 0x09) || (ver == 0x0a))
				f.Hio_Read(buf, 1, 16);		// Channel remap table

			if (ver >= 0x0b)
			{
				c_int pan_Len = ver >= 0x0c ? 32 : 16;

				if (f.Hio_Read(buf, 1, (size_t)pan_Len) != (size_t)pan_Len)	// Panning table
					return -1;

				for (c_int i = 0; i < pan_Len; i++)
					mod.Xxc[i].Pan = 0x80 + 2 * (int8)buf[i];
			}

			if (ver >= 0x0d)
			{
				mod.Bpm = f.Hio_Read8();
				mod.Spd = f.Hio_Read8();
			}

			m.C4Rate = Constants.C4_Ntsc_Rate;

			// Orders
			//
			// Andre Timmermans <andre.timmermans@atos.net> says:
			//
			// Order table: track numbers in this table are not explained,
			// but as you noticed you have to perform -1 to obtain the index
			// in the track table. For value 0, found in some files, I think
			// it means an empty track.
			//
			// 2021 note: this is misleading. Do not subtract 1 from the logical
			// track values found in the order table; load the mapping table to
			// index 1 instead
			for (c_int i = 0; i < mod.Len; i++)
				mod.Xxo[i] = (byte)i;

			mod.Xxp = new Xmp_Pattern[mod.Pat];
			if (mod.Xxp == null)
				return -1;

			for (c_int i = 0; i < mod.Pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern(mod, i) < 0)
					return -1;

				mod.Xxp[i].Rows = ver >= 0x0e ? f.Hio_Read16L() : 64;

				if (mod.Xxp[i].Rows > 256)
					return -1;

				for (c_int j = 0; j < mod.Chn; j++)
				{
					uint16 t = f.Hio_Read16L();
					mod.Xxp[i].Index[j] = t;
				}
			}

			// Instruments

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			// Probe for 2-byte loop start 1.0 format
			// in facing_n.amf and sweetdrm.amf have only the sample
			// loop start specified in 2 bytes
			//
			// These modules are an early variant of the AMF 1.0 format. Since
			// normal AMF 1.0 files have 32-bit lengths/loop start/loop end,
			// this is possibly caused by these fields having been expanded for
			// the 1.0 format, but M2AMF 1.3 writing instrument structs with
			// the old length (which would explain the missing 6 bytes)
			if (ver == 0x0a)
			{
				c_long pos = f.Hio_Tell();
				if (pos < 0)
					return -1;

				for (c_int i = 0; i < mod.Ins; i++)
				{
					uint8 b = f.Hio_Read8();
					if ((b != 0) && (b != 1))
					{
						no_LoopEnd = true;
						break;
					}

					f.Hio_Seek(32 + 13, SeekOrigin.Current);
					if (f.Hio_Read32L() > (uint32)mod.Ins)	// Check index
					{
						no_LoopEnd = true;
						break;
					}

					uint32 len = f.Hio_Read32L();
					if (len > 0x100000)				// Check len
					{
						no_LoopEnd = true;
						break;
					}

					if (f.Hio_Read16L() == 0x0000)	// Check c2spd
					{
						no_LoopEnd = true;
						break;
					}

					if (f.Hio_Read8() > 0x40)		// Check volume
					{
						no_LoopEnd = true;
						break;
					}

					uint32 val = f.Hio_Read32L();	// Check loop start
					if (val > len)
					{
						no_LoopEnd = true;
						break;
					}

					val = f.Hio_Read32L();			// Check loop end
					if (val > len)
					{
						no_LoopEnd = true;
						break;
					}
				}

				f.Hio_Seek(pos, SeekOrigin.Begin);
			}

			for (c_int i = 0; i < mod.Ins; i++)
			{
				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					return -1;

				f.Hio_Read8();

				f.Hio_Read(buf, 1, 32);
				lib.common.LibXmp_Instrument_Name(mod, i, buf, 32, encoder);

				f.Hio_Read(buf, 1, 13);		// Sample name
				f.Hio_Read32L();                        // Sample index

				mod.Xxi[i].Nsm = 1;
				mod.Xxi[i].Sub[0].Sid = i;
				mod.Xxi[i].Sub[0].Pan = 0x80;

				if (ver >= 0x0a)
					mod.Xxs[i].Len = (c_int)f.Hio_Read32L();
				else
					mod.Xxs[i].Len = f.Hio_Read16L();

				c_int c2spd = f.Hio_Read16L();
				lib.period.LibXmp_C2Spd_To_Note(c2spd, out mod.Xxi[i].Sub[0].Xpo, out mod.Xxi[i].Sub[0].Fin);
				mod.Xxi[i].Sub[0].Vol = f.Hio_Read8();

				// Andre Timmermans <andre.timmermans@atos.net> says:
				//
				// [Miodrag Vallat's] doc tells that in version 1.0 only
				// sample loop start is present (2 bytes) but the files I
				// have tells both start and end are present (2*4 bytes).
				// Maybe it should be read as version < 1.0.
				//
				// CM: confirmed with Maelcum's "The tribal zone"
				if (no_LoopEnd)
				{
					mod.Xxs[i].Lps = f.Hio_Read16L();
					mod.Xxs[i].Lpe = mod.Xxs[i].Len;
				}
				else if (ver >= 0x0a)
				{
					mod.Xxs[i].Lps = (c_int)f.Hio_Read32L();
					mod.Xxs[i].Lpe = (c_int)f.Hio_Read32L();
				}
				else
				{
					// Non-looping samples are stored with lpe=-1, not 0
					mod.Xxs[i].Lps = f.Hio_Read16L();
					mod.Xxs[i].Lpe = f.Hio_Read16L();

					if (mod.Xxs[i].Lpe == 0xffff)
						mod.Xxs[i].Lpe = 0;
				}

				if (no_LoopEnd)
					mod.Xxs[i].Flg = mod.Xxs[i].Lps > 0 ? Xmp_Sample_Flag.Loop : Xmp_Sample_Flag.None;
				else
					mod.Xxs[i].Flg = mod.Xxs[i].Lpe > mod.Xxs[i].Lps ? Xmp_Sample_Flag.Loop : Xmp_Sample_Flag.None;
			}

			if (f.Hio_Error() != 0)
				return -1;

			// Tracks
			//
			// Index 0 is a blank track that isn't stored in the file. To keep
			// things simple, load the mapping table to index 1 so the table
			// index is the same as the logical track value. Older versions
			// attempted to remap it to index 0 and subtract 1 from the index,
			// breaking modules that directly reference the empty track in the
			// order table (see "cosmos st.amf")
			CPointer<c_int> trkMap = CMemory.CAlloc<c_int>(mod.Trk + 1);
			if (trkMap.IsNull)
				return -1;

			c_int newTrk = 0;

			for (c_int i = 1; i <= mod.Trk; i++)	// Read track table
			{
				uint16 t = f.Hio_Read16L();
				trkMap[i] = t;

				if (t > newTrk)
					newTrk = t;
			}

			for (c_int i = 0; i < mod.Pat; i++)		// Read track table
			{
				for (c_int j = 0; j < mod.Chn; j++)
				{
					uint16 k = (uint16)mod.Xxp[i].Index[j];

					// Use empty track if an invalid track is requested
					// (such as in Lasse Makkonen "faster and louder")
					if (k > mod.Trk)
						k = 0;

					mod.Xxp[i].Index[j] = trkMap[k];
				}
			}

			mod.Trk = newTrk + 1;		// + empty track
			CMemory.Free(trkMap);

			if (f.Hio_Error() != 0)
				return -1;

			mod.Xxt = new Xmp_Track[mod.Trk];
			if (mod.Xxt == null)
				return -1;

			// Alloc track 0 as empty track
			if (lib.common.LibXmp_Alloc_Track(mod, 0, 64) < 0)
				return -1;

			// Alloc rest of the tracks
			for (c_int i = 1; i < mod.Trk; i++)
			{
				if (lib.common.LibXmp_Alloc_Track(mod, i, 64) < 0)		// FIXME!
					return -1;

				// Previous versions loaded this as a 24-bit value, but it's
				// just a word. The purpose of the third byte is unknown, and
				// DSMI just ignores it
				c_int size = f.Hio_Read16L();
				f.Hio_Read8();

				if (f.Hio_Error() != 0)
					return -1;

				// Version 0.1 AMFs do not count the end-of-track marker in
				// the event count, so add 1. This hasn't been verified yet
				if ((ver == 0x01) && (size != 0))
					size++;

				for (c_int j = 0; j < size; j++)
				{
					uint8 t1 = f.Hio_Read8();		// Row
					uint8 t2 = f.Hio_Read8();		// Type
					uint8 t3 = f.Hio_Read8();		// Parameter

					if ((t1 == 0xff) && (t2 == 0xff) && (t3 == 0xff))
						break;

					// If an event is encountered past the end of the
					// track, treat it the same as the track end. This is
					// encountered in "Avoid.amf"
					if (t1 >= mod.Xxt[i].Rows)
					{
						if (f.Hio_Seek((size - j - 1) * 3, SeekOrigin.Current) != 0)
							return -1;

						break;
					}

					Xmp_Event @event = mod.Xxt[i].Event[t1];

					if (t2 < 0x7f)		// Note
					{
						if (t2 > 0)
							@event.Note = (byte)(t2 + 1);

						// A volume value of 0xff indicates that
						// the old volume should be reused. Prior
						// libxmp versions also forgot to add 1 here
						@event.Vol = (byte)((t3 != 0xff) ? (t3 + 1) : 0);
					}
					else if (t2 == 0x7f)	// Note retrigger
					{
						// AMF.TXT claims that this duplicates the
						// previous event, which is a lie. M2AMF emits
						// this for MODs when an instrument change
						// occurs with no note, indicating it should
						// retrigger (like in PT 2.3). Ignore this.
						//
						// See: "aladdin - aladd.pc.amf", "eye.amf"
					}
					else if (t2 == 0x80)	// Instrument
					{
						@event.Ins = (byte)(t3 + 1);
					}
					else					// Effects
					{
						uint8 fxP = 0;
						uint8 fxT = 0;

						switch (t2)
						{
							case 0x81:
							{
								fxT = Effects.Fx_S3M_Speed;
								fxP = t3;
								break;
							}

							case 0x82:
							{
								if ((int8)t3 > 0)
								{
									fxT = Effects.Fx_VolSlide;
									fxP = (uint8)(t3 << 4);
								}
								else
								{
									fxT = Effects.Fx_VolSlide;
									fxP = (uint8)(-(int8)t3 & 0x0f);
								}
								break;
							}

							case 0x83:
							{
								// See volume notes above. Previous
								// releases forgot to add 1 here
								@event.Vol = (byte)(t3 + 1);
								break;
							}

							case 0x84:
							{
								// AT: Not explained for 0x84, pitch
								// slide, value 0x00 corresponds to
								// S3M E00 and 0x80 stands for S3M F00
								// (I checked with M2AMF)
								if ((int8)t3 >= 0)
								{
									fxT = Effects.Fx_Porta_Dn;
									fxP = t3;
								}
								else if (t3 == 0x80)
								{
									fxT = Effects.Fx_Porta_Up;
									fxP = 0;
								}
								else
								{
									fxT = Effects.Fx_Porta_Up;
									fxP = (uint8)(-(int8)t3);
								}
								break;
							}

							case 0x85:
							{
								// Porta abs -- unknown
								break;
							}

							case 0x86:
							{
								fxT = Effects.Fx_TonePorta;
								fxP = t3;
								break;
							}

							// AT: M2AMF maps both tremolo and tremor to
							// 0x87. Since tremor is only found in certain
							// formats, maybe it would be better to
							// consider it is a tremolo
							case 0x87:
							{
								fxT = Effects.Fx_Tremolo;
								fxP = t3;
								break;
							}

							case 0x88:
							{
								fxT = Effects.Fx_Arpeggio;
								fxP = t3;
								break;
							}

							case 0x89:
							{
								fxT = Effects.Fx_Vibrato;
								fxP = t3;
								break;
							}

							case 0x8a:
							{
								if ((int8)t3 > 0)
								{
									fxT = Effects.Fx_Tone_VSlide;
									fxP = (uint8)(t3 << 4);
								}
								else
								{
									fxT = Effects.Fx_Tone_VSlide;
									fxP = (uint8)(-(int8)t3 & 0x0f);
								}
								break;
							}

							case 0x8b:
							{
								if ((int8)t3 > 0)
								{
									fxT = Effects.Fx_Vibra_VSlide;
									fxP = (uint8)(t3 << 4);
								}
								else
								{
									fxT = Effects.Fx_Vibra_VSlide;
									fxP = (uint8)(-(int8)t3 & 0x0f);
								}
								break;
							}

							case 0x8c:
							{
								fxT = Effects.Fx_Break;
								fxP = t3;
								break;
							}

							case 0x8d:
							{
								fxT = Effects.Fx_Jump;
								fxP = t3;
								break;
							}

							case 0x8e:
							{
								// Sync -- unknown
								break;
							}

							case 0x8f:
							{
								fxT = Effects.Fx_Extended;
								fxP = (uint8)((Effects.Ex_Retrig << 4) | (t3 & 0x0f));
								break;
							}

							case 0x90:
							{
								fxT = Effects.Fx_Offset;
								fxP = t3;
								break;
							}

							case 0x91:
							{
								if ((int8)t3 > 0)
								{
									fxT = Effects.Fx_Extended;
									fxP = (uint8)((Effects.Ex_F_VSlide_Up << 4) | (t3 << 4));
								}
								else
								{
									fxT = Effects.Fx_Extended;
									fxP = (uint8)((Effects.Ex_F_VSlide_Dn << 4) | (t3 & 0x0f));
								}
								break;
							}

							case 0x92:
							{
								if ((int8)t3 > 0)
								{
									fxT = Effects.Fx_Porta_Dn;
									fxP = (uint8)(0xf0 | (fxP & 0x0f));
								}
								else
								{
									fxT = Effects.Fx_Porta_Up;
									fxP = (uint8)(0xf0 | (fxP & 0x0f));
								}
								break;
							}

							case 0x93:
							{
								fxT = Effects.Fx_Extended;
								fxP = (uint8)((Effects.Ex_Delay << 4) | (t3 & 0x0f));
								break;
							}

							case 0x94:
							{
								fxT = Effects.Fx_Extended;
								fxP = (uint8)((Effects.Ex_Cut << 4) | (t3 & 0x0f));
								break;
							}

							case 0x95:
							{
								fxT = Effects.Fx_Speed;

								if (t3 < 0x21)
									t3 = 0x21;

								fxP = t3;
								break;
							}

							case 0x96:
							{
								if ((int8)t3 > 0)
								{
									fxT = Effects.Fx_Porta_Dn;
									fxP = (uint8)(0xe0 | (fxP & 0x0f));
								}
								else
								{
									fxT = Effects.Fx_Porta_Up;
									fxP = (uint8)(0xe0 | (fxP & 0x0f));
								}
								break;
							}

							case 0x97:
							{
								// Same as S3M pan, but param is offset by -0x40
								if (t3 == 0x64)		// 0xA4 - 0x40
								{
									fxT = Effects.Fx_Surround;
									fxP = 1;
								}
								else if ((t3 >= 0xc0) || (t3 <= 0x40))
								{
									c_int pan = ((int8)t3 << 1) + 0x80;

									fxT = Effects.Fx_SetPan;
									fxP = (uint8)Math.Min(0xff, pan);
								}
								break;
							}
						}

						@event.FxT = fxT;
						@event.FxP = fxP;
					}
				}
			}

			// Samples

			for (c_int i = 0; i < mod.Ins; i++)
			{
				if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.Uns, mod.Xxs[i], null, i) < 0)
					return -1;
			}

			m.Quirk |= Quirk_Flag.FineFx;
			m.Module_Flags = Xmp_Module_Flags.Uses_Tracks;

			return 0;
		}
	}
}
