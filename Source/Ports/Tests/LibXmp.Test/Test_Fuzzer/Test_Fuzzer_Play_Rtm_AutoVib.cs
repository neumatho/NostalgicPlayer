/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Fuzzer
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Fuzzer
	{
		/********************************************************************/
		/// <summary>
		/// This RTM has two instruments that use autovibrato depths
		/// out-of-range in Real Tracker, but which play correctly.
		/// One instrument has a positive rate and the other has a negative
		/// rate (another out-of-range, but working, setting), and they are
		/// played offset so that both channels should be synchronized.
		///
		/// This previously caused out-of-bounds array accesses in lfo.c
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_Rtm_AutoVib()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "F"), "Play_Rtm_AutoVib_Oob_Depth_Rate.rtm", "Play_Rtm_AutoVib_Oob_Depth_Rate.data");
		}
	}
}
