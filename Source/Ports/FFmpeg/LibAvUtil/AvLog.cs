/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum AvLog
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Skip repeated messages, this requires the user app to use av_log() instead of
		/// (f)printf as the 2 would otherwise interfere and lead to
		/// "Last message repeated x times" messages below (f)printf messages with some
		/// bad luck.
		/// Also to receive the last, "last repeated" line if any, the user app must
		/// call av_log(NULL, AV_LOG_QUIET, "%s", ""); at the end
		/// </summary>
		Skip_Repeated = 1,

		/// <summary>
		/// Include the log severity in messages originating from codecs.
		///
		/// Results in messages such as:
		/// [rawvideo @ 0xDEADBEEF] [error] encode did not produce valid pts
		/// </summary>
		Print_Level = 2,

		/// <summary>
		/// Include system time in log output
		/// </summary>
		Print_Time = 4,

		/// <summary>
		/// Include system date and time in log output
		/// </summary>
		Print_DateTime = 8
	}
}
