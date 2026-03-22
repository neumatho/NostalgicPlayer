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
		/// IT modules from trackers other than Impulse Tracker support more
		/// than 99 samples and instruments. This module contains 200
		/// instruments (the MPT 1.16 maximum) and 255 samples; libxmp should
		/// be able to play this without issue
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_It_Ins_199()
		{
			Compare_Mixer_Data(dataDirectory, "Ins199.it", "Ins199.data");
		}
	}
}
