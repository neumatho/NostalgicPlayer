/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
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
		public void Test_Player_Invalid_Mod_Length()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Module_Data m = ctx.M;

			Create_Simple_Module(opaque, 2, 2);

			New_Event(opaque, 0, 0, 0, 49, 1, 0, 0, 0, 0, 0);
			m.Mod.Len = 0;

			opaque.Xmp_Start_Player(44100, 0);

			opaque.Xmp_Play_Frame();
			opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info _);

			// Fix length so mod is freed correctly
			m.Mod.Len = 2;

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
