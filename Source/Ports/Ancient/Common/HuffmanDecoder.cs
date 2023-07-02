/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.Ancient.Exceptions;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Common
{
	/// <summary>
	/// Uses the Huffman algorithm to decrunch data
	/// </summary>
	internal class HuffmanDecoder<T> where T : struct
	{
		#region Node class
		private class Node
		{
			public uint32_t[] Sub;
			public T Value;

			public Node(uint32_t sub0, uint32_t sub1, T value)
			{
				Sub = new [] { sub0, sub1 };
				Value = value;
			}
		}
		#endregion

		private readonly List<Node> table;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public HuffmanDecoder()
		{
			table = new List<Node>();
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public HuffmanDecoder(params HuffmanCode<T>[] args) : this()
		{
			foreach (HuffmanCode<T> item in args)
				Insert(item);
		}



		/********************************************************************/
		/// <summary>
		/// Decode a value
		/// </summary>
		/********************************************************************/
		public T Decode(Func<uint32_t> bitReader)
		{
			if (table.Count == 0)
				throw new DecompressionException();

			uint32_t i = 0;

			while ((table[(int)i].Sub[0] != 0) || (table[(int)i].Sub[1] != 0))
			{
				i = table[(int)i].Sub[bitReader() != 0 ? 1 : 0];
				if (i == 0)
					throw new DecompressionException();
			}

			return table[(int)i].Value;
		}



		/********************************************************************/
		/// <summary>
		/// Insert a code into the tree
		/// </summary>
		/********************************************************************/
		public void Insert(HuffmanCode<T> code)
		{
			uint32_t i = 0;
			uint32_t length = (uint32_t)table.Count;

			for (int32_t currentBit = (int32_t)code.Length; currentBit >= 0; currentBit--)
			{
				uint32_t codeBit = (currentBit != 0) && (((code.Code >> (currentBit - 1)) & 1) != 0) ? (uint32_t)1 : 0;
				if (i != length)
				{
					if ((currentBit == 0) || ((table[(int)i].Sub[0] == 0) && (table[(int)i].Sub[1] == 0)))
						throw new DecompressionException();

					ref uint32_t tmp = ref table[(int)i].Sub[codeBit];
					if (tmp == 0)
					{
						i = length;
						tmp = i;
					}
					else
						i = tmp;
				}
				else
				{
					Node node = new Node((currentBit != 0) && (codeBit == 0) ? length + 1 : 0, (currentBit != 0) && (codeBit != 0) ? length + 1 : 0, currentBit != 0 ? default(T) : code.Value);
					table.Add(node);
					length++;
					i++;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Create orderly Huffman table, as used by Deflate and BZip2
		/// </summary>
		/********************************************************************/
		public void CreateOrderlyHuffmanTable(uint8_t[] bitLengths, uint32_t bitTableLength)
		{
			uint8_t minDepth = 32, maxDepth = 0;

			// Some optimization: more tables
			uint16_t[] firstIndex = new uint16_t[33], lastIndex = new uint16_t[33];
			uint16_t[] nextIndexBuffer = new uint16_t[bitTableLength];

			for (uint32_t i = 1; i < 33; i++)
				firstIndex[i] = 0xffff;

			uint32_t realItems = 0;
			for (uint32_t i = 0; i < bitTableLength; i++)
			{
				uint8_t length = bitLengths[i];
				if (length > 32)
					throw new DecompressionException();

				if (length != 0)
				{
					if (length < minDepth)
						minDepth = length;

					if (length > maxDepth)
						maxDepth = length;

					if (firstIndex[length] == 0xffff)
					{
						firstIndex[length] = (uint16_t)i;
						lastIndex[length] = (uint16_t)i;
					}
					else
					{
						nextIndexBuffer[lastIndex[length]] = (uint16_t)i;
						lastIndex[length] = (uint16_t)i;
					}

					realItems++;
				}
			}

			if (maxDepth == 0)
				throw new DecompressionException();

			// Optimization, the multiple depends how sparse the tree really is. (minimum is *2)
			// Usually it is sparse
			table.Capacity = (int)realItems * 3;

			uint32_t code = 0;
			for (uint32_t depth = minDepth; depth <= maxDepth; depth++)
			{
				if (firstIndex[depth] != 0xffff)
					nextIndexBuffer[lastIndex[depth]] = (uint16_t)bitTableLength;

				for (uint32_t i = firstIndex[depth]; i < bitTableLength; i = nextIndexBuffer[i])
				{
					Insert(new HuffmanCode<T>(depth, code >> (int)(maxDepth - depth), (T)(object)i));
					code += (uint32_t)(1 << (int)(maxDepth - depth));
				}
			}
		}
	}
}
