/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Player
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Player
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Player_Scan()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Assert.IsNotNull(opaque, "Can't create context");

			c_int ret = LoadModule(dataDirectory, "Ode2Ptk.s3m", opaque);
			Assert.AreEqual(0, ret, "Can't load module");

			opaque.Xmp_Start_Player(44100, 0);
			opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);
			Assert.AreEqual(85472, info.Total_Time, "Incorrect total time");

			opaque.Xmp_End_Player();
			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
