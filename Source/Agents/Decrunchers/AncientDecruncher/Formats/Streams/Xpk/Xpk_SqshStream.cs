/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common;
using Polycode.NostalgicPlayer.Kit.Exceptions;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats.Streams.Xpk
{
	/// <summary>
	/// This stream read data crunched with XPK (SQSH)
	/// </summary>
	internal class Xpk_SqshStream : XpkStream
	{
		private static readonly byte[,] bitLengthTable = new byte[7, 8]
		{
			{ 2, 3, 4, 5, 6, 7, 8, 0 },
			{ 3, 2, 4, 5, 6, 7, 8, 0 },
			{ 4, 3, 5, 2, 6, 7, 8, 0 },
			{ 5, 4, 6, 2, 3, 7, 8, 0 },
			{ 6, 5, 7, 2, 3, 4, 8, 0 },
			{ 7, 6, 8, 2, 3, 4, 5, 0 },
			{ 8, 7, 6, 2, 3, 4, 5, 0 }
		};

		private static readonly byte[] lengthBits = { 1, 1, 1, 3, 5 };
		private static readonly uint[] lengthAdditions = { 2, 4, 6, 8, 16 };

		private static readonly byte[] distanceBits = { 12, 8, 14 };
		private static readonly uint[] distanceAdditions = { 0x101, 1, 0x1101 };

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Xpk_SqshStream(string agentName, Stream wrapperStream) : base(agentName, wrapperStream)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will decrunch a single chunk of data
		/// </summary>
		/********************************************************************/
		protected override void DecompressImpl(byte[] chunk, byte[] rawData)
		{
			ushort rawSize = (ushort)((chunk[0] << 8) | chunk[1]);

			if (rawData.Length != rawSize)
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

			using (MemoryStream chunkStream = new MemoryStream(chunk, false))
			{
				ForwardInputStream inputStream = new ForwardInputStream(agentName, chunkStream, 2, (uint)chunk.Length);
				MsbBitReader bitReader = new MsbBitReader(inputStream);

				uint ReadBits(uint count) => bitReader.ReadBits8(count);

				int ReadSignedBits(byte bits)
				{
					int ret = (int)ReadBits(bits);
					if ((ret & (1 << (bits - 1))) != 0)
						ret |= ~0 << bits;

					return ret;
				}

				uint ReadBit() => bitReader.ReadBits8(1);
				byte ReadByte() => inputStream.ReadByte();

				ForwardOutputStream outputStream = new ForwardOutputStream(agentName, rawData, 0, rawSize);

				HuffmanDecoder<byte> modDecoder = new HuffmanDecoder<byte>(agentName,
					new HuffmanCode<byte>(1, 0b0001, 0),
					new HuffmanCode<byte>(2, 0b0000, 1),
					new HuffmanCode<byte>(3, 0b0010, 2),
					new HuffmanCode<byte>(4, 0b0110, 3),
					new HuffmanCode<byte>(4, 0b0111, 4)
				);

				HuffmanDecoder<byte> lengthDecoder = new HuffmanDecoder<byte>(agentName,
					new HuffmanCode<byte>(1, 0b0000, 0),
					new HuffmanCode<byte>(2, 0b0010, 1),
					new HuffmanCode<byte>(3, 0b0110, 2),
					new HuffmanCode<byte>(4, 0b1110, 3),
					new HuffmanCode<byte>(4, 0b1111, 4)
				);

				HuffmanDecoder<byte> distanceDecoder = new HuffmanDecoder<byte>(agentName,
					new HuffmanCode<byte>(1, 0b01, 0),
					new HuffmanCode<byte>(2, 0b00, 1),
					new HuffmanCode<byte>(2, 0b01, 2)
				);

				// First byte is special
				byte currentSample = ReadByte();
				outputStream.WriteByte(currentSample);

				uint accum1 = 0, accum2 = 0, prevBits = 0;

				while (!outputStream.Eof)
				{
					byte bits = 0;
					uint count = 0;
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

						void HandleTable(uint newBits)
						{
							if ((prevBits < 2) || (newBits == 0))
								throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

							bits = bitLengthTable[prevBits - 2, newBits - 1];
							if (bits == 0)
								throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

							HandleCondCase();
						}

						uint mod = modDecoder.Decode(ReadBit);

						switch (mod)
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
									bits = (byte)prevBits;
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
								throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);
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
						uint lengthIndex = lengthDecoder.Decode(ReadBit);

						count = ReadBits(lengthBits[lengthIndex]) + lengthAdditions[lengthIndex];
						if (count >= 3)
						{
							if (accum1 != 0)
								accum1--;

							if ((count > 3) && (accum1 != 0))
								accum1--;
						}

						uint distanceIndex = distanceDecoder.Decode(ReadBit);
						uint distance = ReadBits(distanceBits[distanceIndex]) + distanceAdditions[distanceIndex];

						count = Math.Min(count, rawSize - outputStream.GetOffset());
						currentSample = outputStream.Copy(distance, count);
					}
					else
					{
						count = Math.Min(count, rawSize - outputStream.GetOffset());
						for (uint i = 0; i < count; i++)
						{
							currentSample = (byte)((sbyte)currentSample - ReadSignedBits(bits));
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
}
