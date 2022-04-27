/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common;
using Polycode.NostalgicPlayer.Kit.Exceptions;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats.Streams.Xpk
{
	/// <summary>
	/// This stream read data crunched with XPK (RAKE)
	/// </summary>
	internal class Xpk_RakeStream : XpkStream
	{
		// Is there some logic into this?
		private static readonly byte[][] decTable = new byte[255][]
		{
			new byte[] { 1, 0x01}, new byte[] { 3, 0x03}, new byte[] { 5, 0x05}, new byte[] { 6, 0x09}, new byte[] { 7, 0x0c}, new byte[] { 9, 0x13}, new byte[] {12, 0x34}, new byte[] {18, 0xc0},
			new byte[] {18, 0xc2}, new byte[] {18, 0xc3}, new byte[] {18, 0xc6}, new byte[] {16, 0x79}, new byte[] {18, 0xc7}, new byte[] {18, 0xd6}, new byte[] {18, 0xd7}, new byte[] {18, 0xd8},
			new byte[] {17, 0xa8}, new byte[] {17, 0x92}, new byte[] {17, 0x8a}, new byte[] {17, 0x82}, new byte[] {16, 0x6c}, new byte[] {17, 0x94}, new byte[] {18, 0xda}, new byte[] {18, 0xca},
			new byte[] {16, 0x7b}, new byte[] {13, 0x36}, new byte[] {13, 0x39}, new byte[] {13, 0x48}, new byte[] {14, 0x49}, new byte[] {14, 0x50}, new byte[] {15, 0x62}, new byte[] {15, 0x5e},
			new byte[] {16, 0x6f}, new byte[] {17, 0x83}, new byte[] {17, 0x87}, new byte[] {15, 0x56}, new byte[] {11, 0x21}, new byte[] {12, 0x31}, new byte[] {13, 0x38}, new byte[] {13, 0x3d},
			new byte[] { 8, 0x0f}, new byte[] { 4, 0x04}, new byte[] { 6, 0x08}, new byte[] {10, 0x1c}, new byte[] {12, 0x27}, new byte[] {13, 0x42}, new byte[] {13, 0x3a}, new byte[] {12, 0x30},
			new byte[] {12, 0x32}, new byte[] { 9, 0x16}, new byte[] { 8, 0x11}, new byte[] { 7, 0x0b}, new byte[] { 5, 0x06}, new byte[] {10, 0x19}, new byte[] {10, 0x1a}, new byte[] {10, 0x18},
			new byte[] {11, 0x26}, new byte[] {17, 0x98}, new byte[] {17, 0x99}, new byte[] {17, 0x9b}, new byte[] {17, 0x9e}, new byte[] {17, 0x9f}, new byte[] {17, 0xa6}, new byte[] {16, 0x73},
			new byte[] {17, 0x7f}, new byte[] {17, 0x81}, new byte[] {17, 0x84}, new byte[] {17, 0x85}, new byte[] {15, 0x5d}, new byte[] {14, 0x4d}, new byte[] {14, 0x4f}, new byte[] {13, 0x45},
			new byte[] {13, 0x3c}, new byte[] { 9, 0x17}, new byte[] {10, 0x1d}, new byte[] {12, 0xff}, new byte[] {13, 0x41}, new byte[] {17, 0x8c}, new byte[] {18, 0xaa}, new byte[] {19, 0xdb},
			new byte[] {19, 0xdc}, new byte[] {16, 0x77}, new byte[] {15, 0x63}, new byte[] {16, 0x7c}, new byte[] {16, 0x76}, new byte[] {16, 0x71}, new byte[] {16, 0x7d}, new byte[] {12, 0x2c},
			new byte[] {13, 0x3b}, new byte[] {16, 0x7a}, new byte[] {16, 0x75}, new byte[] {15, 0x55}, new byte[] {15, 0x60}, new byte[] {16, 0x74}, new byte[] {17, 0xa4}, new byte[] {18, 0xab},
			new byte[] {18, 0xac}, new byte[] { 7, 0x0a}, new byte[] { 6, 0x07}, new byte[] { 9, 0x15}, new byte[] {11, 0x20}, new byte[] {11, 0x24}, new byte[] {10, 0x1b}, new byte[] { 8, 0x10},
			new byte[] { 9, 0x12}, new byte[] {12, 0x33}, new byte[] {14, 0x4b}, new byte[] {15, 0x53}, new byte[] {19, 0xdd}, new byte[] {19, 0xde}, new byte[] {18, 0xad}, new byte[] {19, 0xdf},
			new byte[] {19, 0xe0}, new byte[] {18, 0xae}, new byte[] {17, 0x88}, new byte[] {18, 0xaf}, new byte[] {19, 0xe1}, new byte[] {19, 0xe2}, new byte[] {13, 0x37}, new byte[] {12, 0x2e},
			new byte[] {18, 0xb0}, new byte[] {18, 0xb1}, new byte[] {19, 0xe3}, new byte[] {19, 0xe4}, new byte[] {18, 0xb2}, new byte[] {18, 0xb3}, new byte[] {19, 0xe5}, new byte[] {19, 0xe6},
			new byte[] {19, 0xe7}, new byte[] {19, 0xe8}, new byte[] {18, 0xb4}, new byte[] {17, 0x9a}, new byte[] {18, 0xb5}, new byte[] {18, 0xb6}, new byte[] {18, 0xb7}, new byte[] {19, 0xe9},
			new byte[] {19, 0xea}, new byte[] {18, 0xb8}, new byte[] {19, 0xeb}, new byte[] {19, 0xec}, new byte[] {19, 0xed}, new byte[] {19, 0xee}, new byte[] {18, 0xb9}, new byte[] {19, 0xef},
			new byte[] {19, 0xf0}, new byte[] {18, 0xbb}, new byte[] {18, 0xbc}, new byte[] {19, 0xf1}, new byte[] {19, 0xf2}, new byte[] {18, 0xbd}, new byte[] {18, 0xbe}, new byte[] {19, 0xf3},
			new byte[] {19, 0xf4}, new byte[] {18, 0xbf}, new byte[] {18, 0xc1}, new byte[] {19, 0xf5}, new byte[] {19, 0xf6}, new byte[] {18, 0xc4}, new byte[] {18, 0xc5}, new byte[] {17, 0x95},
			new byte[] {18, 0xc8}, new byte[] {18, 0xc9}, new byte[] {19, 0xf7}, new byte[] {19, 0xf8}, new byte[] {18, 0xcb}, new byte[] {18, 0xcc}, new byte[] {19, 0xf9}, new byte[] {19, 0xfa},
			new byte[] {18, 0xcd}, new byte[] {18, 0xce}, new byte[] {17, 0x96}, new byte[] {18, 0xcf}, new byte[] {18, 0xd0}, new byte[] {19, 0xfb}, new byte[] {19, 0xfc}, new byte[] {18, 0xd1},
			new byte[] {18, 0xd2}, new byte[] {18, 0xd3}, new byte[] {17, 0x9c}, new byte[] {17, 0x9d}, new byte[] {18, 0xd4}, new byte[] {18, 0xd5}, new byte[] {17, 0xa0}, new byte[] {17, 0xa1},
			new byte[] {17, 0xa2}, new byte[] {17, 0xa3}, new byte[] {17, 0xa5}, new byte[] {19, 0xfd}, new byte[] {19, 0xfe}, new byte[] {18, 0xd9}, new byte[] {17, 0xa7}, new byte[] {16, 0x66},
			new byte[] {15, 0x54}, new byte[] {15, 0x57}, new byte[] {16, 0x6b}, new byte[] {16, 0x68}, new byte[] {14, 0x4c}, new byte[] {14, 0x4e}, new byte[] {12, 0x28}, new byte[] {11, 0x23},
			new byte[] { 8, 0x0e}, new byte[] { 7, 0x0d}, new byte[] {10, 0x1f}, new byte[] {13, 0x47}, new byte[] {15, 0x64}, new byte[] {15, 0x58}, new byte[] {15, 0x59}, new byte[] {15, 0x5a},
			new byte[] {12, 0x29}, new byte[] {13, 0x3e}, new byte[] {15, 0x5f}, new byte[] {17, 0x8e}, new byte[] {18, 0xba}, new byte[] {18, 0xa9}, new byte[] {16, 0x70}, new byte[] {14, 0x4a},
			new byte[] {12, 0x2a}, new byte[] { 9, 0x14}, new byte[] {11, 0x22}, new byte[] {12, 0x2f}, new byte[] {16, 0x7e}, new byte[] {16, 0x67}, new byte[] {16, 0x69}, new byte[] {16, 0x65},
			new byte[] {15, 0x51}, new byte[] {16, 0x78}, new byte[] {16, 0x6a}, new byte[] {13, 0x46}, new byte[] {11, 0x25}, new byte[] {16, 0x72}, new byte[] {16, 0x6e}, new byte[] {15, 0x5b},
			new byte[] {15, 0x61}, new byte[] {15, 0x52}, new byte[] {13, 0x40}, new byte[] {13, 0x43}, new byte[] {13, 0x44}, new byte[] {13, 0x3f}, new byte[] {15, 0x5c}, new byte[] {17, 0x93},
			new byte[] {17, 0x80}, new byte[] {17, 0x8d}, new byte[] {17, 0x8b}, new byte[] {17, 0x86}, new byte[] {17, 0x89}, new byte[] {17, 0x97}, new byte[] {17, 0x8f}, new byte[] {17, 0x90},
			new byte[] {17, 0x91}, new byte[] {16, 0x6d}, new byte[] {12, 0x2b}, new byte[] {12, 0x2d}, new byte[] {12, 0x35}, new byte[] {10, 0x1e}, new byte[] { 3, 0x02}
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Xpk_RakeStream(string agentName,  Stream wrapperStream) : base(agentName,  wrapperStream)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will decrunch a single chunk of data
		/// </summary>
		/********************************************************************/
		protected override void DecompressImpl(byte[] chunk,  byte[] rawData)
		{
			uint midStreamOffset = Read16(chunk,  2);
			if (midStreamOffset >= chunk.Length)
				throw new DecruncherException(agentName,  Resources.IDS_ANC_ERR_CORRUPT_DATA);

			using (MemoryStream chunkStream = new MemoryStream(chunk,  false))
			{
				// 2 streams
				// 1st: Bit stream starting from midStreamOffset(+1) going to chunk array length
				// 2nd: Byte stream starting from midStreamOffset going backwards to 4
				ForwardInputStream forwardInputStream = new ForwardInputStream(agentName,  chunkStream,  midStreamOffset + (midStreamOffset & 1),  (uint)chunk.Length);
				BackwardInputStream backwardInputStream = new BackwardInputStream(agentName,  chunkStream,  4,  midStreamOffset);
				MsbBitReader bitReader = new MsbBitReader(forwardInputStream);

				uint ReadBits(uint count) => bitReader.ReadBits32(count);
				uint ReadBit() => bitReader.ReadBits32(1);
				byte ReadByte() => backwardInputStream.ReadByte();

				{
					ushort tmp = Read16(chunk,  0);
					if (tmp > 32)
						throw new DecruncherException(agentName,  Resources.IDS_ANC_ERR_CORRUPT_DATA);

					byte[] buf = forwardInputStream.Consume(4);
					uint content = (uint)((buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | (buf[3]));
					bitReader.Reset(content >> tmp,  (byte)(32 - tmp));
				}

				BackwardOutputStream outputStream = new BackwardOutputStream(agentName,  rawData,  0,  (uint)rawData.Length);

				HuffmanDecoder<uint> lengthDecoder = new HuffmanDecoder<uint>(agentName);

				uint hufCode = 0;
				foreach (byte[] it in decTable)
				{
					lengthDecoder.Insert(new HuffmanCode<uint>(it[0], hufCode >> (32 - it[0]), it[1]));
					hufCode += (uint)1 << (32 - it[0]);
				}

				while (!outputStream.Eof)
				{
					if (ReadBit() == 0)
						outputStream.WriteByte(ReadByte());
					else
					{
						uint count = lengthDecoder.Decode(ReadBit);
						count += 2;

						uint distance;
						if (ReadBit() == 0)
							distance = (uint)(ReadByte() + 1);
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
}
