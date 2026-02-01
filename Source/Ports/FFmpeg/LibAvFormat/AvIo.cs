/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat
{
	/// <summary>
	/// Unbuffered I/O
	/// </summary>
	public static class AvIo
	{
		private const string Url_Scheme_Chars =
			"abcdefghijklmnopqrstuvwxyz" +
			"ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
			"0123456789+-.";

		private const c_int Io_Buffer_Size = 32768;

		private const AvOptFlag E = AvOptFlag.Encoding_Param;
		private const AvOptFlag D = AvOptFlag.Decoding_Param;

		private static readonly AvOption[] _options =
		[
			new AvOption("protocol whitelist", "List of protocols that are allowed to be used", nameof(UrlContext.Protocol_Whitelist), AvOptionType.String, new AvOption.DefaultValueUnion { Str = null }, 0, 0, D),
			new AvOption("protocol blacklist", "List of protocols that are not allowed to be used", nameof(UrlContext.Protocol_Blacklist), AvOptionType.String, new AvOption.DefaultValueUnion { Str = null }, 0, 0, D),
			new AvOption("rw_timeout", "Timeout for IO operations (in microseconds)", nameof(UrlContext.Rw_Timeout), AvOptionType.Int64, new AvOption.DefaultValueUnion { I64 = 0 }, 0, int64_t.MaxValue, E | D),
			new AvOption()
		];

		private static readonly AvClass url_Context_Class = new AvClass
		{
			Class_Name = "URLContext".ToCharPointer(),
			Item_Name = UrlContext_To_Name,
			Option = _options,
			Version = Version.Version_Int,
			Child_Next = UrlContext_Child_Next,
			Child_Class_Iterate = Protocols.FF_UrlContext_Child_Class_Iterate
		};

		private static readonly AvOption[] avIo_Options =
		[
			new AvOption("protocol whitelist", "List of protocols that are allowed to be used", nameof(AvIoContext.Protocol_Whitelist), AvOptionType.String, new AvOption.DefaultValueUnion { Str = null }, 0, 0, D),
			new AvOption()
		];

		internal static readonly AvClass FF_AvIo_Class = new AvClass
		{
			Class_Name = "AVIOContext".ToCharPointer(),
			Item_Name = Log.Av_Default_Item_Name,
			Version = Version.Version_Int,
			Option = avIo_Options,
			Child_Next = AvIo_Child_Next,
			Child_Class_Iterate = Child_Class_Iterate
		};

		/********************************************************************/
		/// <summary>
		/// Connect an URLContext that has been allocated by ffurl_alloc
		/// </summary>
		/********************************************************************/
		public static c_int FFUrl_Connect(UrlContext uc, ref AvDictionary options)//XX 205
		{
			if (uc.Protocol_Whitelist.IsNotNull && (AvString.Av_Match_List(uc.Prot.Name, uc.Protocol_Whitelist, ',') <= 0))
				return Error.EINVAL;

			if (uc.Protocol_Blacklist.IsNotNull && (AvString.Av_Match_List(uc.Prot.Name, uc.Protocol_Blacklist, ',') > 0))
				return Error.EINVAL;

			if (uc.Protocol_Whitelist.IsNull && uc.Prot.Default_Whitelist.IsNotNull)
			{
				uc.Protocol_Whitelist = Mem.Av_StrDup(uc.Prot.Default_Whitelist);

				if (uc.Protocol_Whitelist.IsNull)
					return Error.ENOMEM;
			}

			c_int err = Dict.Av_Dict_Set(ref options, "protocol_whitelist", uc.Protocol_Whitelist, AvDict.None);
			if (err < 0)
				return err;

			err = Dict.Av_Dict_Set(ref options, "protocol_blacklist", uc.Protocol_Blacklist, AvDict.None);
			if (err < 0)
				return err;

			err = uc.Prot.Url_Open2 != null ? uc.Prot.Url_Open2(uc, uc.FileName, uc.Flags, ref options) : uc.Prot.Url_Open(uc, uc.FileName, uc.Flags);

			Dict.Av_Dict_Set(ref options, "protocol_whitelist", (CPointer<char>)null, AvDict.None);
			Dict.Av_Dict_Set(ref options, "protocol_blacklist", (CPointer<char>)null, AvDict.None);

			if (err != 0)
				return err;

			uc.Is_Connected = true;

			// We must be careful here as ffurl_seek() could be slow,
			// for example for http
			if (((uc.Flags & AvIoFlag.Write) != 0) || (CString.strcmp(uc.Prot.Name, "file") != 0))
			{
				if (!uc.Is_Streamed && (FFUrl_Seek(uc, 0, AvSeek.Set) < 0))
					uc.Is_Streamed = true;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Create a URLContext for accessing to the resource indicated by
		/// url, but do not initiate the connection yet
		/// </summary>
		/********************************************************************/
		public static c_int FFUrl_Alloc(out UrlContext puc, CPointer<char> fileName, AvIoFlag flags, AvIoInterruptCb int_Cb)//XX 350
		{
			UrlProtocol p = Url_Find_Protocol(fileName);

			if (p != null)
				return Url_Alloc_For_Protocol(out puc, p, fileName, flags, int_Cb);

			puc = null;

			return Error.Protocol_Not_Found;
		}



		/********************************************************************/
		/// <summary>
		/// Create an URLContext for accessing to the resource indicated by
		/// url, and open it
		/// </summary>
		/********************************************************************/
		public static c_int FFUrl_Open_Whitelist(out UrlContext puc, CPointer<char> fileName, AvIoFlag flags, AvIoInterruptCb int_Cb, ref AvDictionary options, CPointer<char> whitelist, CPointer<char> blacklist, UrlContext parent)//XX 363
		{
			c_int ret = FFUrl_Alloc(out puc, fileName, flags, int_Cb);
			if (ret < 0)
				return ret;

			if (parent != null)
			{
				ret = Opt.Av_Opt_Copy(puc, parent);
				if (ret < 0)
					goto Fail;
			}

			if ((options != null) && ((ret = Opt.Av_Opt_Set_Dict(puc, ref options)) < 0))
				goto Fail;

			if ((options != null) && (puc.Prot.Priv_Data_Class != null) && ((ret = Opt.Av_Opt_Set_Dict(puc.Priv_Data, ref options)) < 0))
				goto Fail;

			ret = Dict.Av_Dict_Set(ref options, "protocol_whitelist", whitelist, AvDict.None);
			if (ret < 0)
				goto Fail;

			ret = Dict.Av_Dict_Set(ref options, "protocol_blacklist", blacklist, AvDict.None);
			if (ret < 0)
				goto Fail;

			ret = Opt.Av_Opt_Set_Dict(puc, ref options);
			if (ret < 0)
				goto Fail;

			ret = FFUrl_Connect(puc, ref options);
			if (ret == 0)
				return 0;

			Fail:
			FFUrl_CloseP(ref puc);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Create and initialize a AVIOContext for accessing the resource
		/// referenced by the URLContext h.
		/// Note: When the URLContext h has been opened in read+write mode,
		/// the AVIOContext can be used only for writing
		/// </summary>
		/********************************************************************/
		internal static c_int FFIo_FdOpen(out AvIoContext sp, UrlContext h)//XX 413
		{
			sp = null;

			c_int buffer_Size;

			c_int max_Packet_Size = h.Max_Packet_Size;

			if (max_Packet_Size != 0)
				buffer_Size = max_Packet_Size;	// No need to bufferize more than one packet
			else
				buffer_Size = Io_Buffer_Size;

			if (((h.Flags & AvIoFlag.Write) == 0) && h.Is_Streamed)
			{
				if (buffer_Size > (c_int.MaxValue / 2))
					return Error.EINVAL;

				buffer_Size *= 2;
			}

			CPointer<uint8_t> buffer = Mem.Av_MAlloc<uint8_t>((size_t)buffer_Size);

			if (buffer.IsNull)
				return Error.ENOMEM;

			sp = AvIoBuf.Avio_Alloc_Context(buffer, buffer_Size, (h.Flags & AvIoFlag.Write) != 0 ? 1 : 0, h, FFUrl_Read2, FFUrl_Write2, FFUrl_Seek2);

			if (sp == null)
			{
				Mem.Av_FreeP(ref buffer);

				return Error.ENOMEM;
			}

			AvIoContext s = sp;

			if (h.Protocol_Whitelist.IsNotNull)
			{
				s.Protocol_Whitelist = Mem.Av_StrDup(h.Protocol_Whitelist);

				if (s.Protocol_Whitelist.IsNull)
				{
					AvIo_CloseP(ref sp);

					return Error.ENOMEM;
				}
			}

			if (h.Protocol_Blacklist.IsNotNull)
			{
				s.Protocol_Blacklist = Mem.Av_StrDup(h.Protocol_Blacklist);

				if (s.Protocol_Blacklist.IsNull)
				{
					AvIo_CloseP(ref sp);

					return Error.ENOMEM;
				}
			}

			s.Direct = (h.Flags & AvIoFlag.Direct) != 0 ? 1 : 0;

			s.Seekable = h.Is_Streamed ? AvIoSeekable.None : AvIoSeekable.Normal;
			s.Max_Packet_Size = max_Packet_Size;
			s.Min_Packet_Size = h.Min_Packet_Size;

			if (h.Prot != null)
			{
				s.Read_Pause = h.Prot.Url_Read_Pause;
				s.Read_Seek = h.Prot.Url_Read_Seek;

				if (h.Prot.Url_Read_Seek != null)
					s.Seekable |= AvIoSeekable.Time;
			}

			((FFIoContext)s).Short_Seek_Get = FFUrl_Get_Short_Seek;
			FF_AvIo_Class.CopyTo(s.Av_Class);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static c_int FFIo_Open_Whitelist(out AvIoContext s, CPointer<char> fileName, AvIoFlag flags, AvIoInterruptCb int_Cb, ref AvDictionary options, CPointer<char> whitelist, CPointer<char> blacklist)//XX 472
		{
			s = null;

			c_int err = FFUrl_Open_Whitelist(out UrlContext h, fileName, flags, int_Cb, ref options, whitelist, blacklist, null);
			if (err < 0)
				return err;

			err = FFIo_FdOpen(out s, h);
			if (err < 0)
			{
				FFUrl_Close(h);
				return err;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Read up to size bytes from the resource accessed by h, and store
		/// the read bytes in buf
		/// </summary>
		/********************************************************************/
		public static c_int FFUrl_Read2(object urlContext, CPointer<uint8_t> buf, c_int size)//XX 549
		{
			UrlContext h = (UrlContext)urlContext;

			if ((h.Flags & AvIoFlag.Read) == 0)
				return Error.EIO;

			return Retry_Transfer_Wrapper(h, buf, null, size, 1, 1);
		}



		/********************************************************************/
		/// <summary>
		/// Write size bytes from buf to the resource accessed by h
		/// </summary>
		/********************************************************************/
		public static c_int FFUrl_Write2(object urlContext, CPointer<uint8_t> buf, c_int size)//XX 565
		{
			UrlContext h = (UrlContext)urlContext;

			if ((h.Flags & AvIoFlag.Write) == 0)
				return Error.EIO;

			// Avoid sending too big packets
			if ((h.Max_Packet_Size != 0) && (size > h.Max_Packet_Size))
				return Error.EIO;

			return Retry_Transfer_Wrapper(h, null, buf, size, size, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Change the position that will be used by the next read/write
		/// operation on the resource accessed by h
		/// </summary>
		/********************************************************************/
		public static int64_t FFUrl_Seek2(object urlContext, int64_t pos, AvSeek whence)//XX 578
		{
			UrlContext h = (UrlContext)urlContext;

			if (h.Prot.Url_Seek == null)
				return Error.ENOSYS;

			int64_t ret = h.Prot.Url_Seek(h, pos, whence & ~AvSeek.Force);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Change the position that will be used by the next read/write
		/// operation on the resource accessed by h
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int64_t FFUrl_Seek(UrlContext h, int64_t pos, AvSeek whence)
		{
			return FFUrl_Seek2(h, pos, whence);
		}



		/********************************************************************/
		/// <summary>
		/// Close the resource accessed by the URLContext h, and free the
		/// memory used by it. Also set the URLContext pointer to NULL
		/// </summary>
		/********************************************************************/
		public static c_int FFUrl_CloseP(ref UrlContext hh)//XX 589
		{
			UrlContext h = hh;
			c_int ret = 0;

			if (h == null)
				return 0;	// Can happend when ffurl_open fails

			if (h.Is_Connected && (h.Prot.Url_Close != null))
				ret = h.Prot.Url_Close(h);

			if (h.Prot.Priv_Data_Allocator != null)
			{
				if (h.Prot.Priv_Data_Class != null)
					Opt.Av_Opt_Free(h.Priv_Data);

				Mem.Av_FreeP(ref h.Priv_Data);
			}

			Opt.Av_Opt_Free(h);
			Mem.Av_FreeP(ref hh);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Close the resource accessed by the URLContext h, and free the
		/// memory used by it
		/// </summary>
		/********************************************************************/
		public static c_int FFUrl_Close(UrlContext h)//XX 589
		{
			return FFUrl_CloseP(ref h);
		}



		/********************************************************************/
		/// <summary>
		/// Close the resource accessed by the AVIOContext s and free it.
		/// This function can only be used if s was opened by avio_open().
		///
		/// The internal buffer is automatically flushed before closing the
		/// resource
		/// </summary>
		/********************************************************************/
		public static c_int AvIo_Close(AvIoContext s)//XX 617
		{
			FFIoContext ctx = AvIo_Internal.FFIoContext(s);

			if (s == null)
				return 0;

			AvIoBuf.AvIo_Flush(s);

			UrlContext h = (UrlContext)s.Opaque;
			s.Opaque = null;

			Mem.Av_FreeP(ref s.Buffer);
			Opt.Av_Opt_Free(s);

			c_int error = s.Error;
			AvIoBuf.AvIo_Context_Free(ref s);

			c_int ret = FFUrl_Close(h);

			if (ret < 0)
				return ret;

			return error;
		}



		/********************************************************************/
		/// <summary>
		/// Close the resource accessed by the AVIOContext *s, free it and
		/// set the pointer pointing to it to NULL.
		/// This function can only be used if s was opened by avio_open().
		///
		/// The internal buffer is automatically flushed before closing the
		/// resource
		/// </summary>
		/********************************************************************/
		public static c_int AvIo_CloseP(ref AvIoContext s)//XX 650
		{
			c_int ret = AvIo_Close(s);
			s = null;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current short seek threshold value for this URL
		/// </summary>
		/********************************************************************/
		public static c_int FFUrl_Get_Short_Seek(object urlContext)//XX 839
		{
			UrlContext h = (UrlContext)urlContext;

			if ((h == null) || (h.Prot == null) || (h.Prot.Url_Get_Short_Seek == null))
				return Error.ENOSYS;

			return h.Prot.Url_Get_Short_Seek(h);
		}



		/********************************************************************/
		/// <summary>
		/// Check if the user has requested to interrupt a blocking function
		/// associated with cb
		/// </summary>
		/********************************************************************/
		public static c_int FF_Check_Interrupt(AvIoInterruptCb cb)//XX 855
		{
			if ((cb != null) && (cb.Callback != null))
				return cb.Callback(cb.Opaque);

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static CPointer<char> UrlContext_To_Name(IClass ptr)//XX 40
		{
			UrlContext h = (UrlContext)ptr;

			if (h.Prot != null)
				return h.Prot.Name;
			else
				return "NULL".ToCharPointer();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IEnumerable<IOptionContext> UrlContext_Child_Next(IOptionContext obj)//XX 49
		{
			UrlContext h = (UrlContext)obj;

			if ((h.Priv_Data != null) && (h.Prot.Priv_Data_Class != null))
				yield return h.Priv_Data;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IEnumerable<IOptionContext> AvIo_Child_Next(IOptionContext obj)//XX 77
		{
			AvIoContext s = (AvIoContext)obj;

			yield return (IOptionContext)s.Opaque;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IEnumerable<AvClass> Child_Class_Iterate()//XX 83
		{
			yield return url_Context_Class;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Url_Alloc_For_Protocol(out UrlContext pUc, UrlProtocol up, CPointer<char> fileName, AvIoFlag flags, AvIoInterruptCb int_Cb)//XX 118
		{
			pUc = null;

			c_int err;

			if (((flags & AvIoFlag.Read) != 0) && (up.Url_Read == null))
				return Error.EIO;

			if (((flags & AvIoFlag.Write) != 0) && (up.Url_Write == null))
				return Error.EIO;

			UrlContext uc = Mem.Av_MAlloczObj<UrlContext>();
			if (uc == null)
			{
				err = Error.ENOMEM;
				goto Fail;
			}

			url_Context_Class.CopyTo(uc.Av_Class);
			uc.FileName = new CPointer<char>((c_int)CString.strlen(fileName) + 1);
			CString.strcpy(uc.FileName, fileName);
			uc.Prot = up;
			uc.Flags = flags;
			uc.Is_Streamed = false;		// Default: not streamed
			uc.Max_Packet_Size = 0;		// Default: stream file

			if (up.Priv_Data_Allocator != null)
			{
				uc.Priv_Data = up.Priv_Data_Allocator();

				if (uc.Priv_Data == null)
				{
					err = Error.ENOMEM;
					goto Fail;
				}

				if (up.Priv_Data_Class != null)
				{
					up.Priv_Data_Class.CopyTo(uc.Priv_Data);
					Opt.Av_Opt_Set_Defaults(uc.Priv_Data);

					if ((AvString.Av_StrStart(uc.FileName, up.Name, out CPointer<char> start) != 0) && (start[0] == ','))
					{
						c_int ret = 0;
						CPointer<char> p = start;
						char sep = p[1, 1];
						CPointer<char> key = null, val;
						p++;

						if (CString.strcmp(up.Name, "subfile") != 0)
							ret = Error.EINVAL;

						while ((ret >= 0) && (key = CString.strchr(p, sep)).IsNotNull && (p < key) && (val = CString.strchr(key + 1, sep)).IsNotNull)
						{
							val[0] = key[0] = '\0';

							ret = Opt.Av_Opt_Set(uc.Priv_Data, p, key + 1, AvOptSearch.None);

							val[0] = key[0] = sep;
							p = val + 1;
						}

						if ((ret < 0) || (p != key))
						{
							err = Error.EINVAL;
							goto Fail;
						}

						CMemory.memmove(start, key + 1, CString.strlen(key));
					}
				}
			}

			if (int_Cb != null)
				uc.Interrupt_Callback = int_Cb.MakeDeepClone();

			pUc = uc;

			return 0;

			Fail:
			pUc = null;

			if (uc != null)
				Mem.Av_FreeP(ref uc.Priv_Data);

			Mem.Av_FreeP(ref uc);

			return err;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static UrlProtocol Url_Find_Protocol(CPointer<char> fileName)//XX 307
		{
			CPointer<char> proto_Str = new CPointer<char>(128);
			CPointer<char> proto_Nested = new CPointer<char>(128);
			size_t proto_Len = CString.strspn(fileName, Url_Scheme_Chars);

			if ((fileName[proto_Len] != ':') && ((CString.strncmp(fileName, "subfile,", 8) != 0) || CString.strchr(fileName + proto_Len + 1, ':').IsNull) || Os_Support.Is_Dos_Path(fileName))
				CString.strcpy(proto_Str, "file");
			else
				AvString.Av_Strlcpy(proto_Str, fileName, Macros.FFMin(proto_Len + 1, (size_t)proto_Str.Length));

			AvString.Av_Strlcpy(proto_Nested, proto_Str, (size_t)proto_Nested.Length);

			CPointer<char> ptr = CString.strchr(proto_Nested, '+');
			if (ptr.IsNotNull)
				ptr[0] = '\0';

			foreach (UrlProtocol up in Protocols.FFUrl_Get_Protocols(null, null))
			{
				if (CString.strcmp(proto_Str, up.Name) == 0)
					return up;

				if (((up.Flags & UrlProtocolFlag.Nested_Scheme) != 0) && (CString.strcmp(proto_Nested, up.Name) == 0))
					return up;
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_int Retry_Transfer_Wrapper(UrlContext h, CPointer<uint8_t> buf, CPointer<uint8_t> cBuf, c_int size, c_int size_Min, c_int read)//XX 504
		{
			c_int fast_Retries = 5;
			int64_t wait_Since = 0;

			c_int len = 0;

			while (len < size_Min)
			{
				if (FF_Check_Interrupt(h.Interrupt_Callback) != 0)
					return Error.Exit;

				c_int ret = read != 0 ? h.Prot.Url_Read(h, buf + len, size - len) : h.Prot.Url_Write(h, cBuf + len, size - len);

				if (ret == Error.EINTR)
					continue;

				if ((h.Flags & AvIoFlag.NonBlock) != 0)
					return ret;

				if (ret == Error.EAGAIN)
				{
					ret = 0;

					if (fast_Retries != 0)
						fast_Retries--;
					else
					{
						if (h.Rw_Timeout != 0)
						{
							if (wait_Since == 0)
								wait_Since = Time.Av_GetTime_Relative();
							else if (Time.Av_GetTime_Relative() > (wait_Since + h.Rw_Timeout))
								return Error.EIO;
						}

						Time.Av_USleep(1000);
					}
				}
				else if (ret == Error.EOF)
					return len > 0 ? len : Error.EOF;
				else if (ret < 0)
					return ret;

				if (ret != 0)
				{
					fast_Retries = Macros.FFMax(fast_Retries, 2);
					wait_Since = 0;
				}

				len += ret;
			}

			return len;
		}
		#endregion
	}
}
