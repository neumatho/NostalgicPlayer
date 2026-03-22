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
		/// This module should play the first pattern backward and then
		/// continue to the second pattern. Each row should increase the play
		/// time by 50ms (and never reset the play time to the start time of
		/// the first pattern)
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Pattern_Jump_It_SamePos()
		{
			Compare_Mixer_Data(dataDirectory, "Pattern_Jump_It_SamePos.it", "Pattern_Jump_It_SamePos.data");
		}
	}
}
