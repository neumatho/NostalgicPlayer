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
	/// This stream read data crunched with PowerPacker
	/// </summary>
	internal class PowerPackerStream : AncientStream
	{
		private byte[] decrunchedData;
		private int bufferIndex;

		private uint dataStart;
		private uint rawSize;
		private byte startShift;

		private readonly byte[] modeTable = new byte[4];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PowerPackerStream(string agentName, Stream wrapperStream) : base(agentName, wrapperStream)
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
			dataStart = (uint)(wrapperStream.Length - 4);

			using (ReaderStream readerStream = new ReaderStream(wrapperStream, true))
			{
				readerStream.Seek(4, SeekOrigin.Begin);
				uint mode = readerStream.Read_B_UINT32();

				for (int i = 0; i < 4; i++)
				{
					modeTable[i] = (byte)(mode >> 24);
					mode <<= 8;
				}

				readerStream.Seek(dataStart, SeekOrigin.Begin);
				uint tmp = readerStream.Read_B_UINT32();

				rawSize = tmp >> 8;
				startShift = (byte)(tmp & 0xff);

				if ((rawSize == 0) || (startShift >= 0x20) || (rawSize > 0x1000000U))
					throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);
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
			if (rawData.Length < rawSize)
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

			BackwardInputStream inputStream = new BackwardInputStream(agentName, wrapperStream, 8, dataStart);
			LsbBitReader bitReader = new LsbBitReader(inputStream);

			uint ReadBits(uint count) => Common.Common.RotateBits(bitReader.ReadBitsBe32(count), count);
			uint ReadBit() => bitReader.ReadBitsBe32(1);

			ReadBits(startShift);

			BackwardOutputStream outputStream = new BackwardOutputStream(agentName, rawData, 0, rawSize);

			for (;;)
			{
				uint count;

				if (ReadBit() == 0)
				{
					count = 1;

					// This does not make much sense I know. But it is what it is...
					for (;;)
					{
						uint tmp = ReadBits(2);
						count += tmp;

						if (tmp < 3)
							break;
					}

					for (uint i = 0; i < count; i++)
						outputStream.WriteByte(ReadBits(8));
				}

				if (outputStream.Eof)
					break;

				uint modeIndex = ReadBits(2);
				uint distance;

				if (modeIndex == 3)
				{
					distance = ReadBits((uint)(ReadBit() != 0 ? modeTable[modeIndex] : 7)) + 1;

					// Ditto
					count = 5;
					for (;;)
					{
						uint tmp = ReadBits(3);
						count += tmp;

						if (tmp < 7)
							break;
					}
				}
				else
				{
					count = modeIndex + 2;
					distance = ReadBits(modeTable[modeIndex]) + 1;
				}

				outputStream.Copy(distance, count);
			}
		}
		#endregion
	}
}
