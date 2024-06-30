/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Mixer
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Mixer
	{
		/********************************************************************/
		/// <summary>
		/// Data immediately prior to and after the loop should not affect
		/// the playback of the loop when using interpolation. The loop of
		/// the sample in this module should be completely silent.
		/// 
		/// Previously, libxmp played a buzz instead due to the sample prior
		/// to the loop not being fixed with spline interpolation. Similar
		/// behavior can be found in Modplug Tracker 1.16
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Mixer_Interpolation_Loop()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Mixer_Data s = ctx.S;

			c_int ret = LoadModule(dataDirectory,"Interpolation_Loop.it", opaque);
			Assert.AreEqual(0, ret, "Load error");

			opaque.Xmp_Start_Player(8000, Xmp_Format.Mono);
			opaque.Xmp_Set_Player(Xmp_Player.Interp, (c_int)Xmp_Interp.Spline);

			// First frame is the only one that should contain data
			Compare_Mixer_Samples_Ext(opaque, dataDirectory, "Interpolation_Loop.data", false, 1);

			// Further frames should be silent
			for (c_int i = 0; i < 10; i++)
			{
				opaque.Xmp_Play_Frame();

				for (c_int j = 0; j < s.TickSize; j++)
					Assert.AreEqual(0, s.Buf32[j], "Mixing error");
			}

			opaque.Xmp_End_Player();
			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
