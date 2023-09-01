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
		private static readonly c_int[] vals_PV =
		{
			143, 143, 154, 158, 154, 143,
			132, 132, 128, 132, 143, 154,
			158, 158, 154, 143, 132, 128,
			132, 132, 143, 154, 158, 154,
			143, 143, 143, 143, 143, 143,
			143, 143, 143, 143, 143, 143
		};

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Persistent_Vibrato()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			Create_Simple_Module(opaque, 2, 2);

			New_Event(opaque, 0, 0, 0, 80, 1, 0, Effects.Fx_Per_Vibrato, 0x88, 0, 0);
			New_Event(opaque, 0, 4, 0, 80, 1, 0, Effects.Fx_Per_Vibrato, 0x40, 0, 0);

			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 6 * 6; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);
				Assert.AreEqual(vals_PV[i], Period(info), "Vibrato error");
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
