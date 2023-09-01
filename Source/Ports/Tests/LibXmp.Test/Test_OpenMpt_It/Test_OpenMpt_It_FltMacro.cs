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
		/// This test uses a custom macro configuration that uses the
		/// instrument volume to control the filter cutoff. A correctly
		/// implemented MIDI Macro system should pass this test
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_FltMacro()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "FltMacro.it", "FltMacro.data");
		}
	}
}
