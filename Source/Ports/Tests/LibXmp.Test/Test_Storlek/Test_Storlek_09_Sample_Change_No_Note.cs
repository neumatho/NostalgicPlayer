/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Storlek
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Storlek
	{
		private struct Data
		{
			public c_int Period;
			public c_int Volume;
			public c_int Instrument;
		}

		/********************************************************************/
		/// <summary>
		/// 09 - Sample change with no note
		///
		/// If a sample number is given without a note, Impulse Tracker will
		/// play the old note with the new sample. This test should play the
		/// same beat twice, exactly the same way both times. Players which
		/// do not handle sample changes correctly will produce various
		/// interesting (but nonetheless incorrect!) results for the second
		/// measure
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_09_Sample_Change_No_Note()
		{
			Data[] data = new Data[100];
			c_int i = 0, j = 0;

			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			LoadModule(dataDirectory, "Storlek_09.it", opaque);
			opaque.Xmp_Start_Player(44100, 0);

			while (true)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

				if (info.Loop_Count > 0)
					break;

				Xmp_Channel_Info ci = info.Channel_Info[0];

				if (info.Row < 16)
				{
					data[i].Period = (c_int)ci.Period;
					data[i].Volume = ci.Volume;
					data[i].Instrument = ci.Instrument;
					i++;
				}
				else
				{
					Assert.AreEqual(data[j].Period, (c_int)ci.Period, "Period mismatch");
					Assert.AreEqual(data[j].Volume, ci.Volume, "Volume mismatch");
					Assert.AreEqual(data[j].Instrument, ci.Instrument, "Instrument mismatch");
					j++;
				}
			}

			opaque.Xmp_End_Player();
			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
