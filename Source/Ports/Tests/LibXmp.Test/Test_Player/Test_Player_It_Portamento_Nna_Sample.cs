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
		/// Tone portamento should cancel the NNA for the current note even
		/// if the current note is a sample change (and thus won't actually
		/// perform the tone portamento)
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_It_Portamento_Nna_Sample()
		{
			Compare_Mixer_Data(dataDirectory, "Portamento_Nna_Sample.it", "Portamento_Nna_Sample.data");
		}
	}
}
