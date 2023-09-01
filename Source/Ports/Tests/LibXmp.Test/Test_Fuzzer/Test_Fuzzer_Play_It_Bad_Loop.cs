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
		/// The sample in this module has junk loop values that get ignored
		/// by the sample loader because the sample is unloadable. This
		/// sample should be rejected by the player, too
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_It_Bad_Loop()
		{
			Playback_Sequence[] sequence = new Playback_Sequence[]
			{
				new Playback_Sequence(Playback_Action.Play_Frames, 10, 0),
				new Playback_Sequence(Playback_Action.Play_End, 0, 0)
			};

			Util.Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_It_Bad_Loop.it", sequence, 4000, 0, 0);
		}
	}
}
