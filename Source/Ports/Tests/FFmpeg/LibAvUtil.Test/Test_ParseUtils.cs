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
	public class Test_ParseUtils : TestBase
	{
		private class Fmt_TimeSpec_Entry
		{
			public Fmt_TimeSpec_Entry(string fmt, string timeSpec)
			{
				Fmt = fmt.ToCharPointer();
				TimeSpec = timeSpec.ToCharPointer();
			}

			public CPointer<char> Fmt { get; }
			public CPointer<char> TimeSpec { get; }
		}

		private uint32_t randomV = Macros.MkTag('L', 'A', 'V', 'U');

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_ParseUtils_()
		{
			RunTest("parseutils");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override c_int DoTest()
		{
			printf("Testing av_parse_video_rate()\n");
			Test_Av_Parse_Video_Rate();

			printf("\nTesting av_parse_color()\n");
			Test_Av_Parse_Color();

			printf("\nTesting av_small_strptime()\n");
			Test_Av_Small_Strptime();

			printf("\nTesting av_parse_time()\n");
			Test_Av_Parse_Time();

			printf("\nTesting av_get_known_color_name()\n");
			Test_Av_Get_Known_Color_Name();

			printf("\nTesting av_find_info_tag()\n");
			Test_Av_Find_Info_Tag();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint32_t Av_Get_Random_Seed_Deterministic()
		{
			return randomV = (randomV * 1664525) + 1013904223;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Av_Parse_Video_Rate()
		{
			string[] rates =
			[
				"-inf",
				"inf",
				"nan",
				"123/0",
				"-123 / 0",
				"",
				"/",
				" 123  /  321",
				"foo/foo",
				"foo/1",
				"1/foo",
				"0/0",
				"/0",
				"1/",
				"1",
				"0",
				"-123/123",
				"-foo",
				"123.23",
				".23",
				"-.23",
				"-0.234",
				"-0.0000001",
				"  21332.2324   ",
				" -21332.2324   "
			];

			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(rates); i++)
			{
				c_int ret = ParseUtils.Av_Parse_Video_Rate(out AvRational q, rates[i].ToCharPointer());
				printf("'%s' -> %d/%d %s\n", rates[i], q.Num, q.Den, ret != 0 ? "ERROR" : "OK");
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Av_Parse_Color()
		{
			CPointer<uint8_t> rgba = new CPointer<uint8_t>(4);

			string[] color_Names =
			[
				"bikeshed",
				"RaNdOm",
				"foo",
				"red",
				"Red ",
				"RED",
				"Violet",
				"Yellow",
				"Red",
				"0x000000",
				"0x0000000",
				"0xff000000",
				"0x3e34ff",
				"0x3e34ffaa",
				"0xffXXee",
				"0xfoobar",
				"0xffffeeeeeeee",
				"#ff0000",
				"#ffXX00",
				"ff0000",
				"ffXX00",
				"red@foo",
				"random@10",
				"0xff0000@1.0",
				"red@",
				"red@0xfff",
				"red@0xf",
				"red@2",
				"red@0.1",
				"red@-1",
				"red@0.5",
				"red@1.0",
				"red@256",
				"red@10foo",
				"red@-1.0",
				"red@-0.0"
			];

			Log.Av_Log_Set_Level(Log.Av_Log_Debug);
			UnitTest.SetRandomMethod(Av_Get_Random_Seed_Deterministic);

			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(color_Names); i++)
			{
				if (ParseUtils.Av_Parse_Color(rgba, color_Names[i].ToCharPointer(), -1, null) >= 0)
					printf("%s -> R(%d) G(%d) B(%d) A(%d)\n", color_Names[i], rgba[0], rgba[1], rgba[2], rgba[3]);
				else
					printf("%s -> error\n", color_Names[i]);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Av_Small_Strptime()
		{
			tm tm = new tm();

			Fmt_TimeSpec_Entry[] fmt_TimeSpec_Entries =
			[
				new Fmt_TimeSpec_Entry("%Y-%m-%d", "2012-12-21"),
				new Fmt_TimeSpec_Entry("%Y - %m - %d", "2012-12-21"),
				new Fmt_TimeSpec_Entry("%Y-%m-%d %H:%M:%S", "2012-12-21 20:12:21"),
				new Fmt_TimeSpec_Entry("  %Y - %m - %d %H : %M : %S", "   2012 - 12 -  21   20 : 12 : 21"),
				new Fmt_TimeSpec_Entry("  %Y - %b - %d %H : %M : %S", "   2012 - nOV -  21   20 : 12 : 21"),
				new Fmt_TimeSpec_Entry("  %Y - %B - %d %H : %M : %S", "   2012 - nOVemBeR -  21   20 : 12 : 21"),
				new Fmt_TimeSpec_Entry("  %Y - %B%d %H : %M : %S", "   2012 - may21   20 : 12 : 21"),
				new Fmt_TimeSpec_Entry("  %Y - %B%d %H : %M : %S", "   2012 - mby21   20 : 12 : 21"),
				new Fmt_TimeSpec_Entry("  %Y - %B - %d %H : %M : %S", "   2012 - JunE -  21   20 : 12 : 21"),
				new Fmt_TimeSpec_Entry("  %Y - %B - %d %H : %M : %S", "   2012 - Jane -  21   20 : 12 : 21"),
				new Fmt_TimeSpec_Entry("  %Y - %B - %d %H : %M : %S", "   2012 - January -  21   20 : 12 : 21")
			];

			Log.Av_Log_Set_Level(Log.Av_Log_Debug);

			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(fmt_TimeSpec_Entries); i++)
			{
				Fmt_TimeSpec_Entry e = fmt_TimeSpec_Entries[i];
				printf("fmt:'%s' spec:'%s' -> ", e.Fmt, e.TimeSpec);

				CPointer<char> p = ParseUtils.Av_Small_Strptime(e.TimeSpec, e.Fmt, ref tm);

				if (p.IsNotNull)
					printf("%04d-%02d-%2d %02d:%02d:%02d\n", 1900 + tm.tm_Year, tm.tm_Mon + 1, tm.tm_MDay, tm.tm_Hour, tm.tm_Min, tm.tm_Sec);
				else
					printf("error\n");
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Av_Parse_Time()
		{
			CPointer<char> tzStr = "TZ=CET-1".ToCharPointer();

			string[] time_String =
			[
				"now",
				"12:35:46",
				"2000-12-20 0:02:47.5z",
				"2012 - 02-22  17:44:07",
				"2000-12-20T010247.6",
				"2000-12-12 1:35:46+05:30",
				"2002-12-12 22:30:40-02"
			];

			string[] duration_String =
			[
				"2:34:56.79",
				"-1:23:45.67",
				"42.1729",
				"-1729.42",
				"12:34",
				"2147483648s",
				"4294967296ms",
				"8589934592us",
				"9223372036854775808us",
			];

			Log.Av_Log_Set_Level(Log.Av_Log_Debug);
			CEnvironment.putenv(tzStr);

			printf("(now is 2012-03-17 09:14:13.2 +0100, local time is UTC+1)\n");

			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(time_String); i++)
			{
				printf("%-24s -> ", time_String[i]);

				if (ParseUtils.Av_Parse_Time(out int64_t tv, time_String[i].ToCharPointer(), 0) != 0)
					printf("error\n");
				else
				{
					time_t tvi = tv / 1000000;
					tm tm = CTime.gmtime(tvi);

					printf("%14lld.%06d = %04d-%02d-%02dT%02d:%02d:%02dZ\n",
						tv / 1000000, (c_int)(tv % 1000000),
						1900 + tm.tm_Year, tm.tm_Mon + 1, tm.tm_MDay, tm.tm_Hour, tm.tm_Min, tm.tm_Sec);
				}
			}

			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(duration_String); i++)
			{
				printf("%-24s -> ", duration_String[i]);

				if (ParseUtils.Av_Parse_Time(out int64_t tv, duration_String[i].ToCharPointer(), 1) != 0)
					printf("error\n");
				else
					printf("%+21lld\n", tv);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Av_Get_Known_Color_Name()
		{
			foreach (var color in ParseUtils.Av_Get_Known_Color_Name())
				printf("%s -> R(%d) G(%d) B(%d) A(%d)\n", color.Name, color.rgba[0], color.rgba[1], color.rgba[2], color.rgba[3]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Av_Find_Info_Tag()
		{
			CPointer<char> args = "?tag1=val1&tag2=val2&tag3=val3&tag41=value 41&tag42=random1".ToCharPointer();
			string[] tags =
			[
				"tag1", "tag2", "tag3", "tag4", "tag41", "41", "random1"
			];
			CPointer<char> buff = new CPointer<char>(16);

			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(tags); ++i)
			{
				if (ParseUtils.Av_Find_Info_Tag(buff, buff.Length, tags[i].ToCharPointer(), args) != 0)
					printf("%d. %s found: %s\n", i, tags[i], buff);
				else
					printf("%d. %s not found\n", i, tags[i]);
			}
		}
	}
}
