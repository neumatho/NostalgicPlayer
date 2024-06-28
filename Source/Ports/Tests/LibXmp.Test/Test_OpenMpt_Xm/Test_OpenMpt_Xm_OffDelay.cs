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
		/// Note Delays combined with anything else are a lot of fun! Note
		/// Off combined with a Note Delay will cause envelopes to retrigger!
		/// And that is actually all it does if there is an envelope. No fade
		/// out, no nothing.
		/// 
		/// Claudio's note -- I didn't implement the envelope retrigger
		/// thing, but the test works nonetheless. Maybe I'm doing something
		/// wrong?
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_OffDelay()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "OffDelay.xm", "OffDelay.data");
		}
	}
}
