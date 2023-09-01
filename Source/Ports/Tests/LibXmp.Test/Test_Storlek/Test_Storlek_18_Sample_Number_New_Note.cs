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
		/// 18 - Sample number causes new note
		///
		/// A sample number encountered when no note is playing should
		/// restart the last sample played, and at the same pitch.
		/// Additionally, a note cut should clear the channel's state,
		/// thereby disabling this behavior.
		///
		/// This song should play six notes, with the last three an octave
		/// higher than the first three
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_18_Sample_Number_New_Note()
		{
			Compare_Mixer_Data(dataDirectory, "Storlek_18.it", "Storlek_18.data");
		}
	}
}
