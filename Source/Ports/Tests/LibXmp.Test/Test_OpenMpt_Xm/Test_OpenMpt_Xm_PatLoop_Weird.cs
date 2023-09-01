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
		/// This is similar to PatLoop-Break.xm. The voice should say "1 4 2"
		/// and then repeat "3 4 2" forever
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_Xm_PatLoop_Weird()
		{
			Compare_Mixer_Data_Loops(Path.Combine(dataDirectory, "OpenMpt", "Xm"), "PatLoop-Weird.xm", "PatLoop-Weird.data", 4);
		}
	}
}
