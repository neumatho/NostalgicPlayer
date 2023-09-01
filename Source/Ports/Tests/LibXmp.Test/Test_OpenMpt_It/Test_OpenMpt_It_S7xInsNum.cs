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
		/// Changing the NNA action through the S7x command only affects the
		/// current note - The NNA action is reset on every note change, and
		/// not on every instrument change
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_OpenMpt_It_S7xInsNum()
		{
			Compare_Mixer_Data(Path.Combine(dataDirectory, "OpenMpt", "It"), "S7xInsNum.it", "S7xInsNum.data");
		}
	}
}
