/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Linq;
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
	public class Test_Dict : TestBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Dict_()
		{
			RunTest("dict");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override c_int DoTest()
		{
			AvDictionary dict = null;

			printf("Testing av_dict_get_string() and av_dict_parse_string()\n");

			Dict.Av_Dict_Get_String(dict, out CPointer<char> buffer, '=', ',');
			printf("%s\n", buffer);
			Mem.Av_FreeP(ref buffer);

			Dict.Av_Dict_Set(ref dict, "aaa", "aaa", AvDict.None);
			Dict.Av_Dict_Set(ref dict, "b,b", "bbb", AvDict.None);
			Dict.Av_Dict_Set(ref dict, "c=c", "ccc", AvDict.None);
			Dict.Av_Dict_Set(ref dict, "ddd", "d,d", AvDict.None);
			Dict.Av_Dict_Set(ref dict, "eee", "e=e", AvDict.None);
			Dict.Av_Dict_Set(ref dict, "f,f", "f=f", AvDict.None);
			Dict.Av_Dict_Set(ref dict, "g=g", "g,g", AvDict.None);
			Test_Separators(dict, ',', '=');
			Dict.Av_Dict_Free(ref dict);

			Dict.Av_Dict_Set(ref dict, "aaa", "aaa", AvDict.None);
			Dict.Av_Dict_Set(ref dict, "bbb", "bbb", AvDict.None);
			Dict.Av_Dict_Set(ref dict, "ccc", "ccc", AvDict.None);
			Dict.Av_Dict_Set(ref dict, "\\,=\'\"", "\\,=\'\"", AvDict.None);
			Test_Separators(dict, '"', '=');
			Test_Separators(dict, '\'', '=');
			Test_Separators(dict, ',', '"');
			Test_Separators(dict, ',', '\'');
			Test_Separators(dict, '\'', '"');
			Test_Separators(dict, '"', '\'');
			Dict.Av_Dict_Free(ref dict);

			printf("\nTesting av_dict_set()\n");

			Dict.Av_Dict_Set(ref dict, "a", "a", AvDict.None);
			Dict.Av_Dict_Set(ref dict, "b", Mem.Av_StrDup("b".ToCharPointer()), AvDict.Dont_Strdup_Val);
			Dict.Av_Dict_Set(ref dict, Mem.Av_StrDup("c".ToCharPointer()), "c".ToCharPointer(), AvDict.Dont_Strdup_Key);
			Dict.Av_Dict_Set(ref dict, Mem.Av_StrDup("d".ToCharPointer()), Mem.Av_StrDup("d".ToCharPointer()), AvDict.Dont_Strdup_Key | AvDict.Dont_Strdup_Val);
			Dict.Av_Dict_Set(ref dict, "e", "e", AvDict.Dont_Overwrite);
			Dict.Av_Dict_Set(ref dict, "e", "f", AvDict.Dont_Overwrite);
			Dict.Av_Dict_Set(ref dict, "f", "f", AvDict.None);
			Dict.Av_Dict_Set(ref dict, "e", (CPointer<char>)null, AvDict.None);
			Dict.Av_Dict_Set(ref dict, "ff", "f", AvDict.None);
			Dict.Av_Dict_Set(ref dict, "ff", "f", AvDict.Append);

			if (Dict.Av_Dict_Get(dict, (CPointer<char>)null, AvDict.None).FirstOrDefault() != null)
				printf("av_dict_get() does not correctly handle NULL key.\n");

			foreach (AvDictionaryEntry e in Dict_Iterate(dict))
				printf("%s %s\n", e.Key, e.Value);

			Dict.Av_Dict_Free(ref dict);

			if ((Dict.Av_Dict_Set(ref dict, null, "a", AvDict.None) >= 0) ||
				(Dict.Av_Dict_Set(ref dict, null, "b", AvDict.None) >= 0) ||
				(Dict.Av_Dict_Set(ref dict, (CPointer<char>)null, null, AvDict.Dont_Strdup_Key) >= 0) ||
				(Dict.Av_Dict_Set(ref dict, (CPointer<char>)null, Mem.Av_StrDup("b".ToCharPointer()), AvDict.Dont_Strdup_Val) >= 0) ||
				(Dict.Av_Dict_Count(dict) != 0))
			{
				printf("av_dict_get() does not correctly handle NULL key.\n");
			}

			foreach (AvDictionaryEntry e in Dict_Iterate(dict))
				printf("'%s' '%s'\n", e.Key, e.Value);

			Dict.Av_Dict_Free(ref dict);

			printf("\nTesting av_dict_set_int()\n");

			Dict.Av_Dict_Set_Int(ref dict, "1", 1, AvDict.Dont_Strdup_Val);
			Dict.Av_Dict_Set_Int(ref dict, Mem.Av_StrDup("2".ToCharPointer()), 2, AvDict.Dont_Strdup_Key);
			Dict.Av_Dict_Set_Int(ref dict, Mem.Av_StrDup("3".ToCharPointer()), 3, AvDict.Dont_Strdup_Key | AvDict.Dont_Strdup_Val);
			Dict.Av_Dict_Set_Int(ref dict, "4", 4, AvDict.None);
			Dict.Av_Dict_Set_Int(ref dict, "5", 5, AvDict.Dont_Overwrite);
			Dict.Av_Dict_Set_Int(ref dict, "5", 6, AvDict.Dont_Overwrite);
			Dict.Av_Dict_Set_Int(ref dict, "12", 1, AvDict.None);
			Dict.Av_Dict_Set_Int(ref dict, "12", 2, AvDict.Append);

			foreach (AvDictionaryEntry e in Dict_Iterate(dict))
				printf("%s %s\n", e.Key, e.Value);

			Dict.Av_Dict_Free(ref dict);

			printf("\nTesting av_dict_set() with existing AVDictionaryEntry.key as key\n");

			if (Dict.Av_Dict_Set(ref dict, "key", "old", AvDict.None) < 0)
				return 1;

			AvDictionaryEntry e1 = Dict.Av_Dict_Get(dict, "key", AvDict.None).First();

			if (Dict.Av_Dict_Set(ref dict, e1.Key, "new val OK".ToCharPointer(), AvDict.None) < 0)
				return 1;

			e1 = Dict.Av_Dict_Get(dict, "key", AvDict.None).First();
			printf("%s\n", e1.Value);

			if (Dict.Av_Dict_Set(ref dict, e1.Key, e1.Value, AvDict.None) < 0)
				return 1;

			e1 = Dict.Av_Dict_Get(dict, "key", AvDict.None).First();
			printf("%s\n", e1.Value);

			Dict.Av_Dict_Free(ref dict);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private IEnumerable<AvDictionaryEntry> Dict_Iterate(AvDictionary m)
		{
			AvDictionaryEntry[] dict_Get = Dict.Av_Dict_Get(m, string.Empty, AvDict.Ignore_Suffix).ToArray();
			c_int i = 0;

			foreach (AvDictionaryEntry e in Dict.Av_Dict_Iterate(m))
			{
				if (!ReferenceEquals(dict_Get[i++], e))
					printf("Iterating with av_dict_iterate() yields a different result than iterating with av_dict_get() and AV_DICT_IGNORE_SUFFIX\n");

				yield return e;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Print_Dict(AvDictionary m)
		{
			foreach (AvDictionaryEntry t in Dict.Av_Dict_Iterate(m))
				printf("%s %s   ", t.Key, t.Value);

			printf("\n");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Separators(AvDictionary m, char pair, char val)
		{
			AvDictionary dict = null;
			char[] pairs = [ pair, '\0' ];
			char[] vals = [ val, '\0' ];

			Dict.Av_Dict_Copy(ref dict, m, AvDict.None);
			Print_Dict(dict);

			Dict.Av_Dict_Get_String(dict, out CPointer<char> buffer, val, pair);
			printf("%s\n", buffer);
			Dict.Av_Dict_Free(ref dict);

			c_int ret = Dict.Av_Dict_Parse_String(ref dict, buffer, vals, pairs, AvDict.None);
			printf("ret %d\n", ret);
			Mem.Av_FreeP(ref buffer);

			Print_Dict(dict);
			Dict.Av_Dict_Free(ref dict);
		}
	}
}
