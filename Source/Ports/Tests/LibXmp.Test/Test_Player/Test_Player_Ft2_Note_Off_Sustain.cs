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
		/// Instrument numbers reset sustain release unless they are
		/// accompanied by a key off (or K00 without toneporta). These also
		/// prevent the envelope positions from being reset.
		///
		/// Active invalid instruments do not block clearing sustain release,
		/// unlike with envelope position reset.
		///
		/// 00-07: lone instrument number resets envelopes/release.
		/// 08-0F: delayed keyoff + instrument number resets envelopes/release.
		/// 10-15: keyoff + instrument number does not reset envelopes/release.
		/// 16-1B: K00 + instrument number does not reset envelopes/release.
		/// 1C-23: Envelope positions are not reset (or advanced) while an
		///        invalid instrument is active, but release is reset on rows
		///        1E and 1F. The envelope should continue where it left off
		///        starting at row 20 and hold at the sustain point.
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Note_Off_Sustain()
		{
			Compare_Mixer_Data(dataDirectory, "Ft2_Note_Off_Sustain.xm", "Ft2_Note_Off_Sustain.data");
		}
	}
}
