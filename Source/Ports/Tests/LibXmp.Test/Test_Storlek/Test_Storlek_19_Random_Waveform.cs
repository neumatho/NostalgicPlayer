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
		/********************************************************************/
		/// <summary>
		/// 19 - Random waveform
		///
		/// The random waveform for vibrato/tremolo/panbrello should not use
		/// a static lookup table, but should be based on some form of random
		/// number generator. Particularly, each playing channel should have
		/// a different value sequence.
		///
		/// Correct playback of this song should result in three stereo
		/// effects. It might also be helpful to view the internal player
		/// variables in Impulse/Schism Tracker's info page detail view (page
		/// up from the sample VU meters)
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_19_Random_Waveform()
		{
			c_int[] val0 = new c_int[48 * 6];
			c_int[] val1 = new c_int[48 * 6];
			c_int i;

			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			LoadModule(dataDirectory, "Storlek_19.it", opaque);
			opaque.Xmp_Start_Player(44100, 0);

			// Play it
			for (i = 0; i < 16 * 6; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

				Xmp_Channel_Info c0 = info.channel_Info[0];
				Xmp_Channel_Info c1 = info.channel_Info[1];

				val0[i] = c0.Volume;
				val1[i] = c1.Volume;
			}

			for (; i < 32 * 6; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

				Xmp_Channel_Info c0 = info.channel_Info[0];
				Xmp_Channel_Info c1 = info.channel_Info[1];

				val0[i] = (c_int)c0.Period;
				val1[i] = (c_int)c1.Period;
			}

			for (; i < 48 * 6; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

				Xmp_Channel_Info c0 = info.channel_Info[0];
				Xmp_Channel_Info c1 = info.channel_Info[1];

				val0[i] = c0.Pan;
				val1[i] = c1.Pan;
			}

			// Now play it again and compare
			opaque.Xmp_Restart_Module();

			c_int flag = 0;

			for (i = 0; i < 16 * 6; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

				Xmp_Channel_Info c0 = info.channel_Info[0];
				Xmp_Channel_Info c1 = info.channel_Info[1];

				if ((c0.Volume == val0[i]) && (c1.Volume == val1[i]))
					flag++;
			}

			for (; i < 32 * 6; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

				Xmp_Channel_Info c0 = info.channel_Info[0];
				Xmp_Channel_Info c1 = info.channel_Info[1];

				if ((c0.Period == val0[i]) && (c1.Period == val1[i]))
					flag++;
			}

			for (; i < 48 * 6; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

				Xmp_Channel_Info c0 = info.channel_Info[0];
				Xmp_Channel_Info c1 = info.channel_Info[1];

				if ((c0.Pan == val0[i]) && (c1.Pan == val1[i]))
					flag++;
			}

			Assert.IsTrue(flag < 30, "Random values error");

			opaque.Xmp_End_Player();
			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
