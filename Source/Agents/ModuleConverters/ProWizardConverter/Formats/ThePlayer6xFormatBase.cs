/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// The Player base class for version 5.0A, 6.0A and 6.1A
	/// </summary>
	internal abstract class ThePlayer6xFormatBase : ProWizardConverterWorkerBase
	{
		private static sbyte[] packTable =
		{
			0, 1, 2, 4, 8, 16, 32, 64, -128, -64, -32, -16, -8, -4, -2, -1
		};

		private uint trackDataOffset;
		private uint sampleDataOffset;

		private byte numberOfSamples;
		private bool packedSamples;
		private bool deltaSamples;

		private uint[] sampleStartOffsets;
		private bool[] samplePacked;

		private ushort[] trackOffsetTable;

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Prepare conversion by initialize what is needed etc.
		/// </summary>
		/********************************************************************/
		protected override bool PrepareConversion(ModuleStream moduleStream)
		{
			// Get different information
			GotId(moduleStream);	// Just to make sure the file pointer is at the right position

			sampleDataOffset = moduleStream.Read_B_UINT16();
			moduleStream.Seek(1, SeekOrigin.Current);
			numberOfSamples = moduleStream.Read_UINT8();
			packedSamples = (numberOfSamples & 0x40) != 0;
			deltaSamples = (numberOfSamples & 0x80) != 0;
			numberOfSamples &= 0x1f;

			if (packedSamples)
				moduleStream.Seek(4, SeekOrigin.Current);

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
			samplePacked = new bool[numberOfSamples];
			uint startOffset = 0;

			for (int i = 0; i < numberOfSamples; i++)
			{
				ushort length = moduleStream.Read_B_UINT16();
				byte fineTune = moduleStream.Read_UINT8();
				byte volume = moduleStream.Read_UINT8();
				ushort loopStart = moduleStream.Read_B_UINT16();

				if (length >= 0x8000)
				{
					// Special case. Repeat another sample
					length = unchecked((ushort)-(short)length);		// Find sample index

					// Get the offset to the sample
					sampleStartOffsets[i] = sampleStartOffsets[length - 1];

					// Get new sample length
					length = (ushort)(sampleLengths[length - 1] / 2);
				}
				else
				{
					sampleStartOffsets[i] = startOffset;
					startOffset += length;

					if ((fineTune & 0x80) == 0)
						startOffset += length;
				}

				ushort loopLength;

				if (loopStart == 0xffff)
				{
					// No loop
					loopStart = 0x0000;
					loopLength = 0x0001;
				}
				else
				{
					// Loop
					loopLength = (ushort)(length - loopStart);
				}

				samplePacked[i] = (fineTune & 0x80) != 0;

				yield return new SampleInfo
				{
					Name = null,
					Length = length,
					LoopStart = loopStart,
					LoopLength = loopLength,
					Volume = volume,
					FineTune = (byte)(fineTune & 0x0f)
				};
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the patterns
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<byte[]> GetPatterns(ModuleStream moduleStream)
		{
			byte[] pattern = new byte[1024];
			List<ushort> orderedList = trackOffsetTable.OrderBy(o => o).ToList();

			for (int i = 0; i < numberOfPatterns; i++)
			{
				// Build each channel from the tracks
				for (int j = 0; j < 4; j++)
				{
					ushort trackOffset = trackOffsetTable[i * 4 + j];
					int index = orderedList.IndexOf(trackOffset);
					uint endOffset = index < orderedList.Count - 1 ? trackDataOffset + orderedList[index + 1] : sampleDataOffset;

					moduleStream.Seek(trackDataOffset + trackOffset, SeekOrigin.Begin);

					for (int k = 0; k < 64; )
					{
						// Did we reach the boundary?
						if (moduleStream.Position < endOffset)
						{
							// No, it's safe to read the track data
							k += ConvertData(moduleStream, pattern, k * 16 + j * 4);
						}
						else
						{
							// Out of range
							pattern[k * 16 + j * 4] = 0x00;
							pattern[k * 16 + j * 4 + 1] = 0x00;
							pattern[k * 16 + j * 4 + 2] = 0x00;
							pattern[k * 16 + j * 4 + 3] = 0x00;
							k++;
						}
					}
				}

				yield return pattern;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write all the samples
		/// </summary>
		/********************************************************************/
		protected override bool WriteSampleData(ModuleStream moduleStream, ConverterStream converterStream)
		{
			if (deltaSamples || packedSamples)
			{
				for (int i = 0; i < numberOfSamples; i++)
				{
					int length = (int)sampleLengths[i];
					if (length != 0)
					{
						moduleStream.Seek(sampleDataOffset + sampleStartOffsets[i], SeekOrigin.Begin);

						sbyte[] destinationBuffer = new sbyte[length];

						// Is the sample packed?
						if (samplePacked[i])
						{
							// Yes, decompress it
							sbyte sample = 0;

							for (int j = 0; j < length; )
							{
								byte nib1 = moduleStream.Read_UINT8();
								byte nib2 = (byte)(nib1 & 0x0f);
								nib1 >>= 4;

								sample -= packTable[nib1];
								destinationBuffer[j++] = sample;
								sample -= packTable[nib2];
								destinationBuffer[j++] = sample;
							}
						}
						else if (deltaSamples)	// Are the samples stored as delta values?
						{
							// Copy the first sample
							sbyte sample = moduleStream.Read_INT8();
							destinationBuffer[0] = sample;

							// De-delta the rest
							for (int j = 1; j < length; j++)
							{
								sample -= moduleStream.Read_INT8();
								destinationBuffer[j] = sample;
							}
						}
						else
							moduleStream.ReadSigned(destinationBuffer, 0, length);

						converterStream.Write(MemoryMarshal.Cast<sbyte, byte>(destinationBuffer));
					}
				}
			}
			else
			{
				for (int i = 0; i < numberOfSamples; i++)
				{
					int length = (int)sampleLengths[i];
					if (length != 0)
					{
						moduleStream.Seek(sampleDataOffset + sampleStartOffsets[i], SeekOrigin.Begin);

						// Check to see if we miss too much from the last sample
						if (moduleStream.Length - moduleStream.Position < (length - MaxNumberOfMissingBytes))
							return false;

						moduleStream.SetSampleDataInfo(i, length);
						converterStream.WriteSampleDataMarker(i, length);
					}
				}
			}

			return true;
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Will convert a single track line to a ProTracker track line
		/// </summary>
		/********************************************************************/
		protected abstract int ConvertData(ModuleStream moduleStream, byte[] pattern, int writeOffset);
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format and return
		/// its version. -1 for unknown
		/// </summary>
		/********************************************************************/
		protected int CheckForThePlayerFormat(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 4)
				return -1;

			uint sampleInfoOffset = 0;
			int type = 50;

			// Start to check for signature
			if (GotId(moduleStream))
				sampleInfoOffset = 4;

			// Check sample offset
			ushort sampleOffset = moduleStream.Read_B_UINT16();
			if (((sampleOffset & 0x1) != 0) || (sampleOffset < 0x24))
				return -1;

			// Check number of patterns
			numberOfPatterns = moduleStream.Read_UINT8();
			if ((numberOfPatterns == 0) || (numberOfPatterns > 64))
				return -1;

			// Check number of samples
			byte sampleCount = moduleStream.Read_UINT8();
			if (sampleCount > 0xdf)
				return -1;

			// Check for packed samples
			if ((sampleCount & 0x40) != 0)
				sampleInfoOffset += 8;
			else
				sampleInfoOffset += 4;

			sampleCount &= 0x1f;
			if (sampleCount == 0)
				return -1;

			// Check sample offset
			if (sampleOffset <= (sampleCount * 6 + numberOfPatterns * 8))
				return -1;

			// Check sample information
			moduleStream.Seek(sampleInfoOffset, SeekOrigin.Begin);

			uint samplesSize = 0;
			ushort[] lengths = new ushort[sampleCount];

			for (int i = 0; i < sampleCount; i++)
			{
				// Get sample length and fine tune
				ushort temp1 = moduleStream.Read_B_UINT16();
				byte fineTune = moduleStream.Read_UINT8();

				// Check sample length
				if (temp1 >= 0x8000)
				{
					// Special case. Repeat another sample
					temp1 = unchecked((ushort)-(short)temp1);
					if (temp1 > 31)
						return -1;

					temp1 = lengths[temp1 - 1];
				}
				else
				{
					// Add the sample size to the total size
					samplesSize += temp1;

					// If fine tune is negative, the sample is compressed
					if ((fineTune & 0x80) == 0)
						samplesSize += temp1;	// Not packed
				}

				// Remember the length
				lengths[i] = temp1;

				// Check fine tune and volume
				if ((fineTune & 0x70) != 0)
					return -1;

				if (moduleStream.Read_UINT8() > 64)
					return -1;

				// Check loop
				temp1 = moduleStream.Read_B_UINT16();
				if (temp1 < 0x8000)
				{
					// Got loop, now find out the start and length
					if ((lengths[i] - temp1) >= 0x8000)
						return -1;
				}
			}

			// Check first track offset. It has to be zero
			if (moduleStream.Read_B_UINT16() != 0)
				return -1;

			moduleStream.Seek(-2, SeekOrigin.Current);

			// Check track table
			for (int i = 0; i < numberOfPatterns; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					ushort temp2 = moduleStream.Read_B_UINT16();
					ushort temp3 = (ushort)(moduleStream.Read_B_UINT16() - 3);

					if (temp3 < temp2)
						return -1;

					if (temp2 > sampleOffset)
						return -1;

					if (temp3 > sampleOffset)
						return -1;

					moduleStream.Seek(-2, SeekOrigin.Current);
				}

				moduleStream.Seek(2, SeekOrigin.Current);
			}

			// Check position table
			int taken = 0;	// Number of positions taken
			byte temp4;

			while ((temp4 = moduleStream.Read_UINT8()) != 0xff)
			{
				if ((temp4 % 2) == 0)
				{
					if (temp4 > 0x7e)
						return -1;
				}
				else
				{
					type = 60;

					if (temp4 > 0x3f)
						return -1;
				}

				taken++;
				if (taken > 0x7f)
					return -1;
			}

			// Check module size
			if ((sampleOffset + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return -1;

			// Find out if the module is a 6.0A or 6.1A by scanning the track data
			uint endOffset = sampleOffset - 4U;
			uint offset = (uint)moduleStream.Position;
			uint temp = 0;

			while (moduleStream.Position < endOffset)
			{
				byte byt1 = moduleStream.Read_UINT8();
				byte byt2 = moduleStream.Read_UINT8();

				if ((byt1 == 0x00) && (byt2 == 0x00))
					temp++;

				moduleStream.Seek(-1, SeekOrigin.Current);
			}

			// If we haven't found any 0x00 pair, it's a 6.1A module
			if (temp == 0)
				type = 61;
			else
			{
				// Make a second test
				endOffset -= 4;
				temp = 0;

				moduleStream.Seek(offset, SeekOrigin.Begin);

				while (moduleStream.Position < endOffset)
				{
					if ((moduleStream.Read_UINT8() == 0x80) && (moduleStream.Read_UINT8() <= 0x0f))
					{
						ushort temp1 = moduleStream.Read_B_UINT16();
						if ((moduleStream.Position - temp1) > offset)
							temp++;
					}
				}

				if (temp == 0)
					type = 61;
			}

			return type;
		}



		/********************************************************************/
		/// <summary>
		/// Return a string indicating if the module contains packed samples
		/// or not
		/// </summary>
		/********************************************************************/
		protected string PackedFormat => packedSamples ? Resources.IDS_PROWIZ_THEPLAYER_PACKED : deltaSamples ? Resources.IDS_PROWIZ_THEPLAYER_DELTA : string.Empty;



		/********************************************************************/
		/// <summary>
		/// Return the position list. Note that the returned list must be the
		/// same length as to be played
		/// </summary>
		/********************************************************************/
		protected Span<byte> LoadPositionList(ModuleStream moduleStream, byte divisor)
		{
			CreateOffsetTable(moduleStream);

			byte[] positionList = new byte[128];
			int numberOfPositions = 0;
			byte pos;

			while ((pos = moduleStream.Read_UINT8()) != 0xff)
			{
				pos /= divisor;
				positionList[numberOfPositions++] = pos;
			}

			trackDataOffset = (uint)moduleStream.Position;

			return positionList.AsSpan(0, numberOfPositions);
		}



		/********************************************************************/
		/// <summary>
		/// Will convert a relative note into a period
		/// </summary>
		/********************************************************************/
		protected void GetNote(byte trackByte, out byte byt1, out byte byt2)
		{
			// Is there a hi-bit sample number?
			if ((trackByte & 0x01) != 0)
			{
				byt1 = 0x10;
				trackByte &= 0xfe;
			}
			else
				byt1 = 0x00;

			// Any note?
			if (trackByte != 0)
			{
				trackByte -= 2;
				trackByte /= 2;

				byt1 |= periods[trackByte, 0];
				byt2 = periods[trackByte, 1];
			}
			else
				byt2 = 0x00;
		}



		/********************************************************************/
		/// <summary>
		/// Will check the effect and change some of them
		/// </summary>
		/********************************************************************/
		protected void CheckEffect(ref byte byt3, ref byte byt4)
		{
			switch (byt3 & 0x0f)
			{
				// Pattern break
				case 0xd:
				{
					byt4 = 0x00;
					break;
				}

				// Filter
				case 0xe:
				{
					if (byt4 == 0x02)
						byt4 = 0x01;

					break;
				}

				// Vibrato + Volume slide
				// Tone portamento + Volume slide
				// Volume slide
				case 0x5:
				case 0x6:
				case 0xa:
				{
					if (byt4 >= 0x80)
						byt4 = (byte)((-(sbyte)byt4) << 4);

					break;
				}

				// Arpeggio
				case 0x8:
				{
					byt3 &= 0xf0;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will set multiple rows with the values given
		/// </summary>
		/********************************************************************/
		protected void SetMultipleRows(byte count, byte byt1, byte byt2, byte byt3, byte byt4, byte[] pattern, ref int writeOffset)
		{
			while (count != 0)
			{
				pattern[writeOffset] = byt1;
				pattern[writeOffset + 1] = byt2;
				pattern[writeOffset + 2] = byt3;
				pattern[writeOffset + 3] = byt4;

				writeOffset += 16;
				count--;
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Check if the module has an ID or not
		/// </summary>
		/********************************************************************/
		private bool GotId(ModuleStream moduleStream)
		{
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint temp = moduleStream.Read_B_UINT32();
			if ((temp != 0x50353041) && (temp != 0x50363041) && (temp != 0x50363141))
			{
				moduleStream.Seek(0, SeekOrigin.Begin);
				return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Create the track offset table
		/// </summary>
		/********************************************************************/
		private void CreateOffsetTable(ModuleStream moduleStream)
		{
			trackOffsetTable = new ushort[4 * numberOfPatterns];
			moduleStream.ReadArray_B_UINT16s(trackOffsetTable, 0, trackOffsetTable.Length);
		}
		#endregion
	}
}
