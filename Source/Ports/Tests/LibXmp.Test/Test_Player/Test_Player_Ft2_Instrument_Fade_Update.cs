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
		/// Instrument fade is applied to a channel on lines with note +
		/// instrument number + no toneporta/K00. Instrument memory, despite
		/// updating the active sample, will NOT update this value (and
		/// possibly others)
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Instrument_Fade_Update()
		{
			Compare_Mixer_Data(dataDirectory, "Ft2_Instrument_Fade_Update.xm", "Ft2_Instrument_Fade_Update.data");
		}
	}
}
