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
		/// Previously, libxmp would not attempt to scan later in a module if
		/// the first sequence fails to scan. This is fine for non-marker
		/// formats, but S3M and IT can begin with an end marker and contain
		/// valid data afterward. These modules are now supported
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_S3M_Marker_Sequence()
		{
			Playback_Sequence[] sequence = new Playback_Sequence[]
			{
				new Playback_Sequence(Playback_Action.Play_Frames, 1, -(c_int)Xmp_Error.End),
				new Playback_Sequence(Playback_Action.Play_Frames, 1, -(c_int)Xmp_Error.End),
				new Playback_Sequence(Playback_Action.Play_Set_Position, 1, 1),
				new Playback_Sequence(Playback_Action.Play_Frames, 4, 0),
				new Playback_Sequence(Playback_Action.Play_End, 0, 0)
			};

			Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_S3M_Marker_Sequence.s3m", sequence, 4000, Xmp_Format.Default, Xmp_Interp.Nearest);
		}
	}
}
