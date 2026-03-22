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
		/// While out-of-range notes prevent updating the active sample,
		/// they don't prevent updating other instrument properties like
		/// envelopes, fadeout rate, and vibrato. This module tests fadeout
		/// rate only.
		///
		/// 00-03: normal fadeout for instrument 2.
		/// 04-07: ›A#9 - instrument 2 sample continues with instrument 1
		///        fadeout.
		/// 08-0B: B-(-1) - instrument 1 C#0 sample plays with instrument 1
		///        fadeout.
		/// 0C-0F: A#(-1) - instrument 2 sample continues with instrument 1
		///        fadeout.
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Note_Range_Instrument_Fade()
		{
			Compare_Mixer_Data(dataDirectory, "Ft2_Note_Range_Instrument_Fade.xm", "Ft2_Note_Range_Instrument_Fade.data");
		}
	}
}
