/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Storlek
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Storlek
	{
		/********************************************************************/
		/// <summary>
		/// 24 - Short envelope loops
		///
		/// Envelope loops should include both of the loop points. Each
		/// instrument in this test should play differently: first with no
		/// envelope, then a stuttering effect, rapid left/right pan shift,
		/// and finally an arpeggio effect
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_24_Short_Envelope_Loops()
		{
			Compare_Mixer_Data(dataDirectory, "Storlek_24.it", "Storlek_24.data");
		}
	}
}
