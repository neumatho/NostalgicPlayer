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
		/// 16 - Global volume
		///
		/// This test checks the overall handling of the global volume (Vxx)
		/// and global volume slide (Wxx) effects. If played properly, both
		/// notes should fade in from silence to full volume, back to
		/// silence, and then fade in and out a second time. (Note that the
		/// volume should be fading to and from maximum, i.e. two W80 effects
		/// at speed 9 should change the volume from 0 to 128.)
		///
		/// If the notes start at full volume instead of fading in, the V80
		/// in channel 2 is probably overriding the V00 in channel 3 on the
		/// first row. Similarly, if the volume is suddenly cut on row 4, the
		/// V00 is probably incorrectly taking precedence over the V80.
		/// Generally, for effects that alter global state, the
		/// highest-numbered channel takes precedence.
		///
		/// Since two W80 effects at speed 9 raise the volume from zero to
		/// the maximum of 128, the V80 on row 3 should not have any effect
		/// on the volume.
		///
		/// Also, there are two spurious volume effects in channel 3, on rows
		/// 7 and 11. Both of these effects should be ignored as out-of-range
		/// data, not clamped to the maximum (or minimum!) volume.
		///
		/// Finally, the previous value for the global volume slide effect is
		/// saved per channel. If the volume fades in and out, and does not
		/// fade back in, it is almost certainly because separate global
		/// volume slide parameters are not stored for each channel
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_16_Global_Volume()
		{
			c_int[] vol = new c_int[80];
			c_int i = 0;

			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			LoadModule(dataDirectory, "Storlek_16.it", opaque);
			opaque.Xmp_Start_Player(44100, 0);

			while (true)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

				if (info.Loop_Count > 0)
					break;

				Xmp_Channel_Info ci = info.Channel_Info[0];

				if (info.Row < 8)
					vol[i++] = ci.Volume;
				else
				{
					Assert.AreEqual(vol[i - 72], ci.Volume, "Volume error");
					i++;
				}
			}

			opaque.Xmp_End_Player();
			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
