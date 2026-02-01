/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// These flags can be passed in AVCodecContext.flags before initialization.
	/// Note: Not everything is supported yet
	/// </summary>
	[Flags]
	public enum AvCodecFlag
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Allow decoders to produce frames with data planes that are not aligned
		/// to CPU requirements (e.g. due to cropping)
		/// </summary>
		Unaligned = 1 << 0,

		/// <summary>
		/// Use fixed qscale
		/// </summary>
		QScale = 1 << 1,

		/// <summary>
		/// 4 MV per MB allowed / advanced prediction for H.263
		/// </summary>
		_4Mv = 1 << 2,

		/// <summary>
		/// Output even those frames that might be corrupted
		/// </summary>
		Output_Corrupt = 1 << 3,

		/// <summary>
		/// Use qpel MC
		/// </summary>
		Qpel = 1 << 4,

		/// <summary>
		/// Request the encoder to output reconstructed frames, i.e.\ frames that would
		/// be produced by decoding the encoded bitstream. These frames may be retrieved
		/// by calling avcodec_receive_frame() immediately after a successful call to
		/// avcodec_receive_packet().
		///
		/// Should only be used with encoders flagged with the
		/// AV_CODEC_CAP_ENCODER_RECON_FRAME capability.
		///
		/// Note:
		/// Each reconstructed frame returned by the encoder corresponds to the last
		/// encoded packet, i.e. the frames are returned in coded order rather than
		/// presentation order.
		///
		/// Note:
		/// Frame parameters (like pixel format or dimensions) do not have to match the
		/// AVCodecContext values. Make sure to use the values from the returned frame
		/// </summary>
		Recon_Frame = 1 << 6,

		/// <summary>
		/// Decoding:
		/// Request the decoder to propagate each packet's AVPacket.opaque and
		/// AVPacket.opaque_ref to its corresponding output AVFrame.
		///
		/// Encoding:
		/// Request the encoder to propagate each frame's AVFrame.opaque and
		/// AVFrame.opaque_ref values to its corresponding output AVPacket.
		///
		/// May only be set on encoders that have the
		/// AV_CODEC_CAP_ENCODER_REORDERED_OPAQUE capability flag.
		///
		/// Note:
		/// While in typical cases one input frame produces exactly one output packet
		/// (perhaps after a delay), in general the mapping of frames to packets is
		/// M-to-N, so
		/// - Any number of input frames may be associated with any given output packet.
		///   This includes zero - e.g. some encoders may output packets that carry only
		///   metadata about the whole stream.
		/// - A given input frame may be associated with any number of output packets.
		///   Again this includes zero - e.g. some encoders may drop frames under certain
		///   conditions.
		///
		/// This implies that when using this flag, the caller must NOT assume that
		/// - a given input frame's opaques will necessarily appear on some output packet;
		/// - every output packet will have some non-NULL opaque value.
		/// .
		/// When an output packet contains multiple frames, the opaque values will be
		/// taken from the first of those.
		///
		/// Note:
		/// The converse holds for decoders, with frames and packets switched
		/// </summary>
		Copy_Opaque = 1 << 7,

		/// <summary>
		/// Signal to the encoder that the values of AVFrame.duration are valid and
		/// should be used (typically for transferring them to output packets).
		///
		/// If this flag is not set, frame durations are ignored
		/// </summary>
		Frame_Duration = 1 << 8,

		/// <summary>
		/// Use internal 2pass ratecontrol in first pass mode
		/// </summary>
		Pass1 = 1 << 9,

		/// <summary>
		/// Use internal 2pass ratecontrol in second pass mode
		/// </summary>
		Pass2 = 1 << 10,

		/// <summary>
		/// Loop filter
		/// </summary>
		Loop_Filter = 1 << 11,

		/// <summary>
		/// Only decode/encode grayscale
		/// </summary>
		Gray = 1 << 13,

		/// <summary>
		/// Error[?] variables will be set during encoding
		/// </summary>
		Psnr = 1 << 15,

		/// <summary>
		/// Use interlaced DCT
		/// </summary>
		Interlaced_Dct = 1 << 18,

		/// <summary>
		/// Force low delay
		/// </summary>
		Low_Delay = 1 << 19,

		/// <summary>
		/// Place global headers in extradata instead of every keyframe
		/// </summary>
		Global_Header = 1 << 22,

		/// <summary>
		/// Use only bitexact stuff (except (I)DCT)
		/// </summary>
		BitExact = 1 << 23,

		/// <summary>
		/// H.263 advanced intra coding / MPEG-4 AC prediction
		/// </summary>
		Ac_Pred = 1 << 24,

		/// <summary>
		/// Interlaced motion estimation
		/// </summary>
		Interlaced_Me = 1 << 29,

		/// <summary>
		/// 
		/// </summary>
		Closed_Gop = 1 << 31,
	}
}
