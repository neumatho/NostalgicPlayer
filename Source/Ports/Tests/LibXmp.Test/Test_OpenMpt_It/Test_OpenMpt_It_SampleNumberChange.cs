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
		/// Contrary to InstrumentNumberChange.it, even ModPlug Tracker 1.16
		/// passes it
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_SampleNumberChange()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "SampleNumberChange.it", "SampleNumberChange.data");
		}
	}
}
