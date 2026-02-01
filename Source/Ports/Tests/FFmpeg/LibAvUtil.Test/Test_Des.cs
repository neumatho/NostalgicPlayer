/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.FFmpegTest;

namespace Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.LibAvUtil.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class Test_Des : TestBase
	{
		private class Union_Word_Byte
		{
			public CPointer<uint8_t> Byte = new CPointer<uint8_t>(8);

			public uint64_t Word
			{
				get => IntReadWrite.Av_RN64(Byte);

				set => IntReadWrite.Av_WN64(Byte, value);
			}
		}

		private static readonly uint8_t[] test_Key = [ 0x12, 0x34, 0x56, 0x78, 0x9a, 0xbc, 0xde, 0xf0 ];
		private static readonly uint8_t[] plain = [ 0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54, 0x32, 0x10 ];
		private static readonly uint8_t[] crypt_Ref = [ 0x4a, 0xb6, 0x5b, 0x3d, 0x4b, 0x06, 0x15, 0x18 ];

		private static readonly uint8_t[] cbc_Key = 
		[
			0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef,
			0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0x01,
			0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0x01, 0x23
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test()
		{
			RunTest(null);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override c_int DoTest()
		{
			AvDes d = new AvDes();
			Union_Word_Byte[] key = ArrayHelper.InitializeArray<Union_Word_Byte>(3);
			Union_Word_Byte data = new Union_Word_Byte();
			Union_Word_Byte ct = new Union_Word_Byte();
			uint64_t[] roundKeys = new uint64_t[16];
			CPointer<uint8_t> tmp = new CPointer<uint8_t>(8);

			key[0].Word = IntReadWrite.Av_RB64(test_Key);
			data.Word = IntReadWrite.Av_RB64(plain);

			Des.Gen_RoundKeys(roundKeys, key[0].Word);

			if (Des.Des_EncDec(data.Word, roundKeys, 0) != IntReadWrite.Av_RB64(crypt_Ref))
			{
				CConsole.printf("Test 1 failed\n");

				return 1;
			}

			Des.Av_Des_Init(d, test_Key, 64, 0);
			Des.Av_Des_Crypt(d, tmp, plain, 1, null, 0);

			if (CMemory.memcmp(tmp, crypt_Ref, (size_t)crypt_Ref.Length) != 0)
			{
				printf("Public API decryption failed\n");

				return 1;
			}

			if (!Run_Test(0, 0) || !Run_Test(0, 1) || !Run_Test(1, 0) || !Run_Test(1, 1))
			{
				printf("Partial Monte-Carlo test failed\n");

				return 1;
			}

			for (c_int i = 0; i < 1000; i++)
			{
				key[0].Word = ((uint64_t)(uint)RandomGenerator.GetRandomNumber() << 32) | (uint)RandomGenerator.GetRandomNumber();
				key[1].Word = ((uint64_t)(uint)RandomGenerator.GetRandomNumber() << 32) | (uint)RandomGenerator.GetRandomNumber();
				key[2].Word = ((uint64_t)(uint)RandomGenerator.GetRandomNumber() << 32) | (uint)RandomGenerator.GetRandomNumber();
				data.Word = ((uint64_t)(uint)RandomGenerator.GetRandomNumber() << 32) | (uint)RandomGenerator.GetRandomNumber();

				CPointer<uint8_t> fullKey = new CPointer<uint8_t>(3 * 8);
				CMemory.memcpy(fullKey, key[0].Byte, 8);
				CMemory.memcpy(fullKey + 8, key[1].Byte, 8);
				CMemory.memcpy(fullKey + 16, key[2].Byte, 8);

				Des.Av_Des_Init(d, fullKey, 192, 0);
				Des.Av_Des_Crypt(d, ct.Byte, data.Byte, 1, null, 0);

				Des.Av_Des_Init(d, fullKey, 192, 1);
				Des.Av_Des_Crypt(d, ct.Byte, ct.Byte, 1, null, 1);

				if (ct.Word != data.Word)
				{
					printf("Test 2 failed\n");

					return 1;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Run_Test(c_int cbc, c_int decrypt)
		{
			AvDes d = new AvDes();
			c_int delay = (cbc != 0) && (decrypt == 0) ? 2 : 1;

			CPointer<uint8_t> large_Buffer = new CPointer<uint8_t>(10002 * 8);
			CPointer<uint8_t> tmp = new CPointer<uint8_t>(8);

			IntReadWrite.Av_WB64(large_Buffer + (0 * 8), 0x4e6f772069732074UL);
			IntReadWrite.Av_WB64(large_Buffer + (1 * 8), 0x1234567890abcdefUL);
			IntReadWrite.Av_WB64(tmp, 0x1234567890abcdefUL);

			Des.Av_Des_Init(d, cbc_Key, 192, decrypt);
			Des.Av_Des_Crypt(d, large_Buffer + (delay * 8), large_Buffer + (0 * 8), 10000, cbc != 0 ? tmp : null, decrypt);

			uint64_t res = IntReadWrite.Av_RB64(large_Buffer + ((9999 + delay) * 8));

			if (cbc != 0)
			{
				if (decrypt != 0)
					return res == 0xc5cecf63ecec514cUL;
				else
					return res == 0xcb191f85d1ed8439UL;
			}
			else
			{
				if (decrypt != 0)
					return res == 0x8325397644091a0aUL;
				else
					return res == 0xdd17e8b8b437d232UL;
			}
		}
	}
}
