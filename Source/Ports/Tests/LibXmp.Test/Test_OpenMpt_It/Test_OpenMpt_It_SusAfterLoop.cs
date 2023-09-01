/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_It
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_It
	{
		/********************************************************************/
		/// <summary>
		/// When a sample sustain loop, which is placed partly or completely
		/// behind a “normal” sample loop is exited (through a note-off
		/// event), and the current sample playback position is past the
		/// normal loop’s end, it is adjusted to current position - loop end
		/// + loop start.
		/// 
		/// Non-advertized secondary breakage here: sample reverse needs to
		/// be canceled when exiting a bidirectional sustain loop into a
		/// regular loop that isn't bidirectional
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_SusAfterLoop()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "SusAfterLoop.it", "SusAfterLoop.data");
		}
	}
}
