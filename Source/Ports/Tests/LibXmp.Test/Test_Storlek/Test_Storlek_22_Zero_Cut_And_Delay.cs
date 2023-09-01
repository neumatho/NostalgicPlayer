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
		/// 22 - Zero value for note cut and note delay
		///
		/// Impulse Tracker handles SD0 and SC0 as SD1 and SC1, respectively.
		/// (As a side note, Scream Tracker 3 ignores notes with SD0
		/// completely, and doesn't cut notes at all with SC0.)
		///
		/// If these effects are handled correctly, the notes on the first
		/// row should trigger simultaneously; the next pair of notes should
		/// not; and the final two sets should both play identically and cut
		/// the notes after playing for one tick
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_22_Zero_Cut_And_Delay()
		{
			Compare_Mixer_Data(dataDirectory, "Storlek_22.it", "Storlek_22.data");
		}
	}
}
