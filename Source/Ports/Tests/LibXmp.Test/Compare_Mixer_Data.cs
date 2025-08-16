/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Compare_Mixer_Data(string path, string mod, string data)
		{
			Compare_Mixer_Data(path, mod, data, 1, false, Xmp_Mode.Auto);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Compare_Mixer_Data_Loops(string path, string mod, string data, c_int loops)
		{
			Compare_Mixer_Data(path, mod, data, loops, false, Xmp_Mode.Auto);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Compare_Mixer_Data_No_Rv(string path, string mod, string data)
		{
			Compare_Mixer_Data(path, mod, data, 1, true, Xmp_Mode.Auto);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Compare_Mixer_Data_Player_Mode(string path, string mod, string data, Xmp_Mode mode)
		{
			Compare_Mixer_Data(path, mod, data, 1, false, mode);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Compare_Mixer_Data(string path, string mod, string data, c_int loops, bool ignore_Rv, Xmp_Mode mode)
		{
			using (StreamReader sr = new StreamReader(OpenStream(path, data)))
			{
				string line;

				Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
				Assert.IsNotNull(opaque, "Can't create context");

				c_int ret = LoadModule(path, mod, opaque);
				Assert.AreEqual(0, ret, "Can't load module");

				Xmp_Context ctx = GetContext(opaque);
				Player_Data p = ctx.P;

				opaque.Xmp_Start_Player(44100, 0);
				opaque.Xmp_Set_Player(Xmp_Player.Mix, 100);
				opaque.Xmp_Set_Player(Xmp_Player.Mode, (c_int)mode);

				c_int max_Channels = p.Virt.Virt_Channels;

				CSScanF csscanf = new CSScanF();

				while (true)
				{
					opaque.Xmp_Play_Frame();
					opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info fi);

					if (fi.Loop_Count >= loops)
						break;

					for (c_int i = 0; i < max_Channels; i++)
					{
						// Channel info doesn't get updated for NNA channels,
						// need to get the info period from the channel data
						Channel_Data xc = p.Xc_Data[i];
						c_int ci_Period = xc.Info_Period;

						c_int voc = Map_Channel(p, i);
						if ((voc < 0) || ((xc.Note_Flags & Note_Flag.Sample_End) != 0))
							continue;

						Mixer_Voice vi = p.Virt.Voice_Array[voc];

						line = sr.ReadLine();
						Assert.IsNotNull(line, "Early EOF");

						c_int num = csscanf.Parse(line, "%d %d %d %d %d %d %d %d %d %d %d %d");
						c_int time = (c_int)csscanf.Results[0];
						c_int row = (c_int)csscanf.Results[1];
						c_int frame = (c_int)csscanf.Results[2];
						c_int chan = (c_int)csscanf.Results[3];
						c_int period = (c_int)csscanf.Results[4];
						c_int note = (c_int)csscanf.Results[5];
						c_int ins = (c_int)csscanf.Results[6];
						c_int vol = (c_int)csscanf.Results[7];
						c_int pan = (c_int)csscanf.Results[8];
						c_int pos0 = (c_int)csscanf.Results[9];

						// Allow some error in values derived from floating point math
						Assert.IsTrue(Math.Abs(fi.Time - time) <= 1, "Time mismatch");
						Assert.AreEqual(row, fi.Row, "Row mismatch");
						Assert.AreEqual(frame, fi.Frame, "Frame mismatch");
						Assert.AreEqual(chan, i, "Channel mismatch");
						Assert.AreEqual(period, ci_Period, "Period mismatch");
						Assert.AreEqual(note, vi.Note, "Note mismatch");
						Assert.AreEqual(ins, vi.Ins, "Instrument");

						if (!ignore_Rv)
						{
							Assert.AreEqual(vol, vi.Vol, "Volume mismatch");
							Assert.AreEqual(pan, vi.Pan, "Pan mismatch");
						}

						// x87 precision edge cases can slightly change loop
						// wrapping behavior, which the abs() can't catch
						if (((vi.Pos0 == vi.Start) && (pos0 == vi.End)) || ((vi.Pos0 == vi.End) && (pos0 == vi.Start)))
							pos0 = vi.Pos0;

						Assert.IsTrue(Math.Abs(vi.Pos0 - pos0) <= 1, "Position mismatch");

						if (num >= 11)
						{
							c_int cutOff = (c_int)csscanf.Results[10];

							if ((cutOff >= 254) && (vi.Filter.CutOff >= 254))
								cutOff = vi.Filter.CutOff;

							Assert.AreEqual(cutOff, vi.Filter.CutOff, "CutOff mismatch");
						}

						if (num >= 12)
						{
							c_int resonance = (c_int)csscanf.Results[11];
							Assert.AreEqual(resonance, vi.Filter.Resonance, "Resonance mismatch");
						}
					}
				}

				line = sr.ReadLine();
				Assert.IsNull(line, "Not end of data file");

				opaque.Xmp_End_Player();
				opaque.Xmp_Release_Module();
				opaque.Xmp_Free_Context();
			}
		}
		#endregion
	}
}
