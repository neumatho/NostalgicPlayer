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
		/// Jxx (pattern jump) resets the break row set by Cxx (pattern cut)
		/// to 0
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Pattern_Jump_Liq_Break_No()
		{
			Compare_Mixer_Data(dataDirectory, "Pattern_Jump_Liq_Break_No.liq", "Pattern_Jump_Liq_Break_No.data");
		}
	}
}
