/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Read_Mem
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Read_Mem
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Read_Mem_Hio_NoSize()
		{
			Hio h = Hio.Hio_Open_Mem(null, -1, false);
			Assert.IsNull(h, "hio_open");

			h = Hio.Hio_Open_Mem(null, 0, false);
			Assert.IsNull(h, "hio_open");
		}
	}
}
