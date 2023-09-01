/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Effect
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Effect
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Per_TonePorta()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Frame_Info info = null;

			Create_Simple_Module(opaque, 2, 2);

			New_Event(opaque, 0, 0, 0, 49, 1, 0, 0, 0, 0, 0);
			New_Event(opaque, 0, 1, 0, 72, 1, 0, Effects.Fx_Per_TPorta, 1, 0, 0);
			New_Event(opaque, 0, 55, 0, 0, 0, 0, Effects.Fx_Per_TPorta, 0, 0, 0);

			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 60 * 6; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out info);
			}

			Assert.AreEqual(586, Period(info), "Slide error");

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
