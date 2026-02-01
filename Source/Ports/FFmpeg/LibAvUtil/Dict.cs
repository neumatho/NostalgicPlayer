/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class Dict
	{
		/********************************************************************/
		/// <summary>
		/// Get number of entries in dictionary
		/// </summary>
		/********************************************************************/
		public static c_int Av_Dict_Count(AvDictionary m)//XX 37
		{
			return m != null ? m.Count : 0;
		}



		/********************************************************************/
		/// <summary>
		/// Iterate over a dictionary
		///
		/// Iterates through all entries in the dictionary.
		///
		/// Warning: The returned AVDictionaryEntry key/value must not be
		/// changed.
		///
		/// Warning: As av_dict_set() invalidates all previous entries
		/// returned by this function, it must not be called while iterating
		/// over the dict
		/// </summary>
		/********************************************************************/
		public static IEnumerable<AvDictionaryEntry> Av_Dict_Iterate(AvDictionary m)//XX 42
		{
			if (m == null)
				yield break;

			for (c_int i = 0; i < m.Count; i++)
				yield return m.Elems[i];
		}



		/********************************************************************/
		/// <summary>
		/// Get a dictionary entry with matching key.
		///
		/// The returned entry key or value must not be changed, or it will
		/// cause undefined behavior
		/// </summary>
		/********************************************************************/
		public static IEnumerable<AvDictionaryEntry> Av_Dict_Get(AvDictionary m, string key, AvDict flags)
		{
			return Av_Dict_Get(m, key.ToCharPointer(), flags);
		}



		/********************************************************************/
		/// <summary>
		/// Get a dictionary entry with matching key.
		///
		/// The returned entry key or value must not be changed, or it will
		/// cause undefined behavior
		/// </summary>
		/********************************************************************/
		public static IEnumerable<AvDictionaryEntry> Av_Dict_Get(AvDictionary m, CPointer<char> key, AvDict flags)//XX 60
		{
			c_uint j;

			if (key.IsNull)
				yield break;

			foreach (AvDictionaryEntry entry in Av_Dict_Iterate(m))
			{
				CPointer<char> s = entry.Key;

				if ((flags & AvDict.Match_Case) != 0)
				{
					for (j = 0; (s[j] == key[j]) && (key[j] != 0); j++)
						;
				}
				else
				{
					for (j = 0; (AvString.Av_ToUpper(s[j]) == AvString.Av_ToUpper(key[j])) && (key[j] != 0); j++)
						;
				}

				if (key[j] != 0)
					continue;

				if ((s[j] != 0) && ((flags & AvDict.Ignore_Suffix) == 0))
					continue;

				yield return entry;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set the given entry in *pm, overwriting an existing entry.
		///
		/// Note: If AV_DICT_DONT_STRDUP_KEY or AV_DICT_DONT_STRDUP_VAL is
		/// set, these arguments will be freed on error.
		///
		/// Warning: Adding a new entry to a dictionary invalidates all
		/// existing entries previously returned with av_dict_get() or
		/// av_dict_iterate()
		/// </summary>
		/********************************************************************/
		public static c_int Av_Dict_Set(ref AvDictionary pm, string key, string value, AvDict flags)
		{
			return Av_Dict_Set(ref pm, key.ToCharPointer(), value.ToCharPointer(), flags);
		}



		/********************************************************************/
		/// <summary>
		/// Set the given entry in *pm, overwriting an existing entry.
		///
		/// Note: If AV_DICT_DONT_STRDUP_KEY or AV_DICT_DONT_STRDUP_VAL is
		/// set, these arguments will be freed on error.
		///
		/// Warning: Adding a new entry to a dictionary invalidates all
		/// existing entries previously returned with av_dict_get() or
		/// av_dict_iterate()
		/// </summary>
		/********************************************************************/
		public static c_int Av_Dict_Set(ref AvDictionary pm, string key, CPointer<char> value, AvDict flags)
		{
			return Av_Dict_Set(ref pm, key.ToCharPointer(), value, flags);
		}



		/********************************************************************/
		/// <summary>
		/// Set the given entry in *pm, overwriting an existing entry.
		///
		/// Note: If AV_DICT_DONT_STRDUP_KEY or AV_DICT_DONT_STRDUP_VAL is
		/// set, these arguments will be freed on error.
		///
		/// Warning: Adding a new entry to a dictionary invalidates all
		/// existing entries previously returned with av_dict_get() or
		/// av_dict_iterate()
		/// </summary>
		/********************************************************************/
		public static c_int Av_Dict_Set(ref AvDictionary pm, CPointer<char> key, CPointer<char> value, AvDict flags)//XX 86
		{
			AvDictionary m = pm;
			AvDictionaryEntry tag = null;
			CPointer<char> copy_Key = null, copy_Value = null;
			c_int err;

			if ((flags & AvDict.Dont_Strdup_Val) != 0)
				copy_Value = value;
			else if (value.IsNotNull)
				copy_Value = Mem.Av_StrDup(value);

			if (key.IsNull)
			{
				err = Error.EINVAL;
				goto Err_Out;
			}

			if ((flags & AvDict.Dont_Strdup_Key) != 0)
				copy_Key = key;
			else
				copy_Key = Mem.Av_StrDup(key);

			if (copy_Key.IsNull || (value.IsNotNull && copy_Value.IsNull))
				goto Enomem;

			if ((flags & AvDict.MultiKey) == 0)
				tag = Av_Dict_Get(m, key, flags).FirstOrDefault();
			else if ((flags & AvDict.Dedup) != 0)
			{
				foreach (AvDictionaryEntry _tag in Av_Dict_Get(m, key, flags))
				{
					tag = _tag;

					if ((value.IsNull && tag.Value.IsNull) || (value.IsNotNull && tag.Value.IsNotNull && (CString.strcmp(value, tag.Value) == 0)))
					{
						Mem.Av_Free(copy_Key);
						Mem.Av_Free(copy_Value);

						return 0;
					}
				}
			}

			if (m == null)
				m = pm = Mem.Av_MAlloczObj<AvDictionary>();

			if (m == null)
				goto Enomem;

			if (tag != null)
			{
				if ((flags & AvDict.Dont_Overwrite) != 0)
				{
					Mem.Av_Free(copy_Key);
					Mem.Av_Free(copy_Value);

					return 0;
				}

				if (copy_Value.IsNotNull && ((flags & AvDict.Append) != 0))
				{
					size_t oldLen = CString.strlen(tag.Value);
					size_t new_Part_Len = CString.strlen(copy_Value);
					size_t len = oldLen + new_Part_Len + 1;
					CPointer<char> newVal = Mem.Av_Realloc(tag.Value, len);

					if (newVal.IsNull)
						goto Enomem;

					CMemory.memcpy(newVal + oldLen, copy_Value, new_Part_Len + 1);
					Mem.Av_FreeP(ref copy_Value);
					copy_Value = newVal;
				}
				else
					Mem.Av_Free(tag.Value);

				Mem.Av_Free(tag.Key);
				tag = m.Elems[--m.Count];
			}
			else if (copy_Value.IsNotNull)
			{
				CPointer<AvDictionaryEntry> tmp = Mem.Av_Realloc_ArrayObj(m.Elems, (size_t)(m.Count + 1));

				if (tmp.IsNull)
					goto Enomem;

				m.Elems = tmp;
			}

			if (copy_Value.IsNotNull)
			{
				m.Elems[m.Count].Key = copy_Key;
				m.Elems[m.Count].Value = copy_Value;
				m.Count++;
			}
			else
			{
				err = 0;
				goto End;
			}

			return 0;

			Enomem:
			err = Error.ENOMEM;

			Err_Out:
			Mem.Av_Free(copy_Value);

			End:
			if ((m != null) && (m.Count == 0))
			{
				Mem.Av_FreeP(ref m.Elems);
				Mem.Av_FreeP(ref pm);
			}

			Mem.Av_Free(copy_Key);

			return err;
		}



		/********************************************************************/
		/// <summary>
		/// Convenience wrapper for av_dict_set() that converts the value to
		/// a string and stores it.
		///
		/// Note: If ::AV_DICT_DONT_STRDUP_KEY is set, key will be freed on
		/// error
		/// </summary>
		/********************************************************************/
		public static c_int Av_Dict_Set_Int(ref AvDictionary pm, string key, int64_t value, AvDict flags)//XX 177
		{
			return Av_Dict_Set_Int(ref pm, key.ToCharPointer(), value, flags);
		}



		/********************************************************************/
		/// <summary>
		/// Convenience wrapper for av_dict_set() that converts the value to
		/// a string and stores it.
		///
		/// Note: If ::AV_DICT_DONT_STRDUP_KEY is set, key will be freed on
		/// error
		/// </summary>
		/********************************************************************/
		public static c_int Av_Dict_Set_Int(ref AvDictionary pm, CPointer<char> key, int64_t value, AvDict flags)//XX 177
		{
			CPointer<char> valueStr = new CPointer<char>(22);

			CString.snprintf(valueStr, (size_t)valueStr.Length, "%lld", value);
			flags &= ~AvDict.Dont_Strdup_Val;

			return Av_Dict_Set(ref pm, key, valueStr, flags);
		}



		/********************************************************************/
		/// <summary>
		/// Parse the key/value pairs list and add the parsed entries to a
		/// dictionary.
		///
		/// In case of failure, all the successfully set entries are stored
		/// in *pm. You may need to manually free the created dictionary
		/// </summary>
		/********************************************************************/
		public static c_int Av_Dict_Parse_String(ref AvDictionary pm, CPointer<char> str, string key_Val_Sep, string pairs_Sep, AvDict flags)//XX 210
		{
			return Av_Dict_Parse_String(ref pm, str, key_Val_Sep.ToCharPointer(), pairs_Sep.ToCharPointer(), flags);
		}



		/********************************************************************/
		/// <summary>
		/// Parse the key/value pairs list and add the parsed entries to a
		/// dictionary.
		///
		/// In case of failure, all the successfully set entries are stored
		/// in *pm. You may need to manually free the created dictionary
		/// </summary>
		/********************************************************************/
		public static c_int Av_Dict_Parse_String(ref AvDictionary pm, CPointer<char> str, CPointer<char> key_Val_Sep, CPointer<char> pairs_Sep, AvDict flags)//XX 210
		{
			if (str.IsNull)
				return 0;

			// Ignore STRDUP flags
			flags &= ~(AvDict.Dont_Strdup_Key | AvDict.Dont_Strdup_Val);

			while (str[0] != 0)
			{
				c_int ret = Parse_Key_Value_Pair(ref pm, ref str, key_Val_Sep, pairs_Sep, flags);
				if (ret < 0)
					return ret;

				if (str[0] != 0)
					str++;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Free all the memory allocated for an AVDictionary struct and all
		/// keys and values
		/// </summary>
		/********************************************************************/
		public static void Av_Dict_Free(ref AvDictionary pm)//XX 233
		{
			AvDictionary m = pm;

			if (m != null)
			{
				while (m.Count-- != 0)
				{
					Mem.Av_FreeP(ref m.Elems[m.Count].Key);
					Mem.Av_FreeP(ref m.Elems[m.Count].Value);
				}

				Mem.Av_FreeP(ref m.Elems);
			}

			Mem.Av_FreeP(ref pm);
		}



		/********************************************************************/
		/// <summary>
		/// Copy entries from one AVDictionary struct into another.
		///
		/// Note: Metadata is read using the ::AV_DICT_IGNORE_SUFFIX flag
		/// </summary>
		/********************************************************************/
		public static c_int Av_Dict_Copy(ref AvDictionary dst, AvDictionary src, AvDict flags)//XX 247
		{
			foreach (AvDictionaryEntry t in Av_Dict_Iterate(src))
			{
				c_int ret = Av_Dict_Set(ref dst, t.Key, t.Value, flags);

				if (ret < 0)
					return ret;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Get dictionary entries as a string.
		///
		/// Create a string containing dictionary's entries.
		/// Such string may be passed back to av_dict_parse_string().
		/// Note: String is escaped with backslashes ('\').
		///
		/// Warning: Separators cannot be neither '\\' nor '\0'. They also
		/// cannot be the same
		/// </summary>
		/********************************************************************/
		public static c_int Av_Dict_Get_String(AvDictionary m, out CPointer<char> buffer, char key_Val_Sep, char pairs_Sep)//XX 260
		{
			c_int cnt = 0;
			char[] special_Chars = [ pairs_Sep, key_Val_Sep, '\0' ];

			if ((pairs_Sep == '\0') || (key_Val_Sep == '\0') || (pairs_Sep == key_Val_Sep) || (pairs_Sep == '\\') || (key_Val_Sep == '\\'))
			{
				buffer = Mem.Av_StrDup(CString.Empty);

				return buffer.IsNotNull ? Error.EINVAL : Error.ENOMEM;
			}

			if (Av_Dict_Count(m) == 0)
			{
				buffer = Mem.Av_StrDup(CString.Empty);

				return buffer.IsNotNull ? 0 : Error.ENOMEM;
			}

			BPrint.Av_BPrint_Init(out AVBPrint bprint, 64, BPrint.Av_BPrint_Size_Unlimited);

			foreach (AvDictionaryEntry t in Av_Dict_Iterate(m))
			{
				if (cnt++ != 0)
					BPrint.Av_BPrint_Append_Data(bprint, pairs_Sep.ToCharPointer(), 1);

				BPrint.Av_BPrint_Escape(bprint, t.Key, special_Chars, AvEscapeMode.Backslash, AvEscapeFlag.None);
				BPrint.Av_BPrint_Append_Data(bprint, key_Val_Sep.ToCharPointer(), 1);
				BPrint.Av_BPrint_Escape(bprint, t.Value, special_Chars, AvEscapeMode.Backslash, AvEscapeFlag.None);
			}

			return BPrint.Av_BPrint_Finalize(bprint, out buffer);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Parse_Key_Value_Pair(ref AvDictionary pm, ref CPointer<char> buf, CPointer<char> key_Val_Sep, CPointer<char> pairs_Sep, AvDict flags)//XX 186
		{
			CPointer<char> key = AvString.Av_Get_Token(ref buf, key_Val_Sep);
			CPointer<char> val = null;
			c_int ret;

			if (key.IsNotNull && (key[0] != 0) && (CString.strspn(buf, key_Val_Sep) != 0))
			{
				buf++;
				val = AvString.Av_Get_Token(ref buf, pairs_Sep);
			}

			if (key.IsNotNull && (key[0] != 0) && val.IsNotNull && (val[0] != 0))
				ret = Av_Dict_Set(ref pm, key, val, flags);
			else
				ret = Error.EINVAL;

			Mem.Av_FreeP(ref key);
			Mem.Av_FreeP(ref val);

			return ret;
		}
		#endregion
	}
}
