/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using System.Text;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// 
	/// </summary>
	internal class Common
	{
		// OpenMPT uses a slightly different table
		private static readonly uint8[] preAmp_Table =
		{
			0x60, 0x60, 0x60, 0x70,	// 0-7
			0x80, 0x88, 0x90, 0x98,	// 8-15
			0xA0, 0xA4, 0xA8, 0xB0,	// 16-23
			0xB4, 0xB8, 0xBC, 0xC0	// 24-31
		};

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint32 Magic4(char a, char b, char c, char d)
		{
			return ((uint32)a << 24) | ((uint32)b << 16) | ((uint32)c << 8) | d;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Init_Instrument(Module_Data m)
		{
			Xmp_Module mod = m.Mod;

			if (mod.Ins > 0)
			{
				mod.Xxi = ArrayHelper.InitializeArray<Xmp_Instrument>(mod.Ins);
				if (mod.Xxi == null)
					return -1;
			}

			if (mod.Smp > 0)
			{
				// Sanity check
				if (mod.Smp > Constants.Max_Samples)
					return -1;

				mod.Xxs = ArrayHelper.InitializeArray<Xmp_Sample>(mod.Smp);
				if (mod.Xxs == null)
					return -1;

				m.Xtra = ArrayHelper.InitializeArray<Extra_Sample_Data>(mod.Smp);
				if (m.Xtra == null)
					return -1;

				for (c_int i = 0; i < mod.Smp; i++)
					m.Xtra[i].C5Spd = m.C4Rate;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Sample number adjustment (originally by Vitamin/CAIG).
		/// Only use this AFTER a previous usage of libxmp_init_instrument,
		/// and don't use this to free samples that have already been loaded
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Realloc_Samples(Module_Data m, c_int new_Size)
		{
			Xmp_Module mod = m.Mod;

			// Sanity check
			if (new_Size < 0)
				return -1;

			if (new_Size == 0)
			{
				// Don't rely on implementation-defined realloc(x,0) behaviour
				mod.Smp = 0;
				mod.Xxs = null;
				m.Xtra = null;

				return 0;
			}

			Array.Resize(ref mod.Xxs, new_Size);
			Array.Resize(ref m.Xtra, new_Size);

			if (new_Size > mod.Smp)
			{
				c_int clear_Size = new_Size - mod.Smp;

				for (c_int i = mod.Smp; i < new_Size; i++)
				{
					mod.Xxs[i] = new Xmp_Sample();
					m.Xtra[i] = new Extra_Sample_Data
					{
						C5Spd = m.C4Rate
					};
				}
			}

			mod.Smp = new_Size;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Alloc_SubInstrument(Xmp_Module mod, c_int i, c_int num)
		{
			if (num == 0)
				return 0;

			mod.Xxi[i].Sub = ArrayHelper.InitializeArray<Xmp_SubInstrument>(num);
			if (mod.Xxi[i].Sub == null)
				return -1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Init_Pattern(Xmp_Module mod)
		{
			mod.Xxt = new Xmp_Track[mod.Trk];
			if (mod.Xxt == null)
				return -1;

			mod.Xxp = new Xmp_Pattern[mod.Pat];
			if (mod.Xxp == null)
				return -1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Alloc_Pattern(Xmp_Module mod, c_int num)
		{
			// Sanity check
			if ((num < 0) || (num >= mod.Pat) || (mod.Xxp[num] != null))
				return -1;

			mod.Xxp[num] = new Xmp_Pattern
			{
				Index = new c_int[mod.Chn]
			};
			if (mod.Xxp[num] == null)
				return -1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Alloc_Track(Xmp_Module mod, c_int num, c_int rows)
		{
			// Sanity check
			if ((num < 0) || (num >= mod.Trk) || (mod.Xxt[num] != null) || (rows <= 0))
				return -1;

			mod.Xxt[num] = new Xmp_Track
			{
				Event = ArrayHelper.InitializeArray<Xmp_Event>(rows)
			};
			if (mod.Xxt[num] == null)
				return -1;

			mod.Xxt[num].Rows = rows;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Alloc_Tracks_In_Pattern(Xmp_Module mod, c_int num)
		{
			for (c_int i = 0; i < mod.Chn; i++)
			{
				c_int t = num * mod.Chn + i;
				c_int rows = mod.Xxp[num].Rows;

				if (LibXmp_Alloc_Track(mod, t, rows) < 0)
					return -1;

				mod.Xxp[num].Index[i] = t;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Alloc_Pattern_Tracks(Xmp_Module mod, c_int num, c_int rows)
		{
			// Sanity check
			if ((rows <= 0) || (rows > 256))
				return -1;

			if (LibXmp_Alloc_Pattern(mod, num) < 0)
				return -1;

			mod.Xxp[num].Rows = rows;

			if (LibXmp_Alloc_Tracks_In_Pattern(mod, num) < 0)
				return -1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Instrument_Name(Xmp_Module mod, c_int i, uint8[] r, c_int n, Encoding encoder)
		{
			Ports.LibXmp.Common.Clamp(ref n, 0, 31);

			LibXmp_Copy_Adjust(out mod.Xxi[i].Name, r, n, encoder);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Copy_Adjust(out string s, uint8[] r, c_int n, Encoding encoder)
		{
			s = encoder.GetString(r, 0, n).TrimEnd();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Read_Title(Hio f, out string t, c_int s, Encoding encoder)
		{
			uint8[] buf = new uint8[Constants.Xmp_Name_Size];

			if (s < 0)
			{
				t = null;
				return;
			}

			if (s >= Constants.Xmp_Name_Size)
				s = Constants.Xmp_Name_Size - 1;

			s = (c_int)f.Hio_Read(buf, 1, (size_t)s);
			buf[s] = 0;

			LibXmp_Copy_Adjust(out t, buf, s, encoder);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Set_Type(Module_Data m, string fmt)
		{
			m.Mod.Type = fmt;
		}



		/********************************************************************/
		/// <summary>
		/// Generate a Schism Tracker version string.
		/// Schism Tracker versions are stored as follows:
		///
		/// s_Ver &lt;= 0x50; 0.s_Ver
		/// s_Ver &gt; 0x50, &lt; 0xfff: days from epoch=(s_Ver - 0x50)
		/// s_Ver = 0xfff: days from epoch=l_Ver
		/// </summary>
		/********************************************************************/
		public void LibXmp_Schism_Tracker_String(out string buf, size_t size, c_int s_Ver, c_int l_Ver)
		{
			if (s_Ver >= 0x50)
			{
				int64 t = Schism_Tracker_Date(2009, 10, 31);

				if (s_Ver == 0xfff)
					t += l_Ver;
				else
					t += s_Ver - 0x50;

				// Date algorithm reimplemented from OpenMPT
				c_int year = (c_int)((t * 10000L + 14780) / 3652425);
				c_int dayOfYear = (c_int)(t - (365L * year + (year / 4) - (year / 100) + (year / 400)));

				if (dayOfYear < 0)
				{
					year--;
					dayOfYear = (c_int)(t - (365L * year + (year / 4) - (year / 100) + (year / 400)));
				}

				c_int month = (100 * dayOfYear + 52) / 3060;
				c_int day = dayOfYear - (month * 306 + 5) / 10 + 1;

				year += (month + 2) / 12;
				month = (month + 2) % 12 + 1;

				buf = string.Format("Schism Tracker {0:d4}-{1:d2}-{2:d2}", year, month, day);
			}
			else
				buf = string.Format("Schism Tracker 0.{0:x}", s_Ver);
		}



		/********************************************************************/
		/// <summary>
		/// Old MPT modules (from MPT &lt;= 1.16, older versions of OpenMPT)
		/// rely on a pre-amp routine that scales mix volume down. This is
		/// based on the module's channel count and a tracker pre-amp setting
		/// that isn't saved in the module. This setting defaults to 128.
		/// When fixed to 128, it can be optimized out.
		///
		/// In OpenMPT, this pre-amp routine is only available in the MPT and
		/// OpenMPT 1.17 RC1 and RC2 mix modes. Changing a module to the
		/// compatible or 1.17 RC3 mix modes will permanently disable it for
		/// that module. OpenMPT applies the old mix modes to MPT &lt;= 1.16
		/// modules, "IT 8.88", and in old OpenMPT-made modules that specify
		/// one of these mix modes in their extended properties.
		///
		/// Set mod.chn and m.mvol first!
		/// </summary>
		/********************************************************************/
		public void LibXmp_Apply_Mpt_PreAmp(Module_Data m)
		{
			c_int chn = m.Mod.Chn;
			Ports.LibXmp.Common.Clamp(ref chn, 1, 31);

			m.MVol = (m.MVol * 96) / preAmp_Table[chn >> 1];
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Schism_Tracker_Date(c_int year, c_int month, c_int day)
		{
			c_int mm = (month + 9) % 12;
			c_int yy = year - mm / 10;

			yy = yy * 365 + (yy / 4) - (yy / 100) + (yy / 400);
			mm = (mm * 306 + 5) / 10;

			return yy + mm + (day - 1);
		}
		#endregion
	}
}
