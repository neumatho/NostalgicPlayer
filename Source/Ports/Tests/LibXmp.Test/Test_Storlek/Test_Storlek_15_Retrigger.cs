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
		/// 15 - Retrigger
		///
		/// The retrig effect, in theory, should create an internal counter
		/// that decrements from the "speed" value (the 'y' in Qxy) until it
		/// reaches zero, at which point it resets. This timer should be
		/// unaffected by changes in the retrig speed – that is, beginning a
		/// retrig and then changing the speed prior to the next retrig point
		/// should not affect the timing of the next note. Additionally,
		/// retrig is entirely independent of song speed, and the counter
		/// should reset when new note is played.
		///
		/// As a side note, I would like to point out that the bassdrum
		/// sample uses a silent loop at the end. This is a workaround for
		/// Impulse Tracker's behavior of ignoring the retrig effect if no
		/// note is currently playing in the channel. Some people seem to
		/// have misinterpreted this, coming to the conclusion that retrig
		/// values greater than the song speed are ignored. However, this
		/// behavior is rather inconvenient when dealing with very short
		/// samples. I encourage the authors of other players to treat this
		/// behavior as a bug in Impulse Tracker's playback engine and
		/// retrigger notes when the timer expires regardless of the current
		/// state of the channel
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_15_Retrigger()
		{
			c_int count = 0;

			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			LoadModule(dataDirectory, "Storlek_15.it", opaque);
			opaque.Xmp_Start_Player(44100, 0);

			while (true)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

				if (info.Loop_Count > 0)
					break;

				Xmp_Channel_Info ci0 = info.channel_Info[0];
				Xmp_Channel_Info ci1 = info.channel_Info[1];

				Assert.IsTrue(((ci0.Position == 0) && (ci1.Position == 0)) || ((ci0.Position != 0) && (ci1.Position != 0)), "Retrigger error");
				count++;
			}

			Assert.AreEqual(176, count, "Short end");

			opaque.Xmp_End_Player();
			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
