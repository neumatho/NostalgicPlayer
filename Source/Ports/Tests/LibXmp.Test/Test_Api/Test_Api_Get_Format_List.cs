/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Api
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Api
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Api_Get_Format_List()
		{
			string[] list = Ports.LibXmp.LibXmp.Xmp_Get_Format_List();
			Assert.IsNotNull(list, "Returned null");

			c_int i;
			for (i = 0; list[i] != null; i++)
				Assert.IsNotNull(list[i], "Empty format name");

			Assert.AreEqual(4, i, "Wrong number of formats");
		}
	}
}
