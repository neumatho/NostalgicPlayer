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
		/// Modplug Tracker uses shared toneporta memory like IT and updates
		/// the toneporta parameters every tick (GF G00 and G0 GFF are
		/// equivalent).
		/// TODO: there are numerous errors here due to libxmp incorrectly
		/// applying (wrong) Impulse Tracker semantics
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Mpt_It_Double_TonePorta()
		{
			Compare_Mixer_Data(dataDirectory, "Mpt_It_Double_TonePorta.it", "Mpt_It_Double_TonePorta.data");
		}
	}
}
