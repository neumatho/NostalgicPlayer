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
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_It_High_Transpose()
		{
			Playback_Sequence[] sequence = new Playback_Sequence[]
			{
				new Playback_Sequence(Playback_Action.Play_Frames, 24, 0),
				new Playback_Sequence(Playback_Action.Play_End, 0, 0)
			};

			// This IT has a sample with the maximum possible transpose value
			// 9999999 (without hex editing). In linear frequency mode, this
			// could cause libxmp to calculate a note value of 241, which
			// caused a leftshift of a negative value in libxmp_period_to_bend.
			// This should play without causing any problems.
			Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_It_High_Transpose.it", sequence, 4000, Xmp_Format.Default, Xmp_Interp.Nearest);
		}
	}
}
