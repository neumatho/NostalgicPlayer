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
		/// This FAR underflows the tempo in old tempo mode, one of the
		/// several places in far_extras.c that used to potentially left
		/// shift a negative tempo.
		///
		/// This needs to play a lot of frames (5 rows * 32 native frames per
		/// row) for futureproofing, though libxmp only executes 16 frames
		/// per row currently
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_Far_Old_Tempo_Mode_Underflow()
		{
			Playback_Sequence[] sequence = new Playback_Sequence[]
			{
				new Playback_Sequence(Playback_Action.Play_Frames, 5 * 32, 0),
				new Playback_Sequence(Playback_Action.Play_End, 0, 0)
			};

			Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_Far_Old_Tempo_Mode_Underflow.far", sequence, 4000, Xmp_Format.Default, Xmp_Interp.Nearest);
		}
	}
}
