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
		/// The S77 / S79 / S7B commands pause the instrument envelopes, they
		/// do not turn them off. S78 / S7A / S7C should resume the envelope
		/// at exactly the position where it was paused. In this test, it is
		/// again very important that the envelope position is incremented
		/// before the point is evaluated, not afterwards (see EnvLoops.it)
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_S77()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "S77.it", "S77.data");
		}
	}
}
