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
		/// In this test, all possible combinations of the envelope sustain
		/// point and envelope loops are tested, and you can see their
		/// behaviour on note-off. If the sustain point is at the loop end
		/// and the sustain loop has been released, don't loop anymore.
		/// Probably the most important thing for this test is that in
		/// Fasttracker 2 (and Impulse Tracker), envelope position is
		/// incremented before the point is evaluated, not afterwards, so
		/// when no ticks have been processed yet, the envelope position
		/// should be invalid
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_EnvLoops()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "EnvLoops.xm", "EnvLoops.data");
		}
	}
}
