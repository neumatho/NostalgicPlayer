/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.LibAncient.Common;
using Polycode.NostalgicPlayer.Ports.LibAncient.Common.Buffers;
using Polycode.NostalgicPlayer.Ports.LibAncient.Exceptions;
using Buffer = Polycode.NostalgicPlayer.Ports.LibAncient.Common.Buffers.Buffer;

namespace Polycode.NostalgicPlayer.Ports.LibAncient.Internal.Decompressors
{
	/// <summary>
	/// StoneCracker decompressor
	/// </summary>
	internal class StoneCrackerDecompressor : Decompressor
	{
		private readonly Buffer packedData;

		private readonly uint32_t generation;
		private uint32_t dataOffset;

		private uint32_t rleSize;
		private uint32_t rawSize;
		private uint32_t packedSize;

		private readonly uint8_t[] modes = new uint8_t[4];
		private readonly uint8_t[] rle = new uint8_t[3];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private StoneCrackerDecompressor(Buffer packedData, bool exactSizeKnown) : base(DecompressorType.StoneCracker)
		{
			this.packedData = packedData;

			uint32_t hdr = packedData.ReadBe32(0);
			if (!DetectHeaderAndGeneration(hdr, out generation))
				throw new InvalidFormatException();

			bool initialized = false;

			// This is a fallback if we have accidentally identified the wrong version
			if (generation == 2)
			{
				try
				{
					Initialize(packedData, hdr);
					initialized = true;
				}
				catch (Exception)
				{
					generation = 1;
				}
			}

			if (!initialized)
				Initialize(packedData, hdr);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool DetectHeader(uint32_t hdr, uint32_t footer)
		{
			return DetectHeaderAndGeneration(hdr, out _);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public new static Decompressor Create(Buffer packedData, bool exactSizeKnown)
		{
			return new StoneCrackerDecompressor(packedData, exactSizeKnown);
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
			if (rawSize == 0)
				yield break;

			uint8_t[] outputData = new uint8_t[rawSize];
			WrappedArrayBuffer rawData = new WrappedArrayBuffer(outputData);

			// The algorithms are all simple LZ compressors. However they will differ from version to version
			// on which order they read data, how much and on some other details
			// some of them can be nicely combined, others not so.
			//
			// To the creators credit, you can see generally improving performance as the versions increase
			// Although it is still limited by the simple nature of the implementation and does not really
			// compare to the more generic data compression algorithms
			switch (generation)
			{
				case 1:
				{
					DecompressGen1(rawData);
					break;
				}

				case 2:
				case 3:
				{
					DecompressGen23(rawData);
					break;
				}

				case 4:
				case 5:
				case 6:
				{
					DecompressGen456(rawData);
					break;
				}

				case 7:
				{
					DecompressGen7(rawData);
					break;
				}

				case 8:
				{
					DecompressGen8(rawData);
					break;
				}

				default:
					throw new DecompressionException();
			}

			yield return outputData;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool DetectHeaderAndGeneration(uint32_t hdr, out uint32_t generation)
		{
			// StoneCracker 2.71 (and others from 2.69 - 2.8 series) do not have any sensible identification value
			// first 3 bytes are constants for RLE-encoder (byte values for RLE-encoder, at least they are unique)
			// last byte is bit length.
			//
			// 2.92 and 2.99 do not have either any proper identification word either, however its
			// bit lengths for decompressor are stored in the first 4 bytes, which forms identifiable
			// value.
			//
			// Thus for detecting 2.71 and friends, we are creating lots of false positives here.
			// At least we can rule those out later when detecting the actual header content
			// Final complication is that values for 2.71/2.9X overlap, this we need to handle
			// later as well
			//
			// Thomas Neumann: Because the possibility of false positives, I have decides to comment out
			// the detection of these versions
/*			if ((hdr >= 0x08090a08) && (hdr <= 0x08090a0e) && (hdr != 0x08090a09))
			{
				// Can be generation 1 as well. Needs to be determined later
				generation = 2;
				return true;
			}

			if (((hdr & 0xff) >= 0x08) && ((hdr & 0xff) <= 0x0e))
			{
				uint8_t byte0 = (uint8_t)(hdr >> 24);
				uint8_t byte1 = (uint8_t)(hdr >> 16);
				uint8_t byte2 = (uint8_t)(hdr >> 8);

				// Only limiter I can think of apart from the last byte is that the rle-bytes are unique
				if ((byte0 != byte1) && (byte0 != byte2) && (byte1 != byte2))
				{
					generation = 1;
					return true;
				}
			}
*/
			// Specials
			generation = (hdr & 0xffff_ff00) switch
			{
				var h when h == Common.Common.FourCC("1AM\0") => 3,
				var h when h == Common.Common.FourCC("2AM\0") => 6,
				_ => 0
			};

			if (generation != 0)
				return true;

			// From 3.00 and onwards we can be certain of the format
			generation = hdr switch
			{
				var h when h == Common.Common.FourCC("S300") => 3,
				var h when h == Common.Common.FourCC("S310") => 4,
				var h when h == Common.Common.FourCC("S400") => 5,
				var h when h == Common.Common.FourCC("S401") => 6,
				var h when h == Common.Common.FourCC("S403") => 7,
				var h when h == Common.Common.FourCC("Z&G!") => 7,		// Switchback / Rebels
				var h when h == Common.Common.FourCC("ZULU") => 7,		// Whammer Slammer / Rebels
				var h when h == Common.Common.FourCC("S404") => 8,
				var h when h == Common.Common.FourCC("AYS!") => 8,		// High Anxiety / Abyss
				_ => 0
			};

			return generation != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Initialize(Buffer packedData, uint32_t hdr)
		{
			void ReadModes(uint32_t value)
			{
				for (uint32_t i = 0; i < 4; i++)
				{
					modes[i] = (uint8_t)(value >> 24);

					if ((modes[i] < 8) || (modes[i] > 14))
						throw new InvalidFormatException();

					value <<= 8;
				}
			}

			switch (generation)
			{
				case 1:
				{
					dataOffset = 18;

					rle[0] = (uint8_t)(hdr >> 24);
					rle[1] = (uint8_t)(hdr >> 16);
					rle[2] = (uint8_t)(hdr >> 8);

					modes[0] = (uint8_t)hdr;

					if (packedData.Size() < dataOffset)
						throw new InvalidFormatException();

					for (uint32_t i = 1; i < 3; i++)
					{
						modes[i] = packedData.Read8(i + 15);

						if ((modes[i] < 4) || (modes[i] > 7))
							throw new InvalidFormatException();
					}

					rleSize = packedData.ReadBe32(4);
					rawSize = packedData.ReadBe32(8);
					packedSize = packedData.ReadBe32(12);
					break;
				}

				case 2:
				{
					ReadModes(hdr);
					goto case 6;
				}

				case 4:
				case 5:
				case 6:
				{
					dataOffset = 12;

					if (packedData.Size() < dataOffset)
						throw new InvalidFormatException();

					rawSize = packedData.ReadBe32(4);
					packedSize = packedData.ReadBe32(8);
					break;
				}

				case 3:
				{
					dataOffset = 16;

					if (packedData.Size() < dataOffset)
						throw new InvalidFormatException();

					ReadModes(packedData.ReadBe32(4));
					rawSize = packedData.ReadBe32(8);
					packedSize = packedData.ReadBe32(12);
					break;

				}

				case 7:
				case 8:
				{
					dataOffset = 16;

					if (packedData.Size() < (dataOffset + 2))
						throw new InvalidFormatException();

					rawSize = packedData.ReadBe32(8);
					packedSize = packedData.ReadBe32(12) + 2;
					break;
				}

				default:
					throw new InvalidFormatException();
			}

			if ((packedSize > GetMaxPackedSize()) || (rawSize > GetMaxRawSize()))
				throw new InvalidFormatException();

			// Final sanity checks on old formats, especially on 2.71 which can still be false positive
			// For a sanity check we assume the compressor is actually compressing, which is not an exactly wrong thing to do
			// (of course there could be expanding files, but it is doubtful someone would actually use them)
			// Also, the compressor seem to crash on large files. Lets cap the filesize to 1M
			if ((generation == 1) && ((rleSize > rawSize) || (packedSize > rleSize) || (rawSize > 1048_576)))
				throw new InvalidFormatException();

			packedSize += dataOffset;

			if (packedSize > packedData.Size())
				throw new InvalidFormatException();
		}



		/********************************************************************/
		/// <summary>
		/// v2.71
		/// </summary>
		/********************************************************************/
		private void DecompressGen1(Buffer rawData)
		{
			BackwardInputStream inputStream = new BackwardInputStream(packedData, dataOffset, packedSize);
			MsbBitReader bitReader = new MsbBitReader(inputStream);

			MemoryBuffer tmpBuffer = new MemoryBuffer(rleSize);
			BackwardOutputStream outputStream = new BackwardOutputStream(tmpBuffer, 0, rleSize);

			uint32_t ReadBits(uint32_t count)
			{
				return bitReader.ReadBitsBe32(count);
			}

			uint32_t ReadBit()
			{
				return bitReader.ReadBitsBe32(1);
			}

			// Anchor-bit handling
			{
				uint32_t value = inputStream.ReadBE32();
				uint32_t tmp = value;
				uint32_t count = 0;

				while (tmp != 0)
				{
					tmp <<= 1;
					count++;
				}

				if (count != 0)
					count--;

				if (count != 0)
					bitReader.Reset(value >> (int)(32 - count), (uint8_t)count);
			}

			VariableLengthCodeDecoder countVlcDecoder = new VariableLengthCodeDecoder(3, modes[2] + 1, -1, 0, 0, 0, modes[1] + 1);
			uint8_t[] distanceBits = [0, 0, 0, 8, 9, 10, (uint8_t)(modes[0] + 1)];

			HuffmanDecoder<uint8_t> scDecoder = new HuffmanDecoder<uint8_t>(
				new HuffmanCode<uint8_t>(2, 0b000, 0x80),
				new HuffmanCode<uint8_t>(3, 0b010, 0x81),
				new HuffmanCode<uint8_t>(3, 0b011, 6),
				new HuffmanCode<uint8_t>(2, 0b010, 3),
				new HuffmanCode<uint8_t>(3, 0b110, 4),
				new HuffmanCode<uint8_t>(3, 0b111, 5)
			);

			while (!outputStream.Eof)
			{
				uint8_t index = scDecoder.Decode(ReadBit);
				uint32_t count = countVlcDecoder.Decode(ReadBits, index & 0x7fU);

				if ((index & 0x80) != 0)
				{
					if (count == 0)
						count = 8;		// Why?!?

					for (uint32_t i = 0; i < count; i++)
						outputStream.WriteByte((uint8_t)ReadBits(8));
				}
				else
				{
					uint32_t distance = ReadBits(distanceBits[index]);
					outputStream.Copy(distance, count);
				}
			}

			ForwardInputStream finalInputStream = new ForwardInputStream(tmpBuffer, 0, rleSize);
			ForwardOutputStream finalOutputStream = new ForwardOutputStream(rawData, 0, rawSize);

			while (!finalOutputStream.Eof)
			{
				uint8_t cmd = finalInputStream.ReadByte();

				if (cmd == rle[0])
				{
					uint32_t count = finalInputStream.ReadByte() + 1U;

					if (count == 1)
						finalOutputStream.WriteByte(cmd);
					else
					{
						uint8_t value = finalInputStream.ReadByte();

						for (uint32_t i = 0; i <= count; i++)
							finalOutputStream.WriteByte(value);
					}
				}
				else if (cmd == rle[1])
				{
					uint32_t count = finalInputStream.ReadByte() + 1U;

					if (count == 1)
						finalOutputStream.WriteByte(cmd);
					else
					{
						for (uint32_t i = 0; i <= count; i++)
							finalOutputStream.WriteByte(rle[2]);
					}
				}
				else
					finalOutputStream.WriteByte(cmd);
			}
		}



		/********************************************************************/
		/// <summary>
		/// v2.92, v2.99
		/// v3.00
		/// </summary>
		/********************************************************************/
		private void DecompressGen23(Buffer rawData)
		{
			BackwardInputStream inputStream = new BackwardInputStream(packedData, dataOffset, packedSize);
			LsbBitReader bitReader = new LsbBitReader(inputStream);

			BackwardOutputStream outputStream = new BackwardOutputStream(rawData, 0, rawSize);

			// Anchor-bit handling
			{
				uint32_t value = inputStream.ReadBE32();

				if (value != 0)
				{
					for (int32_t i = 31; i > 0; i--)
					{
						if ((value & (1 << i)) != 0)
						{
							bitReader.Reset((uint32_t)(value & ((1 << i) - 1)), (uint8_t)i);
							break;
						}
					}
				}
			}

			uint32_t ReadBits(uint32_t count)
			{
				return bitReader.ReadBitsBe32(count);
			}

			uint32_t ReadBit()
			{
				return bitReader.ReadBitsBe32(1);
			}

			uint32_t ReadCount(uint32_t threshold, uint32_t bits)
			{
				uint32_t ret = 0;
				uint32_t tmp;

				do
				{
					tmp = Common.Common.RotateBits(ReadBits(bits), bits);
					ret += tmp;
				}
				while (tmp == threshold);

				return ret;
			}

			bool gen3 = generation >= 3;

			while (!outputStream.Eof)
			{
				if (ReadBit() != 0)
				{
					uint32_t count = ReadCount(7, 3);
					if (gen3)
						count++;

					// For 2.92 zero count could meant count of 65536
					// For 2.99 it would be 4G
					// Nevertheless it is an error
					// 3.00 fixes this by +1
					if (count == 0)
						throw new DecompressionException();

					for (uint32_t i = 0; i < count; i++)
						outputStream.WriteByte((uint8_t)ReadBits(8));
				}
				else
				{
					uint32_t mode = Common.Common.RotateBits(ReadBits(2), 2);
					uint32_t count;
					uint32_t modeBits = modes[mode] + 1U;

					if (mode == 3)
					{
						if (ReadBit() != 0)
						{
							count = ReadCount(7, 3) + 5;
							if (gen3)
								modeBits = 8;
						}
						else
							count = ReadCount(127, 7) + (gen3 ? 5U : 19U);
					}
					else
						count = mode + 2;

					uint32_t distance = Common.Common.RotateBits(ReadBits(modeBits), modeBits) + 1;
					outputStream.Copy(distance, count);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// v3.10, v3.11b
		/// Pads output file, and this new size is also defined as rawSize
		/// in header!
		///
		/// pre v4.00, v4.01
		/// </summary>
		/********************************************************************/
		private void DecompressGen456(Buffer rawData)
		{
			ForwardInputStream inputStream = new ForwardInputStream(packedData, dataOffset, packedSize);
			MsbBitReader bitReader = new MsbBitReader(inputStream);

			ForwardOutputStream outputStream = new ForwardOutputStream(rawData, 0, rawSize);

			uint32_t ReadBits(uint32_t count)
			{
				if (generation == 4)
					return bitReader.ReadBitsBe32(count);

				return bitReader.ReadBitsBe16(count);
			}

			uint32_t ReadBit()
			{
				return ReadBits(1);
			}

			uint32_t ReadCount(uint32_t threshold, uint32_t bits)
			{
				uint32_t ret = 0;
				uint32_t tmp;

				do
				{
					tmp = ReadBits(bits);
					ret += tmp;
				}
				while (tmp == threshold);

				return ret;
			}

			uint8_t[] modes = [10, 11, 12, 8];

			while (!outputStream.Eof)
			{
				if (ReadBit() != 0)
				{
					uint32_t mode = ReadBits(2);
					uint32_t distance = ReadBits(modes[mode]);

					// Will obviously throw if distance is 0
					uint32_t count;

					if (mode >= 2)
					{
						if (generation == 4)
							count = ReadCount(15, 4) + 3;
						else
							count = ReadCount(7, 3) + 3;
					}
					else
						count = mode + 3;

					// Yet another bug
					if (count > (rawSize - outputStream.GetOffset()))
						count = (uint32_t)(rawSize - outputStream.GetOffset());
					
					outputStream.Copy(distance, count);
				}
				else
				{
					uint32_t count = ReadCount(7, 3);

					// A regression in 3.10 that is not fixed until 4.01
					if (generation == 6)
						count++;

					if (count == 0)
						throw new DecompressionException();

					for (uint32_t i = 0; i < count; i++)
						outputStream.WriteByte((uint8_t)ReadBits(8));
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// v4.02a
		/// </summary>
		/********************************************************************/
		private void DecompressGen7(Buffer rawData)
		{
			BackwardInputStream inputStream = new BackwardInputStream(packedData, dataOffset, packedSize - 2);
			LsbBitReader bitReader = new LsbBitReader(inputStream);

			BackwardOutputStream outputStream = new BackwardOutputStream(rawData, 0, rawSize);

			// Incomplete value at start
			{
				uint16_t bitCount = packedData.ReadBe16(packedSize - 2);
				if (bitCount > 16)
					throw new DecompressionException();

				uint16_t value = inputStream.ReadBE16();
				bitReader.Reset(value, (uint8_t)(bitCount & 0xff));
			}

			uint32_t ReadBits(uint32_t count)
			{
				return bitReader.ReadBitsBe16(count);
			}

			uint32_t ReadBit()
			{
				return bitReader.ReadBitsBe16(1);
			}

			VariableLengthCodeDecoder vlcDecoder = new VariableLengthCodeDecoder(5, 8, 10, 12);

			while (!outputStream.Eof)
			{
				if (ReadBit() != 0)
				{
					uint32_t distance = vlcDecoder.Decode(ReadBits, ReadBits(2)) + 1;

					// Christmas tree!
					uint32_t count = 2;

					if (ReadBit() == 0)
					{
						count++;

						if (ReadBit() == 0)
						{
							count++;

							if (ReadBit() == 0)
							{
								count++;
								uint32_t tmp;

								do
								{
									tmp = ReadBits(3);
									count += tmp;
								}
								while (tmp == 7);
							}
						}
					}
					
					outputStream.Copy(distance, count);
				}
				else
					outputStream.WriteByte((uint8_t)ReadBits(8));
			}
		}



		/********************************************************************/
		/// <summary>
		/// v4.10
		/// </summary>
		/********************************************************************/
		private void DecompressGen8(Buffer rawData)
		{
			BackwardInputStream inputStream = new BackwardInputStream(packedData, dataOffset, packedSize - 2);
			MsbBitReader bitReader = new MsbBitReader(inputStream);

			BackwardOutputStream outputStream = new BackwardOutputStream(rawData, 0, rawSize);

			// Incomplete value at start
			uint16_t modeBits;
			{
				uint16_t bitCount = (uint16_t)(packedData.ReadBe16(packedSize - 2) & 15);
				uint16_t value = inputStream.ReadBE16();
				bitReader.Reset((uint32_t)(value >> (16 - bitCount)), (uint8_t)(bitCount & 0xff));

				modeBits = inputStream.ReadBE16();
				if ((modeBits < 10) || (modeBits > 14))
					throw new DecompressionException();
			}

			uint32_t ReadBits(uint32_t count)
			{
				return bitReader.ReadBitsBe16(count);
			}

			uint32_t ReadBit()
			{
				return bitReader.ReadBitsBe16(1);
			}

			VariableLengthCodeDecoder countVlcDecoder = new VariableLengthCodeDecoder(0, 1, 2, 3, 2, 1, 0, 8, -3, 2, 0, 5);
			VariableLengthCodeDecoder distanceVlcDecoder = new VariableLengthCodeDecoder(5, 9, modeBits);

			HuffmanDecoder<uint8_t> countDecoder = new HuffmanDecoder<uint8_t>(
				new HuffmanCode<uint8_t>(1, 0b00000000, 0x80),
				new HuffmanCode<uint8_t>(2, 0b00000011, 1),
				new HuffmanCode<uint8_t>(3, 0b00000101, 2),
				new HuffmanCode<uint8_t>(4, 0b00001000, 7),
				new HuffmanCode<uint8_t>(5, 0b00010010, 3),
				new HuffmanCode<uint8_t>(6, 0b00100110, 4),
				new HuffmanCode<uint8_t>(7, 0b01001110, 5),
				new HuffmanCode<uint8_t>(8, 0b10011110, 6),
				new HuffmanCode<uint8_t>(8, 0b10011111, 0x8b)
			);

			HuffmanDecoder<uint8_t> distanceDecoder = new HuffmanDecoder<uint8_t>(
				new HuffmanCode<uint8_t>(1, 0b01, 2),
				new HuffmanCode<uint8_t>(2, 0b00, 1),
				new HuffmanCode<uint8_t>(2, 0b01, 0)
			);

			while (!outputStream.Eof)
			{
				uint8_t countIndex = countDecoder.Decode(ReadBit);
				uint32_t count = countVlcDecoder.Decode(ReadBits, countIndex & 0x7fU) + 1;
				uint32_t tmp;

				if (count == 278)
				{
					do
					{
						tmp = ReadBits(8);
						count += tmp;
					}
					while (tmp == 0xff);
				}

				if ((countIndex & 0x80) != 0)
				{
					for (uint32_t i = 0; i < count; i++)
						outputStream.WriteByte((uint8_t)ReadBits(8));
				}
				else
				{
					uint32_t distance = distanceVlcDecoder.Decode(ReadBits, distanceDecoder.Decode(ReadBit)) + 1;
					outputStream.Copy(distance, count);
				}
			}
		}
		#endregion
	}
}
