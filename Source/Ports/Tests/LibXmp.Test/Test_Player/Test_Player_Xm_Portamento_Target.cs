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
		/// Don't assume a default portamento target. Also check the OpenMPT
		/// Porta-Pickup.xm test. Real life case happens in "Unknown danger."
		/// sent by Georgy Lomsadze (UNDANGER.XM)
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Xm_Portamento_Target()
		{
			Compare_Mixer_Data(dataDirectory, "Xm_Portamento_Target.xm", "Xm_Portamento_Target.data");
		}
	}
}
