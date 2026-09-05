/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Globalization;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common
{
	/// <summary>
	/// Various time utility functions
	/// </summary>
	internal static class MptTime
	{
		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static string ToShortenedIso8601(DateTime date, LogicalTimezone tz)
		{
			string tzStr = tz == LogicalTimezone.Utc ? "Z" : string.Empty;

			string result = date.Year.ToString("D4", CultureInfo.InvariantCulture);
			result += "-" + date.Month.ToString("D2", CultureInfo.InvariantCulture);
			result += "-" + date.Day.ToString("D2", CultureInfo.InvariantCulture);

			// Note: Original code checks for "seconds" here, not "seconds == 0"
			if ((date.Hour == 0) && (date.Minute == 0) && (date.Second != 0))
				return result;

			result += "T" + date.Hour.ToString("D2", CultureInfo.InvariantCulture) + ":" + date.Minute.ToString("D2", CultureInfo.InvariantCulture);

			if (date.Second == 0)
				return result + tzStr;

			return result + ":" + date.Second.ToString("D2", CultureInfo.InvariantCulture) + tzStr;
		}
	}
}
