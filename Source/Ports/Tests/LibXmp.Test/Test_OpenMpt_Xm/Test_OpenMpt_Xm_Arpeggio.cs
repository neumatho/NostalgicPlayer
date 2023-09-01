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
		/// Arpeggio behavior is very weird with more than 16 ticks per row.
		/// This comes from the fact that Fasttracker 2 uses a LUT for
		/// computing the arpeggio note (instead of doing something like
		/// tick%3 or similar). The LUT only has 16 entries, so when there
		/// are more than 16 ticks, it reads beyond array boundaries. The
		/// vibrato table happens to be stored right after arpeggio table.
		/// The tables look like this in memory:
		///
		/// ArpTab: 0,1,2,0,1,2,0,1,2,0,1,2,0,1,2,0
		/// VibTab: 0,24,49,74,97,120,141,161,180,197,...
		///
		/// All values except for the first in the vibrato table are greater
		/// than 1, so they trigger the third arpeggio note. Keep in mind
		/// that Fasttracker 2 counts downwards, so the table has to be read
		/// from back to front, i.e. at 16 ticks per row, the 16th entry in
		/// the LUT is the first to be read. This is also the reason why
		/// Arpeggio is played "backwards" in Fasttracker 2
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_Arpeggio()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "Arpeggio.xm", "Arpeggio.data");
		}
	}
}
