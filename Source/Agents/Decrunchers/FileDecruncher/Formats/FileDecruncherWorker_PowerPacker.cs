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
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.FileDecruncher.Formats
{
	/// <summary>
	/// Can depack PowerPacker data files
	/// </summary>
	internal class FileDecruncherWorker_PowerPacker : FileDecruncherAgentBase
	{
		private byte[] sourceBuffer;
		private int sourceIndex;
		private uint counter;
		private uint shiftIn;

		#region IFileDecruncherAgent implementation
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(ModuleStream moduleStream)
		{
			// Check the file size
			if (moduleStream.Length < 12)
				return AgentResult.Unknown;

			// Check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);

			if (moduleStream.Read_B_UINT32() != 0x50503230)		// PP20
				return AgentResult.Unknown;

			// Check the offset sizes
			if ((moduleStream.Read_UINT8() > 16) || (moduleStream.Read_UINT8() > 16) || (moduleStream.Read_UINT8() > 16) || (moduleStream.Read_UINT8() > 16))
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Return the size of the depacked data
		/// </summary>
		/********************************************************************/
		public override int GetDepackedLength(ModuleStream moduleStream)
		{
			// Seek to the last 4 bytes
			moduleStream.Seek(-4, SeekOrigin.End);

			return (int)moduleStream.Read_B_UINT32() >> 8;
		}



		/********************************************************************/
		/// <summary>
		/// Depack the file and store the result in the buffer given
		/// </summary>
		/********************************************************************/
		public override AgentResult Depack(byte[] source, byte[] destination, int safetySize, out string errorMessage)
		{
			errorMessage = string.Empty;

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

						if (destinationIndex <= safetySize)
							break;				// Stop depacking
					}

					// Decode what to copy from the destination file
					uint idx = GetBits(2);
					if (idx > 3)
					{
						errorMessage = Resources.IDS_ERR_CORRUPT_DATA;
						return AgentResult.Error;
					}

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

					if (destinationIndex <= safetySize)
						break;					// Stop depacking
				}

				// Check to see if the file is corrupt
				if (destinationIndex < safetySize)
				{
					errorMessage = Resources.IDS_ERR_CORRUPT_DATA;
					return AgentResult.Error;
				}

				// Copy the data back in memory
				Array.Copy(destination, safetySize, destination, 0, destination.Length - safetySize);
			}
			finally
			{
				sourceBuffer = null;
			}

			return AgentResult.Ok;
		}
		#endregion

		#region Private methods
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
