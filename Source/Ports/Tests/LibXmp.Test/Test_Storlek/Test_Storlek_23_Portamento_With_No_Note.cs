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
		/// 23 - Portamento with no note
		///
		/// Stray portamento effects with no target note should do nothing.
		/// Relatedly, a portamento should clear the target note when it is
		/// reached.
		///
		/// The first section of this test should first play the same
		/// increasing tone three times, with the last GFF effect not
		/// resetting the note to the base frequency; the next part should
		/// play two rising tones at different pitches, and finish an octave
		/// lower than it started
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_23_Portamento_With_No_Note()
		{
			Compare_Mixer_Data(dataDirectory, "Storlek_23.it", "Storlek_23.data");
		}
	}
}
