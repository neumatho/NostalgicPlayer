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
		/// This test is just there to be sure that the filter-reset.it and
		/// filter-reset-carry.it test cases do not break NNA background
		/// channels
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_Filter_Nna()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "Filter-Nna.it", "Filter-Nna.data");
		}
	}
}
