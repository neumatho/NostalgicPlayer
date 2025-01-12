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
		/// Description: Instrument swapping should happen instantly (not at
		/// the end of the sample or end of sample loop like in
		/// PTInstrSwap.mod) when there is an E9x retrigger command next to a
		/// lone instrument number. As with regular sample swapping, the
		/// sample finetune is not updated. The left and right channel of
		/// this module should sound identical.
		///
		/// Note that the retrigger command can cause semi-random tiny delays
		/// with ProTracker on a real Amiga, so if there are small
		/// differences in phase between the left and right channel when
		/// playing this test in ProTracker but not in an external player,
		/// this is acceptable
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Mod_InstrSwapRetrigger()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Mod"), "InstrSwapRetrigger.mod", "InstrSwapRetrigger.data");
		}
	}
}
