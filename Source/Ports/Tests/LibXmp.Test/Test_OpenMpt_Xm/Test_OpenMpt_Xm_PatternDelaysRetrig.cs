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
		/// Only the very first tick of a row should be considered as the
		/// “first tick”, even if the row is repeated multiple times using
		/// the pattern delay (EEx) command (i.e. multiples of the song speed
		/// should not be considered as the first tick). This is shown in
		/// this test by using the extra-fine portamento commands, which are
		/// only executed on the first tick
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_PatternDelaysRetrig()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "PatternDelaysRetrig.xm", "PatternDelaysRetrig.data");
		}
	}
}
