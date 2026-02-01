/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using System.Text;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat
{
	/// <summary>
	/// Buffered I/O
	/// </summary>
	public static class AvIoBuf
	{
		private const c_int Io_Buffer_Size = 32768;

		/// <summary>
		/// Do seeks within this distance ahead of the current buffer by skipping
		/// data instead of calling the protocol seek function, for seekable
		/// protocols
		/// </summary>
		private const c_int Short_Seek_Treshold = 32768;

		private delegate c_uint GetStr16_Read_Delegate(AvIoContext s);

		/********************************************************************/
		/// <summary>
		/// ftell() equivalent for AVIOContext
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int64_t AvIo_Tell(AvIoContext s)
		{
			return AvIo_Seek(s, 0, AvSeek.Cur);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static void FFIo_Init_Context(FFIoContext ctx, CPointer<c_uchar> buffer, c_int buffer_Size, c_int write_Flag, object opaque, FormatFunc.Read_Packet_Buffer_Delegate read_Packet, FormatFunc.Write_Packet_Buffer_Delegate write_Packet, FormatFunc.Seek_Delegate seek)//XX 50
		{
			AvIoContext s = ctx.Pub;

			ctx.Clear();

			s.Buffer = buffer;
			ctx.Orig_Buffer_Size = s.Buffer_Size = buffer_Size;
			s.Buf_Ptr = buffer;
			s.Buf_Ptr_Max = buffer;
			s.Opaque = opaque;
			s.Direct = 0;

			Url_ResetBuf(s, write_Flag != 0 ? AvIoFlag.Write : AvIoFlag.Read);

			s.Write_Packet = write_Packet;
			s.Read_Packet = read_Packet;
			s.Seek = seek;
			s.Pos = 0;
			s.Eof_Reached = 0;
			s.Error = 0;
			s.Seekable = seek != null ? AvIoSeekable.Normal : AvIoSeekable.None;
			s.Min_Packet_Size = 0;
			s.Max_Packet_Size = 0;
			s.Update_Checksum = null;
			ctx.Short_Seek_Threshold = Short_Seek_Treshold;

			if ((read_Packet == null) && (write_Flag == 0))
			{
				s.Pos = buffer_Size;
				s.Buf_End = s.Buffer + buffer_Size;
			}

			s.Read_Pause = null;
			s.Read_Seek = null;

			s.Write_Data_Type = null;
			s.Ignore_Boundary_Point = 0;
			ctx.Current_Type = AvIoDataMarkerType.Unknown;
			ctx.Last_Time = UtilConstants.Av_NoPts_Value;
			ctx.Short_Seek_Get = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static void FFIo_Init_Read_Context(FFIoContext s, CPointer<uint8_t> buffer, c_int buffer_Size)//XX 99
		{
			FFIo_Init_Context(s, buffer, buffer_Size, 0, null, null, null, null);
		}



		/********************************************************************/
		/// <summary>
		/// Allocate and initialize an AVIOContext for buffered I/O. It must
		/// be later freed with avio_context_free()
		/// </summary>
		/********************************************************************/
		public static AvIoContext Avio_Alloc_Context(CPointer<c_uchar> buffer, c_int buffer_Size, c_int write_Flag, object opaque, FormatFunc.Read_Packet_Buffer_Delegate read_Packet, FormatFunc.Write_Packet_Buffer_Delegate write_Packet, FormatFunc.Seek_Delegate seek)//XX 109
		{
			FFIoContext s = Mem.Av_MAllocObj<FFIoContext>();

			if (s == null)
				return null;

			FFIo_Init_Context(s, buffer, buffer_Size, write_Flag, opaque, read_Packet, write_Packet, seek);

			return s.Pub;
		}



		/********************************************************************/
		/// <summary>
		/// Free the supplied IO context and everything associated with it
		/// </summary>
		/********************************************************************/
		public static void AvIo_Context_Free(ref AvIoContext pS)//XX 126
		{
			AvIoContext s = pS;

			if (s != null)
			{
				Mem.Av_FreeP(ref s.Protocol_Whitelist);
				Mem.Av_FreeP(ref s.Protocol_Blacklist);
			}

			Mem.Av_FreeP(ref pS);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void AvIo_W8(AvIoContext s, c_int b)//XX 184
		{
			s.Buf_Ptr[0, 1] = (c_uchar)b;

			if (s.Buf_Ptr >= s.Buf_End)
				Flush_Buffer(s);
		}



		/********************************************************************/
		/// <summary>
		/// Read size bytes from AVIOContext, returning a pointer.
		/// Note that the data pointed at by the returned pointer is only
		/// valid until the next call that references the same IO context
		/// </summary>
		/********************************************************************/
		internal static void FFIo_Fill(AvIoContext s, c_int b, int64_t count)//XX 192
		{
			while (count > 0)
			{
				c_int len = Macros.FFMin(s.Buf_End - s.Buf_Ptr, (c_int)count);

				CMemory.memset<c_uchar>(s.Buf_Ptr, (c_uchar)b, (size_t)len);
				s.Buf_Ptr += len;

				if (s.Buf_Ptr >= s.Buf_End)
					Flush_Buffer(s);

				count -= len;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Force flushing of buffered data.
		///
		/// For write streams, force the buffered data to be immediately
		/// written to the output, without to wait to fill the internal
		/// buffer.
		///
		/// For read streams, discard all currently buffered data, and
		/// advance the reported file position to that of the underlying
		/// stream. This does not read new data, and does not perform any
		/// seeks
		/// </summary>
		/********************************************************************/
		public static void AvIo_Flush(AvIoContext s)//XX 228
		{
			c_int seekBack = s.Write_Flag != 0 ? Macros.FFMin(0, s.Buf_Ptr - s.Buf_Ptr_Max) : 0;

			Flush_Buffer(s);

			if (seekBack != 0)
				AvIo_Seek(s, seekBack, AvSeek.Cur);
		}



		/********************************************************************/
		/// <summary>
		/// fseek() equivalent for AVIOContext
		/// </summary>
		/********************************************************************/
		public static int64_t AvIo_Seek(AvIoContext s, int64_t offset, AvSeek whence)//XX 236
		{
			FFIoContext ctx = AvIo_Internal.FFIoContext(s);
			int64_t offset1;

			whence &= ~AvSeek.Force;	// Force flag does nothing

			if (s == null)
				return Error.EINVAL;

			if ((whence & AvSeek.Size) != 0)
				return s.Seek != null ? s.Seek(s.Opaque, offset, AvSeek.Size) : Error.ENOSYS;

			c_int buffer_Size = s.Buf_End - s.Buffer;

			// Pos is the absolute position that the beginning of s->buffer corresponds to in the file
			int64_t pos = s.Pos - (s.Write_Flag != 0 ? 0 : buffer_Size);

			if ((whence != AvSeek.Cur) && (whence != AvSeek.Set))
				return Error.EINVAL;

			if (whence == AvSeek.Cur)
			{
				offset1 = pos + (s.Buf_Ptr - s.Buffer);

				if (offset == 0)
					return offset1;

				if (offset > (int64_t.MaxValue - offset1))
					return Error.EINVAL;

				offset += offset1;
			}

			if (offset < 0)
				return Error.EINVAL;

			c_int short_Seek = ctx.Short_Seek_Threshold;

			if (ctx.Short_Seek_Get != null)
			{
				c_int tmp = ctx.Short_Seek_Get(s.Opaque);
				short_Seek = Macros.FFMax(tmp, short_Seek);
			}

			offset1 = offset - pos;		// "offset1" is the relative offset from the beginning of s->buffer
			s.Buf_Ptr_Max = s.Buf_Ptr_Max > s.Buf_Ptr ? s.Buf_Ptr_Max : s.Buf_Ptr;

			if (((s.Direct == 0) || (s.Seek == null)) && (offset1 >= 0) && (offset1 <= (s.Write_Flag != 0 ? s.Buf_Ptr_Max - s.Buffer : buffer_Size)))
			{
				// Can do the seek inside the buffer
				s.Buf_Ptr = s.Buffer + offset1;
			}
			else if ((((s.Seekable & AvIoSeekable.Normal) == 0) || (offset1 <= (buffer_Size + short_Seek))) && (s.Write_Flag == 0) && (offset1 >= 0) && ((s.Direct == 0) || (s.Seek == null)))
			{
				while ((s.Pos < offset) && (s.Eof_Reached == 0))
					Fill_Buffer(s);

				if (s.Eof_Reached != 0)
					return Error.EOF;

				s.Buf_Ptr = s.Buf_End - (s.Pos - offset);
			}
			else if ((s.Write_Flag == 0) && (offset1 < 0) && (-offset1 < (buffer_Size >> 1)) && (s.Seek != null) && (offset > 0))
			{
				pos -= Macros.FFMin(buffer_Size >> 1, pos);

				int64_t res = s.Seek(s.Opaque, pos, AvSeek.Set);

				if (res < 0)
					return res;

				s.Buf_End = s.Buf_Ptr = s.Buffer;
				s.Pos = pos;
				s.Eof_Reached = 0;

				Fill_Buffer(s);

				return AvIo_Seek(s, offset, AvSeek.Set);
			}
			else
			{
				if (s.Write_Flag != 0)
					Flush_Buffer(s);

				if (s.Seek == null)
					return Error.EPIPE;

				int64_t res = s.Seek(s.Opaque, offset, AvSeek.Set);

				if (res < 0)
					return res;

				ctx.Seek_Count++;

				if (s.Write_Flag == 0)
					s.Buf_End = s.Buffer;

				s.Checksum_Ptr = s.Buf_Ptr = s.Buf_Ptr_Max = s.Buffer;
				s.Pos = offset;
			}

			s.Eof_Reached = 0;

			return offset;
		}



		/********************************************************************/
		/// <summary>
		/// Skip given number of bytes forward
		/// </summary>
		/********************************************************************/
		public static int64_t AvIo_Skip(AvIoContext s, int64_t offset)//XX 321
		{
			return AvIo_Seek(s, offset, AvSeek.Cur);
		}



		/********************************************************************/
		/// <summary>
		/// Get the filesize
		/// </summary>
		/********************************************************************/
		public static int64_t AvIo_Size(AvIoContext s)//XX 326
		{
			FFIoContext ctx = AvIo_Internal.FFIoContext(s);

			if (s == null)
				return Error.EINVAL;

			if (ctx.Written_Output_Size != 0)
				return ctx.Written_Output_Size;

			if (s.Seek == null)
				return Error.ENOSYS;

			int64_t size = s.Seek(s.Opaque, 0, AvSeek.Size);

			if (size < 0)
			{
				size = s.Seek(s.Opaque, -1, AvSeek.End);

				if (size < 0)
					return size;

				size++;

				s.Seek(s.Opaque, s.Pos, AvSeek.Set);
			}

			return size;
		}



		/********************************************************************/
		/// <summary>
		/// Similar to feof() but also returns nonzero on read errors
		/// </summary>
		/********************************************************************/
		public static c_int AvIo_FEof(AvIoContext s)//XX 349
		{
			if (s == null)
				return 0;

			if (s.Eof_Reached != 0)
			{
				s.Eof_Reached = 0;
				Fill_Buffer(s);
			}

			return s.Eof_Reached;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int AvIo_R8(AvIoContext s)//XX 606
		{
			if (s.Buf_Ptr >= s.Buf_End)
				Fill_Buffer(s);

			if (s.Buf_Ptr < s.Buf_End)
				return s.Buf_Ptr[0, 1];

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Read size bytes from AVIOContext into buf
		/// </summary>
		/********************************************************************/
		public static c_int AvIo_Read(AvIoContext s, CPointer<char> buf, c_int size)
		{
			CPointer<c_uchar> tmpBuf = new CPointer<c_uchar>(size);

			c_int len = AvIo_Read(s, tmpBuf, size);

			for (c_int i = 0; i < len; i++)
				buf[i] = (char)tmpBuf[i];

			return len;
		}



		/********************************************************************/
		/// <summary>
		/// Read size bytes from AVIOContext into buf
		/// </summary>
		/********************************************************************/
		public static c_int AvIo_Read(AvIoContext s, CPointer<c_uchar> buf, c_int size)//XX 615
		{
			c_int size1 = size;

			while (size > 0)
			{
				c_int len = Macros.FFMin(s.Buf_End - s.Buf_Ptr, size);

				if ((len == 0) || (s.Write_Flag != 0))
				{
					if (((s.Direct != 0) || (size > s.Buffer_Size)) && (s.Update_Checksum == null) && (s.Read_Packet != null))
					{
						// Bypass the buffer and read data directly into buf
						len = Read_Packet_Wrapper(s, buf, size);

						if (len == Error.EOF)
						{
							// Do not modify buffer if EOF reached so that a seek back can
							// be done without rereading data
							s.Eof_Reached = 1;
							break;
						}
						else if (len < 0)
						{
							s.Eof_Reached = 1;
							s.Error = len;
							break;
						}
						else
						{
							s.Pos += len;
							AvIo_Internal.FFIoContext(s).Bytes_Read += len;
							s.Bytes_Read = AvIo_Internal.FFIoContext(s).Bytes_Read;
							size -= len;
							buf += len;

							// Reset the buffer
							s.Buf_Ptr = s.Buffer;
							s.Buf_End = s.Buffer;
						}
					}
					else
					{
						Fill_Buffer(s);

						len = s.Buf_End - s.Buf_Ptr;

						if (len == 0)
							break;
					}
				}
				else
				{
					CMemory.memcpy(buf, s.Buf_Ptr, (size_t)len);

					buf += len;
					s.Buf_Ptr += len;
					size -= len;
				}
			}

			if (size1 == size)
			{
				if (s.Error != 0)
					return s.Error;

				if (AvIo_FEof(s) != 0)
					return Error.EOF;
			}

			return size1 - size;
		}



		/********************************************************************/
		/// <summary>
		/// Read size bytes from AVIOContext into buf.
		/// Check that exactly size bytes have been read
		/// </summary>
		/********************************************************************/
		internal static c_int FFIo_Read_Size(AvIoContext s, CPointer<c_uchar> buf, c_int size)//XX 665
		{
			c_int ret = AvIo_Read(s, buf, size);

			if (ret == size)
				return ret;

			if ((ret < 0) && (ret != Error.EOF))
				return ret;

			return Error.InvalidData;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_uint AvIo_RL16(AvIoContext s)//XX 717
		{
			c_uint val = (c_uint)AvIo_R8(s);
			val |= (c_uint)AvIo_R8(s) << 8;

			return val;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_uint AvIo_RL32(AvIoContext s)//XX 733
		{
			c_uint val = AvIo_RL16(s);
			val |= AvIo_RL16(s) << 16;

			return val;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint64_t AvIo_RL64(AvIoContext s)//XX 741
		{
			uint64_t val = AvIo_RL32(s);
			val |= (uint64_t)AvIo_RL32(s) << 16;

			return val;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_uint AvIo_RB16(AvIoContext s)//XX 749
		{
			c_uint val = (c_uint)AvIo_R8(s) << 8;
			val |= (c_uint)AvIo_R8(s);

			return val;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_uint AvIo_RB24(AvIoContext s)//XX 757
		{
			c_uint val = AvIo_RB16(s) << 8;
			val |= (c_uint)AvIo_R8(s);

			return val;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_uint AvIo_RB32(AvIoContext s)//XX 764
		{
			c_uint val = AvIo_RB16(s) << 16;
			val |= AvIo_RB16(s);

			return val;
		}



		/********************************************************************/
		/// <summary>
		/// Read a string from pb into buf. The reading will terminate when
		/// either a NULL character was encountered, maxlen bytes have been
		/// read, or nothing more can be read from pb. The result is
		/// guaranteed to be NULL-terminated, it will be truncated if buf is
		/// too small.
		/// Note that the string is not interpreted or validated in any way,
		/// it might get truncated in the middle of a sequence for multi-byte
		/// encodings
		/// </summary>
		/********************************************************************/
		public static c_int AvIo_Get_Str(AvIoContext s, c_int maxLen, CPointer<char> buf, c_int bufLen)//XX 869
		{
			if (bufLen <= 0)
				return Error.EINVAL;

			// Reserve 1 byte for terminating 0
			bufLen = Macros.FFMin(bufLen - 1, maxLen);

			c_int i;
			for (i = 0; i < bufLen; i++)
			{
				buf[i] = (char)AvIo_R8(s);

				if (buf[i] == '\0')
					return i + 1;
			}

			buf[i] = '\0';

			for (; i < maxLen; i++)
			{
				if (AvIo_R8(s) == 0)
					return i + 1;
			}

			return maxLen;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int AvIo_Get_Str16LE(AvIoContext pb, c_int maxLen, CPointer<char> buf, c_int bufLen)//XX 906
		{
			return Get_Str16(pb, maxLen, buf, bufLen, AvIo_RL16);
		}



		/********************************************************************/
		/// <summary>
		/// Ensures that the requested seekback buffer size will be available
		///
		/// Will ensure that when reading sequentially up to buf_size,
		/// seeking within the current pos and pos+buf_size is possible.
		/// Once the stream position moves outside this window or another
		/// ffio_ensure_seekback call requests a buffer outside this window
		/// this guarantee is lost
		/// </summary>
		/********************************************************************/
		internal static c_int FFIo_Ensure_SeekBack(AvIoContext s, int64_t buf_Size)//XX 1026
		{
			c_int max_Buffer_Size = s.Max_Packet_Size != 0 ? s.Max_Packet_Size : Io_Buffer_Size;
			ptrdiff_t filled = s.Buf_End - s.Buf_Ptr;

			if (buf_Size <= (s.Buf_End - s.Buf_Ptr))
				return 0;

			if (buf_Size > (c_int.MaxValue - max_Buffer_Size))
				return Error.EINVAL;

			buf_Size += max_Buffer_Size - 1;

			if (((buf_Size + (s.Buf_Ptr - s.Buffer)) <= s.Buffer_Size) || (s.Seekable != AvIoSeekable.None) || (s.Read_Packet == null))
				return 0;

			if (buf_Size <= s.Buffer_Size)
			{
				Update_Checksum(s);
				CMemory.memmove(s.Buffer, s.Buf_Ptr, (size_t)filled);
			}
			else
			{
				CPointer<uint8_t> buffer = Mem.Av_MAlloc<uint8_t>((size_t)buf_Size);

				if (buffer.IsNull)
					return Error.ENOMEM;

				Update_Checksum(s);
				CMemory.memcpy(buffer, s.Buf_Ptr, (size_t)filled);
				Mem.Av_Free(s.Buffer);

				s.Buffer = buffer;
				s.Buffer_Size = (c_int)buf_Size;
			}

			s.Buf_Ptr = s.Buffer;
			s.Buf_End = s.Buffer + filled;
			s.Checksum_Ptr = s.Buffer;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static c_int FFIo_Limit(AvIoContext s, c_int size)//XX 1064
		{
			FFIoContext ctx = AvIo_Internal.FFIoContext(s);

			if (ctx.MaxSize >= 0)
			{
				int64_t pos = AvIo_Tell(s);
				int64_t remaining = ctx.MaxSize - pos;

				if (remaining < size)
				{
					int64_t newSize = AvIo_Size(s);

					if ((ctx.MaxSize == 0) || (ctx.MaxSize < newSize))
						ctx.MaxSize = newSize - (newSize == 0 ? 1 : 0);

					if ((pos > ctx.MaxSize) && (ctx.MaxSize >= 0))
						ctx.MaxSize = Error.EIO;

					if (ctx.MaxSize >= 0)
						remaining = ctx.MaxSize - pos;
				}

				if ((ctx.MaxSize >= 0) && (remaining < size) && (size > 1))
				{
					Log.Av_Log(null, remaining != 0 ? Log.Av_Log_Error : Log.Av_Log_Debug, "Truncating packet of size %d to %lld\n", size, remaining + (remaining == 0 ? 1 : 0));

					size = (c_int)(remaining + (remaining == 0 ? 1 : 0));
				}
			}

			return size;
		}



		/********************************************************************/
		/// <summary>
		/// Rewind the AVIOContext using the specified buffer containing the
		/// first buf_size bytes of the file.
		/// Used after probing to avoid seeking.
		/// Joins buf and s->buffer, taking any overlap into consideration.
		/// Note: s.buffer must overlap with buf or they can't be joined and
		/// the function fails
		/// </summary>
		/********************************************************************/
		internal static c_int FFIo_Rewind_With_Probe_Data(AvIoContext s, ref CPointer<c_uchar> bufP, c_int buf_Size)//XX 1151
		{
			CPointer<uint8_t> buf = bufP;

			if (s.Write_Flag != 0)
			{
				Mem.Av_FreeP(ref bufP);

				return Error.EINVAL;
			}

			c_int buffer_Size = s.Buf_End - s.Buffer;

			// The buffers must touch or overlap
			int64_t buffer_Start = s.Pos - buffer_Size;

			if (buffer_Start > buf_Size)
			{
				Mem.Av_FreeP(ref bufP);

				return Error.EINVAL;
			}

			c_int overlap = (c_int)(buf_Size - buffer_Start);
			c_int new_Size = buf_Size + buffer_Size - overlap;

			c_int alloc_Size = Macros.FFMax(s.Buffer_Size, new_Size);

			if (alloc_Size > buf_Size)
			{
				buf = bufP = Mem.Av_Realloc_F(buf, (size_t)alloc_Size);

				if (buf.IsNull)
					return Error.ENOMEM;
			}

			if (new_Size > buf_Size)
			{
				CMemory.memcpy(buf + buf_Size, s.Buffer + overlap, (size_t)(buffer_Size - overlap));
				buf_Size = new_Size;
			}

			Mem.Av_Free(s.Buffer);

			s.Buf_Ptr = s.Buffer = buf;
			s.Buffer_Size = alloc_Size;
			s.Pos = buf_Size;
			s.Buf_End = s.Buf_Ptr + buf_Size;
			s.Eof_Reached = 0;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Open a write only memory stream
		/// </summary>
		/********************************************************************/
		public static c_int AvIo_Open_Dyn_Buf(out AvIoContext s)//XX 1365
		{
			return Url_Open_Dyn_Buf_Internal(out s, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Return the written size and a pointer to the buffer. The buffer
		/// must be freed with av_free().
		/// Padding of AV_INPUT_BUFFER_PADDING_SIZE is added to the buffer
		/// </summary>
		/********************************************************************/
		public static c_int AvIo_Close_Dyn_Buf(AvIoContext s, out CPointer<uint8_t> pBuffer)//XX 1410
		{
			c_int padding = 0;

			if (s == null)
			{
				pBuffer = null;

				return 0;
			}

			// Don't attempt to pad fixed-size packet buffers
			if (s.Max_Packet_Size == 0)
			{
				FFIo_Fill(s, 0, Defs.Av_Input_Buffer_Padding_Size);
				padding = Defs.Av_Input_Buffer_Padding_Size;
			}

			AvIo_Flush(s);

			DynBuffer d = (DynBuffer)s.Opaque;
			pBuffer = d.Buffer;
			c_int size = d.Size;

			AvIo_Context_Free(ref s);

			return size - padding;
		}



		/********************************************************************/
		/// <summary>
		/// Free a dynamic buffer
		/// </summary>
		/********************************************************************/
		internal static void FFIo_Free_Dyn_Buf(ref AvIoContext s)//XX 1438
		{
			if (s == null)
				return;

			DynBuffer d = (DynBuffer)s.Opaque;
			Mem.Av_Free(d.Buffer);

			AvIo_Context_Free(ref s);
		}


		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void WriteOut(AvIoContext s, CPointer<uint8_t> data, c_int len)//XX 136
		{
			FFIoContext ctx = AvIo_Internal.FFIoContext(s);

			if (s.Error == 0)
			{
				c_int ret = 0;

				if (s.Write_Data_Type != null)
					ret = s.Write_Data_Type(s.Opaque, data, len, ctx.Current_Type, ctx.Last_Time);
				else if (s.Write_Packet != null)
					ret = s.Write_Packet(s.Opaque, data, len);

				if (ret < 0)
					s.Error = ret;
				else
				{
					ctx.Bytes_Written += len;
					s.Bytes_Written = ctx.Bytes_Written;

					if ((s.Pos + len) > ctx.Written_Output_Size)
						ctx.Written_Output_Size = s.Pos + len;
				}
			}

			if ((ctx.Current_Type == AvIoDataMarkerType.Sync_Point) || (ctx.Current_Type == AvIoDataMarkerType.Boundary_Point))
				ctx.Current_Type = AvIoDataMarkerType.Unknown;

			ctx.Last_Time = UtilConstants.Av_NoPts_Value;
			ctx.WriteOut_Count++;
			s.Pos += len;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Flush_Buffer(AvIoContext s)//XX 168
		{
			s.Buf_Ptr_Max = s.Buf_Ptr > s.Buf_Ptr_Max ? s.Buf_Ptr : s.Buf_Ptr_Max;

			if ((s.Write_Flag != 0) && (s.Buf_Ptr_Max > s.Buffer))
			{
				WriteOut(s, s.Buffer, s.Buf_Ptr_Max - s.Buffer);

				if (s.Update_Checksum != null)
				{
					s.Checksum = s.Update_Checksum(s.Checksum, s.Checksum_Ptr, (c_uint)(s.Buf_Ptr_Max - s.Checksum_Ptr));
					s.Checksum_Ptr = s.Buffer;
				}
			}

			s.Buf_Ptr = s.Buf_Ptr_Max = s.Buffer;

			if (s.Write_Flag == 0)
				s.Buf_End = s.Buffer;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Read_Packet_Wrapper(AvIoContext s, CPointer<uint8_t> buf, c_int size)//XX 501
		{
			if (s.Read_Packet == null)
				return Error.EINVAL;

			c_int ret = s.Read_Packet(s.Opaque, buf, size);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Fill_Buffer(AvIoContext s)//XX 514
		{
			FFIoContext ctx = AvIo_Internal.FFIoContext(s);

			c_int max_Buffer_Size = s.Max_Packet_Size != 0 ? s.Max_Packet_Size : Io_Buffer_Size;
			CPointer<uint8_t> dst = (s.Buf_End - s.Buffer + max_Buffer_Size) <= s.Buffer_Size ? s.Buf_End : s.Buffer;
			c_int len = s.Buffer_Size - (dst - s.Buffer);

			// Can't fill the buffer without read_packet, just set EOF if appropriate
			if ((s.Read_Packet == null) && (s.Buf_Ptr >= s.Buf_End))
				s.Eof_Reached = 1;

			// No need to do anything if EOF already reached
			if (s.Eof_Reached != 0)
				return;

			if ((s.Update_Checksum != null) && (dst == s.Buffer))
			{
				if (s.Buf_End > s.Checksum_Ptr)
					s.Checksum = s.Update_Checksum(s.Checksum, s.Checksum_Ptr, (c_uint)(s.Buf_End - s.Checksum_Ptr));

				s.Checksum_Ptr = s.Buffer;
			}

			// Make buffer smaller in case it ended up large after probing
			if ((s.Read_Packet != null) && (ctx.Orig_Buffer_Size != 0) && (s.Buffer_Size > ctx.Orig_Buffer_Size) && (len >= ctx.Orig_Buffer_Size))
			{
				if ((dst == s.Buffer) && (s.Buf_Ptr != dst))
				{
					c_int ret = Set_Buf_Size(s, ctx.Orig_Buffer_Size);

					if (ret < 0)
						Log.Av_Log(s, Log.Av_Log_Warning, "Failed to decrease buffer size\n");

					s.Checksum_Ptr = dst = s.Buffer;
				}

				len = ctx.Orig_Buffer_Size;
			}

			len = Read_Packet_Wrapper(s, dst, len);

			if (len == Error.EOF)
			{
				// Do not modify buffer if EOF reached so that a seek back can
				// be done without rereading data
				s.Eof_Reached = 1;
			}
			else if (len < 0)
			{
				s.Eof_Reached = 1;
				s.Error = len;
			}
			else
			{
				s.Pos += len;
				s.Buf_Ptr = dst;
				s.Buf_End = dst + len;
				AvIo_Internal.FFIoContext(s).Bytes_Read += len;
				s.Bytes_Read = AvIo_Internal.FFIoContext(s).Bytes_Read;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_int Get_Str16(AvIoContext pb, c_int maxLen, CPointer<char> buf, c_int bufLen, GetStr16_Read_Delegate read)//XX
		{
			CPointer<uint8_t> _buf = new CPointer<uint8_t>(bufLen);
			CPointer<uint8_t> q = _buf;
			c_int ret = 0;

			if (bufLen <= 0)
				return Error.EINVAL;

			while ((ret + 1) < maxLen)
			{
				uint32_t ch = Common.Get_Utf16(() => (ret += 2) <= maxLen ? (uint16_t)read(pb) : (uint16_t)0, out bool error);

				if (error)
					break;

				if (ch == 0)
					break;

				Common.Put_Utf8(ch, tmp =>
				{
					if ((q - _buf) < (bufLen - 1))
						q[0, 1] = tmp;
				});
			}

			c_int len = q - _buf;
			Encoding.UTF8.GetString(_buf.AsSpan(len)).CopyTo(buf.AsSpan());

			buf[len] = '\0';

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Update_Checksum(AvIoContext s)//XX 1018
		{
			if ((s.Update_Checksum != null) && (s.Buf_Ptr > s.Checksum_Ptr))
				s.Checksum = s.Update_Checksum(s.Checksum, s.Checksum_Ptr, (c_uint)(s.Buf_Ptr - s.Checksum_Ptr));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Set_Buf_Size(AvIoContext s, c_int buf_Size)//XX 1090
		{
			CPointer<uint8_t> buffer = Mem.Av_MAlloc<uint8_t>((size_t)buf_Size);

			if (buffer.IsNull)
				return Error.ENOMEM;

			Mem.Av_Free(s.Buffer);
			s.Buffer = buffer;

			AvIo_Internal.FFIoContext(s).Orig_Buffer_Size = s.Buffer_Size = buf_Size;
			s.Buf_Ptr = s.Buf_Ptr_Max = buffer;

			Url_ResetBuf(s, s.Write_Flag != 0 ? AvIoFlag.Write : AvIoFlag.Read);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Url_ResetBuf(AvIoContext s, AvIoFlag flags)//XX 1137
		{
			if ((flags & AvIoFlag.Write) != 0)
			{
				s.Buf_End = s.Buffer + s.Buffer_Size;
				s.Write_Flag = 1;
			}
			else
			{
				s.Buf_End = s.Buffer;
				s.Write_Flag = 0;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Dyn_Buf_Write(object opaque, CPointer<uint8_t> buf, c_int buf_Size)//XX 1281
		{
			DynBuffer d = (DynBuffer)opaque;

			// Reallocate buffer if needed
			c_uint new_Size = (c_uint)(d.Pos + buf_Size);

			if ((new_Size < d.Pos) || (new_Size > c_int.MaxValue))
				return Error.ERANGE;

			if (new_Size > d.Allocated_Size)
			{
				c_uint new_Allocated_Size = d.Allocated_Size != 0 ? (c_uint)d.Allocated_Size : new_Size;

				while (new_Size > new_Allocated_Size)
					new_Allocated_Size += (new_Allocated_Size / 2) + 1;

				new_Allocated_Size = Macros.FFMin(new_Allocated_Size, (c_uint)c_int.MaxValue);

				c_int err = Mem.Av_ReallocP(ref d.Buffer, new_Allocated_Size);

				if (err < 0)
				{
					d.Allocated_Size = 0;
					d.Size = 0;

					return err;
				}

				d.Allocated_Size = (c_int)new_Allocated_Size;
			}

			CMemory.memcpy(d.Buffer + d.Pos, buf, (size_t)buf_Size);
			d.Pos = (c_int)new_Size;

			if (d.Pos > d.Size)
				d.Size = d.Pos;

			return buf_Size;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Dyn_Packet_Buf_Write(object opaque, CPointer<uint8_t> buf, c_int buf_Size)//XX 1313
		{
			CPointer<c_uchar> buf1 = new CPointer<c_uchar>(4);

			// Packetized write: output the header
			IntReadWrite.Av_WB32(buf1, (uint32_t)buf_Size);

			c_int ret = Dyn_Buf_Write(opaque, buf1, 4);

			if (ret < 0)
				return ret;

			// Then the data
			return Dyn_Buf_Write(opaque, buf, buf_Size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int64_t Dyn_Buf_Seek(object opaque, int64_t offset, AvSeek whence)//XX 1328
		{
			DynBuffer d = (DynBuffer)opaque;

			if (whence == AvSeek.Cur)
				offset += d.Pos;
			else if (whence == AvSeek.End)
				offset += d.Size;

			if (offset < 0)
				return Error.EINVAL;

			if (offset > c_int.MaxValue)
				return Error.ERANGE;

			d.Pos = (c_int)offset;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Url_Open_Dyn_Buf_Internal(out AvIoContext s, c_int max_Packet_Size)//XX 1344
		{
			FFIoContext pb = new FFIoContext();
			DynBuffer d = new DynBuffer();

			c_uint io_Buffer_Size = max_Packet_Size != 0 ? (c_uint)max_Packet_Size : 1024U;

			d.Io_Buffer = new CPointer<uint8_t>(io_Buffer_Size);
			d.Io_Buffer_Size = (c_int)io_Buffer_Size;

			FFIo_Init_Context(pb, d.Io_Buffer, d.Io_Buffer_Size, 1, d, null, max_Packet_Size != 0 ? Dyn_Packet_Buf_Write : Dyn_Buf_Write, max_Packet_Size != 0 ? null : Dyn_Buf_Seek);

			s = pb.Pub;
			s.Max_Packet_Size = max_Packet_Size;

			return 0;
		}
		#endregion
	}
}
