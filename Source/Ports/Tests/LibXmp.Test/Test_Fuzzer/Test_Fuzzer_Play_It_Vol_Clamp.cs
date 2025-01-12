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
		/// This IT contains out-of-bounds global volume values in the sample
		/// and instrument, as well as out-of-bounds envelope volume and
		/// channel volume. libxmp was clamping the latter two before but not
		/// the former. In conjunction with NNA spam which is hard to
		/// analyze, these were causing signed integer overflows in the mixer.
		///
		/// Since the UB is hard to reproduce in a controlled manner, just
		/// make sure the mixer is seeing a normal volume level on this note
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_It_Vol_Clamp()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "F"), "Play_It_Vol_Clamp.it", "Play_It_Vol_Clamp.data");
		}
	}
}
