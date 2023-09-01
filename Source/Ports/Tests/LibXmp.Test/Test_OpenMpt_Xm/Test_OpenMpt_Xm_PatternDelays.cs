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
		/// If there are multiple pattern delays (EEx), only the one on the
		/// rightmost channel is considered (even if the EEx parameter is 0).
		/// Even ModPlug Tracker 1.16 passes this. The second pattern is not
		/// very important, it only tests the command X ModPlug Tracker
		/// extension, which adds fine pattern delays (like in the IT format)
		/// to XM files
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_PatternDelays()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "PatternDelays.xm", "PatternDelays.data");
		}
	}
}
