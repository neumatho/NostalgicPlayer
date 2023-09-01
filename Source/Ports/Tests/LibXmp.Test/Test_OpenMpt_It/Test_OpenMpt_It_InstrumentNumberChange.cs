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
		/// While Impulse Tracker cuts playing samples if it encounters an
		/// invalid sample number in sample mode, the same does not happen if
		/// we are in instrument mode
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_InstrumentNumberChange()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "InstrumentNumberChange.it", "InstrumentNumberChange.data");
		}
	}
}
