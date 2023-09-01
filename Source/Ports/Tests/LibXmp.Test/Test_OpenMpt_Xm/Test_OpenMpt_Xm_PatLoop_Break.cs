/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_Xm
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_Xm
	{
		/********************************************************************/
		/// <summary>
		/// This is a nice test for E6x + Dxx behaviour. First make sure that
		/// E6x is played correctly by your player. A position jump should
		/// not clear the pattern loop memory (just like in Impulse Tracker).
		///
		/// Claudio's note: without looping, xmp plays this as 123-123-123-2
		/// instead of the expected 123-123-123-23. It stops before the final
		/// 3 because it detects that it already passed there and end of
		/// module is detected. If looped, it correctly plays 123-123-123-23
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_PatLoop_Break()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "PatLoop-Break.xm", "PatLoop-Break.data");
		}
	}
}
