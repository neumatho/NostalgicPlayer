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
		/// This input caused invalid double to integer conversions due to
		/// using bad loop values for invalid (unloaded) samples. These would
		/// result in negative integer frame sample counts and other oddities
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_Gdm_Bad_Loop()
		{
			Playback_Sequence[] sequence = new Playback_Sequence[]
			{
				new Playback_Sequence(Playback_Action.Play_Frames, 3, 0),
				new Playback_Sequence(Playback_Action.Play_End, 0, 0)
			};

			Util.Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_Gdm_Bad_Loop.gdm", sequence, 4000, 0, 0);
		}
	}
}
