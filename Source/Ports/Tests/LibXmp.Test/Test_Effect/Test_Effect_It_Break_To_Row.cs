/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.Utility;
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
		public void Test_Effect_It_Break_To_Row()
		{
			using (StreamReader sr = new StreamReader(OpenStream(dataDirectory, "Break_To_Row.data")))
			{
				string line;

				Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
				LoadModule(dataDirectory, "Break_To_Row.it", opaque);
				opaque.Xmp_Get_Module_Info(out Xmp_Module_Info mInfo);
				opaque.Xmp_Start_Player(44100, 0);

				CSScanF csscanf = new CSScanF();

				while (true)
				{
					opaque.Xmp_Play_Frame();
					opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

					if (info.Loop_Count > 0)
						break;

					for (c_int i = 0; i < mInfo.Mod.Chn; i++)
					{
						line = sr.ReadLine();

						csscanf.Parse(line, "%d %d %d %d %d %d %d %d");
						c_int time = (c_int)csscanf.Results[0];
						c_int row = (c_int)csscanf.Results[1];
						c_int frame = (c_int)csscanf.Results[2];
						c_int chan = (c_int)csscanf.Results[3];
						c_int period = (c_int)csscanf.Results[4];
						c_int volume = (c_int)csscanf.Results[5];
						c_int ins = (c_int)csscanf.Results[6];
						c_int pan = (c_int)csscanf.Results[7];

						Xmp_Channel_Info ci = info.Channel_Info[chan];

						Assert.AreEqual(time, info.Time, "Time mismatch");
						Assert.AreEqual(row, info.Row, "Row mismatch");
						Assert.AreEqual(frame, info.Frame, "Frame mismatch");
						Assert.AreEqual(period, (c_int)ci.Period, "Period mismatch");
						Assert.AreEqual(volume, ci.Volume, "Volume mismatch");
						Assert.AreEqual(pan, ci.Pan, "Pan mismatch");
					}
				}

				line = sr.ReadLine();
				Assert.IsNull(line, "Not end of data file");

				opaque.Xmp_End_Player();
				opaque.Xmp_Release_Module();
				opaque.Xmp_Free_Context();
			}
		}
	}
}
