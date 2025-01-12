/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_Mod
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_Mod
	{
		/********************************************************************/
		/// <summary>
		/// The interpretation of this scenario highly differs between
		/// various replayers. If the sample number next to a portamento
		/// effect differs from the previous number, the new sample's default
		/// volume should be applied and
		///
		/// o the old sample should be played until reaching its end or loop
		///   end (ProTracker 1/2). If the sample loops, the new sample
		///   should be activated after the loop ended.
		///
		/// o the old sample should keep playing (various other players)
		///
		/// OpenMPT implements the second option in normal playback mode and
		/// the first option in ProTracker 1/2 mode
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Mod_PortaSmpChange()
		{
			// Test generic MOD player behavior
			Compare_Mixer_Data_Player_Mode(Path.Combine(dataDirectory, "OpenMpt", "Mod"), "PortaSmpChange.mod", "PortaSmpChange.data", Xmp_Mode.Mod);

			// Test ProTracker 1/2 behavior
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Mod"), "PortaSmpChange.mod", "PortaSmpChange_PT.data");
		}
	}
}
