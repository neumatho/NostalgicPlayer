/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_Mod
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_Mod
	{
		/********************************************************************/
		/// <summary>
		/// ProTracker 1/2 has a slightly buggy offset implementation which
		/// adds the offset in two different places (according to
		/// tracker_notes.txt coming with libxmp: "once before playing this
		/// instrument (as is expected), and once again after this instrument
		/// has been played"). The left and right channel of this module
		/// should sound identical. OpenMPT emulates this behaviour correctly
		/// in ProTracker mode
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Mod_PtOffset()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Mod"), "PtOffset.mod", "PtOffset.data");
		}
	}
}
