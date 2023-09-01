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
		/// A cutoff value of 0 should not be reset to full cutoff when
		/// triggering a note just because the filter envelope is enabled.
		/// This bug is probably very specific to OpenMPT, because it gets
		/// rid of some unneccessary code
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_FilterEnvReset()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "FilterEnvReset.it", "FilterEnvReset.data");
		}
	}
}
