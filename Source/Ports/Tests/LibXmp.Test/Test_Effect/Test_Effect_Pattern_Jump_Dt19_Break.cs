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
		/// DTM pattern jump resets break row to 0.
		/// Note: prior to Digital Tracker 1.9, Digital Tracker completely
		/// ignores the parameter of the pattern break effect?
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Pattern_Jump_Dt19_Break()
		{
			Compare_Mixer_Data(dataDirectory, "Pattern_Jump_Dt19_Break.dtm", "Pattern_Jump_Dt19_Break.data");
		}
	}
}
