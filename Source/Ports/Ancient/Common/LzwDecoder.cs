/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.Ancient.Exceptions;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Common
{
	/// <summary>
	/// Common LZW decoder
	/// </summary>
	internal class LzwDecoder
	{
		private readonly uint32_t maxCode;
		private readonly uint32_t literalCodes;
		private readonly uint32_t stackLength;
		private uint32_t freeIndex;

		private uint32_t prevCode;
		private uint32_t newCode = 0;

		private readonly uint32_t[] prefix;
		private readonly uint8_t[] suffix;
		private readonly uint8_t[] stack;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public LzwDecoder(uint32_t maxCode, uint32_t literalCodes, uint32_t stackLength, uint32_t firstCode)
		{
			this.maxCode = maxCode;
			this.literalCodes = literalCodes;
			this.stackLength = stackLength;
			freeIndex = literalCodes;
			prevCode = firstCode;

			prefix = new uint32_t[maxCode - literalCodes];
			suffix = new uint8_t[maxCode - literalCodes];
			stack = new uint8_t[stackLength];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset(uint32_t firstCode)
		{
			freeIndex = literalCodes;
			prevCode = firstCode;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Add(uint32_t code)
		{
			if (freeIndex < maxCode)
			{
				suffix[freeIndex - literalCodes] = (uint8_t)newCode;
				prefix[freeIndex - literalCodes] = prevCode;
				freeIndex++;
			}

			prevCode = code;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Write(uint32_t code, bool addNew, Action<uint8_t> func)
		{
			uint32_t SuffixLookup(uint32_t value)
			{
				if (value >= freeIndex)
					throw new DecompressionException();

				return value < literalCodes ? value : suffix[value - literalCodes];
			}

			uint32_t stackPos = 0;

			uint32_t tmp = newCode;

			if (addNew)
				code = prevCode;

			newCode = SuffixLookup(code);

			while (code >= literalCodes)
			{
				if ((stackPos + 1) >= stackLength)
					throw new DecompressionException();

				stack[stackPos++] = (uint8_t)newCode;
				code = prefix[code - literalCodes];
				newCode = SuffixLookup(code);
			}

			stack[stackPos++] = (uint8_t)newCode;

			while (stackPos != 0)
				func(stack[--stackPos]);

			if (addNew)
				func((uint8_t)tmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool IsLiteral(uint32_t code)
		{
			return code < freeIndex;
		}
	}
}
