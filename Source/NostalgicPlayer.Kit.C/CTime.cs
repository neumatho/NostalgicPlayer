/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polycode.NostalgicPlayer.Kit.C.Containers;

namespace Polycode.NostalgicPlayer.Kit.C
{
	/// <summary>
	/// C like time methods
	/// </summary>
	public static class CTime
	{
		/********************************************************************/
		/// <summary>
		/// Thread-safe conversion from time_t (seconds since
		/// 1970-01-01 00:00:00 UTC) to broken down UTC time. Fills the
		/// supplied tm buffer and returns it
		/// </summary>
		/********************************************************************/
		public static tm gmtime_r(time_t timer, out tm buf)
		{
			// Convert to UTC DateTimeOffset. This matches POSIX gmtime semantics
			// (no timezone offset, no DST calculation)
			DateTimeOffset dto;

			try
			{
				// FromUnixTimeSeconds interprets the input as seconds from Unix epoch (UTC)
				dto = DateTimeOffset.FromUnixTimeSeconds(timer);
			}
			catch (ArgumentOutOfRangeException)
			{
				// Clamp out-of-range values to min/max representable DateTimeOffset
				// to avoid throwing. This mirrors typical undefined behavior handling
				dto = timer >= 0 ? DateTimeOffset.MaxValue : DateTimeOffset.MinValue;
			}

			buf.tm_Sec = dto.Second;
			buf.tm_Min = dto.Minute;
			buf.tm_Hour = dto.Hour;
			buf.tm_MDay = dto.Day;
			buf.tm_Mon = dto.Month - 1;
			buf.tm_Year = dto.Year - 1900;
			buf.tm_WDay = (c_int)dto.DayOfWeek;
			buf.tm_YDay = dto.DayOfYear - 1;
			buf.tm_IsDst = 0;

			return buf;
		}



		/********************************************************************/
		/// <summary>
		/// Thread-safe conversion from time_t to broken down local time
		/// (similar to POSIX localtime_r). DST flag is set if in effect
		/// </summary>
		/********************************************************************/
		public static tm localtime_r(time_t timer, out tm buf)
		{
			DateTimeOffset dto;

			try
			{
				// Convert to local time preserving wall clock representation
				dto = DateTimeOffset.FromUnixTimeSeconds(timer).ToLocalTime();
			}
			catch (ArgumentOutOfRangeException)
			{
				dto = timer >= 0 ? DateTimeOffset.MaxValue : DateTimeOffset.MinValue;
			}

			DateTime local = dto.LocalDateTime;

			buf.tm_Sec = local.Second;
			buf.tm_Min = local.Minute;
			buf.tm_Hour = local.Hour;
			buf.tm_MDay = local.Day;
			buf.tm_Mon = local.Month - 1;
			buf.tm_Year = local.Year - 1900;
			buf.tm_WDay = (c_int)local.DayOfWeek;
			buf.tm_YDay = local.DayOfYear - 1;
			buf.tm_IsDst = TimeZoneInfo.Local.IsDaylightSavingTime(local) ? 1 : 0;

			return buf;
		}



		/********************************************************************/
		/// <summary>
		/// Convert broken-down local time to time_t (seconds since epoch).
		/// Returns -1 on error
		/// </summary>
		/********************************************************************/
		public static time_t mktime(tm dt)
		{
			try
			{
				c_int year = dt.tm_Year + 1900;
				c_int month = dt.tm_Mon + 1;
				c_int day = dt.tm_MDay;
				c_int hour = dt.tm_Hour;
				c_int minute = dt.tm_Min;
				c_int second = dt.tm_Sec;

				DateTime local = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
				DateTimeOffset off = new DateTimeOffset(local);

				return off.ToUnixTimeSeconds();
			}
			catch
			{
				return -1;
			}
		}
	}
}
