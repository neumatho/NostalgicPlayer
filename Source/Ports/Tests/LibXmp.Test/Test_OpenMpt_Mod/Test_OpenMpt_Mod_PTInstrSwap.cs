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
		/// Description: When specifying an instrument number without a note,
		/// ProTracker 1/2 instantly applies the new instrument settings, but
		/// does not use the new sample until the end of the (loop of the)
		/// current sample is reached. In this example, sample 2 should be
		/// played at maximum volume as soon as instrument number 1 is
		/// encountered in the pattern, then sample 1 should be triggered
		/// somewhere around row 10 and then stop playing at around row 18
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Mod_PTInstrSwap()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Mod"), "PTInstrSwap.mod", "PTInstrSwap.data");
		}
	}
}
