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
	/// XPK-RAKE decompressor
	/// </summary>
	internal class RakeDecompressor : XpkDecompressor
	{
		// Is there some logic into this?
		private static readonly uint8_t[][] decTable = new uint8_t[255][]
		{
			new uint8_t[] { 1, 0x01}, new uint8_t[] { 3, 0x03}, new uint8_t[] { 5, 0x05}, new uint8_t[] { 6, 0x09}, new uint8_t[] { 7, 0x0c}, new uint8_t[] { 9, 0x13}, new uint8_t[] {12, 0x34}, new uint8_t[] {18, 0xc0},
			new uint8_t[] {18, 0xc2}, new uint8_t[] {18, 0xc3}, new uint8_t[] {18, 0xc6}, new uint8_t[] {16, 0x79}, new uint8_t[] {18, 0xc7}, new uint8_t[] {18, 0xd6}, new uint8_t[] {18, 0xd7}, new uint8_t[] {18, 0xd8},
			new uint8_t[] {17, 0xa8}, new uint8_t[] {17, 0x92}, new uint8_t[] {17, 0x8a}, new uint8_t[] {17, 0x82}, new uint8_t[] {16, 0x6c}, new uint8_t[] {17, 0x94}, new uint8_t[] {18, 0xda}, new uint8_t[] {18, 0xca},
			new uint8_t[] {16, 0x7b}, new uint8_t[] {13, 0x36}, new uint8_t[] {13, 0x39}, new uint8_t[] {13, 0x48}, new uint8_t[] {14, 0x49}, new uint8_t[] {14, 0x50}, new uint8_t[] {15, 0x62}, new uint8_t[] {15, 0x5e},
			new uint8_t[] {16, 0x6f}, new uint8_t[] {17, 0x83}, new uint8_t[] {17, 0x87}, new uint8_t[] {15, 0x56}, new uint8_t[] {11, 0x21}, new uint8_t[] {12, 0x31}, new uint8_t[] {13, 0x38}, new uint8_t[] {13, 0x3d},
			new uint8_t[] { 8, 0x0f}, new uint8_t[] { 4, 0x04}, new uint8_t[] { 6, 0x08}, new uint8_t[] {10, 0x1c}, new uint8_t[] {12, 0x27}, new uint8_t[] {13, 0x42}, new uint8_t[] {13, 0x3a}, new uint8_t[] {12, 0x30},
			new uint8_t[] {12, 0x32}, new uint8_t[] { 9, 0x16}, new uint8_t[] { 8, 0x11}, new uint8_t[] { 7, 0x0b}, new uint8_t[] { 5, 0x06}, new uint8_t[] {10, 0x19}, new uint8_t[] {10, 0x1a}, new uint8_t[] {10, 0x18},
			new uint8_t[] {11, 0x26}, new uint8_t[] {17, 0x98}, new uint8_t[] {17, 0x99}, new uint8_t[] {17, 0x9b}, new uint8_t[] {17, 0x9e}, new uint8_t[] {17, 0x9f}, new uint8_t[] {17, 0xa6}, new uint8_t[] {16, 0x73},
			new uint8_t[] {17, 0x7f}, new uint8_t[] {17, 0x81}, new uint8_t[] {17, 0x84}, new uint8_t[] {17, 0x85}, new uint8_t[] {15, 0x5d}, new uint8_t[] {14, 0x4d}, new uint8_t[] {14, 0x4f}, new uint8_t[] {13, 0x45},
			new uint8_t[] {13, 0x3c}, new uint8_t[] { 9, 0x17}, new uint8_t[] {10, 0x1d}, new uint8_t[] {12, 0xff}, new uint8_t[] {13, 0x41}, new uint8_t[] {17, 0x8c}, new uint8_t[] {18, 0xaa}, new uint8_t[] {19, 0xdb},
			new uint8_t[] {19, 0xdc}, new uint8_t[] {16, 0x77}, new uint8_t[] {15, 0x63}, new uint8_t[] {16, 0x7c}, new uint8_t[] {16, 0x76}, new uint8_t[] {16, 0x71}, new uint8_t[] {16, 0x7d}, new uint8_t[] {12, 0x2c},
			new uint8_t[] {13, 0x3b}, new uint8_t[] {16, 0x7a}, new uint8_t[] {16, 0x75}, new uint8_t[] {15, 0x55}, new uint8_t[] {15, 0x60}, new uint8_t[] {16, 0x74}, new uint8_t[] {17, 0xa4}, new uint8_t[] {18, 0xab},
			new uint8_t[] {18, 0xac}, new uint8_t[] { 7, 0x0a}, new uint8_t[] { 6, 0x07}, new uint8_t[] { 9, 0x15}, new uint8_t[] {11, 0x20}, new uint8_t[] {11, 0x24}, new uint8_t[] {10, 0x1b}, new uint8_t[] { 8, 0x10},
			new uint8_t[] { 9, 0x12}, new uint8_t[] {12, 0x33}, new uint8_t[] {14, 0x4b}, new uint8_t[] {15, 0x53}, new uint8_t[] {19, 0xdd}, new uint8_t[] {19, 0xde}, new uint8_t[] {18, 0xad}, new uint8_t[] {19, 0xdf},
			new uint8_t[] {19, 0xe0}, new uint8_t[] {18, 0xae}, new uint8_t[] {17, 0x88}, new uint8_t[] {18, 0xaf}, new uint8_t[] {19, 0xe1}, new uint8_t[] {19, 0xe2}, new uint8_t[] {13, 0x37}, new uint8_t[] {12, 0x2e},
			new uint8_t[] {18, 0xb0}, new uint8_t[] {18, 0xb1}, new uint8_t[] {19, 0xe3}, new uint8_t[] {19, 0xe4}, new uint8_t[] {18, 0xb2}, new uint8_t[] {18, 0xb3}, new uint8_t[] {19, 0xe5}, new uint8_t[] {19, 0xe6},
			new uint8_t[] {19, 0xe7}, new uint8_t[] {19, 0xe8}, new uint8_t[] {18, 0xb4}, new uint8_t[] {17, 0x9a}, new uint8_t[] {18, 0xb5}, new uint8_t[] {18, 0xb6}, new uint8_t[] {18, 0xb7}, new uint8_t[] {19, 0xe9},
			new uint8_t[] {19, 0xea}, new uint8_t[] {18, 0xb8}, new uint8_t[] {19, 0xeb}, new uint8_t[] {19, 0xec}, new uint8_t[] {19, 0xed}, new uint8_t[] {19, 0xee}, new uint8_t[] {18, 0xb9}, new uint8_t[] {19, 0xef},
			new uint8_t[] {19, 0xf0}, new uint8_t[] {18, 0xbb}, new uint8_t[] {18, 0xbc}, new uint8_t[] {19, 0xf1}, new uint8_t[] {19, 0xf2}, new uint8_t[] {18, 0xbd}, new uint8_t[] {18, 0xbe}, new uint8_t[] {19, 0xf3},
			new uint8_t[] {19, 0xf4}, new uint8_t[] {18, 0xbf}, new uint8_t[] {18, 0xc1}, new uint8_t[] {19, 0xf5}, new uint8_t[] {19, 0xf6}, new uint8_t[] {18, 0xc4}, new uint8_t[] {18, 0xc5}, new uint8_t[] {17, 0x95},
			new uint8_t[] {18, 0xc8}, new uint8_t[] {18, 0xc9}, new uint8_t[] {19, 0xf7}, new uint8_t[] {19, 0xf8}, new uint8_t[] {18, 0xcb}, new uint8_t[] {18, 0xcc}, new uint8_t[] {19, 0xf9}, new uint8_t[] {19, 0xfa},
			new uint8_t[] {18, 0xcd}, new uint8_t[] {18, 0xce}, new uint8_t[] {17, 0x96}, new uint8_t[] {18, 0xcf}, new uint8_t[] {18, 0xd0}, new uint8_t[] {19, 0xfb}, new uint8_t[] {19, 0xfc}, new uint8_t[] {18, 0xd1},
			new uint8_t[] {18, 0xd2}, new uint8_t[] {18, 0xd3}, new uint8_t[] {17, 0x9c}, new uint8_t[] {17, 0x9d}, new uint8_t[] {18, 0xd4}, new uint8_t[] {18, 0xd5}, new uint8_t[] {17, 0xa0}, new uint8_t[] {17, 0xa1},
			new uint8_t[] {17, 0xa2}, new uint8_t[] {17, 0xa3}, new uint8_t[] {17, 0xa5}, new uint8_t[] {19, 0xfd}, new uint8_t[] {19, 0xfe}, new uint8_t[] {18, 0xd9}, new uint8_t[] {17, 0xa7}, new uint8_t[] {16, 0x66},
			new uint8_t[] {15, 0x54}, new uint8_t[] {15, 0x57}, new uint8_t[] {16, 0x6b}, new uint8_t[] {16, 0x68}, new uint8_t[] {14, 0x4c}, new uint8_t[] {14, 0x4e}, new uint8_t[] {12, 0x28}, new uint8_t[] {11, 0x23},
			new uint8_t[] { 8, 0x0e}, new uint8_t[] { 7, 0x0d}, new uint8_t[] {10, 0x1f}, new uint8_t[] {13, 0x47}, new uint8_t[] {15, 0x64}, new uint8_t[] {15, 0x58}, new uint8_t[] {15, 0x59}, new uint8_t[] {15, 0x5a},
			new uint8_t[] {12, 0x29}, new uint8_t[] {13, 0x3e}, new uint8_t[] {15, 0x5f}, new uint8_t[] {17, 0x8e}, new uint8_t[] {18, 0xba}, new uint8_t[] {18, 0xa9}, new uint8_t[] {16, 0x70}, new uint8_t[] {14, 0x4a},
			new uint8_t[] {12, 0x2a}, new uint8_t[] { 9, 0x14}, new uint8_t[] {11, 0x22}, new uint8_t[] {12, 0x2f}, new uint8_t[] {16, 0x7e}, new uint8_t[] {16, 0x67}, new uint8_t[] {16, 0x69}, new uint8_t[] {16, 0x65},
			new uint8_t[] {15, 0x51}, new uint8_t[] {16, 0x78}, new uint8_t[] {16, 0x6a}, new uint8_t[] {13, 0x46}, new uint8_t[] {11, 0x25}, new uint8_t[] {16, 0x72}, new uint8_t[] {16, 0x6e}, new uint8_t[] {15, 0x5b},
			new uint8_t[] {15, 0x61}, new uint8_t[] {15, 0x52}, new uint8_t[] {13, 0x40}, new uint8_t[] {13, 0x43}, new uint8_t[] {13, 0x44}, new uint8_t[] {13, 0x3f}, new uint8_t[] {15, 0x5c}, new uint8_t[] {17, 0x93},
			new uint8_t[] {17, 0x80}, new uint8_t[] {17, 0x8d}, new uint8_t[] {17, 0x8b}, new uint8_t[] {17, 0x86}, new uint8_t[] {17, 0x89}, new uint8_t[] {17, 0x97}, new uint8_t[] {17, 0x8f}, new uint8_t[] {17, 0x90},
			new uint8_t[] {17, 0x91}, new uint8_t[] {16, 0x6d}, new uint8_t[] {12, 0x2b}, new uint8_t[] {12, 0x2d}, new uint8_t[] {12, 0x35}, new uint8_t[] {10, 0x1e}, new uint8_t[] { 3, 0x02}
		};

		private readonly Buffer packedData;

		private readonly size_t midStreamOffset;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private RakeDecompressor(uint32_t hdr, Buffer packedData)
		{
			this.packedData = packedData;

			if (!DetectHeaderXpk(hdr) || (packedData.Size() < 4))
				throw new InvalidFormatException();

			midStreamOffset = packedData.ReadBe16(2);
			if (midStreamOffset >= packedData.Size())
				throw new InvalidFormatException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool DetectHeaderXpk(uint32_t hdr)
		{
			return hdr == Common.Common.FourCC("RAKE");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static XpkDecompressor Create(uint32_t hdr, Buffer packedData, ref State state)
		{
			return new RakeDecompressor(hdr, packedData);
		}



		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		public override void DecompressImpl(Buffer rawData, List<Buffer> previousBuffers)
		{
			// 2 streams
			// 1st: Bit stream starting from midStreamOffset(+1) going to chunk array length
			// 2nd: Byte stream starting from midStreamOffset going backwards to 4
			ForwardInputStream forwardInputStream = new ForwardInputStream(packedData, midStreamOffset + (midStreamOffset & 1), packedData.Size());
			BackwardInputStream backwardInputStream = new BackwardInputStream(packedData, 4, midStreamOffset);
			MsbBitReader bitReader = new MsbBitReader(forwardInputStream);

			uint32_t ReadBits(uint32_t count) => bitReader.ReadBitsBe32(count);
			uint32_t ReadBit() => bitReader.ReadBitsBe32(1);
			uint8_t ReadByte() => backwardInputStream.ReadByte();

			{
				uint16_t tmp = packedData.ReadBe16(0);
				if (tmp > 32)
					throw new DecompressionException();

				Span<uint8_t> buf = forwardInputStream.Consume(4);
				uint32_t content = (uint32_t)((buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | (buf[3]));
				bitReader.Reset(content >> tmp, (uint8_t)(32 - tmp));
			}

			BackwardOutputStream outputStream = new BackwardOutputStream(rawData, 0, rawData.Size());

			HuffmanDecoder<uint32_t> lengthDecoder = new HuffmanDecoder<uint32_t>();

			uint32_t hufCode = 0;
			foreach (uint8_t[] it in decTable)
			{
				lengthDecoder.Insert(new HuffmanCode<uint32_t>(it[0], hufCode >> (32 - it[0]), it[1]));
				hufCode += (uint32_t)1 << (32 - it[0]);
			}

			while (!outputStream.Eof)
			{
				if (ReadBit() == 0)
					outputStream.WriteByte(ReadByte());
				else
				{
					uint32_t count = lengthDecoder.Decode(ReadBit);
					count += 2;

					uint32_t distance;
					if (ReadBit() == 0)
						distance = (uint32_t)(ReadByte()) + 1;
					else
					{
						if (ReadBit() == 0)
							distance = ((ReadBits(3) << 8) | (ReadByte())) + 0x101;
						else
							distance = ((ReadBits(6) << 8) | (ReadByte())) + 0x901;
					}

					outputStream.Copy(distance, count);
				}
			}
		}
	}
}
