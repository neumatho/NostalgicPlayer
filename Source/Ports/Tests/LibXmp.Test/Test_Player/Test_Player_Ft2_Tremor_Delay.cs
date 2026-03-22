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
		/// More FT2 tremor+delay interaction tests.
		///
		/// 00-0B: more verification that delay indeed resets the tremor
		///        counter.
		/// 0C-17: ED0 is, as usual, a no-op. The tremor counter doesn't
		///        reset on these lines.
		/// 18-23: EDx, x>=speed never resets the tremor counter because its
		///        row is never processed. However, it reveals something
		///        interesting:
		///        tremor does not continue updating in the ticks before the
		///        row is processed.
		/// 24-2B: an example of the same quirk using ED2 at speed 3. The
		///        tremor state resets on tick 2 and *would* update on tick 1
		///        if tremor continued to update on rows before the delay
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Tremor_Delay()
		{
			Compare_Mixer_Data(dataDirectory, "Ft2_Tremor_Delay.xm", "Ft2_Tremor_Delay.data");
		}
	}
}
