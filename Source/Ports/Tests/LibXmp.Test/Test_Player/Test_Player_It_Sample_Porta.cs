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
		/// In sample mode, sample is retriggered even with tone portamento
		/// if the previous sample is finished. See UNATCOReturn_music.it
		/// sample 12 in pattern 62 (reported by Alexander Null)
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_It_Sample_Porta()
		{
			Compare_Mixer_Data(dataDirectory, "It_Sample_Porta.it", "It_Sample_Porta.data");
		}
	}
}
