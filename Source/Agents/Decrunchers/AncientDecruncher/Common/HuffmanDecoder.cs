/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Exceptions;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common
{
	/// <summary>
	/// Uses the Huffman algorithm to decrunch data
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class HuffmanDecoder<T> where T : struct
	{
		#region Node class
		private class Node
		{
			public int[] Sub;
			public T Value;

			public Node(int sub0, int sub1, T value)
			{
				Sub = new [] { sub0, sub1 };
				Value = value;
			}
		}
		#endregion

		private readonly string agentName;

		private readonly List<Node> table;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public HuffmanDecoder(string agentName)
		{
			this.agentName = agentName;

			table = new List<Node>();
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public HuffmanDecoder(string agentName, params HuffmanCode<T>[] args) : this(agentName)
		{
			foreach (HuffmanCode<T> item in args)
				Insert(item);
		}



		/********************************************************************/
		/// <summary>
		/// Decode a value
		/// </summary>
		/********************************************************************/
		public T Decode(Func<uint> bitReader)
		{
			if (table.Count == 0)
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

			int i = 0;

			while ((table[i].Sub[0] != 0) || (table[i].Sub[1] != 0))
			{
				i = table[i].Sub[bitReader() != 0 ? 1 : 0];
				if (i == 0)
					throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);
			}

			return table[i].Value;
		}



		/********************************************************************/
		/// <summary>
		/// Insert a code into the tree
		/// </summary>
		/********************************************************************/
		public void Insert(HuffmanCode<T> code)
		{
			int i = 0;
			int length = table.Count;

			for (int currentBit = (int)code.Length; currentBit >= 0; currentBit--)
			{
				uint codeBit = (currentBit != 0) && (((code.Code >> (currentBit - 1)) & 1) != 0) ? (uint)1 : 0;
				if (i != length)
				{
					if ((currentBit == 0) || ((table[i].Sub[0] == 0) && (table[i].Sub[1] == 0)))
						throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

					ref int tmp = ref table[i].Sub[codeBit];
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
		public void CreateOrderlyHuffmanTable(byte[] bitLengths, uint bitTableLength)
		{
			byte minDepth = 32, maxDepth = 0;

			// Some optimization: more tables
			ushort[] firstIndex = new ushort[33], lastIndex = new ushort[33];
			ushort[] nextIndexBuffer = new ushort[bitTableLength];

			for (int i = 1; i < 33; i++)
				firstIndex[i] = 0xffff;

			uint realItems = 0;
			for (uint i = 0; i < bitTableLength; i++)
			{
				byte length = bitLengths[i];
				if (length > 32)
					throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

				if (length != 0)
				{
					if (length < minDepth)
						minDepth = length;

					if (length > maxDepth)
						maxDepth = length;

					if (firstIndex[length] == 0xffff)
					{
						firstIndex[length] = (ushort)i;
						lastIndex[length] = (ushort)i;
					}
					else
					{
						nextIndexBuffer[lastIndex[length]] = (ushort)i;
						lastIndex[length] = (ushort)i;
					}

					realItems++;
				}
			}

			if (maxDepth == 0)
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

			// Optimization, the multiple depends how sparse the tree really is. (minimum is *2)
			// Usually it is sparse
			table.Capacity = (int)realItems * 3;

			uint code = 0;
			for (uint depth = minDepth; depth <= maxDepth; depth++)
			{
				if (firstIndex[depth] != 0xffff)
					nextIndexBuffer[lastIndex[depth]] = (ushort)bitTableLength;

				for (uint i = firstIndex[depth]; i < bitTableLength; i = nextIndexBuffer[i])
				{
					Insert(new HuffmanCode<T>(depth, code >> (int)(maxDepth - depth), (T)(object)i));
					code += (uint)(1 << (int)(maxDepth - depth));
				}
			}
		}
	}
}
