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
		/// Don't allow note delay to carry into NNA channels
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_It_Note_Delay_Nna()
		{
			Compare_Mixer_Data(dataDirectory, "It_Note_Delay_Nna.it", "It_Note_Delay_Nna.data");
		}
	}
}
