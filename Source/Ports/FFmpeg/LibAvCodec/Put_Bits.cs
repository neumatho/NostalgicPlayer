/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	public static class Put_Bits
	{
		/********************************************************************/
		/// <summary>
		/// Initialize the PutBitContext
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Init_Put_Bits(PutBitContext s, CPointer<uint8_t> buffer, c_int buffer_Size)
		{
			if (buffer_Size < 0)
			{
				buffer_Size = 0;
				buffer.SetToNull();
			}

			s.Buf = buffer;
			s.Buf_End = s.Buf + buffer_Size;
			s.Buf_Ptr = s.Buf;
			s.Bit_Left = CodecConstants.Buf_Bits;
			s.Bit_Buf = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Return the total number of bits written to the bitstream
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Put_Bits_Count(PutBitContext s)
		{
			return ((s.Buf_Ptr - s.Buf) * 8) + CodecConstants.Buf_Bits - s.Bit_Left;
		}



		/********************************************************************/
		/// <summary>
		/// Pad the end of the output stream with zeroes
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Flush_Put_Bits(PutBitContext s)
		{
			if (s.Bit_Left < CodecConstants.Buf_Bits)
				s.Bit_Buf <<= s.Bit_Left;

			while (s.Bit_Left < CodecConstants.Buf_Bits)
			{
				s.Buf_Ptr[0, 1] = (uint8_t)(s.Bit_Buf >> (CodecConstants.Buf_Bits - 8));
				s.Bit_Buf <<= 8;
				s.Bit_Left += 8;
			}

			s.Bit_Left = CodecConstants.Buf_Bits;
			s.Bit_Buf = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Write up to 31 bits into a bitstream.
		/// Use put_bits32 to write 32 bits
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void _Put_Bits(PutBitContext s, c_int n, BitBuf value)
		{
			Put_Bits_No_Assert(s, n, value);
		}



		/********************************************************************/
		/// <summary>
		/// Return the pointer to the byte where the bitstream writer will
		/// put the next bit
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<uint8_t> Put_Bits_Ptr(PutBitContext s)
		{
			return s.Buf_Ptr;
		}



		/********************************************************************/
		/// <summary>
		/// Skip the given number of bytes.
		/// PutBitContext must be flushed ＆ aligned to a byte boundary
		/// before calling this
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Skip_Put_Bytes(PutBitContext s, c_int n)
		{
			s.Buf_Ptr += n;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Put_Bits_No_Assert(PutBitContext s, c_int n, BitBuf value)
		{
			BitBuf bit_Buf = s.Bit_Buf;
			c_int bit_Left = s.Bit_Left;

			if (n < bit_Left)
			{
				bit_Buf = (bit_Buf << n) | value;
				bit_Left -= n;
			}
			else
			{
				bit_Buf <<= bit_Left;
				bit_Buf |= value >> (n - bit_Left);

				if ((s.Buf_End - s.Buf_Ptr) >= sizeof(BitBuf))
				{
					IntReadWrite.Av_WB64(s.Buf_Ptr, bit_Buf);
					s.Buf_Ptr += sizeof(BitBuf);
				}
				else
					Log.Av_Log(null, Log.Av_Log_Error, "Internal error, put_bits buffer too small\n");

				bit_Left += CodecConstants.Buf_Bits - n;
				bit_Buf = value;
			}

			s.Bit_Buf = bit_Buf;
			s.Bit_Left = bit_Left;
		}
		#endregion
	}
}
