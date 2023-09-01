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
		/// When "Compatible Gxx" is enabled, the key-off flag should also
		/// be removed when continuing a note using a portamento command
		/// (row 2, 4, 6). This test case was written to discover a code
		/// regression when fixing Off-Porta.it)
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_Off_Porta_CompatGxx()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "Off-Porta-CompatGxx.it", "Off-Porta-CompatGxx.data");
		}
	}
}
