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
		/// 04 - Compatible Gxx on
		///
		/// If this test is played correctly, the first note will slide up
		/// and back down once, and the final series should play four
		/// distinct notes. If the first note slides up again, either (a)
		/// the player is testing the flag incorrectly (Gxx memory is only
		/// linked if the flag is not set), or (b) the effect memory values
		/// are not set to zero at start of playback
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_04_Compatible_Gxx_On()
		{
			Compare_Mixer_Data(dataDirectory, "Storlek_04.it", "Storlek_04.data");
		}
	}
}
