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
		public void Test_Path_Set()
		{
			Path sp = Path.LibXmp_Path_Init();

			Test_Set(sp, "test path", "test path");
			Test_Set(sp, "another_path", "another_path");
			Test_Set(sp, "i//luv//slashes//", "i/luv/slashes");
			Test_Set(sp, "//////lololol///////////", "/lololol");
			Test_Set(sp, "//////", "/");
			Test_Set(sp, "c:\\this\\too", "c:/this/too");
			Test_Set(sp, "z:\\\\lmao\\\\\\\\", "z:/lmao");

			c_int ret = sp.LibXmp_Path_Set(null);
			Assert.AreEqual(-1, ret, "Fail on NULL");

			sp.LibXmp_Path_Free();
			sp.LibXmp_Path_Free();	// Junk data but should be okay to free
		}
	}
}
