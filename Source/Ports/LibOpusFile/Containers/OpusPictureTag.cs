/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibOpusFile.Containers
{
	/// <summary>
	/// The contents of a METADATA_BLOCK_PICTURE tag
	/// </summary>
	public class OpusPictureTag
	{
		/// <summary>
		/// The picture type according to the ID3v2 APIC frame.
		/// </summary>
		public PictureType Type;

		/// <summary>
		/// The MIME type of the picture, in printable ASCII characters 0x20-0x7E.
		/// The MIME type may also be "-->" to signify that the data part
		/// is a URL pointing to the picture instead of the picture data itself.
		/// In this case, a terminating NUL is appended to the URL string in #data,
		/// but #data_length is set to the length of the string excluding that
		/// terminating NUL
		/// </summary>
		public CPointer<byte> Mime_Type;

		/// <summary>
		/// The description of the picture, in UTF-8
		/// </summary>
		public CPointer<byte> Description;

		/// <summary>
		/// The width of the picture in pixels
		/// </summary>
		public opus_uint32 Width;

		/// <summary>
		/// The height of the picture in pixels
		/// </summary>
		public opus_uint32 Height;

		/// <summary>
		/// The color depth of the picture in bits-per-pixel (not
		/// bits-per-channel)
		/// </summary>
		public opus_uint32 Depth;

		/// <summary>
		/// For indexed-color pictures (e.g., GIF), the number of colors used, or 0
		/// for non-indexed pictures
		/// </summary>
		public opus_uint32 Colors;

		/// <summary>
		/// The length of the picture data in bytes
		/// </summary>
		public opus_uint32 Data_Length;

		/// <summary>
		/// The binary picture data
		/// </summary>
		public CPointer<byte> Data;

		/// <summary>
		/// The format of the picture data, if known
		/// </summary>
		public PictureFormat Format;
	}
}
