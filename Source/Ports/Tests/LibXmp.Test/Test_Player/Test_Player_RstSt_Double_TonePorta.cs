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
		/// rst SoundTracker uses shared toneporta memory like FT2 and
		/// updates the toneporta parameters every tick (MF 300 and M0 3F0
		/// are equivalent).
		/// TODO: rows 1C/1D don't play correctly because libxmp doesn't
		/// support processing toneporta parameters every tick
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_RstSt_Double_TonePorta()
		{
			Compare_Mixer_Data(dataDirectory, "RstSt_Double_TonePorta.xm", "RstSt_Double_TonePorta.data");
		}
	}
}
