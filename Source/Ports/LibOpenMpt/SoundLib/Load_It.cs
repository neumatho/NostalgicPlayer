/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// IT (Impulse Tracker) module loader
	/// Notes: Also handles MPTM loading, as the formats are almost identical
	/// </summary>
	internal partial class CSoundFile
	{
		/********************************************************************/
		/// <summary>
		/// Get version of Impulse Tracker that was used to create an IT/S3M
		/// file
		/// </summary>
		/********************************************************************/
		private string GetImpulseTrackerVersion(uint16 cwtv, uint16 cmwt)//XX 345
		{
			string version;

			cwtv &= 0xfff;

			if (cmwt > 0x0214)
				version = "Impulse Tracker 2.15";
			else if ((cwtv >= 0x0215) && (cwtv <= 0x0217))
			{
				string[] versions = [ "1-2", "3", "4-5" ];

				version = string.Format("Impulse Tracker 2.14p{0}", versions[cwtv - 0x0215]);
			}
			else
				version = string.Format("Impulse Tracker {0}.{1:x2}", (cwtv & 0x0f00) >> 8, cwtv & 0xff);

			return version;
		}



		/********************************************************************/
		/// <summary>
		/// Get version of Schism Tracker that was used to create an IT/S3M
		/// file
		/// </summary>
		/********************************************************************/
		private string GetSchismTrackerVersion(uint16 cwtv, uint32 reserved)//XX 365
		{
			// Schism Tracker version information in a nutshell:
			// < 0x020: a proper version (files saved by such versions are likely very rare)
			// = 0x020: any version between the 0.2a release (2005-04-29?) and 2007-04-17
			// = 0x050: anywhere from 2007-04-17 to 2009-10-31
			// > 0x050: the number of days since 2009-10-31
			// = 0xFFF: any version starting from 2020-10-28 (exact version stored in reserved value)
			cwtv &= 0xfff;

			if (cwtv > 0x50)
			{
				int32 date = (int32)(ItTools.SchismTrackerEpoch + (cwtv < 0xfff ? cwtv - 0x050 : reserved));
				int32 y = (int32)((Util.Mul32To64(10000, date) + 14780) / 3652425);
				int32 ddd = date - ((365 * y) + (y / 4) - (y / 100) + (y / 400));

				if (ddd < 0)
				{
					y--;
					ddd = date - ((365 * y) + (y / 4) - (y / 100) + (y / 400));
				}

				int32 mi = ((100 * ddd) + 52) / 3060;

				return string.Format("Schism Tracker {0:D4}-{1:D2}-{2:D2}", y + ((mi + 2) / 12), ((mi + 2) % 12) + 1, ddd - (((mi * 306) + 5) / 10) + 1);
			}
			else
				return string.Format("Schism Tracker 0.{0:x2}", cwtv);
		}
	}
}
