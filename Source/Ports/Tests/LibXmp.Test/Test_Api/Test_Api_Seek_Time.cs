/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Api
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Api
	{
		private static readonly c_int[] pos_Ode2Ptk =
		{
			0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 
			2, 3, 3, 3, 3, 3, 3, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 
			5, 6, 6, 6, 6, 7, 7, 7, 7, 8, 8, 8, 8, 9, 9, 9, 9, 
			10, 10, 10, 11, 11, 11, 11, 12, 12, 12, 12, 13, 13, 
			13, 13, 14, 14, 14, 14, 15, 15, 15, 15, 16, 16, 16, 
			17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 
			17, 17, 17, 17, 17, 17, 17, 17, 17, 17
		};

		private static readonly c_int[] pos_Dlr =
		{
			0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 4, 4, 4, 4,
			5, 5, 5, 5, 6, 6, 6, 7, 7, 7, 7, 8, 8, 8, 8, 9, 9,
			9, 9, 10, 10, 10, 11, 11, 11, 11, 12, 12, 12, 12, 13,
			13, 13, 14, 14, 14, 14, 15, 15, 15, 15, 16, 16, 16,
			17, 17, 18, 18, 18, 18, 18, 18, 19, 19, 20, 20, 20,
			21, 21, 21, 21, 22, 22, 22, 22, 23, 23, 23, 24, 24,
			24, 24, 25, 25, 25, 25, 26, 26, 26, 27, 27, 28, 28
		};

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Api_Seek_Time()
		{
			// Seek ode2ptk
			Ports.LibXmp.LibXmp ctx = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			LoadModule(dataDirectory, "Ode2Ptk.s3m", ctx);
			ctx.Xmp_Start_Player(8000, 0);

			for (c_int i = 0; i < 100; i++)
			{
				c_int ret = ctx.Xmp_Seek_Time(i * 1000);
				Assert.AreEqual(pos_Ode2Ptk[i], ret, "Seek error");
			}

			ctx.Xmp_Release_Module();
			ctx.Xmp_Free_Context();

			// Seek dans le rue
			ctx = Ports.LibXmp.LibXmp.Xmp_Create_Context();

			LoadModule(Path.Combine(dataDirectory, "M"), "Xyce-Dans_La_Rue.xm", ctx);
			ctx.Xmp_Start_Player(8000, 0);

			for (c_int i = 0; i < 100; i++)
			{
				c_int ret = ctx.Xmp_Seek_Time(i * 1000);
				Assert.AreEqual(pos_Dlr[i], ret, "Seek error");
			}

			ctx.Xmp_Release_Module();
			ctx.Xmp_Free_Context();
		}
	}
}
