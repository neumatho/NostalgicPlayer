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
		/// Note cut, note off, and note fade should be performed regardless
		/// of whether or not they are provided with an invalid instrument
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_It_Cut_Invalid_Ins()
		{
			Compare_Mixer_Data(dataDirectory, "It_Cut_Invalid_Ins.it", "It_Cut_Invalid_Ins.data");
		}
	}
}
