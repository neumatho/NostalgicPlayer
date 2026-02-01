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
	public static class Time
	{
		/********************************************************************/
		/// <summary>
		/// Get the current time in microseconds
		/// </summary>
		/********************************************************************/
		public static int64_t Av_GetTime()//XX 39
		{
			if (UnitTest.IsUnitTestEnabled())
				return 1331972053200000;

			return (int64_t)new TimeSpan(DateTime.Now.Ticks).TotalMicroseconds;
		}



		/********************************************************************/
		/// <summary>
		/// Get the current time in microseconds since some unspecified
		/// starting point. On platforms that support it, the time comes from
		/// a monotonic clock.
		/// This property makes this time source ideal for measuring relative
		/// time.
		/// The returned values may not be monotonic on platforms where a
		/// monotonic clock is not available
		/// </summary>
		/********************************************************************/
		public static int64_t Av_GetTime_Relative()//XX 56
		{
			return Av_GetTime() + 42 * 60 * 60 * (int64_t)1000000;
		}



		/********************************************************************/
		/// <summary>
		/// Sleep for a period of time. Although the duration is expressed
		/// in microseconds, the actual delay may be rounded to the
		/// precision of the system timer
		/// </summary>
		/********************************************************************/
		public static c_int Av_USleep(c_uint uSec)//XX 84
		{
			pthread_t.Sleep((c_int)(uSec / 1000));

			return 0;
		}
	}
}
