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
		/// Description: ProTracker instrument swapping (see PTInstrSwap.mod
		/// test case) should also work when the "source" sample is not
		/// looped. However, when the "target" sample is not looped, sample
		/// playback should stop as with PTSwapEmpty.mod. Conceptually this
		/// can be explained because in this case, the sample loop goes from
		/// 0 to 2 in "oneshot" mode, i.e. it will loop a (hopefully) silent
		/// part of the sample
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Mod_PTSwapNoLoop()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Mod"), "PTSwapNoLoop.mod", "PTSwapNoLoop.data");
		}
	}
}
