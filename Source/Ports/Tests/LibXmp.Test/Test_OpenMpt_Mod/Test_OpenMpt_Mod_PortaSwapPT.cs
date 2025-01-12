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
		/// Description: Related to PortaSmpChange.mod, this tests the
		/// portamento with sample swap behaviour for ProTracker-compatible
		/// MODs. Noteworthy tested behaviours:
		/// 
		/// * When doing a sample swap without portamento, the new sample
		///   keeps using the old sample’s finetune.
		/// * When doing a sample swap with portamento, the new sample’s
		///   finetune is instantly applied, and the new sample is started as
		///   soon as the old sample’s loop is finished playing
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Mod_PortaSwapPT()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Mod"), "PortaSwapPT.mod", "PortaSwapPT.data");
		}
	}
}
