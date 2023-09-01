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
		/// A test focusing on finding the correct sample playback position
		/// when switching samples “on the fly” (using instrument numbers
		/// without notes next to them). The module should remain silent when
		/// being played.
		/// 
		/// Obs: libxmp doesn't keep them silent due to minute phase
		/// differences, but it's switching instruments correctly so we'll
		/// consider it ok
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_SwapTest()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "SwapTest.it", "SwapTest.data");
		}
	}
}
