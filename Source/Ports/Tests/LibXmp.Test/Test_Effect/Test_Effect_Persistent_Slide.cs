/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		public void Test_Effect_Persistent_Slide()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			Create_Simple_Module(opaque, 2, 2);

			// Standard pitch bend
			New_Event(opaque, 0, 0, 0, 49, 1, 0, Effects.Fx_Per_Porta_Up, 2, 0, 0);

			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 80; i++)
			{
				c_int k = 856 - i * 10;

				for (c_int j = 0; j < 6; j++)
				{
					opaque.Xmp_Play_Frame();
					opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);
					Assert.AreEqual(k - j * 2, Period(info), "Slide up error");
				}
			}

			New_Event(opaque, 0, 0, 0, 84, 1, 0, Effects.Fx_Per_Porta_Dn, 2, 0, 0);

			opaque.Xmp_Restart_Module();

			for (c_int i = 0; i < 80; i++)
			{
				c_int k = 113 + i * 10;

				for (c_int j = 0; j < 6; j++)
				{
					opaque.Xmp_Play_Frame();
					opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);
					Assert.AreEqual(k + j * 2, Period(info), "Slide down error");
				}
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
