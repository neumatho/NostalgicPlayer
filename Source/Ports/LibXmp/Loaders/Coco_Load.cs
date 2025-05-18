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
	internal class Coco_Load : IFormatLoader
	{
		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Coco = new Format_Loader
		{
			Id = Guid.Parse("563FB8BB-5AD0-46B6-94A9-B4361E4E7DC1"),
			Name = "Coconizer",
			Description = "Tracker created by Playfield for the Acorn computer.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Coco_Load(LibXmp libXmp)
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
			return new Coco_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			uint8[] buf = new uint8[20];

			uint8 x = f.Hio_Read8();

			// Check number of channels
			if ((x != 0x84) && (x != 0x88))
				return -1;

			// Read title
			if (f.Hio_Read(buf, 1, 20) != 20)
				return -1;

			if (Check_Cr(buf, 20) != 0)
				return -1;

			c_int n = f.Hio_Read8();	// Instruments
			if ((n <= 0) || (n > 100))
				return -1;

			f.Hio_Read8();				// Sequences
			f.Hio_Read8();				// Patterns

			uint32 y = f.Hio_Read32L();	// Offset of sequence table
			if ((y < 64) || (y > 0x00100000))
				return -1;

			y = f.Hio_Read32L();		// Offset of patterns
			if ((y < 64) || (y > 0x00100000))
				return -1;

			for (c_int i = 0; i < n; i++)
			{
				c_int ofs = (c_int)f.Hio_Read32L();
				c_int len = (c_int)f.Hio_Read32L();
				c_int vol = (c_int)f.Hio_Read32L();
				c_int lps = (c_int)f.Hio_Read32L();
				c_int lsz = (c_int)f.Hio_Read32L();

				if ((ofs < 64) || (ofs > 0x00100000))
					return -1;

				if ((vol < 0) || (vol > 0xff))
					return -1;

				if ((len < 0) || (lps < 0) || (lsz < 0))
					return -1;

				if ((len > 0x00100000) || (lps > 0x00100000) || (lsz > 0x00100000))
					return -1;

				if ((lps > 0) && ((lps + lsz - 1) > len))
					return -1;

				f.Hio_Read(buf, 1, 11);
				f.Hio_Read8();		// Unused
			}

			f.Hio_Seek(start + 1, SeekOrigin.Begin);
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
			c_int[] smp_Ptr = new c_int[100];
			byte[] buf = new byte[12];

			mod.Chn = f.Hio_Read8() & 0x3f;
			lib.common.LibXmp_Read_Title(f, out mod.Name, 20, encoder);

			c_int idx = mod.Name.IndexOf((char)0x0d);
			if (idx != -1)
				mod.Name = mod.Name.Substring(0, idx);

			lib.common.LibXmp_Set_Type(m, "Coconizer");

			mod.Ins = mod.Smp = f.Hio_Read8();
			mod.Len = f.Hio_Read8();
			mod.Pat = f.Hio_Read8();
			mod.Trk = mod.Pat * mod.Chn;

			c_int seq_Ptr = (c_int)f.Hio_Read32L();
			c_int pat_Ptr = (c_int)f.Hio_Read32L();

			if (f.Hio_Error() != 0)
				return -1;

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			m.Vol_Table = VolTable.LibXmp_Arch_Vol_Table;
			m.VolBase = 0xff;

			for (c_int i = 0; i < mod.Ins; i++)
			{
				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					return -1;

				smp_Ptr[i] = (c_int)f.Hio_Read32L();
				mod.Xxs[i].Len = (c_int)f.Hio_Read32L();
				mod.Xxi[i].Sub[0].Vol = (c_int)(0xff - f.Hio_Read32L());
				mod.Xxi[i].Sub[0].Pan = 0x80;
				mod.Xxs[i].Lps = (c_int)f.Hio_Read32L();
				mod.Xxs[i].Lpe = mod.Xxs[i].Lps + (c_int)f.Hio_Read32L();

				if (mod.Xxs[i].Lpe != 0)
					mod.Xxs[i].Lpe -= 1;

				mod.Xxs[i].Flg = mod.Xxs[i].Lps > 0 ? Xmp_Sample_Flag.Loop : Xmp_Sample_Flag.None;

				f.Hio_Read(buf, 1, 11);
				mod.Xxi[i].Name = encoder.GetString(buf, 0, 11);

				idx = mod.Xxi[i].Name.IndexOf((char)0x0d);
				if (idx != -1)
					mod.Xxi[i].Name = mod.Xxi[i].Name.Substring(0, idx);

				f.Hio_Read8();  // Unused
				mod.Xxi[i].Sub[0].Sid = i;

				if (mod.Xxs[i].Len > 0)
					mod.Xxi[i].Nsm = 1;

				if (f.Hio_Error() != 0)
					return -1;
			}

			// Sequence

			f.Hio_Seek(start + seq_Ptr, SeekOrigin.Begin);

			for (c_int i = 0; ; i++)
			{
				uint8 x = f.Hio_Read8();

				if (x == 0xff)
					break;

				if (i < mod.Len)
					mod.Xxo[i] = x;
			}

			// Patterns

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				return -1;

			if (f.Hio_Seek(start + pat_Ptr, SeekOrigin.Begin) < 0)
				return -1;

			for (c_int i = 0; i < mod.Pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, 64) < 0)
					return -1;

				for (c_int j = 0; j < 64; j++)
				{
					for (c_int k = 0; k < mod.Chn; k++)
					{
						Xmp_Event @event = Ports.LibXmp.Common.Event(m, i, k, j);

						@event.FxP = f.Hio_Read8();
						@event.FxT = f.Hio_Read8();
						@event.Ins = f.Hio_Read8();
						@event.Note = f.Hio_Read8();

						if (@event.Note != 0)
							@event.Note += 12;

						if (f.Hio_Error() != 0)
							return -1;

						Fix_Effect(@event);
					}
				}
			}

			// Read samples

			for (c_int i = 0; i < mod.Ins; i++)
			{
				if (mod.Xxi[i].Nsm == 0)
					continue;

				f.Hio_Seek(start + smp_Ptr[i], SeekOrigin.Begin);

				if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.Vidc, mod.Xxs[i], null, i) < 0)
					return -1;
			}

			for (c_int i = 0; i < mod.Chn; i++)
				mod.Xxc[i].Pan = Common.DefPan(m, (((i + 3) / 2) % 2) * 0xff);

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Check_Cr(CPointer<uint8> s, c_int n)
		{
			while (n-- != 0)
			{
				if (s[0, 1] == 0x0d)
					return 0;
			}

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Fix_Effect(Xmp_Event e)
		{
			switch (e.FxT)
			{
				// 00 xy Normal play or Arpeggio
				//     x First haltnote to add
				//     y Second halftone to subtract
				case 0x00:
				{
					e.FxT = Effects.Fx_Arpeggio;
					break;
				}

				// 01 xx Slide pitch up (until Amis Max), Frequency+InfoByte*64
				// 05 xx Slide pitch up (no limit), Frequency+InfoByte*16
				case 0x01:
				case 0x05:
				{
					e.FxT = Effects.Fx_Porta_Up;
					break;
				}

				// 02 xx Slide pitch down (until Amis Min), Frequency-InfoByte*64
				// 06 xx Slide pitch down (0 limit), Frequency-InfoByte*16
				case 0x02:
				case 0x06:
				{
					e.FxT = Effects.Fx_Porta_Dn;
					break;
				}

				// 03 xx Fine volume up
				case 0x03:
				{
					e.FxT = Effects.Fx_F_VSlide_Up;
					break;
				}

				// 04 xx Fine volume down
				case 0x04:
				{
					e.FxT = Effects.Fx_F_VSlide_Dn;
					break;
				}

				// 07 xy Set stereo position
				//     y Stereo position (1-7,ignored). 1=left, 4=center, 7=right
				case 0x07:
				{
					if ((e.FxP > 0) && (e.FxP < 8))
					{
						e.FxT = Effects.Fx_SetPan;
						e.FxP = (byte)(42 * e.FxP - 40);
					}
					else
						e.FxT = e.FxP = 0;

					break;
				}

				// 08 xx Start auto fine volume up
				// 09 xx Start auto fine volume down
				// 0A xx Start auto pitch up
				// 0B xx Start auto pitch down
				case 0x08:
				case 0x09:
				case 0x0a:
				case 0x0b:
				{
					// FIXME
					e.FxT = e.FxP = 0;
					break;
				}

				// 0C xx Set volume
				case 0x0c:
				{
					e.FxT = Effects.Fx_VolSet;
					e.FxP = (byte)(0xff - e.FxP);
					break;
				}

				// 0D xy Pattern break
				case 0x0d:
				{
					e.FxT = Effects.Fx_Break;
					break;
				}

				// 0E xx Position jump
				case 0x0e:
				{
					e.FxT = Effects.Fx_Jump;
					break;
				}

				// 0F xx Set speed
				case 0x0f:
				{
					e.FxT = Effects.Fx_Speed;
					break;
				}

				// 10 xx Unused
				case 0x10:
				{
					e.FxT = e.FxP = 0;
					break;
				}

				// 11 xx Fine slide pitch up
				// 12 xx Fine slide pitch down
				case 0x11:
				case 0x12:
				{
					// FIXME
					e.FxT = e.FxP = 0;
					break;
				}

				// 13 xx Volume up
				case 0x13:
				{
					e.FxT = Effects.Fx_VolSlide_Up;
					break;
				}

				// 14 xx Volume down
				case 0x14:
				{
					e.FxT = Effects.Fx_VolSlide_Dn;
					break;
				}

				default:
				{
					e.FxT = e.FxP = 0;
					break;
				}
			}
		}
		#endregion
	}
}
