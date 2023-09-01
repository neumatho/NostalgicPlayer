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
		/// 11 - Infinite loop exploit
		///
		/// (Note: on this test, "fail" status is given for players which
		/// deadlock while loading or calculating the duration, and is not
		/// based on actual playback behavior. Incidentally, this will cause
		/// Impulse Tracker to freeze.)
		///
		/// This is a particularly evil pattern loop setup that exploits two
		/// possible problems at the same time, and it will very likely cause
		/// any player to get "stuck".
		///
		/// The first problem here is the duplicated loopback effect on the
		/// first channel; the correct way to handle this is discussed in the
		/// previous test. The second problem, and quite a bit more difficult
		/// to handle, is the seemingly strange behavior after the third
		/// channel's loop plays once. What happens is the second SB1 in the
		/// first channel "empties" its loopback counter, and when it reaches
		/// the first SB1 again, the value is reset to 1. However, the second
		/// channel hasn't looped yet, so playback returns to the first row.
		/// The next time around, the second channel is done, but the first
		/// one needs to loop again — creating an infinite loop situation.
		/// Even Impulse Tracker gets snagged by this
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Storlek_11_Infinite_Pattern_Loop()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			LoadModule(dataDirectory, "Storlek_11.it", opaque);
			opaque.Xmp_Start_Player(44100, 0);

			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);
			Assert.IsTrue(info.Loop_Count > 0, "Loop not detected");

			opaque.Xmp_End_Player();
			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
