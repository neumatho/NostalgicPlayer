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
		public void Test_Path_Move()
		{
			Path sp = Path.LibXmp_Path_Init();
			Path sp2 = Path.LibXmp_Path_Init();

			Test_Set(sp, "path a", "path a");
			Test_Set(sp2, "path b", "path b");
			Test_Move(sp, sp2, "path b");

			sp.LibXmp_Path_Free();
			sp2.LibXmp_Path_Free();

			sp = Path.LibXmp_Path_Init();
			sp2 = Path.LibXmp_Path_Init();

			Test_Set(sp, "path a", "path a");
			Test_Move(sp, sp2, null);
		}
	}
}
