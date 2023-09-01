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
		/// When a note with a new instrument is played and the volume
		/// envelope is in fadeout, the volume envelope should reset.
		/// If the pan/pitch envelopes do NOT have carry set, they should
		/// be reset independently of this check
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_It_Fade_Env_Reset()
		{
			Compare_Mixer_Data(dataDirectory, "It_Fade_Env_Reset.it", "It_Fade_Env_Reset.data");
		}
	}
}
