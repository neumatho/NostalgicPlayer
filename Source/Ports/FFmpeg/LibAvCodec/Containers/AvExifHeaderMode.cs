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
	public enum AvExifHeaderMode
	{
		/// <summary>
		/// The TIFF header starts with 0x49492a00, or 0x4d4d002a.
		/// This one is used internally by FFmpeg
		/// </summary>
		Tiff_Header,

		/// <summary>
		/// Skip the TIFF header, assume little endian
		/// </summary>
		Assume_Le,

		/// <summary>
		/// Skip the TIFF header, assume big endian
		/// </summary>
		Assume_Be,

		/// <summary>
		/// The first four bytes point to the actual start, then it's AV_EXIF_TIFF_HEADER
		/// </summary>
		T_Off,

		/// <summary>
		/// The first six bytes contain "Exif\0\0", then it's AV_EXIF_TIFF_HEADER
		/// </summary>
		Exif00
	}
}
