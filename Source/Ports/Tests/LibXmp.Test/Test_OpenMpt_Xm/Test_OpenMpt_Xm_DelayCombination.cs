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
		/// Naturally, Fasttracker 2 ignores notes next to an out-of-range
		/// note delay. However, to check whether the delay is out of range,
		/// it is simply compared against the current song speed, not taking
		/// any pattern delays into account. No notes should be triggered in
		/// this test case, even though the second row is technically longer
		/// than six ticks
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_DelayCombination()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "DelayCombination.xm", "DelayCombination.data");
		}
	}
}
