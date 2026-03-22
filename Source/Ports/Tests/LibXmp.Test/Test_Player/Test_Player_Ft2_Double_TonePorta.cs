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
		/// Combining Mx+3xx/5xy results in both effects advancing toneporta.
		/// Fasttracker 2 has a bug, however, that the 3xx in this case will
		/// use the rate set by Mx. In other words, Mx+3xx effectively
		/// applies double whatever the Mx rate is (including from memory)
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Ft2_Double_TonePorta()
		{
			Compare_Mixer_Data(dataDirectory, "Ft2_Double_TonePorta.xm", "Ft2_Double_TonePorta.data");
		}
	}
}
