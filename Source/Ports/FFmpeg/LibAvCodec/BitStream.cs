/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	internal static class BitStream
	{
		/********************************************************************/
		/// <summary>
		/// Copy the content of src to the bitstream
		/// </summary>
		/********************************************************************/
		public static void FF_Copy_Bits(PutBitContext pb, CPointer<uint8_t> src, c_int length)
		{
			c_int words = length >> 4;
			c_int bits = length & 15;

			if (length == 0)
				return;

			if ((words < 16) || ((Put_Bits.Put_Bits_Count(pb) & 7) != 0))
			{
				for (c_int i = 0; i < words; i++)
					Put_Bits._Put_Bits(pb, 16, IntReadWrite.Av_RB16(src + (2 * i)));
			}
			else
			{
				c_int i;

				for (i = 0; (Put_Bits.Put_Bits_Count(pb) & 31) != 0; i++)
					Put_Bits._Put_Bits(pb, 8, src[i]);

				Put_Bits.Flush_Put_Bits(pb);
				CMemory.memcpy(Put_Bits.Put_Bits_Ptr(pb), src + i, (size_t)((2 * words) - i));
				Put_Bits.Skip_Put_Bytes(pb, (2 * words) - i);
			}

			Put_Bits._Put_Bits(pb, bits, IntReadWrite.Av_RB16(src + (2 * words)) >> (16 - bits));
		}
	}
}
