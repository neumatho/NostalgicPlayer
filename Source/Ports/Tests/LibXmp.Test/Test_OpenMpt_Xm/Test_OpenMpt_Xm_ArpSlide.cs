/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_Xm
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_Xm
	{
		/********************************************************************/
		/// <summary>
		/// Like in some other trackers (e.g. Impulse Tracker), arpeggio
		/// notes are supposed to be relative to the current note frequency,
		/// i.e. the arpeggio should still sound as intended after executing
		/// a portamento. However, this is not quite as simple as in
		/// Impulse Tracker, since the base note is first rounded (up or down)
		/// to the nearest semitone and then the arpeggiated note is added
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_ArpSlide()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "ArpSlide.xm", "ArpSlide.data");
		}
	}
}
