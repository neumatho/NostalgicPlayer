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
		/// Octalyser pattern jump resets break row to 0
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Pattern_Jump_Octalyser_Break()
		{
			Compare_Mixer_Data(dataDirectory, "Pattern_Jump_Octalyser_Break.mod", "Pattern_Jump_Octalyser_Break.data");
		}
	}
}
