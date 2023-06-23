/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Ports.LibMpg123.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123
{
	/// <summary>
	/// Different reader implementations
	/// </summary>
	internal class Readers
	{
		private readonly LibMpg123 lib;

		private readonly Reader bad_Reader;
		private readonly Reader[] readers;

		private enum ReaderType
		{
			Stream,
			Icy_Stream,
			Feed,
			Buf_Stream,
			Buf_Icy_Stream
		}

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Readers(LibMpg123 libMpg123)
		{
			lib = libMpg123;

			bad_Reader = new Reader
			{
				Init = Bad_Init,
				Close = Bad_Close,
				FullRead = Bad_FullRead,
				Head_Read = Bad_Head_Read,
				Head_Shift = Bad_Head_Shift,
				Skip_Bytes = Bad_Skip_Bytes,
				Read_Frame_Body = Bad_Read_Frame_Body,
				Back_Bytes = Bad_Back_Bytes,
				Seek_Frame = Bad_Seek_Frame,
				Tell = Bad_Tell,
				Rewind = Bad_Rewind,
				Forget = null
			};

			readers = new Reader[]
			{
				new Reader // READER_STREAM
				{
					Init = Default_Init,
					Close = Stream_Close,
					FullRead = Plain_FullRead,
					Head_Read = Generic_Head_Read,
					Head_Shift = Generic_Head_Shift,
					Skip_Bytes = Stream_Skip_Bytes,
					Read_Frame_Body = Generic_Read_Frame_Body,
					Back_Bytes = Stream_Back_Bytes,
					Seek_Frame = Stream_Seek_Frame,
					Tell = Generic_Tell,
					Rewind = Stream_Rewind,
					Forget = null
				},
				new Reader // READER_ICY_STREAM
				{
					Init = Default_Init,
					Close = Stream_Close,
					FullRead = Icy_FullRead,
					Head_Read = Generic_Head_Read,
					Head_Shift = Generic_Head_Shift,
					Skip_Bytes = Stream_Skip_Bytes,
					Read_Frame_Body = Generic_Read_Frame_Body,
					Back_Bytes = Stream_Back_Bytes,
					Seek_Frame = Stream_Seek_Frame,
					Tell = Generic_Tell,
					Rewind = Stream_Rewind,
					Forget = null
				},
				new Reader // READER_FEED
				{
					Init = Feed_Init,
					Close = Stream_Close,
					FullRead = Feed_Read,
					Head_Read = Generic_Head_Read,
					Head_Shift = Generic_Head_Shift,
					Skip_Bytes = Feed_Skip_Bytes,
					Read_Frame_Body = Generic_Read_Frame_Body,
					Back_Bytes = Feed_Back_Bytes,
					Seek_Frame = Feed_Seek_Frame,
					Tell = Generic_Tell,
					Rewind = Stream_Rewind,
					Forget = Buffered_Forget
				},
				new Reader // READER_BUF_STREAM
				{
					Init = Default_Init,
					Close = Stream_Close,
					FullRead = Buffered_FullRead,
					Head_Read = Generic_Head_Read,
					Head_Shift = Generic_Head_Shift,
					Skip_Bytes = Stream_Skip_Bytes,
					Read_Frame_Body = Generic_Read_Frame_Body,
					Back_Bytes = Stream_Back_Bytes,
					Seek_Frame = Stream_Seek_Frame,
					Tell = Generic_Tell,
					Rewind = Stream_Rewind,
					Forget = Buffered_Forget
				},
				new Reader // READER_BUF_ICY_STREAM
				{
					Init = Default_Init,
					Close = Stream_Close,
					FullRead = Buffered_FullRead,
					Head_Read = Generic_Head_Read,
					Head_Shift = Generic_Head_Shift,
					Skip_Bytes = Stream_Skip_Bytes,
					Read_Frame_Body = Generic_Read_Frame_Body,
					Back_Bytes = Stream_Back_Bytes,
					Seek_Frame = Stream_Seek_Frame,
					Tell = Generic_Tell,
					Rewind = Stream_Rewind,
					Forget = Buffered_Forget
				}
			};
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Bc_Prepare(BufferChain bc, size_t pool_Size, size_t bufBlock)
		{
			Bc_PoolSize(bc, pool_Size, bufBlock);

			bc.Pool = null;
			bc.Pool_Fill = 0;

			Bc_Init(bc);	// Ensure that members are zeroed for read-only use
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Bc_PoolSize(BufferChain bc, size_t pool_Size, size_t bufBlock)
		{
			bc.Pool_Size = pool_Size;
			bc.BufBlock = bufBlock;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Bc_Cleanup(BufferChain bc)
		{
			Buffy_Del_Chain(bc.Pool);

			bc.Pool = null;
			bc.Pool_Fill = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Externally called function, returns 0 on success, -1 on error
		/// </summary>
		/********************************************************************/
		public int Feed_More(Mpg123_Handle fr, Span<c_uchar> in_, c_long count)
		{
			c_int ret = Bc_Add(fr.RDat.Buffer, in_, count);
			if (ret != 0)
				ret = (c_int)Mpg123_Errors.Err;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Open_Bad(Mpg123_Handle mh)
		{
			lib.icy.Clear_Icy(mh.Icy);

			mh.Rd = bad_Reader;
			mh.RDat.Flags = 0;

			Bc_Init(mh.RDat.Buffer);

			mh.RDat.FileLen = -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Open_Stream(Mpg123_Handle fr, string bs_FileNam, Stream fd)
		{
			bool filePt_Opened = true;
			Stream filePt;

			lib.icy.Clear_Icy(fr.Icy);		// Can be done inside frame_clear ...?

			if (string.IsNullOrEmpty(bs_FileNam))
			{
				filePt = fd;
				filePt_Opened = false;		// And don't try to close it...
			}
			else
			{
				try
				{
					filePt = File.OpenRead(bs_FileNam);
				}
				catch (Exception)
				{
					fr.Err = Mpg123_Errors.Bad_File;
					return Mpg123_Errors.Err;
				}
			}

			// Now we have something behind filept and can init the reader
			fr.RDat.FileLen = -1;
			fr.RDat.FilePt = filePt;
			fr.RDat.Flags = 0;

			if (filePt_Opened)
				fr.RDat.Flags |= ReaderFlags.Fd_Opened;

			return Open_Finish(fr);
		}

		#region Private members
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ssize_t Posix_Read(Stream fd, Memory<c_uchar> buf, size_t count)
		{
			return fd.Read(buf.Span.Slice(0, (int)count));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ssize_t Posix_LSeek(Stream fd, off_t offset, SeekOrigin whence)
		{
			try
			{
				return fd.Seek(offset, whence);
			}
			catch (Exception)
			{
				return -1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ssize_t Nix_LSeek(Stream fd, off_t offset, SeekOrigin whence)
		{
			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// A normal read
		/// </summary>
		/********************************************************************/
		private ssize_t Plain_Read(Mpg123_Handle fr, Memory<c_uchar> buf, size_t count)
		{
			ssize_t ret = Io_Read(fr.RDat, buf, count);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ssize_t Icy_FullRead(Mpg123_Handle fr, Memory<c_uchar> buf, ssize_t count)
		{
			ssize_t ret;
			ssize_t cnt = 0;

			if ((fr.RDat.Flags & ReaderFlags.Seekable) != 0)
				return -1;

			// There used to be a check for expected file end here (length value or ID3 flag).
			// This is not needed:
			// 1. EOF is indicated by fdread returning zero bytes anyway.
			// 2. We get false positives of EOF for either files that grew or
			// 3. ... files that have ID3v1 tags in between (stream with intro)
			while (cnt < count)
			{
				// All icy code is inside this if block, everything else is the plain fullread we know
				if (fr.Icy.Next < count - cnt)
				{
					c_uchar[] temp_Buff = new c_uchar[1];
					size_t meta_Size;
					ssize_t cut_Pos;

					// We are near icy-metaint boundary, read up to the boundary
					if (fr.Icy.Next > 0)
					{
						cut_Pos = fr.Icy.Next;
						ret = fr.RDat.FdRead(fr, buf.Slice((int)cnt), (size_t)cut_Pos);
						if (ret < 1)
						{
							if (ret == 0)
								break;		// Just EOF

							return (c_int)Mpg123_Errors.Err;
						}

						if ((fr.RDat.Flags & ReaderFlags.Buffered) == 0)
							Saturate_Add(ref fr.RDat.FilePos, ret, Constant.Off_Max);

						cnt += ret;
						fr.Icy.Next -= (off_t)ret;

						if (fr.Icy.Next > 0)
							continue;
					}

					// Now off to read icy data
					//
					// One byte icy-meta size (must be multiplied by 16 to get icy-meta length)
					ret = fr.RDat.FdRead(fr, temp_Buff, 1);		// Getting one single byte
					if (ret < 0)
						return (c_int)Mpg123_Errors.Err;

					if (ret == 0)
						break;

					if ((fr.RDat.Flags & ReaderFlags.Buffered) == 0)
						Saturate_Add(ref fr.RDat.FilePos, ret, Constant.Off_Max);	// 1...

					if (((meta_Size = temp_Buff[0]) * 16) != 0)
					{
						// We have got some metadata
						c_uchar[] meta_Buff = new c_uchar[meta_Size + 1];
						if (meta_Buff != null)
						{
							ssize_t left = (ssize_t)meta_Size;

							while (left > 0)
							{
								ret = fr.RDat.FdRead(fr, meta_Buff.AsMemory((int)((ssize_t)meta_Size - left)), (size_t)left);

								// 0 is error here, too... there _must_ be the ICY data, the server promised!
								if (ret < 1)
									return (c_int)Mpg123_Errors.Err;

								left -= ret;
							}

							meta_Buff[meta_Size] = 0;	// string paranoia

							if ((fr.RDat.Flags & ReaderFlags.Buffered) == 0)
								Saturate_Add(ref fr.RDat.FilePos, ret, Constant.Off_Max);

							fr.Icy.Data = meta_Buff;
							fr.MetaFlags |= Mpg123_MetaFlags.New_Icy;
						}
						else
						{
							fr.Rd.Skip_Bytes(fr, (off_t)meta_Size);
						}
					}

					fr.Icy.Next = fr.Icy.Interval;
				}
				else
				{
					ret = Plain_FullRead(fr, buf.Slice((int)cnt), count - cnt);
					if (ret < 0)
						return (c_int)Mpg123_Errors.Err;

					if (ret == 0)
						break;

					cnt += ret;
					fr.Icy.Next -= (off_t)ret;
				}
			}

			return cnt;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ssize_t Plain_FullRead(Mpg123_Handle fr, Memory<c_uchar> buf, ssize_t count)
		{
			ssize_t cnt = 0;

			// There used to be a check for expected file end here (length value or ID3 flag).
			// This is not needed:
			// 1. EOF is indicated by fdread returning zero bytes anyway.
			// 2. We get false positives of EOF for either files that grew or
			// 3. ... files that have ID3v1 tags in between (stream with intro)
			while (cnt < count)
			{
				ssize_t ret = fr.RDat.FdRead(fr, buf.Slice((int)cnt), (size_t)(count - cnt));
				if (ret < 0)
					return (ssize_t)Mpg123_Errors.Err;

				if (ret == 0)
					break;

				if ((fr.RDat.Flags & ReaderFlags.Buffered) == 0)
					Saturate_Add(ref fr.RDat.FilePos, ret, Constant.Off_Max);

				cnt += ret;
			}

			return cnt;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private off_t Stream_LSeek(Mpg123_Handle fr, off_t pos, SeekOrigin whence)
		{
			off_t ret = Io_Seek(fr.RDat, pos, whence);

			if (ret >= 0)
				fr.RDat.FilePos = ret;
			else
			{
				fr.Err = Mpg123_Errors.LSeek_Failed;
				ret = (off_t)Mpg123_Errors.Err;
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Stream_Close(Mpg123_Handle fr)
		{
			if ((fr.RDat.Flags & ReaderFlags.Fd_Opened) != 0)
				fr.RDat.FilePt.Dispose();

			fr.RDat.FilePt = null;

			if ((fr.RDat.Flags & ReaderFlags.Buffered) != 0)
				Bc_Reset(fr.RDat.Buffer);

			if ((fr.RDat.Flags & ReaderFlags.HandleIo) != 0)
			{
				if (fr.RDat.Cleanup_Handle != null)
					fr.RDat.Cleanup_Handle(fr.RDat.IOHandle);

				fr.RDat.IOHandle = null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Stream_Seek_Frame(Mpg123_Handle fr, off_t newFrame)
		{
			// Seekable streams can go backwards and jump forwards.
			// Non-seekable streams still can go forward, just not jump
			if (((fr.RDat.Flags & ReaderFlags.Seekable) != 0) || (newFrame >= fr.Num))
			{
				off_t preFrame;		// A leading frame we jump to
				off_t seek_To;		// The byte offset we want to reach
				off_t to_Skip;		// Bytes to skip to get there (can be negative)

				// Now seek to nearest leading index position and read from there until newFrame is reached.
				// We use skip_Bytes, which handles seekable and non-seekable streams
				// (the latter only for positive offset, which we ensured before entering here)
				seek_To = lib.frame.Frame_Index_Find(fr, newFrame, out preFrame);

				// No need to seek to index position if we are closer already.
				// But I am picky about fr.Num == newFrame, play safe by reading the frame again.
				// If you think that's stupid, don't call a seek to the current frame
				if ((fr.Num >= newFrame) || (fr.Num < preFrame))
				{
					to_Skip = seek_To - fr.Rd.Tell(fr);

					if (fr.Rd.Skip_Bytes(fr, to_Skip) != seek_To)
						return (c_int)Mpg123_Errors.Err;

					fr.Num = preFrame - 1;	// Watch out! I am going to read preFrame... fr.Num should indicate the frame before!
				}

				while (fr.Num < newFrame)
				{
					// Try to be non-fatal now... frameNum only gets advanced on success anyway
					if (lib.parse.Read_Frame(fr) == 0)
						break;
				}

				// Now the wanted frame should be ready for decoding
				return (c_int)Mpg123_Errors.Ok;
			}
			else
			{
				fr.Err = Mpg123_Errors.No_Seek;
				return (c_int)Mpg123_Errors.Err;	// Invalid, no seek happened
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Generic_Head_Read(Mpg123_Handle fr, out c_ulong newHead)
		{
			newHead = 0;
			c_uchar[] hBuf = new c_uchar[4];

			c_int ret = (c_int)fr.Rd.FullRead(fr, hBuf, 4);
			if (ret == (c_int)Mpg123_Errors.Need_More)
				return ret;

			if (ret != 4)
				return 0;

			newHead = ((c_ulong)hBuf[0] << 24) | ((c_ulong)hBuf[1] << 16) | ((c_ulong)hBuf[2] << 8) | hBuf[3];

			return 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Generic_Head_Shift(Mpg123_Handle fr, ref c_ulong head)
		{
			c_uchar[] hBuf = new c_uchar[1];

			c_int ret = (c_int)fr.Rd.FullRead(fr, hBuf, 1);
			if (ret == (c_int)Mpg123_Errors.Need_More)
				return ret;

			if (ret != 1)
				return 0;

			head <<= 8;
			head |= hBuf[0];
			head &= 0xffffffff;

			return 1;
		}



		/********************************************************************/
		/// <summary>
		/// Returns reached position... negative ones are bad
		/// </summary>
		/********************************************************************/
		private off_t Stream_Skip_Bytes(Mpg123_Handle fr, off_t len)
		{
			if ((fr.RDat.Flags & ReaderFlags.Seekable) != 0)
			{
				off_t ret = Stream_LSeek(fr, len, SeekOrigin.Current);
				return ret < 0 ? (off_t)Mpg123_Errors.Err : ret;
			}
			else if (len >= 0)
			{
				c_uchar[] buf = new c_uchar[1024];	// ThOr: Compaq cxx complained and it makes sense to me... or should one do a cast? What for?
				ssize_t ret;

				while (len > 0)
				{
					ssize_t num = len < buf.Length ? len : buf.Length;
					ret = fr.Rd.FullRead(fr, buf, num);

					if (ret < 0)
						return (off_t)ret;
					else if (ret == 0)
						break;		// EOF... an error? Interface defined to tell the actual position...

					len -= (off_t)ret;
				}

				return fr.Rd.Tell(fr);
			}
			else if ((fr.RDat.Flags & ReaderFlags.Buffered) != 0)
			{
				// Perhaps we _can_ go a bit back
				if (fr.RDat.Buffer.Pos >= -len)
				{
					fr.RDat.Buffer.Pos += len;

					return fr.Rd.Tell(fr);
				}
				else
				{
					fr.Err = Mpg123_Errors.No_Seek;

					return (off_t)Mpg123_Errors.Err;
				}
			}
			else
			{
				fr.Err = Mpg123_Errors.No_Seek;

				return (off_t)Mpg123_Errors.Err;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Stream_Back_Bytes(Mpg123_Handle fr, off_t bytes)
		{
			off_t want = fr.Rd.Tell(fr) - bytes;

			if (want < 0)
				return (c_int)Mpg123_Errors.Err;

			if (Stream_Skip_Bytes(fr, -bytes) != want)
				return (c_int)Mpg123_Errors.Err;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Returns size on success... otherwise an error code less than 0
		/// </summary>
		/********************************************************************/
		private c_int Generic_Read_Frame_Body(Mpg123_Handle fr, Memory<c_uchar> buf, c_int size)
		{
			c_long l = (c_long)fr.Rd.FullRead(fr, buf, size);

			return (l >= 0) && (l < size) ? (c_int)Mpg123_Errors.Err : l;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private off_t Generic_Tell(Mpg123_Handle fr)
		{
			if ((fr.RDat.Flags & ReaderFlags.Buffered) != 0)
			{
				fr.RDat.FilePos = fr.RDat.Buffer.FileOff;
				Saturate_Add(ref fr.RDat.FilePos, fr.RDat.Buffer.Pos, Constant.Off_Max);
			}

			return fr.RDat.FilePos;
		}



		/********************************************************************/
		/// <summary>
		/// This does not (fully) work for non-seekable streams... You have
		/// to check for that flag, pal!
		/// </summary>
		/********************************************************************/
		private void Stream_Rewind(Mpg123_Handle fr)
		{
			if ((fr.RDat.Flags & ReaderFlags.Seekable) != 0)
			{
				fr.RDat.FilePos = Stream_LSeek(fr, 0, SeekOrigin.Begin);
				fr.RDat.Buffer.FileOff = fr.RDat.FilePos;
			}

			if ((fr.RDat.Flags & ReaderFlags.Buffered) != 0)
			{
				fr.RDat.Buffer.Pos = 0;
				fr.RDat.Buffer.FirstPos = 0;
				fr.RDat.FilePos = fr.RDat.Buffer.FileOff;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private off_t Get_FileInfo(Mpg123_Handle fr)
		{
			off_t len = Io_Seek(fr.RDat, 0, SeekOrigin.End);
			if (len < 0)
				return -1;
			else if (len >= 128)
			{
				if (Io_Seek(fr.RDat, -128, SeekOrigin.End) < 0)
					return -1;

				if (fr.Rd.FullRead(fr, fr.Id3Buf, 128) != 128)
					return -1;

				if ((fr.Id3Buf[0] == 0x54) && (fr.Id3Buf[1] == 0x41) && (fr.Id3Buf[2] == 0x47))		// TAG
					len -= 128;
			}

			if (Io_Seek(fr.RDat, 0, SeekOrigin.Begin) < 0)
				return -1;

			fr.RDat.FilePos = 0;	// Un-do out seeking here

			return len;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Buffy Buffy_New(size_t size, size_t minSize)
		{
			Buffy newBuf = new Buffy();
			if (newBuf == null)
				return null;

			newBuf.RealSize = (ssize_t)(size > minSize ? size : minSize);
			newBuf.Data = new c_uchar[newBuf.RealSize];
			if (newBuf.Data == null)
				return null;

			newBuf.Size = 0;
			newBuf.Next = null;

			return newBuf;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Buffy_Del(Buffy buf)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Delete this buffy and all following buffies
		/// </summary>
		/********************************************************************/
		private void Buffy_Del_Chain(Buffy buf)
		{
			while (buf != null)
			{
				Buffy next = buf.Next;
				Buffy_Del(buf);
				buf = next;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Bc_Init(BufferChain bc)
		{
			bc.First = null;
			bc.Last = bc.First;
			bc.Size = 0;
			bc.Pos = 0;
			bc.FirstPos = 0;
			bc.FileOff = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Either stuff the buffer back into the pool or free it for good
		/// </summary>
		/********************************************************************/
		private void Bc_Free(BufferChain bc, Buffy buf)
		{
			if (buf == null)
				return;

			if (bc.Pool_Fill < bc.Pool_Size)
			{
				buf.Next = bc.Pool;
				bc.Pool = buf;
				++bc.Pool_Fill;
			}
			else
				Buffy_Del(buf);
		}



		/********************************************************************/
		/// <summary>
		/// Fetch a buffer from the pool (if possible) or create one
		/// </summary>
		/********************************************************************/
		private Buffy Bc_Alloc(BufferChain bc, size_t size)
		{
			// Easy route: Just try the first available buffer.
			// Size does not matter, it's only a hint for creation of new buffers
			if (bc.Pool != null)
			{
				Buffy buf = bc.Pool;
				bc.Pool = buf.Next;
				buf.Next = null;	// That shall be set to a sensible value later
				buf.Size = 0;
				--bc.Pool_Fill;

				return buf;
			}
			else
			{
				return Buffy_New(size, bc.BufBlock);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int Bc_Fill_Pool(BufferChain bc)
		{
			// Remove superfluous ones
			while (bc.Pool_Fill > bc.Pool_Size)
			{
				// Lazyness: Just work on the front
				Buffy buf = bc.Pool;
				bc.Pool = buf.Next;
				Buffy_Del(buf);
				--bc.Pool_Fill;
			}

			// Add missing ones
			while (bc.Pool_Fill < bc.Pool_Size)
			{
				// Again, just work on the front
				Buffy buf = Buffy_New(0, bc.BufBlock);		// Use default block size
				if (buf == null)
					return -1;

				buf.Next = bc.Pool;
				bc.Pool = buf;
				++bc.Pool_Fill;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Bc_Reset(BufferChain bc)
		{
			// Free current chain, possibly stuffing back into the pool
			while (bc.First != null)
			{
				Buffy buf = bc.First;
				bc.First = buf.Next;

				Bc_Free(bc, buf);
			}

			Bc_Fill_Pool(bc);	// Ignoring an error here...
			Bc_Init(bc);
		}



		/********************************************************************/
		/// <summary>
		/// Create a new buffy at the end to be filled
		/// </summary>
		/********************************************************************/
		private int Bc_Append(BufferChain bc, ssize_t size)
		{
			if (size < 1)
				return -1;

			Buffy newBuf = Bc_Alloc(bc, (size_t)size);
			if (newBuf == null)
				return -2;

			if (bc.Last != null)
				bc.Last.Next = newBuf;
			else if (bc.First == null)
				bc.First = newBuf;

			bc.Last = newBuf;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Append a new buffer and copy content to it
		/// </summary>
		/********************************************************************/
		private c_int Bc_Add(BufferChain bc, Span<c_uchar> data, ssize_t size)
		{
			c_int ret = 0;
			ssize_t part = 0;
			ssize_t dataOffset = 0;

			while (size > 0)
			{
				// Try to fill up the last buffer block
				if ((bc.Last != null) && (bc.Last.Size < bc.Last.RealSize))
				{
					part = bc.Last.RealSize - bc.Last.Size;

					if (part > size)
						part = size;

					data.Slice((int)dataOffset, (int)part).CopyTo(bc.Last.Data.AsSpan((int)bc.Last.Size, (int)part));
					bc.Last.Size += part;
					size -= part;
					bc.Size += part;
					dataOffset += part;
				}

				// If there is still data left, put it into a new buffer block
				if ((size > 0) && ((ret = Bc_Append(bc, size)) != 0))
					break;
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Common handler for "You want more than I can give." situation
		/// </summary>
		/********************************************************************/
		private ssize_t Bc_Need_More(BufferChain bc)
		{
			// Go back to firstpos, undo the previous reads
			bc.Pos = bc.FirstPos;

			return (ssize_t)Mpg123_Errors.Need_More;
		}



		/********************************************************************/
		/// <summary>
		/// Give some data, advancing position but not forgetting yet
		/// </summary>
		/********************************************************************/
		private ssize_t Bc_Give(BufferChain bc, Memory<c_uchar> out_, ssize_t size)
		{
			Buffy b = bc.First;
			ssize_t gotCount = 0;
			ssize_t offset = 0;

			if ((bc.Size - bc.Pos) < size)
				return Bc_Need_More(bc);

			// Find the current buffer
			while ((b != null) && (offset + b.Size) <= bc.Pos)
			{
				offset += b.Size;
				b = b.Next;
			}

			// Now start copying from there
			while ((gotCount < size) && (b != null))
			{
				ssize_t lOff = bc.Pos - offset;
				ssize_t chunk = size - gotCount;	// Amount of bytes to get from here...

				b.Data.AsSpan((int)lOff, (int)chunk).CopyTo(out_.Slice((int)gotCount).Span);
				gotCount += chunk;
				bc.Pos += chunk;
				offset += b.Size;

				b = b.Next;
			}

			return gotCount;
		}



		/********************************************************************/
		/// <summary>
		/// Skip some bytes and return new position.
		/// The buffers are still there, just the read pointer is moved!
		/// </summary>
		/********************************************************************/
		private ssize_t Bc_Skip(BufferChain bc, ssize_t count)
		{
			if (count >= 0)
			{
				if ((bc.Size - bc.Pos) < count)
					return Bc_Need_More(bc);
				else
					return bc.Pos += count;
			}
			else
				return (ssize_t)Mpg123_Errors.Err;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ssize_t Bc_SeekBack(BufferChain bc, ssize_t count)
		{
			if ((count >= 0) && (count <= bc.Pos))
				return bc.Pos -= count;
			else
				return (ssize_t)Mpg123_Errors.Err;
		}



		/********************************************************************/
		/// <summary>
		/// Throw away buffies that we passed
		/// </summary>
		/********************************************************************/
		private void Bc_Forget(BufferChain bc)
		{
			Buffy b = bc.First;

			// Free all buffers that are def'n'tly outdated.
			// We have buffers until filepos... delete all buffers fully below it
			while ((b != null) && (bc.Pos >= b.Size))
			{
				Buffy n = b.Next;		// != null or this is indeed the end and the last cycle anyway
				if (n == null)
					bc.Last = null;		// Going to delete the last buffy...

				bc.FileOff += (off_t)b.Size;
				bc.Pos -= b.Size;
				bc.Size -= b.Size;

				Bc_Free(bc, b);
				b = n;
			}

			bc.First = b;
			bc.FirstPos = bc.Pos;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Feed_Init(Mpg123_Handle fr)
		{
			Bc_Init(fr.RDat.Buffer);
			Bc_Fill_Pool(fr.RDat.Buffer);

			fr.RDat.FileLen = 0;
			fr.RDat.FilePos = 0;
			fr.RDat.Flags |= ReaderFlags.Buffered;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ssize_t Feed_Read(Mpg123_Handle fr, Memory<c_uchar> out_, ssize_t count)
		{
			ssize_t gotCount = Bc_Give(fr.RDat.Buffer, out_, count);

			if ((gotCount >= 0) && (gotCount != count))
				return (ssize_t)Mpg123_Errors.Err;
			else
			{
				return gotCount;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Returns reached position... negative ones are bad...
		/// </summary>
		/********************************************************************/
		private off_t Feed_Skip_Bytes(Mpg123_Handle fr, off_t len)
		{
			// This is either the new buffer offset or some negative error value
			off_t res = (off_t)Bc_Skip(fr.RDat.Buffer, len);
			if (res < 0)
				return res;

			return fr.RDat.Buffer.FileOff + res;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Feed_Back_Bytes(Mpg123_Handle fr, off_t bytes)
		{
			if (bytes >= 0)
				return Bc_SeekBack(fr.RDat.Buffer, bytes) >= 0 ? 0 : (c_int)Mpg123_Errors.Err;
			else
				return Feed_Skip_Bytes(fr, -bytes) >= 0 ? 0 : (c_int)Mpg123_Errors.Err;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Feed_Seek_Frame(Mpg123_Handle fr, off_t num)
		{
			return (c_int)Mpg123_Errors.Err;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Buffered_Forget(Mpg123_Handle fr)
		{
			Bc_Forget(fr.RDat.Buffer);

			fr.RDat.FilePos = fr.RDat.Buffer.FileOff;
			Saturate_Add(ref fr.RDat.FilePos, fr.RDat.Buffer.Pos, Constant.Off_Max);
		}



		/********************************************************************/
		/// <summary>
		/// The specific stuff for buffered stream reader
		/// </summary>
		/********************************************************************/
		private ssize_t Buffered_FullRead(Mpg123_Handle fr, Memory<c_uchar> out_, ssize_t count)
		{
			BufferChain bc = fr.RDat.Buffer;

			if ((bc.Size - bc.Pos) < count)
			{   // Add more stuff to buffer. If hitting end of file, adjust count
				c_uchar[] readBuf = new c_uchar[4096];
				ssize_t need = count - (bc.Size - bc.Pos);

				while (need > 0)
				{
					ssize_t got = fr.RDat.FullRead(fr, readBuf, readBuf.Length);
					if (got < 0)
						return (ssize_t)Mpg123_Errors.Err;

					c_int ret;
					if ((got > 0) && ((ret = Bc_Add(bc, readBuf, got)) != 0))
						return (ssize_t)Mpg123_Errors.Err;

					need -= got;	// May underflow here...
					if (got < readBuf.Length)	// That naturally catches got == 0, too
						break;	// End
				}

				if ((bc.Size - bc.Pos) < count)
					count = bc.Size - bc.Pos;	// We want only what we got
			}

			ssize_t gotCount = Bc_Give(bc, out_, count);

			if (gotCount != count)
				return (ssize_t)Mpg123_Errors.Err;
			else
				return gotCount;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Default_Init(Mpg123_Handle fr)
		{
			fr.RDat.FdRead = Plain_Read;

			fr.RDat.Read = fr.RDat.R_Read != null ? fr.RDat.R_Read : Posix_Read;
			fr.RDat.LSeek = fr.RDat.R_LSeek != null ? fr.RDat.R_LSeek : Posix_LSeek;

			// ICY streams of any sort shall not be seekable
			if (fr.P.Icy_Interval > 0)
				fr.RDat.LSeek = Nix_LSeek;

			fr.RDat.FilePos = 0;
			fr.RDat.FileLen = (fr.P.Flags & Mpg123_Param_Flags.No_Peek_End) != 0 ? -1 : Get_FileInfo(fr);

			if ((fr.P.Flags & Mpg123_Param_Flags.Force_Seekable) != 0)
				fr.RDat.Flags |= ReaderFlags.Seekable;

			// Don't enable seeking on ICY streams, just plain normal files.
			// This check is necessary since the client can enforce ICY parsing on files
			// that would otherwise be seekable.
			// It is a task for the future to make the ICY parsing safe with seeks ... or not
			if (fr.RDat.FileLen >= 0)
			{
				fr.RDat.Flags |= ReaderFlags.Seekable;

				if ((fr.Id3Buf[0] == 0x54) && (fr.Id3Buf[1] == 0x41) && (fr.Id3Buf[2] == 0x47))		// TAG
				{
					fr.RDat.Flags |= ReaderFlags.Id3Tag;
					fr.MetaFlags |= Mpg123_MetaFlags.New_Id3;
				}
			}	// Switch reader to a buffered one, if allowed
			else if ((fr.P.Flags & Mpg123_Param_Flags.SeekBuffer) != 0)
			{
				if (fr.Rd == readers[(int)ReaderType.Stream])
				{
					fr.Rd = readers[(int)ReaderType.Buf_Stream];
					fr.RDat.FullRead = Plain_FullRead;
				}
				else if (fr.Rd == readers[(int)ReaderType.Icy_Stream])
				{
					fr.Rd = readers[(int)ReaderType.Buf_Icy_Stream];
					fr.RDat.FullRead = Icy_FullRead;
				}
				else
					return -1;

				Bc_Init(fr.RDat.Buffer);

				fr.RDat.FileLen = 0;		// We carry the offset, but never know how big the stream is
				fr.RDat.Flags |= ReaderFlags.Buffered;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Bad_Init(Mpg123_Handle mh)
		{
			mh.Err = Mpg123_Errors.No_Reader;

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Bad_Close(Mpg123_Handle mh)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ssize_t Bad_FullRead(Mpg123_Handle mh, Memory<c_uchar> data, ssize_t count)
		{
			mh.Err = Mpg123_Errors.No_Reader;

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Bad_Head_Read(Mpg123_Handle mh, out c_ulong newHead)
		{
			newHead = 0;

			mh.Err = Mpg123_Errors.No_Reader;

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Bad_Head_Shift(Mpg123_Handle mh, ref c_ulong head)
		{
			mh.Err = Mpg123_Errors.No_Reader;

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private off_t Bad_Skip_Bytes(Mpg123_Handle mh, off_t len)
		{
			mh.Err = Mpg123_Errors.No_Reader;

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Bad_Read_Frame_Body(Mpg123_Handle mh, Memory<c_uchar> data, c_int size)
		{
			mh.Err = Mpg123_Errors.No_Reader;

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Bad_Back_Bytes(Mpg123_Handle mh, off_t bytes)
		{
			mh.Err = Mpg123_Errors.No_Reader;

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Bad_Seek_Frame(Mpg123_Handle mh, off_t num)
		{
			mh.Err = Mpg123_Errors.No_Reader;

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private off_t Bad_Tell(Mpg123_Handle mh)
		{
			mh.Err = Mpg123_Errors.No_Reader;

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Bad_Rewind(Mpg123_Handle mh)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Final code common to open_stream and open_stream_handle
		/// </summary>
		/********************************************************************/
		private Mpg123_Errors Open_Finish(Mpg123_Handle fr)
		{
			if (fr.P.Icy_Interval > 0)
			{
				fr.Icy.Interval = fr.P.Icy_Interval;
				fr.Icy.Next = fr.Icy.Interval;
				fr.Rd = readers[(int)ReaderType.Icy_Stream];
			}
			else
				fr.Rd = readers[(int)ReaderType.Stream];

			if (fr.Rd.Init(fr) < 0)
				return Mpg123_Errors.Err;

			return Mpg123_Errors.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Wrappers for actual reading/seeking... I'm full of wrappers here
		/// </summary>
		/********************************************************************/
		private off_t Io_Seek(Reader_Data rDat, off_t offset, SeekOrigin whence)
		{
			if ((rDat.Flags & ReaderFlags.HandleIo) != 0)
			{
				if (rDat.R_LSeek_Handle != null)
					return (off_t)rDat.R_LSeek_Handle(rDat.IOHandle, offset, whence);
				else
					return -1;
			}
			else
				return (off_t)rDat.LSeek(rDat.FilePt, offset, whence);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ssize_t Io_Read(Reader_Data rDat, Memory<c_uchar> buf, size_t count)
		{
			if ((rDat.Flags & ReaderFlags.HandleIo) != 0)
			{
				if (rDat.R_Read_Handle != null)
					return rDat.R_Read_Handle(rDat.IOHandle, buf, count);
				else
					return -1;
			}
			else
				return rDat.Read(rDat.FilePt, buf, count);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Saturate_Add(ref off_t inOut, ssize_t add, uint64_t limit)
		{
			inOut = ((off_t)limit - add >= inOut) ? (inOut + (off_t)add) : (off_t)limit;
		}
		#endregion
	}
}
