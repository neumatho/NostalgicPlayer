/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.Utility;
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
		public void Test_Player_Period_Amiga()
		{
			using (StreamReader sr = new StreamReader(OpenStream(dataDirectory, "Periods.data")))
			{
				string line;
				c_int ret;

				Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

				Create_Simple_Module(opaque, 2, 2);

				opaque.Xmp_Start_Player(44100, 0);

				// Test note periods
				for (c_int i = 0; i < 60; i++)
				{
					New_Event(opaque, 0, i, 0, i, 1, 0, 0x0f, 1, 0, 0);
					New_Event(opaque, 0, i, 1, 60 + i, 1, 0, 0, 0, 0, 0);
				}

				CSScanF csscanf = new CSScanF();

				for (c_int i = 0; i < 60; i++)
				{
					opaque.Xmp_Play_Frame();
					opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

					line = sr.ReadLine();
					ret = csscanf.Parse(line, "%d %d");
					Assert.AreEqual(2, ret, "Read error");

					c_int p0 = (c_int)csscanf.Results[0];
					c_int p1 = (c_int)csscanf.Results[1];
					Assert.AreEqual((c_uint)p0, info.channel_Info[0].Period, "Bad period");
					Assert.AreEqual((c_uint)p1, info.channel_Info[1].Period, "Bad period");
				}

				line = sr.ReadLine();
				ret = csscanf.Parse(line, "%d %d");
				Assert.AreEqual(0, ret, "Not end of data file");

				opaque.Xmp_Release_Module();
				opaque.Xmp_Free_Context();
			}
		}
	}
}
