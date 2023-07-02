/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.Ancient.Common;
using Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers;
using Polycode.NostalgicPlayer.Ports.Ancient.Exceptions;
using Crc32 = Polycode.NostalgicPlayer.Ports.Ancient.Common.Crc32;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Internal.Decompressors.Xpk
{
	/// <summary>
	/// XPK-BZP2 decompressor
	/// </summary>
	internal class BZip2Decompressor : XpkDecompressor
	{
		private readonly Buffer packedData;

		private readonly size_t blockSize;
		private size_t packedSize;
		private size_t rawSize;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private BZip2Decompressor(uint32_t hdr, Buffer packedData)
		{
			this.packedData = packedData;
			packedSize = packedData.Size();

			uint32_t blockHdr = packedData.ReadBe32(0);

			if (!DetectHeader(blockHdr))
				throw new InvalidFormatException();

			blockSize = ((blockHdr & 0xff) - '0') * 100_000;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool DetectHeader(uint32_t hdr)
		{
			return ((hdr & 0xffff_ff00) == Common.Common.FourCC("BZh\0")) && ((hdr & 0xff) >= '1') && ((hdr & 0xff) <= '9');
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool DetectHeaderXpk(uint32_t hdr)
		{
			return hdr == Common.Common.FourCC("BZP2");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static XpkDecompressor Create(uint32_t hdr, Buffer packedData, ref State state)
		{
			return new BZip2Decompressor(hdr, packedData);
		}



		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		public override void DecompressImpl(Buffer rawData, List<Buffer> previousBuffers)
		{
			size_t packedSize = this.packedSize != 0 ? this.packedSize : packedData.Size();

			ForwardInputStream inputStream = new ForwardInputStream(packedData, 4, packedSize);
			MsbBitReader bitReader = new MsbBitReader(inputStream);

			uint32_t ReadBits(uint32_t count) => bitReader.ReadBits8(count);
			uint32_t ReadBit() => bitReader.ReadBits8(1);

			using (AutoExpandingForwardOutputStream outputStream = new AutoExpandingForwardOutputStream(rawData))
			{
				// Stream verification
				//
				// There is so much wrong in bzip2 CRC-calculation :(
				// 1. The bit ordering is opposite what everyone else does with CRC32
				// 2. The block CRCs are calculated separately, no way of calculating a complete
				//    CRC without knowing the block layout
				// 3. The CRC is the end of the stream and the stream is bit aligned. You
				//    can't read CRC without decompressing the stream
				uint32_t crc = 0;
				void CalculateBlockCrc(size_t blockPos, size_t blockSize)
				{
					crc = (crc << 1) | (crc >> 31);
					crc ^= Crc32.Crc32Rev(rawData, blockPos, blockSize, 0);
				}

				HuffmanDecoder<uint8_t> selectorDecoder = new HuffmanDecoder<uint8_t>(
					// Incomplete Huffman table. Errors possible
					new HuffmanCode<uint8_t>(1, 0b000000, 0),
					new HuffmanCode<uint8_t>(2, 0b000010, 1),
					new HuffmanCode<uint8_t>(3, 0b000110, 2),
					new HuffmanCode<uint8_t>(4, 0b001110, 3),
					new HuffmanCode<uint8_t>(5, 0b011110, 4),
					new HuffmanCode<uint8_t>(6, 0b111110, 5)
				);

				HuffmanDecoder<int32_t> deltaDecoder = new HuffmanDecoder<int32_t>(
					new HuffmanCode<int32_t>(1, 0b00, 0),
					new HuffmanCode<int32_t>(2, 0b10, 1),
					new HuffmanCode<int32_t>(2, 0b11, -1)
				);

				MemoryBuffer tmpBuffer = new MemoryBuffer(blockSize);

				// This is the dark, ancient secret of bzip2.
				// Versions before 0.9.5 had a data randomization for "too regular"
				// data problematic for the bwt-implementation at that time.
				// Although it is never utilized anymore, the support is still there
				// and this is exactly the kind of ancient stuff we want to support :)
				//
				// On this specific part (since it is a table of magic numbers)
				// we have no way other than copying it from the original reference
				for (;;)
				{
					uint32_t blockHdrHigh = ReadBits(32);
					uint32_t blockHdrLow = ReadBits(16);

					if ((blockHdrHigh == 0x31415926) && (blockHdrLow == 0x5359))
					{
						// A block
						//
						// This is rather spaghetti...
						ReadBits(32);		// Block CRC, not interested
						bool randomized = ReadBit() != 0;

						// Basically the random inserted is one LSB after n-th bytes
						// per defined in the table
						uint32_t randomPos = 1;
						uint32_t randomCounter = (uint32_t)(BZip2Table.RandomTable[0] - 1);

						bool RandomBit()
						{
							// Beauty is in the eye of the beholder: this is smallest form to hide the ugliness
							return (randomCounter-- == 0) ? (randomCounter = BZip2Table.RandomTable[randomPos++ & 511]) != 0 : false;
						}

						uint32_t currentPtr = ReadBits(24);

						uint32_t currentBlockSize = 0;
						{
							uint32_t numHuffmanItems = 2;
							uint32_t[] hufmannValues = new uint32_t[256];

							{
								// This is just a little bit inefficient but still we reading bit by bit since
								// reference does it. (bitstream format details do not spill over)
								bool[] usedMap = new bool[16];
								for (uint32_t i = 0; i < 16; i++)
									usedMap[i] = ReadBit() != 0;

								bool[] huffmanMap = new bool[256];
								for (uint32_t i = 0; i < 16; i++)
								{
									for (uint32_t j = 0; j < 16; j++)
										huffmanMap[i * 16 + j] = usedMap[i] ? ReadBit() != 0 : false;
								}

								for (uint32_t i = 0; i < 256; i++)
								{
									if (huffmanMap[i])
										numHuffmanItems++;
								}

								if (numHuffmanItems == 2)
									throw new DecompressionException();

								for (uint32_t currentValue = 0, i = 0; i < 256; i++)
								{
									if (huffmanMap[i])
										hufmannValues[currentValue++] = i;
								}
							}

							uint32_t huffmanGroups = ReadBits(3);
							if ((huffmanGroups < 2) || (huffmanGroups > 6))
								throw new DecompressionException();

							uint32_t selectorsUsed = ReadBits(15);
							if (selectorsUsed == 0)
								throw new DecompressionException();

							MemoryBuffer huffmanSelectorList = new MemoryBuffer(selectorsUsed);

							uint8_t UnMtf(uint8_t value, uint8_t[] map)
							{
								uint8_t ret = map[value];

								if (value != 0)
								{
									uint8_t tmp = map[value];

									for (uint32_t i = value; i != 0; i--)
										map[i] = map[i - 1];

									map[0] = tmp;
								}

								return ret;
							}

							// Create Huffman selectors
							uint8_t[] selectorMtfMap = { 0, 1, 2, 3, 4, 5 };

							for (uint32_t i = 0; i < selectorsUsed; i++)
							{
								uint8_t item = UnMtf(selectorDecoder.Decode(ReadBit), selectorMtfMap);
								if (item >= huffmanGroups)
									throw new DecompressionException();

								huffmanSelectorList[i] = item;
							}

							HuffmanDecoder<uint32_t>[] dataDecoders = new HuffmanDecoder<uint32_t>[huffmanGroups];

							// Create all tables
							for (uint32_t i = 0; i < huffmanGroups; i++)
							{
								uint8_t[] bitLengths = new uint8_t[258];

								uint32_t currentBits = ReadBits(5);
								for (uint32_t j = 0; j < numHuffmanItems; j++)
								{
									int32_t delta;

									do
									{
										delta = deltaDecoder.Decode(ReadBit);
										currentBits = (uint32_t)(currentBits + delta);
									}
									while (delta != 0);

									if ((currentBits < 1) || (currentBits > 20))
										throw new DecompressionException();

									bitLengths[j] = (uint8_t)currentBits;
								}

								dataDecoders[i] = new HuffmanDecoder<uint32_t>();
								dataDecoders[i].CreateOrderlyHuffmanTable(bitLengths, numHuffmanItems);
							}

							// Huffman decode + unRLE + unMTF
							HuffmanDecoder<uint32_t> currentHuffmanDecoder = null;
							uint32_t currentHuffmanIndex = 0;
							uint8_t[] dataMtfMap = new uint8_t[256];

							for (uint32_t i = 0; i < numHuffmanItems - 2; i++)
								dataMtfMap[i] = (uint8_t)i;

							uint32_t currentRunLength = 0;
							uint32_t currentRleWeight = 1;

							void DecodeRle()
							{
								if (currentRunLength != 0)
								{
									if ((currentBlockSize + currentRunLength) > blockSize)
										throw new DecompressionException();

									for (uint32_t i = 0; i < currentRunLength; i++)
										tmpBuffer[currentBlockSize++] = (uint8_t)hufmannValues[dataMtfMap[0]];
								}

								currentRunLength = 0;
								currentRleWeight = 1;
							}

							for (uint32_t streamIndex = 0; ; streamIndex++)
							{
								if ((streamIndex % 50) == 0)
								{
									if (currentHuffmanIndex >= selectorsUsed)
										throw new DecompressionException();

									currentHuffmanDecoder = dataDecoders[huffmanSelectorList[currentHuffmanIndex++]];
								}

								uint32_t symbolMtf = currentHuffmanDecoder.Decode(ReadBit);

								// Stop marker is referenced only once, and it is the last one.
								// This means we do not have to un-mtf it for detection
								if (symbolMtf == numHuffmanItems - 1)
									break;

								if (currentBlockSize >= blockSize)
									throw new DecompressionException();

								if (symbolMtf < 2)
								{
									currentRunLength += currentRleWeight << (int)symbolMtf;
									currentRleWeight <<= 1;
								}
								else
								{
									DecodeRle();
									uint8_t symbol = UnMtf((uint8_t)(symbolMtf - 1), dataMtfMap);

									if (currentBlockSize >= blockSize)
										throw new DecompressionException();

									tmpBuffer[currentBlockSize++] = (uint8_t)hufmannValues[symbol];
								}
							}

							DecodeRle();
							if (currentPtr >= currentBlockSize)
								throw new DecompressionException();
						}

						// Inverse BWT + final RLE decoding.
						//
						// There are a few dark corners here as well
						// 1. Can the stream end at 4 literals without count? I assume it is a valid optimization (and that this does not spillover to next block)
						// 2. Can the RLE-step include counts 252 to 255 even if reference does not do them? I assume yes here as well
						// 3. Can the stream be empty? We do not take issue here about that (that should be culled out earlier already)
						uint32_t[] sums = new uint32_t[256];
						for (uint32_t i = 0; i < currentBlockSize; i++)
							sums[tmpBuffer[i]]++;

						uint32_t[] rank = new uint32_t[256];
						for (uint32_t tot = 0, i = 0; i < 256; i++)
						{
							rank[i] = tot;
							tot += sums[i];
						}

						// Not at all happy about the memory consumption, but it simplifies the implementation a lot
						// and by sacrificing 4 * size (size as in actual block size) we do not have to slow search nor another temporary buffer
						// since by calculating forward table we can do forward decoding of the data on the same pass as iBWT.
						//
						// Also, because I'm lazy
						uint32_t[] forwardIndex = new uint32_t[currentBlockSize];
						for (uint32_t i = 0; i < currentBlockSize; i++)
							forwardIndex[rank[tmpBuffer[i]]++] = i;

						// Output + final RLE decoding
						uint8_t currentCh = 0;
						uint32_t currentChCount = 0;

						void OutputByte(uint8_t ch)
						{
							if (randomized && RandomBit())
								ch ^= 1;

							if (currentChCount == 0)
							{
								currentCh = ch;
								currentChCount = 1;
							}
							else
							{
								if ((ch == currentCh) && (currentChCount != 4))
									currentChCount++;
								else
								{
									void OutputBlock(uint32_t count)
									{
										for (uint32_t i = 0; i < count; i++)
											outputStream.WriteByte(currentCh);
									}

									if (currentChCount == 4)
									{
										OutputBlock((uint32_t)(ch + 4));
										currentChCount = 0;
									}
									else
									{
										OutputBlock(currentChCount);
										currentCh = ch;
										currentChCount = 1;
									}
								}
							}
						}

						size_t destOffsetStart = outputStream.GetOffset();

						// And now the final iBWT + unRLE is easy...
						for (uint32_t i = 0; i < currentBlockSize; i++)
						{
							currentPtr = forwardIndex[currentPtr];
							OutputByte(tmpBuffer[currentPtr]);
						}

						// Cleanup the state, a bit hackish way to do it
						if (currentChCount != 0)
							OutputByte((uint8_t)(currentChCount == 4 ? 0 : ~currentCh));

						CalculateBlockCrc(destOffsetStart, outputStream.GetOffset() - destOffsetStart);
					}
					else if ((blockHdrHigh == 0x17724538) && (blockHdrLow == 0x5090))
					{
						// End of blocks
						uint32_t rawCrc = ReadBits(32);
						if (crc != rawCrc)
							throw new VerificationException();

						break;
					}
					else
						throw new DecompressionException();
				}

				rawSize = outputStream.GetOffset();
				this.packedSize = inputStream.GetOffset();
			}
		}
	}
}
