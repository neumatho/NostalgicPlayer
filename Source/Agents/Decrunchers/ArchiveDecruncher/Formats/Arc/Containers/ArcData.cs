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
	internal class ArcData
	{
		// RLE90
		public int Rle_In { get; set; }
		public int Rle_Out { get; set; }
		public bool In_Rle_Code { get; set; }
		public int Last_Byte { get; set; }

		// LZW and Huffman
		public uint[] Codes_Buffered { get; } = new uint[8];
		public uint Buffered_Pos { get; set; }
		public uint Buffered_Width { get; set; }
		public uint Lzw_Bits_In { get; set; }
		public uint Lzw_In { get; set; }
		public uint Lzw_Out { get; set; }
		public bool Lzw_Eof { get; set; }
		public uint Max_Code { get; set; }
		public uint First_Code { get; set; }
		public uint Next_Code { get; set; }
		public uint Current_Width { get; set; }
		public uint Init_Width { get; set; }
		public uint Max_Width { get; set; }
		public uint Continue_Left { get; set; }
		public uint Continue_Code { get; set; }
		public uint Last_Code { get; set; }
		public bool KwKwK { get; set; }
		public uint Last_First_Value { get; set; }

		public byte[] Window { get; set; }
		public ArcCode[] Tree { get; set; }
		public ArcLookup[] Huffman_Lookup { get; set; }
		public ArcHuffmanIndex[] Huffman_Tree { get; set; }
		public uint Num_Huffman { get; set; }
	}
}
