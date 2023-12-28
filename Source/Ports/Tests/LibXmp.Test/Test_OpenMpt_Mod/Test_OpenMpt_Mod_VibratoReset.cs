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
		/// Like many other trackers, ProTracker does not advance the vibrato
		/// position on the first tick of the row. However, it also does not
		/// apply the vibrato offset on the first tick, which results in
		/// somewhat funky-sounding vibratos. OpenMPT uses this behaviour
		/// only in ProTracker 1/2 mode. The same applies to the tremolo
		/// effect
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Mod_VibratoReset()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Mod"), "VibratoReset.mod", "VibratoReset.data");
		}
	}
}
