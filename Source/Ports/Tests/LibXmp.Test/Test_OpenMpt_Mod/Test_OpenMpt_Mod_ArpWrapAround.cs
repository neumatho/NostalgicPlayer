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
		/// If an arpeggio parameter exceeds the Amiga frequency range,
		/// ProTracker wraps it around. In fact, the first note that is too
		/// high (which would be C-7 in OpenMPT or C-4 in ProTracker) becomes
		/// inaudible, C#7 / C#4 becomes C-4 / C-1, and so on. OpenMPT won't
		/// play the first case correctly, but that's rather complicated to
		/// emulate and probably not all that important
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Mod_ArpWrapAround()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Mod"), "ArpWrapAround.mod", "ArpWrapAround.data");
		}
	}
}
