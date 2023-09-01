/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_OpenMpt_It
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_OpenMpt_It
	{
		/********************************************************************/
		/// <summary>
		/// Another test case with an empty sample map slot which is simply
		/// ignored by Impulse Tracker
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_GxSmp()
		{
			Compare_Mixer_Data_No_Rv(Path.Combine(dataDirectory, "OpenMpt", "It"), "GxSmp.it", "GxSmp.data");
		}
	}
}
