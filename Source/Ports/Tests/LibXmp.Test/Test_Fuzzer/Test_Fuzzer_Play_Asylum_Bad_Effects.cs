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
		/// This ASYLUM module contains a bunch of invalid effects.
		/// libxmp was previously loading these without any conversion and
		/// could crash with a division by zero from ST 2.6 tempo effect
		/// misuse
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_Asylum_Bad_Effects()
		{
			Playback_Sequence[] sequence = new Playback_Sequence[]
			{
				new Playback_Sequence(Playback_Action.Play_Frames, 4, 0),
				new Playback_Sequence(Playback_Action.Play_End, 0, 0)
			};

			Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_Asylum_Bad_Effects.amf", sequence, 4000, Xmp_Format.Default, Xmp_Interp.Nearest);
		}
	}
}
