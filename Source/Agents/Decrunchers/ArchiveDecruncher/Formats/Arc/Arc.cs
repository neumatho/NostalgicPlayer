/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Arc.Containers;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Arc
{
	/// <summary>
	/// Arc decruncher routines
	/// </summary>
	internal static class Arc
	{
		// ARC method 0x08: Read maximum code width from stream, but ignore it
		private const int Arc_Ignore_Code_In_Stream = 0x7ffe;

		// Spark method 0xff: Read maximum code width from stream
		private const int Arc_Max_Code_In_Stream = 0x7fff;

		private const uint Arc_No_Code = 0xffffffff;
		private const int Arc_Reset_Code = 256;
		private const int Arc_Buffer_Size = 8192;		// Buffer size for multi-stage compression

		private const int Lookup_Bits = 11;
		private const int Lookup_Mask = ((1 << Lookup_Bits) - 1);
		private const int Huffman_Tree_Max = 256;

		/********************************************************************/
		/// <summary>
		/// Unpack a buffer containing an ARC/ArcFS/Spark compressed stream
		/// into an uncompressed representation of the stream. The unpacked
		/// methods should be handled separately from this function since
		/// they don't need a second output buffer for the uncompressed
		/// data
		/// </summary>
		/********************************************************************/
		public static string Unpack(byte[] dest, int dest_Len, byte[] src, int src_Len, int method, int max_Width)
		{
			switch ((ArcMethod)(method & 0x7f))
			{
				case ArcMethod.Unpacked_Old:
				case ArcMethod.Unpacked:
				{
					Array.Copy(src, dest, Math.Min(src_Len, dest_Len));
					break;
				}

				// RLE90
				case ArcMethod.Packed:
				{
					if (Unpack_Rle90(dest, dest_Len, src, src_Len) < 0)
						return "failed unpack";

					break;
				}

				// RLE90 + Huffman coding
				case ArcMethod.Squeezed:
				{
					if (Unpack_Huffman_Rle90(dest, dest_Len, src, src_Len) < 0)
						return "failed unsqueeze";

					break;
				}

				// RLE90 + LZW 9-12 bit dynamic
				case ArcMethod.Crunched:
				{
					if (max_Width > 16)
						return "invalid uncrunch width";

					if (max_Width <= 0)
						max_Width = Arc_Ignore_Code_In_Stream;

					if (Unpack_Lzw_Rle90(dest, dest_Len, src, src_Len, 9, max_Width) != 0)
						return "failed uncrunch";

					break;
				}

				// LZW 9-13 bit dynamic
				case ArcMethod.Squashed:
				{
					if (Unpack_Lzw(dest, dest_Len, src, src_Len, 9, 13) != 0)
						return "failed unsquash";

					break;
				}

				// LZW 9-16 bit dynamic
				case ArcMethod.Compressed:
				{
					if (max_Width > 16)
						return "invalid uncompress width";

					if (max_Width <= 0)
						max_Width = Arc_Max_Code_In_Stream;

					if (Unpack_Lzw(dest, dest_Len, src, src_Len, 9, max_Width) != 0)
						return "failed uncompress";

					break;
				}

				default:
					return "unsupported method";
			}

			return null;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int Unpack_Init(ArcData arc, int init_Width, int max_Width, bool is_Dynamic)
		{
			arc.Rle_Out = 0;
			arc.Rle_In = 0;
			arc.In_Rle_Code = false;
			arc.Last_Byte = 0;

			arc.Buffered_Pos = 0;
			arc.Buffered_Width = 0;
			arc.Lzw_Bits_In = 0;
			arc.Lzw_In = 0;
			arc.Lzw_Out = 0;
			arc.Lzw_Eof = false;
			arc.Max_Code = (1U << max_Width);
			arc.First_Code = is_Dynamic ? 257U : 256U;
			arc.Current_Width = (uint)init_Width;
			arc.Init_Width = (uint)init_Width;
			arc.Max_Width = (uint)max_Width;
			arc.Continue_Left = 0;
			arc.Continue_Code = 0;
			arc.Last_Code = Arc_No_Code;
			arc.Last_First_Value = 0;
			arc.KwKwK = false;
			arc.Window = null;
			arc.Tree = null;
			arc.Huffman_Lookup = null;
			arc.Huffman_Tree = null;
			arc.Num_Huffman = 0;

			if (max_Width != 0)
			{
				if ((max_Width < 9) || (max_Width > 16))
					return -1;

				arc.Tree = ArrayHelper.InitializeArray<ArcCode>(1 << max_Width);

				for (int i = 0; i < 256; i++)
				{
					ArcCode c = arc.Tree[i];

					c.Length = 1;
					c.Value = (byte)i;
				}

				arc.Next_Code = arc.First_Code;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int Unpack_Window(ArcData arc, int window_Size)
		{
			arc.Window = new byte[window_Size];

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Unpack_Free(ArcData arc)
		{
			arc.Window = null;
			arc.Tree = null;
			arc.Huffman_Lookup = null;
			arc.Huffman_Tree = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static uint Get_Bytes(byte[] buffer, uint pos, int num)
		{
			switch (num)
			{
				case 0:
					return 0;

				case 1:
					return buffer[pos];

				case 2:
					return (uint)(buffer[pos] | (buffer[pos + 1] << 8));

				case 3:
					return (uint)(buffer[pos] | (buffer[pos + 1] << 8) | (buffer[pos + 2] << 16));

				default:
					return (uint)(buffer[pos] | (buffer[pos + 1] << 8) | (buffer[pos + 2] << 16) | (buffer[pos + 3] << 24));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int Read_Bits(ArcData arc, byte[] src, int src_Offset, int src_Len, uint num_Bits)
		{
			if ((arc.Lzw_Bits_In + num_Bits) > (src_Len << 3))
			{
				arc.Lzw_Bits_In = (uint)(src_Len << 3);
				arc.Lzw_In = (uint)src_Len;

				return -1;
			}

			uint ret = Get_Bytes(src, (uint)(src_Offset + arc.Lzw_In), (int)(src_Len - arc.Lzw_In));
			ret = (ret >> (int)(arc.Lzw_Bits_In & 7)) & (0xffffU << (int)num_Bits >> 16);

			arc.Lzw_Bits_In += num_Bits;
			arc.Lzw_In = arc.Lzw_Bits_In >> 3;

			return (int)ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static uint Next_Code(ArcData arc, byte[] src, int src_Offset, int src_Len)
		{
			// Codes are read 8 at a time in the original ARC/ArcFS/Spark software,
			// presumably to simplify file IO. This buffer needs to be simulated.
			// 
			// When the code width changes, the extra buffered codes are discarded.
			// Despite this, the final number of codes won't always be a multiple of 8
			if ((arc.Buffered_Pos >= 8) || (arc.Buffered_Width != arc.Current_Width))
			{
				uint i;

				for (i = 0; i < 8; i++)
				{
					int value = Read_Bits(arc, src, src_Offset, src_Len, arc.Current_Width);
					if (value < 0)
						break;

					arc.Codes_Buffered[i] = (uint)value;
				}

				for (; i < 8; i++)
					arc.Codes_Buffered[i] = Arc_No_Code;

				arc.Buffered_Pos = 0;
				arc.Buffered_Width = arc.Current_Width;
			}

			return arc.Codes_Buffered[arc.Buffered_Pos++];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void UnLzw_Add(ArcData arc)
		{
			if ((arc.Last_Code != Arc_No_Code) && (arc.Next_Code < arc.Max_Code))
			{
				uint len = arc.Tree[arc.Last_Code].Length;

				ArcCode e = arc.Tree[arc.Next_Code++];
				e.Prev = (ushort)arc.Last_Code;
				e.Length = (ushort)(len != 0 ? len + 1 : 0);
				e.Value = (byte)arc.Last_First_Value;

				// Automatically expand width
				if ((arc.Next_Code >= (1U << (int)arc.Current_Width)) && (arc.Current_Width < arc.Max_Width))
					arc.Current_Width++;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int UnLzw_Get_Length(ArcData arc, ArcCode e)
		{
			uint length = 1;
			int code;

			if (e.Length != 0)
				return e.Length;

			do
			{
				if (length >= arc.Max_Code)
					return 0;

				length++;
				code = e.Prev;
				e = arc.Tree[code];
			}
			while (code > 256);

			return (int)length;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int UnLzw_Block(ArcData arc, byte[] dest, int dest_Len, byte[] src, int src_Offset, int src_Len)
		{
			while (arc.Lzw_Out < dest_Len)
			{
				uint code;
				bool set_Last_First;

				// Interrupted while writing out code? Resume output...
				if (arc.Continue_Code != 0)
				{
					code = arc.Continue_Code;
					set_Last_First = false;
					goto Continue_Code;
				}

				code = Next_Code(arc, src, src_Offset, src_Len);

				if (code >= arc.Max_Code)
				{
					arc.Lzw_Eof = true;
					break;
				}

				if ((code == Arc_Reset_Code) && (arc.First_Code == 257))
				{
					arc.Next_Code = arc.First_Code;
					arc.Current_Width = arc.Init_Width;
					arc.Last_Code = Arc_No_Code;

					for (int i = 256; i < arc.Max_Code; i++)
						arc.Tree[i].Length = 0;

					continue;
				}

				// Add next code first to avoid KwKwK problem
				if (code == arc.Next_Code)
				{
					UnLzw_Add(arc);
					arc.KwKwK = true;
				}

				// Emit code
				set_Last_First = true;

				Continue_Code:
				ushort start_Code = (ushort)code;
				ArcCode e = arc.Tree[code];
				int len;

				if (arc.Continue_Code == 0)
				{
					len = UnLzw_Get_Length(arc, e);

					if (len == 0)
						return -1;
				}
				else
					len = (int)arc.Continue_Left;

				if ((uint)len > (dest_Len - arc.Lzw_Out))
				{
					// Calculate arc->continue_left, skip arc->continue_left,
					// emit remaining len from end of dest
					int num_Emit = (int)(dest_Len - arc.Lzw_Out);

					arc.Continue_Left = (uint)(len - num_Emit);
					arc.Continue_Code = code;

					for (; len > num_Emit; len--)
						e = arc.Tree[e.Prev];
				}
				else
					arc.Continue_Code = 0;

				int pos = (int)(arc.Lzw_Out + len - 1);
				arc.Lzw_Out += (uint)len;

				for (; len > 0; len--)
				{
					code = e.Value;
					dest[pos--] = (byte)code;
					e = arc.Tree[e.Prev];
				}

				// Only set this if this is the tail end of the chain,
				// i.e. the first section written
				if (set_Last_First)
					arc.Last_First_Value = code;

				if (arc.Continue_Code != 0)
					return 0;

				if (!arc.KwKwK)
					UnLzw_Add(arc);

				arc.Last_Code = start_Code;
				arc.KwKwK = false;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int UnRle90_Block(ArcData arc, byte[] dest, int dest_Len, byte[] src, int src_Len)
		{
			int i;

			for (i = 0; i < src_Len;)
			{
				if (arc.In_Rle_Code)
				{
					arc.In_Rle_Code = false;

					if (i >= src_Len)
						return -1;

					if (src[i] == 0)
					{
						if (arc.Rle_Out >= dest_Len)
							return -1;

						dest[arc.Rle_Out++] = 0x90;
						arc.Last_Byte = 0x90;
					}
					else
					{
						int len = src[i] - 1;

						if ((arc.Rle_Out + len) > dest_Len)
							return -1;

						Array.Fill(dest, (byte)arc.Last_Byte, arc.Rle_Out, len);
						arc.Rle_Out += len;
					}

					i++;
				}

				int start = i;

				while ((i < src_Len) && (src[i] != 0x90))
					i++;

				if (i > start)
				{
					int len = i - start;

					if ((len + arc.Rle_Out) > dest_Len)
					{
						// In some uncommon cases, ArcFS seems to output extra data beyond the
						// expected end of the file when unpacking crunched files. In the few
						// that have CRCs, ignoring the extra data still passes the check
						len = dest_Len - arc.Rle_Out;
						if (len == 0)
							break;
					}

					Array.Copy(src, start, dest, arc.Rle_Out, len);
					arc.Rle_Out += len;
					arc.Last_Byte = src[i - 1];
				}

				if ((i < src_Len) && (src[i] == 0x90))
				{
					arc.In_Rle_Code = true;
					i++;
				}
			}

			arc.Rle_In = i;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int Unpack_Rle90(byte[] dest, int dest_Len, byte[] src, int src_Len)
		{
			ArcData arc = new ArcData();

			if (Unpack_Init(arc, 0, 0, false) != 0)
				return -1;

			if (UnRle90_Block(arc, dest, dest_Len, src, src_Len) != 0)
				goto Err;

			if (arc.Rle_Out != dest_Len)
				goto Err;

			Unpack_Free(arc);

			return 0;

			Err:
			Unpack_Free(arc);

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int Unpack_Lzw(byte[] dest, int dest_Len, byte[] src, int src_Len, int init_Width, int max_Width)
		{
			ArcData arc = new ArcData();
			bool isDynamic = init_Width != max_Width;
			int src_Offset = 0;

			// This is only used for Spark method 0xff, which doesn't use RLE
			if (max_Width == Arc_Max_Code_In_Stream)
			{
				if (src_Len < 2)
					return -1;

				max_Width = src[src_Offset];
				src_Offset++;
				src_Len--;

				if ((max_Width < 9) || (max_Width > 16))
					return -1;
			}

			if (Unpack_Init(arc, init_Width, max_Width, isDynamic) != 0)
				return -1;

			if (UnLzw_Block(arc, dest, dest_Len, src, src_Offset, src_Len) != 0)
				goto Err;

			if (arc.Lzw_Out != dest_Len)
				goto Err;

			Unpack_Free(arc);

			return 0;

			Err:
			Unpack_Free(arc);

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int Unpack_Lzw_Rle90(byte[] dest, int dest_Len, byte[] src, int src_Len, int init_Width, int max_Width)
		{
			ArcData arc = new ArcData();
			bool isDynamic = init_Width != max_Width;
			int src_Offset = 0;

			// This is only used for Spark method 0xff, which doesn't use RLE
			if (max_Width == Arc_Max_Code_In_Stream)
				return -1;

			if (max_Width == Arc_Ignore_Code_In_Stream)
			{
				if (src_Len < 2)
					return -1;

				src_Offset++;
				src_Len--;
				max_Width = 12;
			}

			if ((max_Width < 9) || (max_Width > 16))
				return -1;

			if (Unpack_Init(arc, init_Width, max_Width, isDynamic) != 0)
				return -1;

			if (Unpack_Window(arc, Arc_Buffer_Size) != 0)
				goto Err;

			while (!arc.Lzw_Eof)
			{
				arc.Lzw_Out = 0;

				if (UnLzw_Block(arc, arc.Window, Arc_Buffer_Size, src, src_Offset, src_Len) != 0)
					goto Err;

				if (UnRle90_Block(arc, dest, dest_Len, arc.Window, (int)arc.Lzw_Out) != 0)
					goto Err;
			}

			if (arc.Rle_Out != dest_Len)
				goto Err;

			Unpack_Free(arc);

			return 0;

			Err:
			Unpack_Free(arc);

			return -1;
		}

		// Huffman decoding based on this blog post by Phaeron.
		// https://www.virtualdub.org/blog2/entry_345.html

		/********************************************************************/
		/// <summary>
		/// Make sure the tree isn't garbage
		/// </summary>
		/********************************************************************/
		private static int Huffman_Check_Tree(ArcHuffmanIndex[] tree)
		{
			bool[] visited = new bool[Huffman_Tree_Max];
			byte[] stack = new byte[Huffman_Tree_Max];
			int stack_Pos = 1;

			stack[0] = 0;

			while (stack_Pos > 0)
			{
				int i = stack[--stack_Pos];
				ArcHuffmanIndex e = tree[i];
				visited[i] = true;

				if (e.Value[0] >= 0)
				{
					if (visited[e.Value[0]])
						return -1;

					stack[stack_Pos++] = (byte)e.Value[0];
				}

				if (e.Value[1] >= 0)
				{
					if (visited[e.Value[1]])
						return -1;

					stack[stack_Pos++] = (byte)e.Value[1];
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int Huffman_Init(ArcData arc, byte[] src, int src_Len)
		{
			int table_Size = 1 << Lookup_Bits;

			if (src_Len < 2)
				return -1;

			arc.Num_Huffman = (uint)(src[0] | (src[1] << 8));

			if ((arc.Num_Huffman == 0) || (arc.Num_Huffman > Huffman_Tree_Max))
				return -1;

			arc.Lzw_In = 2 + 4 * arc.Num_Huffman;
			arc.Lzw_Bits_In = (arc.Lzw_In << 3);

			if (arc.Lzw_In > src_Len)
				return -1;

			// Precompute Huffman tree and lookup table
			arc.Huffman_Lookup = ArrayHelper.InitializeArray<ArcLookup>(table_Size);
			arc.Huffman_Tree = ArrayHelper.InitializeArray<ArcHuffmanIndex>((int)arc.Num_Huffman);

			for (uint i = 0; i < arc.Num_Huffman; i++)
			{
				ArcHuffmanIndex e = arc.Huffman_Tree[i];

				e.Value[0] = (short)(src[i * 4 + 2] | (src[i * 4 + 3] << 8));
				e.Value[1] = (short)(src[i * 4 + 4] | (src[i * 4 + 5] << 8));

				if ((e.Value[0] >= (int)arc.Num_Huffman) || (e.Value[1] >= (int)arc.Num_Huffman))
					return -1;
			}

			if (Huffman_Check_Tree(arc.Huffman_Tree) < 0)
				return -1;

			for (int i = 0; i < table_Size; i++)
			{
				int index = 0;
				int value = i;
				int bits;

				if (arc.Huffman_Lookup[i].Length != 0)
					continue;

				for (bits = 0; (index >= 0) && (bits < Lookup_Bits); bits++)
				{
					index = arc.Huffman_Tree[index].Value[value & 1];
					value >>= 1;
				}

				if (index >= 0)
				{
					arc.Huffman_Lookup[i].Value = (ushort)index;
					continue;
				}

				int iter = 1 << bits;

				for (int j = i; j < table_Size; j += iter)
				{
					arc.Huffman_Lookup[j].Value = (ushort)~index;
					arc.Huffman_Lookup[j].Length = (byte)bits;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int Huffman_Read_Bits(ArcData arc, byte[] src, int src_Len)
		{
			ArcHuffmanIndex[] tree = arc.Huffman_Tree;

			if (arc.Lzw_In >= src_Len)
				return -1;

			// Optimize short values with precomputed table
			uint peek = Get_Bytes(src, arc.Lzw_In, (int)(src_Len - arc.Lzw_In)) >> (int)(arc.Lzw_Bits_In & 7);

			ArcLookup e = arc.Huffman_Lookup[peek & Lookup_Mask];

			if (e.Length != 0)
			{
				arc.Lzw_Bits_In += e.Length;
				arc.Lzw_In = arc.Lzw_Bits_In >> 3;

				return e.Value;
			}

			// The table also allows skipping the first few bits of long codes
			uint bits_End = (uint)(src_Len << 3);
			arc.Lzw_Bits_In += Lookup_Bits;
			int index = e.Value;

			while ((index >= 0) && (arc.Lzw_Bits_In < bits_End))
			{
				// Force unsigned here to avoid potential sign extension
				uint bit = (uint)(src[arc.Lzw_Bits_In >> 3] >> (int)(arc.Lzw_Bits_In & 7));
				arc.Lzw_Bits_In++;

				index = tree[index].Value[bit & 1];
			}

			arc.Lzw_In = arc.Lzw_Bits_In >> 3;

			// This translates truncated code indices to negative
			// values (i.e. failure), no check required
			return ~index;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int UnHuffman_Block(ArcData arc, byte[] dest, int dest_Len, byte[] src, int src_Len)
		{
			while (arc.Lzw_Out < dest_Len)
			{
				int value = Huffman_Read_Bits(arc, src, src_Len);

				if (value >= 256)
				{
					// End of stream code
					arc.Lzw_In = (uint)src_Len;
					arc.Lzw_Eof = true;

					return 0;
				}

				if (value < 0)
					return -1;

				dest[arc.Lzw_Out++] = (byte)value;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int Unpack_Huffman_Rle90(byte[] dest, int dest_Len, byte[] src, int src_Len)
		{
			ArcData arc = new ArcData();

			if (Unpack_Init(arc, 0, 0, false) != 0)
				return -1;

			if (Unpack_Window(arc, Arc_Buffer_Size) != 0)
				goto Err;

			if (Huffman_Init(arc, src, src_Len) != 0)
				goto Err;

			while (!arc.Lzw_Eof)
			{
				arc.Lzw_Out = 0;

				if (UnHuffman_Block(arc, arc.Window, Arc_Buffer_Size, src, src_Len) != 0)
					goto Err;

				if (UnRle90_Block(arc, dest, dest_Len, arc.Window, (int)arc.Lzw_Out) != 0)
					goto Err;
			}

			if (arc.Rle_Out != dest_Len)
				goto Err;

			Unpack_Free(arc);

			return 0;

			Err:
			Unpack_Free(arc);

			return -1;
		}
		#endregion
	}
}
