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
		/// Two samples played back at the same frequency should always step
		/// through their sample frames at the same rate, regardless of the
		/// lengths of their loops. Example: a sample with a loop of 16
		/// frames and a sample with a loop of 32 frames should repeat their
		/// loops at exactly a rate of 2:1 if played at the same frequency
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Sample_Sync()
		{
			Compare_Mixer_Data(dataDirectory, "Sample_Sync.it", "Sample_Sync.data");
		}
	}
}
