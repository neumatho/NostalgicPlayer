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
		/// Instrument memory defaults to 0 (no instrument) for all channels
		/// at playback start. Left = Right
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Instrument_Memory_Default()
		{
			Compare_Mixer_Data(dataDirectory, "Ft2_Instrument_Memory_Default.xm", "Ft2_Instrument_Memory_Default.data");
		}
	}
}
