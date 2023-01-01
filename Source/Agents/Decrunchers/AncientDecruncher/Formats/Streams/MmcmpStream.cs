/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats.Streams
{
	/// <summary>
	/// This stream read data crunched with MMCMP
	/// </summary>
	internal class MmcmpStream : AncientStream
	{
		private static readonly byte[] valueThresholds8 = new byte[8]
		{
			0x1, 0x3, 0x7, 0xf, 0x1e, 0x3c, 0x78, 0xf8
		};

		private static readonly byte[] extraBits8 = new byte[8]
		{
			3, 3, 3, 3,  2, 1, 0, 0
		};

		private static readonly ushort[] valueThresholds16 = new ushort[16]
		{
			0x1, 0x3, 0x7, 0xf, 0x1e, 0x3c, 0x78, 0xf0,
			0x1f0, 0x3f0, 0x7f0, 0xff0, 0x1ff0, 0x3ff0, 0x7ff0, 0xfff0
		};

		private static readonly byte[] extraBits16 = new byte[16]
		{
			4, 4, 4, 4,  3, 2, 1, 0,  0, 0, 0, 0,  0, 0, 0, 0
		};

		private readonly uint crunchedSize;
		private readonly uint rawSize;
		private readonly uint blocksOffset;
		private readonly uint blocks;
		private readonly ushort version;

		private readonly uint[] blockArray;
		private readonly uint[] blockOrder;
		private int blockNumber;

		private byte[] rawData = null;
		private int bufferIndex;

		private readonly SortedDictionary<uint, byte[]> decompressedSubBlocks;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MmcmpStream(string agentName, Stream wrapperStream) : base(agentName, wrapperStream)
		{
			using (ReaderStream readerStream = new ReaderStream(wrapperStream, true))
			{
				readerStream.Seek(10, SeekOrigin.Begin);

				version = readerStream.Read_L_UINT16();
				blocks = readerStream.Read_L_UINT16();
				rawSize = readerStream.Read_L_UINT32();
				blocksOffset = readerStream.Read_L_UINT32();

				// Check for overflow
				if (OverflowCheck.Sum(blocksOffset, blocks * 4) > readerStream.Length)
					throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

				// Read all the blocks
				blockArray = new uint[blocks];

				readerStream.Seek(blocksOffset, SeekOrigin.Begin);
				readerStream.ReadArray_L_UINT32s(blockArray, 0, (int)blocks);

				// Calculate the packed size and find the order of the blocks
				SortedDictionary<uint, uint> blockOffsets = new SortedDictionary<uint, uint>();

				crunchedSize = 0;
				for (uint i = 0; i < blocks; i++)
				{
					uint blockAddr = blockArray[i];
					if (OverflowCheck.Sum(blockAddr, 20) >= readerStream.Length)
						throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

					readerStream.Seek(blockAddr + 4, SeekOrigin.Begin);
					uint blockSize = readerStream.Read_L_UINT32();
					readerStream.Seek(4, SeekOrigin.Current);
					blockSize += (uint)readerStream.Read_L_UINT16() * 8 + 20;
					crunchedSize = Math.Max(crunchedSize, OverflowCheck.Sum(blockAddr, blockSize));

					readerStream.Seek(6, SeekOrigin.Current);
					uint offset = readerStream.Read_L_UINT32();
					blockOffsets[offset] = i;
				}

				if (crunchedSize > readerStream.Length)
					throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

				blockOrder = blockOffsets.Values.ToArray();
			}

			decompressedSubBlocks = new SortedDictionary<uint, byte[]>();
			blockNumber = 0;
		}

		#region Stream implementation
		/********************************************************************/
		/// <summary>
		/// Read data from the stream
		/// </summary>
		/********************************************************************/
		public override int Read(byte[] buffer, int offset, int count)
		{
			int taken = 0;

			while (count > 0)
			{
				if (rawData == null)
				{
					// Has we reached the end of the file
					if (blockNumber == blocks)
					{
						if (decompressedSubBlocks.Count == 0)
							break;

						// Still some sub-blocks left, so return the next one from there
						KeyValuePair<uint, byte[]> firstBlock = decompressedSubBlocks.First();
						decompressedSubBlocks.Remove(firstBlock.Key);

						rawData = firstBlock.Value;
					}
					else
					{
						// Read next block and decompress it
						rawData = Decompress(blockNumber);
					}

					bufferIndex = 0;
				}

				// Copy decrunched data into the read buffer
				int todo = Math.Min(count, rawData.Length - bufferIndex);
				Array.Copy(rawData, bufferIndex, buffer, offset, todo);

				count -= todo;
				taken += todo;
				bufferIndex += todo;
				offset += todo;

				if (bufferIndex == rawData.Length)
					rawData = null;
			}

			return taken;
		}
		#endregion

		#region DecruncherStream overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the decrunched data
		/// </summary>
		/********************************************************************/
		protected override int GetDecrunchedLength()
		{
			return (int)rawSize;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Decrunch a single block and return it
		/// </summary>
		/********************************************************************/
		private byte[] Decompress(int block)
		{
			uint blockAddr = blockArray[blockOrder[block]];

			using (ReaderStream readerStream = new ReaderStream(wrapperStream, true))
			{
				// Start to seek to the beginning of the block
				readerStream.Seek(blockAddr, SeekOrigin.Begin);

				// Read block header
				uint unpackedBlockSize = readerStream.Read_L_UINT32();
				uint packedBlockSize = readerStream.Read_L_UINT32();
				uint fileChecksum = readerStream.Read_L_UINT32();
				uint subBlocks = readerStream.Read_L_UINT16();
				ushort flags = readerStream.Read_L_UINT16();

				uint packTableSize = readerStream.Read_L_UINT16();
				if (packTableSize >= packedBlockSize)
					throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

				byte[] rawData = null;

				ushort bitCount = readerStream.Read_L_UINT16();

				if (decompressedSubBlocks.Count > 0)
				{
					// Read the first subblock output offset
					uint offset = readerStream.Read_L_UINT32();

					// If the current block offset is higher than an already decompressed block,
					// ignore the current block for now
					KeyValuePair<uint, byte[]> firstBlock = decompressedSubBlocks.First();
					if (offset > firstBlock.Key)
					{
						decompressedSubBlocks.Remove(firstBlock.Key);
						return firstBlock.Value;
					}
				}

				// Increment the current block number
				blockNumber++;

				// Read the whole block into memory for optimization
				byte[] compressedData = new byte[OverflowCheck.Sum(subBlocks * 8, 20, packTableSize, packedBlockSize)];

				readerStream.Seek(blockAddr, SeekOrigin.Begin);
				readerStream.Read(compressedData, 0, compressedData.Length);

				using (MemoryStream ms = new MemoryStream(compressedData))
				{
					ForwardInputStream inputStream = new ForwardInputStream(agentName, ms, OverflowCheck.Sum(/*blockAddr,*/ subBlocks * 8,  20, packTableSize), OverflowCheck.Sum(/*blockAddr,*/ subBlocks * 8, 20, packedBlockSize));
					LsbBitReader bitReader = new LsbBitReader(inputStream);

					uint ReadBits(uint count) => bitReader.ReadBits8(count);

					uint currentSubBlock = 0;
					uint outputOffset = 0, outputSize = 0;

					void ReadNextSubBlock()
					{
						if (currentSubBlock >= subBlocks)
							throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

						readerStream.Seek(blockAddr + currentSubBlock * 8 + 20, SeekOrigin.Begin);
						outputOffset = readerStream.Read_L_UINT32();
						outputSize = readerStream.Read_L_UINT32();

						if (OverflowCheck.Sum(outputOffset, outputSize) > rawSize)
							throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

						rawData = new byte[outputSize];

						// Remember this in our decompressed block list
						decompressedSubBlocks[outputOffset] = rawData;

						// Reset the output offset to 0, since we have a new buffer holding this sub-block only
						outputOffset = 0;

						currentSubBlock++;
					}

					uint checksum = 0, checksumPartial = 0, checksumRot = 0;

					void WriteByte(byte value, bool allowOverrun = false)
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
						for (uint j = 0; j < packedBlockSize; j++)
							WriteByte(inputStream.ReadByte());
					}
					else if ((flags & 4) == 0)
					{
						// 8 bit compression

						// In case the bit-count is not enough to store a value, symbol at the end
						// of the codemap is created and this marks as a new bitCount
						if (bitCount >= 8)
							throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

						byte[] oldValue = { 0, 0 };
						uint chIndex = 0;
						uint tablePtr = blockAddr + subBlocks * 8 + 20;

						for (uint j = 0; j < unpackedBlockSize;)
						{
							byte value = (byte)ReadBits((uint)bitCount + 1);
							if (value >= valueThresholds8[bitCount])
							{
								uint newBitCount = ReadBits(extraBits8[bitCount]) + (uint)((value - valueThresholds8[bitCount]) << extraBits8[bitCount]);
								if (bitCount != newBitCount)
								{
									bitCount = (ushort)(newBitCount & 0x7);
									continue;
								}

								value = (byte)(0xf8 + ReadBits(3));
								if ((value == 0xff) && (ReadBits(1) != 0))
									break;
							}

							if (value >= packTableSize)
								throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

							readerStream.Seek(tablePtr + value, SeekOrigin.Begin);
							value = readerStream.Read_UINT8();

							if ((flags & 2) != 0)
							{
								// Delta
								value += oldValue[chIndex];
								oldValue[chIndex] = value;

								if ((flags & 0x100) != 0)	// Stereo
									chIndex ^= 1;
							}

							WriteByte(value);
							j++;
						}
					}
					else
					{
						// 16 bit compression
						if (bitCount >= 16)
							throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

						short[] oldValue = { 0, 0 };
						uint chIndex = 0;

						for (uint j = 0; j < unpackedBlockSize;)
						{
							int value = (int)ReadBits((uint)bitCount + 1);
							if (value >= valueThresholds16[bitCount])
							{
								uint newBitCount = ReadBits(extraBits16[bitCount]) + (uint)((value - valueThresholds16[bitCount]) << extraBits16[bitCount]);
								if (bitCount != newBitCount)
								{
									bitCount = (ushort)(newBitCount & 0xf);
									continue;
								}

								value = (int)(0xfff0 + ReadBits(4));
								if ((value == 0xffff) && (ReadBits(1) != 0))
									break;
							}

							if ((value & 1) != 0)
								value = -value - 1;

							value >>= 1;

							if ((flags & 2) != 0)
							{
								// Delta
								value += oldValue[chIndex];
								oldValue[chIndex] = (short)value;

								if ((flags & 0x100) != 0)	// Stereo
									chIndex ^= 1;
							}
							else if ((flags & 0x200) == 0)	// abs16
								value ^= 0x8000;

							if ((flags & 0x400) != 0)
							{
								// Big endian
								WriteByte((byte)(value >> 8));
								WriteByte((byte)value, true);
							}
							else
							{
								// Little endian
								WriteByte((byte)value);
								WriteByte((byte)(value >> 8), true);
							}

							j += 2;
						}
					}

					if (checksum != fileChecksum)
						throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_WRONG_BLOCK_CHECKSUM);
				}

				// Return the first sub-block in the decompressed list
				KeyValuePair<uint, byte[]> returnBlock = decompressedSubBlocks.First();
				decompressedSubBlocks.Remove(returnBlock.Key);

				return returnBlock.Value;
			}
		}
		#endregion
	}
}
