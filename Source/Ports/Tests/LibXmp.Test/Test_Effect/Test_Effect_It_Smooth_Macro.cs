/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Effect
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Effect
	{
		/********************************************************************/
		/// <summary>
		/// Test that the OpenMPT IT extended effect \xx (Smooth MIDI Macro)
		/// works correctly. Also uses custom parametered macros and Zxx
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_It_Smooth_Macro()
		{
			Compare_Mixer_Data(dataDirectory, "It_Smooth_Macro.it", "It_Smooth_Macro.data");
		}
	}
}
