/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Effect
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Effect
	{
		/********************************************************************/
		/// <summary>
		/// PTM pattern jump resets break row to 0.
		/// TODO: at speeds >=2, this occurs every tick, breaking Bxx Dxx
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Pattern_Jump_Ptm_Break()
		{
			Compare_Mixer_Data(dataDirectory, "Pattern_Jump_Ptm_Break.ptm", "Pattern_Jump_Ptm_Break.data");
		}
	}
}
