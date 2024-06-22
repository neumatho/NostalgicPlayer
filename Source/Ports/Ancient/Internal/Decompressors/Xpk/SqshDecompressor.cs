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
	/// XPK-SQSH decompressor
	/// </summary>
	internal class SqshDecompressor : XpkDecompressor
	{
		private static readonly uint8_t[,] bitLengthTable = new uint8_t[7, 8]
		{
			{ 2, 3, 4, 5, 6, 7, 8, 0 },
			{ 3, 2, 4, 5, 6, 7, 8, 0 },
			{ 4, 3, 5, 2, 6, 7, 8, 0 },
			{ 5, 4, 6, 2, 3, 7, 8, 0 },
			{ 6, 5, 7, 2, 3, 4, 8, 0 },
			{ 7, 6, 8, 2, 3, 4, 5, 0 },
			{ 8, 7, 6, 2, 3, 4, 5, 0 }
		};

		private readonly Buffer packedData;
		private readonly uint32_t rawSize;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private SqshDecompressor(uint32_t hdr, Buffer packedData)
		{
			this.packedData = packedData;

			if (!DetectHeaderXpk(hdr) || (packedData.Size() < 3))
				throw new InvalidFormatException();

			rawSize = packedData.ReadBe16(0);
			if (rawSize == 0)
				throw new InvalidFormatException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool DetectHeaderXpk(uint32_t hdr)
		{
			return hdr == Common.Common.FourCC("SQSH");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static XpkDecompressor Create(uint32_t hdr, Buffer packedData, ref State state)
		{
			return new SqshDecompressor(hdr, packedData);
		}



		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		public override void DecompressImpl(Buffer rawData, List<Buffer> previousBuffers)
		{
			if (rawData.Size() != rawSize)
				throw new DecompressionException();

			ForwardInputStream inputStream = new ForwardInputStream(packedData, 2, packedData.Size());
			MsbBitReader bitReader = new MsbBitReader(inputStream);

			uint32_t ReadBits(uint32_t count) => bitReader.ReadBits8(count);

			int32_t ReadSignedBits(uint8_t bits)
			{
				int32_t ret = (int32_t)ReadBits(bits);
				if ((ret & (1 << (bits - 1))) != 0)
					ret |= ~0 << bits;

				return ret;
			}

			uint32_t ReadBit() => bitReader.ReadBits8(1);
			uint8_t ReadByte() => inputStream.ReadByte();

			ForwardOutputStream outputStream = new ForwardOutputStream(rawData, 0, rawSize);

			HuffmanDecoder<uint8_t> modDecoder = new HuffmanDecoder<uint8_t>(
				new HuffmanCode<uint8_t>(1, 0b0001, 0),
				new HuffmanCode<uint8_t>(2, 0b0000, 1),
				new HuffmanCode<uint8_t>(3, 0b0010, 2),
				new HuffmanCode<uint8_t>(4, 0b0110, 3),
				new HuffmanCode<uint8_t>(4, 0b0111, 4)
			);

			HuffmanDecoder<uint8_t> lengthDecoder = new HuffmanDecoder<uint8_t>(
				new HuffmanCode<uint8_t>(1, 0b0000, 0),
				new HuffmanCode<uint8_t>(2, 0b0010, 1),
				new HuffmanCode<uint8_t>(3, 0b0110, 2),
				new HuffmanCode<uint8_t>(4, 0b1110, 3),
				new HuffmanCode<uint8_t>(4, 0b1111, 4)
			);

			HuffmanDecoder<uint8_t> distanceDecoder = new HuffmanDecoder<uint8_t>(
				new HuffmanCode<uint8_t>(1, 0b01, 1),
				new HuffmanCode<uint8_t>(2, 0b00, 0),
				new HuffmanCode<uint8_t>(2, 0b01, 2)
			);

			// First byte is special
			uint8_t currentSample = ReadByte();
			outputStream.WriteByte(currentSample);

			uint32_t accum1 = 0;
			uint32_t accum2 = 0;
			uint32_t prevBits = 0;

			VariableLengthCodeDecoder lengthVlcDecoder = new VariableLengthCodeDecoder(1, 1, 1, 3, 5);
			VariableLengthCodeDecoder distanceVlcDecoder = new VariableLengthCodeDecoder(8, 12, 14);

			while (!outputStream.Eof)
			{
				uint8_t bits = 0;
				uint32_t count = 0;
				bool doRepeat = false;

				if (accum1 >= 8)
				{
					void HandleCondCase()
					{
						if (bits == 8)
						{
							if (accum2 < 20)
								count = 1;
							else
							{
								count = 2;
								accum2 += 8;
							}
						}
						else
						{
							count = 5;
							accum2 += 8;
						}
					}

					void HandleTable(uint32_t newBits)
					{
						if ((prevBits < 2) || (newBits == 0))
							throw new DecompressionException();

						bits = bitLengthTable[prevBits - 2, newBits - 1];
						if (bits == 0)
							throw new DecompressionException();

						HandleCondCase();
					}

					switch (modDecoder.Decode(ReadBit))
					{
						case 0:
						{
							if (prevBits == 8)
							{
								bits = 8;
								HandleCondCase();
							}
							else
							{
								bits = (uint8_t)prevBits;
								count = 5;
								accum2 += 8;
							}
							break;
						}

						case 1:
						{
							doRepeat = true;
							break;
						}

						case 2:
						{
							HandleTable(2);
							break;
						}

						case 3:
						{
							HandleTable(3);
							break;
						}

						case 4:
						{
							HandleTable(ReadBits(2) + 4);
							break;
						}

						default:
							throw new DecompressionException();
					}
				}
				else
				{
					if (ReadBit() != 0)
						doRepeat = true;
					else
					{
						count = 1;
						bits = 8;
					}
				}

				if (doRepeat)
				{
					count = lengthVlcDecoder.Decode(ReadBits, lengthDecoder.Decode(ReadBit)) + 2;
					if (count >= 3)
					{
						if (accum1 != 0)
							accum1--;

						if ((count > 3) && (accum1 != 0))
							accum1--;
					}

					uint32_t distance = distanceVlcDecoder.Decode(ReadBits, distanceDecoder.Decode(ReadBit)) + 1;

					count = Math.Min(count, (uint32_t)(rawSize - outputStream.GetOffset()));
					currentSample = outputStream.Copy(distance, count);
				}
				else
				{
					count = Math.Min(count, (uint32_t)(rawSize - outputStream.GetOffset()));
					for (uint32_t i = 0; i < count; i++)
					{
						currentSample = (uint8_t)((int8_t)currentSample - ReadSignedBits(bits));
						outputStream.WriteByte(currentSample);
					}

					if (accum1 != 31)
						accum1++;

					prevBits = bits;
				}

				accum2 -= accum2 >> 3;
			}
		}
	}
}
