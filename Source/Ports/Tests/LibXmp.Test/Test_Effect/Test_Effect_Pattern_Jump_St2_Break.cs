/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Effect
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Effect
	{
		/********************************************************************/
		/// <summary>
		/// ST2 always breaks to row 0(!).
		/// TODO: channel 2 should mute on row 1, but does not, because
		/// libxmp doesn't implement ST2's delayed pattern jump behavior
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Pattern_Jump_St2_Break()
		{
			Compare_Mixer_Data(dataDirectory, "Pattern_Jump_St2_Break.stm", "Pattern_Jump_St2_Break.data");
		}
	}
}
