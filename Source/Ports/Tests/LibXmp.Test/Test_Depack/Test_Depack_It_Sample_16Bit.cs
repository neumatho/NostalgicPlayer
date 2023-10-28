/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using System.Runtime.InteropServices;
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
		public void Test_Depack_It_Sample_16Bit()
		{
			uint8[] tmp = new uint8[10000];
			uint8[] dest = new uint8[10000];

			using (Stream stream = OpenStream(dataDirectory, "It-Sample-16Bit.raw"))
			{
				Hio f = Hio.Hio_Open_File(stream);
				Assert.IsNotNull(f, "Can't open data file");

				c_int ret = Sample.ItSex_Decompress16(f, MemoryMarshal.Cast<uint8, int16>(dest), 4646, tmp, tmp.Length, false);
				Assert.AreEqual(0, ret, "Decompression fail");

				if (Is_Big_Endian())
					Util.Convert_Endian(dest, 0, 4646);

				ret = Util.Check_Md5(dest, 4646 * 2, "1E2395653F9BD7838006572D8FCDB646");
				Assert.AreEqual(0, ret, "MD5 error");
			}
		}
	}
}
