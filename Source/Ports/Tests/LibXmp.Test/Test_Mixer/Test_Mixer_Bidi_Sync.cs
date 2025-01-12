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
		// Due to various former quirks of the renderer, an extended amount
		// of the input modules needs to be rendered to make sure it doesn't
		// slowly diverge over time. The inputs are 6 * 256 frames long
		private const c_int Render_Frames = 6 * 256;

		// Checking the pre-downmix buffer, so allow at least +-(2^12) in error.
		// The lower this value can be, the better
		private const c_int Render_Error = 1 << 17;

		/********************************************************************/
		/// <summary>
		/// libxmp should tries to render bidirectional samples as closely
		/// to forward samples as possible. The two input modules for this
		/// test have been crafted to be almost completely silent when the
		/// two samples are synchronized, and libxmp should be able to keep
		/// them synchronized at different rates and interpolation settings
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Mixer_Bidi_Sync()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			c_int ret = LoadModule(dataDirectory, "Bidi_Sync.it", opaque);
			Assert.AreEqual(0, ret, "Load error");

			Bidi_Sync_Helper(opaque, "it");
			opaque.Xmp_Release_Module();

			ret = LoadModule(dataDirectory, "Bidi_Sync.xm", opaque);
			Assert.AreEqual(0, ret, "Load error");

			Bidi_Sync_Helper(opaque, "xm");
			opaque.Xmp_Release_Module();

			opaque.Xmp_Free_Context();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Bidi_Sync_Helper(Ports.LibXmp.LibXmp opaque, string str)
		{
//			Bidi_Sync_Test_Mode(opaque, 8000, Xmp_Interp.Linear, str + ":linear:8k");
			Bidi_Sync_Test_Mode(opaque, 8000, Xmp_Interp.Spline, str + ":spline:8k");
//			Bidi_Sync_Test_Mode(opaque, 11025, Xmp_Interp.Nearest, str + ":nearest:11k");
//			Bidi_Sync_Test_Mode(opaque, 11025, Xmp_Interp.Linear, str + ":linear:11k");
			Bidi_Sync_Test_Mode(opaque, 11025, Xmp_Interp.Spline, str + ":spline:11k");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Bidi_Sync_Test_Mode(Ports.LibXmp.LibXmp opaque, c_int rate, Xmp_Interp interp, string str)
		{
			Xmp_Context ctx = GetContext(opaque);
			Mixer_Data s = ctx.S;

			opaque.Xmp_Start_Player(rate, Xmp_Format.Mono);
			opaque.Xmp_Set_Player(Xmp_Player.Interp, (c_int)interp);

			for (c_int i = 0; i < Render_Frames; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

				for (c_int j = 0; j < info.Buffer_Size / 2; j++)
					Assert.IsTrue((s.Buf32[j] >= -Render_Error) && (s.Buf32[j] <= Render_Error), str);
			}
		}
	}
}
