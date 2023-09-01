/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_Xm
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_Xm
	{
		/********************************************************************/
		/// <summary>
		/// Two tests in one: An offset effect that points beyond the sample
		/// end should stop playback on this channel. The note must not be
		/// picked up by further portamento effects.
		///
		/// Skale Tracker doesn't emulate this FT2 quirk. This test changes
		/// the tracker ID to something not recognized as FT2-compatible.
		/// Armada Tanks game music doesn't play correctly with this quirk
		/// enabled
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_3xx_No_Old_Samp_Noft()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "3xx-No-Old-Samp-Noft.xm", "3xx-No-Old-Samp-Noft.data");
		}
	}
}
