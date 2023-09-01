/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_String
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_String
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_String_Adjustment()
		{
			char[] str = { 'h', 'e', 'l', 'l', 'o', (char)1, (char)2, (char)30, (char)31, (char)127,
				(char)128, 'w', 'o', 'r', 'l', 'd', ' ', ' ', ' ', (char)0 };
			string _string = new string(str);

			Load_Helpers.LibXmp_Adjust_String(ref _string);
			Assert.AreEqual("hello    \u007f\u0080world", _string, "Adjustment error");
		}
	}
}
