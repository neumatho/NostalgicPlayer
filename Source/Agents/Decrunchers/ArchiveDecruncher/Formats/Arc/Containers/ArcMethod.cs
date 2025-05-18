/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Arc.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal enum ArcMethod
	{
		Unpacked_Old = 0x01,
		Unpacked = 0x02,
		Packed = 0x03,			// RLE90
		Squeezed = 0x04,		// RLE90 + Huffman coding
		Crunched_5 = 0x05,		// LZW 12-bit static (old hash)
		Crunched_6 = 0x06,		// RLE90 + LZW 12-bit static (old hash)
		Crunched_7 = 0x07,		// RLE90 + LZW 12-bit static (new hash)
		Crunched = 0x08,		// RLE90 + LZW 9-12 bit dynamic
		Squashed = 0x09,		// LZW 9-13 bit dynamic (PK extension)
		Trimmed = 0x0a,			// RLE90 + LZW with adaptive Huffman coding
		Compressed = 0x7f,		// LZW 9-16 bit dynamic (Spark extension)
	}
}
