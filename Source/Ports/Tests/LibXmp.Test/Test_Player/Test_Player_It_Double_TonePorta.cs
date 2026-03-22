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
		/// Combining Gx+Gxx/Lxy results in both effects advancing toneporta.
		/// IT has the same bug as FT2 where the shared toneporta memory is
		/// reused by both effects, but unlike FT2, it will correctly update
		/// memory from both parameters on the start tick.
		/// TODO: libxmp gets rows 00-0F wrong because it does not reload
		/// the toneporta memory rate for each effect every tick
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_It_Double_TonePorta()
		{
			Compare_Mixer_Data(dataDirectory, "It_Double_TonePorta.it", "It_Double_TonePorta.data");
		}
	}
}
