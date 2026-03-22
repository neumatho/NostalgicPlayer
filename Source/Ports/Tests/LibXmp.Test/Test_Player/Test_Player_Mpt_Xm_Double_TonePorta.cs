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
		/// MadTracker 2 uses shared toneporta memory like FT2 and updates
		/// the toneporta parameters every tick AFTER the start tick. This
		/// causes something like M0 315 to use prior memory for M0 on the
		/// first toneporta update and then use 15h for M0 on every other
		/// update that row.
		/// TODO: rows 1C/1D don't play correctly because libxmp doesn't
		/// support the above broken behavior
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Mpt_Xm_Double_TonePorta()
		{
			Compare_Mixer_Data(dataDirectory, "Mpt_Xm_Double_TonePorta.xm", "Mpt_Xm_Double_TonePorta.data");
		}
	}
}
