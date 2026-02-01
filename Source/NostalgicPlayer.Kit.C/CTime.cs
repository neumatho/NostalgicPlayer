/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Text;
using Polycode.NostalgicPlayer.Kit.C.Containers;

namespace Polycode.NostalgicPlayer.Kit.C
{
	/// <summary>
	/// C like time methods
	/// </summary>
	public static class CTime
	{
		// Day names and month names
		private static readonly string[] dayNames = [ "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" ];
		private static readonly string[] dayNamesShort = [ "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" ];
		private static readonly string[] monthNames = [ "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" ];
		private static readonly string[] monthNamesShort = [ "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" ];

		/********************************************************************/
		/// <summary>
		/// Thread-safe conversion from time_t (seconds since
		/// 1970-01-01 00:00:00 UTC) to broken down UTC time. Fills the
		/// supplied tm buffer and returns it
		/// </summary>
		/********************************************************************/
		public static tm gmtime(time_t timer)
		{
			return gmtime_r(timer, out _);
		}



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
		/// (similar to POSIX localtime_r). DST flag is set if in effect.
		/// Respects TZ environment variable if set via CEnvironment.putenv()
		/// </summary>
		/********************************************************************/
		public static tm localtime_r(time_t timer, out tm buf)
		{
			DateTimeOffset dto;
			TimeZoneInfo timeZone = TimeZoneInfo.Local;

			// Check if TZ environment variable is set
			CPointer<char> tzValue = CEnvironment.getenv("TZ");

			if (tzValue.IsNotNull)
			{
				try
				{
					// Try to find the timezone by ID
					timeZone = TimeZoneInfo.FindSystemTimeZoneById(tzValue.ToString());
				}
				catch
				{
					// If timezone not found, fall back to local
					timeZone = TimeZoneInfo.Local;
				}
			}

			try
			{
				// Convert to UTC first, then to target timezone
				dto = DateTimeOffset.FromUnixTimeSeconds(timer);
				dto = TimeZoneInfo.ConvertTime(dto, timeZone);
			}
			catch (ArgumentOutOfRangeException)
			{
				dto = timer >= 0 ? DateTimeOffset.MaxValue : DateTimeOffset.MinValue;
			}

			DateTime local = dto.DateTime;

			buf.tm_Sec = local.Second;
			buf.tm_Min = local.Minute;
			buf.tm_Hour = local.Hour;
			buf.tm_MDay = local.Day;
			buf.tm_Mon = local.Month - 1;
			buf.tm_Year = local.Year - 1900;
			buf.tm_WDay = (c_int)local.DayOfWeek;
			buf.tm_YDay = local.DayOfYear - 1;
			buf.tm_IsDst = timeZone.IsDaylightSavingTime(local) ? 1 : 0;

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



		/********************************************************************/
		/// <summary>
		/// Format a time structure into a string
		/// </summary>
		/********************************************************************/
		public static size_t strftime(CPointer<char> s, size_t max, string fmt, tm tm)
		{
			return strftime(s, max, fmt.ToCharPointer(), tm);
		}



		/********************************************************************/
		/// <summary>
		/// Format a time structure into a string
		/// </summary>
		/********************************************************************/
		public static size_t strftime(CPointer<char> s, size_t max, CPointer<char> fmt, tm tm)
		{
			if (s.IsNull || fmt.IsNull || (max == 0))
				return 0;

			StringBuilder sb = new StringBuilder();
			c_int fmtIdx = 0;
			c_int fmtLen = fmt.Length;

			while ((fmtIdx < fmtLen) && (fmt[fmtIdx] != '\0'))
			{
				char c = fmt[fmtIdx];

				if (c != '%')
				{
					sb.Append(c);
					fmtIdx++;
					continue;
				}

				// Handle format specifier
				fmtIdx++;	// Skip '%'

				if ((fmtIdx >= fmtLen) || (fmt[fmtIdx] == '\0'))
					break;

				// Handle modifiers (E and O - POSIX extensions, we ignore them for now)
				bool hasE = false;
				bool hasO = false;

				if ((fmt[fmtIdx] == 'E') || (fmt[fmtIdx] == 'O'))
				{
					hasE = fmt[fmtIdx] == 'E';
					hasO = fmt[fmtIdx] == 'O';
					fmtIdx++;

					if ((fmtIdx >= fmtLen) || (fmt[fmtIdx] == '\0'))
						break;
				}

				char spec = fmt[fmtIdx];
				fmtIdx++;

				switch (spec)
				{
					case 'a':	// Abbreviated weekday name
					{
						c_int wDay = tm.tm_WDay;

						if ((wDay >= 0) && (wDay < 7))
							sb.Append(dayNamesShort[wDay]);

						break;
					}

					case 'A':	// Full weekday name
					{
						c_int wDay = tm.tm_WDay;

						if ((wDay >= 0) && (wDay < 7))
							sb.Append(dayNames[wDay]);

						break;
					}

					case 'b':	// Abbreviated month name
					case 'h':	// Same as %b
					{
						c_int mon = tm.tm_Mon;

						if ((mon >= 0) && (mon < 12))
							sb.Append(monthNamesShort[mon]);

						break;
					}

					case 'B':	// Full month name
					{
						c_int mon = tm.tm_Mon;

						if ((mon >= 0) && (mon < 12))
							sb.Append(monthNames[mon]);

						break;
					}

					case 'c':	// Preferred date and time representation
					{
						// Format: "Thu Aug 23 14:55:02 2001"
						sb.Append(Format_DateTime(tm, "%a %b %e %H:%M:%S %Y"));
						break;
					}

					case 'C':	// Century (year / 100) as 2-digit number
					{
						c_int century = (tm.tm_Year + 1900) / 100;
						sb.Append(century.ToString("D2"));
						break;
					}

					case 'd':	// Day of month (01-31)
					{
						sb.Append(tm.tm_MDay.ToString("D2"));
						break;
					}

					case 'D':	// Short MM/DD/YY date
					{
						sb.Append(Format_DateTime(tm, "%m/%d/%y"));
						break;
					}

					case 'e':	// Day of month, space-padded ( 1-31)
					{
						sb.Append(tm.tm_MDay.ToString().PadLeft(2, ' '));
						break;
					}

					case 'F':	// ISO 8601 date format (YYYY-MM-DD)
					{
						sb.Append(Format_DateTime(tm, "%Y-%m-%d"));
						break;
					}

					case 'g':	// ISO 8601 week-based year, last 2 digits
					case 'G':	// ISO 8601 week-based year (4 digits)
					{
						// Simplified: just use normal year
						c_int year = tm.tm_Year + 1900;
						sb.Append(spec == 'g' ? (year % 100).ToString("D2") : year.ToString("D4"));
						break;
					}

					case 'H':	// Hour (00-23)
					{
						sb.Append(tm.tm_Hour.ToString("D2"));
						break;
					}

					case 'I':	// Hour (01-12)
					{
						c_int hour12 = tm.tm_Hour % 12;

						if (hour12 == 0)
							hour12 = 12;

						sb.Append(hour12.ToString("D2"));
						break;
					}

					case 'j':	// Day of year (001-366)
					{
						sb.Append((tm.tm_YDay + 1).ToString("D3"));
						break;
					}

					case 'k':	// Hour (0-23), space-padded
					{
						sb.Append(tm.tm_Hour.ToString().PadLeft(2, ' '));
						break;
					}

					case 'l':	// Hour (1-12), space-padded
					{
						c_int hour12 = tm.tm_Hour % 12;

						if (hour12 == 0)
							hour12 = 12;

						sb.Append(hour12.ToString().PadLeft(2, ' '));
						break;
					}

					case 'm':	// Month (01-12)
					{
						sb.Append((tm.tm_Mon + 1).ToString("D2"));
						break;
					}

					case 'M':	// Minute (00-59)
					{
						sb.Append(tm.tm_Min.ToString("D2"));
						break;
					}

					case 'n':	// Newline
					{
						sb.Append('\n');
						break;
					}

					case 'p':	// AM or PM
					{
						sb.Append(tm.tm_Hour < 12 ? "AM" : "PM");
						break;
					}

					case 'P':	// am or pm (lowercase)
					{
						sb.Append(tm.tm_Hour < 12 ? "am" : "pm");
						break;
					}

					case 'r':	// 12-hour time (%I:%M:%S %p)
					{
						sb.Append(Format_DateTime(tm, "%I:%M:%S %p"));
						break;
					}

					case 'R':	// 24-hour HH:MM time (%H:%M)
					{
						sb.Append(Format_DateTime(tm, "%H:%M"));
						break;
					}

					case 's':	// Seconds since epoch (Unix timestamp)
					{
						// Convert tm back to time_t
						time_t timestamp = mktime(tm);
						sb.Append(timestamp.ToString());
						break;
					}

					case 'S':	// Second (00-59)
					{
						sb.Append(tm.tm_Sec.ToString("D2"));
						break;
					}

					case 't':	// Tab
					{
						sb.Append('\t');
						break;
					}

					case 'T':	// ISO 8601 time format (%H:%M:%S)
					{
						sb.Append(Format_DateTime(tm, "%H:%M:%S"));
						break;
					}

					case 'u':	// ISO 8601 weekday (1-7, Monday = 1)
					{
						c_int wDay = tm.tm_WDay;
						c_int isoDay = wDay == 0 ? 7 : wDay;	// Sunday = 7, Monday = 1
						sb.Append(isoDay.ToString());
						break;
					}

					case 'U':	// Week number (00-53), Sunday as first day of week
					{
						c_int week = CalculateWeek(tm, 0);
						sb.Append(week.ToString("D2"));
						break;
					}

					case 'V':	// ISO 8601 week number (01-53)
					{
						// Simplified implementation
						c_int week = CalculateWeek(tm, 1);
						sb.Append(week.ToString("D2"));
						break;
					}

					case 'w':	// Weekday (0-6, Sunday = 0)
					{
						sb.Append(tm.tm_WDay.ToString());
						break;
					}

					case 'W':	// Week number (00-53), Monday as first day of week
					{
						c_int week = CalculateWeek(tm, 1);
						sb.Append(week.ToString("D2"));
						break;
					}

					case 'x':	// Preferred date representation
					{
						sb.Append(Format_DateTime(tm, "%m/%d/%y"));
						break;
					}

					case 'X':	// Preferred time representation
					{
						sb.Append(Format_DateTime(tm, "%H:%M:%S"));
						break;
					}

					case 'y':	// Year, last 2 digits (00-99)
					{
						c_int year = tm.tm_Year + 1900;
						sb.Append((year % 100).ToString("D2"));
						break;
					}

					case 'Y':	// Year (4 digits)
					{
						c_int year = tm.tm_Year + 1900;
						sb.Append(year.ToString("D4"));
						break;
					}

					case 'z':	// +hhmm or -hhmm numeric timezone
					case 'Z':	// Timezone name or abbreviation
					{
						// Not supported in tm structure, output empty string
						break;
					}

					case '%':	// Literal %
					{
						sb.Append('%');
						break;
					}

					default:	// Unknown specifier, output as-is
					{
						sb.Append('%');

						if (hasE)
							sb.Append('E');

						if (hasO)
							sb.Append('O');

						sb.Append(spec);
						break;
					}
				}
			}

			string result = sb.ToString();
			size_t resultLen = (size_t)result.Length;

			// Copy to output buffer
			if (resultLen >= max)
			{
				// Not enough space, return 0 per C standard behavior
				return 0;
			}

			size_t copyLen = resultLen;
			for (size_t i = 0; i < copyLen; i++)
				s[i] = result[(c_int)i];

			s[copyLen] = '\0';

			return copyLen;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Helper method to format date/time recursively
		/// </summary>
		/********************************************************************/
		private static string Format_DateTime(tm tm, string format)
		{
			CPointer<char> fmt = format.ToCharPointer();
			CPointer<char> buf = new CPointer<char>(1024);

			strftime(buf, (size_t)buf.Length, fmt, tm);

			return buf.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Calculate week number
		/// </summary>
		/********************************************************************/
		private static c_int CalculateWeek(tm tm, c_int firstDayOfWeek)
		{
			// firstDayOfWeek: 0 = Sunday, 1 = Monday
			c_int yDay = tm.tm_YDay;
			c_int wDay = tm.tm_WDay;

			// Calculate days to subtract to get to start of week
			c_int daysToWeekStart = (wDay - firstDayOfWeek + 7) % 7;
			c_int weekStartDay = yDay - daysToWeekStart;

			// Week number is (day of first full week + days since) / 7
			c_int week = (weekStartDay + 7) / 7;

			return week;
		}
		#endregion
	}
}
