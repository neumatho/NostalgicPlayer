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
		/// Real Tracker 2 supports loading an XM with 128 instruments (and
		/// possibly more natively) but eventually starts failing to allocate
		/// memory for samples. 82x16 is the maximum I could save. libxmp
		/// should not crash or leak samples
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_Rtm_82Ins_X_16Smp()
		{
			Playback_Sequence[] sequence = new Playback_Sequence[]
			{
				new Playback_Sequence(Playback_Action.Play_Frames, 2, 0),
				new Playback_Sequence(Playback_Action.Play_End, 0, 0)
			};

			Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_Rtm_82Ins_X_16Smp.rtm", sequence, 4000, Xmp_Format.Default, Xmp_Interp.Nearest);
		}
	}
}
