/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

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
		public void Test_Api_Restart_Module()
		{
			Ports.LibXmp.LibXmp ctx = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			c_int ret;

			LoadModule(dataDirectory, "Ode2Ptk.s3m", ctx);
			ctx.Xmp_Start_Player(8000, 0);

			for (c_int i = 0; i < 100; i++)
			{
				ret = ctx.Xmp_Play_Frame();
				Assert.AreEqual(0, ret, "Play frame error");
			}

			ctx.Xmp_Restart_Module();

			ret = ctx.Xmp_Play_Frame();
			Assert.AreEqual(0, ret, "Play frame error");

			ctx.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);
			Assert.AreEqual(0, info.Pos, "Module restart error");

			ctx.Xmp_Release_Module();
			ctx.Xmp_Free_Context();
		}
	}
}
