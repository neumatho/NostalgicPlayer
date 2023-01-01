/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Private;

namespace Polycode.NostalgicPlayer.Agent.Player.Flac.Test.LibFlac
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class TestCrc
	{
		private const int DataSize = 32768;

		private Flac__byte[] data;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestInitialize]
		public void Initialize()
		{
			data = new Flac__byte[DataSize];

			// Initialize data reproducibly with pseudo-random values
			for (uint32_t i = 1; i < DataSize; i++)
				data[i] = Crc8_Update_Ref((Flac__byte)(i % 256), data[i - 1]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Crc8()
		{
			Flac__uint8 crc0 = 0;
			Flac__uint8 crc1 = Crc.Flac__Crc8(data, 0);
			Assert.AreEqual(crc0, crc1, "Returned non-zero CRC for zero bytes of data");

			for (uint32_t i = 0; i < DataSize; i++)
			{
				crc0 = Crc8_Update_Ref(data[i], crc0);
				crc1 = Crc.Flac__Crc8(data, i + 1);
				Assert.AreEqual(crc0, crc1, $"Result did not match reference CRC for {i + 1} bytes of test data");
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Crc16()
		{
			Flac__uint16 crc0 = 0;
			Flac__uint16 crc1 = Crc.Flac__Crc16(data, 0);
			Assert.AreEqual(crc0, crc1, "Returned non-zero CRC for zero bytes of data");

			for (uint32_t i = 0; i < DataSize; i++)
			{
				crc0 = Crc16_Update_Ref(data[i], crc0);
				crc1 = Crc.Flac__Crc16(data, i + 1);
				Assert.AreEqual(crc0, crc1, $"Result did not match reference CRC for {i + 1} bytes of test data");
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Crc16_Update()
		{
			Flac__uint16 crc0 = 0;
			Flac__uint16 crc1 = 0;

			for (uint32_t i = 0; i < DataSize; i++)
			{
				crc0 = Crc16_Update_Ref(data[i], crc0);
				crc1 = Crc.Flac__Crc16_Update(data[i], crc1);
				Assert.AreEqual(crc0, crc1, $"Result did not match reference CRC for {i + 1} bytes of test data");
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Crc16_32Bit_Words()
		{
			Flac__uint32[] words = MemoryMarshal.Cast<Flac__byte, Flac__uint32>(data).ToArray();

			for (uint32_t n = 1; n <= 16; n++)
			{
				Console.WriteLine($"Testing with length={n}");

				Flac__uint16 crc0 = 0;
				Flac__uint16 crc1 = 0;

				for (uint32_t i = 0; i <= words.Length - n; i += n)
				{
					for (uint32_t k = 0; k < n; k++)
					{
						crc0 = Crc16_Update_Ref((Flac__byte)(words[i + k] >> 24), crc0);
						crc0 = Crc16_Update_Ref((Flac__byte)((words[i + k] >> 16) & 0xff), crc0);
						crc0 = Crc16_Update_Ref((Flac__byte)((words[i + k] >> 8) & 0xff), crc0);
						crc0 = Crc16_Update_Ref((Flac__byte)(words[i + k] & 0xff), crc0);
					}

					crc1 = Crc.Flac__Crc16_Update_Words32(words, i, n, crc1);
					Assert.AreEqual(crc0, crc1, $"Result did not match reference CRC for {i + n} words of test data");
				}

				crc1 = Crc.Flac__Crc16_Update_Words32(words, 0, 0, crc1);
				Assert.AreEqual(crc0, crc1, "Called with zero bytes changed CRC value");
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Crc16_64Bit_Words()
		{
			Flac__uint64[] words = MemoryMarshal.Cast<Flac__byte, Flac__uint64>(data).ToArray();

			for (uint32_t n = 1; n <= 16; n++)
			{
				Console.WriteLine($"Testing with length={n}");

				Flac__uint16 crc0 = 0;
				Flac__uint16 crc1 = 0;

				for (uint32_t i = 0; i <= words.Length - n; i += n)
				{
					for (uint32_t k = 0; k < n; k++)
					{
						crc0 = Crc16_Update_Ref((Flac__byte)(words[i + k] >> 56), crc0);
						crc0 = Crc16_Update_Ref((Flac__byte)((words[i + k] >> 48) & 0xff), crc0);
						crc0 = Crc16_Update_Ref((Flac__byte)((words[i + k] >> 40) & 0xff), crc0);
						crc0 = Crc16_Update_Ref((Flac__byte)((words[i + k] >> 32) & 0xff), crc0);
						crc0 = Crc16_Update_Ref((Flac__byte)((words[i + k] >> 24) & 0xff), crc0);
						crc0 = Crc16_Update_Ref((Flac__byte)((words[i + k] >> 16) & 0xff), crc0);
						crc0 = Crc16_Update_Ref((Flac__byte)((words[i + k] >> 8) & 0xff), crc0);
						crc0 = Crc16_Update_Ref((Flac__byte)(words[i + k] & 0xff), crc0);
					}

					crc1 = Crc.Flac__Crc16_Update_Words64(words, i, n, crc1);
					Assert.AreEqual(crc0, crc1, $"Result did not match reference CRC for {i + n} words of test data");
				}

				crc1 = Crc.Flac__Crc16_Update_Words64(words, 0, 0, crc1);
				Assert.AreEqual(crc0, crc1, "Called with zero bytes changed CRC value");
			}
		}

		#region Reference implementations of CRC-8 and CRC-16 to check against
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__uint8 Crc8_Update_Ref(Flac__byte @byte, Flac__uint8 crc)
		{
			const Flac__uint8 Crc8_Polynomial = 0x07;

			crc ^= @byte;

			for (int i = 0; i < 8; i++)
				crc = (Flac__uint8)((crc << 1) ^ ((crc >> 7) != 0 ? Crc8_Polynomial : 0));

			return crc;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__uint16 Crc16_Update_Ref(Flac__byte @byte, Flac__uint16 crc)
		{
			const Flac__uint16 Crc16_Polynomial = 0x8005;

			crc ^= (Flac__uint16)(@byte << 8);

			for (int i = 0; i < 8; i++)
				crc = (Flac__uint16)((crc << 1) ^ ((crc >> 15) != 0 ? Crc16_Polynomial : 0));

			return crc;
		}
		#endregion
	}
}
