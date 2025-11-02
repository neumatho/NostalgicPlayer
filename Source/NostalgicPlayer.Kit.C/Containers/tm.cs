/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.C.Containers
{
#pragma warning disable CS8981
	/// <summary>
	/// Structure containing a calendar date and time broken down into its components
	/// </summary>
	public struct tm
	{
#pragma warning restore CS8981
		/// <summary>
		/// Seconds after the minute (0 - 61)
		/// It's generally 0-59. The extra range is to accommodate for leap seconds in certain systems
		/// </summary>
		public c_int tm_Sec;

		/// <summary>
		/// Minutes after the hour (0 - 59)
		/// </summary>
		public c_int tm_Min;

		/// <summary>
		/// Hours since midnight (0 - 23)
		/// </summary>
		public c_int tm_Hour;

		/// <summary>
		/// Day of the month (1 - 31)
		/// </summary>
		public c_int tm_MDay;

		/// <summary>
		/// Months since January (0 - 11)
		/// </summary>
		public c_int tm_Mon;

		/// <summary>
		/// Years since 1900
		/// </summary>
		public c_int tm_Year;

		/// <summary>
		/// Days since Sunday (0 - 6)
		/// </summary>
		public c_int tm_WDay;

		/// <summary>
		/// Days since January 1 (0 - 365)
		/// </summary>
		public c_int tm_YDay;

		/// <summary>
		/// Daylight Saving Time flag.
		/// It's greater than zero if Daylight Saving Time is in effect,
		/// zero if Daylight Saving Time is not in effect,
		/// and less than zero if the information is not available
		/// </summary>
		public c_int tm_IsDst;
	}
}
