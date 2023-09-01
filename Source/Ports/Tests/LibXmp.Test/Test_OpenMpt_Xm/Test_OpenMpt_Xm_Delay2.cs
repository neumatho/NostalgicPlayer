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
		/// Rogue note delay test. It seems that internally, Fasttracker 2
		/// always acts like the last played note is next to a note delay
		/// (EDx with x > 0) if there is no note. Doing exactly this is
		/// probably the easiest way to pass this test. This also explains
		/// Fasttracker 2’s behaviour if there is an instrument number next
		/// to such a rogue note delay, which is shown in this test. Both
		/// channels should play exactly the same combination of snare and
		/// bass sounds
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_Delay2()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "Delay2.xm", "Delay2.data");
		}
	}
}
