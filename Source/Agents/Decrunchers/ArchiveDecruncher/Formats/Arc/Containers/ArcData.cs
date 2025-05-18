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
		public int Rle_In;
		public int Rle_Out;
		public bool In_Rle_Code;
		public int Last_Byte;

		// LZW and Huffman
		public readonly uint[] Codes_Buffered = new uint[8];
		public uint Buffered_Pos;
		public uint Buffered_Width;
		public uint Lzw_Bits_In;
		public uint Lzw_In;
		public uint Lzw_Out;
		public bool Lzw_Eof;
		public uint Max_Code;
		public uint First_Code;
		public uint Next_Code;
		public uint Current_Width;
		public uint Init_Width;
		public uint Max_Width;
		public uint Continue_Left;
		public uint Continue_Code;
		public uint Last_Code;
		public bool KwKwK;
		public uint Last_First_Value;

		public byte[] Window;
		public ArcCode[] Tree;
		public ArcLookup[] Huffman_Lookup;
		public ArcHuffmanIndex[] Huffman_Tree;
		public uint Num_Huffman;
	}
}
