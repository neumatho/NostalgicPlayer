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
		public void Test_Api_Channel_Vol()
		{
			Ports.LibXmp.LibXmp ctx = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			LoadModule(dataDirectory, "Ode2Ptk.s3m", ctx);

			// State check
			c_int ret = ctx.Xmp_Channel_Vol(Constants.Xmp_Max_Channels, 2);
			Assert.AreEqual(-(c_int)Xmp_Error.State, ret, "State check error");

			ctx.Xmp_Start_Player(8000, 0);

			// Invalid channel
			ret = ctx.Xmp_Channel_Vol(Constants.Xmp_Max_Channels, 2);
			Assert.AreEqual(-(c_int)Xmp_Error.Invalid, ret, "Invalid channel error");

			ret = ctx.Xmp_Channel_Vol(-1, 2);
			Assert.AreEqual(-(c_int)Xmp_Error.Invalid, ret, "Invalid channel error");

			for (c_int i = 0; i < Constants.Xmp_Max_Channels; i++)
			{
				// Query status
				ret = ctx.Xmp_Channel_Vol(i, -1);
				Assert.AreEqual(100, ret, "Volume error");
			}

			for (c_int i = 0; i < Constants.Xmp_Max_Channels; i++)
			{
				// Set
				ret = ctx.Xmp_Channel_Vol(i, i);
				Assert.AreEqual(100, ret, "Previous vol error");

				// Query
				ret = ctx.Xmp_Channel_Vol(i, -1);
				Assert.AreEqual(i, ret, "Channel vol error");
			}

			ctx.Xmp_Release_Module();
			ctx.Xmp_Free_Context();
		}
	}
}
