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
		private const c_double Fine = 0.0625;

		private static readonly c_double[] deltas_IFP =
		[
			0.0, 0.0, 0.0, 0.0,
			-Fine * 0x0f, 0.0, 0.0, 0.0,		// K0F
			-Fine * 0x71, 0.0, 0.0, 0.0,		// K71
			Fine * 0x69, 0.0, 0.0, 0.0,			// L69
			Fine * 0x17, 0.0, 0.0, 0.0,			// L17
			0.0, -0x23, -0x23, -0x23,			// I23
			Fine * 0x23, 0.0, 0.0, 0.0,			// L00
			-Fine * 0x23, 0.0, 0.0, 0.0,		// K00
			0.0, 0x23, 0x23, 0x23,				// J00
			0.0, 0x14, 0x14, 0x14,				// J14
			-Fine * 0x14, 0.0, 0.0, 0.0,		// K00
			0.0, -0x14, -0x14, -0x14,			// I00
			Fine * 0x14, 0.0, 0.0, 0.0,			// L00
			-Fine * 0x1e, 0.0, 0.0, 0.0,		// K1E
			0.0, 0x1e, 0x1e, 0x1e,				// J00
			Fine * 0x1e, 0.0, 0.0, 0.0,			// L00
			0.0, -0x1e, -0x1e, -0x1e,			// I00
			Fine * 0x31, 0.0, 0.0, 0.0,			// L31
			0.0, -0x31, -0x31, -0x31,			// I00
			0.0, 0x31, 0x31, 0x31,				// J00
			-Fine * 0x31, 0.0, 0.0, 0.0,		// K00
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Imf_Fine_Porta()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			Create_Simple_Module(opaque, 2, 2);

			// IMF fine up/down
			New_Event(opaque, 0, 0, 0, 49, 1, 0, Effects.Fx_Speed, 0x04, 0, 0);
			New_Event(opaque, 0, 1, 0, 0, 0, 0, Effects.Fx_Imf_FPorta_Up, 0x0f, 0, 0);
			New_Event(opaque, 0, 2, 0, 0, 0, 0, Effects.Fx_Imf_FPorta_Up, 0x71, 0, 0);
			New_Event(opaque, 0, 3, 0, 0, 0, 0, Effects.Fx_Imf_FPorta_Dn, 0x69, 0, 0);
			New_Event(opaque, 0, 4, 0, 0, 0, 0, Effects.Fx_Imf_FPorta_Dn, 0x17, 0, 0);

			// Shared memory
			New_Event(opaque, 0, 5, 0, 0, 0, 0, Effects.Fx_Porta_Up, 0x23, 0, 0);
			New_Event(opaque, 0, 6, 0, 0, 0, 0, Effects.Fx_Imf_FPorta_Dn, 0x00, 0, 0);
			New_Event(opaque, 0, 7, 0, 0, 0, 0, Effects.Fx_Imf_FPorta_Up, 0x00, 0, 0);
			New_Event(opaque, 0, 8, 0, 0, 0, 0, Effects.Fx_Porta_Dn, 0x00, 0, 0);

			New_Event(opaque, 0, 9, 0, 0, 0, 0, Effects.Fx_Porta_Dn, 0x14, 0, 0);
			New_Event(opaque, 0, 10, 0, 0, 0, 0, Effects.Fx_Imf_FPorta_Up, 0x00, 0, 0);
			New_Event(opaque, 0, 11, 0, 0, 0, 0, Effects.Fx_Porta_Up, 0x00, 0, 0);
			New_Event(opaque, 0, 12, 0, 0, 0, 0, Effects.Fx_Imf_FPorta_Dn, 0x00, 0, 0);

			New_Event(opaque, 0, 13, 0, 0, 0, 0, Effects.Fx_Imf_FPorta_Up, 0x1e, 0, 0);
			New_Event(opaque, 0, 14, 0, 0, 0, 0, Effects.Fx_Porta_Dn, 0x00, 0, 0);
			New_Event(opaque, 0, 15, 0, 0, 0, 0, Effects.Fx_Imf_FPorta_Dn, 0x00, 0, 0);
			New_Event(opaque, 0, 16, 0, 0, 0, 0, Effects.Fx_Porta_Up, 0x00, 0, 0);

			New_Event(opaque, 0, 17, 0, 0, 0, 0, Effects.Fx_Imf_FPorta_Dn, 0x31, 0, 0);
			New_Event(opaque, 0, 18, 0, 0, 0, 0, Effects.Fx_Porta_Up, 0x00, 0, 0);
			New_Event(opaque, 0, 19, 0, 0, 0, 0, Effects.Fx_Porta_Dn, 0x00, 0, 0);
			New_Event(opaque, 0, 20, 0, 0, 0, 0, Effects.Fx_Imf_FPorta_Up, 0x00, 0, 0);

			opaque.Xmp_Start_Player(Constants.Xmp_Min_SRate, 0);

			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);
			c_int prev = (c_int)info.Channel_Info[0].Period;

			for (c_int i = 1; i < deltas_IFP.Length; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
				c_int next = (c_int)info.Channel_Info[0].Period;

				c_double d = ((next - prev) / 4096.0) - deltas_IFP[i];
				Assert.IsTrue((d >= -0.01) && (d <= 0.01), $"{i}: delta error: {(next - prev) / 4096.0} != {deltas_IFP[i]}");

				prev = next;
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
