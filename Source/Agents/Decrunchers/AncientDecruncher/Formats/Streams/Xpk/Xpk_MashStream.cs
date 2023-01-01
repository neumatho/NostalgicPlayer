/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common;
using Polycode.NostalgicPlayer.Kit.Exceptions;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats.Streams.Xpk
{
	/// <summary>
	/// This stream read data crunched with XPK (MASH)
	/// </summary>
	internal class Xpk_MashStream : XpkStream
	{
		private static readonly byte[] distanceBits =
		{
			5, 7, 9, 10, 11, 12, 13, 14
		};

		private static readonly uint[] distanceAdditions =
		{
			0, 0x20, 0xa0, 0x2a0, 0x6a0, 0xea0, 0x1ea0, 0x3ea0
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Xpk_MashStream(string agentName, Stream wrapperStream) : base(agentName, wrapperStream)
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
				byte ReadByte() => inputStream.ReadByte();

				uint rawSize = (uint)rawData.Length;
				ForwardOutputStream outputStream = new ForwardOutputStream(agentName, rawData, 0, rawSize);

				HuffmanDecoder<uint> litDecoder = new HuffmanDecoder<uint>(agentName,
					new HuffmanCode<uint>(1, 0b000000, 0),
					new HuffmanCode<uint>(2, 0b000010, 1),
					new HuffmanCode<uint>(3, 0b000110, 2),
					new HuffmanCode<uint>(4, 0b001110, 3),
					new HuffmanCode<uint>(5, 0b011110, 4),
					new HuffmanCode<uint>(6, 0b111110, 5),
					new HuffmanCode<uint>(6, 0b111111, 6)
				);

				while (!outputStream.Eof)
				{
					uint litLength = litDecoder.Decode(ReadBit);
					if (litLength == 6)
					{
						uint litBits;
						for (litBits = 1; litBits <= 17; litBits++)
						{
							if (ReadBit() == 0)
								break;
						}

						if (litBits == 17)
							throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

						litLength = (uint)(ReadBits(litBits) + (1 << (int)litBits) + 4);
					}

					for (uint i = 0; i < litLength; i++)
						outputStream.WriteByte(ReadByte());

					uint count, distance;

					void ReadDistance()
					{
						uint tableIndex = ReadBits(3);
						distance = ReadBits(distanceBits[tableIndex]) + distanceAdditions[tableIndex];
					}

					if (ReadBit() != 0)
					{
						uint countBits;
						for (countBits = 1; countBits <= 16; countBits++)
						{
							if (ReadBit() == 0)
								break;
						}

						if (countBits == 16)
							throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

						count = (uint)(ReadBits(countBits) + (1 << (int)countBits) + 2);
						ReadDistance();
					}
					else
					{
						if (ReadBit() != 0)
						{
							ReadDistance();
							count = 3;
						}
						else
						{
							distance = ReadBits(9);
							count = 2;
						}
					}

					// Hack to make it work
					if ((distance == 0) && outputStream.Eof)
						break;

					// Zero distance when we are at the end of the stream...
					// There seems to be almost systematic extra one byte at the end of the stream...
					count = Math.Min(count, rawSize - outputStream.GetOffset());
					outputStream.Copy(distance, count);
				}
			}
		}
	}
}
