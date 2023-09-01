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
		/// A bidirectional sustain loop currently playing in reverse, with
		/// the position past the end of the bidirectional main loop, should
		/// not modify the current position in the sample when sustain is
		/// released. Instead, the sample should continue playing in reverse
		/// until it reaches the loop.
		/// Also see OpenMPT SusAfterLoop.it
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_It_Sus_After_Loop_Bidi()
		{
			Compare_Mixer_Data(dataDirectory, "It_Sus_After_Loop_Bidi.it", "It_Sus_After_Loop_Bidi.data");
		}
	}
}
