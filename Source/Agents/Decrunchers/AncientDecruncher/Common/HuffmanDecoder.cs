/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
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
	internal class HuffmanDecoder<T>
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
				throw new DepackerException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

			int i = 0;

			while ((table[i].Sub[0] != 0) || (table[i].Sub[1] != 0))
			{
				i = table[i].Sub[bitReader() != 0 ? 1 : 0];
				if (i == 0)
					throw new DepackerException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);
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
						throw new DepackerException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

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

		#region Private methods
		#endregion
	}
}
