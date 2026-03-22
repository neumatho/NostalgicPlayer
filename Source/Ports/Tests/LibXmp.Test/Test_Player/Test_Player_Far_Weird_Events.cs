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
		/// Three weird edge cases that should be impossible to insert into
		/// a Farandole Composer module from the editor.
		///
		/// 1) instrument non-zero but no note: should do absolutely nothing.
		/// 2) note with no volume: the volume is interpreted as 0. An
		///    example of this exists in order/pattern 16 of
		///    Prescience/aurora.far.
		/// 3) note with volume >15: the volume is interpreted as 0
		///    (usually?). This also causes graphical bugs in the fake VU
		///    meter.
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Far_Weird_Events()
		{
			Compare_Mixer_Data(dataDirectory, "Far_Weird_Events.far", "Far_Weird_Events.data");
		}
	}
}
