/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec
{
	/// <summary>
	/// Common code shared by all WMA variants
	/// </summary>
	internal static class Wma_Common
	{
		/********************************************************************/
		/// <summary>
		/// Get the samples per frame for this stream
		/// </summary>
		/********************************************************************/
		public static c_int FF_Wma_Get_Frame_Len_Bits(c_int sample_Rate, c_int version, c_uint decode_Flags)//XX 32
		{
			c_int frame_Len_Bits;

			if (sample_Rate <= 16000)
				frame_Len_Bits = 9;
			else if ((sample_Rate <= 22050) || ((sample_Rate <= 32000) && (version == 1)))
				frame_Len_Bits = 10;
			else if ((sample_Rate <= 48000) || (version < 3))
				frame_Len_Bits = 11;
			else if (sample_Rate <= 96000)
				frame_Len_Bits = 12;
			else
				frame_Len_Bits = 13;

			if (version == 3)
			{
				c_int tmp = (c_int)(decode_Flags & 0x6);

				if (tmp == 0x2)
					++frame_Len_Bits;
				else if (tmp == 0x4)
					--frame_Len_Bits;
				else if (tmp == 0x6)
					frame_Len_Bits -= 2;
			}

			return frame_Len_Bits;
		}
	}
}
