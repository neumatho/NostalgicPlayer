/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common;
using Polycode.NostalgicPlayer.Kit.Exceptions;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats.Streams.Xpk
{
	/// <summary>
	/// This stream read data crunched with XPK (BZP2)
	/// </summary>
	internal class Xpk_Bzp2Stream : XpkStream
	{
		private static readonly ushort[] randomTable =
		{
			619 - 1, 720 - 1, 127 - 1, 481 - 1, 931 - 1, 816 - 1, 813 - 1, 233 - 1, 566 - 1, 247 - 1, 985 - 1, 724 - 1, 205 - 1, 454 - 1, 863 - 1, 491 - 1,
			741 - 1, 242 - 1, 949 - 1, 214 - 1, 733 - 1, 859 - 1, 335 - 1, 708 - 1, 621 - 1, 574 - 1,  73 - 1, 654 - 1, 730 - 1, 472 - 1, 419 - 1, 436 - 1,
			278 - 1, 496 - 1, 867 - 1, 210 - 1, 399 - 1, 680 - 1, 480 - 1,  51 - 1, 878 - 1, 465 - 1, 811 - 1, 169 - 1, 869 - 1, 675 - 1, 611 - 1, 697 - 1,
			867 - 1, 561 - 1, 862 - 1, 687 - 1, 507 - 1, 283 - 1, 482 - 1, 129 - 1, 807 - 1, 591 - 1, 733 - 1, 623 - 1, 150 - 1, 238 - 1,  59 - 1, 379 - 1,
			684 - 1, 877 - 1, 625 - 1, 169 - 1, 643 - 1, 105 - 1, 170 - 1, 607 - 1, 520 - 1, 932 - 1, 727 - 1, 476 - 1, 693 - 1, 425 - 1, 174 - 1, 647 - 1,
			 73 - 1, 122 - 1, 335 - 1, 530 - 1, 442 - 1, 853 - 1, 695 - 1, 249 - 1, 445 - 1, 515 - 1, 909 - 1, 545 - 1, 703 - 1, 919 - 1, 874 - 1, 474 - 1,
			882 - 1, 500 - 1, 594 - 1, 612 - 1, 641 - 1, 801 - 1, 220 - 1, 162 - 1, 819 - 1, 984 - 1, 589 - 1, 513 - 1, 495 - 1, 799 - 1, 161 - 1, 604 - 1,
			958 - 1, 533 - 1, 221 - 1, 400 - 1, 386 - 1, 867 - 1, 600 - 1, 782 - 1, 382 - 1, 596 - 1, 414 - 1, 171 - 1, 516 - 1, 375 - 1, 682 - 1, 485 - 1,
			911 - 1, 276 - 1,  98 - 1, 553 - 1, 163 - 1, 354 - 1, 666 - 1, 933 - 1, 424 - 1, 341 - 1, 533 - 1, 870 - 1, 227 - 1, 730 - 1, 475 - 1, 186 - 1,
			263 - 1, 647 - 1, 537 - 1, 686 - 1, 600 - 1, 224 - 1, 469 - 1,  68 - 1, 770 - 1, 919 - 1, 190 - 1, 373 - 1, 294 - 1, 822 - 1, 808 - 1, 206 - 1,
			184 - 1, 943 - 1, 795 - 1, 384 - 1, 383 - 1, 461 - 1, 404 - 1, 758 - 1, 839 - 1, 887 - 1, 715 - 1,  67 - 1, 618 - 1, 276 - 1, 204 - 1, 918 - 1,
			873 - 1, 777 - 1, 604 - 1, 560 - 1, 951 - 1, 160 - 1, 578 - 1, 722 - 1,  79 - 1, 804 - 1,  96 - 1, 409 - 1, 713 - 1, 940 - 1, 652 - 1, 934 - 1,
			970 - 1, 447 - 1, 318 - 1, 353 - 1, 859 - 1, 672 - 1, 112 - 1, 785 - 1, 645 - 1, 863 - 1, 803 - 1, 350 - 1, 139 - 1,  93 - 1, 354 - 1,  99 - 1,
			820 - 1, 908 - 1, 609 - 1, 772 - 1, 154 - 1, 274 - 1, 580 - 1, 184 - 1,  79 - 1, 626 - 1, 630 - 1, 742 - 1, 653 - 1, 282 - 1, 762 - 1, 623 - 1,
			680 - 1,  81 - 1, 927 - 1, 626 - 1, 789 - 1, 125 - 1, 411 - 1, 521 - 1, 938 - 1, 300 - 1, 821 - 1,  78 - 1, 343 - 1, 175 - 1, 128 - 1, 250 - 1,
			170 - 1, 774 - 1, 972 - 1, 275 - 1, 999 - 1, 639 - 1, 495 - 1,  78 - 1, 352 - 1, 126 - 1, 857 - 1, 956 - 1, 358 - 1, 619 - 1, 580 - 1, 124 - 1,
			737 - 1, 594 - 1, 701 - 1, 612 - 1, 669 - 1, 112 - 1, 134 - 1, 694 - 1, 363 - 1, 992 - 1, 809 - 1, 743 - 1, 168 - 1, 974 - 1, 944 - 1, 375 - 1,
			748 - 1,  52 - 1, 600 - 1, 747 - 1, 642 - 1, 182 - 1, 862 - 1,  81 - 1, 344 - 1, 805 - 1, 988 - 1, 739 - 1, 511 - 1, 655 - 1, 814 - 1, 334 - 1,
			249 - 1, 515 - 1, 897 - 1, 955 - 1, 664 - 1, 981 - 1, 649 - 1, 113 - 1, 974 - 1, 459 - 1, 893 - 1, 228 - 1, 433 - 1, 837 - 1, 553 - 1, 268 - 1,
			926 - 1, 240 - 1, 102 - 1, 654 - 1, 459 - 1,  51 - 1, 686 - 1, 754 - 1, 806 - 1, 760 - 1, 493 - 1, 403 - 1, 415 - 1, 394 - 1, 687 - 1, 700 - 1,
			946 - 1, 670 - 1, 656 - 1, 610 - 1, 738 - 1, 392 - 1, 760 - 1, 799 - 1, 887 - 1, 653 - 1, 978 - 1, 321 - 1, 576 - 1, 617 - 1, 626 - 1, 502 - 1,
			894 - 1, 679 - 1, 243 - 1, 440 - 1, 680 - 1, 879 - 1, 194 - 1, 572 - 1, 640 - 1, 724 - 1, 926 - 1,  56 - 1, 204 - 1, 700 - 1, 707 - 1, 151 - 1,
			457 - 1, 449 - 1, 797 - 1, 195 - 1, 791 - 1, 558 - 1, 945 - 1, 679 - 1, 297 - 1,  59 - 1,  87 - 1, 824 - 1, 713 - 1, 663 - 1, 412 - 1, 693 - 1,
			342 - 1, 606 - 1, 134 - 1, 108 - 1, 571 - 1, 364 - 1, 631 - 1, 212 - 1, 174 - 1, 643 - 1, 304 - 1, 329 - 1, 343 - 1,  97 - 1, 430 - 1, 751 - 1,
			497 - 1, 314 - 1, 983 - 1, 374 - 1, 822 - 1, 928 - 1, 140 - 1, 206 - 1,  73 - 1, 263 - 1, 980 - 1, 736 - 1, 876 - 1, 478 - 1, 430 - 1, 305 - 1,
			170 - 1, 514 - 1, 364 - 1, 692 - 1, 829 - 1,  82 - 1, 855 - 1, 953 - 1, 676 - 1, 246 - 1, 369 - 1, 970 - 1, 294 - 1, 750 - 1, 807 - 1, 827 - 1,
			150 - 1, 790 - 1, 288 - 1, 923 - 1, 804 - 1, 378 - 1, 215 - 1, 828 - 1, 592 - 1, 281 - 1, 565 - 1, 555 - 1, 710 - 1,  82 - 1, 896 - 1, 831 - 1,
			547 - 1, 261 - 1, 524 - 1, 462 - 1, 293 - 1, 465 - 1, 502 - 1,  56 - 1, 661 - 1, 821 - 1, 976 - 1, 991 - 1, 658 - 1, 869 - 1, 905 - 1, 758 - 1,
			745 - 1, 193 - 1, 768 - 1, 550 - 1, 608 - 1, 933 - 1, 378 - 1, 286 - 1, 215 - 1, 979 - 1, 792 - 1, 961 - 1,  61 - 1, 688 - 1, 793 - 1, 644 - 1,
			986 - 1, 403 - 1, 106 - 1, 366 - 1, 905 - 1, 644 - 1, 372 - 1, 567 - 1, 466 - 1, 434 - 1, 645 - 1, 210 - 1, 389 - 1, 550 - 1, 919 - 1, 135 - 1,
			780 - 1, 773 - 1, 635 - 1, 389 - 1, 707 - 1, 100 - 1, 626 - 1, 958 - 1, 165 - 1, 504 - 1, 920 - 1, 176 - 1, 193 - 1, 713 - 1, 857 - 1, 265 - 1,
			203 - 1,  50 - 1, 668 - 1, 108 - 1, 645 - 1, 990 - 1, 626 - 1, 197 - 1, 510 - 1, 357 - 1, 358 - 1, 850 - 1, 858 - 1, 364 - 1, 936 - 1, 638 - 1
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Xpk_Bzp2Stream(string agentName, Stream wrapperStream) : base(agentName, wrapperStream)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will decrunch a single chunk of data
		/// </summary>
		/********************************************************************/
		protected override void DecompressImpl(byte[] chunk, byte[] rawData)
		{
			uint blockSize = (uint)((chunk[3] - '0') * 100000);

			using (MemoryStream chunkStream = new MemoryStream(chunk, false))
			{
				ForwardInputStream inputStream = new ForwardInputStream(agentName, chunkStream, 4, (uint)chunk.Length);
				MsbBitReader bitReader = new MsbBitReader(inputStream);

				uint ReadBits(uint count) => bitReader.ReadBits8(count);
				uint ReadBit() => bitReader.ReadBits8(1);

				ForwardOutputStream outputStream = new ForwardOutputStream(agentName, rawData, 0, (uint)rawData.Length);

				// Stream verification
				//
				// There is so much wrong in bzip2 CRC-calculation :(
				// 1. The bit ordering is opposite what everyone else does with CRC32
				// 2. The block CRCs are calculated separately, no way of calculating a complete
				//    CRC without knowing the block layout
				// 3. The CRC is the end of the stream and the stream is bit aligned. You
				//    can't read CRC without decompressing the stream
				uint crc = 0;
				void CalculateBlockCrc(uint blockPos, uint blockSize)
				{
					crc = (crc << 1) | (crc >> 31);
					crc ^= Crc32.Crc32Rev(agentName, rawData, blockPos, blockSize, 0);
				}

				HuffmanDecoder<byte> selectorDecoder = new HuffmanDecoder<byte>(agentName,
					// Incomplete Huffman table. Errors possible
					new HuffmanCode<byte>(1, 0b000000, 0),
					new HuffmanCode<byte>(2, 0b000010, 1),
					new HuffmanCode<byte>(3, 0b000110, 2),
					new HuffmanCode<byte>(4, 0b001110, 3),
					new HuffmanCode<byte>(5, 0b011110, 4),
					new HuffmanCode<byte>(6, 0b111110, 5)
				);

				HuffmanDecoder<int> deltaDecoder = new HuffmanDecoder<int>(agentName,
					new HuffmanCode<int>(1, 0b00, 0),
					new HuffmanCode<int>(2, 0b10, 1),
					new HuffmanCode<int>(2, 0b11, -1)
				);

				byte[] tmpBuffer = new byte[blockSize];

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
					uint blockHdrHigh = ReadBits(32);
					uint blockHdrLow = ReadBits(16);

					if ((blockHdrHigh == 0x31415926) && (blockHdrLow == 0x5359))
					{
						// A block
						//
						// This is rather spaghetti...
						ReadBits(32);		// Block CRC, not interested
						bool randomized = ReadBit() != 0;

						// Basically the random inserted is one LSB after n-th bytes
						// per defined in the table
						uint randomPos = 1;
						uint randomCounter = (uint)(randomTable[0] - 1);

						bool RandomBit()
						{
							// Beauty is in the eye of the beholder: this is smallest form to hide the ugliness
							return (randomCounter-- == 0) ? (randomCounter = randomTable[randomPos++ & 511]) != 0 : false;
						}

						uint currentPtr = ReadBits(24);

						uint currentBlockSize = 0;
						{
							uint numHuffmanItems = 2;
							uint[] hufmannValues = new uint[256];

							{
								// This is just a little bit inefficient but still we reading bit by bit since
								// reference does it. (bitstream format details do not spill over)
								bool[] usedMap = new bool[16];
								for (int i = 0; i < 16; i++)
									usedMap[i] = ReadBit() != 0;

								bool[] huffmanMap = new bool[256];
								for (int i = 0; i < 16; i++)
								{
									for (int j = 0; j < 16; j++)
										huffmanMap[i * 16 + j] = usedMap[i] ? ReadBit() != 0 : false;
								}

								for (int i = 0; i < 256; i++)
								{
									if (huffmanMap[i])
										numHuffmanItems++;
								}

								if (numHuffmanItems == 2)
									throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

								for (uint currentValue = 0, i = 0; i < 256; i++)
								{
									if (huffmanMap[i])
										hufmannValues[currentValue++] = i;
								}
							}

							uint huffmanGroups = ReadBits(3);
							if ((huffmanGroups < 2) || (huffmanGroups > 6))
								throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

							uint selectorsUsed = ReadBits(15);
							if (selectorsUsed == 0)
								throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

							byte[] huffmanSelectorList = new byte[selectorsUsed];

							byte UnMtf(byte value, byte[] map)
							{
								byte ret = map[value];

								if (value != 0)
								{
									byte tmp = map[value];

									for (uint i = value; i != 0; i--)
										map[i] = map[i - 1];

									map[0] = tmp;
								}

								return ret;
							}

							// Create Huffman selectors
							byte[] selectorMtfMap = { 0, 1, 2, 3, 4, 5 };

							for (uint i = 0; i < selectorsUsed; i++)
							{
								byte item = UnMtf(selectorDecoder.Decode(ReadBit), selectorMtfMap);
								if (item >= huffmanGroups)
									throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

								huffmanSelectorList[i] = item;
							}

							HuffmanDecoder<uint>[] dataDecoders = new HuffmanDecoder<uint>[huffmanGroups];

							// Create all tables
							for (uint i = 0; i < huffmanGroups; i++)
							{
								byte[] bitLengths = new byte[258];

								uint currentBits = ReadBits(5);
								for (uint j = 0; j < numHuffmanItems; j++)
								{
									int delta;

									do
									{
										delta = deltaDecoder.Decode(ReadBit);
										currentBits = (uint)(currentBits + delta);
									}
									while (delta != 0);

									if ((currentBits < 1) || (currentBits > 20))
										throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

									bitLengths[j] = (byte)currentBits;
								}

								dataDecoders[i] = new HuffmanDecoder<uint>(agentName);
								dataDecoders[i].CreateOrderlyHuffmanTable(bitLengths, numHuffmanItems);
							}

							// Huffman decode + unRLE + unMTF
							HuffmanDecoder<uint> currentHuffmanDecoder = null;
							uint currentHuffmanIndex = 0;
							byte[] dataMtfMap = new byte[256];

							for (uint i = 0; i < numHuffmanItems - 2; i++)
								dataMtfMap[i] = (byte)i;

							uint currentRunLength = 0;
							uint currentRleWeight = 1;

							void DecodeRle()
							{
								if (currentRunLength != 0)
								{
									if ((currentBlockSize + currentRunLength) > blockSize)
										throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

									for (uint i = 0; i < currentRunLength; i++)
										tmpBuffer[currentBlockSize++] = (byte)hufmannValues[dataMtfMap[0]];
								}

								currentRunLength = 0;
								currentRleWeight = 1;
							}

							for (uint streamIndex = 0; ; streamIndex++)
							{
								if ((streamIndex % 50) == 0)
								{
									if (currentHuffmanIndex >= selectorsUsed)
										throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

									currentHuffmanDecoder = dataDecoders[huffmanSelectorList[currentHuffmanIndex++]];
								}

								uint symbolMtf = currentHuffmanDecoder.Decode(ReadBit);

								// Stop marker is referenced only once, and it is the last one.
								// This means we do not have to un-mtf it for detection
								if (symbolMtf == numHuffmanItems - 1)
									break;

								if (currentBlockSize >= blockSize)
									throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

								if (symbolMtf < 2)
								{
									currentRunLength += currentRleWeight << (int)symbolMtf;
									currentRleWeight <<= 1;
								}
								else
								{
									DecodeRle();
									byte symbol = UnMtf((byte)(symbolMtf - 1), dataMtfMap);

									if (currentBlockSize >= blockSize)
										throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

									tmpBuffer[currentBlockSize++] = (byte)hufmannValues[symbol];
								}
							}

							DecodeRle();
							if (currentPtr >= currentBlockSize)
								throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);
						}

						// Inverse BWT + final RLE decoding.
						//
						// There are a few dark corners here as well
						// 1. Can the stream end at 4 literals without count? I assume it is a valid optimization (and that this does not spillover to next block)
						// 2. Can the RLE-step include counts 252 to 255 even if reference does not do them? I assume yes here as well
						// 3. Can the stream be empty? We do not take issue here about that (that should be culled out earlier already)
						uint[] sums = new uint[256];
						for (uint i = 0; i < currentBlockSize; i++)
							sums[tmpBuffer[i]]++;

						uint[] rank = new uint[256];
						for (uint tot = 0, i = 0; i < 256; i++)
						{
							rank[i] = tot;
							tot += sums[i];
						}

						// Not at all happy about the memory consumption, but it simplifies the implementation a lot
						// and by sacrificing 4 * size (size as in actual block size) we do not have to slow search nor another temporary buffer
						// since by calculating forward table we can do forward decoding of the data on the same pass as iBWT.
						//
						// Also, because I'm lazy
						uint[] forwardIndex = new uint[currentBlockSize];
						for (uint i = 0; i < currentBlockSize; i++)
							forwardIndex[rank[tmpBuffer[i]]++] = i;

						// Output + final RLE decoding
						byte currentCh = 0;
						uint currentChCount = 0;

						void OutputByte(byte ch)
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
									void OutputBlock(uint count)
									{
										for (uint i = 0; i < count; i++)
											outputStream.WriteByte(currentCh);
									}

									if (currentChCount == 4)
									{
										OutputBlock((uint)(ch + 4));
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

						uint destOffsetStart = outputStream.GetOffset();

						// And now the final iBWT + unRLE is easy...
						for (uint i = 0; i < currentBlockSize; i++)
						{
							currentPtr = forwardIndex[currentPtr];
							OutputByte(tmpBuffer[currentPtr]);
						}

						// Cleanup the state, a bit hackish way to do it
						if (currentChCount != 0)
							OutputByte((byte)(currentChCount == 4 ? 0 : ~currentCh));

						CalculateBlockCrc(destOffsetStart, outputStream.GetOffset() - destOffsetStart);
					}
					else if ((blockHdrHigh == 0x17724538) && (blockHdrLow == 0x5090))
					{
						// End of blocks
						uint rawCrc = ReadBits(32);
						if (crc != rawCrc)
							throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_WRONG_BLOCK_CHECKSUM);

						break;
					}
					else
						throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);
				}

				if (rawData.Length != outputStream.GetOffset())
					throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);
			}
		}
	}
}
