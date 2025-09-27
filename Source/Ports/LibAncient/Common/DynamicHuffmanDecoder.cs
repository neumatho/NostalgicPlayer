/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibAncient.Exceptions;

namespace Polycode.NostalgicPlayer.Ports.LibAncient.Common
{
	/// <summary>
	/// Uses the dynamic Huffman algorithm to decrunch data
	/// </summary>
	internal class DynamicHuffmanDecoder
	{
		#region Node class
		private class Node
		{
			// ReSharper disable InconsistentNaming
			public uint32_t Frequency;
			public uint32_t Index;
			public uint32_t Parent;
			public uint32_t LeftLeaf;
			public uint32_t RightLeaf;
			// ReSharper restore InconsistentNaming
		}
		#endregion

		private readonly uint32_t maxCount;
		private readonly uint32_t initialCount;
		private uint32_t count;

		private readonly Node[] nodes;
		private readonly uint32_t[] codeMap;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DynamicHuffmanDecoder(uint32_t maxCount, uint32_t? initialCount = null)
		{
			this.maxCount = maxCount;
			this.initialCount = initialCount ?? maxCount;

			if (this.initialCount > maxCount)
				throw new DecompressionException();

			nodes = ArrayHelper.InitializeArray<Node>((int)maxCount * 2 - 1);
			codeMap = new uint[maxCount * 2 - 1];

			Reset();
		}



		/********************************************************************/
		/// <summary>
		/// Decode a value
		/// </summary>
		/********************************************************************/
		public uint32_t Decode(Func<uint32_t> bitReader)
		{
			if (count == 0)
				throw new DecompressionException();

			if (count == 1)
				return 0;

			uint32_t code = maxCount * 2 - 2;

			while (code >= maxCount)
				code = bitReader() != 0 ? nodes[code].RightLeaf : nodes[code].LeftLeaf;

			return code;
		}



		/********************************************************************/
		/// <summary>
		/// Insert a code into the tree
		/// </summary>
		/********************************************************************/
		public void Update(uint32_t code)
		{
			if (code >= count)
				throw new DecompressionException();

			// This is a bug in LH2. Nobody else uses this codepath, so we can let it be...
			if (count == 1)
			{
				nodes[0].Frequency = 1;
				return;
			}

			while (code != maxCount * 2 - 2)
			{
				nodes[code].Frequency++;

				uint32_t index = nodes[code].Index;
				uint32_t destIndex = index;
				uint32_t freq = nodes[code].Frequency;

				while ((destIndex != (maxCount * 2 - 2)) && (freq > nodes[codeMap[destIndex + 1]].Frequency))
					destIndex++;

				if (index != destIndex)
				{
					ref uint32_t GetParentLeaf(uint32_t currentCode)
					{
						Node parent = nodes[nodes[currentCode].Parent];
						return ref parent.LeftLeaf == currentCode ? ref parent.LeftLeaf : ref parent.RightLeaf;
					}

					void Swap(ref uint32_t a, ref uint32_t b)
					{
						(a, b) = (b, a);
					}

					uint32_t destCode = codeMap[destIndex];
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
		public uint32_t GetMaxFrequency()
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

			for (uint32_t i = 0; i < count; i++)
			{
				nodes[i].Frequency = 1;
				nodes[i].Index = i + (maxCount - count) * 2;
				nodes[i].Parent = maxCount * 2 - count + (i >> 1);
				nodes[i].LeftLeaf = 0;
				nodes[i].RightLeaf = 0;

				codeMap[i + (maxCount - count) * 2] = i;
			}

			for (uint32_t i = maxCount * 2 - count, j = 0; i < maxCount * 2 - 1; i++, j += 2)
			{
				uint32_t l = (j >= count) ? j + (maxCount - count) * 2 : j;
				uint32_t r = (j + 1 >= count) ? j + 1 + (maxCount - count) * 2 : j + 1;

				nodes[i].Frequency = nodes[l].Frequency + nodes[r].Frequency;
				nodes[i].Index = i;
				nodes[i].Parent = maxCount + (i >> 1);
				nodes[i].LeftLeaf = l;
				nodes[i].RightLeaf = r;

				codeMap[i] = i;
			}
		}
		#endregion
	}
}
