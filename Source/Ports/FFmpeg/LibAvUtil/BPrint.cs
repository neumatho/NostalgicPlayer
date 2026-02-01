/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class BPrint
	{
		/// <summary>
		/// Buffer will be reallocated as necessary, with an amortized linear cost
		/// </summary>
		public const c_uint Av_BPrint_Size_Unlimited = c_uint.MaxValue;

		/// <summary>
		/// Use the exact size available in the AVBPrint structure itself.
		/// 
		/// Thus ensuring no dynamic memory allocation. The internal buffer is large
		/// enough to hold a reasonable paragraph of text, such as the current paragraph
		/// </summary>
		public const c_uint Av_BPrint_Size_Automatic = 1;

		/// <summary>
		/// Do not write anything to the buffer, only calculate the total length.
		/// 
		/// The write operations can then possibly be repeated in a buffer with
		/// exactly the necessary size (using `size_init = size_max = AVBPrint.len + 1`)
		/// </summary>
		public const c_uint Av_BPrint_Size_Count_Only = 0;

		private static readonly CPointer<char> Whitespaces = " \n\t\r".ToCharPointer();


		/********************************************************************/
		/// <summary>
		///  Test if the print buffer is complete (not truncated).
		///
		/// It may have been truncated due to a memory allocation failure
		/// or the size_max limit (compare size and size_max if necessary)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Av_BPrint_Is_Complete(AVBPrint buf)
		{
			return buf.Len < buf.Size ? 1 : 0;
		}



		/********************************************************************/
		/// <summary>
		/// Init a print buffer
		/// </summary>
		/********************************************************************/
		public static void Av_BPrint_Init(out AVBPrint buf, c_uint size_Init, c_uint size_Max)//XX 69
		{
			buf = new AVBPrint();

			c_uint size_Auto = (c_uint)buf.Internal_Buffer.Length;

			if (size_Max == Av_BPrint_Size_Automatic)
				size_Max = size_Auto;

			buf.Str = buf.Internal_Buffer;
			buf.Len = 0;
			buf.Size = Macros.FFMin(size_Auto, size_Max);
			buf.Size_Max = size_Max;
			buf.Str[0] = '\0';

			if (size_Init > buf.Size)
				Av_BPrint_Alloc(buf, size_Init - 1);
		}



		/********************************************************************/
		/// <summary>
		/// Init a print buffer using a pre-existing buffer.
		///
		/// The buffer will not be reallocated.
		/// In case size equals zero, the AVBPrint will be initialized to
		/// use the internal buffer as if using AV_BPRINT_SIZE_COUNT_ONLY
		/// with av_bprint_init()
		/// </summary>
		/********************************************************************/
		public static void Av_BPrint_Init_For_Buffer(out AVBPrint buf, CPointer<char> buffer, c_uint size)//XX 85
		{
			if (size == 0)
			{
				Av_BPrint_Init(out buf, 0, Av_BPrint_Size_Count_Only);
				return;
			}

			buf = new AVBPrint();

			buf.Str = buffer;
			buf.Len = 0;
			buf.Size = size;
			buf.Size_Max = size;
			buf.Str[0] = '\0';
		}



		/********************************************************************/
		/// <summary>
		/// Append a formatted string to a print buffer
		/// </summary>
		/********************************************************************/
		public static void Av_BPrintf(AVBPrint buf, string fmt, params object[] args)
		{
			Av_BPrintf(buf, fmt.ToCharPointer(), args);
		}



		/********************************************************************/
		/// <summary>
		/// Append a formatted string to a print buffer
		/// </summary>
		/********************************************************************/
		public static void Av_BPrintf(AVBPrint buf, CPointer<char> fmt, params object[] args)//XX 122
		{
			c_int extra_Len;

			while (true)
			{
				c_uint room = Av_BPrint_Room(buf);
				CPointer<char> dst = room != 0 ? buf.Str + buf.Len : null;

				extra_Len = CString.snprintf(dst, room, fmt, args);

				if (extra_Len <= 0)
					return;

				if (extra_Len < room)
					break;

				if (Av_BPrint_Alloc(buf, (c_uint)extra_Len) != 0)
					break;
			}

			Av_BPrint_Grow(buf, (c_uint)extra_Len);
		}



		/********************************************************************/
		/// <summary>
		/// Append char c n times to a print buffer
		/// </summary>
		/********************************************************************/
		public static void Av_BPrint_Chars(AVBPrint buf, char c, c_uint n)//XX 130
		{
			c_uint room;

			while (true)
			{
				room = Av_BPrint_Room(buf);

				if (n < room)
					break;

				if (Av_BPrint_Alloc(buf, n) != 0)
					break;
			}

			if (room != 0)
			{
				c_uint real_N = Macros.FFMin(n, room - 1);
				CMemory.memset(buf.Str + buf.Len, c, real_N);
			}

			Av_BPrint_Grow(buf, n);
		}



		/********************************************************************/
		/// <summary>
		/// Append data to a print buffer
		/// </summary>
		/********************************************************************/
		public static void Av_BPrint_Append_Data(AVBPrint buf, CPointer<char> data, c_uint size)//XX 148
		{
			c_uint room;

			while (true)
			{
				room = Av_BPrint_Room(buf);

				if (size < room)
					break;

				if (Av_BPrint_Alloc(buf, size) != 0)
					break;
			}

			if (room != 0)
			{
				c_uint real_N = Macros.FFMin(size, room - 1);
				CMemory.memcpy(buf.Str + buf.Len, data, real_N);
			}

			Av_BPrint_Grow(buf, size);
		}



		/********************************************************************/
		/// <summary>
		/// Append a formatted date and time to a print buffer
		/// </summary>
		/********************************************************************/
		public static void Av_BPrint_Strftime(AVBPrint buf, string fmt, tm tm)
		{
			Av_BPrint_Strftime(buf, fmt.ToCharPointer(), tm);
		}



		/********************************************************************/
		/// <summary>
		/// Append a formatted date and time to a print buffer
		/// </summary>
		/********************************************************************/
		public static void Av_BPrint_Strftime(AVBPrint buf, CPointer<char> fmt, tm tm)//XX 166
		{
			size_t l = 0;

			size_t fmt_Len = CString.strlen(fmt);

			if (fmt.IsNull)
				return;

			while (true)
			{
				c_uint room = Av_BPrint_Room(buf);

				if ((room != 0) && ((l = CTime.strftime(buf.Str + buf.Len, room, fmt, tm)) != 0))
					break;

				// Due to the limitations of strftime() it is not possible to know if
				// the output buffer is too small or the output is empty.
				// However, a 256x output space requirement compared to the format
				// string length is so unlikely we can safely assume empty output. This
				// allows supporting possibly empty format strings like "%p". 
				if ((room >> 8) > fmt_Len)
					break;

				// strftime does not tell us how much room it would need: let us
				// retry with twice as much until the buffer is large enough
				room = (c_uint)(room == 0 ? fmt_Len + 1 : (room <= (c_int.MaxValue / 2) ? room * 2 : c_int.MaxValue));

				if (Av_BPrint_Alloc(buf, room) != 0)
				{
					// Impossible to grow, try to manage something useful anyway
					room = Av_BPrint_Room(buf);

					if (room < 1024)
					{
						// if strftime fails because the buffer has (almost) reached
						// its maximum size, let us try in a local buffer; 1k should
						// be enough to format any real date+time string
						CPointer<char> buf2 = new CPointer<char>(1024);

						l = CTime.strftime(buf2, (c_uint)buf2.Length, fmt, tm);
						if (l != 0)
						{
							Av_BPrintf(buf, "%s", buf2);
							return;
						}
					}

					if (room != 0)
					{
						// If anything else failed and the buffer is not already
						// truncated, let us add a stock string and force truncation
						CPointer<char> txt = "[truncated strftime output]".ToCharPointer();

						CMemory.memset(buf.Str + buf.Len, '!', room);
						CMemory.memcpy(buf.Str + buf.Len, txt, Macros.FFMin((c_uint)txt.Length - 1, room));

						Av_BPrint_Grow(buf, room);  // Force truncation
					}

					return;
				}
			}

			Av_BPrint_Grow(buf, (c_uint)l);
		}



		/********************************************************************/
		/// <summary>
		/// Reset the string to "" but keep internal allocated data
		/// </summary>
		/********************************************************************/
		public static void Av_BPrint_Clear(AVBPrint buf)//XX 227
		{
			if (buf.Len != 0)
			{
				buf.Str[0] = '\0';
				buf.Len = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Finalize a print buffer.
		///
		/// The print buffer can no longer be used afterwards, but the len
		/// and size fields are still valid
		/// </summary>
		/********************************************************************/
		public static c_int Av_BPrint_Finalize(AVBPrint buf, out CPointer<char> ret_Str)//XX 235
		{
			c_uint real_Size = Macros.FFMin(buf.Len + 1, buf.Size);
			CPointer<char> str;
			c_int ret = 0;

			if (Av_BPrint_Is_Allocated(buf))
			{
				str = Mem.Av_Realloc(buf.Str, real_Size);

				if (str.IsNull)
					str = buf.Str;

				buf.Str.SetToNull();
			}
			else
			{
				str = Mem.Av_MemDup(buf.Str, real_Size);

				if (str.IsNull)
					ret = Error.ENOMEM;
			}

			ret_Str = str;

			buf.Size = real_Size;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Escape the content in src and append it to dstbuf
		/// </summary>
		/********************************************************************/
		public static void Av_BPrint_Escape(AVBPrint dstBuf, CPointer<char> src, char[] special_Chars, AvEscapeMode mode, AvEscapeFlag flags)//XX 263
		{
			CPointer<char> src0 = src;

			if (mode == AvEscapeMode.Auto)
				mode = AvEscapeMode.Backslash;	// TODO: implement a heuristic

			switch (mode)
			{
				case AvEscapeMode.Quote:
				{
					// Enclose the string between ''
					Av_BPrint_Chars(dstBuf, '\'', 1);

					for (; src[0] != '\0'; src++)
					{
						if (src[0] == '\'')
							Av_BPrintf(dstBuf, "'\\''");
						else
							Av_BPrint_Chars(dstBuf, src[0], 1);
					}

					Av_BPrint_Chars(dstBuf, '\'', 1);
					break;
				}

				case AvEscapeMode.Xml:
				{
					// Escape XML non-markup character data as per 2.4 by default:
					//  [^<&]* - ([^<&]* ']]>' [^<&]*)
					//
					// Additionally, given one of the AV_ESCAPE_FLAG_XML_* flags,
					// escape those specific characters as required
					for (; src[0] != '\0'; src++)
					{
						switch (src[0])
						{
							case '&':
							{
								Av_BPrintf(dstBuf, "%s", "&amp;");
								break;
							}

							case '<':
							{
								Av_BPrintf(dstBuf, "%s", "&lt;");
								break;
							}

							case '>':
							{
								Av_BPrintf(dstBuf, "%s", "&gt;");
								break;
							}

							case '\'':
							{
								if ((flags & AvEscapeFlag.Xml_Single_Quotes) == 0)
									goto default;

								Av_BPrintf(dstBuf, "%s", "&apos;");
								break;
							}

							case '"':
							{
								if ((flags & AvEscapeFlag.Xml_Double_Quotes) == 0)
									goto default;

								Av_BPrintf(dstBuf, "%s", "&quot;");
								break;
							}

							default:
							{
								Av_BPrint_Chars(dstBuf, src[0], 1);
								break;
							}
						}
					}
					break;
				}

				// case AV_ESCAPE_MODE_BACKSLASH or unknown mode
				default:
				{
					// \-escape characters
					for (; src[0] != '\0'; src++)
					{
						bool is_First_Last = ((src == src0) || (src[1] == '\0'));
						bool is_WS = CString.strchr(Whitespaces, src[0]).IsNotNull;
						bool is_Strictly_Special = (special_Chars != null) && CString.strchr(special_Chars, src[0]).IsNotNull;
						bool is_Special = is_Strictly_Special || CString.strchr("'\\".ToCharPointer(), src[0]).IsNotNull || (is_WS && ((flags & AvEscapeFlag.Whitespace) != 0));

						if (is_Strictly_Special || (((flags & AvEscapeFlag.Strict) == 0) && (is_Special || (is_WS && is_First_Last))))
							Av_BPrint_Chars(dstBuf, '\\', 1);

						Av_BPrint_Chars(dstBuf, src[0], 1);
					}
					break;
				}
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_uint Av_BPrint_Room(AVBPrint buf)
		{
			return buf.Size - Macros.FFMin(buf.Len, buf.Size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool Av_BPrint_Is_Allocated(AVBPrint buf)
		{
			return buf.Str != buf.Internal_Buffer;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Av_BPrint_Alloc(AVBPrint buf, c_uint room)//XX 36
		{
			if (buf.Size == buf.Size_Max)
				return Error.EIO;

			if (Av_BPrint_Is_Complete(buf) == 0)
				return Error.InvalidData;

			c_uint min_Size = buf.Len + 1 + Macros.FFMin(c_uint.MaxValue - buf.Len - 1, room);
			c_uint new_Size = buf.Size > (buf.Size_Max / 2) ? buf.Size_Max : buf.Size * 2;

			if (new_Size < min_Size)
				new_Size = Macros.FFMin(buf.Size_Max, min_Size);

			CPointer<char> old_Str = Av_BPrint_Is_Allocated(buf) ? buf.Str : null;
			CPointer<char> new_Str = Mem.Av_Realloc(old_Str, new_Size);

			if (new_Str.IsNull)
				return Error.ENOMEM;

			if (old_Str.IsNull)
				CMemory.memcpy(new_Str, buf.Str, buf.Len + 1);

			buf.Str = new_Str;
			buf.Size = new_Size;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Av_BPrint_Grow(AVBPrint buf, c_uint extra_Len)// 60
		{
			// Arbitrary margin to avoid small overflows
			extra_Len = Macros.FFMin(extra_Len, c_uint.MaxValue - 5 - buf.Len);
			buf.Len += extra_Len;

			if (buf.Size != 0)
				buf.Str[Macros.FFMin(buf.Len, buf.Size - 1)] = '\0';
		}
		#endregion
	}
}
