/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// The encoding byte values from ID3v2
	/// </summary>
	public enum Mpg123_Id3_Enc : c_uchar
	{
		/// <summary>
		/// Note: This sometimes can mean anything in practice...
		/// </summary>
		Latin1 = 0,
		/// <summary>
		/// UTF16, UCS-2 ... it's all the same for practical purposes
		/// </summary>
		Utf16Bom = 1,
		/// <summary>
		/// Big-endian UTF-16, BOM see note for mpg123_text_utf16be
		/// </summary>
		Utf16Be = 2,
		/// <summary>
		/// Our lovely overly ASCII-compatible 8 byte encoding for the world
		/// </summary>
		Utf8 = 3,
		/// <summary>
		/// Placeholder to check valid range of encoding byte
		/// </summary>
		Enc_Max = 3
	}
}
