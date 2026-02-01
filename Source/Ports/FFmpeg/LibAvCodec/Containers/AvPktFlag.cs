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
	public enum AvPktFlag
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// The packet contains a keyframe
		/// </summary>
		Key = 0x0001,

		/// <summary>
		/// The packet content is corrupted
		/// </summary>
		Corrupt = 0x0002,

		/// <summary>
		/// Flag is used to discard packets which are required to maintain valid
		/// decoder state but are not required for output and should be dropped
		/// after decoding
		/// </summary>
		Discard = 0x0004,

		/// <summary>
		/// The packet comes from a trusted source.
		///
		/// Otherwise-unsafe constructs such as arbitrary pointers to data
		/// outside the packet may be followed
		/// </summary>
		Trusted = 0x0008,

		/// <summary>
		/// Flag is used to indicate packets that contain frames that can
		/// be discarded by the decoder. I.e. Non-reference frames
		/// </summary>
		Disposable = 0x0010
	}
}
