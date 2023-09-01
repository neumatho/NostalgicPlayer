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
		/// Tests each volume slide value for multi retrigger (Qxy) and also
		/// changing both the retrigger rate and volume slide rate for a
		/// playing note
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_It_Multi_Retrig()
		{
			Compare_Mixer_Data(dataDirectory, "It_Multi_Retrigger.it", "It_Multi_Retrigger.data");
		}
	}
}
