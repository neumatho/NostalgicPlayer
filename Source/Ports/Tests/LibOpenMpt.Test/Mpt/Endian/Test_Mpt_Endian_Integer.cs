/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOpenMpt.Test.Mpt.Endian
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Mpt_Endian
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Mpt_Endian_Integer()
		{
			#pragma warning disable MSTEST0032
			Assert.AreEqual(int8.MinValue, int8le.MinValue);
			Assert.AreEqual(uint8.MinValue, uint8le.MinValue);
			#pragma warning restore MSTEST0032

			Assert.AreEqual(int16.MinValue, int16le.MinValue);
			Assert.AreEqual(uint16.MinValue, uint16le.MinValue);

			Assert.AreEqual(int32.MinValue, int32le.MinValue);
			Assert.AreEqual(uint32.MinValue, uint32le.MinValue);

			Assert.AreEqual(int64.MinValue, int64le.MinValue);
			Assert.AreEqual(uint64.MinValue, uint64le.MinValue);

			#pragma warning disable MSTEST0032
			Assert.AreEqual(int8.MaxValue, int8le.MaxValue);
			Assert.AreEqual(uint8.MaxValue, uint8le.MaxValue);
			#pragma warning restore MSTEST0032

			Assert.AreEqual(int16.MaxValue, int16le.MaxValue);
			Assert.AreEqual(uint16.MaxValue, uint16le.MaxValue);

			Assert.AreEqual(int32.MaxValue, int32le.MaxValue);
			Assert.AreEqual(uint32.MaxValue, uint32le.MaxValue);

			Assert.AreEqual(int64.MaxValue, int64le.MaxValue);
			Assert.AreEqual(uint64.MaxValue, uint64le.MaxValue);

			int8le le8 = -128;
			int8be be8 = -128;
			Assert.AreEqual(-128, le8);
			Assert.AreEqual(-128, be8);

			int16le le16 = 0x1234;
			int16be be16 = 0x1234;
			Assert.AreEqual(0x1234, le16);
			Assert.AreEqual(0x1234, be16);
			Assert.AreEqual(0, CMemory.memcmp(GetRawBytes(le16), "\x34\x12", 2));
			Assert.AreEqual(0, CMemory.memcmp(GetRawBytes(be16), "\x12\x34", 2));

			uint32le le32 = 0xffeeddccU;
			uint32be be32 = 0xffeeddccU;
			Assert.AreEqual<uint32>(0xffeeddccU, le32);
			Assert.AreEqual<uint32>(0xffeeddccU, be32);
			Assert.AreEqual(0, CMemory.memcmp(GetRawBytes(le32), "\xcc\xdd\xee\xff", 4));
			Assert.AreEqual(0, CMemory.memcmp(GetRawBytes(be32), "\xff\xee\xdd\xcc", 4));

			uint64le le64 = 0xdeadc0de15c0ffeeUL;
			uint64be be64 = 0xdeadc0de15c0ffeeUL;
			Assert.AreEqual<uint64>(0xdeadc0de15c0ffeeUL, le64);
			Assert.AreEqual<uint64>(0xdeadc0de15c0ffeeUL, be64);
			Assert.AreEqual(0, CMemory.memcmp(GetRawBytes(le64), "\xee\xff\xc0\x15\xde\xc0\xad\xde", 8));
			Assert.AreEqual(0, CMemory.memcmp(GetRawBytes(be64), "\xde\xad\xc0\xde\x15\xc0\xff\xee", 8));
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Return the raw bytes of the given value, exactly as they are
		/// stored in memory
		/// </summary>
		/********************************************************************/
		private static CPointer<byte> GetRawBytes<T>(T value) where T : unmanaged
		{
			byte[] data = new byte[Unsafe.SizeOf<T>()];
			MemoryMarshal.Write(data, in value);

			return new CPointer<byte>(data);
		}
		#endregion
	}
}
