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
		/// 25 - Portamento and pitch slide
		///
		/// The first two segments of this test should both play identically,
		/// with a slight downward slide followed by a faster slide back up,
		/// and then remain at the starting pitch. The third and fourth are
		/// provided primarily for reference for possible misimplementations.
		/// (The third slides down and then continually rises; the fourth
		/// slides down slightly and then plays a constant pitch.)
		///
		/// If the first and third segment are identical, then either Fx and
		/// Gxx aren't sharing their values, or the Fx values aren't being
		/// multiplied (volume column F1 should be equivalent to a "normal"
		/// F04).
		///
		/// If the first and fourth are identical, then the volume column is
		/// most likely being processed prior to the effect column.
		/// Apparently, Impulse Tracker handles the volume column effects
		/// last; note how the G04 needs to be placed on the next row after
		/// the F1 in the second segment in order to mirror the behavior of
		/// the first
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_25_Portamento_And_Pitch_Slide()
		{
			Compare_Mixer_Data(dataDirectory, "Storlek_25.it", "Storlek_25.data");
		}
	}
}
