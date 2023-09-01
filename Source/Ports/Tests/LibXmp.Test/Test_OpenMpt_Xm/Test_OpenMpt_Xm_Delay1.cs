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
		/// A fun test for rogue note delays that outputs a nice drum beat
		/// if played correctly. Combinations of note delays with and without
		/// instrument number are tested
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_Delay1()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "Delay1.xm", "Delay1.data");
		}
	}
}
