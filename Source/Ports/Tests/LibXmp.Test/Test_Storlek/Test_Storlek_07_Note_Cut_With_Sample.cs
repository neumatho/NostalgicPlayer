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
		/// 07 - Note cut with sample
		///
		/// Some players ignore sample numbers next to a note cut. When
		/// handled correctly, this test should play a square wave, cut it,
		/// and then play the noise sample.
		///
		/// If this test is not handled correctly, make sure samples are
		/// checked regardless of the note's value
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_07_Note_Cut_With_Sample()
		{
			Compare_Mixer_Data(dataDirectory, "Storlek_07.it", "Storlek_07.data");
		}
	}
}
