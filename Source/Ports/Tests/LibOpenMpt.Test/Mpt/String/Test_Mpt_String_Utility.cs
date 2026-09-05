/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpenMpt.Test.Mpt.String
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Mpt_String
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Mpt_String_Utility()
		{
			Assert.AreEqual(string.Empty, MptString.Trim_Left(" "));
			Assert.AreEqual(string.Empty, MptString.Trim_Right(" "));
			Assert.AreEqual(string.Empty, MptString.Trim(" "));

			// Weird things with std::string containing \0 in the middle and trimming \0
			Assert.AreEqual(6U, new StdString(Encoding.Latin1.GetBytes("\0\ta\0b "), 6).length());
			Assert.AreEqual("\0\ta\0b", MptString.Trim_Right(new StdString(Encoding.Latin1.GetBytes("\0\ta\0b "), 6)));
			Assert.AreEqual("\ta\0b", MptString.Trim(new StdString(Encoding.Latin1.GetBytes("\0\ta\0b\0"), 6), new StdString(new uint8[] { 0 }, 1)));
		}
	}
}
