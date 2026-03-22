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
		/// Lines with instrument number and K00 set default volume/pan.
		/// Lines with instrument number and K00 DON'T reset the fadeout
		/// position
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_K00_Is_Note_Off()
		{
			Compare_Mixer_Data(dataDirectory, "Ft2_K00_Is_Note_Off.xm", "Ft2_K00_Is_Note_Off.data");
		}
	}
}
