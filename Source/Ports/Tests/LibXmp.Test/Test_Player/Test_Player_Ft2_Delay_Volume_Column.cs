/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Player
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Player
	{
		/********************************************************************/
		/// <summary>
		/// Volume column volume and pan mostly work as usual with
		/// Fasttracker 2 note delay. The one exception seems to be keyoff +
		/// no ins# + pan + delay, which causes the pan effect to be ignored
		/// for some reason.
		///
		/// Pattern 0 tests no envelope, pattern 1 tests envelope with no
		/// sustain, and pattern 2 tests envelope with sustain. They all
		/// behave the same.
		///
		/// 00-03: note/no note + no ins# + volume + delay -> works
		/// 04-07: note/no note + no ins# + pan    + delay -> works
		/// 08-0B: keyoff       + no ins# + volume + delay -> works
		/// 0C-0F: keyoff       + no ins# + pan    + delay -> DOESN'T WORK
		/// 10-13: note/no note + ins#    + volume + delay -> works
		/// 14-17: note/no note + ins#    + pan    + delay -> works
		/// 18-1B: keyoff       + ins#    + volume + delay -> works
		/// 1C-1F: keyoff       + ins#    + pan    + delay -> works
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Delay_Volume_Column()
		{
			Compare_Mixer_Data(dataDirectory, "Ft2_Delay_Volume_Column.xm", "Ft2_Delay_Volume_Column.data");
		}
	}
}
