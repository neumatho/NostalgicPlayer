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
		public void Test_Api_Channel_Mute()
		{
			Ports.LibXmp.LibXmp ctx = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			LoadModule(dataDirectory, "Ode2Ptk.s3m", ctx);

			// State check
			c_int ret = ctx.Xmp_Channel_Mute(Constants.Xmp_Max_Channels, 2);
			Assert.AreEqual(-(c_int)Xmp_Error.State, ret, "State check error");

			ctx.Xmp_Start_Player(8000, 0);

			// Invalid channel
			ret = ctx.Xmp_Channel_Mute(Constants.Xmp_Max_Channels, 2);
			Assert.AreEqual(-(c_int)Xmp_Error.Invalid, ret, "Invalid channel error");

			ret = ctx.Xmp_Channel_Mute(-1, 2);
			Assert.AreEqual(-(c_int)Xmp_Error.Invalid, ret, "Invalid channel error");

			for (c_int i = 0; i < Constants.Xmp_Max_Channels; i++)
			{
				// Query status
				ret = ctx.Xmp_Channel_Mute(i, -1);
				Assert.AreEqual(0, ret, "Mute status error");
			}

			for (c_int i = 0; i < Constants.Xmp_Max_Channels; i++)
			{
				if ((i & 1) != 0)
				{
					// Mute
					ret = ctx.Xmp_Channel_Mute(i, 1);
					Assert.AreEqual(0, ret, "Previous status error");

					// Query
					ret = ctx.Xmp_Channel_Mute(i, -1);
					Assert.AreEqual(1, ret, "Mute channel error");
				}

				if (i < (Constants.Xmp_Max_Channels / 2))
				{
					// Toggle
					ret = ctx.Xmp_Channel_Mute(i, 2);

					if ((i & 1) != 0)
					{
						Assert.AreEqual(1, ret, "Previous status error");

						// Query
						ret = ctx.Xmp_Channel_Mute(i, -1);
						Assert.AreEqual(0, ret, "Toggle channel error");
					}
					else
					{
						Assert.AreEqual(0, ret, "Previous status error");

						// Query
						ret = ctx.Xmp_Channel_Mute(i, -1);
						Assert.AreEqual(1, ret, "Toggle channel error");
					}
				}
				else
				{
					// Unmute
					ret = ctx.Xmp_Channel_Mute(i, 0);

					if ((i & 1) != 0)
						Assert.AreEqual(1, ret, "Previous status error");
					else
						Assert.AreEqual(0, ret, "Previous status error");

					// Query
					ret = ctx.Xmp_Channel_Mute(i, -1);
					Assert.AreEqual(0, ret, "Unmute channel error");
				}
			}

			ctx.Xmp_Release_Module();
			ctx.Xmp_Free_Context();
		}
	}
}
