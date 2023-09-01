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
		/// Test the Modplug extended effects S9E/X9E Play Forward and
		/// S9F/X9F Play Reverse (particularly how S9F interacts with IT
		/// loops). Also make sure similar reverse effects for other tracker
		/// formats render roughly how they're supposed to
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Reverse()
		{
			Compare_Mixer_Data(dataDirectory, "Reverse_It.it", "Reverse_It.data");
			Compare_Mixer_Data(dataDirectory, "Reverse_Xm.xm", "Reverse_Xm.data");
		}
	}
}
