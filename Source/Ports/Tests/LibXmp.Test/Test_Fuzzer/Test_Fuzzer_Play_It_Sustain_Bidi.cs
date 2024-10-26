/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Fuzzer
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Fuzzer
	{
		/********************************************************************/
		/// <summary>
		/// Test player bugs caused by sustain release combined with
		/// bidirectional loops, which unfortunately keep showing up
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_It_Sustain_Bidi()
		{
			Playback_Sequence[] sequence = new Playback_Sequence[]
			{
				new Playback_Sequence(Playback_Action.Play_Frames, 2, 0),
				new Playback_Sequence(Playback_Action.Play_End, 0, 0)
			};

			Playback_Sequence[] sequence_Long = new Playback_Sequence[]
			{
				new Playback_Sequence(Playback_Action.Play_Frames, 20, 0),
				new Playback_Sequence(Playback_Action.Play_End, 0, 0)
			};

			// This module has a sample with a bidi sustain loop and a non-bidi
			// regular loop. This was able to cause crashes in libxmp due to its
			// bad bidi and sustain support prior to overhaul
			Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_It_Sustain_Bidi.it", sequence, 4000, Xmp_Format.Default, Xmp_Interp.Nearest);

			// This module has a sample with a bidi sustain loop and a non-bidi
			// regular loop. It encounters an edge case, where sustain is released
			// when the position is negative but the loop hasn't been reflected
			// around its starting point yet
			Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_It_Sustain_Bidi2.it", sequence_Long, 4000, Xmp_Format.Default, Xmp_Interp.Nearest);

			// This module has a sample with a bidi sustain loop and an invalid
			// regular loop. The high frequency it plays at can trigger the mixer
			// loop's hang detection and leave its position at a negative value,
			// at which point sustain release can be used to cause trouble
			Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_It_Sustain_Bidi3.it", sequence_Long, 4000, Xmp_Format.Default, Xmp_Interp.Nearest);
		}
	}
}
