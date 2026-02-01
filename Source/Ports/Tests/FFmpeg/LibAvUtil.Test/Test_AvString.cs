/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.FFmpegTest;

namespace Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.LibAvUtil.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class Test_AvString : TestBase
	{
		private readonly string[] strings =
		[
			"''",
			"",
			":",
			"\\",
			"'",
			"    ''    :",
			"    ''  ''  :",
			"foo   '' :",
			"'foo'",
			"foo     ",
			"  '  foo  '  ",
			"foo\\",
			"foo':  blah:blah",
			"foo\\:  blah:blah",
			"foo\'",
			"'foo :  '  :blahblah",
			"\\ :blah",
			"     foo",
			"      foo       ",
			"      foo     \\ ",
			"foo ':blah",
			" foo   bar    :   blahblah",
			"\\f\\o\\o",
			"'foo : \\ \\  '   : blahblah",
			"'\\fo\\o:': blahblah",
			"\\'fo\\o\\:':  foo  '  :blahblah"
		];

		private readonly CPointer<char> haystack = "Education consists mainly in what we have unlearned.".ToCharPointer();
		private readonly string[] needle = [ "learned.", "unlearned.", "Unlearned" ];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_AvString_()
		{
			RunTest("avstring");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override c_int DoTest()
		{
			Test_Av_Get_Token();
			Test_Av_Append_Path_Component();
			Test_Av_Strnstr();
			Test_Av_StriReplace();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Av_Get_Token()
		{
			printf("Testing av_get_token()\n");

			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(strings); i++)
			{
				CPointer<char> p = strings[i].ToCharPointer();
				printf($"|%s|", p);

				CPointer<char> q = AvString.Av_Get_Token(ref p, ":".ToCharPointer());
				printf($" -> |%s|", q);
				printf($" + |%s|\n", p);

				Mem.Av_Free(q);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Av_Append_Path_Component()
		{
			printf("Testing av_append_path_component()\n");

			void Test_Append_Path_Component(string path, string component, string expected)
			{
				CPointer<char> fullPath = AvString.Av_Append_Path_Component(path.ToCharPointer(), component.ToCharPointer());
				printf("%s = %s\n", fullPath.IsNotNull ? fullPath : "(null)", expected);

				Mem.Av_Free(fullPath);
			}

			Test_Append_Path_Component(null, null, "(null)");
			Test_Append_Path_Component("path", null, "path");
			Test_Append_Path_Component(null, "comp", "comp");
			Test_Append_Path_Component("path", "comp", "path/comp");
			Test_Append_Path_Component("path/", "comp", "path/comp");
			Test_Append_Path_Component("path", "/comp", "path/comp");
			Test_Append_Path_Component("path/", "/comp", "path/comp");
			Test_Append_Path_Component("path/path2/", "/comp/comp2", "path/path2/comp/comp2");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Av_Strnstr()
		{
			void Test_Strnstr(CPointer<char> haystack, string needle, size_t hay_Length, CPointer<char> expected)
			{
				CPointer<char> ptr = AvString.Av_Strnstr(haystack, needle.ToCharPointer(), hay_Length);

				if (ptr != expected)
					printf("expected: %p, received %p\n", expected, ptr);
			}

			Test_Strnstr(haystack, needle[0], CString.strlen(haystack), haystack + 44);
			Test_Strnstr(haystack, needle[1], CString.strlen(haystack), haystack + 42);
			Test_Strnstr(haystack, needle[2], CString.strlen(haystack), null);
			Test_Strnstr(haystack, strings[1], CString.strlen(haystack), haystack);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Av_StriReplace()
		{
			void Test_StriReplace(CPointer<char> haystack, string needle, string expected)
			{
				CPointer<char> ptr = AvString.Av_StriReplace(haystack, needle.ToCharPointer(), "instead".ToCharPointer());

				if (ptr.IsNull)
					printf("error, received null pointer!\n");
				else
				{
					if (CString.strcmp(ptr, expected) != 0)
						printf("expected: %s, received %s", expected, ptr);

					Mem.Av_Free(ptr);
				}
			}

			Test_StriReplace(haystack, needle[0], "Education consists mainly in what we have uninstead");
			Test_StriReplace(haystack, needle[1], "Education consists mainly in what we have instead");
			Test_StriReplace(haystack, needle[2], "Education consists mainly in what we have instead.");
			Test_StriReplace(haystack, needle[1], "Education consists mainly in what we have instead");
		}
	}
}
