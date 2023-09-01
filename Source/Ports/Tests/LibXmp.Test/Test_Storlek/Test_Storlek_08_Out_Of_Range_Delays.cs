/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Storlek
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Storlek
	{
		/********************************************************************/
		/// <summary>
		/// 08 - Out-of-range note delays
		///
		/// This test is to make sure note delay is handled correctly if the
		/// delay value is out of range. The correct behavior is to act as if
		/// the entire row is empty. Some players ignore the delay value and
		/// play the note on the first tick.
		///
		/// Oddly, Impulse Tracker does save the instrument number, even if
		/// the delay value is out of range. I'm assuming this is a bug;
		/// nevertheless, if a player is going to claim 100% IT compatibility,
		/// it needs to copy the bugs as well.
		///
		/// When played correctly, this should play the first three notes
		/// using the square wave sample, with equal time between the start
		/// and end of each note, and the last note should be played with the
		/// noise sample
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_08_Out_Of_Range_Delays()
		{
			Compare_Mixer_Data(dataDirectory, "Storlek_08.it", "Storlek_08.data");
		}
	}
}
