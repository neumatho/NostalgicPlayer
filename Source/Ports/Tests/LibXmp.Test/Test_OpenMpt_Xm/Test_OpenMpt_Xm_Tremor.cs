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
		/// The tremor counter is not updated on the first tick, and the
		/// counter is only ever reset after a phase switch (from on to off
		/// or vice versa), so the best technique to implement tremor is to
		/// count down to zero and memorize the current phase (on / off), and
		/// when you reach zero, switch to the other phase by reading the
		/// current tremor parameter. Keep in mind that T00 recalls the
		/// tremor effect memory, but the phase length is always incremented
		/// by one, i.e. T12 means “on for two ticks, off for three”
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_Tremor()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "Tremor.xm", "Tremor.data");
		}
	}
}
