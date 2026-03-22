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
		/// Verify IMF default panning is actually applied in the player.
		/// TODO: ins# without note only temporarily sets panning, libxmp
		/// understandably gets this wrong
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Imf_Sample_Default_Pan()
		{
			Compare_Mixer_Data(dataDirectory, "Imf_Smpl_SetPan.imf", "Imf_Smpl_SetPan.data");
		}
	}
}
