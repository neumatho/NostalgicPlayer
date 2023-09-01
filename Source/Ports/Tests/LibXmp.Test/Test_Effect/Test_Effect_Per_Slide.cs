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
		public void Test_Effect_Per_Slide()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Frame_Info info;

			Create_Simple_Module(opaque, 2, 2);

			// Persistent portamento up
			New_Event(opaque, 0, 0, 0, 49, 1, 0, Effects.Fx_Per_Porta_Up, 2, 0, 0);
			New_Event(opaque, 0, 60, 0, 0, 0, 0, Effects.Fx_Per_Porta_Up, 0, 0, 0);

			opaque.Xmp_Start_Player(44100, 0);

			c_int j = 0, k = 0;
			for (c_int i = 0; i < 60; i++)
			{
				k = 856 - i * 10;

				for (j = 0; j < 6; j++)
				{
					opaque.Xmp_Play_Frame();
					opaque.Xmp_Get_Frame_Info(out info);
					Assert.AreEqual(k - j * 2, Period(info), "Slide up error");
				}
			}

			j--;
			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(k - j * 2, Period(info), "Slide up error");

			// Persistent portamento down
			New_Event(opaque, 0, 0, 0, 84, 1, 0, Effects.Fx_Per_Porta_Dn, 2, 0, 0);
			New_Event(opaque, 0, 60, 0, 0, 0, 0, Effects.Fx_Per_Porta_Dn, 0, 0, 0);

			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 60; i++)
			{
				k = 113 + i * 10;

				for (j = 0; j < 6; j++)
				{
					opaque.Xmp_Play_Frame();
					opaque.Xmp_Get_Frame_Info(out info);
					Assert.AreEqual(k + j * 2, Period(info), "Slide down error");
				}
			}

			j--;
			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out info);
			Assert.AreEqual(k + j * 2, Period(info), "Slide down error");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
