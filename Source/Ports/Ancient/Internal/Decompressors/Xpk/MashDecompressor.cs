/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.Ancient.Common;
using Polycode.NostalgicPlayer.Ports.Ancient.Exceptions;
using Buffer = Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers.Buffer;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Internal.Decompressors.Xpk
{
	/// <summary>
	/// XPK-MASH decompressor
	/// </summary>
	internal class MashDecompressor : XpkDecompressor
	{
		private static readonly uint8_t[] distanceBits =
		{
			5, 7, 9, 10, 11, 12, 13, 14
		};

		private static readonly uint32_t[] distanceAdditions =
		{
			0, 0x20, 0xa0, 0x2a0, 0x6a0, 0xea0, 0x1ea0, 0x3ea0
		};

		private readonly Buffer packedData;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private MashDecompressor(uint32_t hdr, Buffer packedData)
		{
			this.packedData = packedData;

			if (!DetectHeaderXpk(hdr))
				throw new InvalidFormatException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool DetectHeaderXpk(uint32_t hdr)
		{
			return hdr == Common.Common.FourCC("MASH");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static XpkDecompressor Create(uint32_t hdr, Buffer packedData, ref State state)
		{
			return new MashDecompressor(hdr, packedData);
		}



		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		public override void DecompressImpl(Buffer rawData, List<Buffer> previousBuffers)
		{
			ForwardInputStream inputStream = new ForwardInputStream(packedData, 0, packedData.Size());
			MsbBitReader bitReader = new MsbBitReader(inputStream);

			uint32_t ReadBits(uint32_t count) => bitReader.ReadBits8(count);
			uint32_t ReadBit() => bitReader.ReadBits8(1);
			uint8_t ReadByte() => inputStream.ReadByte();

			size_t rawSize = rawData.Size();
			ForwardOutputStream outputStream = new ForwardOutputStream(rawData, 0, rawSize);

			HuffmanDecoder<uint32_t> litDecoder = new HuffmanDecoder<uint32_t>(
				new HuffmanCode<uint32_t>(1, 0b000000, 0),
				new HuffmanCode<uint32_t>(2, 0b000010, 1),
				new HuffmanCode<uint32_t>(3, 0b000110, 2),
				new HuffmanCode<uint32_t>(4, 0b001110, 3),
				new HuffmanCode<uint32_t>(5, 0b011110, 4),
				new HuffmanCode<uint32_t>(6, 0b111110, 5),
				new HuffmanCode<uint32_t>(6, 0b111111, 6)
			);

			while (!outputStream.Eof)
			{
				uint32_t litLength = litDecoder.Decode(ReadBit);
				if (litLength == 6)
				{
					uint32_t litBits;
					for (litBits = 1; litBits <= 17; litBits++)
					{
						if (ReadBit() == 0)
							break;
					}

					if (litBits == 17)
						throw new DecompressionException();

					litLength = (uint32_t)(ReadBits(litBits) + (1 << (int)litBits) + 4);
				}

				for (uint32_t i = 0; i < litLength; i++)
					outputStream.WriteByte(ReadByte());

				uint32_t count, distance;

				void ReadDistance()
				{
					uint32_t tableIndex = ReadBits(3);
					distance = ReadBits(distanceBits[tableIndex]) + distanceAdditions[tableIndex];
				}

				if (ReadBit() != 0)
				{
					uint32_t countBits;

					for (countBits = 1; countBits <= 16; countBits++)
					{
						if (ReadBit() == 0)
							break;
					}

					if (countBits == 16)
						throw new DecompressionException();

					count = (uint32_t)(ReadBits(countBits) + (1 << (int)countBits) + 2);
					ReadDistance();
				}
				else
				{
					if (ReadBit() != 0)
					{
						ReadDistance();
						count = 3;
					}
					else
					{
						distance = ReadBits(9);
						count = 2;
					}
				}

				// Hack to make it work
				if ((distance == 0) && outputStream.Eof)
					break;

				// Zero distance when we are at the end of the stream...
				// There seems to be almost systematic extra one byte at the end of the stream...
				count = Math.Min(count, (uint32_t)(rawSize - outputStream.GetOffset()));
				outputStream.Copy(distance, count);
			}
		}
	}
}
