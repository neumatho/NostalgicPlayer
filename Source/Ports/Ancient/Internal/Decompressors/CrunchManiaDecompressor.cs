/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.Ancient.Common;
using Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers;
using Polycode.NostalgicPlayer.Ports.Ancient.Exceptions;
using Buffer = Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers.Buffer;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Internal.Decompressors
{
	/// <summary>
	/// Crunch Mania decompressor
	/// </summary>
	internal class CrunchManiaDecompressor : Decompressor
	{
		private readonly Buffer packedData;

		private readonly uint32_t rawSize;
		private readonly uint32_t packedSize;

		private readonly bool isSampled;
		private readonly bool isLzh;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private CrunchManiaDecompressor(Buffer packedData) : base(DecompressorType.CrunchMania)
		{
			this.packedData = packedData;

			uint32_t hdr = packedData.ReadBe32(0);
			if (!DetectHeader(hdr) || (packedData.Size() < 20))
				throw new InvalidFormatException();

			rawSize = packedData.ReadBe32(6);
			packedSize = packedData.ReadBe32(10);

			if ((rawSize == 0) || (packedSize == 0) || (rawSize > GetMaxRawSize()) || (packedSize > GetMaxRawSize()) || OverflowCheck.Sum(packedSize, 14) > packedData.Size())
				throw new InvalidFormatException();

			isSampled = ((hdr >> 8) & 0xff) == 'm';
			isLzh = (hdr & 0xff) == '2';
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool DetectHeader(uint32_t hdr)
		{
			if ((hdr == Common.Common.FourCC("CrM!")) ||
			    (hdr == Common.Common.FourCC("CrM2")) ||
			    (hdr == Common.Common.FourCC("Crm!")) ||
			    (hdr == Common.Common.FourCC("Crm2")))
				return true;

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public new static Decompressor Create(Buffer packedData)
		{
			return new CrunchManiaDecompressor(packedData);
		}



		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		public override size_t GetRawSize()
		{
			return rawSize;
		}



		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<uint8_t[]> DecompressImpl()
		{
			uint8_t[] outputData = new uint8_t[rawSize];
			WrappedArrayBuffer rawData = new WrappedArrayBuffer(outputData);

			BackwardInputStream inputStream = new BackwardInputStream(packedData, 14, packedSize + 14 - 6);
			LsbBitReader bitReader = new LsbBitReader(inputStream);

			{
				// There are empty bits?!? at the start of the stream. Take them out
				size_t bufOffset = packedSize + 14 - 6;
				uint32_t originalBitsContent = packedData.ReadBe32(bufOffset);
				uint16_t originalShift = packedData.ReadBe16(bufOffset + 4);
				uint8_t bufBitsLength = (uint8_t)(originalShift + 16);
				uint32_t bufBitsContent = originalBitsContent >> (16 - originalShift);
				bitReader.Reset(bufBitsContent, bufBitsLength);
			}

			uint32_t ReadBits(uint32_t count) => bitReader.ReadBits8(count);
			uint32_t ReadBit() => bitReader.ReadBits8(1);

			BackwardOutputStream outputStream = new BackwardOutputStream(rawData, 0, rawSize);

			if (isLzh)
			{
				void ReadHuffmanTable(HuffmanDecoder<uint32_t> dec, uint32_t codeLength)
				{
					uint32_t maxDepth = ReadBits(4);
					if (maxDepth == 0)
						throw new DecompressionException();

					uint32_t[] lengthTable = new uint32_t[15];

					for (uint32_t i = 0; i < maxDepth; i++)
						lengthTable[i] = ReadBits(Math.Min(i + 1, codeLength));

					uint32_t code = 0;
					for (uint32_t depth = 1; depth <= maxDepth; depth++)
					{
						for (uint32_t i = 0; i < lengthTable[depth - 1]; i++)
						{
							uint32_t value = ReadBits(codeLength);

							dec.Insert(new HuffmanCode<uint32_t>(depth, code >> (int)(maxDepth - depth), value));
							code += (uint32_t)(1 << (int)(maxDepth - depth));
						}
					}
				}

				do
				{
					HuffmanDecoder<uint32_t> lengthDecoder = new HuffmanDecoder<uint32_t>();
					HuffmanDecoder<uint32_t> distanceDecoder = new HuffmanDecoder<uint32_t>();

					ReadHuffmanTable(lengthDecoder, 9);
					ReadHuffmanTable(distanceDecoder, 4);

					uint32_t items = ReadBits(16) + 1;
					for (uint32_t i = 0; i < items; i++)
					{
						uint32_t count = lengthDecoder.Decode(ReadBit);

						if ((count & 0x100) != 0)
							outputStream.WriteByte((uint8_t)count);
						else
						{
							count += 3;

							uint32_t distanceBits = distanceDecoder.Decode(ReadBit);
							uint32_t distance;

							if (distanceBits == 0)
								distance = ReadBits(1) + 1;
							else
								distance = (ReadBits(distanceBits) | (uint32_t)(1 << (int)distanceBits)) + 1;

							outputStream.Copy(distance, count);
						}
					}
				}
				while (ReadBit() != 0);
			}
			else
			{
				HuffmanDecoder<uint8_t> lengthDecoder = new HuffmanDecoder<uint8_t>(
					new HuffmanCode<uint8_t>(1, 0b000, 0),
					new HuffmanCode<uint8_t>(2, 0b010, 1),
					new HuffmanCode<uint8_t>(3, 0b110, 2),
					new HuffmanCode<uint8_t>(3, 0b111, 3)
				);

				HuffmanDecoder<uint8_t> distanceDecoder = new HuffmanDecoder<uint8_t>(
					new HuffmanCode<uint8_t>(1, 0b00, 1),
					new HuffmanCode<uint8_t>(2, 0b10, 0),
					new HuffmanCode<uint8_t>(2, 0b11, 2)
				);

				VariableLengthCodeDecoder lengthVlc = new VariableLengthCodeDecoder(1, 2, 4, 8);
				VariableLengthCodeDecoder distanceVlc = new VariableLengthCodeDecoder(5, 9, 14);

				while (!outputStream.Eof)
				{
					if (ReadBit() != 0)
						outputStream.WriteByte((uint8_t)ReadBits(8));
					else
					{
						uint8_t lengthIndex = lengthDecoder.Decode(ReadBit);
						uint32_t count = lengthVlc.Decode(ReadBits, lengthIndex) + 2;

						if (count == 23)
						{
							if (ReadBit() != 0)
								count = ReadBits(5) + 15;
							else
								count = ReadBits(14) + 15;

							for (uint32_t i = 0; i < count; i++)
								outputStream.WriteByte((uint8_t)ReadBits(8));
						}
						else
						{
							if (count > 23)
								count--;

							uint8_t distanceIndex = distanceDecoder.Decode(ReadBit);
							uint32_t distance = distanceVlc.Decode(ReadBits, distanceIndex);

							outputStream.Copy(distance, count);
						}
					}
				}
			}

			if (!outputStream.Eof)
				throw new DecompressionException();

			if (isSampled)
				DltaDecode.Decode(rawData, rawData, 0, rawSize);

			yield return outputData;
		}
	}
}
