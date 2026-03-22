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
		/// This seems to be related to EnvOff.xm. Sound output should never
		/// go completely silent between the notes.
		///
		/// Lachesis note: this claim seems to be dependent on tick-length
		/// ramping. Speed is 3, tick 0 of release is sustain, tick 1 is
		/// midway between points, tick 2 hits volume 0. libxmp output
		/// matches the *end* of the comparison sample ticks
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_Pickup()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "Pickup.xm", "Pickup.data");
		}
	}
}
