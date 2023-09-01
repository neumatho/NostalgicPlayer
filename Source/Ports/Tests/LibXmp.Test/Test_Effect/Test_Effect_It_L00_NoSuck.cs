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
		/// Tone portamento speed should be set even if there's no target
		/// note
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_It_L00_NoSuck()
		{
			Compare_Mixer_Data(dataDirectory, "L00_NoSuck.it", "L00_NoSuck.data");
		}
	}
}
