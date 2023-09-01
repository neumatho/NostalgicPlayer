/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Player
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Player
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Virtual_Channel()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Module_Data m = ctx.M;

			Create_Simple_Module(opaque, 2, 2);

			Set_Instrument_Volume(opaque, 0, 0, 22);
			Set_Instrument_Volume(opaque, 1, 0, 33);
			Set_Instrument_Nna(opaque, 0, 0, Xmp_Inst_Nna.Cont, Xmp_Inst_Dct.Off, Xmp_Inst_Dca.Cut);

			m.Mod.Spd = 3;
			opaque.Xmp_Scan_Module();

			Set_Instrument_Envelope(opaque, 0, 0, 0, 64);
			Set_Instrument_Envelope(opaque, 0, 1, 34 * 3, 0);

			for (c_int i = 0; i < 34; i++)
			{
				for (c_int j = 0; j < 4; j++)
					New_Event(opaque, 0, i, j, 60, 1, 44, 0, 0, 0, 0);
			}

			Set_Quirk(opaque, Quirk_Flag.It, Read_Event.It);

			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 34; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info fi);

				c_int used = (i + 1) * 4;
				if (used > 128)
					used = 128;

				Assert.AreEqual(used, fi.Virt_Used, "Number of virtual channels");

				opaque.Xmp_Play_Frame();
				opaque.Xmp_Play_Frame();
			}

			for (c_int i = 0; i < 34; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info fi);

				c_int used = (33 - i) * 4;
				if (used > 128)
					used = 128;
				else if (used < 4)
					used = 4;

				Assert.AreEqual(used, fi.Virt_Used, "Number of virtual channels");

				opaque.Xmp_Play_Frame();
				opaque.Xmp_Play_Frame();
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
