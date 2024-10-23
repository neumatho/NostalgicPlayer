/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.OpusFile.Containers
{
	/// <summary>
	/// The metadata from an Ogg Opus stream.
	///
	/// This structure holds the in-stream metadata corresponding to the 'comment'
	/// header packet of an Ogg Opus stream.
	/// The comment header is meant to be used much like someone jotting a quick
	/// note on the label of a CD.
	/// It should be a short, to the point text note that can be more than a couple
	/// words, but not more than a short paragraph.
	///
	/// The metadata is stored as a series of (tag, value) pairs, in length-encoded
	/// string vectors, using the same format as Vorbis (without the final "framing
	/// bit"), Theora, and Speex, except for the packet header.
	/// The first occurrence of the '=' character delimits the tag and value.
	/// A particular tag may occur more than once, and order is significant.
	/// The character set encoding for the strings is always UTF-8, but the tag
	/// names are limited to ASCII, and treated as case-insensitive.
	/// See https://www.xiph.org/vorbis/doc/v-comment.html (the Vorbis
	/// comment header specification) for details.
	///
	/// In filling in this structure, libopusfile will null-terminate the
	/// #user_comments strings for safety.
	/// However, the bitstream format itself treats them as 8-bit clean vectors,
	/// possibly containing NUL characters, so the #comment_lengths array should be
	/// treated as their authoritative length.
	/// </summary>
	public class OpusTags
	{
		/// <summary>
		/// The array of comment string vectors
		/// </summary>
		public Pointer<Pointer<byte>> User_Comments;

		/// <summary>
		/// An array of the corresponding length of each vector, in bytes
		/// </summary>
		public Pointer<c_int> Comment_Lengths;

		/// <summary>
		/// The total number of comment streams
		/// </summary>
		public c_int Comments;

		/// <summary>
		/// The null-terminated vendor string.
		/// This identifies the software used to encode the stream
		/// </summary>
		public Pointer<byte> Vendor;
	}
}
