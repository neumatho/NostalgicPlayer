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
		/// Instrument defaults work the same as normal on delay rows.
		/// Pattern 0 tests no envelope, pattern 1 tests envelope.
		///
		/// 00-07: "Note memory"/"retrigger" rows without ins# do not apply
		///        defaults.
		/// 08-0A: Keyoff without ins# rows do not apply defaults.
		/// 
		/// 10-13: "Note memory"/"retrigger" rows with ins# apply defaults.
		/// 14-17: Keyoff with ins# rows apply defaults.
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Delay_Defaults()
		{
			Compare_Mixer_Data(dataDirectory, "Ft2_Delay_Defaults.xm", "Ft2_Delay_Defaults.data");
		}
	}
}
