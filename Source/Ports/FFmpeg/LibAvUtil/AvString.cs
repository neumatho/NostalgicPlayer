/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class AvString
	{
		/// <summary></summary>
		public static readonly CPointer<char> Whitespaces = " \n\t\r".ToCharPointer();

		/********************************************************************/
		/// <summary>
		/// Locale-independent conversion of ASCII isdigit
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Av_IsDigit(c_int c)
		{
			return (c >= '0') && (c <= '9');
		}



		/********************************************************************/
		/// <summary>
		/// Locale-independent conversion of ASCII isspace
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Av_IsSpace(c_int c)
		{
			return (c == ' ') || (c == '\f') || (c == '\n') || (c == '\r') || (c == '\t') || (c == '\v');
		}



		/********************************************************************/
		/// <summary>
		/// Locale-independent conversion of ASCII characters to uppercase
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Av_ToUpper(c_int c)
		{
			if ((c >= 'a') && (c <= 'z'))
				c ^= 0x20;

			return c;
		}



		/********************************************************************/
		/// <summary>
		/// Locale-independent conversion of ASCII characters to lowercase
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Av_ToLower(c_int c)
		{
			if ((c >= 'A') && (c <= 'Z'))
				c ^= 0x20;

			return c;
		}



		/********************************************************************/
		/// <summary>
		/// Return non-zero if pfx is a prefix of str. If it is, *ptr is set
		/// to the address of the first character in str after the prefix
		/// </summary>
		/********************************************************************/
		public static c_int Av_StrStart(CPointer<char> str, CPointer<char> pfx, out CPointer<char> ptr)//XX 36
		{
			while ((pfx[0] != 0) && (pfx[0] == str[0]))
			{
				pfx++;
				str++;
			}

			if (pfx[0] == 0)
				ptr = str;
			else
				ptr = null;

			return pfx[0] == 0 ? 1 : 0;
		}



		/********************************************************************/
		/// <summary>
		/// Return non-zero if pfx is a prefix of str independent of case. If
		/// it is, *ptr is set to the address of the first character in str
		/// after the prefix
		/// </summary>
		/********************************************************************/
		public static c_int Av_StriStart(CPointer<char> str, CPointer<char> pfx, out CPointer<char> ptr)//XX 47
		{
			while ((pfx[0] != 0) && (Av_ToUpper(pfx[0]) == Av_ToUpper(str[0])))
			{
				pfx++;
				str++;
			}

			if (pfx[0] == 0)
				ptr = str;
			else
				ptr = null;

			return pfx[0] == 0 ? 1 : 0;
		}



		/********************************************************************/
		/// <summary>
		/// Locate the first case-independent occurrence in the string
		/// haystack of the string needle. A zero-length string needle is
		/// considered to match at the start of haystack.
		///
		/// This function is a case-insensitive version of the standard
		/// strstr()
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_StriStr(CPointer<char> s1, CPointer<char> s2)//XX 58
		{
			if (s2.IsNull)
				return s1;

			do
			{
				if (Av_StriStart(s1, s2, out _) != 0)
					return s1;
			}
			while (s1[0, 1] != '\0');

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Locate the first occurrence of the string needle in the string
		/// haystack where not more than hay_length characters are searched.
		/// A zero-length string needle is considered to match at the start
		/// of haystack.
		///
		/// This function is a length-limited version of the standard
		/// strstr()
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_Strnstr(CPointer<char> haystack, CPointer<char> needle, size_t hay_Length)//XX 71
		{
			size_t needle_Len = CString.strlen(needle);

			if (needle_Len == 0)
				return haystack;

			while (hay_Length >= needle_Len)
			{
				hay_Length--;

				if (CMemory.memcmp(haystack, needle, needle_Len) == 0)
					return haystack;

				haystack++;
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Copy the string src to dst, but no more than size - 1 bytes, and
		/// null-terminate dst.
		///
		/// This function is the same as BSD strlcpy().
		///
		/// Warning: since the return value is the length of src, src
		/// absolutely _must_ be a properly 0-terminated string, otherwise
		/// this will read beyond the end of the buffer and possibly crash
		/// </summary>
		/********************************************************************/
		public static size_t Av_Strlcpy(CPointer<char> dst, CPointer<char> src, size_t size)//XX 85
		{
			size_t len = 0;

			while ((++len < size) && (src[0] != '\0'))
				dst[0, 1] = src[0, 1];

			if (len <= size)
				dst[0] = '\0';

			return len + CString.strlen(src) - 1;
		}



		/********************************************************************/
		/// <summary>
		/// Append output to a string, according to a format. Never write
		/// out of the destination buffer, and always put a terminating 0
		/// within the buffer
		/// </summary>
		/********************************************************************/
		public static size_t Av_Strlcatf(CPointer<char> dst, size_t size, string fmt, params object[] args)//XX 103
		{
			return Av_Strlcatf(dst, size, fmt.ToCharPointer(), args);
		}



		/********************************************************************/
		/// <summary>
		/// Append output to a string, according to a format. Never write
		/// out of the destination buffer, and always put a terminating 0
		/// within the buffer
		/// </summary>
		/********************************************************************/
		public static size_t Av_Strlcatf(CPointer<char> dst, size_t size, CPointer<char> fmt, params object[] args)//XX 103
		{
			size_t len = CString.strlen(dst);

			len += (size_t)CString.snprintf(dst + len, size > len ? size - len : 0, fmt, args);

			return len;
		}



		/********************************************************************/
		/// <summary>
		/// Print arguments following specified format into a large enough
		/// auto allocated buffer. It is similar to GNU asprintf()
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_Asprintf(string fmt, params object[] args)//XX 115
		{
			CPointer<char> p = null;

			c_int len = CString.snprintf(null, 0, fmt, args);

			if (len < 0)
				goto End;

			p = Mem.Av_MAlloc<char>((size_t)len + 1);

			if (p.IsNull)
				goto End;

			len = CString.snprintf(p, (size_t)len + 1, fmt, args);

			if (len < 0)
				Mem.Av_FreeP(ref p);

			End:
			return p;
		}



		/********************************************************************/
		/// <summary>
		/// Unescape the given string until a non escaped terminating char,
		/// and return the token corresponding to the unescaped string.
		///
		/// The normal \ and ' escaping is supported. Leading and trailing
		/// whitespaces are removed, unless they are escaped with '\' or are
		/// enclosed between ''
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_Get_Token(ref CPointer<char> buf, CPointer<char> term)//XX 143
		{
			CPointer<char> @out = Mem.Av_Realloc<char>(null, CString.strlen(buf) + 1);
			CPointer<char> ret = @out, end = @out;
			CPointer<char> p = buf;

			if (@out.IsNull)
				return null;

			p += CString.strspn(p, Whitespaces);

			while ((p[0] != 0) && (CString.strspn(p, term) == 0))
			{
				char c = p[0, 1];

				if ((c == '\\') && (p[0] != 0))
				{
					@out[0, 1] = p[0, 1];
					end = @out;
				}
				else if (c == '\'')
				{
					while ((p[0] != 0) && (p[0] != '\''))
						@out[0, 1] = p[0, 1];

					if (p[0] != 0)
					{
						p++;
						end = @out;
					}
				}
				else
					@out[0, 1] = c;
			}

			do
			{
				@out[0, -1] = '\0';
			}
			while ((@out >= end) && (CString.strspn(@out, Whitespaces) != 0));

			buf = p;

			CPointer<char> small_Ret = Mem.Av_Realloc(ret, (size_t)(@out - ret + 2));

			return small_Ret.IsNotNull ? small_Ret : ret;
		}



		/********************************************************************/
		/// <summary>
		/// Split the string into several tokens which can be accessed by
		/// successive calls to av_strtok().
		///
		/// A token is defined as a sequence of characters not belonging to
		/// the set specified in delim.
		///
		/// On the first call to av_strtok(), s should point to the string to
		/// parse, and the value of saveptr is ignored. In subsequent calls,
		/// s should be NULL, and saveptr should be unchanged since the
		/// previous call.
		///
		/// This function is similar to strtok_r() defined in POSIX.1
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_Strtok(CPointer<char> s, CPointer<char> delim, ref CPointer<char> savePtr)//XX 179
		{
			if (s.IsNull && (s = savePtr).IsNull)
				return null;

			// Skip leading delimiters
			s += CString.strspn(s, delim);

			// s now points to the first non delimiter char, or to the end of the string
			if (s[0] == '\0')
			{
				savePtr = null;

				return null;
			}

			CPointer<char> tok = s++;

			// Skip non delimiters
			s += CString.strcspn(s, delim);

			if (s[0] != '\0')
			{
				s[0] = '\0';
				savePtr = s + 1;
			}
			else
				savePtr = null;

			return tok;
		}



		/********************************************************************/
		/// <summary>
		/// Locale-independent case-insensitive compare.
		/// 
		/// Note: This means only ASCII-range characters are case-insensitive
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Av_Strcasecmp(CPointer<char> a, string b)
		{
			return Av_Strcasecmp(a, b.ToCharPointer());
		}



		/********************************************************************/
		/// <summary>
		/// Locale-independent case-insensitive compare.
		/// 
		/// Note: This means only ASCII-range characters are case-insensitive
		/// </summary>
		/********************************************************************/
		public static c_int Av_Strcasecmp(CPointer<char> a, CPointer<char> b)//XX 208
		{
			uint8_t c1, c2;

			do
			{
				c1 = (uint8_t)Av_ToLower(a[0, 1]);
				c2 = (uint8_t)Av_ToLower(b[0, 1]);
			}
			while ((c1 != 0) && (c1 == c2));

			return c1 - c2;
		}



		/********************************************************************/
		/// <summary>
		/// Locale-independent case-insensitive compare.
		/// 
		/// Note: This means only ASCII-range characters are case-insensitive
		/// </summary>
		/********************************************************************/
		public static c_int Av_Strncasecmp(CPointer<char> a, CPointer<char> b, size_t n)//XX 218
		{
			uint8_t c1, c2;

			if (n <= 0)
				return 0;

			do
			{
				c1 = (uint8_t)Av_ToLower(a[0, 1]);
				c2 = (uint8_t)Av_ToLower(b[0, 1]);
			}
			while ((--n != 0) && (c1 != 0) && (c1 == c2));

			return c1 - c2;
		}



		/********************************************************************/
		/// <summary>
		/// Locale-independent strings replace.
		/// Note: This means only ASCII-range characters are replaced.
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_StriReplace(CPointer<char> str, CPointer<char> from, CPointer<char> to)//XX 230
		{
			CPointer<char> ret = null;
			CPointer<char> pStr = str;
			CPointer<char> pStr2;

			size_t toLen = CString.strlen(to);
			size_t fromLen = CString.strlen(from);

			AVBPrint pBuf;

			BPrint.Av_BPrint_Init(out pBuf, 1, BPrint.Av_BPrint_Size_Unlimited);

			while ((pStr2 = Av_StriStr(pStr, from)).IsNotNull)
			{
				BPrint.Av_BPrint_Append_Data(pBuf, pStr, (c_uint)(pStr2 - pStr));

				pStr = pStr2 + fromLen;

				BPrint.Av_BPrint_Append_Data(pBuf, to, (c_uint)toLen);
			}

			BPrint.Av_BPrint_Append_Data(pBuf, pStr, (c_uint)CString.strlen(pStr));

			if (BPrint.Av_BPrint_Is_Complete(pBuf) == 0)
				BPrint.Av_BPrint_Finalize(pBuf, out _);
			else
				BPrint.Av_BPrint_Finalize(pBuf, out ret);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Append path component to the existing path.
		/// Path separator '/' is placed between when needed.
		/// Resulting string have to be freed with av_free()
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_Append_Path_Component(CPointer<char> path, CPointer<char> component)//XX 297
		{
			if (path.IsNull)
				return Mem.Av_StrDup(component);

			if (component.IsNull)
				return Mem.Av_StrDup(path);

			size_t p_Len = CString.strlen(path);
			size_t c_Len = CString.strlen(component);

			if ((p_Len > (size_t.MaxValue - c_Len)) || ((p_Len + c_Len) > (size_t.MaxValue - 2)))
				return null;

			CPointer<char> fullPath = Mem.Av_MAlloc<char>(p_Len + c_Len + 2);

			if (fullPath.IsNotNull)
			{
				if (p_Len != 0)
				{
					Av_Strlcpy(fullPath, path, p_Len + 1);

					if (c_Len != 0)
					{
						if ((fullPath[p_Len - 1] != '/') && (component[0] != '/'))
							fullPath[p_Len++] = '/';
						else if ((fullPath[p_Len - 1] == '/') && (component[0] == '/'))
							p_Len--;
					}
				}

				Av_Strlcpy(fullPath + p_Len, component, c_Len + 1);
				fullPath[p_Len + c_Len] = '\0';
			}

			return fullPath;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Av_Match_Name(CPointer<char> name, CPointer<char> names)//XX 346
		{
			if (name.IsNull || names.IsNull)
				return 0;

			size_t nameLen = CString.strlen(name);

			while (names[0] != 0)
			{
				c_int negate = '-' == names[0] ? 1 : 0;
				CPointer<char> p = CString.strchr(names, ',');

				if (p.IsNull)
					p = names + CString.strlen(names);

				names += negate;
				size_t len = Macros.FFMax((size_t)(p - names), nameLen);

				if ((Av_Strncasecmp(name, names, len) == 0) || (CString.strncmp("ALL", names, (size_t)Macros.FFMax(3, p - names)) == 0))
					return negate ^ 1;

				names = p + (p[0] == ',' ? 1 : 0);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Check if a name is in a list
		/// </summary>
		/********************************************************************/
		public static c_int Av_Match_List(CPointer<char> name, CPointer<char> list, char separator)//XX 445
		{
			for (CPointer<char> p = name; p.IsNotNull && (p[0] != 0); )
			{
				for (CPointer<char> q = list; q.IsNotNull && (q[0] != 0); )
				{
					for (c_int k = 0; (p[k] == q[k]) || (((p[k] * q[k]) == 0) && ((p[k] + q[k]) == separator)); k++)
					{
						if ((k != 0) && ((p[k] == 0) || (p[k] == separator)))
							return 1;
					}

					q = CString.strchr(q, separator);

					if (q.IsNotNull)
						q++;
				}

				p = CString.strchr(p, separator);

				if (p.IsNotNull)
					p++;
			}

			return 0;
		}
	}
}
