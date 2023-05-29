/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers
{
	/// <summary>
	/// The encoding byte values from ID3v2
	/// </summary>
	internal enum Mpg123_Id3_Enc : c_uchar
	{
		Latin1 = 0,						// < Note: This sometimes can mean anything in practice...
		Utf16Bom = 1,					// < UTF16, UCS-2 ... it's all the same for practical purposes
		Utf16Be = 2,					// < Big-endian UTF-16, BOM see note for mpg123_text_utf16be
		Utf8 = 3,						// < Our lovely overly ASCII-compatible 8 byte encoding for the world
		Enc_Max = 3						// < Placeholder to check valid range of encoding byte
	}
}
