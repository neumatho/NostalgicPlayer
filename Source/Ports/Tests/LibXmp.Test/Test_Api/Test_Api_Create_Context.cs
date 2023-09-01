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
		public void Test_Api_Create_Context()
		{
			Ports.LibXmp.LibXmp ctx = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Assert.IsNotNull(ctx);

			Xmp_State state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Unloaded, state, "State error");

			ctx.Xmp_Free_Context();
		}
	}
}
