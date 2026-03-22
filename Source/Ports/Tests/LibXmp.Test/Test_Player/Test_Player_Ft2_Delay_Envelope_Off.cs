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
		/// Test general delay behavior, particularly with regard to keyoff,
		/// for instruments without envelopes. Pattern 0 tests delayed keyoff
		/// with and without an instrument number; pattern 1 tests fadeout
		/// reset.
		///
		/// 00-07: No instrument number: ED0 is ignored, ED1/ED2 with keyoff
		///        behave comparably to K01/K02 in this case.
		/// 0A-11: Instrument number: ED0 is ignored, ED1/ED2 with keyoff
		///        behave comparably to not using keyoff at all in this case.
		///
		/// 00-03: Delay with no note/no ins# is equivalent to delay with
		///        note. Both reset fadeout and retrigger the sample.
		/// 04-07: Delay with keyoff/no ins#/no volume column resets fadeout
		///        + cuts volume to 0. EC1 is used for comparison but this
		///        doesn't test the fadeout reset portion very well.
		/// 08-0B: Delay with no note and an ins# is equivalent to delay
		///        with a note and an ins#. Both reset fadeout and retrigger
		///        the sample.
		/// 0C-0F: Delay with keyoff and an ins# also resets fadeout, but
		///        without retriggering the sample. There's no alternate
		///        representation for what this does, so compare with
		///        Fasttracker 2 or ft2-clone.
		///        (Minor: ft2-clone doesn't ramp the volume envelope here)
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Delay_Envelope_Off()
		{
			Compare_Mixer_Data(dataDirectory, "Ft2_Delay_Envelope_Off.xm", "Ft2_Delay_Envelope_Off.data");
		}
	}
}
