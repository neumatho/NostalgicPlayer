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
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_It_Bad_Env()
		{
			Playback_Sequence[] sequence = new Playback_Sequence[]
			{
				new Playback_Sequence(Playback_Action.Play_Frames, 8, 0),
				new Playback_Sequence(Playback_Action.Play_End, 0, 0)
			};

			// This IT contains an instrument envelope with a sustain end value of
			// 255, well past the number of envelope points. libxmp should ignore
			// the envelope sustain in this case instead of crashing
			Util.Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_It_Bad_Env_Sustain.it", sequence, 4000, 0, 0);

			// This IT contains misordered envelope points at the start of the volume
			// envelope, which could cause the interpolation formula to emit negative
			// values and overflow later volume calculations (UBSan)
			Util.Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_It_Bad_Env_Order.it", sequence, 4000, 0, 0);
		}
	}
}
