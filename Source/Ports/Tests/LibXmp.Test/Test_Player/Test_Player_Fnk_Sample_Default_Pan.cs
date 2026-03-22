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
		/// Verify FunkTracker default panning is actually applied in the
		/// player.
		/// TODO: libxmp currently gets the "RLO" event wrong
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Fnk_Sample_Default_Pan()
		{
			Compare_Mixer_Data(dataDirectory, "Fnk_Smpl_SetPan.fnk", "Fnk_Smpl_SetPan.data");
		}
	}
}
