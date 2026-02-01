/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;
using Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.FFmpegTest;
using Version = Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Version;

namespace Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.LibAvUtil.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class Test_Opt : TestBase
	{
		private class TestContext : AvClass, IOptionContext
		{
			public AvClass Av_Class => this;
			public ChildContext Child;
			public c_int Num = 0;
			public c_uint UNum = 0;
			public c_int Toggle = 0;
			public CPointer<char> String = null;
			public c_int Flags = 0;
			public AvRational Rational = new AvRational();
			public AvRational Video_Rate = new AvRational();
			public SizeInfo Size = new SizeInfo();
			public AvPixelFormat Pix_Fmt = AvPixelFormat.None;
			public AvSampleFormat Sample_Fmt = AvSampleFormat.None;
			public int64_t Duration = 0;
			public ColorInfo Color = new ColorInfo();
			public readonly AvChannelLayout Channel_Layout = new AvChannelLayout();
			public PtrInfo<uint8_t> Binary = new PtrInfo<uint8_t>();
			public PtrInfo<uint8_t> Binary1 = new PtrInfo<uint8_t>();
			public PtrInfo<uint8_t> Binary2 = new PtrInfo<uint8_t>();
			public int64_t Num64 = 0;
			public c_float Flt = 0;
			public c_double Dbl = 0;
			public CPointer<char> Escape = null;
			public c_int Bool1 = 0;
			public c_int Bool2 = 0;
			public c_int Bool3 = 0;
			public AvDictionary Dict1 = null;
			public AvDictionary Dict2 = null;
			public ArrayInfo<c_int> Array_Int = new ArrayInfo<c_int>();
			public ArrayInfo<CPointer<char>> Array_Str = new ArrayInfo<CPointer<char>>();
			public ArrayInfo<AvDictionary> Array_Dict = new ArrayInfo<AvDictionary>();
		}

		private class ChildContext : AvClass, IOptionContext
		{
			public AvClass Av_Class => this;
			public int64_t Child_Num64 = 0;
			public c_int Child_Num = 0;
		}

		private readonly AvClass test_Class = new AvClass
		{
			Class_Name = "TestContext".ToCharPointer(),
			Item_Name = Test_Get_Name,
			Option = test_Options,
			Child_Next = Test_Child_Next,
			Version = Version.Version_Int,
		};

		[Flags]
		private enum TestFlag
		{
			Cool = 1,
			Lame = 2,
			Mu = 4,
		}

		private static readonly AvOptionArrayDef[] array_Str =
		[
			new AvOptionArrayDef { Sep = '|', Def = "str0|str\\|1|str\\\\2".ToCharPointer() }
		];

		private static readonly AvOptionArrayDef[] array_Dict =
		[
			new AvOptionArrayDef { Def = "k00=v\\\\\\\\00:k01=v\\,01,k10=v\\\\=1\\\\:0".ToCharPointer() }
		];

		private static readonly AvOption[] test_Options =
		[
			new AvOption("num", "set num", nameof(TestContext.Num), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = 0 }, -1,100, AvOptFlag.Encoding_Param),
			new AvOption("unum", "set unum", nameof(TestContext.UNum), AvOptionType.UInt, new AvOption.DefaultValueUnion { I64 = 1U << 31 }, 0, 1U << 31, AvOptFlag.Encoding_Param),
			new AvOption("toggle", "set toggle", nameof(TestContext.Toggle), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = 1 }, 0, 1, AvOptFlag.Encoding_Param),
			new AvOption("rational", "set rational", nameof(TestContext.Rational), AvOptionType.Rational, new AvOption.DefaultValueUnion{ Dbl = 1 }, 0, 10, AvOptFlag.Encoding_Param),
			new AvOption("string", "set string", nameof(TestContext.String), AvOptionType.String, new AvOption.DefaultValueUnion { Str = "default".ToCharPointer() }, 0, 255, AvOptFlag.Encoding_Param),
			new AvOption("escape", "set escape str", nameof(TestContext.Escape), AvOptionType.String, new AvOption.DefaultValueUnion { Str = "\\=,".ToCharPointer() }, 0, 255, AvOptFlag.Encoding_Param),
			new AvOption("flags", "set flags", nameof(TestContext.Flags), AvOptionType.Flags, new AvOption.DefaultValueUnion { I64 = 1 }, 0, c_int.MaxValue, AvOptFlag.Encoding_Param, "flags"),
			new AvOption("cool", "set cool flag", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)TestFlag.Cool }, c_int.MinValue, c_int.MaxValue, AvOptFlag.Encoding_Param, "flags"),
			new AvOption("lame", "set lame flag", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)TestFlag.Lame }, c_int.MinValue, c_int.MaxValue, AvOptFlag.Encoding_Param, "flags"),
			new AvOption("mu", "set mu flag", null, AvOptionType.Const, new AvOption.DefaultValueUnion { I64 = (int64_t)TestFlag.Mu }, c_int.MinValue, c_int.MaxValue, AvOptFlag.Encoding_Param, "flags"),
			new AvOption("size", "set size", nameof(TestContext.Size), AvOptionType.Image_Size, new AvOption.DefaultValueUnion { Str = "200x300".ToCharPointer() }, 0, 0, AvOptFlag.Encoding_Param),
			new AvOption("pix_fmt", "set pixfmt", nameof(TestContext.Pix_Fmt), AvOptionType.Pixel_Fmt, new AvOption.DefaultValueUnion { I64 = (int64_t)AvPixelFormat._0BGR }, -1, c_int.MaxValue, AvOptFlag.Encoding_Param),
			new AvOption("sample_fmt", "set samplefmt", nameof(TestContext.Sample_Fmt), AvOptionType.Sample_Fmt, new AvOption.DefaultValueUnion { I64 = (int64_t)AvSampleFormat.S16 }, -1, c_int.MaxValue, AvOptFlag.Encoding_Param),
			new AvOption("video_rate", "set videorate", nameof(TestContext.Video_Rate), AvOptionType.Video_Rate, new AvOption.DefaultValueUnion { Str = "25".ToCharPointer() }, 0, c_int.MaxValue, AvOptFlag.Encoding_Param),
			new AvOption("duration", "set duration", nameof(TestContext.Duration), AvOptionType.Duration, new AvOption.DefaultValueUnion { I64 = 1000 }, 0, int64_t.MaxValue, AvOptFlag.Encoding_Param),
			new AvOption("color", "set color", nameof(TestContext.Color), AvOptionType.Color, new AvOption.DefaultValueUnion { Str = "pink".ToCharPointer() }, 0, 0, AvOptFlag.Encoding_Param),
			new AvOption("cl", "set channel layout", nameof(TestContext.Channel_Layout), AvOptionType.ChLayout, new AvOption.DefaultValueUnion { Str = "hexagonal".ToCharPointer() }, 0, 0, AvOptFlag.Encoding_Param),
			new AvOption("bin", "set binary value", nameof(TestContext.Binary), AvOptionType.Binary, new AvOption.DefaultValueUnion { Str="62696e00".ToCharPointer() }, 0, 0, AvOptFlag.Encoding_Param),
			new AvOption("bin1", "set binary value", nameof(TestContext.Binary1), AvOptionType.Binary, new AvOption.DefaultValueUnion { Str = null }, 0, 0, AvOptFlag.Encoding_Param),
			new AvOption("bin2", "set binary value", nameof(TestContext.Binary2), AvOptionType.Binary, new AvOption.DefaultValueUnion { Str = CString.Empty }, 0, 0, AvOptFlag.Encoding_Param),
			new AvOption("num64", "set num 64bit", nameof(TestContext.Num64), AvOptionType.Int64, new AvOption.DefaultValueUnion { I64 = 1L << 32 }, -1, 1L << 32, AvOptFlag.Encoding_Param),
			new AvOption("flt", "set float", nameof(TestContext.Flt), AvOptionType.Float, new AvOption.DefaultValueUnion { Dbl = 1.0f / 3 }, 0, 100, AvOptFlag.Encoding_Param),
			new AvOption("dbl", "set double", nameof(TestContext.Dbl), AvOptionType.Double, new AvOption.DefaultValueUnion { Dbl = 1.0 / 3 }, 0, 100, AvOptFlag.Encoding_Param),
			new AvOption("bool1", "set boolean value", nameof(TestContext.Bool1), AvOptionType.Bool, new AvOption.DefaultValueUnion { I64 = -1 }, -1, 1, AvOptFlag.Encoding_Param),
			new AvOption("bool2", "set boolean value", nameof(TestContext.Bool2), AvOptionType.Bool, new AvOption.DefaultValueUnion { I64 = 1 }, -1, 1, AvOptFlag.Encoding_Param),
			new AvOption("bool3", "set boolean value", nameof(TestContext.Bool3), AvOptionType.Bool, new AvOption.DefaultValueUnion { I64 = 0 }, 0, 1, AvOptFlag.Encoding_Param),
			new AvOption("dict1", "set dictionary value", nameof(TestContext.Dict1), AvOptionType.Dict, new AvOption.DefaultValueUnion { Str = null }, 0, 0, AvOptFlag.Encoding_Param),
			new AvOption("dict2", "set dictionary value", nameof(TestContext.Dict2), AvOptionType.Dict, new AvOption.DefaultValueUnion { Str = "happy=':-)'".ToCharPointer() }, 0, 0, AvOptFlag.Encoding_Param),
			new AvOption("array_int", "array of ints", nameof(TestContext.Array_Int), AvOptionType.Int | AvOptionType.Array, new AvOption.DefaultValueUnion(), 0, c_int.MaxValue, AvOptFlag.Runtime_Param),
			new AvOption("array_str", "array of strings", nameof(TestContext.Array_Str), AvOptionType.String | AvOptionType.Array, new AvOption.DefaultValueUnion { Arr = array_Str }, 0, 0, AvOptFlag.Runtime_Param),
			new AvOption("array_dict", "array of dicts", nameof(TestContext.Array_Dict), AvOptionType.Dict | AvOptionType.Array, new AvOption.DefaultValueUnion { Arr = array_Dict }, 0, 0, AvOptFlag.Runtime_Param),
			new AvOption()
		];

		private readonly AvClass child_Class = new AvClass
		{
			Class_Name = "ChildContext".ToCharPointer(),
			Item_Name = Child_Get_Name,
			Option = child_Options,
			Version = Version.Version_Int,
		};

		private static readonly AvOption[] child_Options =
		[
			new AvOption("child_num64", "set num 64bit", nameof(ChildContext.Child_Num64), AvOptionType.Int64, new AvOption.DefaultValueUnion { I64 = 0 }, 0, 100, AvOptFlag.Encoding_Param),
			new AvOption("child_num", "set child_num", nameof(ChildContext.Child_Num), AvOptionType.Int, new AvOption.DefaultValueUnion { I64 = 1 }, 0, 100, AvOptFlag.Encoding_Param),
			new AvOption()
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test()
		{
			RunTest("opt");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override c_int DoTest()
		{
			Test_Default_Values();
			Test_Set_To_Default();
			Test_Get_Set();
			Test_Get_Array();
			Test_Serialize();
			Test_Set_Options();
			Test_Set_Options_From_String();
			Test_Find2();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static CPointer<char> Test_Get_Name(IClass ctx)
		{
			return "test".ToCharPointer();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IEnumerable<IOptionContext> Test_Child_Next(IOptionContext obj)
		{
			TestContext test_Ctx = (TestContext)obj;

			yield return test_Ctx.Child;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static CPointer<char> Child_Get_Name(IClass ctx)
		{
			return "child".ToCharPointer();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Log_Callback_Help(IClass ptr, c_int level, CPointer<char> fmt, params object[] args)
		{
			printf(fmt, args);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Default_Values()
		{
			Log.Av_Log_Set_Level(Log.Av_Log_Debug);
			Log.Av_Log_Set_Callback(Log_Callback_Help);

			printf("Testing default values\n");

			TestContext test_Ctx = new TestContext();
			test_Class.CopyTo(test_Ctx.Av_Class);

			Opt.Av_Opt_Set_Defaults(test_Ctx);

			printf("num=%d\n", test_Ctx.Num);
			printf("unum=%u\n", test_Ctx.UNum);
			printf("toggle=%d\n", test_Ctx.Toggle);
			printf("string=%s\n", test_Ctx.String);
			printf("escape=%s\n", test_Ctx.Escape);
			printf("flags=%d\n", test_Ctx.Flags);
			printf("rational=%d/%d\n", test_Ctx.Rational.Num, test_Ctx.Rational.Den);
			printf("video_rate=%d/%d\n", test_Ctx.Video_Rate.Num, test_Ctx.Video_Rate.Den);
			printf("width=%d height=%d\n", test_Ctx.Size.Width, test_Ctx.Size.Height);
			printf("pix_fmt=%s\n", PixDesc.Av_Get_Pix_Fmt_Name(test_Ctx.Pix_Fmt));
			printf("sample_fmt=%s\n", SampleFmt.Av_Get_Sample_Fmt_Name(test_Ctx.Sample_Fmt));
			printf("duration=%lld\n", test_Ctx.Duration);
			printf("color=%d %d %d %d\n", test_Ctx.Color.Color[0], test_Ctx.Color.Color[1], test_Ctx.Color.Color[2], test_Ctx.Color.Color[3]);
			printf("channel_layout=%lld=%lld\n", test_Ctx.Channel_Layout.U.Mask, (int64_t)AvChannelMask.Hexagonal);

			if (test_Ctx.Binary.Ptr.IsNotNull)
				printf("binary=%x %x %x %x\n", test_Ctx.Binary.Ptr[0], test_Ctx.Binary.Ptr[1], test_Ctx.Binary.Ptr[2], test_Ctx.Binary.Ptr[3]);

			printf("binary_size=%d\n", test_Ctx.Binary.Len);
			printf("num64=%lld\n", test_Ctx.Num64);
			printf("flt=%.6f\n", test_Ctx.Flt);
			printf("dbl=%.6f\n", test_Ctx.Dbl);

			for (c_uint i = 0; i < test_Ctx.Array_Str.Count; i++)
				printf("array_str[%u]=%s\n", i, test_Ctx.Array_Str.Array[i]);

			for (c_uint i = 0; i < test_Ctx.Array_Dict.Count; i++)
			{
				AvDictionary d = test_Ctx.Array_Dict.Array[i];

				foreach (AvDictionaryEntry e in Dict.Av_Dict_Iterate(d))
					printf("array_dict[%u]: %s\t%s\n", i, e.Key, e.Value);
			}

			Opt.Av_Opt_Show2(test_Ctx, null, AvOptFlag.All, AvOptFlag.None);

			Opt.Av_Opt_Free(test_Ctx);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Set_To_Default()
		{
			printf("\nTesting av_opt_is_set_to_default()\n");

			TestContext test_Ctx = new TestContext();
			test_Class.CopyTo(test_Ctx.Av_Class);

			Log.Av_Log_Set_Level(Log.Av_Log_Quiet);

			foreach (AvOption o in Opt.Av_Opt_Next(test_Ctx))
			{
				c_int ret = Opt.Av_Opt_Is_Set_To_Default_By_Name(test_Ctx, o.Name, AvOptSearch.None);
				printf("name:%10s default:%d error:%s\n", o.Name, ret != 0 ? 1 : 0, ret < 0 ? Error.Av_Err2Str(ret) : string.Empty);
			}

			Opt.Av_Opt_Set_Defaults(test_Ctx);

			foreach (AvOption o in Opt.Av_Opt_Next(test_Ctx))
			{
				c_int ret = Opt.Av_Opt_Is_Set_To_Default_By_Name(test_Ctx, o.Name, AvOptSearch.None);
				printf("name:%10s default:%d error:%s\n", o.Name, ret != 0 ? 1 : 0, ret < 0 ? Error.Av_Err2Str(ret) : string.Empty);
			}

			Opt.Av_Opt_Free(test_Ctx);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Get_Set()
		{
			printf("\nTesting av_opt_get/av_opt_set()\n");

			TestContext test_Ctx = new TestContext();
			TestContext test2_Ctx = new TestContext();
			test_Class.CopyTo(test_Ctx.Av_Class);
			test_Class.CopyTo(test2_Ctx.Av_Class);

			Log.Av_Log_Set_Level(Log.Av_Log_Quiet);

			Opt.Av_Opt_Set_Defaults(test_Ctx);

			foreach (AvOption o in Opt.Av_Opt_Next(test_Ctx))
			{
				CPointer<char> value1 = null;
				CPointer<char> value2 = null;
				c_int ret1 = Error.Bug;
				c_int ret2 = Error.Bug;
				c_int ret3 = Error.Bug;

				if (o.Type == AvOptionType.Const)
					continue;

				ret1 = Opt.Av_Opt_Get(test_Ctx, o.Name, AvOptSearch.None, out value1);

				if (ret1 >= 0)
				{
					ret2 = Opt.Av_Opt_Set(test2_Ctx, o.Name, value1, AvOptSearch.None);

					if (ret2 >= 0)
					{
						ret3 = Opt.Av_Opt_Get(test2_Ctx, o.Name, AvOptSearch.None, out value2);
					}
				}

				printf("name: %-11s get: %-16s set: %-16s get: %-16s %s\n", o.Name,
					ret1 >= 0 ? value1 : Error.Av_Err2Str(ret1),
					ret2 >= 0 ? "OK" : Error.Av_Err2Str(ret2),
					ret3 >= 0 ? value2 : Error.Av_Err2Str(ret3),
					(ret1 >= 0) && (ret2 >= 0) && (ret3 >= 0) && (CString.strcmp(value1, value2) == 0) ? "OK" : "Mismatch");

				Mem.Av_Free(value1);
				Mem.Av_Free(value2);
			}

			// av_opt_set(NULL) with an array option resets it
			c_int ret = Opt.Av_Opt_Set(test_Ctx, "array_dict".ToCharPointer(), null, AvOptSearch.None);
			printf("av_opt_set(\"array_dict\", NULL) -> %d\n", ret);
			printf("array_dict=%sNULL; nb_array_dict=%u\n", test_Ctx.Array_Dict.Array.IsNotNull ? "non-" : string.Empty, test_Ctx.Array_Dict.Count);

			// av_opt_get() on an empty array should return a NULL string
			ret = Opt.Av_Opt_Get(test_Ctx, "array_dict".ToCharPointer(), AvOptSearch.Allow_Null, out CPointer<char> val);
			printf("av_opt_get(\"array_dict\") -> %s\n", val.IsNotNull ? val : "NULL");

			Opt.Av_Opt_Free(test_Ctx);
			Opt.Av_Opt_Free(test2_Ctx);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Get_Array()
		{
			printf("\nTesting av_opt_get_array()\n");

			c_int[] int_Array = [ 5, 0, 42, 137, c_int.MaxValue ];

			TestContext test_Ctx = new TestContext();
			test_Class.CopyTo(test_Ctx.Av_Class);

			CPointer<c_int> out_Int = new CPointer<c_int>(int_Array.Length);
			CPointer<c_double> out_Double = new CPointer<c_double>(int_Array.Length);
			CPointer<CPointer<char>> out_Str = new CPointer<CPointer<char>>(int_Array.Length);
			CPointer<AvDictionary> out_Dict = CMemory.callocObj<AvDictionary>(2);

			Log.Av_Log_Set_Level(Log.Av_Log_Quiet);

			Opt.Av_Opt_Set_Defaults(test_Ctx);

			test_Ctx.Array_Int.Array = Mem.Av_MemDup<c_int>(int_Array, (size_t)int_Array.Length);
			test_Ctx.Array_Int.Count = (c_uint)Macros.FF_Array_Elems(int_Array);

			// Retrieve as int
			c_int ret = Opt.Av_Opt_Get_Array<c_int, c_int>(test_Ctx, "array_int".ToCharPointer(), AvOptSearch.None, 1, 3, AvOptionType.Int, ref out_Int);
			printf("av_opt_get_array(\"array_int\", 1, 3, INT)=%d -> [ %d, %d, %d ]\n", ret, out_Int[0], out_Int[1], out_Int[2]);

			// Retrieve as double
			ret = Opt.Av_Opt_Get_Array<c_int, c_double>(test_Ctx, "array_int".ToCharPointer(), AvOptSearch.None, 3, 2, AvOptionType.Double, ref out_Double);
			printf("av_opt_get_array(\"array_int\", 3, 2, DOUBLE)=%d -> [ %.2f, %.2f ]\n", ret, out_Double[0], out_Double[1]);

			// Retrieve as str
			ret = Opt.Av_Opt_Get_Array<c_int, CPointer<char>>(test_Ctx, "array_int".ToCharPointer(), AvOptSearch.None, 0, 5, AvOptionType.String, ref out_Str);
			printf("av_opt_get_array(\"array_int\", 0, 5, STRING)=%d -> [ %s, %s, %s, %s, %s ]\n", ret, out_Str[0], out_Str[1], out_Str[2], out_Str[3], out_Str[4]);

			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(out_Str); i++)
				Mem.Av_FreeP(ref out_Str[i]);

			ret = Opt.Av_Opt_Get_Array<AvDictionary, AvDictionary>(test_Ctx, "array_dict".ToCharPointer(), AvOptSearch.None, 0, 2, AvOptionType.Dict, ref out_Dict);
			printf("av_opt_get_array(\"array_dict\", 0, 2, DICT)=%d\n", ret);

			for (c_int i = 0; i < test_Ctx.Array_Dict.Count; i++)
			{
				foreach (AvDictionaryEntry e in Dict.Av_Dict_Iterate(test_Ctx.Array_Dict.Array[i]))
				{
					AvDictionaryEntry e1 = Dict.Av_Dict_Get(out_Dict[i], e.Key, AvDict.None).FirstOrDefault();

					if ((e1 == null) || (CString.strcmp(e.Value, e1.Value) != 0))
						printf("mismatching dict entry %s: %s/%s\n", e.Key, e.Value, e1 != null ? e1.Value : "<missing>");
				}

				Dict.Av_Dict_Free(ref out_Dict[i]);
			}

			Opt.Av_Opt_Free(test_Ctx);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Serialize()
		{
			printf("\nTest av_opt_serialize()\n");

			TestContext test_Ctx = new TestContext();
			test_Class.CopyTo(test_Ctx.Av_Class);

			CPointer<char> buf;

			Log.Av_Log_Set_Level(Log.Av_Log_Quiet);

			Opt.Av_Opt_Set_Defaults(test_Ctx);

			if (Opt.Av_Opt_Serialize(test_Ctx, AvOptFlag.None, AvSerialize.None, out buf, '=', ',') >= 0)
			{
				printf("%s\n", buf);

				Opt.Av_Opt_Free(test_Ctx);

				test_Ctx.Clear();
				test_Class.CopyTo(test_Ctx.Av_Class);

				c_int ret = Opt.Av_Set_Options_String(test_Ctx, buf, "=".ToCharPointer(), ",".ToCharPointer());
				Mem.Av_Free(buf);

				if (ret < 0)
					printf("Error ret '%d'\n", ret);

				if (Opt.Av_Opt_Serialize(test_Ctx, AvOptFlag.None, AvSerialize.None, out buf, '=', ',') >= 0)
				{
					ChildContext child_Ctx = new ChildContext();

					printf("%s\n", buf);
					Mem.Av_Free(buf);

					child_Class.CopyTo(child_Ctx.Av_Class);
					test_Ctx.Child = child_Ctx;

					if (Opt.Av_Opt_Serialize(test_Ctx, AvOptFlag.None, AvSerialize.Skip_Defaults | AvSerialize.Search_Children, out buf, '=', ',') >= 0)
					{
						printf("%s\n", buf);
						Mem.Av_Free(buf);
					}

					Opt.Av_Opt_Free(child_Ctx);
					test_Ctx.Child = null;
				}
			}

			Opt.Av_Opt_Free(test_Ctx);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Set_Options()
		{
			printf("\nTesting av_set_options_string()\n");

			string[] options =
			[
				string.Empty,
				":",
				"=",
				"foo=:",
				":=foo",
				"=foo",
				"foo=",
				"foo",
				"foo=val",
				"foo==val",
				"toggle=:",
				"string=:",
				"toggle=1 : foo",
				"toggle=100",
				"toggle==1",
				"flags=+mu-lame : num=42: toggle=0",
				"num=42 : string=blahblah",
				"rational=0 : rational=1/2 : rational=1/-1",
				"rational=-1/0",
				"size=1024x768",
				"size=pal",
				"size=bogus",
				"pix_fmt=yuv420p",
				"pix_fmt=2",
				"pix_fmt=bogus",
				"sample_fmt=s16",
				"sample_fmt=2",
				"sample_fmt=bogus",
				"video_rate=pal",
				"video_rate=25",
				"video_rate=30000/1001",
				"video_rate=30/1.001",
				"video_rate=bogus",
				"duration=bogus",
				"duration=123.45",
				"duration=1\\:23\\:45.67",
				"color=blue",
				"color=0x223300",
				"color=0x42FF07AA",
				"cl=FL+FR",
				"cl=foo",
				"bin=boguss",
				"bin=111",
				"bin=ffff",
				"num=bogus",
				"num=44",
				"num=44.4",
				"num=-1",
				"num=-2",
				"num=101",
				"unum=bogus",
				"unum=44",
				"unum=44.4",
				"unum=-1",
				"unum=2147483648",
				"unum=2147483649",
				"num64=bogus",
				"num64=44",
				"num64=44.4",
				"num64=-1",
				"num64=-2",
				"num64=4294967296",
				"num64=4294967297",
				"flt=bogus",
				"flt=2",
				"flt=2.2",
				"flt=-1",
				"flt=101",
				"dbl=bogus",
				"dbl=2",
				"dbl=2.2",
				"dbl=-1",
				"dbl=101",
				"bool1=true",
				"bool2=auto",
				"dict1='happy=\\:-):sad=\\:-('",
				"array_int=0,32,2147483647",
				"array_int=2147483648"		// Out of range, should fail
			];

			TestContext test_Ctx = new TestContext();
			test_Class.CopyTo(test_Ctx.Av_Class);

			Opt.Av_Opt_Set_Defaults(test_Ctx);

			Log.Av_Log_Set_Level(Log.Av_Log_Quiet);

			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(options); i++)
			{
				CPointer<char> option = options[i].ToCharPointer();

				bool silence_Log = CString.strcmp(option, "rational=-1/0") == 0;	// inf formatting differs between platforms
				Log.Av_Log(test_Ctx, Log.Av_Log_Debug, "Setting options string '%s'\n", option);

				if (silence_Log)
					Log.Av_Log_Set_Callback(null);

				if (Opt.Av_Set_Options_String(test_Ctx, option, "=".ToCharPointer(), ":".ToCharPointer()) < 0)
					printf("Error '%s'\n", option);
				else
					printf("OK    '%s'\n", option);

				Log.Av_Log_Set_Callback(Log_Callback_Help);
			}

			Opt.Av_Opt_Free(test_Ctx);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Set_Options_From_String()
		{
			printf("\nTesting av_opt_set_from_string()\n");

			string[] options =
			[
				string.Empty,
				"5",
				"5:hello",
				"5:hello:size=pal",
				"5:size=pal:hello",
				":",
				"=",
				" 5 : hello : size = pal ",
				"a_very_long_option_name_that_will_need_to_be_ellipsized_around_here=42"
			];

			CPointer<CPointer<char>> shorthand = new CPointer<CPointer<char>>(
			[
				"num".ToCharPointer(), "string".ToCharPointer(), new CPointer<char>()
			]);

			TestContext test_Ctx = new TestContext();
			test_Class.CopyTo(test_Ctx.Av_Class);

			Log.Av_Log_Set_Level(Log.Av_Log_Quiet);

			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(options); i++)
			{
				CPointer<char> option = options[i].ToCharPointer();

				Log.Av_Log(test_Ctx, Log.Av_Log_Debug, "Setting options string '%s'\n", option);

				if (Opt.Av_Opt_Set_From_String(test_Ctx, option, shorthand, "=".ToCharPointer(), ":".ToCharPointer()) < 0)
					printf("Error '%s'\n", option);
				else
					printf("OK    '%s'\n", option);
			}

			Opt.Av_Opt_Free(test_Ctx);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Find2()
		{
			printf("\nTesting av_opt_find2()\n");

			TestContext test_Ctx = new TestContext();
			ChildContext child_Ctx = new ChildContext();

			test_Class.CopyTo(test_Ctx.Av_Class);
			child_Class.CopyTo(child_Ctx.Av_Class);
			test_Ctx.Child = child_Ctx;

			Log.Av_Log_Set_Level(Log.Av_Log_Quiet);

			// Should succeed. num exists and has opt_flags 1
			AvOption opt = Opt.Av_Opt_Find2(test_Ctx, "num".ToCharPointer(), null, AvOptFlag.Encoding_Param, AvOptSearch.None, out IOptionContext target);

			if ((opt != null) && (target == test_Ctx))
				printf("OK    '%s'\n", opt.Name);
			else
				printf("Error 'num'\n");

			// Should fail. num64 exists but has opt_flags 1, not 2
			opt = Opt.Av_Opt_Find(test_Ctx, "num64".ToCharPointer(), null, AvOptFlag.Decoding_Param, AvOptSearch.None);

			if (opt != null)
				printf("OK    '%s'\n", opt.Name);
			else
				printf("Error 'num64'\n");

			// Should fail. child_num exists but in a child object we're not searching
			opt = Opt.Av_Opt_Find(test_Ctx, "child_num".ToCharPointer(), null, AvOptFlag.None, AvOptSearch.None);

			if (opt != null)
				printf("OK    '%s'\n", opt.Name);
			else
				printf("Error 'child_num'\n");

			// Should succeed. child_num exists in a child object we're searching
			opt = Opt.Av_Opt_Find2(test_Ctx, "child_num".ToCharPointer(), null, AvOptFlag.None, AvOptSearch.Search_Children, out target);

			if ((opt != null) && (target == child_Ctx))
				printf("OK    '%s'\n", opt.Name);
			else
				printf("Error 'child_num'\n");

			// Should fail. foo doesn't exists
			opt = Opt.Av_Opt_Find(test_Ctx, "foo".ToCharPointer(), null, AvOptFlag.None, AvOptSearch.None);

			if (opt != null)
				printf("OK    '%s'\n", opt.Name);
			else
				printf("Error 'foo'\n");
		}
	}
}
