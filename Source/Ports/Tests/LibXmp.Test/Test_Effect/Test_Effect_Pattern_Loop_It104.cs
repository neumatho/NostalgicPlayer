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
		/// Test Ultra Tracker tempo effects
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Pattern_Loop_It104()
		{
			Compare_Mixer_Data(dataDirectory, "Pattern_Loop_It104.it", "Pattern_Loop_It104.data");
		}
	}
}
