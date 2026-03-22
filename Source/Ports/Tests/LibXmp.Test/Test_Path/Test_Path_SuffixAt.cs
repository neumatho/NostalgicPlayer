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
		public void Test_Path_SuffixAt()
		{
			Path sp = Path.LibXmp_Path_Init();

			Test_Set(sp, "another/path.s3m", "another/path.s3m");
			Test_Suffix_At(sp, 12, ".mod", "another/path.mod");
			Test_Suffix_At(sp, 17, ".AS", null);
			Test_Suffix_At(sp, 16, ".nt", "another/path.mod.nt");
			Test_Suffix_At(sp, 10000, ".abc", null);
			Test_Suffix_At(sp, 16, ".AS", "another/path.mod.AS");
			Test_Suffix_At(sp, 12, ".mod", "another/path.mod");
			Test_Suffix_At(sp, 17, ".nt", null);
			Test_Suffix_At(sp, 8, "\\/\\//loool.it", "another/loool.it");

			c_int ret = sp.LibXmp_Path_Suffix_At(5, null);
			Assert.AreEqual(-1, ret, "Fail on NULL");

			sp.LibXmp_Path_Free();
		}
	}
}
