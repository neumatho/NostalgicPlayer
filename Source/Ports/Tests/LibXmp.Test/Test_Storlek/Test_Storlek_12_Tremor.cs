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
		/// 12 - Tremor effect
		///
		/// Like many other effects, tremor has an effect memory. This
		/// memory stores both the "on" AND "off" values at once — they are
		/// not saved independently.
		///
		/// Also, when tremor is given a zero on- or off-value, that value is
		/// handled as one tick instead. That is, I03 plays the same way as
		/// I13: play for one tick, off for three.
		///
		/// Another potential snag is what happens when the note is "off"
		/// when the row changes and the new row does not have a tremor
		/// effect. In this case, the volume is always restored to normal,
		/// and the next time the effect is used, the off-tick counter
		/// picks up right where it left off.
		///
		/// Finally, the only time the current tremor counts are reset is
		/// when the playback is interrupted. Otherwise, the only part of the
		/// player code that should even touch these values is the tremor
		/// effect handler, and it only ever decreases the values... well,
		/// until they hit zero, at that point they are obviously reset;
		/// but also note, the reset is independent for the on-tick and
		/// off-tick counters.
		///
		/// When this test is played correctly, both notes should play at
		/// exactly the same intervals
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_12_Tremor()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			LoadModule(dataDirectory, "Storlek_12.it", opaque);
			opaque.Xmp_Start_Player(44100, 0);

			while (true)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

				if (info.Loop_Count > 0)
					break;

				Xmp_Channel_Info ci0 = info.channel_Info[0];
				Xmp_Channel_Info ci1 = info.channel_Info[1];

				Assert.AreEqual(ci1.Volume, ci0.Volume, "Tremor error");
			}

			opaque.Xmp_End_Player();
			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
