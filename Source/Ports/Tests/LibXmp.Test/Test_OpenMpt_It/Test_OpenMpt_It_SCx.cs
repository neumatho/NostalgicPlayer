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
		/// The SCx command cuts notes just like a normal note cut (^^^), it
		/// does not simply mute them. However, there is a difference when
		/// placing a lone instrument number after a note that was cut with
		/// SCx and one cut with ^^^, as it can be seen in this test case
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_SCx()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "SCx.it", "SCx.data");
		}
	}
}
