/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Player
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Player
	{
		/********************************************************************/
		/// <summary>
		/// When an invalid instrument is active, note + toneporta still
		/// updates the toneporta target. In this case, the default sample
		/// transpose of +0 is used when computing the target
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Invalid_Porta_Target()
		{
			Compare_Mixer_Data(dataDirectory, "Ft2_Invalid_Porta_Target.xm", "Ft2_Invalid_Porta_Target.data");
		}
	}
}
