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
		/// Imperium Galactica toneporta is effectively identical to
		/// Ultra Tracker toneporta, including double toneporta priority.
		/// The rate from the high FX takes priority over the rate from the
		/// low FX
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Xmf_Double_TonePorta()
		{
			Compare_Mixer_Data(dataDirectory, "Xmf_Double_TonePorta.xmf", "Xmf_Double_TonePorta.data");
		}
	}
}
