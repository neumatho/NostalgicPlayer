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
		/// Impulse Tracker executes the portamento and doesn't switch to
		/// the new sample on the second row when compatible Gxx is enabled
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_PortaSampleCompat()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "PortaSampleCompat.it", "PortaSampleCompat.data");
		}
	}
}
