/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp;
using Polycode.NostalgicPlayer.Ports.LibXmp.Loaders;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Depack
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Depack
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Depack_It_Sample_8Bit()
		{
			uint8[] tmp = new uint8[10000];
			uint8[] dest = new uint8[10000];

			using (Stream stream = OpenStream(dataDirectory, "It-Sample-8Bit.raw"))
			{
				Hio f = Hio.Hio_Open_File(stream);
				Assert.IsNotNull(f, "Can't open data file");

				c_int ret = Sample.ItSex_Decompress8(f, dest, 4879, tmp, tmp.Length, false);
				Assert.AreEqual(0, ret, "Decompression fail");

				ret = Check_Md5(dest, 4879, "299C9144AE2349B90B430AAFDE8D799A");
				Assert.AreEqual(0, ret, "MD5 error");
			}
		}
	}
}
