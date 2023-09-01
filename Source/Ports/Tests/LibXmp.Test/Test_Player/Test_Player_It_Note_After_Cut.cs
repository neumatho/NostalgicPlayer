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
		/// Note data can be recovered after a cut, such as in ub-name.it
		/// after the cut (e.g. with pitch slide effect)
		/// 
		///      F#7 07 .. GF1
		///      ^^^ .. .. .00
		///      F-7 07 .. GF1
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_It_Note_After_Cut()
		{
			Compare_Mixer_Data(dataDirectory, "Note_After_Cut.it", "Note_After_Cut.data");
		}
	}
}
