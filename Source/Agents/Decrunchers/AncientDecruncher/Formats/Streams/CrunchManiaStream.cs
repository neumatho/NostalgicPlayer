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
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats.Streams
{
	/// <summary>
	/// This stream read data crunched with Crunch Mania
	/// </summary>
	internal class CrunchManiaStream : AncientStream
	{
		private static readonly byte[] lengthBits = { 1, 2, 4, 8 };
		private static readonly uint[] lengthAdditions = { 2, 4, 8, 24 };

		private static readonly byte[] distanceBits = { 9, 5, 14 };
		private static readonly uint[] distanceAdditions = { 32, 0, 544 };

		private uint rawSize;
		private uint crunchedSize;

		private bool isSampled;
		private bool isLzh;

		private byte[] decrunchedData;
		private int bufferIndex;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CrunchManiaStream(string agentName, Stream wrapperStream) : base(agentName, wrapperStream)
		{
			ReadAndDecrunch();
		}

		#region Stream implementation
		/********************************************************************/
		/// <summary>
		/// Indicate if the stream supports seeking
		/// </summary>
		/********************************************************************/
		public override bool CanSeek => true;



		/********************************************************************/
		/// <summary>
		/// Return the current position
		/// </summary>
		/********************************************************************/
		public override long Position
		{
			get => bufferIndex;

			set => bufferIndex = (int)value;
		}



		/********************************************************************/
		/// <summary>
		/// Seek to a new position
		/// </summary>
		/********************************************************************/
		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
				{
					bufferIndex = (int)offset;
					break;
				}

				case SeekOrigin.Current:
				{
					bufferIndex += (int)offset;
					break;
				}

				case SeekOrigin.End:
				{
					bufferIndex = decrunchedData.Length + (int)offset;
					break;
				}
			}

			if (bufferIndex < 0)
				bufferIndex = 0;
			else if (bufferIndex > decrunchedData.Length)
				bufferIndex = decrunchedData.Length;

			return bufferIndex;
		}



		/********************************************************************/
		/// <summary>
		/// Read data from the stream
		/// </summary>
		/********************************************************************/
		public override int Read(byte[] buffer, int offset, int count)
		{
			int todo = Math.Min(count, decrunchedData.Length - bufferIndex);
			if (todo > 0)
			{
				Array.Copy(decrunchedData, bufferIndex, buffer, offset, todo);
				bufferIndex += todo;
			}

			return todo;
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
			return decrunchedData.Length;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read and decrunch data
		/// </summary>
		/********************************************************************/
		private void ReadAndDecrunch()
		{
			using (ReaderStream readerStream = new ReaderStream(wrapperStream, true))
			{
				readerStream.Seek(0, SeekOrigin.Begin);

				uint hdr = readerStream.Read_B_UINT32();

				readerStream.Seek(6, SeekOrigin.Begin);

				rawSize = readerStream.Read_B_UINT32();
				crunchedSize = readerStream.Read_B_UINT32();

				if ((rawSize == 0) || (crunchedSize == 0) || (rawSize > 0x1000000) || (crunchedSize > 0x1000000) || OverflowCheck.Sum(crunchedSize, 14) > readerStream.Length)
					throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

				isSampled = ((hdr >> 8) & 0xff) == 'm';
				isLzh = (hdr & 0xff) == '2';
			}

			decrunchedData = new byte[rawSize];

			Decompress(decrunchedData);
		}



		/********************************************************************/
		/// <summary>
		/// Decompress the file and store the result in the output buffer
		/// </summary>
		/********************************************************************/
		private void Decompress(byte[] rawData)
		{
			if (rawData.Length != rawSize)
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

			BackwardInputStream inputStream = new BackwardInputStream(agentName, wrapperStream, 14, crunchedSize + 14 - 6);
			LsbBitReader bitReader = new LsbBitReader(inputStream);

			{
				// There are empty bits?!? at the start of the stream. Take them out
				using (ReaderStream readerStream = new ReaderStream(wrapperStream, true))
				{
					uint bufOffset = crunchedSize + 14 - 6;
					readerStream.Seek(bufOffset, SeekOrigin.Begin);

					uint originalBitsContent = readerStream.Read_B_UINT32();
					ushort originalShift = readerStream.Read_B_UINT16();
					byte bufBitsLength = (byte)(originalShift + 16);
					uint bufBitsContent = originalBitsContent >> (16 - originalShift);
					bitReader.Reset(bufBitsContent, bufBitsLength);
				}
			}

			uint ReadBits(uint count) => bitReader.ReadBits8(count);
			uint ReadBit() => bitReader.ReadBits8(1);

			BackwardOutputStream outputStream = new BackwardOutputStream(agentName, rawData, 0, rawSize);

			if (isLzh)
			{
				void ReadHuffmanTable(HuffmanDecoder<uint> dec, uint codeLength)
				{
					uint maxDepth = ReadBits(4);
					if (maxDepth == 0)
						throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

					uint[] lengthTable = new uint[15];

					for (uint i = 0; i < maxDepth; i++)
						lengthTable[i] = ReadBits(Math.Min(i + 1, codeLength));

					uint code = 0;
					for (uint depth = 1; depth <= maxDepth; depth++)
					{
						for (uint i = 0; i < lengthTable[depth - 1]; i++)
						{
							uint value = ReadBits(codeLength);

							dec.Insert(new HuffmanCode<uint>(depth, code >> (int)(maxDepth - depth), value));
							code += (uint)(1 << (int)(maxDepth - depth));
						}
					}
				}

				do
				{
					HuffmanDecoder<uint> lengthDecoder = new HuffmanDecoder<uint>(agentName);
					HuffmanDecoder<uint> distanceDecoder = new HuffmanDecoder<uint>(agentName);

					ReadHuffmanTable(lengthDecoder, 9);
					ReadHuffmanTable(distanceDecoder, 4);

					uint items = ReadBits(16) + 1;
					for (uint i = 0; i < items; i++)
					{
						uint count = lengthDecoder.Decode(ReadBit);

						if ((count & 0x100) != 0)
							outputStream.WriteByte(count);
						else
						{
							count += 3;

							uint distanceBits = distanceDecoder.Decode(ReadBit);
							uint distance;

							if (distanceBits == 0)
								distance = ReadBits(1) + 1;
							else
								distance = (ReadBits(distanceBits) | (uint)(1 << (int)distanceBits)) + 1;

							outputStream.Copy(distance, count);
						}
					}
				}
				while (ReadBit() != 0);
			}
			else
			{
				HuffmanDecoder<byte> lengthDecoder = new HuffmanDecoder<byte>(agentName,
					new HuffmanCode<byte>(1, 0b000, 0),
					new HuffmanCode<byte>(2, 0b010, 1),
					new HuffmanCode<byte>(3, 0b110, 2),
					new HuffmanCode<byte>(3, 0b111, 3)
				);

				HuffmanDecoder<byte> distanceDecoder = new HuffmanDecoder<byte>(agentName,
					new HuffmanCode<byte>(1, 0b00, 0),
					new HuffmanCode<byte>(2, 0b10, 1),
					new HuffmanCode<byte>(2, 0b11, 2)
				);

				while (!outputStream.Eof)
				{
					if (ReadBit() != 0)
						outputStream.WriteByte(ReadBits(8));
					else
					{
						byte lengthIndex = lengthDecoder.Decode(ReadBit);
						uint count = ReadBits(lengthBits[lengthIndex]) + lengthAdditions[lengthIndex];

						if (count == 23)
						{
							if (ReadBit() != 0)
								count = ReadBits(5) + 15;
							else
								count = ReadBits(14) + 15;

							for (uint i = 0; i < count; i++)
								outputStream.WriteByte(ReadBits(8));
						}
						else
						{
							if (count > 23)
								count--;

							byte distanceIndex = distanceDecoder.Decode(ReadBit);
							uint distance = ReadBits(distanceBits[distanceIndex]) + distanceAdditions[distanceIndex];

							outputStream.Copy(distance, count);
						}
					}
				}
			}

			if (!outputStream.Eof)
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

			if (isSampled)
				DltaDecode.Decode(agentName, rawData, rawData, 0, rawSize);
		}
		#endregion
	}
}
