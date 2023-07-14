/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Common
{
	/// <summary>
	/// </summary>
	internal class LsbBitReader
	{
		private readonly IInputStream inputStream;

		private uint32_t bufContent = 0;
		private uint8_t bufLength = 0;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public LsbBitReader(IInputStream inputStream)
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
			return ReadBitsInternal(count, () =>
			{
				bufContent = inputStream.ReadByte();
				bufLength = 8;
			});
		}



		/********************************************************************/
		/// <summary>
		/// Read count number of bits in big endian format
		/// </summary>
		/********************************************************************/
		public uint32_t ReadBitsBe32(uint32_t count, uint32_t xorKey = 0)
		{
			return ReadBitsInternal(count, () =>
			{
				uint8_t[] tmp = new uint8_t[4];
				Span<uint8_t> buf = inputStream.Consume(4, tmp);

				bufContent = (uint32_t)(((buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3]) ^ xorKey);
				bufLength = 32;
			});
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

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read count number of bits by calling the function given
		/// </summary>
		/********************************************************************/
		private uint32_t ReadBitsInternal(uint32_t count, Action readWord)
		{
			uint32_t ret = 0;
			uint32_t pos = 0;

			while (count != 0)
			{
				if (bufLength == 0)
					readWord();

				uint8_t maxCount = Math.Min((uint8_t)count, bufLength);
				ret |= (uint32_t)((bufContent & ((1 << maxCount) - 1)) << (int)pos);

				bufContent >>= maxCount;
				bufLength -= maxCount;
				count -= maxCount;
				pos += maxCount;
			}

			return ret;
		}
		#endregion
	}
}
