/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.FileDecruncher.Formats.Streams
{
	/// <summary>
	/// This stream read data packed with PowerPacker
	/// </summary>
	internal class PowerPackerStream : DepackerStream
	{
		private const int SafetySize = 1024;

		private readonly string agentName;

		private byte[] depackedData;
		private int bufferIndex;

		private byte[] sourceBuffer;
		private int sourceIndex;
		private uint counter;
		private uint shiftIn;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PowerPackerStream(string agentName, Stream wrapperStream) : base(wrapperStream)
		{
			this.agentName = agentName;

			ReadAndUnpack();
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
			get => bufferIndex - SafetySize;

			set => bufferIndex = (int)value + SafetySize;
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
					bufferIndex = (int)offset + SafetySize;
					break;
				}

				case SeekOrigin.Current:
				{
					bufferIndex += (int)offset;
					break;
				}

				case SeekOrigin.End:
				{
					bufferIndex = depackedData.Length + (int)offset;
					break;
				}
			}

			if (bufferIndex < SafetySize)
				bufferIndex = SafetySize;
			else if (bufferIndex > depackedData.Length)
				bufferIndex = depackedData.Length;

			return bufferIndex - SafetySize;
		}



		/********************************************************************/
		/// <summary>
		/// Read data from the stream
		/// </summary>
		/********************************************************************/
		public override int Read(byte[] buffer, int offset, int count)
		{
			int todo = Math.Min(count, depackedData.Length - bufferIndex - SafetySize);
			if (todo > 0)
			{
				Array.Copy(depackedData, bufferIndex, buffer, offset, todo);
				bufferIndex += todo;
			}

			return todo;
		}
		#endregion

		#region DepackerStream overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the depacked data
		/// </summary>
		/********************************************************************/
		protected override int GetDepackedLength()
		{
			return depackedData.Length - SafetySize;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read and unpack data
		/// </summary>
		/********************************************************************/
		private void ReadAndUnpack()
		{
			wrapperStream.Seek(0, SeekOrigin.Begin);

			// Because PowerPacker depack the data backwards, we need to depack
			// the whole file, before we can return anything
			byte[] source = new byte[wrapperStream.Length];
			int readBytes = wrapperStream.Read(source, 0, source.Length);
			if (readBytes != source.Length)
				throw new DepackerException(agentName, Resources.IDS_ERR_CORRUPT_DATA);

			// Find the length of unpacked data
			int offset = source.Length - 4;
			int unpackedLength = (source[offset] << 16) | (source[offset + 1] << 8) | source[offset + 2];

			depackedData = new byte[unpackedLength + SafetySize];
			bufferIndex = SafetySize;

			Depack(source, depackedData);
		}



		/********************************************************************/
		/// <summary>
		/// Depack the file and store the result in the buffer given
		/// </summary>
		/********************************************************************/
		private void Depack(byte[] source, byte[] destination)
		{
			// Get the offset sizes
			byte[] offsetSizes = new byte[4];

			offsetSizes[0] = source[4];
			offsetSizes[1] = source[5];
			offsetSizes[2] = source[6];
			offsetSizes[3] = source[7];

			// Set the buffer indexes
			sourceBuffer = source;
			sourceIndex = source.Length - 4;
			int destinationIndex = destination.Length;

			try
			{
				counter = 0;

				// Skip bits
				GetBits(source[sourceIndex + 3]);

				// Do it forever, i.e., while the whole file isn't depacked
				for (;;)
				{
					uint bytes, toAdd;

					// Copy some bytes from the source anyway
					if (GetBits(1) == 0)
					{
						bytes = 0;

						do
						{
							toAdd = GetBits(2);
							bytes += toAdd;
						}
						while (toAdd == 3);

						for (int i = 0; i <= bytes; i++)
							destination[--destinationIndex] = (byte)GetBits(8);

						if (destinationIndex <= SafetySize)
							break;				// Stop depacking
					}

					// Decode what to copy from the destination file
					uint idx = GetBits(2);
					if (idx > 3)
						throw new DepackerException(agentName, Resources.IDS_ERR_CORRUPT_DATA);

					byte numBits = offsetSizes[idx];

					// Bytes to copy
					bytes = idx + 1;

					uint offset;

					if (bytes == 4)			// 4 means >= 4
					{
						// And maybe a biffer offset
						if (GetBits(1) == 0)
							offset = GetBits(7);
						else
							offset = GetBits(numBits);

						do
						{
							toAdd = GetBits(3);
							bytes += toAdd;
						}
						while (toAdd == 7);
					}
					else
						offset = GetBits(numBits);

					for (int i = 0; i <= bytes; i++)
					{
						destination[destinationIndex - 1] = destination[destinationIndex + offset];
						destinationIndex--;
					}

					if (destinationIndex <= SafetySize)
						break;					// Stop depacking
				}

				// Check to see if the file is corrupt
				if (destinationIndex < SafetySize)
					throw new DepackerException(agentName, Resources.IDS_ERR_CORRUPT_DATA);
			}
			finally
			{
				sourceBuffer = null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Get a number of bits from the packed data and return it
		/// </summary>
		/********************************************************************/
		private uint GetBits(uint num)
		{
			uint result = 0;

			for (uint i = 0; i < num; i++)
			{
				if (counter == 0)
				{
					counter = 8;
					shiftIn = sourceBuffer[--sourceIndex];
				}

				result = (result << 1) | (shiftIn & 1);
				shiftIn >>= 1;
				counter--;
			}

			return result;
		}
		#endregion
	}
}
