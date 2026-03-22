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
		/// This module is a mirror of the read_event_ft2 tests
		/// (test_new_note*ft2.c, test_no_note*ft2.c, test_porta*ft2.c)
		/// It doesn't need to be updated if those tests are updated, but it
		/// is verified to play the same in libxmp and FT2
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Read_Event()
		{
			Compare_Mixer_Data(dataDirectory, "Read_Event_Ft2.xm", "Read_Event_Ft2.data");
		}
	}
}
