/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.LibAncient.Exceptions;

namespace Polycode.NostalgicPlayer.Ports.LibAncient.Common
{
	/// <summary>
	/// </summary>
	internal class MsbBitReader
	{
		public delegate (uint32_t BufContent, uint8_t BufLength) ReadWord();

		private readonly IInputStream inputStream;

		private uint32_t bufContent = 0;
		private uint8_t bufLength = 0;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MsbBitReader(IInputStream inputStream)
		{
			this.inputStream = inputStream;
		}



		/********************************************************************/
		/// <summary>
		/// Read the number of bits given as argument and return the number
		/// </summary>
		/********************************************************************/
		public uint32_t ReadBits8(uint32_t count)
		{
			return ReadBitsGeneric(count, () => (inputStream.ReadByte(), 8));
		}



		/********************************************************************/
		/// <summary>
		/// Read the number of bits given as argument and return the number
		/// </summary>
		/********************************************************************/
		public uint32_t ReadBitsBe16(uint32_t count)
		{
			return ReadBitsGeneric(count, () => (inputStream.ReadBE16(), 16));
		}



		/********************************************************************/
		/// <summary>
		/// Read the number of bits given as argument and return the number
		/// </summary>
		/********************************************************************/
		public uint32_t ReadBitsBe32(uint32_t count)
		{
			return ReadBitsGeneric(count, () => (inputStream.ReadBE32(), 32));
		}



		/********************************************************************/
		/// <summary>
		/// Reset internal variables
		/// </summary>
		/********************************************************************/
		public void Reset(uint32_t bufContent = 0, uint8_t bufLength = 0)
		{
			this.bufContent = bufContent;
			this.bufLength = bufLength;
		}



		/********************************************************************/
		/// <summary>
		/// Read count number of bits by calling the function given
		/// </summary>
		/********************************************************************/
		public uint32_t ReadBitsGeneric(uint32_t count, ReadWord readWord)
		{
			uint32_t ret = 0;

			if (count > 32)
				throw new DecompressionException();

			while (count != 0)
			{
				if (bufLength == 0)
					(bufContent, bufLength) = readWord();

				uint8_t maxCount = Math.Min((uint8_t)count, bufLength);
				bufLength -= maxCount;

				ret = (uint32_t)((ret << maxCount) | ((bufContent >> bufLength) & ((1 << maxCount) - 1)));
				count -= maxCount;
			}

			return ret;
		}
	}
}
