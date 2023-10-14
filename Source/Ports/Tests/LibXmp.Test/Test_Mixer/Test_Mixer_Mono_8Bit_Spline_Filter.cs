/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Mixer
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Mixer
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Mixer_Mono_8Bit_Spline_Filter()
		{
			using (StreamReader sr = new StreamReader(OpenStream(dataDirectory, "Mixer_8Bit_Spline_Filter.data")))
			{
				Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
				Xmp_Context ctx = GetContext(opaque);
				Mixer_Data s = ctx.S;

				LoadModule(dataDirectory,"Test.it", opaque);

				New_Event(opaque, 0, 0, 0, 30, 1, 0, 0x0f, 2, Effects.Fx_Flt_CutOff, 50);
				New_Event(opaque, 0, 1, 0, 30, 1, 0, 0x0f, 2, Effects.Fx_Flt_CutOff, 120);

				opaque.Xmp_Start_Player(22050, Xmp_Format.Mono);
				opaque.Xmp_Set_Player(Xmp_Player.Interp, (c_int)Xmp_Interp.Spline);

				CSScanF csscanf = new CSScanF();

				for (c_int i = 0; i < 4; i++)
				{
					opaque.Xmp_Play_Frame();
					opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

					for (c_int j = 0; j < info.Buffer_Size / 2; j++)
					{
						string line = sr.ReadLine();
						c_int ret = csscanf.Parse(line, "%d");
						Assert.AreEqual(1, ret, "Read error");
						Assert.IsTrue(Math.Abs(s.Buf32[j] - (c_int)csscanf.Results[0]) <= 1, "Mixing error");
					}
				}

				opaque.Xmp_End_Player();
				opaque.Xmp_Release_Module();
				opaque.Xmp_Free_Context();
			}
		}
	}
}
