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
		/// 02 - Arpeggio with no value
		///
		/// If this test plays correctly, both notes will sound the same,
		/// bending downward smoothly. Incorrect (but perhaps acceptable,
		/// considering the unlikelihood of this combination of pitch bend
		/// and a meaningless arpeggio) handling of the arpeggio effect will
		/// result in a "stutter" on the second note, but the final pitch
		/// should be the same for both notes. Really broken players will
		/// mangle the pitch slide completely due to the arpeggio resetting
		/// the pitch on every third tick
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_02_Arpeggio_No_Value()
		{
			Compare_Mixer_Data(dataDirectory, "Storlek_02.it", "Storlek_02.data");
		}
	}
}
