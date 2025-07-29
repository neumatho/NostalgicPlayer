/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.GuiKit.Extensions
{
	/// <summary>
	/// Extension methods to the TimeSpan class
	/// </summary>
	public static class TimeSpanExtension
	{
		/********************************************************************/
		/// <summary>
		/// Will convert the TimeSpan to a string using the right format
		/// depending on the values
		/// </summary>
		/********************************************************************/
		public static string ToFormattedString(this TimeSpan timeSpan, bool forceHours = false)
		{
			string str;

			if ((int)timeSpan.TotalDays > 0)
				str = timeSpan.ToString(Resources.IDS_TIMEFORMAT_BIG);
			else if (forceHours || ((int)timeSpan.TotalHours > 0))
				str = timeSpan.ToString(Resources.IDS_TIMEFORMAT);
			else
				str = timeSpan.ToString(Resources.IDS_TIMEFORMAT_SMALL);

			return str;
		}
	}
}
