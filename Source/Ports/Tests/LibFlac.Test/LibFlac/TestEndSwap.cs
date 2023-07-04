/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibFlac.Share;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibFlac.Test.LibFlac
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class TestEndSwap
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_16Bit()
		{
			int16_t i16 = 0x1234;
			uint16_t u16 = 0xabcd;

			Console.WriteLine("Testing EndSwap_16 on int16_t");
			Assert.AreNotEqual(i16, EndSwap.EndSwap_16(i16));
			Assert.AreEqual(i16, EndSwap.EndSwap_16(EndSwap.EndSwap_16(i16)));

			Console.WriteLine("Testing EndSwap_16 on uint16_t");
			Assert.AreNotEqual(u16, EndSwap.EndSwap_16(u16));
			Assert.AreEqual(u16, EndSwap.EndSwap_16(EndSwap.EndSwap_16(u16)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_32Bit()
		{
			int32_t i32 = 0x12345678;
			uint32_t u32 = 0xabcdef01;

			Console.WriteLine("Testing EndSwap_32 on int32_t");
			Assert.AreNotEqual(i32, EndSwap.EndSwap_32(i32));
			Assert.AreEqual(i32, EndSwap.EndSwap_32(EndSwap.EndSwap_32(i32)));

			Console.WriteLine("Testing EndSwap_32 on uint32_t");
			Assert.AreNotEqual(u32, EndSwap.EndSwap_32(u32));
			Assert.AreEqual(u32, EndSwap.EndSwap_32(EndSwap.EndSwap_32(u32)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_64Bit()
		{
			int64_t i64 = 0x123456789abcdef0;
			uint64_t u64 = 0xabcdef0123456789;

			Console.WriteLine("Testing EndSwap_64 on int64_t");
			Assert.AreNotEqual(i64, EndSwap.EndSwap_64(i64));
			Assert.AreEqual(i64, EndSwap.EndSwap_64(EndSwap.EndSwap_64(i64)));

			Console.WriteLine("Testing EndSwap_64 on uint64_t");
			Assert.AreNotEqual(u64, EndSwap.EndSwap_64(u64));
			Assert.AreEqual(u64, EndSwap.EndSwap_64(EndSwap.EndSwap_64(u64)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_H2LE_16()
		{
			byte[] data = BitConverter.GetBytes(EndSwap.H2LE_16(0x1234));
			Assert.AreEqual(0x34, data[0]);
			Assert.AreEqual(0x12, data[1]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_H2LE_32()
		{
			byte[] data = BitConverter.GetBytes(EndSwap.H2LE_32(0x12345678));
			Assert.AreEqual(0x78, data[0]);
			Assert.AreEqual(0x56, data[1]);
			Assert.AreEqual(0x34, data[2]);
			Assert.AreEqual(0x12, data[3]);
		}
	}
}