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

namespace Polycode.NostalgicPlayer.Ports.Ancient.Internal.Decompressors
{
	/// <summary>
	/// MMCMP decompressor
	/// </summary>
	internal class MmcmpDecompressor : Decompressor
	{
		private static readonly uint8_t[] extraBits8 =
		[
			3, 3, 3, 3,  2, 1, 0, 0
		];

		private static readonly uint8_t[] extraBits16 =
		[
			4, 4, 4, 4,  3, 2, 1, 0,  0, 0, 0, 0,  0, 0, 0, 0
		];

		private readonly Buffer packedData;

		private readonly uint32_t packedSize;
		private readonly uint32_t rawSize;
		private readonly uint32_t blocksOffset;
		private readonly uint32_t blocks;
		private readonly uint16_t version;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private MmcmpDecompressor(Buffer packedData) : base(DecompressorType.Mmcmp)
		{
			this.packedData = packedData;

			if (!DetectHeader(this.packedData.ReadBe32(0)) || (packedData.ReadBe32(4) != Common.Common.FourCC("ONia")) || (packedData.ReadLe16(8) != 14) || (packedData.Size() < 24))
				throw new InvalidFormatException();

			version = packedData.ReadLe16(10);
			blocks = packedData.ReadLe16(12);
			blocksOffset = packedData.ReadLe32(18);
			rawSize = packedData.ReadLe32(14);

			if (rawSize > GetMaxRawSize())
				throw new InvalidFormatException();

			if (OverflowCheck.Sum(blocksOffset, blocks * 4) > packedData.Size())
				throw new InvalidFormatException();

			packedSize = 0;

			for (uint32_t i = 0; i < blocks; i++)
			{
				uint32_t blockAddr = packedData.ReadLe32(OverflowCheck.Sum(blocksOffset, i * 4));

				if (OverflowCheck.Sum(blockAddr, 20) >= packedData.Size())
					throw new InvalidFormatException();

				uint32_t blockSize = OverflowCheck.Sum(packedData.ReadLe32(blockAddr + 4), (uint32_t)packedData.ReadLe16(blockAddr + 12) * 8 + 20);
				packedSize = Math.Max(packedSize, OverflowCheck.Sum(blockAddr, blockSize));
			}

			if (packedSize > packedData.Size())
				throw new InvalidFormatException();
		}




		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool DetectHeader(uint32_t hdr)
		{
			return hdr == Common.Common.FourCC("ziRC");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public new static Decompressor Create(Buffer packedData)
		{
			return new MmcmpDecompressor(packedData);
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
			uint8_t[] rawData = new uint8_t[rawSize];

			for (uint32_t i = 0; i < blocks; i++)
			{
				uint32_t blockAddr = packedData.ReadLe32(blocksOffset + i * 4);

				// Read block header
				uint32_t unpackedBlockSize = packedData.ReadLe32(blockAddr);
				uint32_t packedBlockSize = packedData.ReadLe32(blockAddr + 4);
				uint32_t fileChecksum = packedData.ReadLe32(blockAddr + 8);
				uint32_t subBlocks = packedData.ReadLe16(blockAddr + 12);
				uint16_t flags = packedData.ReadLe16(blockAddr + 14);

				uint32_t packTableSize = packedData.ReadLe16(blockAddr + 16);
				if (packTableSize > packedBlockSize)
					throw new DecompressionException();

				uint32_t bitCount = packedData.ReadLe16(blockAddr + 18) + 1U;

				ForwardInputStream inputStream = new ForwardInputStream(packedData, OverflowCheck.Sum(blockAddr, subBlocks * 8,  20, packTableSize), OverflowCheck.Sum(blockAddr, subBlocks * 8, 20, packedBlockSize));
				LsbBitReader bitReader = new LsbBitReader(inputStream);

				uint32_t ReadBits(uint32_t count) => bitReader.ReadBits8(count);

				uint32_t currentSubBlock = 0;
				uint32_t outputOffset = 0;
				uint32_t outputSize = 0;

				void ReadNextSubBlock()
				{
					if (currentSubBlock >= subBlocks)
						throw new DecompressionException();

					outputOffset = packedData.ReadLe32(blockAddr + currentSubBlock * 8 + 20);
					outputSize = packedData.ReadLe32(blockAddr + currentSubBlock * 8 + 24);

					if (OverflowCheck.Sum(outputOffset, outputSize) > rawSize)
						throw new DecompressionException();

					currentSubBlock++;
				}

				uint32_t checksum = 0;
				uint32_t checksumPartial = 0;
				uint32_t checksumRot = 0;

				void WriteByte(uint8_t value, bool allowOverrun = false)
				{
					while (outputSize == 0)
					{
						if (allowOverrun && (currentSubBlock >= subBlocks))
							return;

						ReadNextSubBlock();
					}

					outputSize--;
					rawData[outputOffset++] = value;

					if (version >= 0x1310)
					{
						checksum ^= value;
						checksum = (checksum << 1) | (checksum >> 31);
					}
					else
					{
						checksumPartial |= ((uint)value) << (int)checksumRot;
						checksumRot += 8;

						if (checksumRot == 32)
						{
							checksum ^= checksumPartial;
							checksumPartial = 0;
							checksumRot = 0;
						}
					}
				}

				// Flags are:
				// 0 = compressed
				// 1 = delta mode
				// 2 = 16 bit mode
				// 8 = stereo
				// 9 = abs16
				// 10 = endian
				// Flags do not combine nicely
				// no compress - no other flags
				// compressed 8 bit - only delta (and presumably stereo matters)
				// compressed 16 bit - all flags matter
				if ((flags & 1) == 0)
				{
					// Not compressed
					for (uint32_t j = 0; j < packedBlockSize; j++)
						WriteByte(inputStream.ReadByte());
				}
				else if ((flags & 4) == 0)
				{
					// 8 bit compression

					// In case the bit-count is not enough to store a value, symbol at the end
					// of the codemap is created and this marks as a new bitCount
					if (bitCount > 8)
						throw new DecompressionException();

					uint8_t[] oldValue = [ 0, 0 ];
					uint32_t chIndex = 0;
					uint32_t tableOffset = blockAddr + subBlocks * 8 + 20;

					for (uint32_t j = 0; j < unpackedBlockSize; j++)
					{
						uint8_t extraBitCount = extraBits8[bitCount - 1];
						uint8_t threshold = (uint8_t)((1 << (int)bitCount) - (1 << (3 - extraBitCount)));
						uint8_t value = (uint8_t)ReadBits(bitCount);

						if (value >= threshold)
						{
							uint32_t newBitCount = (ReadBits(extraBitCount) | (uint32_t)((value - threshold) << extraBitCount)) + 1;
							if (bitCount != newBitCount)
							{
								bitCount = newBitCount;
								j--;
								continue;
							}
							else
							{
								value = (uint8_t)(0xf8 | ReadBits(3));
								if ((value == 0xff) && (ReadBits(1) != 0))
									break;
							}
						}

						if (value >= packTableSize)
							throw new DecompressionException();

						value = packedData[tableOffset + value];

						if ((flags & 2) != 0)
						{
							// Delta
							value += oldValue[chIndex];
							oldValue[chIndex] = value;

							if ((flags & 0x100) != 0)	// Stereo
								chIndex ^= 1;
						}

						WriteByte(value);
					}
				}
				else
				{
					// 16 bit compression
					//
					// Shameless copy paste from 8-bit variant, with minor changes
					if (bitCount > 16)
						throw new DecompressionException();

					int16_t[] oldValue = [ 0, 0 ];
					uint32_t chIndex = 0;

					for (uint32_t j = 0; j < unpackedBlockSize; j += 2)
					{
						uint8_t extraBitCount = extraBits16[bitCount - 1];
						uint16_t threshold = (uint16_t)((1 << (int)bitCount) - (1 << (4 - extraBitCount)));
						uint16_t value = (uint16_t)ReadBits(bitCount);

						if (value >= threshold)
						{
							uint32_t newBitCount = (ReadBits(extraBitCount) | (uint32_t)((value - threshold) << extraBitCount)) + 1;
							if (bitCount != newBitCount)
							{
								bitCount = newBitCount;
								j -= 2;
								continue;
							}
							else
							{
								value = (uint16_t)(0xfff0 | ReadBits(4));
								if ((value == 0xffff) && (ReadBits(1) != 0))
									break;
							}
						}

						int32_t sValue = value;

						if ((sValue & 1) != 0)
							sValue = -sValue - 1;

						sValue >>= 1;

						if ((flags & 2) != 0)
						{
							// Delta
							sValue += oldValue[chIndex];
							oldValue[chIndex] = (int16_t)sValue;

							if ((flags & 0x100) != 0)	// Stereo
								chIndex ^= 1;
						}
						else if ((flags & 0x200) == 0)	// abs16
							value ^= 0x8000;

						if ((flags & 0x400) != 0)
						{
							// Big endian
							WriteByte((uint8_t)(sValue >> 8));
							WriteByte((uint8_t)sValue, true);
						}
						else
						{
							// Little endian
							WriteByte((uint8_t)sValue);
							WriteByte((uint8_t)(sValue >> 8), true);
						}
					}
				}

				if (checksum != fileChecksum)
					throw new VerificationException();
			}

			yield return rawData;
		}
	}
}
