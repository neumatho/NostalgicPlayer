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
		/// 13 - Tremor with old effects
		///
		/// Just when you think you've figured out tremor, guess what – it's
		/// even more annoying. With Old Effects enabled, all non-zero values
		/// are incremented, so I40 with old effects means play for five
		/// ticks, and turn off for one; I51 means play for six and off for
		/// two.
		///
		/// When this test is played correctly, both notes should play at
		/// exactly the same intervals
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_13_Tremor_With_Old_Effects()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			LoadModule(dataDirectory, "Storlek_13.it", opaque);
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
