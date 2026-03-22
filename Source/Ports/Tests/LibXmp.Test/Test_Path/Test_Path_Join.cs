/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
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
		public void Test_Path_Join()
		{
			Path sp = Path.LibXmp_Path_Init();

			Test_Join(sp, "first", "second", "first/second");
			Test_Join(sp, "/first/second/", "/third/fourth/", "/first/second/third/fourth");
			Test_Join(sp, "//\\//\\//woah//", "\\//\\better//clean\\\\this\\/", "/woah/better/clean/this");

			Test_Join(sp, "first", "", "first");		// Unintended but works
			Test_Join(sp, "", "second", "/second");	// Unintended but works
			Test_Join(sp, "", "", "/");				// Unintended but works

			c_int ret = sp.LibXmp_Path_Join("a".ToCharPointer(), null);
			Assert.AreEqual(-1, ret, "Fail on NULL");

			ret = sp.LibXmp_Path_Join(null, "b".ToCharPointer());
			Assert.AreEqual(-1, ret, "Fail on NULL");

			ret = sp.LibXmp_Path_Join(null, null);
			Assert.AreEqual(-1, ret, "Fail on NULL");

			sp.LibXmp_Path_Free();
		}
	}
}
