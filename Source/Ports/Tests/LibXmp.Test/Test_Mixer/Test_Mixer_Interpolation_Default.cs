/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Mixer_Interpolation_Default()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			LoadModule(dataDirectory, "Test.xm", opaque);

			opaque.Xmp_Start_Player(8000, Xmp_Format.Mono);
			Xmp_Interp interp = (Xmp_Interp)opaque.Xmp_Get_Player(Xmp_Player.Interp);
			Assert.AreEqual(Xmp_Interp.Linear, interp, "Default not linear");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
