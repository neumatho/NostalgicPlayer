/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Linq;
using Polycode.NostalgicPlayer.Ports.Ancient.Exceptions;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal class VariableLengthCodeDecoder
	{
		public delegate uint32_t BitReader(uint32_t count);

		private readonly uint8_t[] bitLengths;
		private readonly uint32_t[] offsets;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// 
		/// Negative lengths can be used to reset the offset
		/// </summary>
		/********************************************************************/
		public VariableLengthCodeDecoder(params int[] args)
		{
			bitLengths = args.Select(CreateBitLength).ToArray();

			offsets = new uint32_t[args.Length];

			uint32_t length = 0;
			uint32_t i = 0;

			void FoldOffsets(int value)
			{
				if (value < 0)
				{
					offsets[i] = 0;
					length = 1U << -value;
					i++;
					return;
				}

				offsets[i] = length;
				length += 1U << value;
				i++;
			}

			foreach (int value in args)
				FoldOffsets(value);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint32_t Decode(BitReader bitReader, uint32_t @base)
		{
			if (@base >= offsets.Length)
				throw new DecompressionException();

			return offsets[@base] + bitReader(bitLengths[@base]);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint8_t CreateBitLength(int value)
		{
			return value >= 0 ? (uint8_t)value : (uint8_t)(-value);
		}
		#endregion
	}
}
