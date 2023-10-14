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
		/// A couple of brief notes about instrument pan swing: All of the
		/// values are calculated with a range of 0-64. Values out of the
		/// 0-64 range are clipped. The swing simply defines the amount of
		/// variance from the current panning value.
		///
		/// Given all of this, a pan swing value of 16 with a center-panned
		/// (32) instrument should produce values between 16 and 48; a swing
		/// of 32 with full right panning (64) will produce values between
		/// 0 -- technically -32 -- and 32.
		///
		/// However, when a set panning effect is used along with a note, it
		/// should override the pan swing for that note.
		///
		/// This test should play sets of notes with: Hard left panning
		/// Left-biased random panning Hard right panning Right-biased random
		/// panning Center panning with no swing Completely random values
		/// </summary>
		/********************************************************************/
		[TestMethod]
		[Ignore]
		public void Test_Storlek_20_Pan_Swing_And_Set_Pan()
		{
			c_int[] values = new c_int[64];

			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			LoadModule(dataDirectory, "Storlek_20.it", opaque);
			opaque.Xmp_Start_Player(44100, 0);
			opaque.Xmp_Set_Player(Xmp_Player.Mix, 100);

			while (true)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

				if (info.Loop_Count > 0)
					break;

				Xmp_Channel_Info ci = info.Channel_Info[0];
				if (info.Frame == 0)
					values[info.Row] = ci.Pan;
			}

			// Check if pan values are kept in the empty rows
			for (c_int i = 0; i < 64; i += 2)
				Assert.AreEqual(values[i], values[i + 1], "Pan value not kept");

			// Check if left pan values are used
			for (c_int i = 0; i < 8; i++)
				Assert.AreEqual(0, values[i], "Pan left not set");

			// Check if left-biased pan values are used
			for (c_int i = 0; i < 8; i++)
				Assert.IsTrue(values[8 + i] < 128, "Pan not left-biased");

			// Check if right pan values are used
			for (c_int i = 0; i < 8; i++)
				Assert.AreEqual(255, values[16 + i], "Pan right not set");

			// Check if right-biased pan values are used
			for (c_int i = 0; i < 8; i++)
				Assert.IsTrue(values[24 + i] >= 124, "Pan not right-biased");

			// Check if center pan values are used
			for (c_int i = 0; i < 16; i++)
				Assert.AreEqual(128, values[32 + i], "Pan center not set");

			// Check pan randomness
			Assert.IsTrue(Util.Check_Randomness(values, 8, 8, 10), "Randomness error");
			Assert.IsTrue(Util.Check_Randomness(values, 24, 8, 10), "Randomness error");
			Assert.IsTrue(Util.Check_Randomness(values, 48, 16, 10), "Randomness error");

			opaque.Xmp_End_Player();
			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
