/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C.Containers;

namespace NostalgicPlayer.Kit.C.Test.Time
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class TestTimeBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected tm Tm_Make(c_int year, c_int month, c_int day, c_int hour, c_int minute, c_int second, c_int wDay, c_int yDay, c_int isDst)
		{
			return new tm
			{
				tm_Year = year - 1900,
				tm_Mon = month - 1,
				tm_MDay = day,
				tm_Hour = hour,
				tm_Min = minute,
				tm_Sec = second,
				tm_WDay = wDay,
				tm_YDay = yDay,
				tm_IsDst = isDst
			};
		}
	}
}
