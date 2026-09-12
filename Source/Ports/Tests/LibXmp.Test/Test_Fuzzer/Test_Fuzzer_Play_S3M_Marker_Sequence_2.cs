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
		/// A side effect of rescanning modules when the player mode changes
		/// is that switching between a mode that does/doesn't support
		/// markers to one that doesn't/does can change the number of
		/// sequences. If the current sequence happened to be one that no
		/// longer exists after the rescan, out-of-bounds reads could occur,
		/// so now libxmp_scan_sequences fixes p->sequence
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_S3M_Marker_Sequence_2()
		{
			Playback_Sequence[] sequence = new Playback_Sequence[]
			{
				new Playback_Sequence(Playback_Action.Play_Frames, 1, 0),
				new Playback_Sequence(Playback_Action.Play_Set_Position, 2, 2),	// Sequence 1
				new Playback_Sequence(Playback_Action.Play_Frames, 1, 0),
				new Playback_Sequence(Playback_Action.Play_Set_Position, 4, 4),	// Sequence 2
				new Playback_Sequence(Playback_Action.Play_Frames, 1, 0),
				new Playback_Sequence(Playback_Action.Play_Set_Position, 6, 6),	// Sequence 3
				new Playback_Sequence(Playback_Action.Play_Frames, 1, 0),
				new Playback_Sequence(Playback_Action.Play_Set_Player_Mode, (c_int)Xmp_Mode.Mod, 0),	// Rescan
				new Playback_Sequence(Playback_Action.Play_Frames, 4, 0),			// Should be sequence 0 now
				new Playback_Sequence(Playback_Action.Play_End, 0, 0)
			};

			Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_S3M_Marker_Sequence_2.s3m", sequence, 4000, Xmp_Format.Default, Xmp_Interp.Nearest);
		}
	}
}
