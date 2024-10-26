/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Read_Mem
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Read_Mem
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Read_Mem_Hio()
		{
			uint8[] mem = new uint8[100];
			uint8[] mem2 = new uint8[100];

			for (c_int i = 0; i < 100; i++)
				mem[i] = (uint8)i;

			Hio h = Hio.Hio_Open_Const_Mem(mem, 100);
			Assert.IsNotNull(h, "hio_open");

			c_int x = h.Hio_Size();
			Assert.AreEqual(100, x, "hio_size");

			x = h.Hio_Read8();
			Assert.AreEqual(0x00, x, "hio_read8");

			x = h.Hio_Read8S();
			Assert.AreEqual(0x01, x, "hio_read8s");

			x = h.Hio_Read16L();
			Assert.AreEqual(0x0302, x, "hio_read16l");

			x = h.Hio_Read16B();
			Assert.AreEqual(0x0405, x, "hio_read16b");

			x = (int)h.Hio_Read24L();
			Assert.AreEqual(0x080706, x, "hio_read24l");

			x = (int)h.Hio_Read24B();
			Assert.AreEqual(0x090a0b, x, "hio_read24b");

			x = (int)h.Hio_Read32L();
			Assert.AreEqual(0x00f0e0d0c, x, "hio_read32l");

			x = (int)h.Hio_Read32B();
			Assert.AreEqual(0x10111213, x, "hio_read32b");

			x = h.Hio_Tell();
			Assert.AreEqual(0x14, x, "hio_fseek");

			x = h.Hio_Seek(2, SeekOrigin.Begin);
			Assert.AreEqual(0, x, "hio_fseek SEEK_SET");

			x = (int)h.Hio_Read32B();
			Assert.AreEqual(0x02030405, x, "hio_read32b");

			x = h.Hio_Seek(3, SeekOrigin.Current);
			Assert.AreEqual(0, x, "hio_fseek SEEK_CUR");

			x = (int)h.Hio_Read(mem2, 1, 50);
			for (c_int i = 0; i < 50; i++)
				Assert.AreEqual(i + 9, mem2[i], "hio_read");

			x = h.Hio_Seek(0, SeekOrigin.End);
			Assert.AreEqual(0, x, "hio_fseek SEEK_END");

			x = h.Hio_Read8();
			bool b = h.Hio_Eof();
			Assert.IsTrue(b, "read8 eof");

			x = h.Hio_Read8S();
			b = h.Hio_Eof();
			Assert.IsTrue(b, "read8s eof");

			x = h.Hio_Read16L();
			b = h.Hio_Eof();
			Assert.IsTrue(b, "read16l eof");

			x = h.Hio_Read16B();
			b = h.Hio_Eof();
			Assert.IsTrue(b, "read16b eof");

			x = (int)h.Hio_Read24L();
			b = h.Hio_Eof();
			Assert.IsTrue(b, "read24l eof");

			x = (int)h.Hio_Read24B();
			b = h.Hio_Eof();
			Assert.IsTrue(b, "read24b eof");

			x = (int)h.Hio_Read32L();
			b = h.Hio_Eof();
			Assert.IsTrue(b, "read32l eof");

			x = (int)h.Hio_Read32B();
			b = h.Hio_Eof();
			Assert.IsTrue(b, "read32b eof");

			// Seek past end
			x = h.Hio_Seek(20, SeekOrigin.Current);
			Assert.AreEqual(0, x, "hio_seek");

			x = h.Hio_Tell();
			Assert.AreEqual(100, x, "hio_seek");

			x = h.Hio_Close();
			Assert.AreEqual(0, x, "hio_close");
		}
	}
}
