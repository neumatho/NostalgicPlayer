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
		/// Changing the player mode rescans the module, and previously,
		/// changing a non-S3M/IT module beginning with FFh to S3M/IT would
		/// cause various issues resulting in an infinite loop trying to
		/// locate the next order
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_Mod_Bad_Marker_Sequence()
		{
			Playback_Sequence[] sequence = new Playback_Sequence[]
			{
				new Playback_Sequence(Playback_Action.Play_Set_Player_Mode, (c_int)Xmp_Mode.S3M, 0),
				new Playback_Sequence(Playback_Action.Play_Frames, 1, -(c_int)Xmp_Error.End),
				new Playback_Sequence(Playback_Action.Play_Frames, 1, -(c_int)Xmp_Error.End),
				new Playback_Sequence(Playback_Action.Play_Set_Position, 2, 2),
				new Playback_Sequence(Playback_Action.Play_Frames, 4, 0),
				new Playback_Sequence(Playback_Action.Play_End, 0, 0)
			};

			Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_Mod_Bad_Marker_Sequence.mod", sequence, 4000, Xmp_Format.Default, Xmp_Interp.Nearest);
		}
	}
}
