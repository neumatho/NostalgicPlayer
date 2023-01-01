/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats.Streams.Xpk
{
	/// <summary>
	/// This stream read data crunched with XPK (LHLB)
	/// </summary>
	internal class Xpk_LhlbStream : XpkStream
	{
		private static readonly byte[] distanceHighBits =
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
			3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
			4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5,
			6, 6, 6, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 7, 7, 7,
			8, 8, 8, 8, 8, 8, 8, 8, 9, 9, 9, 9, 9, 9, 9, 9,

			10, 10, 10, 10, 10, 10, 10, 10, 11, 11, 11, 11, 11, 11, 11, 11,
			12, 12, 12, 12, 13, 13, 13, 13, 14, 14, 14, 14, 15, 15, 15, 15,
			16, 16, 16, 16, 17, 17, 17, 17, 18, 18, 18, 18, 19, 19, 19, 19,
			20, 20, 20, 20, 21, 21, 21, 21, 22, 22, 22, 22, 23, 23, 23, 23,
			24, 24, 25, 25, 26, 26, 27, 27, 28, 28, 29, 29, 30, 30, 31, 31,
			32, 32, 33, 33, 34, 34, 35, 35, 36, 36, 37, 37, 38, 38, 39, 39,
			40, 40, 41, 41, 42, 42, 43, 43, 44, 44, 45, 45, 46, 46, 47, 47,
			48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63
		};

		private static readonly byte[] distanceBits =
		{
			1, 1, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 5, 5, 5, 6
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Xpk_LhlbStream(string agentName, Stream wrapperStream) : base(agentName, wrapperStream)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will decrunch a single chunk of data
		/// </summary>
		/********************************************************************/
		protected override void DecompressImpl(byte[] chunk, byte[] rawData)
		{
			using (MemoryStream chunkStream = new MemoryStream(chunk, false))
			{
				ForwardInputStream inputStream = new ForwardInputStream(agentName, chunkStream, 0, (uint)chunk.Length);
				MsbBitReader bitReader = new MsbBitReader(inputStream);

				uint ReadBits(uint count) => bitReader.ReadBits8(count);
				uint ReadBit() => bitReader.ReadBits8(1);

				ForwardOutputStream outputStream = new ForwardOutputStream(agentName, rawData, 0, (uint)rawData.Length);

				// Same logic as in Choloks pascal implementation
				// Differences to LH1:
				// - LHLB does not halve probabilities at 32k
				// - 314 vs. 317 sized Huffman entry
				// - No end code
				// - Different distance/count logic
				DynamicHuffmanDecoder decoder = new DynamicHuffmanDecoder(agentName, 317);

				while (!outputStream.Eof)
				{
					uint code = decoder.Decode(ReadBit);
					if (code == 316)
						break;

					if (decoder.GetMaxFrequency() < 0x8000)
						decoder.Update(code);

					if (code < 256)
						outputStream.WriteByte(code);
					else
					{
						uint tmp = ReadBits(8);
						uint distance = (uint)distanceHighBits[tmp] << 6;
						uint bits = distanceBits[tmp >> 4];
						tmp = (tmp << (int)bits) | ReadBits(bits);
						distance |= tmp & 63;
						uint count = code - 255;

						outputStream.Copy(distance, count);
					}
				}
			}
		}
	}
}
