/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.FFmpegTest;

namespace Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.LibAvUtil.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class Test_BPrint : TestBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test()
		{
			RunTest("bprint");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override c_int DoTest()
		{
			AVBPrint b;
			CPointer<char> buf = new CPointer<char>(256);
			tm testTime = new tm { tm_Year = 100, tm_Mon = 11, tm_MDay = 20 };

			BPrint.Av_BPrint_Init(out b, 0, BPrint.Av_BPrint_Size_Unlimited);
			BPrint_Pascal(b, 5);
			printf("Short text in unlimited buffer: %u/%u\n", (c_uint)CString.strlen(b.Str), b.Len);
			printf("%s\n", b.Str);
			BPrint.Av_BPrint_Finalize(b, out _);

			BPrint.Av_BPrint_Init(out b, 0, BPrint.Av_BPrint_Size_Unlimited);
			BPrint_Pascal(b, 25);
			printf("Long text in unlimited buffer: %u/%u\n", (c_uint)CString.strlen(b.Str), b.Len);
			BPrint.Av_BPrint_Finalize(b, out _);

			BPrint.Av_BPrint_Init(out b, 0, 2048);
			BPrint_Pascal(b, 25);
			printf("Long text in limited buffer: %u/%u\n", (c_uint)CString.strlen(b.Str), b.Len);
			BPrint.Av_BPrint_Finalize(b, out _);

			BPrint.Av_BPrint_Init(out b, 0, BPrint.Av_BPrint_Size_Automatic);
			BPrint_Pascal(b, 5);
			printf("Short text in automatic buffer: %u/%u\n", (c_uint)CString.strlen(b.Str), b.Len);
			BPrint.Av_BPrint_Finalize(b, out _);

			BPrint.Av_BPrint_Init(out b, 0, BPrint.Av_BPrint_Size_Automatic);
			BPrint_Pascal(b, 25);
			printf("Long text in automatic buffer: %u/%u\n", (c_uint)CString.strlen(b.Str) / 8 * 8, b.Len);
			BPrint.Av_BPrint_Finalize(b, out _);

			BPrint.Av_BPrint_Init(out b, 0, BPrint.Av_BPrint_Size_Count_Only);
			BPrint_Pascal(b, 25);
			printf("Long text count only buffer: %u/%u\n", (c_uint)CString.strlen(b.Str), b.Len);
			BPrint.Av_BPrint_Finalize(b, out _);

			BPrint.Av_BPrint_Init_For_Buffer(out b, buf, (c_uint)buf.Length);
			BPrint_Pascal(b, 25);
			printf("Long text count only buffer: %u/%u\n", (c_uint)CString.strlen(buf), b.Len);

			BPrint.Av_BPrint_Init(out b, 0, BPrint.Av_BPrint_Size_Unlimited);
			BPrint.Av_BPrint_Strftime(b, "%Y-%m-%d", testTime);
			printf("strftime full: %u/%u \"%s\"\n", (c_uint)CString.strlen(buf), b.Len, b.Str);
			BPrint.Av_BPrint_Finalize(b, out _);

			BPrint.Av_BPrint_Init(out b, 0, 8);
			BPrint.Av_BPrint_Strftime(b, "%Y-%m-%d", testTime);
			printf("strftime truncated: %u/%u \"%s\"\n", (c_uint)CString.strlen(buf), b.Len, b.Str);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void BPrint_Pascal(AVBPrint b, c_uint size)
		{
			c_uint[] p = new c_uint[42];

			p[0] = 1;
			BPrint.Av_BPrintf(b, "%8d\n", 1);

			for (c_uint i = 1; i <= size; i++)
			{
				p[i] = 1;

				for (c_uint j = i - 1; j > 0; j--)
					p[j] = p[j] + p[j - 1];

				for (c_uint j = 0; j <= i; j++)
					BPrint.Av_BPrintf(b, "%8d", p[j]);

				BPrint.Av_BPrintf(b, "\n");
			}
		}
	}
}
