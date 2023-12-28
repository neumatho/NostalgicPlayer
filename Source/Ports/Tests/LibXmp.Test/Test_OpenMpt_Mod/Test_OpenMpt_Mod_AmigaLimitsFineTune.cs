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
		/// ProTracker does not always clamp samples to the exact same range
		/// of periods it rather depends on the actual finetune value of the
		/// sample. In contrast to that, ScreamTracker 3 always clamps
		/// periods to the same range in its Amiga mode. This test file
		/// should stay completely in tune at all times
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Mod_AmigaLimitsFineTune()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Mod"), "AmigaLimitsFineTune.mod", "AmigaLimitsFineTune.data");
		}
	}
}
