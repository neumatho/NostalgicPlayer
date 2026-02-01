/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class Opt
	{
		#region Type_Desc class
		private class Type_Desc
		{
			/// <summary></summary>
			public delegate c_int SetArray_Delegate(Type_Desc type, IOptionContext obj, IOptionContext target_Obj, AvOption o, CPointer<char> val, ref object dst);
			/// <summary></summary>
			public delegate c_int GetArray_Delegate(AvOption o, object dst, out CPointer<char> out_Val);
			/// <summary></summary>
			public delegate object AllocArray_Delegate(c_uint nb_Elems);
			/// <summary></summary>
			public delegate object ReallocArray_Delegate(object ptr, c_uint new_Nb_Elems);
			/// <summary></summary>
			public delegate c_int CopyArray_Delegate(Type_Desc type, IClass logCtx, AvOption o, ref object pDst, object pSrc);
			/// <summary></summary>
			public delegate void FreeArray_Delegate(AvOption o, object pItem);
			/// <summary></summary>
			public delegate object CopyElement_Delegate(object src);


			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Type_Desc()
			{
			}



			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Type_Desc(string name, SetArray_Delegate setArray, GetArray_Delegate getArray, AllocArray_Delegate allocArray, ReallocArray_Delegate reallocArray, CopyArray_Delegate copyArray, FreeArray_Delegate freeArray, CopyElement_Delegate copyElement)
			{
				Name = name.ToCharPointer();
				SetArrayFunc = setArray;
				GetArrayFunc = getArray;
				AllocArrayFunc = allocArray;
				ReallocArrayFunc = reallocArray;
				CopyArrayFunc = copyArray;
				FreeArrayFunc = freeArray;
				CopyElementFunc = copyElement;
			}

			/// <summary></summary>
			public CPointer<char> Name { get; }
			/// <summary></summary>
			public SetArray_Delegate SetArrayFunc { get; }
			/// <summary></summary>
			public GetArray_Delegate GetArrayFunc { get; }
			/// <summary></summary>
			public AllocArray_Delegate AllocArrayFunc { get; }
			/// <summary></summary>
			public ReallocArray_Delegate ReallocArrayFunc { get; }
			/// <summary></summary>
			public CopyArray_Delegate CopyArrayFunc { get; }
			/// <summary></summary>
			public FreeArray_Delegate FreeArrayFunc { get; }
			/// <summary></summary>
			public CopyElement_Delegate CopyElementFunc { get; }
		}
		#endregion

		private delegate c_int GetFmt_Delegate(CPointer<char> name);

		private static readonly Type_Desc[] opt_Type_Desc = BuildTypeDesc();

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Type_Desc[] BuildTypeDesc()
		{
			object AllocArray_Helper<T>(c_uint new_Nb_Elems) where T : struct
			{
				return Mem.Av_CAlloc<T>(new_Nb_Elems);
			}

			object AllocArrayObj_Helper<T>(c_uint new_Nb_Elems) where T : class, new()
			{
				return Mem.Av_CAllocObj<T>(new_Nb_Elems);
			}

			object ReallocArray_Helper<T>(object ptr, c_uint new_Nb_Elems) where T : struct
			{
				return Mem.Av_Realloc((CPointer<T>)ptr, new_Nb_Elems);
			}

			object ReallocArrayObj_Helper<T>(object ptr, c_uint new_Nb_Elems) where T : class, new()
			{
				return Mem.Av_ReallocObj((CPointer<T>)ptr, new_Nb_Elems);
			}

			object CopyElement_Helper<T>(object src) where T : struct
			{
				return (T)src;
			}

			Type_Desc[] result = new Type_Desc[(c_int)AvOptionType.Nb];

			result[(c_int)AvOptionType.Flags] = new Type_Desc("<flags>", Opt_Set_Array_Helper<c_int>, Opt_Get_Array_Helper<c_int>, AllocArray_Helper<c_int>, ReallocArray_Helper<c_int>, Opt_Copy_Array_Helper<c_int>, Av_Opt_Free_Helper<c_int>, CopyElement_Helper<c_int>);
			result[(c_int)AvOptionType.Int] = new Type_Desc("<int>", Opt_Set_Array_Helper<c_int>, Opt_Get_Array_Helper<c_int>, AllocArray_Helper<c_int>, ReallocArray_Helper<c_int>, Opt_Copy_Array_Helper<c_int>, Av_Opt_Free_Helper<c_int>, CopyElement_Helper<c_int>);
			result[(c_int)AvOptionType.Int64] = new Type_Desc("<int64>", Opt_Set_Array_Helper<int64_t>, Opt_Get_Array_Helper<int64_t>, AllocArray_Helper<int64_t>, ReallocArray_Helper<int64_t>, Opt_Copy_Array_Helper<int64_t>, Av_Opt_Free_Helper<int64_t>, CopyElement_Helper<int64_t>);
			result[(c_int)AvOptionType.UInt] = new Type_Desc("<unsigned>", Opt_Set_Array_Helper<c_uint>, Opt_Get_Array_Helper<c_uint>, AllocArray_Helper<c_uint>, ReallocArray_Helper<c_uint>, Opt_Copy_Array_Helper<c_uint>, Av_Opt_Free_Helper<c_uint>, CopyElement_Helper<c_uint>);
			result[(c_int)AvOptionType.UInt64] = new Type_Desc("<uint64>", Opt_Set_Array_Helper<uint64_t>, Opt_Get_Array_Helper<uint64_t>, AllocArray_Helper<uint64_t>, ReallocArray_Helper<uint64_t>, Opt_Copy_Array_Helper<uint64_t>, Av_Opt_Free_Helper<uint64_t>, CopyElement_Helper<uint64_t>);
			result[(c_int)AvOptionType.Double] = new Type_Desc("<double>", Opt_Set_Array_Helper<c_double>, Opt_Get_Array_Helper<c_double>, AllocArray_Helper<c_double>, ReallocArray_Helper<c_double>, Opt_Copy_Array_Helper<c_double>, Av_Opt_Free_Helper<c_double>, CopyElement_Helper<c_double>);
			result[(c_int)AvOptionType.Float] = new Type_Desc("<float>", Opt_Set_Array_Helper<c_float>, Opt_Get_Array_Helper<c_float>, AllocArray_Helper<c_float>, ReallocArray_Helper<c_float>, Opt_Copy_Array_Helper<c_float>, Av_Opt_Free_Helper<c_float>, CopyElement_Helper<c_float>);
			result[(c_int)AvOptionType.String] = new Type_Desc("<string>", Opt_Set_Array_Helper<CPointer<char>>, Opt_Get_Array_Helper<CPointer<char>>, AllocArray_Helper<CPointer<char>>, ReallocArray_Helper<CPointer<char>>, Opt_Copy_Array_Helper<CPointer<char>>, Av_Opt_Free_Helper<CPointer<char>>, null);
			result[(c_int)AvOptionType.Rational] = new Type_Desc("<rational>", Opt_Set_Array_Helper<AvRational>, Opt_Get_Array_Helper<AvRational>, AllocArray_Helper<AvRational>, ReallocArray_Helper<AvRational>, Opt_Copy_Array_Helper<AvRational>, Av_Opt_Free_Helper<AvRational>, CopyElement_Helper<AvRational>);
			result[(c_int)AvOptionType.Binary] = new Type_Desc("<binary>", Opt_Set_Array_Helper<CPointer<uint8_t>>, Opt_Get_Array_Helper<CPointer<uint8_t>>, AllocArray_Helper<uint8_t>, ReallocArray_Helper<CPointer<uint8_t>>, Opt_Copy_Array_Helper<CPointer<uint8_t>>, Av_Opt_Free_Helper<CPointer<uint8_t>>, null);
			result[(c_int)AvOptionType.Dict] = new Type_Desc("<dictionary>", Opt_Set_Array_Helper<AvDictionary>, Opt_Get_Array_Helper<AvDictionary>, AllocArrayObj_Helper<AvDictionary>, ReallocArrayObj_Helper<AvDictionary>, Opt_Copy_Array_Helper<AvDictionary>, Av_Opt_Free_Helper<AvDictionary>, null);
			result[(c_int)AvOptionType.Image_Size] = new Type_Desc("<image_size>", Opt_Set_Array_Helper<SizeInfo>, Opt_Get_Array_Helper<SizeInfo>, AllocArray_Helper<SizeInfo>, ReallocArray_Helper<SizeInfo>, Opt_Copy_Array_Helper<SizeInfo>, Av_Opt_Free_Helper<SizeInfo>, CopyElement_Helper<SizeInfo>);
			result[(c_int)AvOptionType.Video_Rate] = new Type_Desc("<video_rate>", Opt_Set_Array_Helper<AvRational>, Opt_Get_Array_Helper<AvRational>, AllocArray_Helper<AvRational>, ReallocArray_Helper<AvRational>, Opt_Copy_Array_Helper<AvRational>, Av_Opt_Free_Helper<AvRational>, CopyElement_Helper<AvRational>);
			result[(c_int)AvOptionType.Pixel_Fmt] = new Type_Desc("<pix_fmt>", Opt_Set_Array_Helper<AvPixelFormat>, Opt_Get_Array_Helper<AvPixelFormat>, AllocArray_Helper<AvPixelFormat>, ReallocArray_Helper<AvPixelFormat>, Opt_Copy_Array_Helper<AvPixelFormat>, Av_Opt_Free_Helper<AvPixelFormat>, CopyElement_Helper<AvPixelFormat>);
			result[(c_int)AvOptionType.Sample_Fmt] = new Type_Desc("<sample_fmt>", Opt_Set_Array_Helper<AvSampleFormat>, Opt_Get_Array_Helper<AvSampleFormat>, AllocArray_Helper<AvSampleFormat>, ReallocArray_Helper<AvSampleFormat>, Opt_Copy_Array_Helper<AvSampleFormat>, Av_Opt_Free_Helper<AvSampleFormat>, CopyElement_Helper<AvSampleFormat>);
			result[(c_int)AvOptionType.Duration] = new Type_Desc("<duration>", Opt_Set_Array_Helper<int64_t>, Opt_Get_Array_Helper<int64_t>, AllocArray_Helper<int64_t>, ReallocArray_Helper<int64_t>, Opt_Copy_Array_Helper<int64_t>, Av_Opt_Free_Helper<int64_t>, CopyElement_Helper<int64_t>);
			result[(c_int)AvOptionType.Color] = new Type_Desc("<color>", Opt_Set_Array_Helper<ColorInfo>, Opt_Get_Array_Helper<ColorInfo>, AllocArray_Helper<ColorInfo>, ReallocArray_Helper<ColorInfo>, Opt_Copy_Array_Helper<ColorInfo>, Av_Opt_Free_Helper<ColorInfo>, CopyElement_Helper<ColorInfo>);
			result[(c_int)AvOptionType.ChLayout] = new Type_Desc("<channel_layout>", Opt_Set_Array_Helper<AvChannelLayout>, Opt_Get_Array_Helper<AvChannelLayout>, AllocArrayObj_Helper<AvChannelLayout>, ReallocArrayObj_Helper<AvChannelLayout>, Opt_Copy_Array_Helper<AvChannelLayout>, Av_Opt_Free_Helper<AvChannelLayout>, null);
			result[(c_int)AvOptionType.Bool] = new Type_Desc("<boolean>", Opt_Set_Array_Helper<c_int>, Opt_Get_Array_Helper<c_int>, AllocArray_Helper<c_int>, ReallocArray_Helper<c_int>, Opt_Copy_Array_Helper<c_int>, Av_Opt_Free_Helper<c_int>, CopyElement_Helper<c_int>);

			for (c_int i = result.Length - 1; i >= 0; i--)
			{
				if (result[i] == null)
					result[i] = new Type_Desc();
			}

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Iterate over all AVOptions belonging to obj
		/// </summary>
		/********************************************************************/
		public static IEnumerable<AvOption> Av_Opt_Next(IOptionContext obj)//XX 48
		{
			if (obj == null)
				yield break;

			AvClass @class = (AvClass)obj;

			if ((@class == null) || @class.Option.IsNull || @class.Option[0].Name.IsNull)
				yield break;

			CPointer<AvOption> last = @class.Option;

			do
			{
				yield return last[0];

				++last;
			}
			while (last[0].Name.IsNotNull);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Av_Opt_Set(IOptionContext obj, CPointer<char> name, CPointer<char> val, AvOptSearch search_Flags)//XX 835
		{
			c_int ret = Opt_Set_Init(obj, name, search_Flags, AvOptionType.None, out IOptionContext target_Obj, out AvOption o, out object dst);

			if (ret < 0)
				return ret;

			ret = ((o.Type & AvOptionType.Array) != 0) ? Opt_Set_Array(obj, target_Obj, o, val, ref dst) : Opt_Set_Elem(obj, target_Obj, o, val, ref dst);

			if (ret < 0)
				return ret;

			target_Obj.GetType().GetField(o.OptionName).SetValue(target_Obj, dst);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Av_Opt_Set_Int(IOptionContext obj, CPointer<char> name, int64_t val, AvOptSearch search_Flags)//XX 880
		{
			return Set_Number(obj, name, 1, 1, val, search_Flags, AvOptionType.None);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Av_Opt_Set_Dict_Val(IOptionContext obj, CPointer<char> name, AvDictionary val, AvOptSearch search_Flags)//XX 984
		{
			c_int ret = Opt_Set_Init(obj, name, search_Flags, AvOptionType.Dict, out _, out _, out object o);

			if (ret < 0)
				return ret;

			AvDictionary dst = (AvDictionary)o;

			Dict.Av_Dict_Free(ref dst);

			return Dict.Av_Dict_Copy(ref dst, val, AvDict.None);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Av_Opt_Get(IOptionContext obj, CPointer<char> name, AvOptSearch search_Flags, out CPointer<char> out_Val)//XX 1215
		{
			out_Val = null;

			AvOption o = Av_Opt_Find2(obj, name, null, AvOptFlag.None, search_Flags, out IOptionContext target_Obj);
			c_int ret;

			if ((o == null) || (target_Obj == null) || ((o.OptionName == null) && (o.Type != AvOptionType.Const)))
				return Error.Option_Not_Found;

			if ((o.Flags & AvOptFlag.Deprecated) != 0)
				Log.Av_Log(obj, Log.Av_Log_Warning, "The \"%s\" option is deprecated: %s\n", name, o.Help);

			FieldInfo fieldInfo = null;
			object dst = null;

			if (!string.IsNullOrEmpty(o.OptionName))
			{
				fieldInfo = obj.GetType().GetField(o.OptionName);
				dst = fieldInfo.GetValue(obj);
			}

			if ((o.Type & AvOptionType.Array) != 0)
			{
				ret = Opt_Get_Array(o, dst, out out_Val);

				if (ret < 0)
					return ret;

				if (out_Val.IsNull && ((search_Flags & AvOptSearch.Allow_Null) == 0))
				{
					out_Val = Mem.Av_StrDup(CString.Empty);

					if (out_Val.IsNull)
						return Error.ENOMEM;
				}

				return 0;
			}

			CPointer<char> buf = new CPointer<char>(128);
			buf[0] = '\0';
			CPointer<char> @out = buf;

			ret = Opt_Get_Elem(o, ref @out, (size_t)buf.Length, dst, search_Flags);

			if (ret < 0)
				return ret;

			if (@out != buf)
			{
				out_Val = @out;

				return 0;
			}

			if (ret >= buf.Length)
				return Error.EINVAL;

			out_Val = Mem.Av_StrDup(@out);

			return out_Val.IsNotNull ? 0 : Error.ENOMEM;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Av_Opt_Get_Dict_Val(IOptionContext obj, CPointer<char> name, AvOptSearch search_Flag, ref AvDictionary out_Val)//XX 1383
		{
			AvOption o = Av_Opt_Find2(obj, name, null, AvOptFlag.None, search_Flag, out IOptionContext target_Obj);

			if ((o == null) || (target_Obj == null))
				return Error.Option_Not_Found;

			if (o.Type != AvOptionType.Dict)
				return Error.EINVAL;

			FieldInfo fieldInfo = obj.GetType().GetField(name.ToString());
			AvDictionary src = (AvDictionary)fieldInfo.GetValue(obj);

			return Dict.Av_Dict_Copy(ref out_Val, src, AvDict.None);
		}



		/********************************************************************/
		/// <summary>
		/// Show the obj options
		/// </summary>
		/********************************************************************/
		public static c_int Av_Opt_Show2(IOptionContext obj, IClass av_Log_Obj, AvOptFlag req_Flags, AvOptFlag rej_Flags)//XX 1666
		{
			if (obj == null)
				return -1;

			Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, "%s AVOptions:\n", ((AvClass)obj).Class_Name);

			Opt_List(obj, av_Log_Obj, null, req_Flags, rej_Flags, AvOptionType.All);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Set the values of all AVOption fields to their default values
		/// </summary>
		/********************************************************************/
		public static void Av_Opt_Set_Defaults(IOptionContext s)//XX 1678
		{
			Av_Opt_Set_Defaults2(s, AvOptFlag.None, AvOptFlag.None);
		}



		/********************************************************************/
		/// <summary>
		/// Set the values of all AVOption fields to their default values.
		/// Only these AVOption fields for which
		/// (opt-›flags ＆ mask) == flags will have their default applied to
		/// s
		/// </summary>
		/********************************************************************/
		public static void Av_Opt_Set_Defaults2(IOptionContext s, AvOptFlag mask, AvOptFlag flags)//XX 1683
		{
			foreach (AvOption opt in Av_Opt_Next(s))
			{
				FieldInfo fieldInfo = null;
				object dst = null;

				if (!string.IsNullOrEmpty(opt.OptionName))
				{
					fieldInfo = s.GetType().GetField(opt.OptionName);
					dst = fieldInfo.GetValue(s);
				}

				if ((opt.Flags & mask) != flags)
					continue;

				if ((opt.Flags & AvOptFlag.Readonly) != 0)
					continue;

				if ((opt.Type & AvOptionType.Array) != 0)
				{
					CPointer<AvOptionArrayDef> arr = opt.Default_Value.Arr;

					if (arr.IsNotNull && (arr[0].Def.IsNotNull))
					{
						Opt_Set_Array(s, s, opt, arr[0].Def, ref dst);
						fieldInfo.SetValue(s, dst);
					}

					continue;
				}

				switch (opt.Type)
				{
					case AvOptionType.Const:
					{
						// Nothing to be done here
						break;
					}

					case AvOptionType.Bool:
					case AvOptionType.Flags:
					case AvOptionType.Int:
					case AvOptionType.UInt:
					case AvOptionType.Int64:
					case AvOptionType.UInt64:
					case AvOptionType.Duration:
					case AvOptionType.Pixel_Fmt:
					case AvOptionType.Sample_Fmt:
					{
						Write_Number(s, opt, ref dst, 1, 1, opt.Default_Value.I64);
						break;
					}

					case AvOptionType.Double:
					case AvOptionType.Float:
					{
						c_double val = opt.Default_Value.Dbl;
						Write_Number(s, opt, ref dst, val, 1, 1);
						break;
					}

					case AvOptionType.Rational:
					{
						AvRational val = Rational.Av_D2Q(opt.Default_Value.Dbl, c_int.MaxValue);
						Write_Number(s, opt, ref dst, 1, val.Den, val.Num);
						break;
					}

					case AvOptionType.Color:
					{
						ColorInfo t = (ColorInfo)dst;
						Set_String_Color(s, opt, opt.Default_Value.Str, ref t);
						dst = t;
						break;
					}

					case AvOptionType.String:
					{
						CPointer<char> t = (CPointer<char>)dst;
						Set_String(s, opt, opt.Default_Value.Str, ref t);
						dst = t;
						break;
					}

					case AvOptionType.Image_Size:
					{
						SizeInfo t = (SizeInfo)dst;
						Set_String_Image_Size(s, opt, opt.Default_Value.Str, ref t);
						dst = t;
						break;
					}

					case AvOptionType.Video_Rate:
					{
						Set_String_Video_Rate(s, opt, opt.Default_Value.Str, out AvRational t);
						dst = t;
						break;
					}

					case AvOptionType.Binary:
					{
						PtrInfo<uint8_t> t = (PtrInfo<uint8_t>)dst;
						Set_String_Binary(s, opt, opt.Default_Value.Str, ref t);
						dst = t;
						break;
					}

					case AvOptionType.ChLayout:
					{
						Set_String_Channel_Layout(s, opt, opt.Default_Value.Str, ref dst);
						break;
					}

					case AvOptionType.Dict:
					{
						AvDictionary t = (AvDictionary)dst;
						Set_String_Dict(s, opt, opt.Default_Value.Str, ref t);
						dst = t;
						break;
					}

					default:
					{
						Log.Av_Log(s, Log.Av_Log_Debug, "AVOption type %d of option %s not implemented yet\n", opt.Type, opt.Name);
						break;
					}
				}

				if (fieldInfo != null)
					fieldInfo.SetValue(s, dst);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse the key/value pairs list in opts. For each key/value pair
		/// found, stores the value in the field in ctx that is named like
		/// the key. ctx must be an AVClass context, storing is done using
		/// AVOptions
		/// </summary>
		/********************************************************************/
		public static c_int Av_Set_Options_String(IOptionContext ctx, CPointer<char> opts, CPointer<char> key_Val_Sep, CPointer<char> pairs_Sep)//XX 1817
		{
			c_int count = 0;

			if (opts.IsNull)
				return 0;

			while (opts[0] != '\0')
			{
				c_int ret = Parse_Key_Value_Pair(ctx, ref opts, key_Val_Sep, pairs_Sep);

				if (ret < 0)
					return ret;

				count++;

				if (opts[0] != '\0')
					opts++;
			}

			return count;
		}



		/********************************************************************/
		/// <summary>
		/// Extract a key-value pair from the beginning of a string
		/// </summary>
		/********************************************************************/
		public static c_int Av_Opt_Get_Key_Value(ref CPointer<char> rOpts, CPointer<char> key_Val_Sep, CPointer<char> pairs_Sep, AvOptFlag2 flags, out CPointer<char> rKey, out CPointer<char> rVal)//XX 1875
		{
			rKey = null;
			rVal = null;

			CPointer<char> key = null;
			CPointer<char> opts = rOpts;

			c_int ret = Get_Key(ref opts, key_Val_Sep, out key);
			if ((ret < 0) && ((flags & AvOptFlag2.Implicit_Key) == 0))
				return Error.EINVAL;

			CPointer<char> val = AvString.Av_Get_Token(ref opts, pairs_Sep);
			if (val.IsNull)
			{
				Mem.Av_Free(key);

				return Error.ENOMEM;
			}

			rOpts = opts;
			rKey = key;
			rVal = val;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Parse the key-value pairs list in opts. For each key=value pair
		/// found, set the value of the corresponding option in ctx
		/// </summary>
		/********************************************************************/
		public static c_int Av_Opt_Set_From_String(IOptionContext ctx, CPointer<char> opts, CPointer<CPointer<char>> shorthand, CPointer<char> key_Val_Sep, CPointer<char> pairs_Sep)//XX 1897
		{
			c_int count = 0;
			CPointer<CPointer<char>> dummy_Shorthand = new CPointer<CPointer<char>>([ new CPointer<char>() ]);
			CPointer<char> key;

			if (opts.IsNull)
				return 0;

			if (shorthand.IsNull)
				shorthand = dummy_Shorthand;

			while (opts[0] != '\0')
			{
				CPointer<char> parsed_Key, value;

				c_int ret = Av_Opt_Get_Key_Value(ref opts, key_Val_Sep, pairs_Sep, shorthand[0].IsNotNull ? AvOptFlag2.Implicit_Key : AvOptFlag2.None, out parsed_Key, out value);

				if (ret < 0)
				{
					if (ret == Error.EINVAL)
						Log.Av_Log(ctx, Log.Av_Log_Error, "No option name near '%s'\n", opts);
					else
						Log.Av_Log(ctx, Log.Av_Log_Error, "Unable to parse '%s': %s\n", opts, Error.Av_Err2Str(ret));

					return ret;
				}

				if (opts[0] != '\0')
					opts++;

				if (parsed_Key.IsNotNull)
				{
					key = parsed_Key;

					while (shorthand[0].IsNotNull)	// Discard all remaining shorthand
						shorthand++;
				}
				else
					key = shorthand[0, 1];

				Log.Av_Log(ctx, Log.Av_Log_Debug, "Setting '%s' to value '%s'\n", key, value);

				ret = Av_Opt_Set(ctx, key, value, AvOptSearch.None);

				if (ret < 0)
				{
					if (ret == Error.Option_Not_Found)
						Log.Av_Log(ctx, Log.Av_Log_Error, "Option '%s' not found\n", key);

					Mem.Av_Free(value);
					Mem.Av_Free(parsed_Key);

					return ret;
				}

				Mem.Av_Free(value);
				Mem.Av_Free(parsed_Key);

				count++;
			}

			return count;
		}



		/********************************************************************/
		/// <summary>
		/// Free all allocated objects in obj
		/// </summary>
		/********************************************************************/
		public static void Av_Opt_Free(IOptionContext obj)//XX1949
		{
			foreach (AvOption o in Av_Opt_Next(obj))
			{
				FieldInfo fieldInfo = null;
				object pItem = null;

				if (!string.IsNullOrEmpty(o.OptionName))
				{
					fieldInfo = obj.GetType().GetField(o.OptionName);
					pItem = fieldInfo.GetValue(obj);
				}

				if ((o.Type & AvOptionType.Array) != 0)
				{
					Type_Desc type_Desc = opt_Type_Desc[(c_int)Type_Base(o.Type)];
					type_Desc.FreeArrayFunc(o, pItem);
				}
				else
					Opt_Free_Elem(o.Type, ref pItem);

				if (fieldInfo != null)
					fieldInfo.SetValue(obj, pItem);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Av_Opt_Free_Helper<T>(AvOption o, object pItem)
		{
			ArrayInfo<T> arrayInfo = (ArrayInfo<T>)pItem;
			Opt_Free_Array(o, ref arrayInfo.Array, ref arrayInfo.Count);
		}



		/********************************************************************/
		/// <summary>
		/// Set all the options from a given dictionary on an object
		/// </summary>
		/********************************************************************/
		public static c_int Av_Opt_Set_Dict2(IOptionContext obj, ref AvDictionary options, AvOptSearch search_Flags)//XX 1962
		{
			AvDictionary tmp = null;

			if (options == null)
				return 0;

			foreach (AvDictionaryEntry t in Dict.Av_Dict_Iterate(options))
			{
				c_int ret = Av_Opt_Set(obj, t.Key, t.Value, search_Flags);

				if (ret == Error.Option_Not_Found)
					ret = Dict.Av_Dict_Set(ref tmp, t.Key, t.Value, AvDict.MultiKey);

				if (ret < 0)
				{
					Log.Av_Log(obj, Log.Av_Log_Error, "Error setting option %s to value %s.\n", t.Key, t.Value);

					Dict.Av_Dict_Free(ref tmp);

					return ret;
				}
			}

			Dict.Av_Dict_Free(ref options);
			options = tmp;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Set all the options from a given dictionary on an object
		/// </summary>
		/********************************************************************/
		public static c_int Av_Opt_Set_Dict(IOptionContext obj, ref AvDictionary options)//XX 1986
		{
			return Av_Opt_Set_Dict2(obj, ref options, AvOptSearch.None);
		}



		/********************************************************************/
		/// <summary>
		/// Look for an option in an object. Consider only options which
		/// have all the specified flags set.
		///
		/// Note: Options found with AV_OPT_SEARCH_CHILDREN flag may not be
		/// settable directly with av_opt_set(). Use special calls which take
		/// an options AVDictionary (e.g. avformat_open_input()) to set
		/// options found with this flag
		/// </summary>
		/********************************************************************/
		public static AvOption Av_Opt_Find(IOptionContext obj, CPointer<char> name, CPointer<char> unit, AvOptFlag opt_Flags, AvOptSearch search_Flags)//XX 1991
		{
			return Av_Opt_Find2(obj, name, unit, opt_Flags, search_Flags, out _);
		}



		/********************************************************************/
		/// <summary>
		/// Look for an option in an object. Consider only options which
		/// have all the specified flags set
		/// </summary>
		/********************************************************************/
		public static AvOption Av_Opt_Find2(IOptionContext obj, CPointer<char> name, CPointer<char> unit, AvOptFlag opt_Flags, AvOptSearch search_Flags, out IOptionContext target_Obj)//XX 1991
		{
			target_Obj = null;

			if (obj == null)
				return null;

			AvClass c = obj as AvClass;

			if (c == null)
				return null;

			if ((search_Flags & AvOptSearch.Search_Children) != 0)
			{
				if ((search_Flags & AvOptSearch.Search_Fake_Obj) != 0)
				{
					foreach (AvClass child in Av_Opt_Child_Class_Iterate(c))
					{
						AvOption o = Av_Opt_Find2((IOptionContext)child, name, unit, opt_Flags, search_Flags, out _);
						if (o != null)
							return o;
					}
				}
				else
				{
					foreach (IOptionContext child in Av_Opt_Child_Next(obj))
					{
						AvOption o = Av_Opt_Find2(child, name, unit, opt_Flags, search_Flags, out target_Obj);
						if (o != null)
							return o;
					}
				}
			}

			foreach (AvOption o in Av_Opt_Next(obj))
			{
				if ((CString.strcmp(o.Name, name) == 0) && ((o.Flags & opt_Flags) == opt_Flags) &&
				    ((unit.IsNull && (o.Type != AvOptionType.Const)) ||
				     (unit.IsNotNull && (o.Type == AvOptionType.Const) && o.Unit.IsNotNull && (CString.strcmp(o.Unit, unit) == 0))))
				{
					if ((search_Flags & AvOptSearch.Search_Fake_Obj) == 0)
						target_Obj = obj;
					else
						target_Obj = null;

					return o;
				}
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Iterate over AVOptions-enabled children of obj
		/// </summary>
		/********************************************************************/
		public static IEnumerable<IOptionContext> Av_Opt_Child_Next(IOptionContext obj)//XX 2042
		{
			AvClass c = (AvClass)obj;

			if (c.Child_Next != null)
				return c.Child_Next(obj);

			return [];
		}



		/********************************************************************/
		/// <summary>
		/// Iterate over potential AVOptions-enabled children of parent
		/// </summary>
		/********************************************************************/
		public static IEnumerable<AvClass> Av_Opt_Child_Class_Iterate(AvClass parent)//XX 2050
		{
			if (parent.Child_Class_Iterate != null)
				return parent.Child_Class_Iterate();

			return [];
		}



		/********************************************************************/
		/// <summary>
		/// Copy options from src object into dest object.
		///
		/// The underlying AVClass of both src and dest must coincide. The
		/// guarantee below does not apply if this is not fulfilled.
		///
		/// Options that require memory allocation (e.g. string or binary)
		/// are malloc'ed in dest object.
		/// Original memory allocated for such options is freed unless both
		/// src and dest options points to the same memory.
		///
		/// Even on error it is guaranteed that allocated options from src
		/// and dest no longer alias each other afterwards; in particular
		/// calling av_opt_free() on both src and dest is safe afterwards if
		/// dest has been memdup'ed from src
		/// </summary>
		/********************************************************************/
		public static c_int Av_Opt_Copy(IOptionContext dst, IOptionContext src)//XX 2151
		{
			c_int ret = 0;

			if (src == null)
				return Error.EINVAL;

			AvClass c = (AvClass)src;

			if ((c == null) || (c != (AvClass)dst))
				return Error.EINVAL;

			foreach (AvOption o in Av_Opt_Next(src))
			{
				FieldInfo field_Dst = dst.GetType().GetField(o.OptionName);
				FieldInfo field_Src = src.GetType().GetField(o.OptionName);

				object dstVal = field_Dst.GetValue(dst);
				object srcVal = field_Src.GetValue(src);

				c_int err = (o.Type & AvOptionType.Array) != 0 ? Opt_Copy_Array(dst, o, ref dstVal, srcVal) : Opt_Copy_Elem(dst, o.Type, ref dstVal, srcVal);
				field_Dst.SetValue(dst, dstVal);

				if (err < 0)
					ret = err;
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// For an array-type option, retrieve the values of one or more
		/// array elements
		/// </summary>
		/********************************************************************/
		public static c_int Av_Opt_Get_Array<T1, T2>(IOptionContext obj, CPointer<char> name, AvOptSearch search_Flags, c_uint start_Elem, c_uint nb_Elems, AvOptionType out_Type, ref CPointer<T2> out_Val)//XX 2195
		{
			AvOption o = Av_Opt_Find2(obj, name, null, AvOptFlag.None, search_Flags, out IOptionContext target_Obj);

			if ((o == null) || (target_Obj == null))
				return Error.Option_Not_Found;

			if (((o.Type & AvOptionType.Array) == 0) || ((out_Type & AvOptionType.Array) != 0))
				return Error.EINVAL;

			FieldInfo fieldInfo = target_Obj.GetType().GetField(o.OptionName);
			ArrayInfo<T1> array = (ArrayInfo<T1>)fieldInfo.GetValue(target_Obj);

			CPointer<T1> pArray = array.Array;
			c_uint array_Size = array.Count;
			c_int ret;

			if ((start_Elem >= array_Size) || ((array_Size - start_Elem) < nb_Elems))
				return Error.EINVAL;

			for (c_uint i = 0; i < nb_Elems; i++)
			{
				T1 src = pArray[start_Elem + i];
				object dst = out_Val[i];

				if (out_Type == Type_Base(o.Type))
				{
					ret = Opt_Copy_Elem(obj, out_Type, ref dst, src);

					if (ret < 0)
						goto Fail;

					out_Val[i] = (T2)dst;
				}
				else if (out_Type == AvOptionType.String)
				{
					CPointer<char> buf = new CPointer<char>(128);
					CPointer<char> @out = buf;

					ret = Opt_Get_Elem(o, ref @out, (size_t)buf.Length, src, search_Flags);

					if (ret < 0)
						goto Fail;

					if (@out == buf)
					{
						@out = Mem.Av_StrDup(buf);

						if (@out.IsNull)
						{
							ret = Error.ENOMEM;
							goto Fail;
						}
					}

					out_Val[i] = (T2)(object)@out;
				}
				else if ((out_Type == AvOptionType.Int64) || (out_Type == AvOptionType.Double) || (out_Type == AvOptionType.Rational))
				{
					c_double num = 1.0;
					c_int den = 1;
					int64_t intNum = 1;

					ret = Read_Number(o, src, ref num, ref den, ref intNum);

					if (ret < 0)
						goto Fail;

					switch (out_Type)
					{
						case AvOptionType.Int64:
						{
							dst = (num == den) ? intNum : num * intNum / den;
							break;
						}

						case AvOptionType.Double:
						{
							dst = num * intNum / den;
							break;
						}

						case AvOptionType.Rational:
						{
							dst = (num == 1.0) && ((c_int)intNum == intNum) ? new AvRational((c_int)intNum, den) : Double_To_Rational(num * intNum / den);
							break;
						}
					}

					out_Val[i] = (T2)dst;
				}
				else
					return Error.ENOSYS;
			}

			return 0;

			Fail:
			for (c_uint i = 0; i < nb_Elems; i++)
			{
				object val = out_Val[i];
				Opt_Free_Elem(out_Type, ref val);
				out_Val[i] = default;
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Get a list of allowed ranges for the given option.
		///
		/// The returned list may depend on other fields in obj like for
		/// example profile
		/// </summary>
		/********************************************************************/
		public static c_int Av_Opt_Query_Ranges(out AvOptionRanges ranges_Arg, IOptionContext obj, CPointer<char> key, AvOptSearch flags)//XX 2470
		{
			AvClass c = (AvClass)obj;
			UtilFunc.QueryRanges_Delegate callback = c.Query_Ranges;

			if (callback == null)
				callback = Av_Opt_Query_Ranges_Default;

			c_int ret = callback(out ranges_Arg, obj, key, flags);
			if (ret >= 0)
			{
				if ((flags & AvOptSearch.Multi_Component_Range) == 0)
					ret = 1;

				ranges_Arg.Nb_Components = ret;
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Get a default list of allowed ranges for the given option.
		///
		/// This list is constructed without using the AVClass.query_ranges()
		/// callback and can be used as fallback from within the callback
		/// </summary>
		/********************************************************************/
		public static c_int Av_Opt_Query_Ranges_Default(out AvOptionRanges ranges_Arg, IOptionContext obj, CPointer<char> key, AvOptSearch flags)//XX 2488
		{
			AvOptionRanges ranges = Mem.Av_MAlloczObj<AvOptionRanges>();
			CPointer<AvOptionRange> range_Array = Mem.Av_MAlloczObj<AvOptionRange>(1);
			AvOptionRange range = Mem.Av_MAlloczObj<AvOptionRange>();
			AvOption field = Av_Opt_Find(obj, key, null, AvOptFlag.None, flags);
			c_int ret;

			ranges_Arg = null;

			if ((ranges == null) || (range == null) || range_Array.IsNull || (field == null))
			{
				ret = Error.ENOMEM;
				goto Fail;
			}

			ranges.Range = range_Array;
			ranges.Range[0] = range;
			ranges.Nb_Ranges = 1;
			ranges.Nb_Components = 1;
			range.Is_Range = true;
			range.Value_Min = field.Min;
			range.Value_Max = field.Max;

			switch (field.Type)
			{
				case AvOptionType.Bool:
				case AvOptionType.Int:
				case AvOptionType.UInt:
				case AvOptionType.Int64:
				case AvOptionType.UInt64:
				case AvOptionType.Pixel_Fmt:
				case AvOptionType.Sample_Fmt:
				case AvOptionType.Float:
				case AvOptionType.Double:
				case AvOptionType.Duration:
				case AvOptionType.Color:
					break;

				case AvOptionType.String:
				{
					range.Component_Min = 0;
					range.Component_Max = 0x10ffff;	// max unicode value
					range.Value_Min = -1;
					range.Value_Max = c_int.MaxValue;
					break;
				}

				case AvOptionType.Rational:
				{
					range.Component_Min = c_int.MinValue;
					range.Component_Max = c_int.MaxValue;
					break;
				}

				case AvOptionType.Image_Size:
				{
					range.Component_Min = 0;
					range.Component_Max = c_int.MaxValue / 128 / 8;
					range.Value_Min = 0;
					range.Value_Max = c_int.MaxValue / 8;
					break;
				}

				case AvOptionType.Video_Rate:
				{
					range.Component_Min = 1;
					range.Component_Max = c_int.MaxValue;
					range.Value_Min = 1;
					range.Value_Max = c_int.MaxValue;
					break;
				}

				default:
				{
					ret = Error.ENOSYS;
					goto Fail;
				}
			}

			ranges_Arg = ranges;

			return 1;

			Fail:
			Mem.Av_Free(ranges);
			Mem.Av_Free(range);
			Mem.Av_Free(range_Array);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Free an AVOptionRanges struct and set it to NULL
		/// </summary>
		/********************************************************************/
		public static void Av_Opt_FreeP_Ranges(ref AvOptionRanges rangesP)//XX 2560
		{
			AvOptionRanges ranges = rangesP;

			if (ranges == null)
				return;

			for (c_int i = 0; i < (ranges.Nb_Ranges * ranges.Nb_Components); i++)
			{
				AvOptionRange range = ranges.Range[i];

				if (range != null)
				{
					Mem.Av_FreeP(ref range.Str);
					Mem.Av_FreeP(ref ranges.Range[i]);
				}
			}

			Mem.Av_FreeP(ref ranges.Range);
			Mem.Av_FreeP(ref rangesP);
		}



		/********************************************************************/
		/// <summary>
		/// Check if given option is set to its default value.
		///
		/// Options o must belong to the obj. This function must not be
		/// called to check child's options state.
		/// See av_opt_is_set_to_default_by_name()
		/// </summary>
		/********************************************************************/
		public static c_int Av_Opt_Is_Set_To_Default(IOptionContext obj, AvOption o)//XX 2579
		{
			if ((o == null) || (obj == null))
				return Error.EINVAL;

			int64_t i64;
			c_double d;
			AvRational q;
			CPointer<char> str;
			c_int ret;

			FieldInfo fieldInfo = null;
			object dst = null;

			if (!string.IsNullOrEmpty(o.OptionName))
			{
				fieldInfo = obj.GetType().GetField(o.OptionName);
				dst = fieldInfo.GetValue(obj);
			}

			if ((o.Type & AvOptionType.Array) != 0)
			{
				CPointer<char> def = o.Default_Value.Arr.IsNotNull ? o.Default_Value.Arr[0].Def : null;

				ret = Opt_Get_Array(o, dst, out CPointer<char> val);

				if (ret < 0)
					return ret;

				if ((val.IsNotNull ? 1 : 0) != (def.IsNotNull ? 1 : 0))
					ret = 0;
				else if (val.IsNotNull)
					ret = CString.strcmp(val, def) == 0 ? 1 : 0;

				Mem.Av_FreeP(ref val);

				return ret;
			}

			switch (o.Type)
			{
				case AvOptionType.Const:
					return 1;

				case AvOptionType.Bool:
				case AvOptionType.Flags:
				case AvOptionType.Pixel_Fmt:
				case AvOptionType.Sample_Fmt:
				case AvOptionType.Int:
				case AvOptionType.UInt:
				case AvOptionType.Duration:
				case AvOptionType.Int64:
				case AvOptionType.UInt64:
				{
					Read_Number(o, dst, out i64);

					return o.Default_Value.I64 == i64 ? 1 : 0;
				}

				case AvOptionType.ChLayout:
				{
					AvChannelLayout ch_Layout = new AvChannelLayout();

					if (o.Default_Value.Str.IsNotNull)
					{
						ret = Channel_Layout.Av_Channel_Layout_From_String(ref ch_Layout, o.Default_Value.Str);

						if (ret < 0)
							return ret;
					}

					ret = Channel_Layout.Av_Channel_Layout_Compare((AvChannelLayout)dst, ch_Layout) == 0 ? 1 : 0;
					Channel_Layout.Av_Channel_Layout_Uninit(ch_Layout);

					return ret;
				}

				case AvOptionType.String:
				{
					str = (CPointer<char>)dst;

					if (str == o.Default_Value.Str)	// 2 nulls
						return 1;

					if (str.IsNull || o.Default_Value.Str.IsNull)
						return 0;

					return CString.strcmp(str, o.Default_Value.Str) == 0 ? 1 : 0;
				}

				case AvOptionType.Double:
				{
					d = (c_double)dst;

					return o.Default_Value.Dbl == d ? 1 : 0;
				}

				case AvOptionType.Float:
				{
					d = (c_float)dst;

					return o.Default_Value.Dbl == d ? 1 : 0;
				}

				case AvOptionType.Rational:
				{
					q = Rational.Av_D2Q(o.Default_Value.Dbl, c_int.MaxValue);

					return Rational.Av_Cmp_Q((AvRational)dst, q) == 0 ? 1 : 0;
				}

				case AvOptionType.Binary:
				{
					PtrInfo<uint8_t> tmp = new PtrInfo<uint8_t>();
					PtrInfo<uint8_t> opt_Ptr = (PtrInfo<uint8_t>)dst;

					if ((opt_Ptr.Len == 0) && (o.Default_Value.Str.IsNull || (CString.strlen(o.Default_Value.Str) == 0)))
						return 1;

					if ((opt_Ptr.Len == 0) || o.Default_Value.Str.IsNull || (CString.strlen(o.Default_Value.Str) == 0))
						return 0;

					if (opt_Ptr.Len != (CString.strlen(o.Default_Value.Str) / 2))
						return 0;

					ret = Set_String_Binary(null, null, o.Default_Value.Str, ref tmp);

					if (ret == 0)
						ret = CMemory.memcmp(opt_Ptr.Ptr, tmp.Ptr, tmp.Len) == 0 ? 1 : 0;

					Mem.Av_Free(tmp.Ptr);

					return ret;
				}

				case AvOptionType.Dict:
				{
					AvDictionary dict1 = null;
					AvDictionary dict2 = (AvDictionary)dst;

					ret = Dict.Av_Dict_Parse_String(ref dict1, o.Default_Value.Str, "=", ":", AvDict.None);

					if (ret < 0)
					{
						Dict.Av_Dict_Free(ref dict1);
						return ret;
					}

					AvDictionaryEntry[] en1Arr = Dict.Av_Dict_Iterate(dict1).ToArray();
					AvDictionaryEntry[] en2Arr = Dict.Av_Dict_Iterate(dict2).ToArray();

					if (en1Arr.Length != en2Arr.Length)
						ret = 0;
					else
					{
						ret = 1;

						for (c_int i = 0; i < en1Arr.Length; i++)
						{
							AvDictionaryEntry en1 = en1Arr[i];
							AvDictionaryEntry en2 = en2Arr[i];

							if ((CString.strcmp(en1.Key, en2.Key) != 0) || (CString.strcmp(en1.Value, en2.Value) != 0))
							{
								ret = 0;
								break;
							}
						}
					}

					Dict.Av_Dict_Free(ref dict1);

					return ret;
				}

				case AvOptionType.Image_Size:
				{
					SizeInfo sizeInfo;

					if (o.Default_Value.Str.IsNull || (CString.strcmp(o.Default_Value.Str, "none") == 0))
					{
						sizeInfo.Width = 0;
						sizeInfo.Height = 0;
					}
					else
					{
						ret = ParseUtils.Av_Parse_Video_Size(out sizeInfo.Width, out sizeInfo.Height, o.Default_Value.Str);

						if (ret < 0)
							return ret;
					}

					return (sizeInfo.Width == ((SizeInfo)dst).Width) && (sizeInfo.Height == ((SizeInfo)dst).Height) ? 1 : 0;
				}

				case AvOptionType.Video_Rate:
				{
					q = new AvRational(0, 0);

					if (o.Default_Value.Str.IsNotNull)
					{
						ret = ParseUtils.Av_Parse_Video_Rate(out q, o.Default_Value.Str);

						if (ret < 0)
							return ret;
					}

					return Rational.Av_Cmp_Q((AvRational)dst, q) == 0 ? 1 : 0;
				}

				case AvOptionType.Color:
				{
					ColorInfo color = new ColorInfo();

					if (o.Default_Value.Str.IsNotNull)
					{
						ret = ParseUtils.Av_Parse_Color(color.Color, o.Default_Value.Str, -1, null);

						if (ret < 0)
							return ret;
					}

					return CMemory.memcmp(color.Color, ((ColorInfo)dst).Color, (size_t)color.Color.Length) == 0 ? 1 : 0;
				}

				default:
				{
					Log.Av_Log(obj, Log.Av_Log_Warning, "Not supported option type: %d, option name: %s\n", o.Type, o.Name);
					break;
				}
			}

			return Error.PatchWelcome;
		}



		/********************************************************************/
		/// <summary>
		/// Check if given option is set to its default value
		/// </summary>
		/********************************************************************/
		public static c_int Av_Opt_Is_Set_To_Default_By_Name(IOptionContext obj, CPointer<char> name, AvOptSearch search_Flags)//XX 2715
		{
			if (obj == null)
				return Error.EINVAL;

			AvOption o = Av_Opt_Find2(obj, name, null, AvOptFlag.None, search_Flags, out IOptionContext target);

			if (o == null)
				return Error.Option_Not_Found;

			return Av_Opt_Is_Set_To_Default(target, o);
		}



		/********************************************************************/
		/// <summary>
		/// Serialize object's options.
		///
		/// Create a string containing object's serialized options.
		/// Such string may be passed back to av_opt_set_from_string() in
		/// order to restore option values.
		/// A key/value or pairs separator occurring in the serialized value
		/// or name string are escaped through the av_escape() function
		/// </summary>
		/********************************************************************/
		public static c_int Av_Opt_Serialize(IOptionContext obj, AvOptFlag opt_Flags, AvSerialize flags, out CPointer<char> buffer, char key_Val_Sep, char pairs_Sep)//XX 2770
		{
			buffer = null;

			c_int cnt = 0;

			if ((pairs_Sep == '\0') || (key_Val_Sep == '\0') || (pairs_Sep == key_Val_Sep) || (pairs_Sep == '\\') || (key_Val_Sep == '\\'))
			{
				Log.Av_Log(obj, Log.Av_Log_Error, "Invalid separator(s) found.");

				return Error.EINVAL;
			}

			if (obj == null)
				return Error.EINVAL;

			BPrint.Av_BPrint_Init(out AVBPrint bprint, 64, BPrint.Av_BPrint_Size_Unlimited);

			c_int ret = Opt_Serialize(obj, opt_Flags, flags, ref cnt, bprint, key_Val_Sep, pairs_Sep);

			if (ret < 0)
				return ret;

			ret = BPrint.Av_BPrint_Finalize(bprint, out buffer);

			if (ret < 0)
				return ret;

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static AvOptionType Type_Base(AvOptionType type)//XX 46
		{
			return type & ~AvOptionType.Array;
		}



		/********************************************************************/
		/// <summary>
		/// Option is plain old data
		/// </summary>
		/********************************************************************/
		private static bool Opt_Is_Pod(AvOptionType type)//XX 87
		{
			switch (type)
			{
				case AvOptionType.Flags:
				case AvOptionType.Int:
				case AvOptionType.Int64:
				case AvOptionType.Double:
				case AvOptionType.Float:
				case AvOptionType.Rational:
				case AvOptionType.UInt64:
				case AvOptionType.Image_Size:
				case AvOptionType.Pixel_Fmt:
				case AvOptionType.Sample_Fmt:
				case AvOptionType.Video_Rate:
				case AvOptionType.Duration:
				case AvOptionType.Color:
				case AvOptionType.Bool:
				case AvOptionType.UInt:
					return true;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static char Opt_Array_Sep(AvOption o)//XX 110
		{
			CPointer<AvOptionArrayDef> d = o.Default_Value.Arr;

			return d.IsNotNull && (d[0].Sep != 0) ? d[0].Sep : ',';
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Opt_Free_Elem(AvOptionType type, ref object ptr)//XX 128
		{
			switch (Type_Base(type))
			{
				case AvOptionType.String:
				case AvOptionType.Binary:
				{
					Mem.Av_FreeP(ref ptr);
					break;
				}

				case AvOptionType.Dict:
				{
					AvDictionary dict = (AvDictionary)ptr;
					Dict.Av_Dict_Free(ref dict);
					ptr = dict;
					break;
				}

				case AvOptionType.ChLayout:
				{
					AvChannelLayout cl = (AvChannelLayout)ptr;
					Channel_Layout.Av_Channel_Layout_Uninit(cl);
					ptr = cl;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Opt_Free_Array<T>(AvOption o, ref CPointer<T> pArray, ref c_uint count)//XX 149
		{
			for (c_uint i = 0; i < count; i++)
			{
				object item = pArray[i];
				Opt_Free_Elem(o.Type, ref item);
				pArray[i] = default;
			}

			Mem.Av_FreeP(ref pArray);
			count = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Perform common setup for option-setting functions
		/// </summary>
		/********************************************************************/
		private static c_int Opt_Set_Init(IOptionContext obj, CPointer<char> name, AvOptSearch search_Flags, AvOptionType require_Type, out IOptionContext pTgt, out AvOption pO, out object pDst)//XX 166
		{
			pTgt = null;
			pO = null;
			pDst = null;

			AvOption o = Av_Opt_Find2(obj, name, null, AvOptFlag.None, search_Flags, out IOptionContext tgt);
			if ((o == null) || (tgt == null))
				return Error.Option_Not_Found;

			if ((o.Flags & AvOptFlag.Readonly) != 0)
				return Error.EINVAL;

			if ((require_Type != 0) && (o.Type != require_Type))
			{
				Log.Av_Log(obj, Log.Av_Log_Error, "Tried to set option '%s' of type %s from value of type %s, this is not supported\n", o.Name, opt_Type_Desc[(c_int)o.Type].Name, opt_Type_Desc[(c_int)require_Type].Name);

				return Error.EINVAL;
			}

			if ((o.Flags & AvOptFlag.Runtime_Param) == 0)
			{
				AvClassStateFlag? state_Flags = null;

				// Try state flags first from the target (child), then from its parent
				AvClass @class = (AvClass)tgt;

				if (!string.IsNullOrEmpty(@class.State_Flags_Name))
					state_Flags = (AvClassStateFlag?)tgt.GetType().GetField(@class.State_Flags_Name)?.GetValue(tgt);

				if ((state_Flags == null) && (obj != tgt))
				{
					@class = (AvClass)obj;

					if (!string.IsNullOrEmpty(@class.State_Flags_Name))
						state_Flags = (AvClassStateFlag?)obj.GetType().GetField(@class.State_Flags_Name)?.GetValue(obj);
				}

				if ((state_Flags != null) && ((state_Flags & AvClassStateFlag.Initialized) != 0))
				{
					Log.Av_Log(obj, Log.Av_Log_Error, "Option '%s' is not a runtime option and so cannot be set after the object has been initialized\n", o.Name);

					return Error.EINVAL;
				}
			}

			if ((o.Flags & AvOptFlag.Deprecated) != 0)
				Log.Av_Log(obj, Log.Av_Log_Warning, "The \"%s\" optiion is deprecated: %s\n", name, o.Help);

			pO = o;
			pTgt = tgt;
			pDst = tgt.GetType().GetField(o.OptionName).GetValue(tgt);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvRational Double_To_Rational(c_double d)//XX 234
		{
			AvRational r = Rational.Av_D2Q(d, 1 << 24);

			if (((r.Num == 0) || (r.Den == 0)) && (d != 0))
				r = Rational.Av_D2Q(d, c_int.MaxValue);

			return r;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Read_Number(AvOption o, object dst, out int64_t intNum)//XX 242
		{
			c_double num = 0;
			c_int den = 0;
			intNum = 0;

			return Read_Number(o, dst, ref num, ref den, ref intNum);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Read_Number(AvOption o, object dst, ref c_double num, ref c_int den, ref int64_t intNum)//XX 242
		{
			switch (Type_Base(o.Type))
			{
				case AvOptionType.Flags:
				{
					intNum = (c_uint)(c_int)dst;

					return 0;
				}

				case AvOptionType.Pixel_Fmt:
				{
					intNum = (int64_t)(AvPixelFormat)dst;

					return 0;
				}

				case AvOptionType.Sample_Fmt:
				{
					intNum = (int64_t)(AvSampleFormat)dst;

					return 0;
				}

				case AvOptionType.Bool:
				case AvOptionType.Int:
				{
					intNum = (c_int)dst;

					return 0;
				}

				case AvOptionType.UInt:
				{
					intNum = (c_uint)dst;

					return 0;
				}

				case AvOptionType.Duration:
				case AvOptionType.Int64:
				case AvOptionType.UInt64:
				{
					intNum = (int64_t)dst;

					return 0;
				}

				case AvOptionType.Float:
				{
					num = (c_float)dst;

					return 0;
				}

				case AvOptionType.Double:
				{
					num = (c_double)dst;

					return 0;
				}

				case AvOptionType.Rational:
				{
					intNum = ((AvRational)dst).Num;
					den = ((AvRational)dst).Den;

					return 0;
				}

				case AvOptionType.Const:
				{
					intNum = o.Default_Value.I64;

					return 0;
				}
			}

			return Error.EINVAL;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Write_Number(IOptionContext obj, AvOption o, ref object dst, c_double num, c_int den, int64_t intNum)//XX 283
		{
			AvOptionType type = Type_Base(o.Type);

			if ((type != AvOptionType.Flags) && ((den == 0) || (o.Max * den < num * intNum) || (o.Min * den > num * intNum)))
			{
				num = den != 0 ? num * intNum / den : ((num != 0) && (intNum != 0) ? c_double.PositiveInfinity : c_double.NaN);
				Log.Av_Log(obj, Log.Av_Log_Error, "Value %f for parameter '%s' out of range [%g - %g]\n", num, o.Name, o.Min, o.Max);

				return Error.ERANGE;
			}

			if (type == AvOptionType.Flags)
			{
				c_double d = num * intNum / den;

				if ((d < -1.5) || (d > 0xffffffff + 0.5) || ((CMath.llrint(d * 256) & 255) != 0))
				{
					Log.Av_Log(obj, Log.Av_Log_Error, "Value %f for parameter '%s' is not a valid set of 32bit integer flags\n", num * intNum / den, o.Name);

					return Error.ERANGE;
				}
			}

			switch (type)
			{
				case AvOptionType.Pixel_Fmt:
				{
					dst = (AvPixelFormat)(CMath.llrint(num / den) * intNum);
					break;
				}

				case AvOptionType.Sample_Fmt:
				{
					dst = (AvSampleFormat)(CMath.llrint(num / den) * intNum);
					break;
				}

				case AvOptionType.Bool:
				case AvOptionType.Flags:
				case AvOptionType.Int:
				{
					dst = (c_int)(CMath.llrint(num / den) * intNum);
					break;
				}

				case AvOptionType.UInt:
				{
					dst = (c_uint)(CMath.llrint(num / den) * intNum);
					break;
				}

				case AvOptionType.Duration:
				case AvOptionType.Int64:
				{
					c_double d = num / den;

					if ((intNum == 1) && (d == int64_t.MaxValue))
						dst = int64_t.MaxValue;
					else
						dst = (int64_t)(CMath.llrint(d) * intNum);

					break;
				}

				case AvOptionType.UInt64:
				{
					c_double d = num / den;

					// We must special case uint64_t here as llrint() does not support values
					// outside the int64_t range and there is no portable function which does
					// "INT64_MAX + 1ULL" is used as it is representable exactly as IEEE double
					// while INT64_MAX is not
					if ((intNum == 1) && (d == uint64_t.MaxValue))
						dst = uint64_t.MaxValue;
					else if (d > (int64_t.MaxValue + 1UL))
						dst = (uint64_t)(((uint64_t)CMath.llrint(d - (int64_t.MaxValue + 1UL)) + (int64_t.MaxValue + 1UL)) * (uint64_t)intNum);
					else
						dst = (uint64_t)(CMath.llrint(d) * intNum);

					break;
				}

				case AvOptionType.Float:
				{
					dst = (c_float)(num * intNum / den);
					break;
				}

				case AvOptionType.Double:
				{
					dst = (c_double)(num * intNum / den);
					break;
				}

				case AvOptionType.Rational:
				case AvOptionType.Video_Rate:
				{
					if ((c_int)num == num)
						dst = new AvRational((c_int)(num * intNum), den);
					else
						dst = Double_To_Rational(num * intNum / den);

					break;
				}

				default:
					return Error.EINVAL;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int HexChar2Int(char c)//XX 358
		{
			if ((c >= '0') && (c <= '9'))
				return c - '0';

			if ((c >= 'a') && (c <= 'f'))
				return c - 'a' + 10;

			if ((c >= 'A') && (c <= 'F'))
				return c - 'A' + 10;

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Set_String_Binary(IOptionContext obj, AvOption o, CPointer<char> val, ref PtrInfo<uint8_t> dst)//XX 402
		{
			Mem.Av_FreeP(ref dst.Ptr);
			dst.Len = 0;

			c_int len;

			if (val.IsNull || ((len = (c_int)CString.strlen(val)) == 0))
				return 0;

			if ((len & 1) != 0)
				return Error.EINVAL;

			len /= 2;

			CPointer<uint8_t> ptr, bin;

			ptr = bin = Mem.Av_MAlloc<uint8_t>((size_t)len);
			if (ptr.IsNull)
				return Error.ENOMEM;

			while (val[0] != 0)
			{
				c_int a = HexChar2Int(val[0, 1]);
				c_int b = HexChar2Int(val[0, 1]);

				if ((a < 0) || (b < 0))
				{
					Mem.Av_Free(bin);

					return Error.EINVAL;
				}

				ptr[0, 1] = (uint8_t)((a << 4) | b);
			}

			dst.Ptr = bin;
			dst.Len = (size_t)len;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Set_String(IOptionContext obj, AvOption o, CPointer<char> val, ref CPointer<char> dst)//XX 402
		{
			Mem.Av_FreeP(ref dst);

			if (val.IsNull)
				return 0;

			dst = Mem.Av_StrDup(val);

			return dst.IsNotNull ? 0 : Error.ENOMEM;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_double Default_NumVal(AvOption opt)//XX 411
		{
			return (opt.Type == AvOptionType.Int64) || (opt.Type == AvOptionType.UInt64) ||
				   (opt.Type == AvOptionType.Const) || (opt.Type == AvOptionType.Flags) ||
				   (opt.Type == AvOptionType.UInt) || (opt.Type == AvOptionType.Int)
				   ? opt.Default_Value.I64 : opt.Default_Value.Dbl;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Set_String_Number(IOptionContext obj, IOptionContext target_Obj, AvOption o, CPointer<char> val, ref object dst)//XX 420
		{
			AvOptionType type = Type_Base(o.Type);
			c_int ret = 0;

			if ((type == AvOptionType.Rational) || (type == AvOptionType.Video_Rate))
			{
				CSScanF sscanF = new CSScanF();

				if (sscanF.Parse(val.ToString(), "%d%*1[:/]%d%c") == 2)
				{
					ret = Write_Number(obj, o, ref dst, 1, (c_int)sscanF.Results[1], (c_int)sscanF.Results[0]);
					if (ret >= 0)
						return ret;

					ret = 0;
				}
			}

			for (;;)
			{
				c_int i = 0;
				char[] buf = new char[256];
				c_int cmd = 0;
				c_double d;
				int64_t intNum = 1;

				if (type == AvOptionType.Flags)
				{
					if ((val[0] == '+') || (val[0] == '-'))
						cmd = val[0, 1];

					for (; (i < (buf.Length - 1)) && (val[i] != 0) && (val[i] != '+') && (val[i] != '-'); i++)
						buf[i] = val[i];

					buf[i] = '\0';
				}

				{
					c_int ci = 0;
					c_double[] const_Values = new c_double[64];
					CPointer<char>[] const_Names = new CPointer<char>[64];
					AvOptSearch search_Flags = ((o.Flags & AvOptFlag.Child_Consts) != 0) ? AvOptSearch.Search_Children : AvOptSearch.None;
					AvOption o_Named = Av_Opt_Find(target_Obj, i != 0 ? buf : val, o.Unit, AvOptFlag.None, search_Flags);

					if ((o_Named != null) && (o_Named.Type == AvOptionType.Const))
					{
						d = Default_NumVal(o_Named);

						if ((o_Named.Flags & AvOptFlag.Deprecated) != 0)
							Log.Av_Log(obj, Log.Av_Log_Warning, "The \"%s\" option is deprecated: %s\n", o_Named.Name, o_Named.Help);
					}
					else
					{
						if (o.Unit.IsNotNull)
						{
							foreach (AvOption _o_Named in Av_Opt_Next(target_Obj))
							{
								o_Named = _o_Named;

								if ((o_Named.Type == AvOptionType.Const) && o_Named.Unit.IsNotNull && (CString.strcmp(o_Named.Unit, o.Unit) == 0))
								{
									if ((size_t)(ci + 6) >= Macros.FF_Array_Elems(const_Values))
									{
										Log.Av_Log(obj, Log.Av_Log_Error, "const_values array too small for %s\n", o.Unit);

										return Error.PatchWelcome;
									}

									const_Names[ci] = o_Named.Name;
									const_Values[ci++] = Default_NumVal(o_Named);
								}
							}
						}

						const_Names[ci] = "default".ToCharPointer();
						const_Values[ci++] = Default_NumVal(o);
						const_Names[ci] = "max".ToCharPointer();
						const_Values[ci++] = o.Max;
						const_Names[ci] = "min".ToCharPointer();
						const_Values[ci++] = o.Min;
						const_Names[ci] = "none".ToCharPointer();
						const_Values[ci++] = 0;
						const_Names[ci] = "all".ToCharPointer();
						const_Values[ci++] = ~0;
						const_Names[ci] = null;
						const_Values[ci] = 0;

						c_int res = Eval.Av_Expr_Parse_And_Eval(out d, i != 0 ? buf : val, const_Names, const_Values, null, null, null, null, null, 0, obj);
						if (res < 0)
						{
							Log.Av_Log(obj, Log.Av_Log_Error, "Unable to parse \"%s\" option value \"%s\"\n", o.Name, val);

							return res;
						}
					}
				}

				if (type == AvOptionType.Flags)
				{
					intNum = (c_uint)(c_int)dst;

					if (cmd == '+')
						d = intNum | (int64_t)d;
					else if (cmd == '-')
						d = intNum & ~(int64_t)d;
				}

				ret = Write_Number(obj, o, ref dst, d, 1, 1);
				if (ret < 0)
					return ret;

				val += i;

				if ((i == 0) || (val[0] == 0))
					return 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Set_String_Image_Size(IOptionContext obj, AvOption o, CPointer<char> val, ref SizeInfo dst)//XX 514
		{
			if (val.IsNull || (CString.strcmp(val, "none") == 0))
			{
				dst.Width = 0;
				dst.Height = 0;

				return 0;
			}

			c_int ret = ParseUtils.Av_Parse_Video_Size(out dst.Width, out dst.Height, val);

			if (ret < 0)
				Log.Av_Log(obj, Log.Av_Log_Error, "Unable to parse \"%s\" option value \"%s\" as image size\n", o.Name, val);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Set_String_Video_Rate(IOptionContext obj, AvOption o, CPointer<char> val, out AvRational dst)//XX 529
		{
			c_int ret = ParseUtils.Av_Parse_Video_Rate(out dst, val);

			if (ret < 0)
				Log.Av_Log(obj, Log.Av_Log_Error, "Unable to parse \"%s\" option value \"%s\" as video rate\n", o.Name, val);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Set_String_Color(IOptionContext obj, AvOption o, CPointer<char> val, ref ColorInfo dst)//XX 537
		{
			if (val.IsNull)
				return 0;
			else
			{
				c_int ret = ParseUtils.Av_Parse_Color(dst.Color, val, -1, obj);

				if (ret < 0)
					Log.Av_Log(obj, Log.Av_Log_Error, "Unable to parse \"%s\" option value \"%s\" as color\n", o.Name, val);

				return ret;
			}

//			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static CPointer<char> Get_Bool_Name(c_int val)//XX 552
		{
			if (val < 0)
				return "auto".ToCharPointer();

			return (val != 0 ? "true" : "false").ToCharPointer();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Set_String_Bool(IOptionContext obj, AvOption o, CPointer<char> val, ref c_int dst)//XX 559
		{
			c_int n;

			if (val.IsNull)
				return 0;

			if (CString.strcmp(val, "auto") == 0)
				n = -1;
			else if (AvString.Av_Match_Name(val, "true,y,yes,enable,enabled,on".ToCharPointer()) != 0)
				n = 1;
			else if (AvString.Av_Match_Name(val, "false,n,no,disable,disabled,off".ToCharPointer()) != 0)
				n = 0;
			else
			{
				n = (c_int)CString.strtol(val, out CPointer<char> end, 10, out bool _);

				if ((val + CString.strlen(val)) != end)
					goto Fail;
			}

			if ((n < o.Min) || (n > o.Max))
				goto Fail;

			dst = n;

			return 0;

			Fail:
			Log.Av_Log(obj, Log.Av_Log_Error, "Unable to parse \"%s\" option value \"%s\" as boolean\n", o.Name, val);

			return Error.EINVAL;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Set_String_Fmt(IOptionContext obj, AvOption o, CPointer<char> val, ref c_int dst, c_int fmt_Nb, GetFmt_Delegate get_Fmt, CPointer<char> desc)//XX 590
		{
			c_int fmt;

			if (val.IsNull || (CString.strcmp(val, "none") == 0))
				fmt = -1;
			else
			{
				fmt = get_Fmt(val);
				if (fmt == -1)
				{
					fmt = (c_int)CString.strtol(val, out CPointer<char> tail, 0, out bool _);

					if ((tail[0] != '\0') || ((c_uint)fmt >= fmt_Nb))
					{
						Log.Av_Log(obj, Log.Av_Log_Error, "Unable to parse \"%s\" option value \"%s\" as %s\n", o.Name, val, desc);

						return Error.EINVAL;
					}
				}
			}

			c_int min = Macros.FFMax((c_int)o.Min, -1);
			c_int max = Macros.FFMin((c_int)o.Max, fmt_Nb - 1);

			// Hack for compatibility with old ffmpeg
			if ((min == 0) && (max == 0))
			{
				min = -1;
				max = fmt_Nb - 1;
			}

			if ((fmt < min) || (fmt > max))
			{
				Log.Av_Log(obj, Log.Av_Log_Error, "Value %d for parameter '%s' out of %s format range [%d - %d]\n", fmt, o.Name, desc, min, max);

				return Error.ERANGE;
			}

			dst = fmt;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Get_Pix_Fmt(CPointer<char> name)//XX 630
		{
			return (c_int)PixDesc.Av_Get_Pix_Fmt(name);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Set_String_Pixel_Fmt(IOptionContext obj, AvOption o, CPointer<char> val, ref AvPixelFormat dst)//XX 646
		{
			c_int t = (c_int)dst;
			c_int ret = Set_String_Fmt(obj, o, val, ref t, (c_int)AvPixelFormat.Nb, Get_Pix_Fmt, "pixel format".ToCharPointer());
			dst = (AvPixelFormat)t;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Get_Sample_Fmt(CPointer<char> name)//XX 641
		{
			return (c_int)SampleFmt.Av_Get_Sample_Fmt(name);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Set_String_Sample_Fmt(IOptionContext obj, AvOption o, CPointer<char> val, ref AvSampleFormat dst)//XX 646
		{
			c_int t = (c_int)dst;
			c_int ret = Set_String_Fmt(obj, o, val, ref t, (c_int)AvSampleFormat.Nb, Get_Sample_Fmt, "sample format".ToCharPointer());
			dst = (AvSampleFormat)t;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Set_String_Dict(IOptionContext obj, AvOption o, CPointer<char> val, ref AvDictionary dst)//XX 652
		{
			AvDictionary options = null;

			if (val.IsNotNull)
			{
				c_int ret = Dict.Av_Dict_Parse_String(ref options, val, "=", ":", 0);
				if (ret < 0)
				{
					Dict.Av_Dict_Free(ref options);
					return ret;
				}
			}

			Dict.Av_Dict_Free(ref dst);
			dst = options;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Set_String_Channel_Layout(IOptionContext obj, AvOption o, CPointer<char> val, ref object dst)//XX 670
		{
			AvChannelLayout channel_Layout = (AvChannelLayout)dst;

			Channel_Layout.Av_Channel_Layout_Uninit(channel_Layout);

			if (val.IsNull)
				return 0;

			c_int ret = Channel_Layout.Av_Channel_Layout_From_String(ref channel_Layout, val);
			dst = channel_Layout;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Opt_Set_Elem(IOptionContext obj, IOptionContext target_Obj, AvOption o, CPointer<char> val, ref object dst)//XX 680
		{
			AvOptionType type = Type_Base(o.Type);
			c_int ret;

			if (val.IsNull && (type != AvOptionType.String) &&
								(type != AvOptionType.Pixel_Fmt) && (type != AvOptionType.Sample_Fmt) &&
								(type != AvOptionType.Image_Size) &&
								(type != AvOptionType.Duration) && (type != AvOptionType.Color) &&
								(type != AvOptionType.Bool))
			{
				return Error.EINVAL;
			}

			switch (type)
			{
				case AvOptionType.Bool:
				{
					c_int t = (c_int)dst;
					ret = Set_String_Bool(obj, o, val, ref t);
					dst = t;

					return ret;
				}

				case AvOptionType.String:
				{
					CPointer<char> t = (CPointer<char>)dst;
					ret = Set_String(obj, o, val, ref t);
					dst = t;

					return ret;
				}

				case AvOptionType.Binary:
				{
					PtrInfo<uint8_t> t = (PtrInfo<uint8_t>)dst;
					ret = Set_String_Binary(obj, o, val, ref t);
					dst = t;

					return ret;
				}

				case AvOptionType.Flags:
				case AvOptionType.Int:
				case AvOptionType.UInt:
				case AvOptionType.Int64:
				case AvOptionType.UInt64:
				case AvOptionType.Float:
				case AvOptionType.Double:
				case AvOptionType.Rational:
					return Set_String_Number(obj, target_Obj, o, val, ref dst);

				case AvOptionType.Image_Size:
				{
					SizeInfo t = (SizeInfo)dst;
					ret = Set_String_Image_Size(obj, o, val, ref t);
					dst = t;

					return ret;
				}

				case AvOptionType.Video_Rate:
				{
					ret = Set_String_Video_Rate(obj, o, val, out AvRational tmp);
					if (ret < 0)
						return ret;

					return Write_Number(obj, o, ref dst, 1, tmp.Den, tmp.Num);
				}

				case AvOptionType.Pixel_Fmt:
				{
					AvPixelFormat t = (AvPixelFormat)dst;
					ret = Set_String_Pixel_Fmt(obj, o, val, ref t);
					dst = t;

					return ret;
				}

				case AvOptionType.Sample_Fmt:
				{
					AvSampleFormat t = (AvSampleFormat)dst;
					ret = Set_String_Sample_Fmt(obj, o, val, ref t);
					dst = t;

					return ret;
				}

				case AvOptionType.Duration:
				{
					int64_t usecs = 0;

					if (val.IsNotNull)
					{
						ret = ParseUtils.Av_Parse_Time(out usecs, val, 1);

						if (ret < 0)
						{
							Log.Av_Log(obj, Log.Av_Log_Error, "Unable to parse \"%s\" option value \"%s\" as duration\n", o.Name, val);

							return ret;
						}
					}

					if ((usecs < o.Min) || (usecs > o.Max))
					{
						Log.Av_Log(obj, Log.Av_Log_Error, "Value %f for parameter '%s' out of range [%g - %g]\n", usecs / 1000000.0, o.Name, o.Min / 1000000.0, o.Max / 1000000.0);

						return Error.ERANGE;
					}

					dst = usecs;

					return 0;
				}

				case AvOptionType.Color:
				{
					ColorInfo t = (ColorInfo)dst;
					ret = Set_String_Color(obj, o, val, ref t);
					dst = t;

					return ret;
				}

				case AvOptionType.ChLayout:
				{
					ret = Set_String_Channel_Layout(obj, o, val, ref dst);

					if (ret < 0)
					{
						Log.Av_Log(obj, Log.Av_Log_Error, "Unable to parse \"%s\" option value \"%s\" as channel layout\n", o.Name, val);

						ret = Error.EINVAL;
					}

					return ret;
				}

				case AvOptionType.Dict:
				{
					AvDictionary t = (AvDictionary)dst;
					ret = Set_String_Dict(obj, o, val, ref t);
					dst = t;

					return ret;
				}
			}

			Log.Av_Log(obj, Log.Av_Log_Error, "Invalid option type.\n");

			return Error.EINVAL;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Opt_Set_Array(IOptionContext obj, IOptionContext target_Obj, AvOption o, CPointer<char> val, ref object dst)//XX 756
		{
			Type_Desc type = opt_Type_Desc[(c_int)Type_Base(o.Type)];

			c_int ret = type.SetArrayFunc(type, obj, target_Obj, o, val, ref dst);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Opt_Set_Array_Helper<T>(Type_Desc type, IOptionContext obj, IOptionContext target_Obj, AvOption o, CPointer<char> val, ref object dst)
		{
			ArrayInfo<T> arrayDst = (ArrayInfo<T>)dst;

			AvOptionArrayDef arr = o.Default_Value.Arr != null ? o.Default_Value.Arr[0] : null;
			char sep = Opt_Array_Sep(o);
			CPointer<char> str = null;

			CPointer<T> elems = null;
			c_uint nb_Elems = 0;
			c_int ret;

			if (val.IsNotNull && (val[0] != 0))
			{
				str = Mem.Av_MAlloc<char>(CString.strlen(val) + 1);
				if (str.IsNull)
					return Error.ENOMEM;
			}

			// Split and unescape the string
			while (val.IsNotNull && (val[0] != 0))
			{
				CPointer<char> p = str;

				if ((arr != null) && (arr.Size_Max != 0) && (nb_Elems >= arr.Size_Max))
				{
					Log.Av_Log(obj, Log.Av_Log_Error, "Cannot assign more than %u elements to array option %s\n", arr.Size_Max, o.Name);

					ret = Error.EINVAL;
					goto Fail;
				}

				for (; val[0] != 0; val++, p++)
				{
					if ((val[0] == '\\') && (val[1] != 0))
						val++;
					else if (val[0] == sep)
					{
						val++;
						break;
					}

					p[0] = val[0];
				}

				p[0] = '\0';

				CPointer<T> tmp = (CPointer<T>)type.ReallocArrayFunc(elems, nb_Elems + 1);
				if (tmp.IsNull)
				{
					ret = Error.ENOMEM;
					goto Fail;
				}

				elems = tmp;

				object tmpElem = elems[nb_Elems];

				ret = Opt_Set_Elem(obj, target_Obj, o, str, ref tmpElem);
				if (ret < 0)
					goto Fail;

				elems[nb_Elems] = (T)tmpElem;

				nb_Elems++;
			}

			Mem.Av_FreeP(ref str);

			Opt_Free_Array(o, ref arrayDst.Array, ref arrayDst.Count);

			if ((arr != null) && (nb_Elems < arr.Size_Min))
			{
				Log.Av_Log(obj, Log.Av_Log_Error, "Cannot assign fewer than %u elements to array option %s\n", arr.Size_Min, o.Name);

				ret = Error.EINVAL;
				goto Fail;
			}

			arrayDst.Array = elems;
			arrayDst.Count = nb_Elems;
			dst = arrayDst;

			return 0;

			Fail:
			Mem.Av_FreeP(ref str);
			Opt_Free_Array(o, ref elems, ref nb_Elems);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Set_Number(IOptionContext obj, CPointer<char> name, c_double num, c_int den, int64_t intNum, AvOptSearch search_Flags, AvOptionType require_Type)//XX 866
		{
			c_int ret = Opt_Set_Init(obj, name, search_Flags, require_Type, out _, out AvOption o, out object dst);

			if (ret < 0)
				return ret;

			return Write_Number(obj, o, ref dst, num, den, intNum);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Format_Duration(CPointer<char> buf, size_t size, int64_t d)//XX 1015
		{
			if ((d < 0) && (d != int64_t.MinValue))
			{
				buf[0, 1] = '-';
				size--;
				d = -d;
			}

			if (d == int64_t.MaxValue)
				CString.snprintf(buf, size, "INT64_MAX");
			else if (d == int64_t.MinValue)
				CString.snprintf(buf, size, "INT64_MIN");
			else if (d > (int64_t)3600 * 1000000)
				CString.snprintf(buf, size, "%lld:%02d:%02d.%06d", d / 3600000000, (c_int)((d / 60000000) % 60), (c_int)((d / 1000000) % 60), (c_int)(d % 1000000));
			else if (d > 60 * 1000000)
				CString.snprintf(buf, size, "%d:%02d.%06d", (c_int)(d / 60000000), (c_int)((d / 1000000) % 60), (c_int)(d % 1000000));
			else
				CString.snprintf(buf, size, "%d.%06d", (c_int)(d / 1000000), (c_int)(d % 1000000));

			CPointer<char> e = buf + CString.strlen(buf);

			while ((e > buf) && (e[-1] == '0'))
				e[-1, -1] = '\0';

			if ((e > buf) && (e[-1] == '.'))
				e[-1, -1] = '\0';
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Opt_Get_Elem(AvOption o, ref CPointer<char> pBuf, size_t buf_Len, object dst, AvOptSearch search_Flags)//XX 1050
		{
			c_int ret;

			switch (Type_Base(o.Type))
			{
				case AvOptionType.Bool:
				{
					ret = CString.snprintf(pBuf, buf_Len, "%s", Get_Bool_Name((c_int)dst));
					break;
				}

				case AvOptionType.Flags:
				{
					ret = CString.snprintf(pBuf, buf_Len, "0x%08X", (c_int)dst);
					break;
				}

				case AvOptionType.Int:
				{
					ret = CString.snprintf(pBuf, buf_Len, "%d", (c_int)dst);
					break;
				}

				case AvOptionType.UInt:
				{
					ret = CString.snprintf(pBuf, buf_Len, "%u", (c_uint)dst);
					break;
				}

				case AvOptionType.Int64:
				{
					ret = CString.snprintf(pBuf, buf_Len, "%lld", (int64_t)dst);
					break;
				}

				case AvOptionType.UInt64:
				{
					ret = CString.snprintf(pBuf, buf_Len, "%llu", (uint64_t)dst);
					break;
				}

				case AvOptionType.Float:
				{
					ret = CString.snprintf(pBuf, buf_Len, "%f", (c_float)dst);
					break;
				}

				case AvOptionType.Double:
				{
					ret = CString.snprintf(pBuf, buf_Len, "%f", (c_double)dst);
					break;
				}

				case AvOptionType.Video_Rate:
				case AvOptionType.Rational:
				{
					ret = CString.snprintf(pBuf, buf_Len, "%d/%d", ((AvRational)dst).Num, ((AvRational)dst).Den);
					break;
				}

				case AvOptionType.Const:
				{
					ret = CString.snprintf(pBuf, buf_Len, "%lld", o.Default_Value.I64);
					break;
				}

				case AvOptionType.String:
				{
					if (((CPointer<char>)dst).IsNotNull)
						pBuf = Mem.Av_StrDup((CPointer<char>)dst);
					else if ((search_Flags & AvOptSearch.Allow_Null) != 0)
					{
						pBuf.SetToNull();

						return 0;
					}
					else
						pBuf = Mem.Av_StrDup(CString.Empty);

					return pBuf.IsNotNull ? 0 : Error.ENOMEM;
				}

				case AvOptionType.Binary:
				{
					if ((((PtrInfo<uint8_t>)dst).Ptr.IsNull) && ((search_Flags & AvOptSearch.Allow_Null) != 0))
					{
						pBuf.SetToNull();

						return 0;
					}

					c_int len = (c_int)((PtrInfo<uint8_t>)dst).Len;

					if ((((uint64_t)len * 2) + 1) > c_int.MaxValue)
						return Error.EINVAL;

					pBuf = Mem.Av_MAlloc<char>((size_t)((len * 2) + 1));
					if (pBuf.IsNull)
						return Error.ENOMEM;

					if (len == 0)
					{
						pBuf[0] = '\0';

						return 0;
					}

					PtrInfo<uint8_t> bin = (PtrInfo<uint8_t>)dst;

					for (c_int i = 0; i < len; i++)
						CString.snprintf(pBuf + (i * 2), 3, "%02X", bin.Ptr[i]);

					return 0;
				}

				case AvOptionType.Image_Size:
				{
					ret = CString.snprintf(pBuf, buf_Len, "%dx%d", ((SizeInfo)dst).Width, ((SizeInfo)dst).Height);
					break;
				}

				case AvOptionType.Pixel_Fmt:
				{
					ret = CString.snprintf(pBuf, buf_Len, "%s", AvUtil.Av_X_If_Null(PixDesc.Av_Get_Pix_Fmt_Name((AvPixelFormat)dst), "none".ToCharPointer()));
					break;
				}

				case AvOptionType.Sample_Fmt:
				{
					ret = CString.snprintf(pBuf, buf_Len, "%s", AvUtil.Av_X_If_Null(SampleFmt.Av_Get_Sample_Fmt_Name((AvSampleFormat)dst), "none".ToCharPointer()));
					break;
				}

				case AvOptionType.Duration:
				{
					int64_t i64 = (int64_t)dst;
					Format_Duration(pBuf, buf_Len, i64);

					ret = (c_int)CString.strlen(pBuf);
					break;
				}

				case AvOptionType.Color:
				{
					ColorInfo c = (ColorInfo)dst;

					ret = CString.snprintf(pBuf, buf_Len, "0x%02x%02x%02x%02x", (c_int)c.Color[0], (c_int)c.Color[1], (c_int)c.Color[2], (c_int)c.Color[3]);
					break;
				}

				case AvOptionType.ChLayout:
				{
					ret = Channel_Layout.Av_Channel_Layout_Describe((AvChannelLayout)dst, pBuf, buf_Len);
					break;
				}

				case AvOptionType.Dict:
				{
					if ((((AvDictionary)dst) == null) && ((search_Flags & AvOptSearch.Allow_Null) != 0))
					{
						pBuf.SetToNull();

						return 0;
					}

					return Dict.Av_Dict_Get_String((AvDictionary)dst, out pBuf, '=', ':');
				}

				default:
					return Error.EINVAL;
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Opt_Get_Array(AvOption o, object dst, out CPointer<char> out_Val)//XX 1155
		{
			Type_Desc type = opt_Type_Desc[(c_int)Type_Base(o.Type)];

			c_int ret = type.GetArrayFunc(o, dst, out out_Val);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Opt_Get_Array_Helper<T>(AvOption o, object dst, out CPointer<char> out_Val)
		{
			c_uint count = ((ArrayInfo<T>)dst).Count;
			char sep = Opt_Array_Sep(o);

			CPointer<char> str = null;
			size_t str_Len = 0;
			c_int ret;

			out_Val = null;

			for (c_uint i = 0; i < count; i++)
			{
				CPointer<char> buf = new CPointer<char>(128);
				CPointer<char> @out = buf;
				size_t out_Len;

				ret = Opt_Get_Elem(o, ref @out, (size_t)buf.Length, ((ArrayInfo<T>)dst).Array[i], AvOptSearch.None);

				if (ret < 0)
					goto Fail;

				out_Len = CString.strlen(@out);

				if ((out_Len > ((size_t.MaxValue / 2U) - (i != 0 ? 1U : 0U))) || (((i != 0 ? 1U : 0U) + (out_Len * 2)) > (size_t.MaxValue - str_Len - 1)))
				{
					ret = Error.ERANGE;
					goto Fail;
				}

				//                                     Terminator       Escaping   Separator
				//                                            ↓               ↓      ↓
				ret = Mem.Av_ReallocP(ref str, str_Len + 1U + (out_Len * 2U) + (i != 0 ? 1U : 0U));

				if (ret < 0)
					goto Fail;

				// Add separator if needed
				if (i != 0)
					str[str_Len++] = sep;

				// Escape the element
				for (c_uint j = 0; j < out_Len; j++)
				{
					char val = @out[j];

					if ((val == sep) || (val == '\\'))
						str[str_Len++] = '\\';

					str[str_Len++] = val;
				}

				str[str_Len] = '\0';

				Fail:
				if (@out != buf)
					Mem.Av_FreeP(ref @out);

				if (ret < 0)
				{
					Mem.Av_FreeP(ref str);

					return ret;
				}
			}

			out_Val = str;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Log_Int_Value(IClass av_Log_Obj, c_int level, int64_t i)//XX 1412
		{
			if (i == c_int.MaxValue)
				Log.Av_Log(av_Log_Obj, level, "INT_MAX");
			else if (i == c_int.MinValue)
				Log.Av_Log(av_Log_Obj, level, "INT_MIN");
			else if (i == uint32_t.MaxValue)
				Log.Av_Log(av_Log_Obj, level, "UINT32_MAX");
			else if (i == int64_t.MaxValue)
				Log.Av_Log(av_Log_Obj, level, "I64_MAX");
			else if (i == int64_t.MinValue)
				Log.Av_Log(av_Log_Obj, level, "I64_MIN");
			else
				Log.Av_Log(av_Log_Obj, level, "%lld", i);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Log_Value(IClass av_Log_Obj, c_int level, c_double d)//XX 1429
		{
			if (d == c_int.MaxValue)
				Log.Av_Log(av_Log_Obj, level, "INT_MAX");
			else if (d == c_int.MinValue)
				Log.Av_Log(av_Log_Obj, level, "INT_MIN");
			else if (d == uint32_t.MaxValue)
				Log.Av_Log(av_Log_Obj, level, "UINT32_MAX");
			else if (d == int64_t.MaxValue)
				Log.Av_Log(av_Log_Obj, level, "I64_MAX");
			else if (d == int64_t.MinValue)
				Log.Av_Log(av_Log_Obj, level, "I64_MIN");
			else if (d == CMath.FLT_MAX)
				Log.Av_Log(av_Log_Obj, level, "FLT_MAX");
			else if (d == CMath.FLT_MIN)
				Log.Av_Log(av_Log_Obj, level, "FLT_MIN");
			else if (d == -CMath.FLT_MAX)
				Log.Av_Log(av_Log_Obj, level, "-FLT_MAX");
			else if (d == -CMath.FLT_MIN)
				Log.Av_Log(av_Log_Obj, level, "-FLT_MIN");
			else if (d == CMath.DBL_MAX)
				Log.Av_Log(av_Log_Obj, level, "DBL_MAX");
			else if (d == CMath.DBL_MIN)
				Log.Av_Log(av_Log_Obj, level, "DBL_MIN");
			else if (d == -CMath.DBL_MAX)
				Log.Av_Log(av_Log_Obj, level, "-DBL_MAX");
			else if (d == -CMath.DBL_MIN)
				Log.Av_Log(av_Log_Obj, level, "-DBL_MIN");
			else
				Log.Av_Log(av_Log_Obj, level, "%g", d);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static CPointer<char> Get_Opt_Const_Name(IOptionContext obj, CPointer<char> unit, int64_t value)//XX 1462
		{
			if (unit.IsNull)
				return null;

			foreach (AvOption opt in Av_Opt_Next(obj))
			{
				if ((opt.Type == AvOptionType.Const) && (CString.strcmp(opt.Unit, unit) == 0) && (opt.Default_Value.I64 == value))
					return opt.Name;
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static CPointer<char> Get_Opt_Flags_String(IOptionContext obj, CPointer<char> unit, int64_t value)//XX 1475
		{
			CPointer<char> flags = new CPointer<char>(512);
			flags[0] = '\0';

			if (unit.IsNull)
				return null;

			foreach (AvOption opt in Av_Opt_Next(obj))
			{
				if ((opt.Type == AvOptionType.Const) && (CString.strcmp(opt.Unit, unit) == 0) && ((opt.Default_Value.I64 & value) != 0))
				{
					if (flags[0] != '\0')
						AvString.Av_Strlcatf(flags, (size_t)flags.Length, "+");

					AvString.Av_Strlcatf(flags, (size_t)flags.Length, "%s", opt.Name);
				}
			}

			if (flags[0] != '\0')
				return Mem.Av_StrDup(flags);

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Log_Type(IClass av_Log_Obj, AvOption o, AvOptionType parent_Type)//XX 1496
		{
			AvOptionType type = Type_Base(o.Type);

			if ((o.Type == AvOptionType.Const) && ((Type_Base(parent_Type) == AvOptionType.Int) || (Type_Base(parent_Type) == AvOptionType.Int64)))
				Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, "%-12lld ", o.Default_Value.I64);
			else if (((size_t)type < Macros.FF_Array_Elems(opt_Type_Desc)) && opt_Type_Desc[(c_int)type].Name.IsNotNull)
			{
				if ((o.Type & AvOptionType.Array) != 0)
					Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, "[%-10s]", opt_Type_Desc[(c_int)type].Name);
				else
					Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, "%-12s ", opt_Type_Desc[(c_int)type].Name);
			}
			else
				Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, "%-12s ", string.Empty);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Log_Default(IOptionContext obj, IClass av_Log_Obj, AvOption opt)//XX 1513
		{
			if ((opt.Type == AvOptionType.Const) || (opt.Type == AvOptionType.Binary))
				return;

			if (((opt.Type == AvOptionType.Color) || (opt.Type == AvOptionType.Image_Size) || (opt.Type == AvOptionType.String) ||
			    (opt.Type == AvOptionType.Dict) || (opt.Type == AvOptionType.ChLayout) || (opt.Type == AvOptionType.Video_Rate)) && opt.Default_Value.Str.IsNull)
				return;

			if ((opt.Type & AvOptionType.Array) != 0)
			{
				CPointer<AvOptionArrayDef> arr = opt.Default_Value.Arr;

				if (arr.IsNotNull && arr[0].Def.IsNotNull)
					Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, " (default %s)", arr[0].Def);

				return;
			}

			Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, " (default ");

			switch (opt.Type)
			{
				case AvOptionType.Bool:
				{
					Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, "%s", Get_Bool_Name((c_int)opt.Default_Value.I64));
					break;
				}

				case AvOptionType.Flags:
				{
					CPointer<char> def_Flags = Get_Opt_Flags_String(obj, opt.Unit, opt.Default_Value.I64);

					if (def_Flags.IsNotNull)
					{
						Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, "%s", def_Flags);
						Mem.Av_FreeP(ref def_Flags);
					}
					else
						Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, "%lld", opt.Default_Value.I64);

					break;
				}

				case AvOptionType.Duration:
				{
					CPointer<char> buf = new CPointer<char>(25);

					Format_Duration(buf, (size_t)buf.Length, opt.Default_Value.I64);
					Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, "%s", buf);
					break;
				}

				case AvOptionType.UInt:
				case AvOptionType.Int:
				case AvOptionType.UInt64:
				case AvOptionType.Int64:
				{
					CPointer<char> def_Const = Get_Opt_Const_Name(obj, opt.Unit, opt.Default_Value.I64);

					if (def_Const.IsNotNull)
						Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, "%s", def_Const);
					else
						Log_Int_Value(av_Log_Obj, Log.Av_Log_Info, opt.Default_Value.I64);

					break;
				}

				case AvOptionType.Double:
				case AvOptionType.Float:
				{
					Log_Value(av_Log_Obj, Log.Av_Log_Info, opt.Default_Value.Dbl);
					break;
				}

				case AvOptionType.Rational:
				{
					AvRational q = Rational.Av_D2Q(opt.Default_Value.Dbl, c_int.MaxValue);
					Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, "%d/%d", q.Num, q.Den);
					break;
				}

				case AvOptionType.Pixel_Fmt:
				{
					Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, "%s", AvUtil.Av_X_If_Null(PixDesc.Av_Get_Pix_Fmt_Name((AvPixelFormat)opt.Default_Value.I64), "none".ToCharPointer()));
					break;
				}

				case AvOptionType.Sample_Fmt:
				{
					Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, "%s", AvUtil.Av_X_If_Null(SampleFmt.Av_Get_Sample_Fmt_Name((AvSampleFormat)opt.Default_Value.I64), "none".ToCharPointer()));
					break;
				}

				case AvOptionType.Color:
				case AvOptionType.Image_Size:
				case AvOptionType.String:
				case AvOptionType.Dict:
				case AvOptionType.Video_Rate:
				case AvOptionType.ChLayout:
				{
					Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, "\"%s\"", opt.Default_Value.Str);
					break;
				}
			}

			Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, ")");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Opt_List(IOptionContext obj, IClass av_Log_Obj, CPointer<char> unit, AvOptFlag req_Flags, AvOptFlag rej_Flags, AvOptionType parent_Type)//XX 1591
		{
			foreach (AvOption opt in Av_Opt_Next(obj))
			{
				if (((opt.Flags & req_Flags) == 0) || ((opt.Flags & rej_Flags) != 0))
					continue;

				// Don't print CONST's on level one.
				// Don't print anything but CONST's on level two.
				// Only print items from the requested unit.
				if (unit.IsNull && (opt.Type == AvOptionType.Const))
					continue;
				else if (unit.IsNotNull && (opt.Type != AvOptionType.Const))
					continue;
				else if (unit.IsNotNull && (opt.Type == AvOptionType.Const) && (CString.strcmp(unit, opt.Unit) != 0))
					continue;
				else if (unit.IsNotNull && (opt.Type == AvOptionType.Const))
					Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, "     %-15s ", opt.Name);
				else
					Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, "  %s%-17s ", (opt.Flags & AvOptFlag.Filtering_Param) != 0 ? " " : "-", opt.Name);

				Log_Type(av_Log_Obj, opt, parent_Type);

				Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, "%c%c%c%c%c%c%c%c%c%c%c",
					(opt.Flags & AvOptFlag.Encoding_Param) != 0 ? 'E' : '.',
					(opt.Flags & AvOptFlag.Decoding_Param) != 0 ? 'D' : '.',
					(opt.Flags & AvOptFlag.Filtering_Param) != 0 ? 'F' : '.',
					(opt.Flags & AvOptFlag.Video_Param) != 0 ? 'V' : '.',
					(opt.Flags & AvOptFlag.Audio_Param) != 0 ? 'A' : '.',
					(opt.Flags & AvOptFlag.Subtitle_Param) != 0 ? 'S' : '.',
					(opt.Flags & AvOptFlag.Export) != 0 ? 'X' : '.',
					(opt.Flags & AvOptFlag.Readonly) != 0 ? 'R' : '.',
					(opt.Flags & AvOptFlag.Bsf_Param) != 0 ? 'B' : '.',
					(opt.Flags & AvOptFlag.Runtime_Param) != 0 ? 'T' : '.',
					(opt.Flags & AvOptFlag.Deprecated) != 0 ? 'P' : '.');

				if (opt.Help.IsNotNull)
					Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, " %s", opt.Help);

				if (Av_Opt_Query_Ranges(out AvOptionRanges r, obj, opt.Name, AvOptSearch.Search_Fake_Obj) >= 0)
				{
					switch (opt.Type)
					{
						case AvOptionType.Int:
						case AvOptionType.UInt:
						case AvOptionType.Int64:
						case AvOptionType.UInt64:
						case AvOptionType.Double:
						case AvOptionType.Float:
						case AvOptionType.Rational:
						{
							for (c_int i = 0; i < r.Nb_Ranges; i++)
							{
								Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, " (from ");
								Log_Value(av_Log_Obj, Log.Av_Log_Info, r.Range[i].Value_Min);
								Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, " to ");
								Log_Value(av_Log_Obj, Log.Av_Log_Info, r.Range[i].Value_Max);
								Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, ")");
							}

							break;
						}
					}

					Av_Opt_FreeP_Ranges(ref r);
				}

				Log_Default(obj, av_Log_Obj, opt);

				Log.Av_Log(av_Log_Obj, Log.Av_Log_Info, "\n");

				if (opt.Unit.IsNotNull && (opt.Type != AvOptionType.Const))
					Opt_List(obj, av_Log_Obj, opt.Unit, req_Flags, rej_Flags, opt.Type);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Store the value in the field in ctx that is named like key.
		/// ctx must be an AVClass context, storing is done using AVOptions
		/// </summary>
		/********************************************************************/
		private static c_int Parse_Key_Value_Pair(IOptionContext ctx, ref CPointer<char> buf, CPointer<char> key_Val_Sep, CPointer<char> pairs_Sep)//XX 1783
		{
			CPointer<char> val;

			CPointer<char> key = AvString.Av_Get_Token(ref buf, key_Val_Sep);

			if (key.IsNull)
				return Error.ENOMEM;

			if ((key[0] != '\0') && (CString.strspn(buf, key_Val_Sep) != 0))
			{
				buf++;

				val = AvString.Av_Get_Token(ref buf, pairs_Sep);

				if (val.IsNull)
				{
					Mem.Av_FreeP(ref key);

					return Error.ENOMEM;
				}
			}
			else
			{
				Log.Av_Log(ctx, Log.Av_Log_Error, "Missing key or no key/value separator found after key '%s'\n", key);

				Mem.Av_Free(key);

				return Error.EINVAL;
			}

			Log.Av_Log(ctx, Log.Av_Log_Debug, "Setting entry with key '%s' to value '%s'\n", key, val);

			c_int ret = Av_Opt_Set(ctx, key, val, AvOptSearch.Search_Children);

			if (ret == Error.Option_Not_Found)
				Log.Av_Log(ctx, Log.Av_Log_Error, "Key '%s' not found.\n", key);

			Mem.Av_Free(key);
			Mem.Av_Free(val);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool Is_Key_Char(char c)//XX 1839
		{
			return ((c_uint)((c | 32) - 'a') < 26) || ((c_uint)(c - '0') < 10) || (c == '-') || (c == '_') || (c == '/') || (c == '.');
		}



		/********************************************************************/
		/// <summary>
		/// Read a key from a string.
		///
		/// The key consists of is_key_char characters and must be terminated
		/// by a character from the delim string; spaces are ignored
		/// </summary>
		/********************************************************************/
		private static c_int Get_Key(ref CPointer<char> rOpts, CPointer<char> delim, out CPointer<char> rKey)//XX 1854
		{
			rKey = null;

			CPointer<char> opts = rOpts;

			CPointer<char> key_Start = opts += CString.strspn(opts, AvString.Whitespaces);

			while (Is_Key_Char(opts[0]))
				opts++;

			CPointer<char> key_End = opts;
			opts += CString.strspn(opts, AvString.Whitespaces);

			if ((opts[0] == 0) || CString.strchr(delim, opts[0]).IsNull)
				return Error.EINVAL;

			opts++;

			rKey = Mem.Av_MAlloc<char>((size_t)(key_End - key_Start + 1));
			if (rKey.IsNull)
				return Error.ENOMEM;

			CMemory.memcpy(rKey, key_Start, (size_t)(key_End - key_Start));

			rKey[key_End - key_Start] = '\0';
			rOpts = opts;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Opt_Copy_Elem(IClass logCtx, AvOptionType type, ref object dst, object src)//XX 2069
		{
			if (type == AvOptionType.String)
			{
				CPointer<char> src_Str = (CPointer<char>)src;
				CPointer<char> dstP = (CPointer<char>)dst;

				if (dstP != src_Str)
					Mem.Av_FreeP(ref dstP);

				if (src_Str.IsNotNull)
				{
					dstP = Mem.Av_StrDup(src_Str);

					if (dstP.IsNull)
						return Error.ENOMEM;
				}

				dst = dstP;
			}
			else if (type == AvOptionType.Binary)
			{
				PtrInfo<uint8_t> src8 = (PtrInfo<uint8_t>)src;
				PtrInfo<uint8_t> dst8 = (PtrInfo<uint8_t>)dst;
				size_t len = src8.Len;

				if (dst8.Ptr != src8.Ptr)
					Mem.Av_FreeP(ref dst8.Ptr);

				dst8.Ptr = Mem.Av_MemDup(src8.Ptr, len);

				if ((len != 0) && dst8.Ptr.IsNull)
				{
					dst8.Len = 0;

					return Error.ENOMEM;
				}

				dst8.Len = len;
			}
			else if (type == AvOptionType.Const)
			{
				// Do nothing
			}
			else if (type == AvOptionType.Dict)
			{
				AvDictionary sDict = (AvDictionary)src;
				AvDictionary dDictP = (AvDictionary)dst;

				if (sDict != dDictP)
					Dict.Av_Dict_Free(ref dDictP);

				dDictP = null;

				c_int ret = Dict.Av_Dict_Copy(ref dDictP, sDict, AvDict.None);

				dst = dDictP;

				return ret;
			}
			else if (type == AvOptionType.ChLayout)
			{
				if (dst != src)
					return Channel_Layout.Av_Channel_Layout_Copy((AvChannelLayout)dst, (AvChannelLayout)src);
			}
			else if (Opt_Is_Pod(type))
			{
				dst = opt_Type_Desc[(c_int)type].CopyElementFunc(src);
			}
			else
			{
				Log.Av_Log(logCtx, Log.Av_Log_Error, "Unhandled option type: %d\n", type);

				return Error.EINVAL;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Opt_Copy_Array(IClass logCtx, AvOption o, ref object pDst, object pSrc)//XX 2117
		{
			Type_Desc type = opt_Type_Desc[(c_int)Type_Base(o.Type)];

			return type.CopyArrayFunc(type, logCtx, o, ref pDst, pSrc);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Opt_Copy_Array_Helper<T>(Type_Desc type, IClass logCtx, AvOption o, ref object pDst, object pSrc)
		{
			ArrayInfo<T> arraySrc = (ArrayInfo<T>)pSrc;
			ArrayInfo<T> arrayDst = (ArrayInfo<T>)pDst;

			c_uint nb_Elems = arraySrc.Count;

			if (pDst != pSrc)
			{
				arrayDst.Array = null;
				arrayDst.Count = 0;
			}

			Opt_Free_Array(o, ref arrayDst.Array, ref arrayDst.Count);
				
			CPointer<T> dst = (CPointer<T>)type.AllocArrayFunc(nb_Elems);

			if (dst == null)
				return Error.ENOMEM;

			for (c_uint i = 0; i < nb_Elems; i++)
			{
				object dstElem = dst[i];
				c_int ret = Opt_Copy_Elem(logCtx, Type_Base(o.Type), ref dstElem, arraySrc.Array[i]);
				dst[i] = (T)dstElem;

				if (ret < 0)
				{
					Opt_Free_Array(o, ref dst, ref nb_Elems);

					return ret;
				}
			}

			arrayDst.Array = dst;
			arrayDst.Count = nb_Elems;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Opt_Serialize(IOptionContext obj, AvOptFlag opt_Flags, AvSerialize flags, ref c_int cnt, AVBPrint bprint, char key_Val_Sep, char pairs_Sep)//XX 2727
		{
			c_int ret;

			char[] special_Chars = [ pairs_Sep, key_Val_Sep, '\0' ];

			if ((flags & AvSerialize.Search_Children) != 0)
			{
				foreach (IOptionContext child in Av_Opt_Child_Next(obj))
				{
					ret = Opt_Serialize(child, opt_Flags, flags, ref cnt, bprint, key_Val_Sep, pairs_Sep);

					if (ret < 0)
						return ret;
				}
			}

			foreach (AvOption o in Av_Opt_Next(obj))
			{
				if (o.Type == AvOptionType.Const)
					continue;

				if (((flags & AvSerialize.Opt_Flags_Exact) != 0) && (o.Flags != opt_Flags))
					continue;
				else if ((o.Flags & opt_Flags) != opt_Flags)
					continue;

				if (((flags & AvSerialize.Skip_Defaults) != 0) && (Av_Opt_Is_Set_To_Default(obj, o) > 0))
					continue;

				ret = Av_Opt_Get(obj, o.Name, AvOptSearch.None, out CPointer<char> buf);

				if (ret < 0)
				{
					BPrint.Av_BPrint_Finalize(bprint, out _);

					return ret;
				}

				if (buf.IsNotNull)
				{
					if (cnt++ != 0)
						BPrint.Av_BPrint_Append_Data(bprint, pairs_Sep.ToCharPointer(), 1);

					BPrint.Av_BPrint_Escape(bprint, o.Name, special_Chars, AvEscapeMode.Backslash, AvEscapeFlag.None);
					BPrint.Av_BPrint_Append_Data(bprint, key_Val_Sep.ToCharPointer(), 1);
					BPrint.Av_BPrint_Escape(bprint, buf, special_Chars, AvEscapeMode.Backslash, AvEscapeFlag.None);

					Mem.Av_FreeP(ref buf);
				}
			}

			return 0;
		}
		#endregion
	}
}
