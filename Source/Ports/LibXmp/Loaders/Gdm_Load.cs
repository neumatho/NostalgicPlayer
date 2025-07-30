/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Text;
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
	internal class Gdm_Load : IFormatLoader
	{
		// ReSharper disable InconsistentNaming
		private static readonly uint32 Magic_GDM = Common.Magic4('G', 'D', 'M', '\xfe');
		private static readonly uint32 Magic_GMFS = Common.Magic4('G', 'M', 'F', 'S');
		// ReSharper restore InconsistentNaming

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Gdm = new Format_Loader
		{
			Id = Guid.Parse("6118D229-7AEC-4FF6-8A0C-F4F5BCCE2564"),
			Name = "General Digital Music",
			Description = "This loader recognizes the “General Digital Music” format, which is the internal format of the “Bells, Whistles and Sound Boards” library. This format has the same limitations as the S3M format.\n\nThe BWSB library was written by Edward Schlunder and first released in 1993.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Gdm_Load(LibXmp libXmp)
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
			return new Gdm_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			if (f.Hio_Read32B() != Magic_GDM)
				return -1;

			f.Hio_Seek(start + 0x47, SeekOrigin.Begin);
			if (f.Hio_Read32B() != Magic_GMFS)
				return -1;

			f.Hio_Seek(start + 4, SeekOrigin.Begin);
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
			uint8[] buffer = new uint8[32];
			uint8[] panMap = new uint8[32];

			f.Hio_Read32B();	// Skip magic

			if (f.Hio_Read(buffer, 1, 32) != 32)
				return -1;

			mod.Name = encoder.GetString(buffer);
			
			if (f.Hio_Read(buffer, 1, 32) != 32)
				return -1;

			string author = encoder.GetString(buffer);
			if (author == "Unknown")
				author = string.Empty;

			mod.Author = author;

			f.Hio_Seek(7, SeekOrigin.Current);

			c_int verMaj = f.Hio_Read8();
			c_int verMin = f.Hio_Read8();
			c_int tracker = f.Hio_Read16L();
			c_int tvMaj = f.Hio_Read8();
			c_int tvMin = f.Hio_Read8();

			if (tracker == 0)
				lib.common.LibXmp_Set_Type(m, string.Format("GDM {0}.{1:D2} (2GDM {2}.{3:D2})", verMaj, verMin, tvMaj, tvMin));
			else
				lib.common.LibXmp_Set_Type(m, string.Format("GDM {0}.{1:D2} (unknown tracker {2}.{3:D2})", verMaj, verMin, tvMaj, tvMin));

			if (f.Hio_Read(panMap, 32, 1) == 0)
				return -1;

			for (c_int i = 0; i < 32; i++)
			{
				if (panMap[i] == 255)
				{
					panMap[i] = 8;
					mod.Xxc[i].Vol = 0;
					mod.Xxc[i].Flg |= Xmp_Channel_Flag.Mute;
				}
				else if (panMap[i] == 16)
					panMap[i] = 8;

				mod.Xxc[i].Pan = 0x80 + (panMap[i] - 8) * 16;
			}

			mod.Gvl = f.Hio_Read8();
			mod.Spd = f.Hio_Read8();
			mod.Bpm = f.Hio_Read8();
			f.Hio_Read16L();	// origfmt
			c_int ord_Ofs = (c_int)f.Hio_Read32L();
			mod.Len = f.Hio_Read8() + 1;
			c_int pat_Ofs = (c_int)f.Hio_Read32L();
			mod.Pat = f.Hio_Read8() + 1;
			c_int ins_Ofs = (c_int)f.Hio_Read32L();
			c_int smp_Ofs = (c_int)f.Hio_Read32L();
			mod.Ins = mod.Smp = f.Hio_Read8() + 1;

			// Sanity check
			if (mod.Ins > Constants.Max_Instruments)
				return -1;

			m.C4Rate = Constants.C4_Ntsc_Rate;

			f.Hio_Seek(start + ord_Ofs, SeekOrigin.Begin);

			for (c_int i = 0; i < mod.Len; i++)
				mod.Xxo[i] = f.Hio_Read8();

			// Read instrument data

			f.Hio_Seek(start + ins_Ofs, SeekOrigin.Begin);

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			for (c_int i = 0; i < mod.Ins; i++)
			{
				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					return -1;

				if (f.Hio_Read(buffer, 1, 32) != 32)
					return -1;

				lib.common.LibXmp_Instrument_Name(mod, i, buffer, 32, encoder);
				f.Hio_Seek(12, SeekOrigin.Current);		// Skip filename
				f.Hio_Read8();                                      // Skip EMS handle

				mod.Xxs[i].Len = (c_int)f.Hio_Read32L();
				mod.Xxs[i].Lps = (c_int)f.Hio_Read32L();
				mod.Xxs[i].Lpe = (c_int)f.Hio_Read32L();
				c_int flg = f.Hio_Read8();
				c_int c4Spd = f.Hio_Read16L();
				c_int vol = f.Hio_Read8();
				c_int pan = f.Hio_Read8();

				mod.Xxi[i].Sub[0].Vol = vol > 0x40 ? 0x40 : vol;
				mod.Xxi[i].Sub[0].Pan = pan > 15 ? 0x80 : 0x80 + (pan - 8) * 16;
				lib.period.LibXmp_C2Spd_To_Note(c4Spd, out mod.Xxi[i].Sub[0].Xpo, out mod.Xxi[i].Sub[0].Fin);

				mod.Xxi[i].Sub[0].Sid = i;
				mod.Xxs[i].Flg = Xmp_Sample_Flag.None;

				if (mod.Xxs[i].Len > 0)
					mod.Xxi[i].Nsm = 1;

				if ((flg & 0x01) != 0)
					mod.Xxs[i].Flg |= Xmp_Sample_Flag.Loop;

				if ((flg & 0x02) != 0)
				{
					mod.Xxs[i].Flg |= Xmp_Sample_Flag._16Bit;
					mod.Xxs[i].Len >>= 1;
					mod.Xxs[i].Lps >>= 1;
					mod.Xxs[i].Lpe >>= 1;
				}
			}

			// Read and convert pattern

			f.Hio_Seek(start + pat_Ofs, SeekOrigin.Begin);

			// Effects in muted channels are processed, so scan patterns first to
			// see the real number of channels
			mod.Chn = 0;

			for (c_int i = 0; i < mod.Pat; i++)
			{
				c_int len = f.Hio_Read16L();
				len -= 2;

				for (c_int r = 0; len > 0; )
				{
					c_int c = f.Hio_Read8();
					if (f.Hio_Error() != 0)
						return -1;

					len--;

					if (c == 0)
					{
						r++;

						// Sanity check
						if (len == 0)
						{
							if (r > 64)
								return -1;
						}
						else
						{
							if (r >= 64)
								return -1;
						}

						continue;
					}

					if (mod.Chn <= (c & 0x1f))
						mod.Chn = (c & 0x1f) + 1;

					if ((c & 0x20) != 0)		// Note and sample follows
					{
						f.Hio_Read8();
						f.Hio_Read8();
						len -= 2;
					}

					if ((c & 0x40) != 0)		// Effect(s) follow
					{
						c_int k;

						do
						{
							k = f.Hio_Read8();
							if (f.Hio_Error() != 0)
								return -1;

							len--;

							if ((k & 0xc0) != 0xc0)
							{
								f.Hio_Read8();
								len--;
							}
						}
						while ((k & 0x20) != 0);
					}
				}
			}

			mod.Trk = mod.Pat * mod.Chn;

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				return -1;

			f.Hio_Seek(start + pat_Ofs, SeekOrigin.Begin);

			for (c_int i = 0; i < mod.Pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, 64) < 0)
					return -1;

				c_int len = f.Hio_Read16L();
				len -= 2;

				for (c_int r = 0; len > 0; )
				{
					c_int c = f.Hio_Read8();
					if (f.Hio_Error() != 0)
						return -1;

					len--;

					if (c == 0)
					{
						r++;
						continue;
					}

					// Sanity check
					if (((c & 0x1f) >= mod.Chn) || (r >= 64))
						return -1;

					Xmp_Event @event = Ports.LibXmp.Common.Event(m, i, c & 0x1f, r);
					c_int k;

					if ((c & 0x20) != 0)		// Note and sample follows
					{
						k = f.Hio_Read8();

						// 0 is empty note
						@event.Note = (byte)(k != 0 ? 12 + 12 * Ports.LibXmp.Common.Msn(k & 0x7f) + Ports.LibXmp.Common.Lsn(k) : 0);
						@event.Ins = f.Hio_Read8();
						len -= 2;
					}

					if ((c & 0x40) != 0)		// Effect(s) follow
					{
						do
						{
							k = f.Hio_Read8();
							if (f.Hio_Error() != 0)
								return -1;

							len--;

							switch ((k & 0xc0) >> 6)
							{
								case 0:
								{
									@event.FxT = (byte)(k & 0x1f);
									@event.FxP = f.Hio_Read8();
									len--;

									Fix_Effect(ref @event.FxT, ref @event.FxP);
									break;
								}

								case 1:
								{
									@event.F2T = (byte)(k & 0x1f);
									@event.F2P = f.Hio_Read8();
									len--;

									Fix_Effect(ref @event.F2T, ref @event.F2P);
									break;
								}

								case 2:
								{
									f.Hio_Read8();
									len--;
									break;
								}
							}
						}
						while ((k & 0x20) != 0);
					}
				}
			}

			// Read samples

			f.Hio_Seek(start + smp_Ofs, SeekOrigin.Begin);

			for (c_int i = 0; i < mod.Ins; i++)
			{
				if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.Uns, mod.Xxs[i], null, i) < 0)
					return -1;
			}

			m.Quirk |= Quirk_Flag.ArpMem | Quirk_Flag.FineFx;

			// BWSB actually gets several aspects of this wrong, but this
			// seems to be the intent. No original GDMs exist so it's not
			// likely there's a reason to simulate its mistakes here
			m.Flow_Mode = FlowMode_Flag.Mode_ST3_321;

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Fix_Effect(ref uint8 fxT, ref uint8 fxP)
		{
			switch (fxT)
			{
				// No effect
				case 0x00:
				{
					fxP = 0;
					break;
				}

				// Same as ProTracker
				case 0x01:
				case 0x02:
				case 0x03:
				case 0x04:
				case 0x05:
				case 0x06:
				case 0x07:
				{
					break;
				}

				case 0x08:
				{
					fxT = Effects.Fx_Tremor;
					break;
				}

				// Same as ProTracker
				case 0x09:
				case 0x0a:
				case 0x0b:
				case 0x0c:
				case 0x0d:
				{
					break;
				}

				case 0x0e:
				{
					// Convert some extended effects to their S3M equivalents. This is
					// necessary because the continue effects were left as the original
					// effect (e.g. FX_VOLSLIDE for the fine volume slides) by 2GDM!
					// Otherwise, these should be the same as ProTracker
					c_int h = Ports.LibXmp.Common.Msn(fxP);
					c_int l = Ports.LibXmp.Common.Lsn(fxP);

					switch (h)
					{
						case Effects.Ex_F_Porta_Up:
						{
							fxT = Effects.Fx_Porta_Up;
							fxP = (byte)(l | 0xf0);
							break;
						}

						case Effects.Ex_F_Porta_Dn:
						{
							fxT = Effects.Fx_Porta_Dn;
							fxP = (byte)(l | 0xf0);
							break;
						}

						// Extra fine portamento up
						case 0x8:
						{
							fxT = Effects.Fx_Porta_Up;
							fxP = (byte)(l | 0xe0);
							break;
						}

						// Extra fine portamento down
						case 0x9:
						{
							fxT = Effects.Fx_Porta_Dn;
							fxP = (byte)(l | 0xe0);
							break;
						}

						case Effects.Ex_F_VSlide_Up:
						{
							// Don't convert 0 as it would turn into volume slide down...
							if (l != 0)
							{
								fxT = Effects.Fx_VolSlide;
								fxP = (byte)((l << 4) | 0xf);
							}
							break;
						}

						case Effects.Ex_F_VSlide_Dn:
						{
							// Don't convert 0 as it would turn into volume slide up...
							if (l != 0)
							{
								fxT = Effects.Fx_VolSlide;
								fxP = (byte)(l | 0xf0);
							}
							break;
						}
					}
					break;
				}

				// Set speed
				case 0x0f:
				{
					fxT = Effects.Fx_S3M_Speed;
					break;
				}

				// Arpeggio
				case 0x10:
				{
					fxT = Effects.Fx_S3M_Arpeggio;
					break;
				}

				// Set internal flag
				case 0x11:
				{
					fxT = fxP = 0;
					break;
				}

				case 0x12:
				{
					fxT = Effects.Fx_Multi_Retrig;
					break;
				}

				case 0x13:
				{
					fxT = Effects.Fx_GlobalVol;
					break;
				}

				case 0x14:
				{
					fxT = Effects.Fx_Fine_Vibrato;
					break;
				}

				// Special misc
				case 0x1e:
				{
					switch (Ports.LibXmp.Common.Msn(fxP))
					{
						// Sample control
						case 0x0:
						{
							if (Ports.LibXmp.Common.Lsn(fxP) == 1)	// Enable surround
							{
								// This is the only sample control effect
								// that 2GDM emits. BWSB ignores it,
								// but supporting it is harmless
								fxT = Effects.Fx_Surround;
								fxP = 1;
							}
							else
								fxT = fxP = 0;

							break;
						}

						// Set pan position
						case 0x8:
						{
							fxT = Effects.Fx_Extended;
							break;
						}

						default:
						{
							fxT = fxP = 0;
							break;
						}
					}
					break;
				}

				case 0x1f:
				{
					fxT = Effects.Fx_S3M_Bpm;
					break;
				}

				default:
				{
					fxT = fxP = 0;
					break;
				}
			}
		}
		#endregion
	}
}
