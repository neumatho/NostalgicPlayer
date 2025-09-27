/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.LibAncient.Common;
using Polycode.NostalgicPlayer.Ports.LibAncient.Exceptions;
using Buffer = Polycode.NostalgicPlayer.Ports.LibAncient.Common.Buffers.Buffer;

namespace Polycode.NostalgicPlayer.Ports.LibAncient.Internal.Decompressors.Xpk
{
	/// <summary>
	/// XPK-RAKE decompressor
	/// </summary>
	internal class RakeDecompressor : XpkDecompressor
	{
		// Is there some logic into this?
		private static readonly uint8_t[][] decTable =
		[
			[ 1, 0x01], [ 3, 0x03], [ 5, 0x05], [ 6, 0x09], [ 7, 0x0c], [ 9, 0x13], [12, 0x34], [18, 0xc0],
			[18, 0xc2], [18, 0xc3], [18, 0xc6], [16, 0x79], [18, 0xc7], [18, 0xd6], [18, 0xd7], [18, 0xd8],
			[17, 0xa8], [17, 0x92], [17, 0x8a], [17, 0x82], [16, 0x6c], [17, 0x94], [18, 0xda], [18, 0xca],
			[16, 0x7b], [13, 0x36], [13, 0x39], [13, 0x48], [14, 0x49], [14, 0x50], [15, 0x62], [15, 0x5e],
			[16, 0x6f], [17, 0x83], [17, 0x87], [15, 0x56], [11, 0x21], [12, 0x31], [13, 0x38], [13, 0x3d],
			[ 8, 0x0f], [ 4, 0x04], [ 6, 0x08], [10, 0x1c], [12, 0x27], [13, 0x42], [13, 0x3a], [12, 0x30],
			[12, 0x32], [ 9, 0x16], [ 8, 0x11], [ 7, 0x0b], [ 5, 0x06], [10, 0x19], [10, 0x1a], [10, 0x18],
			[11, 0x26], [17, 0x98], [17, 0x99], [17, 0x9b], [17, 0x9e], [17, 0x9f], [17, 0xa6], [16, 0x73],
			[17, 0x7f], [17, 0x81], [17, 0x84], [17, 0x85], [15, 0x5d], [14, 0x4d], [14, 0x4f], [13, 0x45],
			[13, 0x3c], [ 9, 0x17], [10, 0x1d], [12, 0xff], [13, 0x41], [17, 0x8c], [18, 0xaa], [19, 0xdb],
			[19, 0xdc], [16, 0x77], [15, 0x63], [16, 0x7c], [16, 0x76], [16, 0x71], [16, 0x7d], [12, 0x2c],
			[13, 0x3b], [16, 0x7a], [16, 0x75], [15, 0x55], [15, 0x60], [16, 0x74], [17, 0xa4], [18, 0xab],
			[18, 0xac], [ 7, 0x0a], [ 6, 0x07], [ 9, 0x15], [11, 0x20], [11, 0x24], [10, 0x1b], [ 8, 0x10],
			[ 9, 0x12], [12, 0x33], [14, 0x4b], [15, 0x53], [19, 0xdd], [19, 0xde], [18, 0xad], [19, 0xdf],
			[19, 0xe0], [18, 0xae], [17, 0x88], [18, 0xaf], [19, 0xe1], [19, 0xe2], [13, 0x37], [12, 0x2e],
			[18, 0xb0], [18, 0xb1], [19, 0xe3], [19, 0xe4], [18, 0xb2], [18, 0xb3], [19, 0xe5], [19, 0xe6],
			[19, 0xe7], [19, 0xe8], [18, 0xb4], [17, 0x9a], [18, 0xb5], [18, 0xb6], [18, 0xb7], [19, 0xe9],
			[19, 0xea], [18, 0xb8], [19, 0xeb], [19, 0xec], [19, 0xed], [19, 0xee], [18, 0xb9], [19, 0xef],
			[19, 0xf0], [18, 0xbb], [18, 0xbc], [19, 0xf1], [19, 0xf2], [18, 0xbd], [18, 0xbe], [19, 0xf3],
			[19, 0xf4], [18, 0xbf], [18, 0xc1], [19, 0xf5], [19, 0xf6], [18, 0xc4], [18, 0xc5], [17, 0x95],
			[18, 0xc8], [18, 0xc9], [19, 0xf7], [19, 0xf8], [18, 0xcb], [18, 0xcc], [19, 0xf9], [19, 0xfa],
			[18, 0xcd], [18, 0xce], [17, 0x96], [18, 0xcf], [18, 0xd0], [19, 0xfb], [19, 0xfc], [18, 0xd1],
			[18, 0xd2], [18, 0xd3], [17, 0x9c], [17, 0x9d], [18, 0xd4], [18, 0xd5], [17, 0xa0], [17, 0xa1],
			[17, 0xa2], [17, 0xa3], [17, 0xa5], [19, 0xfd], [19, 0xfe], [18, 0xd9], [17, 0xa7], [16, 0x66],
			[15, 0x54], [15, 0x57], [16, 0x6b], [16, 0x68], [14, 0x4c], [14, 0x4e], [12, 0x28], [11, 0x23],
			[ 8, 0x0e], [ 7, 0x0d], [10, 0x1f], [13, 0x47], [15, 0x64], [15, 0x58], [15, 0x59], [15, 0x5a],
			[12, 0x29], [13, 0x3e], [15, 0x5f], [17, 0x8e], [18, 0xba], [18, 0xa9], [16, 0x70], [14, 0x4a],
			[12, 0x2a], [ 9, 0x14], [11, 0x22], [12, 0x2f], [16, 0x7e], [16, 0x67], [16, 0x69], [16, 0x65],
			[15, 0x51], [16, 0x78], [16, 0x6a], [13, 0x46], [11, 0x25], [16, 0x72], [16, 0x6e], [15, 0x5b],
			[15, 0x61], [15, 0x52], [13, 0x40], [13, 0x43], [13, 0x44], [13, 0x3f], [15, 0x5c], [17, 0x93],
			[17, 0x80], [17, 0x8d], [17, 0x8b], [17, 0x86], [17, 0x89], [17, 0x97], [17, 0x8f], [17, 0x90],
			[17, 0x91], [16, 0x6d], [12, 0x2b], [12, 0x2d], [12, 0x35], [10, 0x1e], [ 3, 0x02]
		];

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

				uint32_t content = forwardInputStream.ReadBE32();
				bitReader.Reset(content >> tmp, (uint8_t)(32 - tmp));
			}

			BackwardOutputStream outputStream = new BackwardOutputStream(rawData, 0, rawData.Size());

			HuffmanDecoder<uint8_t> lengthDecoder = new HuffmanDecoder<uint8_t>();

			uint32_t hufCode = 0;
			foreach (uint8_t[] it in decTable)
			{
				lengthDecoder.Insert(new HuffmanCode<uint8_t>(it[0], hufCode >> (32 - it[0]), it[1]));
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
