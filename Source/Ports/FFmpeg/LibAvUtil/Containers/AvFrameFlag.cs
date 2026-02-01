/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Flags describing additional frame properties
	/// </summary>
	[Flags]
	public enum AvFrameFlag
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// The frame data may be corrupted, e.g. due to decoding errors
		/// </summary>
		Corrupt = 1 << 0,

		/// <summary>
		/// A flag to mark frames that are keyframes
		/// </summary>
		Key = 1 << 1,

		/// <summary>
		/// A flag to mark the frames which need to be decoded, but shouldn't be output
		/// </summary>
		Discard = 1 << 2,

		/// <summary>
		/// A flag to mark frames whose content is interlaced
		/// </summary>
		Interlaced = 1 << 3,

		/// <summary>
		/// A flag to mark frames where the top field is displayed first if the content
		/// is interlaced
		/// </summary>
		Top_Field_First = 1 << 4,

		/// <summary>
		/// A decoder can use this flag to mark frames which were originally encoded losslessly.
		///
		/// For coding bitstream formats which support both lossless and lossy
		/// encoding, it is sometimes possible for a decoder to determine which method
		/// was used when the bitsream was encoded
		/// </summary>
		Lossless = 1 << 5
	}
}
