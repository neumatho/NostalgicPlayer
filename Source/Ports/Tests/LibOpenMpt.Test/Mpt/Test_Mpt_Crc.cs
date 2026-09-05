/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpenMpt.Test.Mpt
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Mpt
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Mpt_Crc()
		{
			CPointer<c_byte> testStr = new CPointer<c_byte>(Encoding.Latin1.GetBytes("123456789"));

			Assert.AreEqual(0xcbf43926U, new Crc32(testStr.Begin(), testStr.End()).Result());
			Assert.AreEqual(0x89a1897fU, new Crc32_Ogg(testStr.Begin(), testStr.End()).Result());
		}
	}
}
