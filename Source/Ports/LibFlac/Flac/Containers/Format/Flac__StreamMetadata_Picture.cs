/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// FLAC PICTURE structure
	/// </summary>
	public class Flac__StreamMetadata_Picture : IMetadata
	{
		/// <summary>
		/// The kind of picture stored
		/// </summary>
		public Flac__StreamMetadata_Picture_Type Type;

		/// <summary>
		/// Picture data's MIME type, in ASCII printable characters
		/// 0x20-0x7e, NUL terminated. For best compatibility with players,
		/// use picture data of MIME type image/jpeg or image/png. A
		/// MIME type of '-->' is also allowed, in which case the picture
		/// data should be a complete URL. In file storage, the MIME type is
		/// stored as a 32-bit length followed by the ASCII string with no NUL
		/// terminator, but is converted to a plain C string in this structure
		/// for convenience
		/// </summary>
		public byte[] Mime_Type;

		/// <summary>
		/// Picture's description in UTF-8, NUL terminated. In file storage,
		/// the description is stored as a 32-bit length followed by the UTF-8
		/// string with no NUL terminator, but is converted to a plain C string
		/// in this structure for convenience
		/// </summary>
		public Flac__byte[] Description;

		/// <summary>
		/// Picture's width in pixels
		/// </summary>
		public Flac__uint32 Width;

		/// <summary>
		/// Picture's height in pixels
		/// </summary>
		public Flac__uint32 Height;

		/// <summary>
		/// Picture's color depth in bits-per-pixel
		/// </summary>
		public Flac__uint32 Depth;

		/// <summary>
		/// For indexed palettes (like GIF), picture's number of colors (the
		/// number of palette entries), or 0 for non-indexed (i.e. 2^Depth)
		/// </summary>
		public Flac__uint32 Colors;

		/// <summary>
		/// Length of binary picture data in bytes
		/// </summary>
		public Flac__uint32 Data_Length;

		/// <summary>
		/// Binary picture data
		/// </summary>
		public Flac__byte[] Data;
	}
}
