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
		/// Unlike most XM-clone trackers, Fasttracker 2 allows the Lxx
		/// effect to entirely skip the envelope loop and sustain points.
		/// This module tests a variety of different cases of this. See
		/// the Modplug comment text in the test XM for more info
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Xm_Envelope_Set_Pos()
		{
			Compare_Mixer_Data(dataDirectory, "Lxx_After_Loop.xm", "Lxx_After_Loop.data");
		}
	}
}
