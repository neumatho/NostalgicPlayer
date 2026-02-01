/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal enum FFCodecType
	{
		/// <summary>
		/// The codec is a decoder using the decode callback;
		/// audio and video codecs only
		/// </summary>
		Decode,

		/// <summary>
		/// The codec is a decoder using the decode_sub callback;
		/// subtitle codecs only
		/// </summary>
		Decode_Sub,

		/// <summary>
		/// The codec is a decoder using the receive_frame callback;
		/// audio and video codecs only
		/// </summary>
		Receive_Frame,

		/// <summary>
		/// The codec is an encoder using the encode callback;
		/// audio and video codecs only
		/// </summary>
		Encode,

		/// <summary>
		/// The codec is an encoder using the encode_sub callback;
		/// subtitle codecs only
		/// </summary>
		Encode_Sub,

		/// <summary>
		/// The codec is an encoder using the receive_packet callback;
		/// audio and video codecs only
		/// </summary>
		Receive_Packet
	}
}
