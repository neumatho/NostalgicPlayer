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
		/// Volume events do not reset fadeout. Likely to counteract other
		/// bugs, libxmp was resetting fadeout on some volume events, citing
		/// OpenMPT NoteOff.xm and m5v-nine.xm. The former should have
		/// actually been resetting fadeout on the keyoff+delay.
		///
		/// 00-07: volume does not reset fadeout after fadeout has reached 0.
		/// 08-0F: volume does not reset fadeout if the envelope returns 0.
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Volume_FadeOut()
		{
			Compare_Mixer_Data(dataDirectory, "Ft2_Volume_FadeOut.xm", "Ft2_Volume_FadeOut.data");
		}
	}
}
