/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Effect
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Effect
	{
		// From 08_sad_song.it, channel 06, pattern 004/011
		private static readonly c_int[] vals_It_VCol =
		{
			214, 214, 214, 214,	// 0
			214, 214, 214, 214,	// 1
			214, 214, 214, 214,	// 2
			214, 214, 214, 214,	// 3
			286, 286, 286, 286,	// 4
			286, 286, 286, 286,	// 5
			143, 143, 143, 143,	// 6
			143, 143, 143, 143,	// 7
			143, 143, 143, 143,	// 8
			143, 143, 143, 143,	// 9
			286, 286, 286, 286,	// 10
			286, 286, 286, 286,	// 11
			120, 120, 120, 120,	// 12
			120, 120, 120, 120,	// 13
			120, 120, 120, 120,	// 14
			120, 120, 120, 120,	// 15
			120, 127, 135, 135,	// 16
			135, 135, 135, 135,	// 17
			143, 143, 143, 143,	// 18
			143, 143, 143, 143,	// 19
			143, 143, 143, 143,	// 20
			143, 143, 143, 143,	// 21
			143, 147, 151, 156,	// 22
			156, 160, 165, 170,	// 23
			170, 175, 180, 180,	// 24
			180, 180, 180, 180,	// 25
			180, 180, 180, 180,	// 26
			180, 180, 180, 180,	// 27
			180, 185, 191, 196,	// 28
			196, 202, 208, 214,	// 29
			214, 214, 214, 214,	// 30
			214, 214, 214, 214,	// 31
			143, 143, 143, 143,	// 32
			143, 143, 143, 143,	// 33
			143, 143, 143, 143,	// 34
			143, 143, 143, 143,	// 35
			143, 143, 143, 143,	// 36
			143, 143, 143, 143,	// 37
			143, 147, 151, 156,	// 38
			156, 160, 160, 160,	// 39
			160, 160, 160, 160,	// 40
			160, 160, 160, 160,	// 41
			160, 160, 160, 160,	// 42
			160, 160, 160, 160,	// 43
			180, 180, 180, 180,	// 44
			180, 180, 180, 180,	// 45
			180, 185, 191, 191,	// 46
			191, 191, 191, 191,	// 47
			191, 191, 191, 191,	// 48
			191, 191, 191, 191,	// 49
			191, 191, 191, 191,	// 50
			191, 191, 191, 191,	// 51
			286, 286, 286, 286,	// 52
			286, 286, 286, 286,	// 53
			286, 286, 286, 286,	// 54
			286, 286, 286, 286,	// 55
			143, 143, 143, 143,	// 56
			143, 143, 143, 143,	// 57
			143, 143, 143, 143,	// 58
			143, 143, 143, 143,	// 59
			191, 191, 191, 191,	// 60
			191, 191, 191, 191,	// 61
			191, 185, 180, 180,	// 62
			180, 180, 180, 180	// 63
		};

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_It_VCol_G()
		{
			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Xmp_Context ctx = GetContext(opaque);
			Player_Data p = ctx.P;

			c_int ret = LoadModule(dataDirectory, "VCol_G.it", opaque);
			Assert.AreEqual(0, ret, "Can't load module");

			opaque.Xmp_Start_Player(44100, 0);

			for (c_int i = 0; i < 64 * 4; i++)
			{
				opaque.Xmp_Play_Frame();
				opaque.Xmp_Get_Frame_Info(out Xmp_Frame_Info info);

				c_int voc = Util.Map_Channel(p, 0);
				Assert.IsTrue(voc >= 0, "Virtual map");

				Assert.AreEqual(vals_It_VCol[i], Period(info), "Portamento error");
			}

			opaque.Xmp_Release_Module();
			opaque.Xmp_Free_Context();
		}
	}
}
