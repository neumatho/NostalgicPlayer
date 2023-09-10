/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Effect
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Effect
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Persistent_VSlide()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			Create_Simple_Module(opaque, 2, 2);

			// Go up all the way
			New_Event(opaque, 0, 0, 0, 49, 1, 1, Effects.Fx_Per_VSld_Up, 1, 0, 0);

			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 80; i++)
			{
				c_int k = i * 5;
				Common.Clamp(ref k, 0, 64);

				for (c_int j = 0; j < 6; j++)
				{
					opaque.Xmp_Play_Frame();
					opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

					if ((k + j) > 64)
						Assert.AreEqual(64, info.Channel_Info[0].Volume, "Volume slide up error");
					else
						Assert.AreEqual(k + j, info.Channel_Info[0].Volume, "Volume slide up error");
				}
			}

			// Go down all the way
			New_Event(opaque, 0, 0, 0, 84, 1, 65, Effects.Fx_Per_VSld_Dn, 1, 0, 0);

			opaque.Xmp_Restart_Module();

			for (c_int i = 0; i < 80; i++)
			{
				c_int k = 64 - i * 5;
				Common.Clamp(ref k, 0, 64);

				for (c_int j = 0; j < 6; j++)
				{
					opaque.Xmp_Play_Frame();
					opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

					if ((k - j) < 0)
						Assert.AreEqual(0, info.Channel_Info[0].Volume, "Volume slide down error");
					else
						Assert.AreEqual(k - j, info.Channel_Info[0].Volume, "Volume slide down error");
				}
			}

			// Go up just a little
			New_Event(opaque, 0, 0, 0, 49, 1, 1, Effects.Fx_Per_VSld_Up, 1, 0, 0);
			New_Event(opaque, 0, 2, 0, 0, 0, 0, Effects.Fx_Per_VSld_Up, 0, 0, 0);

			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 80; i++)
			{
				c_int k = i * 5;
				Common.Clamp(ref k, 0, 10);

				for (c_int j = 0; j < 6; j++)
				{
					opaque.Xmp_Play_Frame();
					opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

					if ((k + j) > 10)
						Assert.AreEqual(10, info.Channel_Info[0].Volume, "Volume slide up error");
					else
						Assert.AreEqual(k + j, info.Channel_Info[0].Volume, "Volume slide up error");
				}
			}

			// Go down just a little
			New_Event(opaque, 0, 0, 0, 84, 1, 65, Effects.Fx_Per_VSld_Dn, 1, 0, 0);
			New_Event(opaque, 0, 2, 0, 0, 0, 0, Effects.Fx_Per_VSld_Dn, 0, 0, 0);

			opaque.Xmp_Restart_Module();

			for (c_int i = 0; i < 80; i++)
			{
				c_int k = 64 - i * 5;
				Common.Clamp(ref k, 54, 64);

				for (c_int j = 0; j < 6; j++)
				{
					opaque.Xmp_Play_Frame();
					opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

					if ((k - j) < 54)
						Assert.AreEqual(54, info.Channel_Info[0].Volume, "Volume slide down error");
					else
						Assert.AreEqual(k - j, info.Channel_Info[0].Volume, "Volume slide down error");
				}
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
