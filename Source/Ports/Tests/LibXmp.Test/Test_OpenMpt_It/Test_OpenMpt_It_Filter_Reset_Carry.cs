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
		/// I think this is also just an extension to the previous test case,
		/// to make sure that it does not break anything else
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_Filter_Reset_Carry()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "Filter-Reset-Carry.it", "Filter-Reset-Carry.data");
		}
	}
}
