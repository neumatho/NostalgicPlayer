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
		/// This IT uses loop (SB2) and row delay (SEE) on the very first
		/// row of the module. This actually crashed libxmp (in ASan)!
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_It_Row_0_Loop_Row_Delay()
		{
			Playback_Sequence[] sequence = new Playback_Sequence[]
			{
				new Playback_Sequence(Playback_Action.Play_Frames, 8, 0),
				new Playback_Sequence(Playback_Action.Play_End, 0, 0)
			};

			Util.Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_It_Row_0_Loop_Row_Delay.it", sequence, 4000, 0, 0);
		}
	}
}
