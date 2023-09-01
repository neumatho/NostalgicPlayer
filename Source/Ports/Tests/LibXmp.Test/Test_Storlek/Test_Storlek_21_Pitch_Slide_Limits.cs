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
		/// 21 - Pitch slide limits
		///
		/// Impulse Tracker always increases (or decreases, of course) the
		/// pitch of a note with a pitch slide, with no limit on either the
		/// pitch of the note or the amount of increment.
		///
		/// An odd side effect of this test is the harmonic strangeness
		/// resulting from playing frequencies well above the Nyquist
		/// frequency. Different players will seem to play the notes at
		/// wildly different pitches depending on the interpolation
		/// algorithms and resampling rates used. Even changing the mixer
		/// driver in Impulse Tracker will result in different apparent
		/// playback. The important part of the behavior (and about the only
		/// thing that's fully consistent) is that the frequency is changed
		/// at each step
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_21_Pitch_Slide_Limits()
		{
			Compare_Mixer_Data(dataDirectory, "Storlek_21.it", "Storlek_21.data");
		}
	}
}
