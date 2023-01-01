/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// The Player base class for version 4.0A, 4.0B and 4.1A
	/// </summary>
	internal abstract class ThePlayer4xFormatBase : ProWizardConverterWorker31SamplesBase
	{
		private class P4xxChannel
		{
			public byte[] P4xPatternData = new byte[3];
			public sbyte P4xInfo;
			public ushort RepeatLines;
			public long OldPosition = -1;
			public byte[] ProPatternData = new byte[4];
		}

		private byte numberOfSamples;
		private byte numberOfPositions;
		private ushort[,] trackOffsetTable;
		private byte[] positionList;

		private uint trackDataOffset;
		private uint trackTableOffset;
		private uint sampleDataOffset;
		protected uint firstSampleAddress;

		private bool skipLastSample;
		private uint[] sampleStartOffsets;

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Prepare conversion by initialize what is needed etc.
		/// </summary>
		/********************************************************************/
		protected override bool PrepareConversion(ModuleStream moduleStream)
		{
			// Get different information
			moduleStream.Seek(5, SeekOrigin.Begin);

			numberOfPositions = moduleStream.Read_UINT8();
			numberOfSamples = moduleStream.Read_UINT8();

			moduleStream.Seek(1, SeekOrigin.Current);
			trackDataOffset = moduleStream.Read_B_UINT32();
			trackTableOffset = moduleStream.Read_B_UINT32();
			sampleDataOffset = moduleStream.Read_B_UINT32();

			firstSampleAddress = moduleStream.Read_B_UINT32();

			FixAddresses(firstSampleAddress, numberOfSamples, ref trackDataOffset, ref trackTableOffset, ref sampleDataOffset);

			CreateOffsetTable(moduleStream);
			CreatePositionList();

			if (skipLastSample)
				numberOfSamples--;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			sampleStartOffsets = new uint[numberOfSamples];

			moduleStream.Seek(20, SeekOrigin.Begin);

			for (int i = 0, cnt = Math.Min((int)numberOfSamples, 31); i < cnt; i++)
			{
				yield return ReadSampleInfo(moduleStream, out uint startOffset);
				sampleStartOffsets[i] = startOffset - firstSampleAddress;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the position list. Note that the returned list must be the
		/// same length as to be played
		/// </summary>
		/********************************************************************/
		protected override Span<byte> GetPositionList(ModuleStream moduleStream)
		{
			return positionList;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			byte[] pattern = new byte[1024];
			int lastPatternNumber = -1;

			for (int i = 0; i < numberOfPositions; i++)
			{
				if (positionList[i] > lastPatternNumber)
				{
					lastPatternNumber++;

					// Clear the pattern data
					Array.Clear(pattern);

					// Allocate channel structures
					P4xxChannel[] channels = Helpers.InitializeArray<P4xxChannel>(4);

					// Build each channel from the tracks
					int maxRowNumber = 64;

					for (int j = 0; j < 4; j++)
					{
						ushort trackOffset = trackOffsetTable[j, i];
						moduleStream.Seek(trackDataOffset + 4 + trackOffset, SeekOrigin.Begin);

						for (int k = 0; k < maxRowNumber; k++)
						{
							if (ConvertData(moduleStream, channels[j], pattern, k * 16 + j * 4))
							{
								// Got a break (like D or B)
								maxRowNumber = k + 1;
								break;
							}
						}
					}

					yield return pattern;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write all the samples
		/// </summary>
		/********************************************************************/
		protected override bool WriteSampleData(ModuleStream moduleStream, ConverterStream converterStream)
		{
			uint sampleOffset = sampleDataOffset + 4;

			for (int i = 0, cnt = Math.Min((int)numberOfSamples, 31); i < cnt; i++)
			{
				int length = (int)sampleLengths[i];
				if (length != 0)
				{
					moduleStream.Seek(sampleOffset + sampleStartOffsets[i], SeekOrigin.Begin);

					// Check to see if we miss too much from the last sample
					if (moduleStream.Length - moduleStream.Position < (length - MaxNumberOfMissingBytes))
						return false;

					moduleStream.SetSampleDataInfo(i, length);
					converterStream.WriteSampleDataMarker(i, length);
				}
			}

			return true;
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Check the sample information and return the total size of all
		/// the samples
		/// </summary>
		/********************************************************************/
		protected abstract uint CheckSampleInfo(ModuleStream moduleStream, byte sampleCount, out uint firstAddress, out ushort lastLength);



		/********************************************************************/
		/// <summary>
		/// Read the next sample information and return it
		/// </summary>
		/********************************************************************/
		protected abstract SampleInfo ReadSampleInfo(ModuleStream moduleStream, out uint startOffset);
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected bool CheckForThePlayerFormat(ModuleStream moduleStream, uint mark)
		{
			if (moduleStream.Length < 20)
				return false;

			// Start to check the ID
			moduleStream.Seek(0, SeekOrigin.Begin);

			if (moduleStream.Read_B_UINT32() != mark)
				return false;

			// Get number of positions
			moduleStream.Seek(1, SeekOrigin.Current);
			byte positionListLength = moduleStream.Read_UINT8();

			// Get number of samples
			byte sampleCount = moduleStream.Read_UINT8();

			// Get all table offsets
			moduleStream.Seek(1, SeekOrigin.Current);
			uint tdo = moduleStream.Read_B_UINT32();
			uint tto = moduleStream.Read_B_UINT32();
			uint sdo = moduleStream.Read_B_UINT32();

			// Check the sample information
			uint samplesSize = CheckSampleInfo(moduleStream, sampleCount, out uint firstAddress, out ushort lastLength);
			if (samplesSize == 0)
				return false;

			// Check end mark in position table
			moduleStream.Seek(positionListLength * 8 + (sampleCount + 1) * 16 + 4, SeekOrigin.Begin);
			if (moduleStream.Read_B_UINT16() != 0xffff)
				return false;

			FixAddresses(firstAddress, sampleCount, ref tdo, ref tto, ref sdo);

			// Find end of module
			uint endOffset = sdo + 4 + samplesSize;

			if (endOffset > (moduleStream.Length + MaxNumberOfMissingBytes))
			{
				// SPECIAL! Sometimes, there is a useless sample at the end
				//
				// Find offset to last sample
				lastLength *= 2;
				uint temp = endOffset - lastLength;

				if (temp > (moduleStream.Length + MaxNumberOfMissingBytes))
					return false;

				skipLastSample = true;
			}
			else
				skipLastSample = false;

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Fix the given addresses if needed
		/// </summary>
		/********************************************************************/
		private void FixAddresses(uint firstAddress, byte sampleCount, ref uint tdo, ref uint tto, ref uint sdo)
		{
			if (firstAddress != 0)
			{
				uint temp = (sampleCount + 1U) * 16;
				uint temp1 = tto - temp;

				tto = temp;
				tdo -= temp1;
				sdo -= temp1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Create the track offset table
		/// </summary>
		/********************************************************************/
		private void CreateOffsetTable(ModuleStream moduleStream)
		{
			moduleStream.Seek(20 + numberOfSamples * 16, SeekOrigin.Begin);
			trackOffsetTable = new ushort[4, numberOfPositions];

			for (int i = 0; i < numberOfPositions; i++)
			{
				for (int j = 0; j < 4; j++)
					trackOffsetTable[j, i] = (ushort)(moduleStream.Read_B_UINT16() & 0xfffe);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Create position list and find the number of patterns needed
		/// </summary>
		/********************************************************************/
		private void CreatePositionList()
		{
			positionList = BuildPositionList(numberOfPositions, (pos) =>
				((ulong)trackOffsetTable[0, pos] << 48) | ((ulong)trackOffsetTable[1, pos] << 32) | ((ulong)trackOffsetTable[2, pos] << 16) | trackOffsetTable[3, pos]);
		}



		/********************************************************************/
		/// <summary>
		/// Will convert a single P4xx track line to a ProTracker track line
		/// </summary>
		/********************************************************************/
		private bool ConvertData(ModuleStream moduleStream, P4xxChannel channel, byte[] pattern, int writeOffset)
		{
			if (channel.P4xInfo < 0)
			{
				// The same data as the last one
				channel.P4xInfo++;

				// Copy the last pattern data
				pattern[writeOffset++] = channel.ProPatternData[0];
				pattern[writeOffset++] = channel.ProPatternData[1];
				pattern[writeOffset++] = channel.ProPatternData[2];
				pattern[writeOffset] = channel.ProPatternData[3];

				return false;
			}

			if (channel.P4xInfo > 0)
			{
				// Empty line
				channel.P4xInfo--;

				// Clear pattern data
				pattern[writeOffset++] = 0x00;
				pattern[writeOffset++] = 0x00;
				pattern[writeOffset++] = 0x00;
				pattern[writeOffset] = 0x00;

				return false;
			}

			// Read a new line in the module
			byte temp;

			// Do we copy from another place in the module?
			if (channel.RepeatLines != 0)
			{
				// Yes, read the next previous line
				channel.RepeatLines--;

				channel.P4xPatternData[0] = moduleStream.Read_UINT8();
				channel.P4xPatternData[1] = moduleStream.Read_UINT8();
				channel.P4xPatternData[2] = moduleStream.Read_UINT8();
				channel.P4xInfo = moduleStream.Read_INT8();
			}
			else
			{
				if (channel.OldPosition != -1)
				{
					moduleStream.Seek(channel.OldPosition, SeekOrigin.Begin);
					channel.OldPosition = -1;
				}

				// Check to see if it's a repeat command
				temp = moduleStream.Read_UINT8();

				if ((temp & 0x80) != 0)
				{
					// It is, get the lines to repeat
					channel.RepeatLines = moduleStream.Read_UINT8();

					// Get the offset to repeat from
					ushort offset = moduleStream.Read_B_UINT16();

					// Seek to the new position, but remember the current one first
					channel.OldPosition = moduleStream.Position;
					moduleStream.Seek(trackDataOffset + offset + 4, SeekOrigin.Begin);

					// Read the first line
					channel.P4xPatternData[0] = moduleStream.Read_UINT8();
					channel.P4xPatternData[1] = moduleStream.Read_UINT8();
					channel.P4xPatternData[2] = moduleStream.Read_UINT8();
					channel.P4xInfo = moduleStream.Read_INT8();
				}
				else
				{
					// Just a normal line
					channel.P4xPatternData[0] = temp;
					channel.P4xPatternData[1] = moduleStream.Read_UINT8();
					channel.P4xPatternData[2] = moduleStream.Read_UINT8();
					channel.P4xInfo = moduleStream.Read_INT8();
				}
			}

			// Convert the pattern data
			if ((channel.P4xPatternData[0] & 0x1) != 0)
				channel.ProPatternData[0] = 0x10;		// Set hi bit in sample number
			else
				channel.ProPatternData[0] = 0x00;

			// Get note number
			temp = (byte)(channel.P4xPatternData[0] & 0xfe);
			if (temp != 0)
			{
				temp /= 2;
				temp--;

				if (temp < periods.GetLength(0))
				{
					channel.ProPatternData[0] |= periods[temp, 0];
					channel.ProPatternData[1] = periods[temp, 1];
				}
				else
					channel.ProPatternData[1] = 0x00;
			}
			else
				channel.ProPatternData[1] = 0x00;

			// Sample number + effect + effect value
			channel.ProPatternData[2] = channel.P4xPatternData[1];
			channel.ProPatternData[3] = channel.P4xPatternData[2];

			// Check the effect
			bool result = false;

			switch (channel.ProPatternData[2] & 0x0f)
			{
				// Position jump
				case 0xb:
				{
					channel.ProPatternData[3] /= 2;
					result = true;
					break;
				}

				// Pattern break
				case 0xd:
				{
					// Convert the argument from decimal to hex
					channel.ProPatternData[3] = (byte)((channel.ProPatternData[3] / 0x10) * 10 + (channel.ProPatternData[3] % 0x10));
					result = true;
					break;
				}

				// Exx
				case 0xe:
				{
					// Filter
					if ((channel.ProPatternData[3] & 0xf0) == 0x00)
					{
						if (channel.ProPatternData[3] == 0x02)
							channel.ProPatternData[3] = 0x01;
					}
					else
					{
						// Note cut
						if ((channel.ProPatternData[2] & 0xf0) == 0xc0)
							channel.ProPatternData[3]++;
					}
					break;
				}

				// Volume slide
				// Tone portamento + Volume slide
				// Vibrato + Volume slide
				case 0xa:
				case 0x5:
				case 0x6:
				{
					if (channel.ProPatternData[3] >= 0x80)
					{
						channel.ProPatternData[3] = (byte)((byte)(-(sbyte)channel.ProPatternData[3]) << 4);

						if (channel.ProPatternData[3] >= 0x80)
							channel.ProPatternData[3] = (byte)-(sbyte)channel.ProPatternData[3];
					}
					break;
				}

				// Arpeggio
				case 0x8:
				{
					channel.ProPatternData[2] &= 0xf0;
					break;
				}
			}

			// Store the pattern data in the pattern
			pattern[writeOffset++] = channel.ProPatternData[0];
			pattern[writeOffset++] = channel.ProPatternData[1];
			pattern[writeOffset++] = channel.ProPatternData[2];
			pattern[writeOffset] = channel.ProPatternData[3];

			return result;
		}
		#endregion
	}
}
