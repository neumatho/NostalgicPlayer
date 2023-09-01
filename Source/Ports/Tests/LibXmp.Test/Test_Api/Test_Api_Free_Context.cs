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
		public void Test_Api_Free_Context()
		{
			Ports.LibXmp.LibXmp ctx = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Assert.IsNotNull(ctx, "Returned NULL");

			Xmp_State state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Unloaded, state, "State error");

			// Load module
			c_int ret = LoadModule(dataDirectory, "Test.xm", ctx);
			Assert.AreEqual(0, ret, "Load file");

			// Start playing
			ret = ctx.Xmp_Start_Player(44100, Xmp_Format.Default);
			Assert.AreEqual(0, ret, "Min sample rate failed");

			state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Playing, state, "State error");

			// Free context while playing
			ctx.Xmp_Free_Context();
		}
	}
}
