/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Path
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Path
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Path_Append()
		{
			Path sp = Path.LibXmp_Path_Init();

			Test_Append(sp, "hellow!11", "/hellow!11");
			Test_Truncate(sp, 0, "");
			Test_Append(sp, "/\\/\\/owo\\//\\//", "/owo");
			Test_Set(sp, "init", "init");
			Test_Append(sp, "more path", "init/more path");
			Test_Set(sp, "first/", "first");
			Test_Append(sp, "second", "first/second");
			Test_Append(sp, "third", "first/second/third");
			Test_Append(sp, "", "first/second/third");	// Unintended but works

			c_int ret = sp.LibXmp_Path_Append(null);
			Assert.AreEqual(-1, ret, "Fail on NULL");

			sp.LibXmp_Path_Free();
		}
	}
}
