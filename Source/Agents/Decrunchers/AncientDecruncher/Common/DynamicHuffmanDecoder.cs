/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common
{
	/// <summary>
	/// Uses the dynamic Huffman algorithm to decrunch data
	/// </summary>
	internal class DynamicHuffmanDecoder
	{
		#region Node class
		private class Node
		{
			public uint Frequency;
			public uint Index;
			public uint Parent;
			public uint[] Leaves = new uint[2];
		}
		#endregion

		private readonly string agentName;

		private readonly uint maxCount;
		private readonly uint initialCount;
		private uint count;

		private readonly Node[] nodes;
		private readonly uint[] codeMap;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DynamicHuffmanDecoder(string agentName, uint maxCount, uint? initialCount = null)
		{
			this.agentName = agentName;

			this.maxCount = maxCount;
			this.initialCount = initialCount ?? maxCount;

			if (this.initialCount > maxCount)
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

			nodes = Helpers.InitializeArray<Node>((int)maxCount * 2 - 1);
			codeMap = new uint[maxCount * 2 - 1];

			Reset();
		}



		/********************************************************************/
		/// <summary>
		/// Decode a value
		/// </summary>
		/********************************************************************/
		public uint Decode(Func<uint> bitReader)
		{
			if (count == 0)
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

			if (count == 1)
				return 0;

			uint code = maxCount * 2 - 2;
			while (code >= maxCount)
				code = nodes[code].Leaves[bitReader() != 0 ? 1 : 0];

			return code;
		}



		/********************************************************************/
		/// <summary>
		/// Insert a code into the tree
		/// </summary>
		/********************************************************************/
		public void Update(uint code)
		{
			if (code >= count)
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

			// This is a bug in LH2. Nobody else uses this codepath, so we can let it be...
			if (count == 1)
			{
				nodes[0].Frequency = 1;
				return;
			}

			while (code != maxCount * 2 - 2)
			{
				nodes[code].Frequency++;

				uint index = nodes[code].Index;
				uint destIndex = index;
				uint freq = nodes[code].Frequency;

				while ((destIndex != (maxCount * 2 - 2)) && (freq > nodes[codeMap[destIndex + 1]].Frequency))
					destIndex++;

				if (index != destIndex)
				{
					ref uint GetParentLeaf(uint currentCode)
					{
						Node parent = nodes[nodes[currentCode].Parent];
						return ref parent.Leaves[(parent.Leaves[0] == currentCode) ? 0 : 1];
					}

					void Swap(ref uint a, ref uint b)
					{
						(a, b) = (b, a);
					}

					uint destCode = codeMap[destIndex];
					Swap(ref nodes[code].Index, ref nodes[destCode].Index);
					Swap(ref codeMap[index], ref codeMap[destIndex]);
					Swap(ref GetParentLeaf(code), ref GetParentLeaf(destCode));
					Swap(ref nodes[code].Parent, ref nodes[destCode].Parent);
				}

				code = nodes[code].Parent;
			}

			nodes[code].Frequency++;
		}



		/********************************************************************/
		/// <summary>
		/// Return the max frequency used
		/// </summary>
		/********************************************************************/
		public uint GetMaxFrequency()
		{
			return nodes[maxCount * 2 - 2].Frequency;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initialize internal structures
		/// </summary>
		/********************************************************************/
		private void Reset()
		{
			count = initialCount;
			if (count == 0)
				return;

			for (uint i = 0; i < count; i++)
			{
				nodes[i].Frequency = 1;
				nodes[i].Index = i + (maxCount - count) * 2;
				nodes[i].Parent = maxCount * 2 - count + (i >> 1);
				nodes[i].Leaves[0] = 0;
				nodes[i].Leaves[1] = 0;

				codeMap[i + (maxCount - count) * 2] = i;
			}

			for (uint i = maxCount * 2 - count, j = 0; i < maxCount * 2 - 1; i++, j +=2)
			{
				uint l = (j >= count) ? j + (maxCount - count) * 2 : j;
				uint r = (j + 1 >= count) ? j + 1 + (maxCount - count) * 2 : j + 1;

				nodes[i].Frequency = nodes[l].Frequency + nodes[r].Frequency;
				nodes[i].Index = i;
				nodes[i].Parent = maxCount + (i >> 1);
				nodes[i].Leaves[0] = l;
				nodes[i].Leaves[1] = r;

				codeMap[i] = i;
			}
		}
		#endregion
	}
}
