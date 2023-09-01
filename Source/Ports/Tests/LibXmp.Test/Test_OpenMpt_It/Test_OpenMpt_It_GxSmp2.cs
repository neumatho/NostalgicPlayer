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
		/// Going one step further by also changing the sample next to that
		/// portamento
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_GxSmp2()
		{
			Compare_Mixer_Data_No_Rv(Path.Combine(dataDirectory, "OpenMpt", "It"), "GxSmp2.it", "GxSmp2.data");
		}
	}
}
