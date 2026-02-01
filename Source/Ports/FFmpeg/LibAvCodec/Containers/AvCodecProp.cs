/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum AvCodecProp
	{
		/// <summary>
		/// Codec uses only intra compression.
		/// Video and audio codecs only
		/// </summary>
		Intra_Only = 1 << 0,

		/// <summary>
		/// Codec supports lossy compression. Audio and video codecs only.
		/// Note: a codec may support both lossy and lossless
		/// compression modes
		/// </summary>
		Lossy = 1 << 1,

		/// <summary>
		/// Codec supports lossless compression. Audio and video codecs only
		/// </summary>
		Lossless = 1 << 2,

		/// <summary>
		/// Codec supports frame reordering. That is, the coded order (the order in which
		/// the encoded packets are output by the encoders / stored / input to the
		/// decoders) may be different from the presentation order of the corresponding
		/// frames.
		///
		/// For codecs that do not have this property set, PTS and DTS should always be
		/// equal
		/// </summary>
		Reorder = 1 << 3,

		/// <summary>
		/// Video codec supports separate coding of fields in interlaced frames
		/// </summary>
		Fields = 1 << 4,

		/// <summary>
		/// Subtitle codec is bitmap based
		/// Decoded AVSubtitle data can be read from the AVSubtitleRect->pict field
		/// </summary>
		Bitmap_Sub = 1 << 16,

		/// <summary>
		/// Subtitle codec is text based.
		/// Decoded AVSubtitle data can be read from the AVSubtitleRect->ass field
		/// </summary>
		Text_Sub = 1 << 17,
	}
}
