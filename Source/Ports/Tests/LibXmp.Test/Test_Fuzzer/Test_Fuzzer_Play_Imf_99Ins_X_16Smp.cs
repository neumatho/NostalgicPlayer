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
		/// Maximum number of instruments supported by Orpheus with the
		/// maximum number of samples per instrument. libxmp should not crash
		/// or leak samples.
		///
		/// The first module is the result of loading and resaving the XM
		/// equivalent of this module with Orpheus (Orpheus appears to have a
		/// hard limit of 200 samples but will sometimes emit uninitialized
		/// samples with nul magic). The second module is a hexedited
		/// "theoretical" version of what actually should have been produced
		/// by this conversion, which reproduces a crash in affected versions
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_Imf_99Ins_X_16Smp()
		{
			Playback_Sequence[] sequence = new Playback_Sequence[]
			{
				new Playback_Sequence(Playback_Action.Play_Frames, 2, 0),
				new Playback_Sequence(Playback_Action.Play_End, 0, 0)
			};

			Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_Imf_99Ins_X_16Smp.imf", sequence, 4000, Xmp_Format.Default, Xmp_Interp.Nearest);
			Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_Imf_99Ins_X_16Smp_HexEdit.imf", sequence, 4000, Xmp_Format.Default, Xmp_Interp.Nearest);
		}
	}
}
