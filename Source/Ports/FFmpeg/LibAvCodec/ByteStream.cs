/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	public static class ByteStream
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint64_t ByteStream_Get_LE64(ref CPointer<uint8_t> b)//XX 91
		{
			b += 8;

			return IntReadWrite.Av_RL64(b - 8);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream_Put_LE64(ref CPointer<uint8_t> b, uint64_t value)
		{
			IntReadWrite.Av_WL64(b, value);

			b += 8;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream2_Put_LE64U(PutByteContext p, uint64_t value)
		{
			ByteStream_Put_LE64(ref p.Buffer, value);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream2_Put_LE64(PutByteContext p, uint64_t value)
		{
			if ((p.Eof == 0) && ((p.Buffer_End - p.Buffer) >= 8))
			{
				IntReadWrite.Av_WL64(p.Buffer, value);

				p.Buffer += 8;
			}
			else
				p.Eof = 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint64_t ByteStream2_Get_LE64U(GetByteContext g)
		{
			return ByteStream_Get_LE64(ref g.Buffer);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint64_t ByteStream2_Get_LE64(GetByteContext g)
		{
			if ((g.Buffer_End - g.Buffer) < 8)
			{
				g.Buffer = g.Buffer_End;

				return 0;
			}

			return ByteStream2_Get_LE64U(g);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint ByteStream_Get_LE32(ref CPointer<uint8_t> b)//XX 92
		{
			b += 4;

			return IntReadWrite.Av_RL32(b - 4);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream_Put_LE32(ref CPointer<uint8_t> b, c_uint value)
		{
			IntReadWrite.Av_WL32(b, value);

			b += 4;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream2_Put_LE32U(PutByteContext p, c_uint value)
		{
			ByteStream_Put_LE32(ref p.Buffer, value);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream2_Put_LE32(PutByteContext p, c_uint value)
		{
			if ((p.Eof == 0) && ((p.Buffer_End - p.Buffer) >= 4))
			{
				IntReadWrite.Av_WL32(p.Buffer, value);

				p.Buffer += 4;
			}
			else
				p.Eof = 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint ByteStream2_Get_LE32U(GetByteContext g)
		{
			return ByteStream_Get_LE32(ref g.Buffer);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint ByteStream2_Get_LE32(GetByteContext g)
		{
			if ((g.Buffer_End - g.Buffer) < 4)
			{
				g.Buffer = g.Buffer_End;

				return 0;
			}

			return ByteStream2_Get_LE32U(g);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint ByteStream_Get_LE16(ref CPointer<uint8_t> b)//XX 94
		{
			b += 2;

			return IntReadWrite.Av_RL16(b - 2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream_Put_LE16(ref CPointer<uint8_t> b, c_uint value)
		{
			IntReadWrite.Av_WL16(b, (uint16_t)value);

			b += 2;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream2_Put_LE16U(PutByteContext p, c_uint value)
		{
			ByteStream_Put_LE16(ref p.Buffer, value);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream2_Put_LE16(PutByteContext p, c_uint value)
		{
			if ((p.Eof == 0) && ((p.Buffer_End - p.Buffer) >= 2))
			{
				IntReadWrite.Av_WB32(p.Buffer, value);

				p.Buffer += 2;
			}
			else
				p.Eof = 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint ByteStream2_Get_LE16U(GetByteContext g)
		{
			return ByteStream_Get_LE16(ref g.Buffer);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint ByteStream2_Get_LE16(GetByteContext g)
		{
			if ((g.Buffer_End - g.Buffer) < 2)
			{
				g.Buffer = g.Buffer_End;

				return 0;
			}

			return ByteStream2_Get_LE16U(g);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint64_t ByteStream_Get_BE64(ref CPointer<uint8_t> b)//XX 95
		{
			b += 8;

			return IntReadWrite.Av_RB64(b - 8);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream_Put_BE64(ref CPointer<uint8_t> b, uint64_t value)
		{
			IntReadWrite.Av_WB64(b, value);

			b += 8;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream2_Put_BE64U(PutByteContext p, uint64_t value)
		{
			ByteStream_Put_BE64(ref p.Buffer, value);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream2_Put_BE64(PutByteContext p, uint64_t value)
		{
			if ((p.Eof == 0) && ((p.Buffer_End - p.Buffer) >= 8))
			{
				IntReadWrite.Av_WB64(p.Buffer, value);

				p.Buffer += 8;
			}
			else
				p.Eof = 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint64_t ByteStream2_Get_BE64U(GetByteContext g)
		{
			return ByteStream_Get_BE64(ref g.Buffer);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint64_t ByteStream2_Get_BE64(GetByteContext g)
		{
			if ((g.Buffer_End - g.Buffer) < 8)
			{
				g.Buffer = g.Buffer_End;

				return 0;
			}

			return ByteStream2_Get_BE64U(g);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint ByteStream_Get_BE32(ref CPointer<uint8_t> b)//XX 96
		{
			b += 4;

			return IntReadWrite.Av_RB32(b - 4);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream_Put_BE32(ref CPointer<uint8_t> b, c_uint value)
		{
			IntReadWrite.Av_WB32(b, value);

			b += 4;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream2_Put_BE32U(PutByteContext p, c_uint value)
		{
			ByteStream_Put_BE32(ref p.Buffer, value);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream2_Put_BE32(PutByteContext p, c_uint value)
		{
			if ((p.Eof == 0) && ((p.Buffer_End - p.Buffer) >= 4))
			{
				IntReadWrite.Av_WB32(p.Buffer, value);

				p.Buffer += 4;
			}
			else
				p.Eof = 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint ByteStream2_Get_BE32U(GetByteContext g)
		{
			return ByteStream_Get_BE32(ref g.Buffer);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint ByteStream2_Get_BE32(GetByteContext g)
		{
			if ((g.Buffer_End - g.Buffer) < 4)
			{
				g.Buffer = g.Buffer_End;

				return 0;
			}

			return ByteStream2_Get_BE32U(g);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint ByteStream_Get_BE16(ref CPointer<uint8_t> b)//XX 98
		{
			b += 2;

			return IntReadWrite.Av_RL16(b - 2);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream_Put_BE16(ref CPointer<uint8_t> b, c_uint value)
		{
			IntReadWrite.Av_WB16(b, (uint16_t)value);

			b += 2;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream2_Put_BE16U(PutByteContext p, c_uint value)
		{
			ByteStream_Put_BE16(ref p.Buffer, value);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream2_Put_BE16(PutByteContext p, c_uint value)
		{
			if ((p.Eof == 0) && ((p.Buffer_End - p.Buffer) >= 2))
			{
				IntReadWrite.Av_WB16(p.Buffer, (uint16_t)value);

				p.Buffer += 2;
			}
			else
				p.Eof = 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint ByteStream2_Get_BE16U(GetByteContext g)
		{
			return ByteStream_Get_BE16(ref g.Buffer);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint ByteStream2_Get_BE16(GetByteContext g)
		{
			if ((g.Buffer_End - g.Buffer) < 2)
			{
				g.Buffer = g.Buffer_End;

				return 0;
			}

			return ByteStream2_Get_BE16U(g);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream2_Init(out GetByteContext g, CPointer<uint8_t> buf, c_int buf_Size)//XX 137
		{
			g = new GetByteContext();

			g.Buffer = buf;
			g.Buffer_Start = buf;
			g.Buffer_End = buf + buf_Size;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ByteStream2_Init_Writer(out PutByteContext p, CPointer<uint8_t> buf, c_int buf_Size)//XX 147
		{
			p = new PutByteContext();

			p.Buffer = buf;
			p.Buffer_Start = buf;
			p.Buffer_End = buf + buf_Size;
			p.Eof = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int ByteStream2_Get_Bytes_Left(GetByteContext g)//XX 158
		{
			return g.Buffer_End - g.Buffer;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int ByteStream2_Tell(GetByteContext g)//XX 192
		{
			return g.Buffer - g.Buffer_Start;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int ByteStream2_Tell_P(PutByteContext p)//XX 197
		{
			return p.Buffer - p.Buffer_Start;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int ByteStream2_Seek(GetByteContext g, c_int offset, SeekOrigin whence)//XX 212
		{
			switch (whence)
			{
				case SeekOrigin.Current:
				{
					offset = Common.Av_Clip(offset, -(g.Buffer - g.Buffer_Start), g.Buffer_End - g.Buffer);
					g.Buffer += offset;
					break;
				}

				case SeekOrigin.End:
				{
					offset = Common.Av_Clip(offset, -(g.Buffer_End - g.Buffer_Start), 0);
					g.Buffer = g.Buffer_End + offset;
					break;
				}

				case SeekOrigin.Begin:
				{
					offset = Common.Av_Clip(offset, 0, g.Buffer_End - g.Buffer_Start);
					g.Buffer = g.Buffer_Start + offset;
					break;
				}

				default:
					return Error.EINVAL;
			}

			return ByteStream2_Tell(g);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int ByteStream2_Seek_P(PutByteContext p, c_int offset, SeekOrigin whence)//XX 236
		{
			p.Eof = 0;

			switch (whence)
			{
				case SeekOrigin.Current:
				{
					if ((p.Buffer_End - p.Buffer) < offset)
						p.Eof = 1;

					offset = Common.Av_Clip(offset, -(p.Buffer - p.Buffer_Start), p.Buffer_End - p.Buffer);
					p.Buffer += offset;
					break;
				}

				case SeekOrigin.End:
				{
					if (offset > 0)
						p.Eof = 1;

					offset = Common.Av_Clip(offset, -(p.Buffer_End - p.Buffer_Start), 0);
					p.Buffer = p.Buffer_End + offset;
					break;
				}

				case SeekOrigin.Begin:
				{
					if ((p.Buffer_End - p.Buffer_Start) < offset)
						p.Eof = 1;

					offset = Common.Av_Clip(offset, 0, p.Buffer_End - p.Buffer_Start);
					p.Buffer = p.Buffer_Start + offset;
					break;
				}

				default:
					return Error.EINVAL;
			}

			return ByteStream2_Tell_P(p);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint ByteStream2_Get_Buffer<T>(GetByteContext g, CPointer<T> dst, c_uint size) where T : unmanaged//XX 267
		{
			c_uint size2 = Macros.FFMin((c_uint)(g.Buffer_End - g.Buffer), size);
			CMemory.memcpy(dst.Cast<T, uint8_t>(), g.Buffer, size2);

			g.Buffer += size2;

			return size2;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint ByteStream2_Put_Buffer<T>(PutByteContext p, CPointer<T> src, c_uint size) where T : unmanaged//XX 286
		{
			if (p.Eof != 0)
				return 0;

			c_uint size2 = Macros.FFMin((c_uint)(p.Buffer_End - p.Buffer), size);

			if (size2 != size)
				p.Eof = 1;

			CMemory.memcpy(p.Buffer, src.Cast<T, uint8_t>(), size2);

			p.Buffer += size2;

			return size2;
		}
	}
}
