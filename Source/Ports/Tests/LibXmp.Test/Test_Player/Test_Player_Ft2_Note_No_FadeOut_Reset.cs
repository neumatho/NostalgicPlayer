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
		/// Tests some edge cases of FT2 Kxx.
		///
		/// Pattern 0: no envelope
		/// Pattern 1: trivial envelope
		/// Pattern 2: sustain envelope
		///
		/// 00-03: Kxx no-envelope cut is reset by volume like keyoff.
		///        This does not reset fadeout.
		/// 04-07: Kxx==speed is ignored.
		/// 08-0B: Kxx>speed is ignored.
		/// 0C-0F: Kxx fadeout is reset by keyoff+delay.
		/// 10-13: Kxx>=speed is ignored during the next row's delay ticks.
		/// 14-17: Kxx>=speed is ignored on pattern delay row repeats.
		/// 18-1B: Alternate test for Kxx>=speed + delay.
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Note_No_FadeOut_Reset()
		{
			Compare_Mixer_Data(dataDirectory, "Ft2_Note_No_FadeOut_Reset.xm", "Ft2_Note_No_FadeOut_Reset.data");
		}
	}
}
