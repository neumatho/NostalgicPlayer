/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.FFmpegTest;

namespace Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.LibAvUtil.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class Test_Sha : TestBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Sha_()
		{
			RunTest("sha");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override c_int DoTest()
		{
			CPointer<c_uchar> digest = new CPointer<c_uchar>(32);
			c_int[] lengths = [ 160, 224, 256 ];

			AvSha ctx = Sha.Av_Sha_Alloc();

			if (ctx == null)
				return 1;

			for (c_int j = 0; j < 3; j++)
			{
				printf("Testing SHA-%d\n", lengths[j]);

				for (c_int k = 0; k < 3; k++)
				{
					Sha.Av_Sha_Init(ctx, lengths[j]);

					if (k == 0)
						Sha.Av_Sha_Update(ctx, Encoding.Latin1.GetBytes("abc"), 3);
					else if (k == 1)
						Sha.Av_Sha_Update(ctx, Encoding.Latin1.GetBytes("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"), 56);
					else
					{
						for (c_int i = 0; i < 1000 * 1000; i++)
							Sha.Av_Sha_Update(ctx, Encoding.Latin1.GetBytes("a"), 1);
					}

					Sha.Av_Sha_Final(ctx, digest);

					for (c_int i = 0; i < (lengths[j] >> 3); i++)
						printf("%02X", digest[i]);

					printf("\n");
				}

				switch (j)
				{
					case 0:
					{
						// Test vectors (from FIPS PUB 180-1)
						printf("A9993E36 4706816A BA3E2571 7850C26C 9CD0D89D\n" +
						       "84983E44 1C3BD26E BAAE4AA1 F95129E5 E54670F1\n" +
						       "34AA973C D4C4DAA4 F61EEB2B DBAD2731 6534016F\n");
						break;
					}

					case 1:
					{
						// Test vectors (from FIPS PUB 180-2 Appendix A)
						printf("23097d22 3405d822 8642a477 bda255b3 2aadbce4 bda0b3f7 e36c9da7\n" +
						       "75388b16 512776cc 5dba5da1 fd890150 b0c6455c b4f58b19 52522525\n" +
						       "20794655 980c91d8 bbb4c1ea 97618a4b f03f4258 1948b2ee 4ee7ad67\n");
						break;
					}

					case 2:
					{
						// Test vectors (from FIPS PUB 180-2)
						printf("ba7816bf 8f01cfea 414140de 5dae2223 b00361a3 96177a9c b410ff61 f20015ad\n" +
						       "248d6a61 d20638b8 e5c02693 0c3e6039 a33ce459 64ff2167 f6ecedd4 19db06c1\n" +
						       "cdc76e5c 9914fb92 81a1c7e2 84d73e67 f1809a48 a497200e 046d39cc c7112cd0\n");
						break;
					}
				}
			}

			Mem.Av_Free(ctx);

			return 0;
		}
	}
}
