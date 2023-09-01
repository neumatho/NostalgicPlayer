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
		/// This IT uses S74 Instrument Control/Set NNA Continue in sample
		/// mode, which could crash libxmp instead of it being ignored.
		/// This crash relied on a blank channel being available
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_It_Bad_Set_Nna()
		{
			Playback_Sequence[] sequence = new Playback_Sequence[]
			{
				new Playback_Sequence(Playback_Action.Play_Frames, 4, 0),
				new Playback_Sequence(Playback_Action.Play_End, 0, 0)
			};

			Util.Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_It_Bad_Set_Nna.it", sequence, 4000, 0, 0);
		}
	}
}
