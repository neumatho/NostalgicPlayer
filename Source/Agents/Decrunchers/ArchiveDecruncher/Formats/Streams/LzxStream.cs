/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.IO.Hashing;
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Archive;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Streams
{
	/// <summary>
	/// Handle decrunching of a single entry
	/// </summary>
	internal class LzxStream : ArchiveStream
	{
		private static readonly byte[] tableOne = new byte[32]
		{
			0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6,
			7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13, 14, 14
		};

		private static readonly uint[] tableTwo = new uint[32]
		{
			0, 1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 48, 64, 96, 128, 192, 256, 384, 512, 768, 1024,
			1536, 2048, 3072, 4096, 6144, 8192, 12288, 16384, 24576, 32768, 49152
		};

		private static readonly uint[] tableThree = new uint[16]
		{
			0, 1, 3, 7, 15, 31, 63, 127, 255, 511, 1023, 2047, 4095, 8191, 16383, 32767
		};

		private static readonly byte[] tableFour = new byte[34]
		{
			0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
			0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16
		};

		private readonly string agentName;
		private readonly LzxArchive.FileEntry entry;

		private int crunchedBytesRead;
		private bool readAllData;

		private readonly byte[] readBuffer;
		private readonly byte[] decrunchBuffer;

		private int decrunchBufferOffset;
		private int decrunchBufferLeft;

		private readonly Crc32 crc32;
		private int crunchedBytesLeft;
		private int decrunchedBytesLeft;

		private uint lastOffset;
		private uint globalControl;
		private int globalShift;

		private byte[] offsetLen;
		private ushort[] offsetTable;
		private byte[] huffman20Len;
		private ushort[] huffman20Table;
		private byte[] literalLen;
		private ushort[] literalTable;

		private int source;
		private int destination;
		private int sourceEnd;
		private int destinationEnd;

		private int pos;

		private uint decrunchMethod;
		private int decrunchLength;

		private bool forceDecrunch;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public LzxStream(string agentName, LzxArchive.FileEntry entry, Stream archiveStream) : base(archiveStream, true)
		{
			this.agentName = agentName;
			this.entry = entry;

			crunchedBytesRead = 0;
			readAllData = false;

			readBuffer = new byte[16384];
			decrunchBuffer = new byte[258 + 65536 + 258];	// Allow overrun for speed

			crc32 = new Crc32();
		}

		#region DecruncherStream overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the decrunched data
		/// </summary>
		/********************************************************************/
		protected override int GetDecrunchedLength()
		{
			return entry.DecrunchedSize;
		}
		#endregion

		#region ArchiveStream overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the crunched data
		/// </summary>
		/********************************************************************/
		public override int GetCrunchedLength()
		{
			return entry.Merged ? -1 : entry.CrunchedSize;
		}
		#endregion

		#region Stream implementation
		/********************************************************************/
		/// <summary>
		/// Read data from the stream
		/// </summary>
		/********************************************************************/
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (readAllData)
			{
				EndOfStream = true;
				return 0;
			}

			if (crunchedBytesRead == 0)
			{
				// This is the first read, so seek to the right position
				// and skip any bytes needed
				wrapperStream.Seek(entry.Position, SeekOrigin.Begin);

				crunchedBytesLeft = entry.CrunchedSize;
				decrunchBufferOffset = 0;
				decrunchBufferLeft = 0;

				if (entry.DecrunchedBytesToSkip != null)
					SkipBytes(entry.DecrunchedBytesToSkip);

				crc32.Reset();

				decrunchedBytesLeft = entry.DecrunchedSize;
				forceDecrunch = true;
			}

			int totalRead = 0;
			int countLeft = count;

			while (countLeft > 0)
			{
				// Time to read some more data
				ReadCrunchedBytes();

				int todo = Math.Min(countLeft, decrunchBufferLeft);
				Array.Copy(decrunchBuffer, decrunchBufferOffset, buffer, offset, todo);

				countLeft -= todo;
				offset += todo;
				decrunchBufferOffset += todo;
				decrunchBufferLeft -= todo;

				totalRead += todo;

				if ((decrunchedBytesLeft == 0) && (decrunchBufferLeft == 0))
					break;	// EOF
			}

			EndOfStream = totalRead < count;

			if ((decrunchedBytesLeft == 0) && (decrunchBufferLeft == 0))
			{
				readAllData = true;

				byte[] sum = crc32.GetCurrentHash();
				if ((sum[0] != entry.DataCrc[0]) || (sum[1] != entry.DataCrc[1]) || (sum[2] != entry.DataCrc[2]) || (sum[3] != entry.DataCrc[3]))
					throw new DecruncherException(agentName, string.Format(Resources.IDS_ARD_ERR_CRC, "Entry_Data"));
			}

			return totalRead;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Skip the number of decrunched bytes given
		/// </summary>
		/********************************************************************/
		private void SkipBytes(LzxArchive.SkipInfo[] decrunchedBytesToSkip)
		{
			foreach (LzxArchive.SkipInfo skipInfo in decrunchedBytesToSkip)
			{
				int bytesToSkip = skipInfo.DecrunchedSize;
				crc32.Reset();

				forceDecrunch = true;
				decrunchedBytesLeft = bytesToSkip;

				while (bytesToSkip > 0)
				{
					ReadCrunchedBytes();

					int todo = Math.Min(bytesToSkip, decrunchBufferLeft);
					bytesToSkip -= todo;

					decrunchBufferOffset += todo;
					decrunchBufferLeft -= todo;
				}

				byte[] sum = crc32.GetCurrentHash();
				if ((sum[0] != skipInfo.DataCrc[0]) || (sum[1] != skipInfo.DataCrc[1]) || (sum[2] != skipInfo.DataCrc[2]) || (sum[3] != skipInfo.DataCrc[3]))
					throw new DecruncherException(agentName, string.Format(Resources.IDS_ARD_ERR_CRC, "Entry_Data"));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Read next batch of crunched bytes
		/// </summary>
		/********************************************************************/
		private void ReadCrunchedBytes()
		{
			if (forceDecrunch || (decrunchBufferLeft == 0))
			{
				switch (entry.PackMode)
				{
					// Store
					case 0:
					{
						ExtractStore();
						break;
					}

					// Normal
					case 2:
					{
						ExtractNormal();
						break;
					}

					// Unknown
					default:
						throw new DecruncherException(agentName, Resources.IDS_ARD_ERR_UNKNOWN_CRUNCH_METHOD);
				}

				forceDecrunch = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Entry is not crunched, so just copy the bytes
		/// </summary>
		/********************************************************************/
		private void ExtractStore()
		{
			if (crunchedBytesRead == 0)
			{
				// First time, initialize some status variables
				decrunchedBytesLeft = entry.DecrunchedSize;
				if (decrunchedBytesLeft > crunchedBytesLeft)
					decrunchedBytesLeft = crunchedBytesLeft;
			}

			int todo = Math.Min(16384, crunchedBytesLeft);
			int read = wrapperStream.Read(decrunchBuffer, 0, todo);

			crc32.Append(decrunchBuffer.AsSpan(0, read));
			decrunchBufferOffset = 0;
			decrunchBufferLeft = read;

			crunchedBytesLeft -= read;
			decrunchedBytesLeft -= read;

			crunchedBytesRead += read;
		}



		/********************************************************************/
		/// <summary>
		/// Decrunch the data
		/// </summary>
		/********************************************************************/
		private void ExtractNormal()
		{
			if (crunchedBytesRead == 0)
			{
				// First time, initialize some status variables
				globalControl = 0;
				globalShift = -16;
				lastOffset = 1;

				decrunchLength = 0;

				crunchedBytesLeft = entry.CrunchedSize;

				offsetLen = new byte[8];
				offsetTable = new ushort[128];
				huffman20Len = new byte[20];
				huffman20Table = new ushort[96];
				literalLen = new byte[768];
				literalTable = new ushort[5120];

				sourceEnd = (source = 16384) - 1024;
				pos = destinationEnd = destination = 258 + 65536;
			}

			if (forceDecrunch || (decrunchBufferLeft == 0))
			{
				int count;

				// Time to fill the buffer?
				if (pos == destination)
				{
					int temp;

					// Check if we have enough data and read some if not
					//
					// Have we exhausted the current read buffer?
					if (source >= sourceEnd)
					{
						temp = 0;
						if ((count = temp - source + 16384) != 0)
						{
							// Copy the remaining overrun to the start of the buffer
							do
							{
								readBuffer[temp++] = readBuffer[source++];
							}
							while (--count != 0);
						}

						source = 0;
						count = source - temp + 16384;

						// Make sure we don't read too much
						if (crunchedBytesLeft < count)
							count = crunchedBytesLeft;

						if (wrapperStream.Read(readBuffer, temp, count) != count)
							throw new DecruncherException(agentName, string.Format(Resources.IDS_ARD_ERR_EOF, "Data"));

						crunchedBytesRead += count;
						crunchedBytesLeft -= count;
						temp += count;

						if (source >= temp)		// Argh! No more data
							throw new DecruncherException(agentName, string.Format(Resources.IDS_ARD_ERR_EOF, "Data"));
					}

					// Check if we need to read the tables
					if (decrunchLength <= 0)
					{
						if (ReadLiteralTable())
							throw new DecruncherException(agentName, string.Format(Resources.IDS_ARD_ERR_CANT_READ_TABLES));	// Argh! Can't make huffman tables!
					}

					// Unpack some data
					if (destination >= 258 + 65536)
					{
						if ((count = destination - 65536) != 0)
						{
							destination = 0;
							temp = 65536;

							do
							{
								decrunchBuffer[destination++] = decrunchBuffer[temp++];
							}
							while (--count != 0);
						}

						pos = destination;
					}

					destinationEnd = destination + decrunchLength;
					if (destinationEnd > 258 + 65536)
						destinationEnd = 258 + 65536;

					temp = destination;

					Decrunch();

					decrunchLength -= (destination - temp);
				}

				// Calculate amount of data we can use before we need to fill the buffer again
				count = destination - pos;
				if (count > decrunchedBytesLeft)
					count = decrunchedBytesLeft;	// Take only what we need

				crc32.Append(decrunchBuffer.AsSpan(pos, count));
				decrunchBufferLeft = count;
				decrunchBufferOffset = pos;

				decrunchedBytesLeft -= count;
				pos += count;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Read and build the decrunch tables. There better be enough data
		/// in the source buffer or it's stuffed
		/// </summary>
		/********************************************************************/
		private bool ReadLiteralTable()
		{
			uint control = globalControl;
			int shift = globalShift;
			bool abort = false;
			int temp;

			// Fix the control word if necessary
			if (shift < 0)
			{
				shift += 16;
				control += (uint)readBuffer[source++] << (8 + shift);
				control += (uint)readBuffer[source++] << shift;
			}

			// Read the decrunch method
			decrunchMethod = control & 7;
			control >>= 3;

			if ((shift -= 3) < 0)
			{
				shift += 16;
				control += (uint)readBuffer[source++] << (8 + shift);
				control += (uint)readBuffer[source++] << shift;
			}

			// Read and build the offset Huffman table
			if (decrunchMethod == 3)
			{
				for (temp = 0; temp < 8; temp++)
				{
					offsetLen[temp] = (byte)(control & 7);
					control >>= 3;

					if ((shift -= 3) < 0)
					{
						shift += 16;
						control += (uint)readBuffer[source++] << (8 + shift);
						control += (uint)readBuffer[source++] << shift;
					}
				}

				abort = MakeDecodeTable(8, 7, offsetLen, offsetTable);
			}

			// Read decrunch length
			if (!abort)
			{
				decrunchLength = (int)((control & 255) << 16);
				control >>= 8;

				if ((shift -= 8) < 0)
				{
					shift += 16;
					control += (uint)readBuffer[source++] << (8 + shift);
					control += (uint)readBuffer[source++] << shift;
				}

				decrunchLength += (int)((control & 255) << 8);
				control >>= 8;

				if ((shift -= 8) < 0)
				{
					shift += 16;
					control += (uint)readBuffer[source++] << (8 + shift);
					control += (uint)readBuffer[source++] << shift;
				}

				decrunchLength += (int)(control & 255);
				control >>= 8;

				if ((shift -= 8) < 0)
				{
					shift += 16;
					control += (uint)readBuffer[source++] << (8 + shift);
					control += (uint)readBuffer[source++] << shift;
				}
			}

			// Read and build the Huffman literal table
			if (!abort && (decrunchMethod != 1))
			{
				uint pos = 0;
				uint fix = 1;
				uint maxSymbol = 256;

				do
				{
					for (temp = 0; temp < 20; temp++)
					{
						huffman20Len[temp] = (byte)(control & 15);
						control >>= 4;

						if ((shift -= 4) < 0)
						{
							shift += 16;
							control += (uint)readBuffer[source++] << (8 + shift);
							control += (uint)readBuffer[source++] << shift;
						}
					}

					abort = MakeDecodeTable(20, 6, huffman20Len, huffman20Table);
					if (abort)
						break;	// Argh! Table is corrupt!

					do
					{
						uint symbol;

						if ((symbol = huffman20Table[control & 63]) >= 20)
						{
							// Symbol is longer than 6 bits
							do
							{
								symbol = huffman20Table[((control >> 6) & 1) + (symbol << 1)];

								if (shift-- == 0)
								{
									shift += 16;
									control += (uint)readBuffer[source++] << 24;
									control += (uint)readBuffer[source++] << 16;
								}

								control >>= 1;
							}
							while (symbol >= 20);

							temp = 6;
						}
						else
							temp = huffman20Len[symbol];

						control >>= temp;

						if ((shift -= temp) < 0)
						{
							shift += 16;
							control += (uint)readBuffer[source++] << (8 + shift);
							control += (uint)readBuffer[source++] << shift;
						}

						uint count;

						switch (symbol)
						{
							case 17:
							case 18:
							{
								if (symbol == 17)
								{
									temp = 4;
									count = 3;
								}
								else
								{
									temp = (int)(6 - fix);
									count = 19;
								}

								count += (control & tableThree[temp]) + fix;

								control >>= temp;

								if ((shift -= temp) < 0)
								{
									shift += 16;
									control += (uint)readBuffer[source++] << (8 + shift);
									control += (uint)readBuffer[source++] << shift;
								}

								while ((pos < maxSymbol) && (count-- != 0))
									literalLen[pos++] = 0;

								break;
							}

							case 19:
							{
								count = (control & 1) + 3 + fix;

								if (shift-- == 0)
								{
									shift += 16;
									control += (uint)readBuffer[source++] << 24;
									control += (uint)readBuffer[source++] << 16;
								}

								control >>= 1;

								if ((symbol = huffman20Table[control & 63]) >= 20)
								{
									// Symbol is longer than 6 bits
									do
									{
										symbol = huffman20Table[((control >> 6) & 1) + (symbol << 1)];

										if (shift-- == 0)
										{
											shift += 16;
											control += (uint)readBuffer[source++] << 24;
											control += (uint)readBuffer[source++] << 16;
										}

										control >>= 1;
									}
									while (symbol >= 20);

									temp = 6;
								}
								else
									temp = huffman20Len[symbol];

								control >>= temp;

								if ((shift -= temp) < 0)
								{
									shift += 16;
									control += (uint)readBuffer[source++] << (8 + shift);
									control += (uint)readBuffer[source++] << shift;
								}

								symbol = tableFour[literalLen[pos] + 17 - symbol];

								while ((pos < maxSymbol) && (count-- != 0))
									literalLen[pos++] = (byte)symbol;

								break;
							}

							default:
							{
								symbol = tableFour[literalLen[pos] + 17 - symbol];
								literalLen[pos++] = (byte)symbol;
								break;
							}
						}
					}
					while (pos < maxSymbol);

					fix--;
					maxSymbol += 512;
				}
				while (maxSymbol == 768);

				if (!abort)
					abort = MakeDecodeTable(768, 12, literalLen, literalTable);
			}

			globalControl = control;
			globalShift = shift;

			return abort;
		}



		/********************************************************************/
		/// <summary>
		/// Build a fast Huffman decode table from the symbol bit lengths.
		/// There is an alternate algorithm which is faster but also more
		/// complex
		/// </summary>
		/********************************************************************/
		private bool MakeDecodeTable(int numberSymbols, int tableSize, byte[] length, ushort[] table)
		{
			int symbol;
			uint leaf, tableMask, fill, nextSymbol, reverse;
			byte bitNum = 0;
			bool abort = false;

			uint pos = 0;	// Consistantly used as the current position in the decode table

			uint bitMask = tableMask = (uint)(1 << tableSize);

			bitMask >>= 1;	// Don't do the first number
			bitNum++;

			while (!abort && (bitNum <= tableSize))
			{
				for (symbol = 0; symbol < numberSymbols; symbol++)
				{
					if (length[symbol] == bitNum)
					{
						reverse = pos;	// Reverse the order of the position's bits
						leaf = 0;
						fill = (uint)tableSize;

						// Reverse the position
						do
						{
							leaf = (leaf << 1) + (reverse & 1);
							reverse >>= 1;
						}
						while (--fill != 0);

						if ((pos += bitMask) > tableMask)
						{
							abort = true;
							break;			// We will overrun the table! Abort!
						}

						fill = bitMask;
						nextSymbol = (uint)(1 << bitNum);

						do
						{
							table[leaf] = (ushort)symbol;
							leaf += nextSymbol;
						}
						while (--fill != 0);
					}
				}

				bitMask >>= 1;
				bitNum++;
			}

			if (!abort && (pos != tableMask))
			{
				// Clear the rest of the table
				for (symbol = (int)pos; symbol < tableMask; symbol++)
				{
					reverse = (uint)symbol;	// Reverse the order of the position's bits
					leaf = 0;
					fill = (uint)tableSize;

					// Reverse the position
					do
					{
						leaf = (leaf << 1) + (reverse & 1);
						reverse >>= 1;
					}
					while (--fill != 0);

					table[leaf] = 0;
				}

				nextSymbol = tableMask >> 1;
				pos <<= 16;
				tableMask <<= 16;
				bitMask = 32768;

				while (!abort && (bitNum <= 16))
				{
					for (symbol = 0; symbol < numberSymbols; symbol++)
					{
						if (length[symbol] == bitNum)
						{
							reverse = pos >> 16;	// Reverse the order of the position's bits
							leaf = 0;
							fill = (uint)tableSize;

							// Reverse the position
							do
							{
								leaf = (leaf << 1) + (reverse & 1);
								reverse >>= 1;
							}
							while (--fill != 0);

							for (fill = 0; fill < bitNum - tableSize; fill++)
							{
								if (table[leaf] == 0)
								{
									table[(nextSymbol << 1)] = 0;
									table[(nextSymbol << 1) + 1] = 0;
									table[leaf] = (ushort)nextSymbol++;
								}

								leaf = (uint)table[leaf] << 1;
								leaf += (pos >> (15 - (int)fill)) & 1;
							}

							table[leaf] = (ushort)symbol;

							if ((pos += bitMask) > tableMask)
							{
								abort = true;
								break;			// We will overrun the table! Abort!
							}
						}
					}

					bitMask >>= 1;
					bitNum++;
				}
			}

			if (pos != tableMask)
				abort = true;		// The table is incomplete!

			return abort;
		}



		/********************************************************************/
		/// <summary>
		/// Fill up the decrunch buffer. Needs lots of overrun for both
		/// destination and source buffers. Most of the time is spent in this
		/// routine so it's pretty damn optimized
		/// </summary>
		/********************************************************************/
		private void Decrunch()
		{
			uint control = globalControl;
			int shift = globalShift;

			do
			{
				uint temp;

				uint symbol = literalTable[control & 4095];
				if (symbol >= 768)
				{
					control >>= 12;

					if ((shift -= 12) < 0)
					{
						shift += 16;
						control += (uint)readBuffer[source++] << (8 + shift);
						control += (uint)readBuffer[source++] << shift;
					}

					// Literal is longer than 12 bits
					do
					{
						symbol = literalTable[(control & 1) + (symbol << 1)];

						if (shift-- == 0)
						{
							shift += 16;
							control += (uint)readBuffer[source++] << 24;
							control += (uint)readBuffer[source++] << 16;
						}

						control >>= 1;
					}
					while (symbol >= 768);
				}
				else
				{
					temp = literalLen[symbol];
					control >>= (int)temp;

					if ((shift -= (int)temp) < 0)
					{
						shift += 16;
						control += (uint)readBuffer[source++] << (8 + shift);
						control += (uint)readBuffer[source++] << shift;
					}
				}

				if (symbol < 256)
					decrunchBuffer[destination++] = (byte)symbol;
				else
				{
					symbol -= 256;
					uint count = tableTwo[temp = symbol & 31];
					temp = tableOne[temp];

					if ((temp >= 3) && (decrunchMethod == 3))
					{
						temp -= 3;
						count += ((control & tableThree[temp]) << 3);
						control >>= (int)temp;

						if ((shift -= (int)temp) < 0)
						{
							shift += 16;
							control += (uint)readBuffer[source++] << (8 + shift);
							control += (uint)readBuffer[source++] << shift;
						}

						count += (temp = offsetTable[control & 127]);
						temp = offsetLen[temp];
					}
					else
					{
						count += control & tableThree[temp];
						if (count == 0)
							count = lastOffset;
					}

					control >>= (int)temp;
					if ((shift -= (int)temp) < 0)
					{
						shift += 16;
						control += (uint)readBuffer[source++] << (8 + shift);
						control += (uint)readBuffer[source++] << shift;
					}

					lastOffset = count;

					count = tableTwo[temp = (symbol >> 5) & 15] + 3;
					temp = tableOne[temp];
					count += (control & tableThree[temp]);

					control >>= (int)temp;
					if ((shift -= (int)temp) < 0)
					{
						shift += 16;
						control += (uint)readBuffer[source++] << (8 + shift);
						control += (uint)readBuffer[source++] << shift;
					}

					int str = lastOffset < destination ? (int)(destination - lastOffset) : (int)(destination + 65536 - lastOffset);

					do
					{
						decrunchBuffer[destination++] = decrunchBuffer[str++];
					}
					while (--count != 0);
				}
			}
			while ((destination < destinationEnd) && (source < sourceEnd));

			globalControl = control;
			globalShift = shift;
		}
		#endregion
	}
}
