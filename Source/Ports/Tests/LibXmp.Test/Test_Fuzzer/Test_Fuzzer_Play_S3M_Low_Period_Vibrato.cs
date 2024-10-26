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
		/// This S3M produces a very low note periods that could underflow
		/// libxmp's period calculation below 0 in some cases, causing
		/// invalid float to integer conversions
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_S3M_Low_Period_Vibrato()
		{
			Playback_Sequence[] sequence = new Playback_Sequence[]
			{
				new Playback_Sequence(Playback_Action.Play_Frames, 8, 0),
				new Playback_Sequence(Playback_Action.Play_End, 0, 0)
			};

			Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_S3M_Low_Period_Vibrato.s3m", sequence, 4000, Xmp_Format.Default, Xmp_Interp.Nearest);
		}
	}
}
