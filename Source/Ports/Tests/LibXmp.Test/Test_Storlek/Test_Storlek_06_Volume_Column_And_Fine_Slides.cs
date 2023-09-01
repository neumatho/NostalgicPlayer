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
		/// 06 - Volume column and fine slides
		///
		/// Impulse Tracker's handling of volume column pitch slides along
		/// with its normal effect memory is rather odd. While the two do
		/// share their effect memory, fine slides are not handled in the
		/// volume column.
		///
		/// When this test is played 100% correctly, the note will slide very
		/// slightly downward, way up, and then slightly back down
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_06_Volume_Column_And_Fine_Slides()
		{
			Compare_Mixer_Data(dataDirectory, "Storlek_06.it", "Storlek_06.data");
		}
	}
}
