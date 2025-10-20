/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
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
		public void Test_Api_Start_Player()
		{
			Ports.LibXmp.LibXmp ctx = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			c_int ret = ctx.Xmp_Start_Player(Constants.Xmp_Min_SRate, Xmp_Format.Default);
			Assert.AreEqual(-(c_int)Xmp_Error.State, ret, "State check error");

			Xmp_State state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Unloaded, state, "State error");

			LoadModule(dataDirectory, "Ode2Ptk.mod", ctx);

			state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Loaded, state, "State error");

			#pragma warning disable MSTEST0032
			Assert.AreEqual(4000, Constants.Xmp_Min_SRate, "Min sample rate value");
			Assert.AreEqual(49170, Constants.Xmp_Max_SRate, "Max sample rate value");
			#pragma warning restore MSTEST0032

			// Valid sampling rates
			ret = ctx.Xmp_Start_Player(Constants.Xmp_Min_SRate, Xmp_Format.Default);
			Assert.AreEqual(0, ret, "Min sample rate failed");

			state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Playing, state, "State error");

			ctx.Xmp_End_Player();

			state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Loaded, state, "State error");

			ret = ctx.Xmp_Start_Player(Constants.Xmp_Max_SRate, Xmp_Format.Default);
			Assert.AreEqual(0, ret, "Max sample rate failed");

			state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Playing, state, "State error");

			ctx.Xmp_End_Player();

			state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Loaded, state, "State error");

			// Invalid sampling rates
			ret = ctx.Xmp_Start_Player(Constants.Xmp_Min_SRate - 1, Xmp_Format.Default);
			Assert.AreEqual(-(c_int)Xmp_Error.Invalid, ret, "Min sample rate limit failed");

			state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Loaded, state, "State error");

			ctx.Xmp_End_Player();

			ret = ctx.Xmp_Start_Player(Constants.Xmp_Max_SRate + 1, Xmp_Format.Default);
			Assert.AreEqual(-(c_int)Xmp_Error.Invalid, ret, "Max sample rate limit failed");

			state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Loaded, state, "State error");

			ctx.Xmp_End_Player();

			state = (Xmp_State)ctx.Xmp_Get_Player(Xmp_Player.State);
			Assert.AreEqual(Xmp_State.Loaded, state, "State error");

			ctx.Xmp_Free_Context();
		}
	}
}
