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
		/// 03 - Compatible Gxx off
		///
		/// Impulse Tracker links the effect memories for Exx, Fxx, and Gxx
		/// together if "Compatible Gxx" in NOT enabled in the file header.
		/// In other formats, portamento to note is entirely separate from
		/// pitch slide up/down. Several players that claim to be
		/// IT-compatible do not check this flag, and always store the last
		/// Gxx value separately.
		///
		/// When this test is played correctly, the first note will bend up,
		/// down, and back up again, and the final set of notes should only
		/// slide partway down. Players which do not correctly handle the
		/// Compatible Gxx flag will not perform the final pitch slide in the
		/// first part, or will "snap" the final notes
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_03_Compatible_Gxx_Off()
		{
			Compare_Mixer_Data(dataDirectory, "Storlek_03.it", "Storlek_03.data");
		}
	}
}
