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
		public void Test_Path_Truncate()
		{
			Path sp = Path.LibXmp_Path_Init();

			Test_Set(sp, "a/test/path", "a/test/path");
			Test_Truncate(sp, 10000, "a/test/path");
			Test_Truncate(sp, 8, "a/test/p");
			Test_Truncate(sp, 7, "a/test");
			Test_Truncate(sp, 6, "a/test");
			Test_Truncate(sp, 2, "a");
			Test_Truncate(sp, 1, "a");
			Test_Truncate(sp, 0, "");

			sp.LibXmp_Path_Free();
		}
	}
}
