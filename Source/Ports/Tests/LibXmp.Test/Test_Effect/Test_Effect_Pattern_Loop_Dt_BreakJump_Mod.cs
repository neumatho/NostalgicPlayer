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
		/// Since Digital Tracker has strange position jump bugs in 6/8
		/// channel mode, this test uses an "FA04" module. It's not clear if
		/// Digital Tracker was ever capable of creating these, but it loads
		/// them, and they can be fingerprinted
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Pattern_Loop_Dt_BreakJump_Mod()
		{
			Compare_Mixer_Data(dataDirectory, "Pattern_Loop_Dt_BreakJump.mod", "Pattern_Loop_Dt_BreakJump_Mod.data");
		}
	}
}
